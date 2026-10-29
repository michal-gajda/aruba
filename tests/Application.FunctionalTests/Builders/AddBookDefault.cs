namespace Aruba.Application.FunctionalTests.Builders;

public sealed class AddBookDefault
{
    private BookId id = new BookId(Guid.NewGuid());
    private string title = "Default Title";
    private string author = "Default Author";

    public AddBookDefault WithId(BookId id)
    {
        this.id = id;

        return this;
    }

    public AddBookDefault WithTitle(string title)
    {
        this.title = title;

        return this;
    }

    public AddBookDefault WithAuthor(string author)
    {
        this.author = author;

        return this;
    }

    public AddBook Build() => new AddBook
    {
        Id = id,
        Title = title,
        Author = author
    };
}
