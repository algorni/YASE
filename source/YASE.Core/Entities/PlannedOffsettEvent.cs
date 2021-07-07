using System;
using System.Collections.Generic;
using System.Text;

namespace YASE.Core.Entities
{
    public class PlannedOffsettEvent : BaseEvent
    {
        /// <summary>
        /// The offsett (time difference) from the previous generated event 
        /// </summary>
        public TimeSpan EventOffset { get; set; }
    }
}
