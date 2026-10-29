namespace Aruba.Application.CommandHandlers;

using Aruba.Domain.Interfaces;
using Aruba.Application.Commands;
using Aruba.Domain.Entities;

internal sealed class AddBookHandler(IBookRepository repository) : IRequestHandler<AddBook>
{
    public async Task Handle(AddBook request, CancellationToken cancellationToken)
    {
        var entity = new BookEntity(request.Id, request.Title, request.Author);

        await repository.SaveAsync(entity, cancellationToken);
    }
}
