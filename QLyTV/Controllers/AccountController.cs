using QLyTV.Constants;
using QLyTV.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BC = BCrypt.Net.BCrypt;

namespace QLyTV.Controllers
{
    public class AccountController : Controller
    {
        DataClasses1DataContext db = new DataClasses1DataContext("Data Source=m-cuogdzai;Initial Catalog=QLYTV;Integrated Security=True;TrustServerCertificate=True");
        // GET: Đăng ký
        public ActionResult Register()
        {
            return View();
        }

        // POST: Xử lý đăng ký
        [HttpPost]
        public JsonResult postRegister()
        {
            try
            {
                // request các giá trị từ form
                string tendangnhap = Request.Form["tendangnhap"];
                string matkhau = Request.Form["matkhau"];
                string hoten = Request.Form["hoten"];
                string cccd = Request.Form["cccd"];
                string email = Request.Form["email"];
                string sdt = Request.Form["sdt"];
                string diachi = Request.Form["diachi"];

                var isExistedEmail = db.Users.Any(o => o.Email == email);
                if (isExistedEmail)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Email đã tồn tại"
                    }, JsonRequestBehavior.AllowGet);
                }

                var isExistedUserName= db.Users.Any(o => o.UserName == tendangnhap);
                if (isExistedEmail)
                {
                    return Json(new
                    {
                        success = false,
                        message = "tên đăng nhập đã tồn tại"
                    }, JsonRequestBehavior.AllowGet);
                }

                var newObj = new User();
                newObj.UserName = tendangnhap;
                newObj.Password = BC.HashPassword(matkhau);
                newObj.Name = hoten;
                newObj.Identification = cccd;
                newObj.Email = email;
                newObj.PhoneNumber = sdt;
                newObj.Address = diachi;
                newObj.IsActive = true; // Kích hoạt tài khoản

                db.Users.InsertOnSubmit(newObj);
                db.SubmitChanges();

                var user = db.Users.FirstOrDefault(x => x.Email == email);
                var roleDocGia = db.Roles.FirstOrDefault(x => x.Code == RoleConstants.DocGia);
                //tạo role
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleDocGia.Id,
                };

                db.UserRoles.InsertOnSubmit(userRole);
                db.SubmitChanges();

                return Json(new
                {
                    success = true,
                    message = "Đăng ký thành công"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Đăng nhập
        public ActionResult Login()
        {
            var roles = db.Roles.Where(x => x.IsActive).Select(x => new RoleModel
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code
            })
            .OrderBy(x => x.Name)
            .ToList();

            return View(roles);
        }

        // POST: Xử lý đăng nhập
        [HttpPost]
        public JsonResult postLogin(LoginRequestModel model)
        {
            try
            {
                var user = db.Users
                        .FirstOrDefault(x => x.Email == model.Email
                                    && x.UserRoles.Any(ur => ur.RoleId == model.RoleId));
               
                if (user != null && BC.Verify(model.Password, user.Password))
                {
                    Session["UserId"] = user.Id;
                    Session["FullName"] = user.Name;
                    Session["Role"] = user.UserRoles.FirstOrDefault().Role.Code;
                    Session["Email"] = user.Email;
                    return Json(new { success = true, message = "Đăng nhập thành công" }, JsonRequestBehavior.AllowGet);
                }

                // Nếu không tìm thấy tài khoản
                return Json(new { success = false, message = "Email hoặc mật khẩu không đúng" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: "+ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Đăng xuất

        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        public ActionResult AccountInfo()
        {
            int userId = Convert.ToInt32(Session["UserId"] ?? 1); // Giả sử Session lưu UserId
            AccountViewModel viewModel = GetAccountDetails(userId);

            return View(viewModel);

        }
        [HttpPost]
        public ActionResult EditAccount(User updatedUser)
        {
            try
            {
                // Tìm tài khoản trong cơ sở dữ liệu
                var user = db.Users.FirstOrDefault(u => u.Id == updatedUser.Id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài khoản.";
                    return RedirectToAction("AccountInfo", new { userId = updatedUser.Id });
                }
                string pass = Request.Form["Password"];
                string repass = Request.Form["repassword"];

                if (!string.IsNullOrEmpty(pass) || !string.IsNullOrEmpty(repass))
                {
                    if (pass != repass)
                    {
                        return Json(new { success = false, message = "Mật khẩu và xác nhận mật khẩu không khớp!" });
                    }

                    user.Password = BC.HashPassword(pass);
                }

                // Cập nhật thông tin tài khoản
                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;
                user.PhoneNumber = updatedUser.PhoneNumber;
                user.Address = updatedUser.Address;
                user.Identification = updatedUser.Identification;

                db.SubmitChanges();

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("AccountInfo", new { userId = updatedUser.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("AccountInfo", new { userId = updatedUser.Id });
            }
        }



        // Hàm lấy thông tin tài khoản và sách mượn từ database
        private AccountViewModel GetAccountDetails(int userId)
        {
            var viewModel = new AccountViewModel();

            // Lấy thông tin User
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                viewModel.UserInfo = new User
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Identification = user.Identification
                };
            }

            // Lấy thông tin sách đã mượn
            var borrowedBooks = from pm in db.PhieuMuons
                                join ctpm in db.ChiTietPhieuMuons on pm.MaPhieuMuon equals ctpm.MaPhieuMuon
                                join sach in db.Saches on ctpm.MaSach equals sach.MaSach
                                where pm.MaDocGia == userId
                                select new BookBorrowedDetail
                                {
                                    MaSach = sach.MaSach,
                                    TenSach = sach.TenSach,
                                    NgayMuon = (DateTime)pm.NgayMuon,
                                    //SoLuong = ctpm.SoLuong ?? 0,
                                    NgayTraDuKien = (DateTime)pm.NgayTraDuKien 
                                };

            viewModel.BorrowedBooks = borrowedBooks.ToList();

            return viewModel;
        }
    }

}
