using UNIFIEDDASHBOARD.WebApi.Shared.Events;

namespace UNIFIEDDASHBOARD.WebApi.Application.Common.Events;

public interface IEventPublisher : ITransientService
{
    Task PublishAsync(IEvent @event);
}