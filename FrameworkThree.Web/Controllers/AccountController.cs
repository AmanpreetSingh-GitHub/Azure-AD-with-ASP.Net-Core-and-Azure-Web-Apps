using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using FrameworkThree.Web.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FrameworkThree.Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public async Task Signin()
        {
            if (HttpContext.User == null || !HttpContext.User.Identity.IsAuthenticated)
            {
                await HttpContext.Authentication.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });
            }
        }

        [HttpGet]
        public async Task SignOut()
        {
            string callbackUrl = Url.Action("SignOutCallback", "Account", values: null, protocol: "https");

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                string azureADUserObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

                AuthenticationContext authenticationContext = new AuthenticationContext(Startup.AuthenticationAuthority, new NaiveSessionCache(azureADUserObjectID, HttpContext.Session));
                authenticationContext.TokenCache.Clear();

                await HttpContext.Authentication.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = callbackUrl });
                await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = callbackUrl });
            }
        }

        public async Task SessionFinish()
        {
            await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public ActionResult SignOutCallback()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
