using System;
using Ensure.Domain.Entities;
using SQLite;

namespace Ensure.AndroidApp.Data
{
    [Table("EnsureLogs")]
    public class InternalEnsureLog : EnsureLog
    {
        // override property to set as PK for sqlite
        /// <inheritdoc/>
        [PrimaryKey]
        public new string Id { get; set; }

        /// <summary>
        /// Whether the log is synced with server
        /// </summary>
        public EnsureSyncState SyncState { get; set; }
    }
}
