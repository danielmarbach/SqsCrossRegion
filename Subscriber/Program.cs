using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Messages;
using Microsoft.Data.SqlClient;
using NServiceBus;
using NServiceBus.Pipeline;

namespace Subscriber
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var endpointConfiguration = new EndpointConfiguration("Subscriber");
            endpointConfiguration.EnableInstallers();
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(() =>
            {
                return new SqlConnection("YOURCONNECTIONSTRING");
            });
            persistence.SubscriptionSettings().CacheFor(TimeSpan.FromMinutes(5));

            var transport = endpointConfiguration.UseTransport<SqsTransport>();
            transport.Routing().RegisterPublisher(typeof(MyEvent), "Publisher||OtherRegion");
            transport.ClientFactory(() => new DecoratingClient());

            var endpoint = await Endpoint.Start(endpointConfiguration);

            Console.ReadLine();

            await endpoint.Stop();
        }
    }

    class MyHandler : IHandleMessages<MyEvent>
    {
        public Task Handle(MyEvent message, IMessageHandlerContext context)
        {
            Console.WriteLine("Got it");
            return Task.CompletedTask;
        }
    }
}
