using System;
namespace Ensure.AndroidApp.Data
{
    /// <summary>
    /// Represents the state of the log against the server
    /// </summary>
    public enum EnsureSyncState
    {
        /// <summary> Log is synced with server. </summary>
        Synced,
        /// <summary> Log should be pushed as inserted to server. </summary>
        ToAdd,
        /// <summary> Log should be pushed as removed to server </summary>
        ToRemove
    }
}
