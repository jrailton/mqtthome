using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MqttHomeWeb.Models;
using Microsoft.AspNetCore.Authentication;

namespace MqttHomeWeb.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = WebUtility.UrlEncode(returnUrl);
            return View();
        }
        
        public IActionResult OAuth(string returnUrl)
        {
            var redirect = $"{Program.Config["BaseUrl"]}/account/oauthresponse";

            return Redirect($"https://accounts.google.com/o/oauth2/auth?client_id={Program.Config["GoogleAppId"]}&redirect_uri={redirect}&response_type=code&scope=email profile&state={WebUtility.UrlEncode(returnUrl)}");
        }

        public async Task<IActionResult> Logout() {
            await HttpContext.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["warning"] = "You have been logged out";

            return Redirect("/");
        }

        public async Task<IActionResult> OAuthResponse(string code, string state)
        {
            string returnUrl = WebUtility.UrlDecode(state);

            try
            {
                if (string.IsNullOrEmpty(code))
                    throw new Exception();

                var provider = new GoogleOAuthProvider(code);

                var user = provider.GetUserData();

                // this is where the email is checked whether its allowed access or not
                var claims = new[] { new Claim("name", user.email), new Claim(ClaimTypes.Role, DetermineRole(user.email)) };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    //AllowRefresh = <bool>,
                    // Refreshing the authentication session should be allowed.

                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                    // The time at which the authentication ticket expires. A 
                    // value set here overrides the ExpireTimeSpan option of 
                    // CookieAuthenticationOptions set with AddCookie.

                    IsPersistent = true,
                    // Whether the authentication session is persisted across 
                    // multiple requests. When used with cookies, controls
                    // whether the cookie's lifetime is absolute (matching the
                    // lifetime of the authentication ticket) or session-based.

                    IssuedUtc = DateTime.UtcNow,
                    // The time at which the authentication ticket was issued.

                    //RedirectUri = <string>
                    // The full path or absolute URI to be used as an http 
                    // redirect response value.
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    authProperties);

                TempData["success"] = $"Hi {user.given_name}, you\'re signed in and will remain so unless you click 'Sign Out' from the nav bar";

                return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
            }
            catch(Exception err)
            {
                Program.GeneralLog.Error($"Account.OAuthResponse :: ");
                TempData["danger"] = $"Permission to authenticate from Google was denied. {err.Message}";
                return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
            }

        }

        public string DetermineRole(string email)
        {

            if (Program.Config["AdminUsers"].Split(',').Contains(email))
                return "Admin";

            if (Program.Config["OperatorUsers"].Split(',').Contains(email))
                return "Operator";

            if (Program.Config["ViewerUsers"].Split(',').Contains(email))
                return "Viewer";

            throw new Exception("User permission denied");
        }
    }
}