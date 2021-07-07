using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


namespace YASE.Core.Entities
{
    public class GeneratedEvent : BaseEvent
    {
       

        /// <summary>
        /// empty ctor
        /// </summary>
        public GeneratedEvent()
        { 
        
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="plannedOffsettEvent"></param>
        public GeneratedEvent(PlannedOffsettEvent plannedOffsettEvent)
        {   
            this.SourceId = plannedOffsettEvent.SourceId;
            this.Payload = plannedOffsettEvent.Payload;
        }

        public GeneratedEvent(PlannedTimedEvent plannedTimedEvent)
        {
            this.SourceId = plannedTimedEvent.SourceId;
            this.Payload = plannedTimedEvent.Payload;
            this.EventTime = plannedTimedEvent.PlannedEventTime;
        }

        /// <summary>
        /// Guid of the simulation run
        /// </summary>
        public Guid CurrentSimulationRun { get; set; }

        /// <summary>
        /// Simulation Loop
        /// </summary>
        public int CurrentSimulationLoop { get; set; }

        /// <summary>
        /// Simulation Track Name (you can have multiple track per a Plan
        /// </summary>
        public string TrackName { get; set; }

        /// <summary>
        /// The index into a series of event in the same track
        /// </summary>
        public long EventIndex { get; set; }

        /// <summary>
        /// The time when the Event will be generated (by the Runner)
        /// </summary>
        public DateTime EventTime { get; set; }


        ///// <summary>
        ///// Get JSON reppresentation of this entity.
        ///// </summary>
        ///// <returns></returns>
        //public string ToJSON()
        //{
        //    return Newtonsoft.Json.JsonConvert.SerializeObject(this, Formatting.Indented);
        //}
    }
}
