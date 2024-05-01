namespace Catalog.Jobs
{
	public class BackgroundJobOptions
	{
		public string Name { get; set; }
		public string CronPattern { get; set; }
	}	
}