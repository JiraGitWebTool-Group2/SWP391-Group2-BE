using MediatR;

public record DeleteGroupCommand(int GroupId) : IRequest<bool>;