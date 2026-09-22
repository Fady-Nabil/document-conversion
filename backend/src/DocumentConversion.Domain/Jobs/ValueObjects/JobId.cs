namespace DocumentConversion.Domain.Jobs.ValueObjects;

public readonly record struct JobId(Guid Value)
{
    public static JobId New() => new(Guid.NewGuid());
    public static JobId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
