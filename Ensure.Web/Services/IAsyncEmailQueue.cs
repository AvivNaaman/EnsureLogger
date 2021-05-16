namespace Ensure.Web.Services
{
    public interface IAsyncEmailQueue
    {
        void AddToQueue((string,string) from, (string,string) to, string subj, string template, dynamic templateData);
    }
}