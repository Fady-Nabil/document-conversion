namespace BuildingBlocks.Domain.Abstractions;

public abstract class AggregateRoot : Entity, IAggregateRoot
{
    protected AggregateRoot()
    {
    }
}
