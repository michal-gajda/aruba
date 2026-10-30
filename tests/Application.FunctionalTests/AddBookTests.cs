namespace Aruba.Application.FunctionalTests;

using Aruba.Application.FunctionalTests.Builders;
using Aruba.Domain.Interfaces;

[TestClass]
public sealed class AddBookTests : TestBase
{
    [TestMethod]
    public async Task Should_Save_Book()
    {
        var mediator = this.ServiceProvider.GetRequiredService<IMediator>();
        var command = new AddBookDefault()
            .Build();

        await mediator.Send(command, CancellationToken.None);

        var repository = this.ServiceProvider.GetRequiredService<IBookRepository>();
        var entity = await repository.LoadAsync(command.Id, CancellationToken.None);

        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(command.Id);
        entity.Title.ShouldBe(command.Title);
        entity.Author.ShouldBe(command.Author);
    }
}
