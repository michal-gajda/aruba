namespace Aruba.Domain.Entities;

public sealed class BookEntity
{
    public BookId Id { get; }
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;

    public BookEntity(BookId id, string title, string author)
    {
        this.Id = id;
        this.SetTitle(title);
        this.SetAuthor(author);
    }

    public void SetTitle(string title)
    {
        this.Title = title;
    }

    public void SetAuthor(string author)
    {
        this.Author = author;
    }
}
