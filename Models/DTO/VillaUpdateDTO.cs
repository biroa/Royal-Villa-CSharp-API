namespace RoyalVillaApi.Models.DTO;
using System.ComponentModel.DataAnnotations;

public class VillaUpdateDTO
{
    [MaxLength(50)]
    [Required]
    public required string Name { get; set; }
    public string? Details { get; set; }
    public double Rate { get; set; }
    public int Sqft { get; set; }
    public int Occupancy { get; set; }
    public string? ImageUrl { get; set; }

}