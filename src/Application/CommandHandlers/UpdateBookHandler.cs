namespace Aruba.Application.CommandHandlers;

using Aruba.Application.Commands;
using Aruba.Domain.Interfaces;

internal sealed class UpdateBookHandler(IBookRepository repository) : IRequestHandler<UpdateBook>
{
    public async Task Handle(UpdateBook request, CancellationToken cancellationToken)
    {
        var entity =  await repository.LoadAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Book with ID {request.Id} not found.");

        entity.SetTitle(request.Title);
        entity.SetAuthor(request.Author);

        await repository.SaveAsync(entity, cancellationToken);
    }
}
