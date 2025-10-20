using Microsoft.AspNetCore.Mvc;

namespace SmartGear.PM0902.ViewComponents;

public class CategoriesViewComponent : ViewComponent
{
    private static readonly string[] Categories = new[] { "Jerseys", "Shorts", "Shoes", "Bags" };

    public IViewComponentResult Invoke()
        => View(Categories);
}