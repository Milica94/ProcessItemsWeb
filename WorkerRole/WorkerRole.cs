using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Data;
using System.ServiceModel;

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private String internalEndpointName = "InternalEndpoint";

        NetTcpBinding binding = new NetTcpBinding();

        private DataRepository _repo = new DataRepository();

        object lockObject = new object();

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole is running");

            Article article1 = new Article("123");
            Article article2 = new Article("456");
            Article article3 = new Article("789");

            article1.Identifier = "123";
            article2.Identifier = "456";
            article3.Identifier = "789";

            article1.Value = "Jagode";
            article2.Value = "Hleb";
            article3.Value = "Mleko";

            try
            {
                lock (lockObject)
                {
                    _repo.AddArticle(article1);
                    _repo.AddArticle(article2);
                    _repo.AddArticle(article3);
                }
            }
            catch (Exception e) { }

            NotifyOthersServer nos = new NotifyOthersServer();
            nos.Open();

            CloudQueue queue = QueueHelper.GetQueueReference("identifiers");

            while (true)
            {
                CloudQueueMessage message;

                lock (lockObject)
                {
                     message = queue.GetMessage();
                }

                if (message != null)
                {
                    Article article = _repo.GetArticle(message.AsString);

                    if(article != null)
                    {
                        Trace.TraceInformation("Artikal sa identifikatorom {0} je tipa: {1}.", article.Identifier, article.Value);
                        lock (lockObject)
                        {
                            queue.DeleteMessage(message);
                        }

                        List<EndpointAddress> internalEndpoints = RoleEnvironment.Roles[RoleEnvironment.CurrentRoleInstance.Role.Name]
                .Instances.Where(instance => instance.Id != RoleEnvironment.CurrentRoleInstance.Id)
                .Select(process => new EndpointAddress(
                    String.Format("net.tcp://{0}/{1}", process.InstanceEndpoints[internalEndpointName].IPEndpoint.ToString(),
                    internalEndpointName))).ToList();

                        int brotherInstances = internalEndpoints.Count;

                        Task [] tasks = new Task [brotherInstances];
                        for (int i = 0; i < brotherInstances; i++)
                        {
                            int index = i;
                            Task notify = new Task (() =>
                            {
                                INotifyOthers proxy = new ChannelFactory<INotifyOthers>(binding, internalEndpoints[index]).CreateChannel();
                                proxy.Notify(article.Value);
                            });

                            notify.Start();
                            tasks[index] = notify;
                        }
                        Task.WaitAll(tasks);
                    }
                    else
                    {
                        Trace.TraceInformation("Identifikator {0} je neispravan!", message.AsString);
                        lock (lockObject)
                        {
                            queue.DeleteMessage(message);
                        }
                    }
                }

                Thread.Sleep(5000);
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
