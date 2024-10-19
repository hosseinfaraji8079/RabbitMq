using EventBus.Messages.Events;
using MassTransit;
using Quartz;

namespace RabbitMq.Jobs;
public class SyncDataJobs(IBus bus) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        MessageEvent @event = new MessageEvent("Hello from Quartz Job!");
        await bus.Publish(@event);
        Console.WriteLine("Job Executed and Message Published");
    }
}

