using System.ComponentModel.DataAnnotations;

namespace SmartGear.PM0902.Models;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = "";

    [Required, StringLength(160)]
    public string Slug { get; set; } = "";
}