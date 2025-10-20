using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SmartGear.PM0902.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(160)]
    public string Name { get; set; } = "";

    [Range(0, 1_000_000)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice { get; set; }

    [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100%.")]
    public decimal DiscountPercent { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    [Required]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    [BindNever]
    [Required, StringLength(160)]
    public string Slug { get; set; } = "";


    [NotMapped]
    public decimal DiscountedPrice => Math.Round(BasePrice * (1 - (DiscountPercent / 100m)), 2);

    public decimal CalculateTotal(int quantity, decimal vatRate = 0.15m, bool includeVat = true)
    {
        if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity));
        var subtotal = DiscountedPrice * quantity;
        return includeVat ? Math.Round(subtotal * (1 + vatRate), 2) : Math.Round(subtotal, 2);
    }
}