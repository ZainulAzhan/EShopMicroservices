namespace Ordering.Domain.Abstractions;

public abstract class Aggregate<T> : Entity<T>, IAggregate<T>
    where T : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent), "Domain event cannot be null.");
        }        
        _domainEvents.Add(domainEvent);
    }
    public IDomainEvent[] ClearDomainEvents()
    {
        var events = _domainEvents.ToArray();
        _domainEvents.Clear();
        return events;
    }
}
