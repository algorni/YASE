using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace YASE.Core
{
    public class PlannedEvent 
    {
        /// <summary>
        /// Guid of the simulation run
        /// </summary>
        public Guid SimulationRun { get; set; }

        /// <summary>
        /// Simulation Loop
        /// </summary>
        public int? SimulationLoop { get; set; }

        /// <summary>
        /// the source id (device id? entity? element...)
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// the index into a series of event
        /// </summary>
        public long EventIndex { get; set; }

        /// <summary>
        /// The Original Event time (at source)
        /// </summary>
        public DateTime? OriginalEventTime { get; set; }

        /// <summary>
        /// The time when the Event will be generated
        /// </summary>
        public DateTime? EventTime { get; set; }

        /// <summary>
        /// The offsett (time difference) from the previous generated event 
        /// </summary>
        public TimeSpan? EventOffset { get; set; }

        /// <summary>
        /// The custom payload
        /// </summary>
        public dynamic Payload { get; set; }

        /// <summary>
        /// Get JSON reppresentation of this entity.
        /// </summary>
        /// <returns></returns>
        public string ToJSON()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
