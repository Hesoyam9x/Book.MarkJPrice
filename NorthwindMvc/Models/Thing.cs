using System.ComponentModel.DataAnnotations;

namespace NorthwindMvc.Models;

public class Thing
{
    [Range(1, 10)]
    public int? Id { get; set; }
    [Required]
    public string? Color { get; set; }
}