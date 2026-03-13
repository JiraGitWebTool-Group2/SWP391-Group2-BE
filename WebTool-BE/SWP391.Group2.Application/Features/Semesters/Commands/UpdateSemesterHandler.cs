using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Semesters.Dtos;

namespace SWP391.Group2.Application.Features.Semesters.Commands;

public class UpdateSemesterHandler : IRequestHandler<UpdateSemesterCommand, SemesterDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdateSemesterHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SemesterDto?> Handle(UpdateSemesterCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Semesters
            .FirstOrDefaultAsync(x => x.SemesterId == request.SemesterId, cancellationToken);

        if (entity == null)
            return null;

        var status = request.Status.Trim().ToUpperInvariant();

        if (request.StartDate.Date > request.EndDate.Date)
            throw new ArgumentException("StartDate must be less than or equal to EndDate.");

        if (status != "PLANNING" && status != "ACTIVE" && status != "CLOSED")
            throw new ArgumentException("Status must be PLANNING, ACTIVE or CLOSED.");

        var duplicatedCode = await _context.Semesters
            .AnyAsync(x => x.Code == request.Code && x.SemesterId != request.SemesterId, cancellationToken);

        if (duplicatedCode)
            throw new ArgumentException("Semester code already exists.");

        entity.Code = request.Code.Trim();
        //entity.Name = request.Name.Trim();
        entity.StartDate = request.StartDate.Date;
        entity.EndDate = request.EndDate.Date;
        entity.Status = status;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new SemesterDto
        {
            SemesterId = entity.SemesterId,
            Code = entity.Code,
            //Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}