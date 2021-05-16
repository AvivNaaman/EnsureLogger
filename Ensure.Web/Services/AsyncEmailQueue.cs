using System;
using System.Collections.Generic;
using FluentEmail.Core;
using FluentEmail;
using FluentEmail.Core.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ensure.Web.Services
{
    public class AsyncEmailQueue : IAsyncEmailQueue
    {
        private readonly ITemplateRenderer _templateRenderer;
        private readonly ISender _sender;
        private readonly ILogger<AsyncEmailQueue> _logger;

        private Queue<AsyncEmailInfo> sendingQueue { get; set; } = new();   
        private AsyncEmailInfo currentlyHandeled = null;

        public AsyncEmailQueue(ITemplateRenderer templateRenderer, ISender sender, ILogger<AsyncEmailQueue> logger)
        {
            _templateRenderer = templateRenderer;
            _sender = sender;
            _logger = logger;
        }

        public void AddToQueue((string,string) from, (string, string) to, string subj,
            string template, dynamic templateData)
        {
            sendingQueue.Enqueue(new(
                from,
                to,
                subj,
                template,
                templateData
            ));
            HandleNextIfAvailable();
        }

        private void HandleNextIfAvailable()
        {
            if (currentlyHandeled is null)
            {
                currentlyHandeled = sendingQueue.Dequeue();
                IFluentEmail e = new Email(_templateRenderer, _sender)
                    .To(currentlyHandeled.To.Item1, currentlyHandeled.To.Item2)
                    .SetFrom(currentlyHandeled.From.Item1, currentlyHandeled.From.Item2)
                    .Subject(currentlyHandeled.Subject)
                    .UsingTemplate(currentlyHandeled.RazorTemplate, currentlyHandeled.TemplateData);

                Task.Run(() => e.SendAsync()).ContinueWith(e =>
                {
                    if (!e.Result.Successful) _logger.LogError("Failed to send email {0}", currentlyHandeled.To);
                    else _logger.LogInformation("Email sent to {0} succesfully.", currentlyHandeled.To);
                    HandleNextIfAvailable();
                });
            }
        }
    }

    public record AsyncEmailInfo
    (
        (string,string) From,
        (string,string) To,
        string Subject,
        string RazorTemplate,
        dynamic TemplateData
    );
}
