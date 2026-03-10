using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Users.Dtos;
using SWP391.Group2.Domain.Entities;

namespace SWP391.Group2.Application.Features.Users.Command
{
    public class ImportUsersFromExcelHandler
        : IRequestHandler<ImportUsersFromExcelCommand, ImportUsersExcelResultDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly PasswordHasher<User> _hasher = new();

        public ImportUsersFromExcelHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ImportUsersExcelResultDto> Handle(ImportUsersFromExcelCommand cmd, CancellationToken ct)
        {
            var result = new ImportUsersExcelResultDto();

            if (cmd.FileContent == null || cmd.FileContent.Length == 0)
                throw new ArgumentException("File rỗng.");

            if (!cmd.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Chỉ hỗ trợ file .xlsx");

            using var ms = new MemoryStream(cmd.FileContent);
            using var workbook = new XLWorkbook(ms);

            var ws = workbook.Worksheets.FirstOrDefault()
                     ?? throw new ArgumentException("Không tìm thấy worksheet nào trong file.");

            var used = ws.RangeUsed();
            if (used == null) return result; // không có data

            var rows = used.RowsUsed().ToList();
            if (rows.Count == 0) return result;

            //Detect header: Username | Password | Fullname (tùy có/không)
            var startIndex = 1;
            {
                var h1 = rows[0].Cell(1).GetString().Trim().ToLowerInvariant();
                var h2 = rows[0].Cell(2).GetString().Trim().ToLowerInvariant();
                var h3 = rows[0].Cell(3).GetString().Trim().ToLowerInvariant();
                if (h1.Contains("username") && h2.Contains("system_role") && (h3.Contains("fullname") || h3.Contains("full name")))
                    startIndex = 1;
            }

            // Load existing emails để check trùng nhanh (Username map vào Email)
            var existingEmails = await _db.Users
                .Select(u => u.Email.ToLower())
                .ToListAsync(ct);

            var existingSet = existingEmails.ToHashSet();

            var toInsert = new List<User>();

            for (int i = startIndex; i < rows.Count; i++)
            {
                var row = rows[i];
                var rowNumber = row.RowNumber();

                // Excel yêu cầu 3 cột: A Username, B Password, C Fullname
                var username = row.Cell(1).GetString().Trim();
                var system_role = row.Cell(2).GetString().Trim().ToUpper(); // raw để hash
                var fullName = row.Cell(3).GetString().Trim();

                // bỏ dòng trống hoàn toàn
                if (string.IsNullOrWhiteSpace(username) &&
                    string.IsNullOrWhiteSpace(system_role) &&
                    string.IsNullOrWhiteSpace(fullName))
                    continue;

                result.TotalRows++;

                if (string.IsNullOrWhiteSpace(username))
                {
                    result.Skipped++;
                    result.Errors.Add(new ImportUsersExcelRowErrorDto
                    {
                        RowNumber = rowNumber,
                        Username = null,
                        Error = "Thiếu Username."
                    });
                    continue;
                }

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    result.Skipped++;
                    result.Errors.Add(new ImportUsersExcelRowErrorDto
                    {
                        RowNumber = rowNumber,
                        Username = username,
                        Error = "Thiếu Fullname."
                    });
                    continue;
                }

                if (string.IsNullOrWhiteSpace(system_role))
                {
                    result.Skipped++;
                    result.Errors.Add(new ImportUsersExcelRowErrorDto
                    {
                        RowNumber = rowNumber,
                        Username = username,
                    });
                    continue;
                }

                // Username -> Email
                var emailKey = username.ToLowerInvariant();

                // check trùng DB hoặc trùng ngay trong file
                if (existingSet.Contains(emailKey))
                {
                    result.Skipped++;
                    result.Errors.Add(new ImportUsersExcelRowErrorDto
                    {
                        RowNumber = rowNumber,
                        Username = username,
                        Error = "Username đã tồn tại."
                    });
                    continue;
                }

                if (system_role != "ADMIN" && system_role != "LECTURER" && system_role != "STUDENT")
                {
                    result.Skipped++;
                    result.Errors.Add(new ImportUsersExcelRowErrorDto
                    {
                        RowNumber = rowNumber,
                        Username = username,
                        Error = "Role không hợp lệ."
                    });
                    continue;
                }

                var now = DateTime.UtcNow;

                var user = new User
                {
                    Email = username,       // map Username -> Email
                    FullName = fullName,
                    System_Role = system_role,
                    Provider = "LOCAL",
                    ProviderUserId = null,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                //user.PasswordHash = _hasher.HashPassword(user, password.Trim());

                toInsert.Add(user);
                existingSet.Add(emailKey);
            }

            if (toInsert.Count > 0)
            {
                _db.Users.AddRange(toInsert);

                try
                {
                    await _db.SaveChangesAsync(ct);
                }
                catch (DbUpdateException)
                {
                    // đề phòng unique constraint nổ vì race-condition
                    throw new InvalidOperationException("Import thất bại do dữ liệu trùng (unique constraint).");
                }
            }

            result.Inserted = toInsert.Count;
            return result;
        }
    }
}