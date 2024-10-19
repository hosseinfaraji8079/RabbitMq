using EventBus.Messages.Events;
using MassTransit;

namespace RabbitMq.Consumer;

public class ConsumerSyncData : IConsumer<MessageEvent>
{
    public Task Consume(ConsumeContext<MessageEvent> context)
    {
        Console.WriteLine($"Message Consumed: {context.Message.Message}");
        return Task.CompletedTask;
    }
}