namespace Aruba.Application.FunctionalTests;

[TestClass]
public sealed class AddBookTests : TestBase
{
    [TestMethod]
    public async Task TestMethod1()
    {
        var mediator = this.provider.GetRequiredService<IMediator>();
        var command = new AddBookDefault()
            .Build();

        await mediator.Send(command, CancellationToken.None);
    }
}
