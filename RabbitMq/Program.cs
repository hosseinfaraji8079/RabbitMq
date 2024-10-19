using EventBus.Messages.Events;
using MassTransit;
using Quartz;
using RabbitMq.Consumer;
using RabbitMq.Jobs;
using RabbitMq.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// builder.Services.AddMassTransit(x =>
// {
//     x.UsingRabbitMq((context, cfg) =>
//     {
//         x.AddConsumer<PeerUpdateCommandConsumer>();
//         cfg.Host("rabbitmq://rabbitmq", h =>
//         {
//             h.Username("user");
//             h.Password("password");
//         });
//     });
// });

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ConsumerSyncData>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://rabbitmq", h =>
        {
            h.Username("user");
            h.Password("password");
        });

        cfg.ReceiveEndpoint("sync_data", e =>
        {
            e.ConfigureConsumer<ConsumerSyncData>(context);
            e.PrefetchCount = 1;
        });
    });
});

builder.Services.AddMassTransitHostedService();

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    var jobSettings = builder.Configuration.GetSection("Quartz:Jobs").Get<List<JobSettings>>();

    if (jobSettings != null)
        foreach (var settings in jobSettings)
        {
            var jobType = Type.GetType(settings.JobType);
            if (jobType == null)
            {
                throw new InvalidOperationException($"Job type '{settings.JobType}' could not be found.");
            }

            var jobKey = new JobKey(settings.JobName, settings.JobGroup);

            switch (settings.JobName)
            {
                case "SyncDataJobs":
                    q.AddJob<SyncDataJobs>(opts => opts.WithIdentity(jobKey));
                    break;
            }

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(settings.TriggerName, settings.TriggerGroup)
                .WithCronSchedule(settings.CronSchedule));
        }
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();