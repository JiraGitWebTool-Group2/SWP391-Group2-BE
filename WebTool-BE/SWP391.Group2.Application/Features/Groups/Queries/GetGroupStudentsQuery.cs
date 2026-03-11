using MediatR;
using SWP391.Group2.Application.Features.Groups.Dtos;

namespace SWP391.Group2.Application.Features.Groups.Queries;

public record GetGroupStudentsQuery(int GroupId) : IRequest<List<GroupStudentDto>>;