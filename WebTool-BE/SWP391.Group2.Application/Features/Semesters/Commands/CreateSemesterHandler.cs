using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.Semesters.Dtos;
using SWP391.Group2.Domain.Entities;

namespace SWP391.Group2.Application.Features.Semesters.Commands;

public class CreateSemesterHandler : IRequestHandler<CreateSemesterCommand, SemesterDto>
{
    private readonly IApplicationDbContext _context;

    public CreateSemesterHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SemesterDto> Handle(CreateSemesterCommand request, CancellationToken cancellationToken)
    {
        var status = request.Status.Trim().ToUpperInvariant();

        if (request.StartDate.Date > request.EndDate.Date)
            throw new ArgumentException("StartDate must be less than or equal to EndDate.");

        if (status != "PLANNING" && status != "ACTIVE" && status != "CLOSED")
            throw new ArgumentException("Status must be PLANNING, ACTIVE or CLOSED.");

        var exists = await _context.Semesters
            .AnyAsync(x => x.Code == request.Code, cancellationToken);

        if (exists)
            throw new ArgumentException("Semester code already exists.");

        var entity = new Semester
        {
            Code = request.Code.Trim(),
            //Name = request.Name.Trim(),
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Semesters.Add(entity);
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