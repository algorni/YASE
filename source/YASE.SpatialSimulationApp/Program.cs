using GeoJSON.Net.Feature;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using YASE.Core;

namespace YASE.SpatialSimulationApp
{
    class Program
    {
        private static IConfiguration configuration;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Simulation!");


            configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();


            string geoJsonFile = configuration.GetValue<string>("geoJsonFile");

            var geoJson = File.ReadAllText(geoJsonFile);

            //parse the geoJson with the GeoJSON-Net library
            FeatureCollection featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(geoJson);

            SimulationPlan simulationPlan = createOneTimeSimulationPlan(featureCollection);

            var simulationPlanJson = simulationPlan.GetJSON();

            File.WriteAllText("c:\\tmp\\spatialSimulation.json", simulationPlanJson);
        }


        /// <summary>
        /// This is a sample code to generate a simulation...   that can be playbacked by the runner
        /// </summary>
        /// <returns></returns>
        //private static SimulationPlan<CarTrackingKmEvent> createOneTimeSimulationPlan()
        private static SimulationPlan createOneTimeSimulationPlan(FeatureCollection featureCollection)
        {         
            double rndMinBattery = 4.0;
            double rndMaxBattery = 3.0;

            Random rnd = new Random();

            //create a simulation plan            
            SimulationPlan gpsTrackerSimulationPlan = new SimulationPlan();         

            gpsTrackerSimulationPlan.PlanTiming = PlanTimingEnum.OffsetFromSimulationStart;
            
            foreach (var gpsTrackFeature in featureCollection.Features)
            {
                int index = 0;

                if (gpsTrackFeature.Geometry is GeoJSON.Net.Geometry.MultiLineString)
                {
                    var multiLineString = gpsTrackFeature.Geometry as GeoJSON.Net.Geometry.MultiLineString;

                    foreach (var lineString in multiLineString.Coordinates)
                    {
                        foreach (var coordinate in lineString.Coordinates)
                        {
                            PlannedEvent plannedEvent = new PlannedEvent();
                            plannedEvent.EventIndex = index++;

                            //the GPS Tracker Id
                            plannedEvent.SourceId = gpsTrackFeature.Properties["TrackerId"] as string;
                            plannedEvent.TrackName = plannedEvent.SourceId;

                            plannedEvent.Payload = new GpsTrackerTelemetry() { Latitude = coordinate.Latitude, Longitude= coordinate.Longitude };

                            //sampling rate fixed to 5 seconds 
                            plannedEvent.EventOffset =  TimeSpan.FromSeconds(5);

                            if (!gpsTrackerSimulationPlan.PlannedEventsTracks.ContainsKey(plannedEvent.SourceId))
                            {
                                gpsTrackerSimulationPlan.PlannedEventsTracks.Add(plannedEvent.SourceId, new System.Collections.Generic.List<PlannedEvent>());
                            }

                            gpsTrackerSimulationPlan.PlannedEventsTracks[plannedEvent.SourceId].Add(plannedEvent); 

                        }
                    }
                }
            }

            return gpsTrackerSimulationPlan;
        }
    }
}
