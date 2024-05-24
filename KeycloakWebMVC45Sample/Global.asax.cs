using KeycloakWebMVC45Sample.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace KeycloakWebMVC45Sample
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //Online Kullanıcılar cache de kullanılmak üzere tutuluyor;
        protected void Application_EndRequest()
        {
            try
            {
                var loggedInUsers = (Dictionary<CacheModel, DateTime>)HttpRuntime.Cache["LoggedInUsers"];

                if (User?.Identity != null && User.Identity.IsAuthenticated)
                {
                    var userData = (CustomPrincipal)User;
                    var cachemodel = new CacheModel
                    {
                        KullaniciAdi = userData.KullaniciAdi,
                        AdSoyad = userData.AdSoyad
                    };
                    if (loggedInUsers != null)
                    {
                        loggedInUsers[cachemodel] = DateTime.Now;
                        HttpRuntime.Cache["LoggedInUsers"] = loggedInUsers;
                    }
                }

                if (loggedInUsers != null)
                {
                    loggedInUsers = loggedInUsers.Where(c => c.Value >= DateTime.Now.AddMinutes(-10)).ToDictionary(c => c.Key, c => c.Value);
                    HttpRuntime.Cache["LoggedInUsers"] = loggedInUsers;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Programın işletimi sırasında bir hata oluştu. HATA: {0} *", ex);
            }
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                var serializer = new JavaScriptSerializer();

                var serializeModel = serializer.Deserialize<CustomPrincipalSerializeModel>(authTicket.UserData);

                CustomPrincipal newUser = new CustomPrincipal(authTicket.Name)
                {
                    KullaniciAdi = serializeModel.KullaniciAdi,
                    AdSoyad = serializeModel.AdSoyad,
                    IsSSO = serializeModel.IsSSO
                };
                HttpContext.Current.User = newUser;
            }
        }


        //protected void Session_Start(Object sender, EventArgs e)
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return;
        //    }

        //    FormsAuthentication.SignOut();

        //    FormsAuthentication.RedirectToLoginPage("Session=Expired");
        //    HttpContext.Current.Response.End();
        //}
    }
}
