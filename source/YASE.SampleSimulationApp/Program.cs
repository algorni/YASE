﻿using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using YASE.Core;

namespace YASE.SampleSimulationApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Simulation!");


            SimulationPlan simulationPlan = createOneTimeSimulationPlan();
            
            var simulationPlanJson = simulationPlan.GetJSON();


            File.WriteAllText("c:\\tmp\\simulation.json", simulationPlanJson);
        }

      
        /// <summary>
        /// This is a sample code to generate a simulation...   that can be playbacked by the runner
        /// </summary>
        /// <returns></returns>
        //private static SimulationPlan<CarTrackingKmEvent> createOneTimeSimulationPlan()
        private static SimulationPlan createOneTimeSimulationPlan()
        {
            long numberOfItem = 1000;

            double rndMinKm = 1.0;
            double rndMaxKm = 1.0;

            TimeSpan rndMinSpan = TimeSpan.FromSeconds(5.0);
            TimeSpan rndMaxSpan = TimeSpan.FromSeconds(5.0);

            int carIdMin = 0;
            int carIdMax = 0;

            Random rnd = new Random();

            //create a simulation plan            
            SimulationPlan carTrackingKmSimulationPlan = new SimulationPlan();

            carTrackingKmSimulationPlan.PlanTiming = PlanTimingEnum.OffsetFromSimulationStart;
            
            for (int index = 0; index < numberOfItem; index++)
            {
                //CarTrackingKmEvent plannedEvent = new CarTrackingKmEvent();
                PlannedEvent plannedEvent = new PlannedEvent();
                plannedEvent.EventIndex = index;
              
                plannedEvent.SourceId = "SimulatedCar" + rnd.Next(carIdMin, carIdMax).ToString();

                plannedEvent.Payload = new CarTrackingKmEventPayload() { KmIncremental = rndMinKm + ((rndMaxKm - rndMinKm) * rnd.NextDouble()) };

                plannedEvent.EventOffset = new TimeSpan(rndMinSpan.Ticks + (long)((rndMaxSpan.Ticks - rndMinSpan.Ticks) * rnd.NextDouble()));

                carTrackingKmSimulationPlan.PlannedEvents.Add(plannedEvent);
            }

            return carTrackingKmSimulationPlan;
        }
    }
}
