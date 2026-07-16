using System.ComponentModel.DataAnnotations;

namespace LensLab.Data.Entities;

/// <summary>
/// Represents a custom filter preset stored in the database.
/// </summary>
public class CustomFilter
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string JsonData { get; set; } = string.Empty;
}
