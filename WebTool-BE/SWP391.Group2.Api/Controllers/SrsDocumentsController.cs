using MediatR;
using Microsoft.AspNetCore.Mvc;
using SWP391.Group2.Application.Features.SrsDocuments.Queries;

namespace SWP391.Group2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SrsDocumentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SrsDocumentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllSrsDocumentsQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSrsDocumentByIdQuery(id), cancellationToken);

            if (result == null)
            {
                return NotFound(new { message = $"SRS document with id = {id} was not found." });
            }

            return Ok(result);
        }
    }
}