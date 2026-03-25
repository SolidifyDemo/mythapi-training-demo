using System.ComponentModel.DataAnnotations;

namespace MythApi.Gods.Models;

public class GodInput {
    public int? Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Description { get; set; } = null!;

    public int MythologyId { get; set; }
}

