using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using KhaoSat.Models;
using System.Linq;
using System.Threading.Tasks;

namespace KhaoSat.Middleware
{
    public class RoleRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method;
            var empId = context.Session.GetInt32("EmployeeId");

            // 1. Chưa login → redirect về Login nếu truy cập trang khác
            if (empId == null && path != "/home/login")
            {
                context.Response.Redirect("/Home/Login");
                return;
            }

            // 2. Đã login nhưng truy cập Login → redirect theo role
            if (empId != null && path == "/home/login")
            {
                var roleName = context.Session.GetString("RoleName") ?? "";
                if (roleName == "Admin")
                    context.Response.Redirect("/Home/AdminIndex");
                else
                    context.Response.Redirect("/Home/UserIndex");
                return;
            }

            // 3. Xử lý POST Login
            if (path == "/home/login" && method == "POST")
            {
                var form = await context.Request.ReadFormAsync();
                var email = form["email"].ToString();
                var password = form["password"].ToString();

                var db = context.RequestServices.GetService(typeof(AppDbContext)) as AppDbContext;
                if (db == null)
                {
                    await context.Response.WriteAsync("Lỗi kết nối cơ sở dữ liệu!");
                    return;
                }

                var employee = await db.Employees
                    .Include(e => e.Employeeroles)
                        .ThenInclude(er => er.Role)
                    .FirstOrDefaultAsync(e => e.Email == email && e.Password == password);

                if (employee == null)
                {
                    context.Response.Redirect("/Home/Login?error=SaiEmailMatKhau");
                    return;
                }

                // Lưu session
                context.Session.SetInt32("EmployeeId", employee.EmployeeId);
                var roleName = employee.Employeeroles.Select(er => er.Role.Name).FirstOrDefault() ?? "";
                context.Session.SetString("RoleName", roleName);

                // Redirect theo role
                if (roleName == "Admin")
                    context.Response.Redirect("/Home/AdminIndex");
                else
                    context.Response.Redirect("/Home/UserIndex");

                return;
            }

            await _next(context);
        }
    }
}
