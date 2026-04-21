namespace MythApi.Common.Database.Models;

public class Region
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<Mythology> Mythologies { get; set; } = [];
}
