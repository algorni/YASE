using System;
using System.Collections.Generic;
using System.Text;

namespace SpatialSimulationConverter
{
    /// <summary>
    /// If, together with the GPS position, you want to add additional payload just amend this class and feed with what you need. 
    /// </summary>
    public class GpsTrackerTelemetry
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
