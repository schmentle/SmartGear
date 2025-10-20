using System.ComponentModel.DataAnnotations;

namespace SmartGear.PM0902.Models;

public sealed class ProductCreateDto
{
    [Required, StringLength(160)]
    public string Name { get; set; } = "";
    [Range(0, 999999)]
    public decimal BasePrice { get; set; }
    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
    [Required]
    public int CategoryId { get; set; }
}