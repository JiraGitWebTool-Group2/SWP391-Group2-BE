using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.SrsDocuments.Dtos;

namespace SWP391.Group2.Application.Features.SrsDocuments.Queries
{
    public record GetAllSrsDocumentsQuery : IRequest<List<SrsDocumentDto>>;

    public class GetAllSrsDocumentsQueryHandler : IRequestHandler<GetAllSrsDocumentsQuery, List<SrsDocumentDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllSrsDocumentsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SrsDocumentDto>> Handle(GetAllSrsDocumentsQuery request, CancellationToken cancellationToken)
        {
            var srsDocuments = await _context.SrsDocuments
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new SrsDocumentDto
                {
                    SrsId = x.SrsId,
                    ProjectId = x.ProjectId,
                    CreatedByUserId = x.CreatedByUserId,
                    Version = x.Version,
                    ScopeType = x.ScopeType,
                    Title = x.Title,
                    Content = x.Content,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return srsDocuments;
        }
    }
}