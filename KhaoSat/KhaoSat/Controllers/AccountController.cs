using KhaoSat.Models;
using KhaoSat.Utils;
using Microsoft.AspNetCore.Mvc;

namespace KhaoSat.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly MailHelper _mailHelper;

        public AccountController(AppDbContext context, MailHelper mailHelper)
        {
            _context = context;
            _mailHelper = mailHelper;
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(); 
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var emp = _context.Employees.FirstOrDefault(u => u.Email == email);
            if (emp == null)
            {
                TempData["Error"] = "Email không tồn tại!";
                return RedirectToAction("Login");
            }

            string code = CodeGenerator.GenerateCode();

            HttpContext.Session.SetString("ResetCode", code);
            HttpContext.Session.SetString("ResetEmail", email);

            _mailHelper.SendResetCode(email, code);

            TempData["Message"] = "Mã xác nhận đã được gửi tới email.";
            return RedirectToAction("VerifyCode");
        }

        [HttpGet]
        public IActionResult VerifyCode() => View();

        [HttpPost]
        public IActionResult VerifyCode(string code)
        {
            string savedCode = HttpContext.Session.GetString("ResetCode");
            string email = HttpContext.Session.GetString("ResetEmail");

            if (savedCode == code)
            {
                return RedirectToAction("ResetPassword", new { email });
            }

            TempData["Error"] = "Mã xác nhận không đúng.";
            return View();
        }

        // B3: Hiển thị form đặt lại mật khẩu
        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email, string newPassword)
        {
            var emp = _context.Employees.FirstOrDefault(u => u.Email == email);
            if (emp == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Login");
            }

            emp.Password = newPassword;
            _context.SaveChanges();

            TempData["Message"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login", "Home");
        }
    }
}
