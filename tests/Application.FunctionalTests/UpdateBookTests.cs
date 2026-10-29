namespace Aruba.Application.FunctionalTests;

using Aruba.Application.Commands;
using Aruba.Application.FunctionalTests.Builders;
using Aruba.Domain.Interfaces;

[TestClass]
public sealed class UpdateBookTests : TestBase
{
    [TestMethod]
    public async Task Should_Rename_Exists_Book()
    {
        var mediator = this.serviceProvider.GetRequiredService<IMediator>();
        var addBook = new AddBookDefault()
            .Build();

        await mediator.Send(addBook);

        var command = new UpdateBook
        {
            Id = addBook.Id,
            Title = "Updated Title",
            Author = "Updated Author",
        };

        await mediator.Send(command);

        var repository = this.serviceProvider.GetRequiredService<IBookRepository>();
        var entity = await repository.LoadAsync(command.Id);

        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(command.Id);
        entity.Title.ShouldBe(command.Title);
        entity.Author.ShouldBe(command.Author);
    }
}
