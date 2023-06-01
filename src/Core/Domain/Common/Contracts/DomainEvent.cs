using UNIFIEDDASHBOARD.WebApi.Shared.Events;

namespace UNIFIEDDASHBOARD.WebApi.Domain.Common.Contracts;

public abstract class DomainEvent : IEvent
{
    public DateTime TriggeredOn { get; protected set; } = DateTime.UtcNow;
}