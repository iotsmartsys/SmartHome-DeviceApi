namespace Core.Contracts.Repositories;

public record class Criteria<TEntity> where TEntity : class
{
    public readonly TEntity? Entity;
    public Criteria() { }
    public Criteria(TEntity entity)
    {
        Entity = entity;
    }
}
