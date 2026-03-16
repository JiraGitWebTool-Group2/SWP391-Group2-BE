using MediatR;
using SWP391.Group2.Application.Features.Reports.Dtos;

namespace SWP391.Group2.Application.Features.Reports.Commands
{
    public record GenerateProgressReportCommand(
        int ProjectId,
        DateOnly StartDate,
        DateOnly EndDate,
        IReadOnlyCollection<int> Members,
        string ViewType,
        int CreatedByUserId
    ) : IRequest<GeneratedProgressReportDto>;
}