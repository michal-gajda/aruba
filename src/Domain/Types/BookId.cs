namespace Aruba.Domain.Types;

public readonly struct BookId
{
    public BookId(Guid value) => this.Value = value;

    public Guid Value { get; }
}
