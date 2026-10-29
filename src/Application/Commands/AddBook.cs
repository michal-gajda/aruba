namespace Aruba.Application.Commands;

public sealed record class AddBook : IRequest
{
    public required BookId Id { get; init; }
    public required string Title { get; init; }
    public required string Author { get; init; }
}
