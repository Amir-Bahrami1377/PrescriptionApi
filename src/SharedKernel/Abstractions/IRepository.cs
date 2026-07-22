using Prescription.SharedKernel.Entities;

namespace Prescription.SharedKernel.Abstractions;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(TEntity entity);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
