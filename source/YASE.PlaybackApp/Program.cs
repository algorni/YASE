using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using System;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YASE.Core;

namespace YASE.PlaybackApp
{
    class Program
    {
        private static IConfiguration configuration;
        private static EventHubProducerClient eventHubProducerClient;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to YASE - > Yet Another Simulation Engine!");
            Console.WriteLine("This is the playback application; you need to provide a JSON file with a planned simulation and");
            Console.WriteLine("this tool will reproduce it with the correct timing.");

            configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();


            string simulationFile = configuration.GetValue<string>("simulationFile");

            //load the simulation plan from file...
            SimulationPlan simulationPlan = null;

            try
            {
                simulationPlan = SimulationPlan.LoadFromFile(simulationFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"There was an exception while loading the simulation plan:\n{ex.ToString()}");
            }
            
            //now creating the Event Hub Client
            string eventHubConnectionString = configuration.GetValue<string>("eventHubConnectionString");
            string eventHubName = configuration.GetValue<string>("eventHubName");

            eventHubProducerClient = new EventHubProducerClient(eventHubConnectionString, eventHubName);





            Console.WriteLine($"Initializing the Simulation Runner...");
            SimulationRunner simulationRunner = new SimulationRunner(simulationPlan, doSomethingWithTheEventAsync);

            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();


            Console.WriteLine($"Starting the simulation...");
            await simulationRunner.StartRunner(cts.Token);


            // Wait until the app unloads or is cancelled
            WhenCancelled(cts.Token).Wait();

            Console.WriteLine("End program loop.");
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }



        /// <summary>
        /// do something with this simulation!
        /// </summary>
        /// <param name="generatedEvent"></param>
        private static async Task doSomethingWithTheEventAsync(PlannedEvent generatedEvent)
        {
            var eventJson = generatedEvent.ToJSON();

            // Create a batch of events using the SOURCE ID as the Partition
            EventDataBatch eventBatch = await eventHubProducerClient.CreateBatchAsync( 
                new CreateBatchOptions() {  PartitionKey = generatedEvent.SourceId});
                   
            // Add events to the batch. An event is a represented by a collection of bytes and metadata. 
            var eventData = new EventData(Encoding.UTF8.GetBytes(eventJson));
          
            eventBatch.TryAdd(eventData);

            
            await eventHubProducerClient.SendAsync(eventBatch);

            Console.WriteLine($"{DateTime.UtcNow.ToString()}\n{eventJson}");
        }
    }
}
