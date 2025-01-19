using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Dtos;

/// <summary>
/// DTO for adding/updating a spell via the API.  
/// Includes all commonly used fields from the Spell entity.
/// </summary>
public class SpellRequest
{
    [Required]
    [MaxLength(50)]
    public string Id { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "Not Available";

    public List<string> Desc { get; set; } = new();
    public List<string> HigherLevel { get; set; } = new();

    [MaxLength(50)]
    public string Range { get; set; } = "Not Available";

    /// <summary>
    /// Typically a list of string components like ["V","S","M"]
    /// </summary>
    public List<string> Components { get; set; } = new();

    [MaxLength(600)]
    public string Material { get; set; } = "Not Available";

    public bool Ritual { get; set; } = false;

    [MaxLength(50)]
    public string Duration { get; set; } = "Not Available";

    public bool Concentration { get; set; } = false;

    [MaxLength(50)]
    public string CastingTime { get; set; } = "Not Available";

    public int Level { get; set; } = 0;

    [MaxLength(50)]
    public string AttackType { get; set; } = "Not Available";

    /// <summary>
    /// A reference to School (by its Id)
    /// </summary>
    [MaxLength(50)]
    public string? SchoolId { get; set; }

    /// <summary>
    /// A list of string IDs representing classes 
    /// (e.g. ["wizard","sorcerer"])
    /// </summary>
    public List<string> ClassIds { get; set; } = new();

    /// <summary>
    /// A list of string IDs representing subclasses
    /// </summary>
    public List<string> SubclassIds { get; set; } = new();

    [MaxLength(200)]
    public string Url { get; set; } = "Not Available";

    /// <summary>
    /// Set this if the client wants to override the updated date,
    /// else the repository can set it automatically
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}