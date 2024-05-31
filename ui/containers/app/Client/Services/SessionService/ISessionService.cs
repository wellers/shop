using System;

namespace Client.Services.SessionService
{
	public interface ISessionService
	{
		public Guid SessionId { get; set; }
	}
}