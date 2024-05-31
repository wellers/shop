using System;

namespace Client.Services.SessionService
{
	public class SessionService : ISessionService
	{
		public Guid SessionId { get; set; } = Guid.NewGuid();
	}
}