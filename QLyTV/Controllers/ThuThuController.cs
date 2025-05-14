using QLyTV.Constants;
using QLyTV.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BC = BCrypt.Net.BCrypt;

namespace QLyTV.Controllers
{
    public class ThuThuController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=m-cuogdzai;Initial Catalog=QLYTV;Integrated Security=True;TrustServerCertificate=True");
        public ActionResult ListLibrarians()
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var librarians = db.Users
                .Where(u => u.UserRoles.Any(r => r.Role.Code == RoleConstants.ThuThu) && u.IsActive)
                .Select(u => new UserModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                })
                .ToList();

            return View(librarians);
        }

        [HttpPost]
        public JsonResult Paging(string searchQuery = "", int pageSize = 5, int crrPage = 1)
        {
            try
            {
                var librarians = db.Users
                    .Where(u => u.UserRoles.Any(r => r.Role.Code == RoleConstants.ThuThu) && u.IsActive)
                    .AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    librarians = librarians.Where(l => l.Name.Contains(searchQuery) || l.Email.Contains(searchQuery));
                }

                // Tính tổng số trang
                int totalItems = librarians.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Phân trang
                var librarianPaging = librarians
                    .OrderBy(l => l.Id)
                    .Skip((crrPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList() // Lấy dữ liệu từ cơ sở dữ liệu trước
                    .Select(l => new
                    {
                        l.Id,
                        l.Name,
                        l.Email
                    })
                    .ToList();

                return Json(new
                {
                    data = librarianPaging,
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
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }


        public ActionResult GetLibrarianDetails(int id)
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var librarian = db.Users
                .Where(u => u.Id == id && u.IsActive && u.UserRoles.Any(r => r.Role.Code == RoleConstants.ThuThu))
                .Select(u => new UserModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Name = u.Name,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    Identification = u.Identification,

                })
                .FirstOrDefault();

            if (librarian == null)
            {
                return HttpNotFound("Thủ thư không tồn tại hoặc đã bị vô hiệu hóa.");
            }

            return View(librarian);
        }

        [HttpPost]
        public JsonResult AddLibrarian(string name, string username, string email, string phoneNumber, string address, string password)
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

                var roleThuThu = db.Roles.FirstOrDefault(r => r.Code == RoleConstants.ThuThu);
                if (roleThuThu != null)
                {
                    db.UserRoles.InsertOnSubmit(new UserRole { UserId = newUser.Id, RoleId = roleThuThu.Id });
                    db.SubmitChanges();
                }

                return Json(new { success = true, message = "Thêm thủ thư thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var thuThu = db.Users.FirstOrDefault(o => o.Id == id);
            return View(thuThu);
        }

        [HttpPost]
        public JsonResult PostEdit(UserModel thuthu)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingThuThu = db.Users.FirstOrDefault(s => s.Id == thuthu.Id);
                    if (existingThuThu == null)
                    {
                        return Json(new { success = false, message = "Thủ thư không tồn tại!" });
                    }

                    // Kiểm tra trường Name
                    if (string.IsNullOrEmpty(thuthu.Name))
                    {
                        return Json(new { success = false, message = "Tên không được để trống!" });
                    }

                    string pass = Request.Form["Password"];
                    string repass = Request.Form["repassword"];

                    if (!string.IsNullOrEmpty(pass) || !string.IsNullOrEmpty(repass))
                    {
                        if (pass != repass)
                        {
                            return Json(new { success = false, message = "Mật khẩu và xác nhận mật khẩu không khớp!" });
                        }

                        existingThuThu.Password = BC.HashPassword(pass);
                    }

                    existingThuThu.Name = thuthu.Name;
                    existingThuThu.UserName = thuthu.UserName;
                    existingThuThu.Email = thuthu.Email;
                    existingThuThu.Address = thuthu.Address;
                    existingThuThu.PhoneNumber = thuthu.PhoneNumber;
                    existingThuThu.Identification = thuthu.Identification;

                    db.SubmitChanges();

                    return Json(new { success = true, message = "Cập nhật thủ thư thành công!" });
                }

                return Json(new { success = false, message = "Có lỗi xảy ra!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }


        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                if (id == 0)
                    return Json(new { success = false, message = "ID không hợp lệ!" });

                var thuThu = db.Users.FirstOrDefault(d => d.Id == id && d.IsActive == true);
                if (thuThu == null)
                    return Json(new { success = false, message = "Thủ thư không tồn tại hoặc đã bị xoá!" });

                // Đánh dấu thủ thư là không hoạt động
                thuThu.IsActive = false;
                db.SubmitChanges();

                return Json(new { success = true, message = "Xóa thủ thư thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}