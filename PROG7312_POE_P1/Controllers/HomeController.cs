using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PROG7312_POE_P1.Models;

namespace PROG7312_POE_P1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            //returns view associated with this action
            return View();
        }
    }
}
