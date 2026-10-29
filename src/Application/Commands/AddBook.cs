namespace Aruba.Application.Commands;

public sealed record class AddBook
{
    public required BookId Id { get; init; }
    public required string Title { get; init; }
    public required string Author { get; init; }
}
