using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YASE.Core
{
    public class SimulationPlan
    {
        public SimulationPlan()
        {
            PlannedEventsTracks = new Dictionary<string, List<PlannedEvent>>();
        }

        /// <summary>
        /// the way the plan will be executed
        /// </summary>
        public PlanTimingEnum PlanTiming { get; set; }

        /// <summary>
        /// if null --> simulation will start from the beginning of the sequence otherwise it will start with the first item after that time.
        /// </summary>
        public DateTime? SimulationStartTime { get; set; }

        /// <summary>
        /// Loop Simulation is valid only for Offsett Plans not exact time
        /// </summary>
        public int? SimulationLoops { get; set; }

        /// <summary>
        /// the dictionary with the list of planned events
        /// </summary>
        public Dictionary<string, List<PlannedEvent>> PlannedEventsTracks { get; set; }




        /// <summary>
        /// Get JSON reppresentation of this entity.
        /// </summary>
        /// <returns></returns>
        public string GetJSON()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// static builder from JSON file
        /// </summary>
        /// <param name="simulationFile"></param>
        /// <returns></returns>
        public static SimulationPlan LoadFromFile(string simulationFile)
        {
            var simulationPlanJson = File.ReadAllText(simulationFile);

            return FromJson(simulationPlanJson);
        }

        /// <summary>
        /// static builder from the JSON reppresentation
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static SimulationPlan FromJson(string json)
        {
            SimulationPlan  me = Newtonsoft.Json.JsonConvert.DeserializeObject<SimulationPlan>(json);

            return me;
        }
    }
}
