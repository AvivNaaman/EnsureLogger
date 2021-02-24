using System;
namespace Ensure.AndroidApp.Data
{
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
