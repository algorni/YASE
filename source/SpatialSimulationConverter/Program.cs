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
            Console.WriteLine();

            configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

            string geoJsonFile = configuration.GetValue<string>("geoJsonFile");
            Console.WriteLine($"Opening the GeoJSON file: {geoJsonFile}");

            string simulationPlanFile = configuration.GetValue<string>("simulationPlanFile");
            Console.WriteLine($"Output Simulation Plan will be saved as: {simulationPlanFile}");

            var geoJson = File.ReadAllText(geoJsonFile);

            //parse the geoJson with the GeoJSON-Net library
            FeatureCollection featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(geoJson);

            SimulationPlan simulationPlan = createOneTimeSimulationPlan(featureCollection);

            Console.WriteLine("Simulation Plan generated");

            var simulationPlanJson = simulationPlan.GetJSON();

            File.WriteAllText(simulationPlanFile, simulationPlanJson);
        }


        /// <summary>
        /// This is a sample code to generate a simulation...   that can be playbacked by the runner
        /// 
        /// The overall story is: i need a GeoJSON with MultilineString geometries and an attribute (for each geometry) named "TrackerId" 
        /// The Tracker Id will be used as both source id and Track name in the simulation.
        /// Each GPS position will be emitted with 5 seconds of delay beween each other. 
        /// </summary>
        /// <returns></returns>
        //private static SimulationPlan<CarTrackingKmEvent> createOneTimeSimulationPlan()
        private static SimulationPlan createOneTimeSimulationPlan(FeatureCollection featureCollection)
        {                   
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
                          
                            //here i'm using a specific Class as Payload (GpsTrackerTelemetry) you can feed the simulation with the payload you need!
                            //if you want to generate additional payload just change this code here!!
                            plannedEvent.Payload = new GpsTrackerTelemetry() { Latitude = coordinate.Latitude, Longitude= coordinate.Longitude};

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
