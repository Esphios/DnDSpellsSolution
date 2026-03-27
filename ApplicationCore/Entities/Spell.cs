using ApplicationCore.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ApplicationCore.Entities;

public interface IHasStringId
{
    string Id { get; set; }
}

public class Class : IHasStringId
{
    [JsonProperty("index")]
    [MaxLength(50)]
    public required string Id { get; set; }

    [JsonProperty("name")]
    [MaxLength(100)]
    public string Name { get; set; } = Defaults.NotAvailable;

    [JsonProperty("url")]
    [MaxLength(200)]
    public string Url { get; set; } = Defaults.NotAvailable;

    public List<Spell> Spells { get; set; } = [];
}

public class Damage : IHasStringId
{
    [JsonProperty("index")]
    [MaxLength(50)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("damage_type")]
    public DamageType? DamageType { get; set; }

    [JsonProperty("damage_at_slot_level")]
    public DamageAtSlotLevel? DamageAtSlotLevel { get; set; } = new();
}

public class DamageAtSlotLevel : IHasStringId
{
    [JsonProperty("index")]
    [MaxLength(50)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("0")]
    [MaxLength(50)]
    public string _0 { get; set; } = Defaults.NotAvailable;

    [JsonProperty("1")]
    [MaxLength(50)]
    public string _1 { get; set; } = Defaults.NotAvailable;

    [JsonProperty("2")]
    [MaxLength(50)]
    public string _2 { get; set; } = Defaults.NotAvailable;

    [JsonProperty("3")]
    [MaxLength(50)]
    public string _3 { get; set; } = Defaults.NotAvailable;

    [JsonProperty("4")]
    [MaxLength(50)]
    public string _4 { get; set; } = Defaults.NotAvailable;

    [JsonProperty("5")]
    [MaxLength(50)]
    public string _5 { get; set; } = Defaults.NotAvailable;

    [JsonProperty("6")]
    [MaxLength(50)]
    public string _6 { get; set; } = Defaults.NotAvailable;

    [JsonProperty("7")]
    [MaxLength(50)]
    public string _7 { get; set; } = Defaults.NotAvailable;

    [JsonProperty("8")]
    [MaxLength(50)]
    public string _8 { get; set; } = Defaults.NotAvailable;

    [JsonProperty("9")]
    [MaxLength(50)]
    public string _9 { get; set; } = Defaults.NotAvailable;
}

public class DamageType : IHasStringId
{
    [JsonProperty("index")]
    [MaxLength(50)]
    public required string Id { get; set; }

    [JsonProperty("name")]
    [MaxLength(100)]
    public string Name { get; set; } = Defaults.NotAvailable;

    [JsonProperty("url")]
    [MaxLength(200)]
    public string Url { get; set; } = Defaults.NotAvailable;
}

public class Spell : IHasStringId
{
    [JsonProperty("index")]
    [MaxLength(50)]
    public required string Id { get; set; }

    [JsonProperty("name")]
    [MaxLength(100)]
    public string Name { get; set; } = Defaults.NotAvailable;

    [JsonProperty("desc")]
    public List<string> Desc { get; set; } = [];

    [JsonProperty("higher_level")]
    public List<string> HigherLevel { get; set; } = [];

    [JsonProperty("range")]
    [MaxLength(50)]
    public string Range { get; set; } = Defaults.NotAvailable;

    [JsonProperty("components")]
    [MaxLength(300)]
    public List<string> Components { get; set; } = [];

    [JsonProperty("material")]
    [MaxLength(600)]
    public string Material { get; set; } = Defaults.NotAvailable;

    [JsonProperty("ritual")]
    public bool Ritual { get; set; }

    [JsonProperty("duration")]
    [MaxLength(50)]
    public string Duration { get; set; } = Defaults.NotAvailable;

    [JsonProperty("concentration")]
    public bool Concentration { get; set; }

    [JsonProperty("casting_time")]
    [MaxLength(50)]
    public string CastingTime { get; set; } = Defaults.NotAvailable;

    [JsonProperty("level")]
    public int Level { get; set; }

    [JsonProperty("attack_type")]
    [MaxLength(50)]
    public string AttackType { get; set; } = Defaults.NotAvailable;

    [JsonProperty("damage")]
    public Damage? Damage { get; set; } = new();

    [JsonProperty("school")]
    public School? School { get; set; }

    [JsonProperty("classes")]
    public List<Class> Classes { get; set; } = [];

    [JsonProperty("subclasses")]
    public List<Subclass> Subclasses { get; set; } = [];

    [JsonProperty("url")]
    [MaxLength(200)]
    public string Url { get; set; } = Defaults.NotAvailable;

    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public class IgnoreIdsContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        if (property.PropertyName != null &&
            (property.PropertyName.Equals("id", StringComparison.OrdinalIgnoreCase) ||
             property.PropertyName.Equals("index", StringComparison.OrdinalIgnoreCase)))
        {
            property.Ignored = true;
        }

        return property;
    }
}

public class School : IHasStringId
{
    [JsonProperty("index")]
    [MaxLength(50)]
    public required string Id { get; set; }

    [JsonProperty("name")]
    [MaxLength(100)]
    public string Name { get; set; } = Defaults.NotAvailable;

    [JsonProperty("url")]
    [MaxLength(200)]
    public string Url { get; set; } = Defaults.NotAvailable;
}

public class Subclass : IHasStringId
{
    [JsonProperty("index")]
    [MaxLength(50)]
    public required string Id { get; set; }

    [JsonProperty("name")]
    [MaxLength(100)]
    public string Name { get; set; } = Defaults.NotAvailable;

    [JsonProperty("url")]
    [MaxLength(200)]
    public string Url { get; set; } = Defaults.NotAvailable;
}
