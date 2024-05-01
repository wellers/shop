using CronScheduler.Extensions.Scheduler;

namespace Catalog.Jobs
{
    public interface IBackgroundJob : IScheduledJob
    {
        new string Name { get; }
    }    
}