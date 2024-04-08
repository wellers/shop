using CronScheduler.Extensions.Scheduler;

namespace Catalog.Jobs
{
	public class BackgroundJobOptions
	{
		public string Name { get; set; }
		public string CronPattern { get; set; }
	}

	public interface IBackgroundJob : IScheduledJob
	{
		new string Name { get; }
	}
}
