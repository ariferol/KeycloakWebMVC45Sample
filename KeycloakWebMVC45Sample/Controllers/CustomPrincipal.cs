using System.Collections.Generic;
using System.Security.Principal;

namespace KeycloakWebMVC45Sample.Controllers
{
    public class CustomPrincipal : ICustomPrincipal
    {
        private string[] roles;

        public IIdentity Identity { get; private set; }
        public bool IsInRole(string role) { return false; }

        public CustomPrincipal(string tc)
        {
            this.Identity = new GenericIdentity(tc);
        }

        public CustomPrincipal(IIdentity identity)
        {
            this.Identity = identity;
        }

        //public CustomPrincipal(IIdentity identity, string[] roles) : this(identity)
        //{
        //    this.Identity = identity;
        //    this.roles = roles;
        //}

        public string KullaniciAdi { get; set; }
        public string AdSoyad { get; set; }
        public bool IsSSO { get; set; }

    }
}