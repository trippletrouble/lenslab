using System;
using System.ComponentModel.DataAnnotations;

namespace LensLab.Data.Entities;

/// <summary>
/// Represents a record of a recently opened image file.
/// </summary>
public class RecentFile
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    public string FileName { get; set; } = string.Empty;

    public DateTime LastOpened { get; set; } = DateTime.Now;
}
