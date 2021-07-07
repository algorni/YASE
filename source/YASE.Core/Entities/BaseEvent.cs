using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace YASE.Core.Entities
{
    public abstract class BaseEvent
    {
        /// <summary>
        /// the source id (device id? entity? element...)
        /// </summary>
        public string SourceId { get; set; }

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
