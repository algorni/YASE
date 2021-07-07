using System;
using System.Collections.Generic;
using System.Text;

namespace YASE.Core.Entities
{
    public class PlannedTimedEvent : BaseEvent
    {
        /// <summary>
        /// The time when the Event should be generated
        /// </summary>
        public DateTime PlannedEventTime { get; set; }
    }
}
