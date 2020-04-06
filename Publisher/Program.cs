using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Messages;
using Microsoft.Data.SqlClient;
using NServiceBus;

namespace Publisher
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var endpointConfiguration = new EndpointConfiguration("Publisher");
            endpointConfiguration.EnableInstallers();
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(() =>
            {
                return new SqlConnection("YOURCONNECTIONSTRING");
            });
            persistence.SubscriptionSettings().CacheFor(TimeSpan.FromMinutes(5));

            var transport = endpointConfiguration.UseTransport<SqsTransport>();
            transport.ClientFactory(() => new AmazonSQSClient(new AmazonSQSConfig
            {
                RegionEndpoint = RegionEndpoint.USEast1
            }));

            var endpoint = await Endpoint.Start(endpointConfiguration);

            await endpoint.Publish(new MyEvent());

            Console.ReadLine();

            await endpoint.Stop();
        }
    }
}
