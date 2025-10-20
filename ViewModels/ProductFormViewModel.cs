using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartGear.PM0902.ViewModels;

public class ProductFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(160)]
    public string Name { get; set; } = "";

    [Range(0, 1_000_000)]
    public decimal BasePrice { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }

    [Required]
    public int? CategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
}