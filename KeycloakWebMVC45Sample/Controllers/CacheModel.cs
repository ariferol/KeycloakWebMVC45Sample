using System;

namespace KeycloakWebMVC45Sample.Controllers
{
    public class CacheModel : IEquatable<CacheModel>
    {
        public string KullaniciAdi { get; set; }
        public string AdSoyad { get; set; }

        public override int GetHashCode()
        {
            return KullaniciAdi.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as CacheModel);
        }
        public bool Equals(CacheModel obj)
        {
            return obj != null && obj.KullaniciAdi == this.KullaniciAdi;
        }
    }
}