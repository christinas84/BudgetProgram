using BudgetProgram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BudgetProgram.Controllers
{
    public class HomeController : Controller
    {
        [RequireHttps]
        public ActionResult Index(DemoLogin model)
        {
            model.GuestEmail = "cesimmons84+rory@gmail.com";
            model.GuestPassword = "RoryRory1!";
            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}