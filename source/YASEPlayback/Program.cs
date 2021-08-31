using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YASE.Core;
using YASE.Core.Entities;

namespace YASEPlayback
{
    class Program
    {
        private static IConfiguration configuration = null;

        private static CommandOption simulationPlanArg = null;

        private static CommandOption eventHubConnectionStringArg = null;

        private static CommandArgument tracksArg = null;

        private static EventHubProducerClient eventHubProducerClient;



        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to YASE - > Yet Another Simulation Engine!");
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine();

            configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            //.AddCommandLine(args)
            .Build();


            var commandLine = new CommandLineApplication()
            {
                Name = "YASE",
                FullName = "Yet Another Simulation Engine playback command line interface",
                Description = "This utility playbacks the recording, check helps for additional informations.",
                ShortVersionGetter = () => GetFileVersion(typeof(Program).Assembly, 3),
                LongVersionGetter = () => GetFileVersion(typeof(Program).Assembly, 4)
            };

            simulationPlanArg = commandLine.Option("-s | --simulationPlan", "Simulation plan in YASEON format.", CommandOptionType.SingleValue);

            eventHubConnectionStringArg = commandLine.Option("-eh | --eventHubConnectionString", "Event Hub Connection string", CommandOptionType.SingleValue);


            commandLine.Command("listTrack", c =>
            {
                c.HelpOption("-?|-h|--help");
                c.ExtendedHelpText = "Shows the list of track in the simulation plan";

                //simulationPlanArg = c.Argument("s", "Simulation Plan");

                c.OnExecute(() => executeListTrack());
            });


            commandLine.Command("playAll", c =>
            {
                c.HelpOption("-?|-h|--help");
                c.ExtendedHelpText = "Playback the simulation for all the tracks";
                c.OnExecute(() => executePlayTrack());
            });

            commandLine.Command("playTracks", c =>
            {                
                c.HelpOption("-?|-h|--help");
                c.ExtendedHelpText = "Playback the simulation for the provided tracks only.\nPlease provide the track as parameter after the playTracks command.";

                tracksArg = c.Argument("tracks", "List of tracks to playback", true);

                c.OnExecute(() => executePlayTrack(true));
            });

            commandLine.ExtendedHelpText = "Please for additional help / information visit  the git hub repo:\nhttps://github.com/algorni/YASE";

            commandLine.OnExecute(() => {
                commandLine.ShowHelp();
                return 1;
            });


            //execute the command line commands & options
            try
            {
                commandLine.Execute(args);
            }
            catch (Microsoft.Extensions.CommandLineUtils.CommandParsingException ex)
            {
                Console.WriteLine($"There was an error while parsing your parameters: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"There was an error while executing your command line.\n{ex.ToString()}");
            }

            Console.WriteLine("\n\nCiao");
        }



        /// <summary>
        /// do something with this simulation!
        /// </summary>
        /// <param name="generatedEvent"></param>
        private static async Task doSomethingWithTheEventAsync(GeneratedEvent generatedEvent)
        {
            var eventJson = generatedEvent.ToJSON();

            // Create a batch of events using the SOURCE ID as the Partition
            EventDataBatch eventBatch = await eventHubProducerClient.CreateBatchAsync(
                new CreateBatchOptions() { PartitionKey = generatedEvent.SourceId });

            // Add events to the batch. An event is a represented by a collection of bytes and metadata. 
            var eventData = new EventData(Encoding.UTF8.GetBytes(eventJson));

            eventBatch.TryAdd(eventData);

            await eventHubProducerClient.SendAsync(eventBatch);

            Console.WriteLine($"{DateTime.UtcNow.ToString()}\n{eventJson}");
        }


        private static int executeListTrack()
        {
            //load the simulation plan from file...
            SimulationPlan simulationPlan = loadSimulationPlan();

            if (simulationPlan != null)
            {
                Console.WriteLine("\nListing simulation tracks:\n");

                foreach (var track in simulationPlan.PlannedEventsTracks.Keys)
                {
                    Console.WriteLine($"{track}");
                }

                return 0;
            }
            else
                return -1; 
        }
        

