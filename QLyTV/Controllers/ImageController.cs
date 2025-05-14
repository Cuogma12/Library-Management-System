using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLyTV.Controllers
{
    public class ImageController : Controller
    {
        // GET: Image
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult ChinhSua()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChinhSua(HttpPostedFileBase hinhanh)
        {
            if (hinhanh != null && hinhanh.ContentLength > 0)
            {
                // Lấy tên file ảnh
                var fileName = Path.GetFileName(hinhanh.FileName);
                // Xác định đường dẫn để lưu ảnh
                var path = Path.Combine(Server.MapPath("~/Images/BackGround/"), fileName);
                // Lưu ảnh vào thư mục
                hinhanh.SaveAs(path);

                // Lưu đường dẫn ảnh vào cơ sở dữ liệu (hoặc trong model)
                var imageUrl = "/Images/BackGround/" + fileName;

                // Bạn có thể lưu vào model hoặc ViewBag để sử dụng trong View
                ViewBag.ImageUrl = imageUrl;

                // Redirect hoặc trả về view sau khi lưu
                return RedirectToAction("ChinhSua");
            }

            return View();
        }
    }
}