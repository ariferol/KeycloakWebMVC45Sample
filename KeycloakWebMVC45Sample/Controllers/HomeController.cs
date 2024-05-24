using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace KeycloakWebMVC45Sample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Callback()
        {
            var accessToken = HttpContext.Request.Headers["Authorization"];
            var refreshToken = HttpContext.Request.Headers["RefreshToken"];
            JwtSecurityToken decodedToken = (JwtSecurityToken)new JwtSecurityTokenHandler().ReadToken(accessToken);
            var username = decodedToken.Claims.FirstOrDefault(x => x.Type == "preferred_username").Value;
            var fullname = decodedToken.Claims.FirstOrDefault(x => x.Type == "name").Value;
            var gidentity = new GenericIdentity(username, "keycloak_sso_auth");
            gidentity.AddClaims(new[]
            {
                new Claim("UserName",username),
                new Claim("FullName",fullname),
                new Claim("AccessToken",accessToken.ToString()),
                new Claim("RefreshToken",refreshToken.ToString()),
            });

            var principal = new CustomPrincipal(gidentity);
            principal.KullaniciAdi = username;
            principal.AdSoyad = fullname;

            System.Threading.Thread.CurrentPrincipal = principal;
            if (System.Web.HttpContext.Current != null)
                System.Web.HttpContext.Current.User = principal;

            Request.GetOwinContext().Authentication.SignIn(gidentity);
            return RedirectToAction("KeycloakGiris", new { model = new LoginViewModel(), retunUrl = "//" });
        }

        [Authorize]
        public ActionResult KeycloakGiris(LoginViewModel model, string returnUrl)
        {
            if (model is null)
            {
                model = new LoginViewModel();
            }

            if (User.Identity.IsAuthenticated)
            {
                var userClaim = HttpContext.GetOwinContext().Authentication.User.Claims;
                //model.Username = userClaim?.FirstOrDefault(x => x.Type.Equals("UserName"))?.Value;
                model.Username = User.Identity.Name;
                model.Password = "1";

                var ortam = ConfigurationManager.AppSettings["Ortam"];
                String adSoyad = userClaim?.FirstOrDefault(x => x.Type.Equals("FullName"))?.Value;
                var serializeModel = new CustomPrincipalSerializeModel
                {
                    KullaniciAdi = model.Username,
                    AdSoyad = String.IsNullOrEmpty(adSoyad) ? ((CustomPrincipal)User).AdSoyad : adSoyad,
                    IsSSO = true
                };

                var serializer = new JavaScriptSerializer();
                var userData = serializer.Serialize(serializeModel);
                var authTicket = new FormsAuthenticationTicket(
                         1,
                         model.Username,
                         DateTime.Now,
                         DateTime.Now.AddMinutes(60),
                         false,
                         userData);

                var encTicket = FormsAuthentication.Encrypt(authTicket);
                var faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                Response.Cookies.Add(faCookie);

                //Online Kullanıcılar in runtime cache de tutuluyor;
                var cachemodel = new CacheModel
                {
                    KullaniciAdi = serializeModel.KullaniciAdi,
                    AdSoyad = serializeModel.AdSoyad
                };
                if (HttpRuntime.Cache["LoggedInUsers"] != null)
                {
                    var loggedInUsers = (Dictionary<CacheModel, DateTime>)HttpRuntime.Cache["LoggedInUsers"];
                    if (!loggedInUsers.ContainsKey(cachemodel))
                    {
                        loggedInUsers.Add(cachemodel, DateTime.Now);
                        HttpRuntime.Cache["LoggedInUsers"] = loggedInUsers;
                    }
                }
                else
                {
                    var loggedInUsers = new Dictionary<CacheModel, DateTime>();
                    loggedInUsers.Add(cachemodel, DateTime.Now);
                    HttpRuntime.Cache["LoggedInUsers"] = loggedInUsers;
                }

                if (returnUrl != "/" && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return View("Secure", model);
            }

            ModelState.AddModelError("", "Kullanıcı için yetki bilgileri okunamadı!");

            return View("Index", model);
        }

        public ActionResult Logout()
        {
            //Cache den user i siliyoruz;
            var userData = (CustomPrincipal)User;
            var cachemodel = new CacheModel
            {
                KullaniciAdi = userData.KullaniciAdi,
                AdSoyad = userData.AdSoyad
            };
            var loggedInUsers = (Dictionary<CacheModel, DateTime>)HttpRuntime.Cache["LoggedInUsers"];
            if (loggedInUsers != null)
            {
                loggedInUsers.Remove(cachemodel);
                HttpRuntime.Cache["LoggedInUsers"] = loggedInUsers;
            }

            FormsAuthentication.SignOut();

            var formsCookie = new HttpCookie(FormsAuthentication.FormsCookieName, string.Empty)
            {
                Expires = DateTime.Now.AddYears(-1)
            };
            Response.Cookies.Add(formsCookie);

            var aspnetSessionIdCookie = new HttpCookie("ASP.NET_SessionId", "")
            {
                Expires = DateTime.Now.AddYears(-1)
            };
            Response.Cookies.Add(aspnetSessionIdCookie);

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
            Response.Cache.SetNoStore();
            Response.AppendHeader("Pragma", "no-cache");

            if (HttpContext != null && Request != null && Session != null)
            {
                if (Request.Cookies["keycloak_cookie"] != null)
                {
                    var c = new HttpCookie("keycloak_cookie");
                    c.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(c);
                }
                if (HttpContext.Request.Cookies[".AspNet.ApplicationCookie"] != null)
                {
                    var c = new HttpCookie(".AspNet.ApplicationCookie");
                    c.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(c);
                }

                if (HttpContext.Request.Cookies["__RequestVerificationToken"] != null)
                {
                    var c = new HttpCookie("__RequestVerificationToken");
                    c.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(c);
                }                
            }

            Session.Clear();
            Session.Abandon();

            string Ortam = ConfigurationManager.AppSettings["Ortam"];
            string Authority = ConfigurationManager.AppSettings["Authority"];
            string LogoutRedirectUri = ConfigurationManager.AppSettings["LogoutRedirectUri"];
            string ClientId = ConfigurationManager.AppSettings["ClientId"];

            //"http://localhost:8080/realms/testrealm/protocol/openid-connect/logout?post_logout_redirect_uri=http://localhost:55367/Home/Index&client_id=KeycloakWebMVC45Sample");
            string returnUrl = Authority + "protocol/openid-connect/logout?post_logout_redirect_uri=" + LogoutRedirectUri + "&client_id=" + ClientId;
            return Redirect(returnUrl);
        }
    }
}