using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

/// <summary>
/// Allows for skill parts to be read in from JSON correctly since inheritance can cause problems
/// </summary>
class SkillPartJsonConverter : JsonConverter
{
    /// <summary>
    /// Makes sure this only tries to convert objects that could be a SkillPartBase
    /// </summary>
    /// <param name="objectType">The type of object being converted</param>
    /// <returns>If it is some class inheriting from SkillPartBase</returns>
    public override bool CanConvert(Type objectType)
    {
        return typeof(SkillPartBase).IsAssignableFrom(objectType);
    }

    /// <summary>
    /// Takes in a JsonStream that is based from SkillPartBase and returns what effect it should be read as
    /// </summary>
    /// <returns>An empty object of the type the given data can fill</returns>
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
        else if (jObject["chanceOutOf"] != null)
        {
            target = new ConnectedChancePart();
        }
        else
        {
            throw new ArgumentException("Invalid source type");
        }

        serializer.Populate(jObject.CreateReader(), target);

        return target;
    }

    /// <summary>
    /// Writing is not necessary as the game should never write this object type
    /// </summary>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
