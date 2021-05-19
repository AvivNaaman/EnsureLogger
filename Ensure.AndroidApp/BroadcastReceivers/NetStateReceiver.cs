using Android.Content;
using Android.Net;
using Ensure.AndroidApp.Helpers;

namespace Ensure.AndroidApp.BroadcastReceivers
{
    /// <summary>
    /// A Broadcast Receiver to track network connectivity changes & trigger events when it received some
    /// </summary>
    [BroadcastReceiver]
    public class NetStateReceiver : BroadcastReceiver
    {
        /// <summary>
        /// An event handler to a network state change event
        /// </summary>
        /// <param name="isNetConnected">whether the network is currently available</param>
        /// <param name="prevState">whether network was previously available</param>
        public delegate void NetworkStateChangedHandler(bool isNetConnected, bool prevState);

        /// <summary>
        /// Called when there's a change in the network state
        /// </summary>
        public event NetworkStateChangedHandler NetworkStateChanged;

        /// <summary>
        /// Whether the device is currently online
        /// </summary>
        public bool CurrState { get; set; }

        /// <summary>
        /// Whether the device was previously online
        /// </summary>
        public bool PrevState { get; set; }

        public NetStateReceiver()
        {

        }

        /// <inheritdoc/>
        public override void OnReceive(Context context, Intent intent)
        {
            RefreshState(context);
            // Trigger change handler
            NetworkStateChanged?.Invoke(CurrState, PrevState);
        }

        /// <summary>
        /// Refreshes the properties that describe the current network state
        /// </summary>
        public void RefreshState(Context context)
        {
            ConnectivityManager connMgr = context
                .GetSystemService<ConnectivityManager>(Context.ConnectivityService);
            PrevState = CurrState;
            CurrState = connMgr.ActiveNetworkInfo.IsConnected;
        }
    }
}
