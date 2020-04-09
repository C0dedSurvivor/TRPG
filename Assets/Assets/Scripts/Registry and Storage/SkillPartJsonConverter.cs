using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

class SkillPartJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(SkillPartBase).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        object target = null;

        if (reader.TokenType == JsonToken.Null) return null;

        var jObject = JToken.Load(reader);

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
