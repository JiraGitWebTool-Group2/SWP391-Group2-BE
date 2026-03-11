using MediatR;
using SWP391.Group2.Application.Features.Groups.Dtos;

namespace SWP391.Group2.Application.Features.Groups.Queries;

public sealed record GetIntegratedGroupsQuery() : IRequest<List<IntegratedGroupDto>>;