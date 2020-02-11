using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class SkillPartJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(SkillPartBase).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jObject = JToken.Load(reader);
        object target = null;

        //If the object doesn't have a value it is null
        if (!jObject.HasValues)
            return target;

        if (jObject["effect"] != null)
        {
            target = new AddTriggerPart();
        }
        else if (jObject["damage"] != null)
        {
            target = new DamagePart();
        }
        else if (jObject["healing"] != null)
        {
            target = new HealingPart();
        }
        else if (jObject["direction"] != null)
        {
            target = new MovePart();
        }
        else if (jObject["statMod"] != null)
        {
            target = new StatChangePart();
        }
        else if (jObject["status"] != null)
        {
            target = new StatusEffectPart();
        }
        else if (jObject["effectType"] != null)
        {
            target = new UniqueEffectPart();
        }
        else
        {
            throw new ArgumentException("Invalid source type");
        }

        serializer.Populate(jObject.CreateReader(), target);

        return target;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
