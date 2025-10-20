using Microsoft.AspNetCore.Mvc;

namespace SmartGear.PM0902.ViewComponents;

public class MainNavViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View(); 
    }
}