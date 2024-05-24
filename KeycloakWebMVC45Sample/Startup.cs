using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Configuration;

[assembly: OwinStartup(typeof(KeycloakWebMVC45Sample.Startup))]

namespace KeycloakWebMVC45Sample
{
    public class Startup
    {
        const string persistentAuthType = "keycloak_sso_auth";
        string Ortam = ConfigurationManager.AppSettings["Ortam"];
        string Authority = ConfigurationManager.AppSettings["Authority"];
        string RedirectUri = ConfigurationManager.AppSettings["RedirectUri"];
        string ClientId = ConfigurationManager.AppSettings["ClientId"];
        string ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
        string CookieDomain = ConfigurationManager.AppSettings["CookieDomain"];


        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = persistentAuthType,
                CookieHttpOnly = true,//Eger ssl kullanilacaksa burasi false olmali
                CookieName = "keycloak_cookie",
                CookieSameSite = SameSiteMode.None,
                CookieSecure = CookieSecureOption.Always,
                /*CookieDomain degeri yapilan testlerde duzgun calismadigi tespit edildigi icin kapatildi*/
                CookieDomain = CookieDomain,//"localhost",
                CookiePath = "/",
                LoginPath = new PathString("/Home/Index") // Specify your login page URL
            });

            app.SetDefaultSignInAsAuthenticationType(persistentAuthType);

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions()
            {
                RequireHttpsMetadata = false,//Eger ssl kullanilacaksa burasi true olmali
                Authority = Authority,//"http://localhost:8080/realms/testrealm/",
                ClientId = ClientId,// "KeycloakWebMVC45Sample",
                ClientSecret = ClientSecret,//"7gG2apWXHPXqOiOUjA4wEI29BAkGad5x",
                ResponseType = "code",
                SaveTokens = true,
                //Scope = "openid profile email",
                Scope = "openid",
                UseTokenLifetime = false,
                //SignInAsAuthenticationType = persistentAuthType,
                //AuthenticationType = persistentAuthType,
                //SignInAsAuthenticationType = persistentAuthType,
                //PostLogoutRedirectUri = "http://localhost:55367/Home/Index",
                RedirectUri = RedirectUri,//"http://localhost:55367/Home/Callback",
                RedeemCode = true,
                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    RedirectToIdentityProvider = async (context) =>
                    {
                        //context.ProtocolMessage.Parameters["code_challenge"] = "0aspHXI8ZJCXmq0XejAvl8LE9qu5FTFDeQ0aGyY1iDs";
                        //context.ProtocolMessage.Parameters["code_challenge_method"] = "plain";
                        context.ProtocolMessage.Parameters["code_challenge"] = "pYc179kXUFsOTt20SVG2DCKeR63yRzHXBvJzCKhaIHM";
                        context.ProtocolMessage.Parameters["code_challenge_method"] = "S256";//Keycloak client in Advanced tab indaki 'Proof Key for Code Exchange Code' degeri ile ayni olmasi zorunludur!
                    },
                    AuthorizationCodeReceived = async (context) =>
                    {
                        context.TokenEndpointRequest.Parameters["code_verifier"] = "3zqBE3GLdwnJN5EbT5VwcADFctCEPY3E4mPDRViovwI";
                        //context.TokenEndpointRequest.Parameters["code_verifier"] = "0aspHXI8ZJCXmq0XejAvl8LE9qu5FTFDeQ0aGyY1iDs";
                    },
                    TokenResponseReceived = async (responseToken) =>
                    {
                        responseToken.Request.Headers.Add("Authorization", new[] { responseToken.TokenEndpointResponse.AccessToken });
                        responseToken.Request.Headers.Add("RefreshToken", new[] { responseToken.TokenEndpointResponse.RefreshToken });

                        responseToken.SkipToNextMiddleware();
                    }
                    //,
                    //AuthenticationFailed = context =>
                    //{
                    //    // Handle authentication failure
                    //    return Task.FromResult(0);
                    //},
                    //SecurityTokenValidated = context =>
                    //{
                    //    // Handle token validation
                    //    return Task.FromResult(0);
                    //}
                }
            }); ;
        }
    }
}