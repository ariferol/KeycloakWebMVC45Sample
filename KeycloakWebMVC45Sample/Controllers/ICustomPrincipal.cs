using System.Security.Principal;

namespace KeycloakWebMVC45Sample.Controllers
{
    public interface ICustomPrincipal : IPrincipal
    {        
        string KullaniciAdi { get; set; }
        string AdSoyad { get; set; }
    }
}