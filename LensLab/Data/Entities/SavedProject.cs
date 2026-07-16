using System;
using System.ComponentModel.DataAnnotations;

namespace LensLab.Data.Entities;

/// <summary>
/// Represents a saved editing project.
/// </summary>
public class SavedProject
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ProjectName { get; set; } = string.Empty;

    [Required]
    public string ImagePath { get; set; } = string.Empty;

    public string AdjustmentSettingsJson { get; set; } = string.Empty;

    public string? SelectedFilterName { get; set; }

    public double FilterOpacity { get; set; }

    public DateTime LastSaved { get; set; } = DateTime.Now;
}
