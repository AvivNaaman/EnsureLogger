using System;
using AndroidX.Work;
using Android.Content;

namespace Ensure.AndroidApp.Helpers
{
    public class NotificationWorker : Worker
    {
        Context context;

        public NotificationWorker(Context context, WorkerParameters parameters) : base(context, parameters)
        {
            this.context = context;
        }

        public override Result DoWork()
        {
            // get left
            int left = InputData.GetInt(EnsureAndroidConstants.NotificationWorkLeftParamName, 0);
            if (left < 0) return Result.InvokeFailure();
            // notify
            NotificationHelper.NotifyDrink(left, context);
            return Result.InvokeSuccess();
        }
    }
}
