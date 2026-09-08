using NHIGIA.Web.Infrastructure;
using NHIGIA.Web.Models;
using System;
using System.Web.Mvc;
using System.Web.Security;

namespace NHIGIA.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = HrmDataStore.Instance.FindUser(model.Username);
            if (user == null || !user.IsActive || !HrmPasswordHasher.Verify(model.Password, user.PasswordSalt, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng.");
                return View(model);
            }

            var ticket = new FormsAuthenticationTicket(1, user.Username, DateTime.Now, DateTime.Now.AddHours(model.RememberMe ? 168 : 8), model.RememberMe, user.RoleCode);
            var cookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = Request.IsSecureConnection,
                Expires = model.RememberMe ? ticket.Expiration : DateTime.MinValue
            };
            Response.Cookies.Add(cookie);
            user.PasswordHash = null;
            user.PasswordSalt = null;
            HrmSession.Current = user;
            HrmDataStore.Instance.MarkLogin(user.Id, Request.UserHostAddress);
            if (Url.IsLocalUrl(model.ReturnUrl)) return Redirect(model.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }

        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}