        private static int executePlayTrack(bool checkForFilters = false)
        {
            //load the simulation plan from file...
            SimulationPlan simulationPlan = loadSimulationPlan();

            if (simulationPlan != null)
            {
                string[] selectedTracks = checkForFilters? checkForTracksFilter() : null;

                var eventHubConnectionString = getEventHubConnectionString();

                if (string.IsNullOrEmpty(eventHubConnectionString))
                {                    
                    return -1;
                }

                //now creating the Event Hub Client
                eventHubProducerClient = new EventHubProducerClient(eventHubConnectionString);

                Console.WriteLine($"Event Hub Connected: {eventHubProducerClient.IsClosed}");

                Console.WriteLine($"Initializing the Simulation Runner...");
                SimulationRunner simulationRunner = new SimulationRunner(simulationPlan, doSomethingWithTheEventAsync);

                var cts = new CancellationTokenSource();
                AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
                Console.CancelKeyPress += (sender, cpe) => cts.Cancel();

                Console.WriteLine($"Starting the simulation...");
                var runnerTask = simulationRunner.StartRunner(cts.Token, selectedTracks);

                // Wait until the app unloads or is cancelled
                var cancelTask = WhenCancelled(cts.Token);

                Task.WhenAll(runnerTask, cancelTask).Wait();

                Console.WriteLine("End program loop.");

                return 0;
            }
            else
                return -1;
        }

        /// <summary>
        /// load simulation plan file
        /// </summary>
        /// <returns></returns>
        private static SimulationPlan loadSimulationPlan()
        {
            SimulationPlan simulationPlan = null;

            string simulationPlanFile = null;

            //checking configuration 
            if ((simulationPlanArg != null) && (simulationPlanArg.HasValue()))
            {
                //give precedence to the command line
                simulationPlanFile = simulationPlanArg.Value();
            }
            else
            {
                //ok try other configuration options...
                simulationPlanFile = configuration.GetValue<string>("simulationPlan");
            }

            if (string.IsNullOrEmpty(simulationPlanFile))
            {
                Console.WriteLine("This tool requires a <simulationPlan> parameter.  This is the Simulation Plan JSON file to play.");
                return null;
            }

            try
            {
                Console.WriteLine($"Loading Simulation plan from file: {simulationPlanFile}");

                simulationPlan = SimulationPlan.LoadFromFile(simulationPlanFile);

                Console.WriteLine("Simulation plan loaded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"There was an exception while loading the simulation plan:\n{ex.ToString()}");
            }

            return simulationPlan;
        }


        private static string[] checkForTracksFilter()
        {
            List<string> selectedTracks = new List<string>();

            //checking configuration 
            if ((tracksArg != null) && (tracksArg.Values != null))
            {
                selectedTracks = tracksArg.Values;
            }
            else
            {
                //ok try other configuration options...
                selectedTracks.AddRange( configuration.GetValue<string>("selectedTracks").Split(",") );
            }

            if (selectedTracks.Count == 0)
            {                   
                return null;
            }
            else
                return selectedTracks.ToArray();
        }

        private static string getEventHubConnectionString()
        {
            string eventHubConnectionString = null;

            //checking configuration 
            if ((eventHubConnectionStringArg != null) && (eventHubConnectionStringArg.HasValue()))
            {
                //give precedence to the command line
               eventHubConnectionString = eventHubConnectionStringArg.Value();
            }
            else
            {
                //ok try other configuration options...
                eventHubConnectionString = configuration.GetValue<string>("eventHubConnectionString");
            }

            if (string.IsNullOrEmpty(eventHubConnectionString))
            {
                Console.WriteLine("This tool requires a <eventHubConnectionString> parameter to connect to a specific Event Hub topic.");
                return null;
            }
          
            return eventHubConnectionString;
        }

     



        private static string GetFileVersion(Assembly assembly, int components)
        {
            var fileVersion = (string)assembly.CustomAttributes.Single(c => c.AttributeType == typeof(AssemblyFileVersionAttribute)).ConstructorArguments[0].Value;
            var versionComponents = fileVersion.Split('.');
            return string.Join(".", versionComponents.Take(components).ToArray());
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



       
    }
}
