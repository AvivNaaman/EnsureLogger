using Ensure.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensure.Web.Data
{
    /// <summary>
    /// Represents a user in the database
    /// </summary>
	public class AppUser
	{
        /// <summary>
        /// The user's primary key
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// The user's user name (for login)
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// The user's email address
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// A unique value to validate & hash the password with
        /// </summary>
        public byte[] SecurityKey { get; set; } = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")); 
        /// <summary>
        /// The hashed password
        /// </summary>
        public byte[] PasswordHash { get; set; }

        /// <summary>
        /// The daily target of the user
        /// </summary>
		public int DailyTarget { get; set; }
        /// <summary>
        /// The date when the user joined
        /// </summary>
        public DateTime Joined { get; set; }

        public List<EnsureLog> Logs { get; set; }
    }
}
