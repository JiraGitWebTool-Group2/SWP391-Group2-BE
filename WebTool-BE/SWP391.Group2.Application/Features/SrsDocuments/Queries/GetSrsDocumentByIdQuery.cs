using MediatR;
using Microsoft.EntityFrameworkCore;
using SWP391.Group2.Application.Abstractions;
using SWP391.Group2.Application.Features.SrsDocuments.Dtos;

namespace SWP391.Group2.Application.Features.SrsDocuments.Queries
{
    public record GetSrsDocumentByIdQuery(int Id) : IRequest<SrsDocumentDto?>;

    public class GetSrsDocumentByIdQueryHandler : IRequestHandler<GetSrsDocumentByIdQuery, SrsDocumentDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetSrsDocumentByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SrsDocumentDto?> Handle(GetSrsDocumentByIdQuery request, CancellationToken cancellationToken)
        {
            var srsDocument = await _context.SrsDocuments
                .AsNoTracking()
                .Where(x => x.SrsId == request.Id)
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
                .FirstOrDefaultAsync(cancellationToken);

            return srsDocument;
        }
    }
}