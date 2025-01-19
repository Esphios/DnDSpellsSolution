using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Entities
{
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
        public string Name { get; set; } = "Not Available";

        [JsonProperty("url")]
        [MaxLength(200)]
        public string Url { get; set; } = "Not Available";

        public List<Spell> Spells { get; set; } = [];
    }

    public class Damage : IHasStringId
    {
        [JsonProperty("index")]
        [MaxLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("damage_type")]
        public DamageType? DamageType { get; set; } = null;

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
        public string _0 { get; set; } = "Not Available";

        [JsonProperty("1")]
        [MaxLength(50)]
        public string _1 { get; set; } = "Not Available";

        [JsonProperty("2")]
        [MaxLength(50)]
        public string _2 { get; set; } = "Not Available";

        [JsonProperty("3")]
        [MaxLength(50)]
        public string _3 { get; set; } = "Not Available";

        [JsonProperty("4")]
        [MaxLength(50)]
        public string _4 { get; set; } = "Not Available";

        [JsonProperty("5")]
        [MaxLength(50)]
        public string _5 { get; set; } = "Not Available";

        [JsonProperty("6")]
        [MaxLength(50)]
        public string _6 { get; set; } = "Not Available";

        [JsonProperty("7")]
        [MaxLength(50)]
        public string _7 { get; set; } = "Not Available";

        [JsonProperty("8")]
        [MaxLength(50)]
        public string _8 { get; set; } = "Not Available";

        [JsonProperty("9")]
        [MaxLength(50)]
        public string _9 { get; set; } = "Not Available";
    }

    public class DamageType : IHasStringId
    {
        [JsonProperty("index")]
        [MaxLength(50)]
        public required string Id { get; set; }

        [JsonProperty("name")]
        [MaxLength(100)]
        public string Name { get; set; } = "Not Available";

        [JsonProperty("url")]
        [MaxLength(200)]
        public string Url { get; set; } = "Not Available";
    }

    public class Spell : IHasStringId
    {
        [JsonProperty("index")]
        [MaxLength(50)]
        public required string Id { get; set; }

        [JsonProperty("name")]
        [MaxLength(100)]
        public string Name { get; set; } = "Not Available";

        [JsonProperty("desc")]
        public List<string> Desc { get; set; } = [];

        [JsonProperty("higher_level")]
        public List<string> HigherLevel { get; set; } = [];

        [JsonProperty("range")]
        [MaxLength(50)]
        public string Range { get; set; } = "Not Available";

        [JsonProperty("components")]
        [MaxLength(300)]
        public List<string> Components { get; set; } = [];

        [JsonProperty("material")]
        [MaxLength(600)]
        public string Material { get; set; } = "Not Available";

        [JsonProperty("ritual")]
        public bool Ritual { get; set; } = false;

        [JsonProperty("duration")]
        [MaxLength(50)]
        public string Duration { get; set; } = "Not Available";

        [JsonProperty("concentration")]
        public bool Concentration { get; set; } = false;

        [JsonProperty("casting_time")]
        [MaxLength(50)]
        public string CastingTime { get; set; } = "Not Available";

        [JsonProperty("level")]
        public int Level { get; set; } = 0;

        [JsonProperty("attack_type")]
        [MaxLength(50)]
        public string AttackType { get; set; } = "Not Available";

        [JsonProperty("damage")]
        public Damage? Damage { get; set; } = new();

        [JsonProperty("school")]
        public School? School { get; set; } = null;

        [JsonProperty("classes")]
        public List<Class> Classes { get; set; } = [];

        [JsonProperty("subclasses")]
        public List<Subclass> Subclasses { get; set; } = [];

        [JsonProperty("url")]
        [MaxLength(200)]
        public string Url { get; set; } = "Not Available";

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class IgnoreIdsContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyName != null)
            {
                // If the property name is "id" or "index" (in any case), ignore it
                if (property.PropertyName.Equals("id", StringComparison.OrdinalIgnoreCase) ||
                    property.PropertyName.Equals("index", StringComparison.OrdinalIgnoreCase))
                {
                    property.Ignored = true;
                }
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
        public string Name { get; set; } = "Not Available";

        [JsonProperty("url")]
        [MaxLength(200)]
        public string Url { get; set; } = "Not Available";
    }

    public class Subclass : IHasStringId
    {
        [JsonProperty("index")]
        [MaxLength(50)]
        public required string Id { get; set; }

        [JsonProperty("name")]
        [MaxLength(100)]
        public string Name { get; set; } = "Not Available";

        [JsonProperty("url")]
        [MaxLength(200)]
        public string Url { get; set; } = "Not Available";
    }
}