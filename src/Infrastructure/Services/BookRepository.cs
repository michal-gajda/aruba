namespace Aruba.Infrastructure.Services;

using System.Collections.Concurrent;
using Aruba.Domain.Entities;
using Aruba.Domain.Interfaces;

internal sealed class BookRepository(ConcurrentDictionary<BookId, BookEntity> dictionary) : IBookRepository
{
    public Task SaveAsync(BookEntity entity, CancellationToken cancellationToken = default)
    {
        dictionary[entity.Id] = entity;

        return Task.CompletedTask;
    }

    public Task<BookEntity?> LoadAsync(BookId id, CancellationToken cancellationToken = default)
    {
        if (dictionary.TryGetValue(id, out var entity))
        {
            return Task.FromResult<BookEntity?>(entity);
        }

        return Task.FromResult<BookEntity?>(null);
    }
}
