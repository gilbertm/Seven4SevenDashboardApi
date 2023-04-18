using RAFFLE.WebApi.Shared.Events;

namespace RAFFLE.WebApi.Application.Common.Events;

public interface IEventPublisher : ITransientService
{
    Task PublishAsync(IEvent @event);
}