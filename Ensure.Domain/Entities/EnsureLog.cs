using Ensure.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ensure.Domain.Entities
{
	public class EnsureLog
	{
        public EnsureLog()
        {

        }

        public EnsureLog(DateTime logged, EnsureTaste taste, string userId)
        {
            UserId = userId;
            EnsureTaste = taste;
            Logged = logged;
        }

        public EnsureLog(EnsureLog other) : this(other.Logged, other.EnsureTaste, other.UserId)
        {

        }

		public string Id { get; set; } = Guid.NewGuid().ToString();
		public DateTime Logged { get; set; } = DateTime.UtcNow;
		public EnsureTaste EnsureTaste { get; set; }
		public string UserId { get; set; }
	}
}
