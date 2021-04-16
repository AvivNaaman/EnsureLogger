using System;
namespace Ensure.AndroidApp
{
    /// <summary>
    /// An interface for activities which have progressed work that requires loading state
    /// </summary>
    public interface ILoadingStatedActivity
    {
        /// <summary>
        /// Sets the loading state for the UI
        /// </summary>
        /// <param name="isLoading">Should UI indicate loading state</param>
        public void SetUiLoadingState(bool isLoading);
    }
}
