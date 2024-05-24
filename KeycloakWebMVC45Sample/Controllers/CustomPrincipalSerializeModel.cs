using System.Collections.Generic;

namespace KeycloakWebMVC45Sample.Controllers
{
    public class CustomPrincipalSerializeModel
    {
        public string KullaniciAdi { get; set; }
        public string AdSoyad { get; set; }
        public bool IsSSO { get; set; }
    }
}