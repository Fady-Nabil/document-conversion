namespace BuildingBlocks.Domain.Abstractions;

public abstract class Entity
{
    public Guid Id { get; protected set; }

    protected Entity()
    {
    }
}
