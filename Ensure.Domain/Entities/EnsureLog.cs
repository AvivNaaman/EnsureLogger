using Ensure.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ensure.Domain.Entities
{
    /// <summary>
    /// Represents a single ensure drink log
    /// </summary>
	public class EnsureLog
	{
        /// <summary>
        /// Empty constructor (for EF & JSON)
        /// </summary>
        public EnsureLog()
        {

        }

        /// <summary>
        /// A full constructor. Param doc in props.
        /// </summary>
        public EnsureLog(DateTime logged, EnsureTaste taste, string userId)
        {
            UserId = userId;
            EnsureTaste = taste;
            Logged = logged;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other">The log to copy the information from</param>
        public EnsureLog(EnsureLog other) : this(other.Logged, other.EnsureTaste, other.UserId)
        {

        }

        /// <summary>
        /// The primary key of the log.
        /// </summary>
		public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The date when the log was created
        /// </summary>
		public DateTime Logged { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The taste of the logged drink
        /// </summary>
		public EnsureTaste EnsureTaste { get; set; }

        /// <summary>
        /// The primary key of the logging user
        /// </summary>
		public string UserId { get; set; }
	}
}
