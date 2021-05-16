using System;
using System.Collections.Generic;
using Ensure.Domain.Entities;

namespace Ensure.Domain.Models
{
    public class EnsureSyncModel
    {
        /// <summary>
        /// Indicates whether the model should be added or removed from the database
        /// </summary>
        public bool ToAdd { get; set; }

        public EnsureLog ToSync { get; set; }
    }
}
