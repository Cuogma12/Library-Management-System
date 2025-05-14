using QLyTV.Constants;
using QLyTV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BC = BCrypt.Net.BCrypt;

namespace QLyTV.Controllers
{
    public class DocGiaController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=m-cuogdzai;Initial Catalog=QLYTV;Integrated Security=True;TrustServerCertificate=True");
        public ActionResult ListDocGia()
        {
            var docgias = db.Users
                .Where(u => u.UserRoles.Any(r => r.Role.Code == RoleConstants.DocGia) && u.IsActive)
                .Select(u => new UserModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                })
                .ToList();

            return View(docgias);
        }

        [HttpPost]
        public JsonResult Paging(string searchQuery = "", int pageSize = 5, int crrPage = 1)
        {
            try
            {
                var docgias = db.Users
                    .Where(u => u.UserRoles.Any(r => r.Role.Code == RoleConstants.DocGia) && u.IsActive)
                    .AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    docgias = docgias.Where(u => u.Name.Contains(searchQuery) || u.Email.Contains(searchQuery));
                }

                // Tính tổng số trang
                int totalItems = docgias.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Phân trang
                var docgiaPaging = docgias
                    .OrderBy(u => u.Id)
                    .Skip((crrPage - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserModel
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email
                    })
                    .ToList();

                return Json(new
                {
                    data = docgiaPaging,
                    success = true,
                    message = "Lấy dữ liệu thành công",
                    crrPage,
                    pageSize,
                    totalPage = totalPages
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }


        public ActionResult Add()
        {
            return View();
        }

        public ActionResult Edit(int id)
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var docGia = db.Users.FirstOrDefault(o => o.Id == id);
            return View(docGia);
        }

        public ActionResult GetDocGiaDetails(int id)
        {
            var docgia = db.Users
                .Where(u => u.Id == id && u.IsActive && u.UserRoles.Any(r => r.Role.Code == RoleConstants.DocGia))
                .Select(u => new UserModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    Identification = u.Identification,
                })
                .FirstOrDefault();

            if (docgia == null)
            {
                return HttpNotFound("Độc giả không tồn tại hoặc đã bị vô hiệu hóa.");
            }

            return View(docgia);
        }


        [HttpPost]
        public JsonResult AddDocGia(string name, string username, string email, string phoneNumber, string address, string password)
        {
            try
            {
                if (db.Users.Any(u => u.Email == email))
                {
                    return Json(new { success = false, message = "Email đã tồn tại!" });
                }

                var newUser = new User
                {
                    Name = name,
                    UserName = username,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Address = address,
                    Password = BC.HashPassword(password),
                    IsActive = true
                };

                db.Users.InsertOnSubmit(newUser);
                db.SubmitChanges();

                var roleThuThu = db.Roles.FirstOrDefault(r => r.Code == RoleConstants.DocGia);
                if (roleThuThu != null)
                {
                    db.UserRoles.InsertOnSubmit(new UserRole { UserId = newUser.Id, RoleId = roleThuThu.Id });
                    db.SubmitChanges();
                }

                return Json(new { success = true, message = "Thêm độc giả thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EditDocGia(int id, string name, string username, string email, string phoneNumber, string address, string password)
        {
            try
            {
                var docgia = db.Users.FirstOrDefault(u => u.Id == id && u.UserRoles.Any(r => r.Role.Code == RoleConstants.DocGia));
                if (docgia == null)
                {
                    return Json(new { success = false, message = "Độc giả không tồn tại!" });
                }
              
                docgia.Name = name;
                docgia.UserName = username;
                docgia.Email = email;
                docgia.PhoneNumber = phoneNumber;
                docgia.Address = address;
                // Chỉ cập nhật mật khẩu nếu có thay đổi
                if (!string.IsNullOrEmpty(password))
                {
                    docgia.Password = BC.HashPassword(password);
                }
                db.SubmitChanges();

                return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }


        public JsonResult DeactivateDocGia(int id)
        {
            try
            {
                var docgia = db.Users.FirstOrDefault(u => u.Id == id && u.UserRoles.Any(r => r.Role.Code == RoleConstants.DocGia));
                if (docgia == null)
                {
                    return Json(new { success = false, message = "Độc giả không tồn tại!" });
                }

                docgia.IsActive = false;
                db.SubmitChanges();

                return Json(new { success = true, message = "Độc giả đã bị vô hiệu hóa!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

    }
}
