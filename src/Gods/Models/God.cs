using System.ComponentModel.DataAnnotations;

namespace MythApi.Gods.Models;

public class GodInput {
    public int? Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = null!;

    public int MythologyId { get; set; }
}
