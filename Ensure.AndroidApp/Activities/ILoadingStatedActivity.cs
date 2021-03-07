using System;
namespace Ensure.AndroidApp
{
    public interface ILoadingStatedActivity
    {
        /// <summary>
        /// Sets the loading state for the UI
        /// </summary>
        /// <param name="isLoading">Should UI indicate loading state</param>
        public void SetUiLoadingState(bool isLoading);
    }
}
