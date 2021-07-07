using GeoJSON.Net.Feature;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using YASE.Core;
using YASE.Core.Entities;

namespace SpatialSimulationConverter
{
    class Program
    {
        private static IConfiguration configuration;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello SpatialSimulationConverter!");
            Console.WriteLine("------------------------------");

           configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

            string geoJsonFile = configuration.GetValue<string>("geoJsonFile");

            string simulationPlanFile = configuration.GetValue<string>("simulationPlanFile");

            var geoJson = File.ReadAllText(geoJsonFile);

            //parse the geoJson with the GeoJSON-Net library
            FeatureCollection featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(geoJson);

            SimulationPlan simulationPlan = createOneTimeSimulationPlan(featureCollection);

            var simulationPlanJson = simulationPlan.GetJSON();

            File.WriteAllText(simulationPlanFile, simulationPlanJson);
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
               
                //the structure of a Line GeoJSON from QGIS include a MultiLineString
                if (gpsTrackFeature.Geometry is GeoJSON.Net.Geometry.MultiLineString)
                {
                    var multiLineString = gpsTrackFeature.Geometry as GeoJSON.Net.Geometry.MultiLineString;

                    //..which have a List of LineString
                    foreach (var lineString in multiLineString.Coordinates)
                    {
                        //...every line string has a list of coordinate.
                        //here i'm just adding ALL the position one after the other...   keeping it easy...
                        foreach (var coordinate in lineString.Coordinates)
                        {
                            PlannedOffsettEvent plannedEvent = new PlannedOffsettEvent();
                           
                            //the GPS Tracker Id ---> this is a GeoJSON attribute you need to have in the original GeoJSON file!!!
                            plannedEvent.SourceId = gpsTrackFeature.Properties["TrackerId"] as string;
                           
                            //just a simple simulation of a random value of the battery of the tracker...
                            var battery = rndMinBattery + ((rndMaxBattery - rndMinBattery) * rnd.NextDouble());

                            //here i'm using a specific Class as Payload (GpsTrackerTelemetry) you can feed the simulation with the payload you need!
                            plannedEvent.Payload = new GpsTrackerTelemetry() { Latitude = coordinate.Latitude, Longitude= coordinate.Longitude, Battery = battery };

                            //sampling rate fixed to 5 seconds as an example (eventually this could be obtained as an attribute of the GeoJSON)
                            plannedEvent.EventOffset =  TimeSpan.FromSeconds(5);

                            //save into my plan into a Track for my GPS Tracker simulated device.  
                            //if you have multiple Geometries in the GeoJSON you will end up having multiple parallel tracks 
                            //that will be reproduced in parallel by the Simulation runner application
                            if (!gpsTrackerSimulationPlan.PlannedEventsTracks.ContainsKey(plannedEvent.SourceId))
                            {
                                gpsTrackerSimulationPlan.PlannedEventsTracks.Add(plannedEvent.SourceId, new System.Collections.Generic.List<BaseEvent>());
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
