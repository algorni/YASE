using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Converters;

namespace YASE.Core.Entities
{
    public class JSONEventConverter : JsonCreationConverter<BaseEvent>
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //We don't deal with writing json, generally newtonsoft would make a good job of
            //serializing these type of objects without having to use a custom writer anyway
        }

        protected override BaseEvent Create(Type objectType, JObject jObject)
        {
            if (jObject.ContainsKey("PlannedEventTime"))
                return new PlannedTimedEvent();
        
            if (jObject.ContainsKey("EventOffset"))
                return new PlannedOffsettEvent();

            if (jObject.ContainsKey("EventTime"))
                return new GeneratedEvent();

            throw new ApplicationException($"The given Base Event type is not supported!: {jObject.ToString()}");
        }
    }




    //Generic converter class - could combine with above class if you're only dealing
    //with one inheritance chain, but this way it's reusable
    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            T target = Create(objectType, jObject);

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }
    }
}
