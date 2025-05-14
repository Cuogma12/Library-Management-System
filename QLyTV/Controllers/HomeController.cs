using QLyTV.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;


namespace QLyTV.Controllers
{
    public class HomeController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=m-cuogdzai;Initial Catalog=QLYTV;Integrated Security=True;TrustServerCertificate=True");
        public ActionResult Index()
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TenDangNhap = Session["tendangnhap"];
            ViewBag.HoTen = Session["hoten"];
            return View();
        }

        public ActionResult About()
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

          
            return View();
        }

        public ActionResult Contact()
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            
            return View();
        }
    }
}