namespace Aruba.Domain.Interfaces;

using Aruba.Domain.Entities;

public interface IBookRepository
{
    Task SaveAsync(BookEntity entity, CancellationToken cancellationToken = default);
    Task<BookEntity?> LoadAsync(BookId id, CancellationToken cancellationToken = default);
}
