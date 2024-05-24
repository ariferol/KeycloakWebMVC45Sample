using System.ComponentModel.DataAnnotations;

namespace KeycloakWebMVC45Sample.Controllers
{
    public class LoginViewModel
    {
        [Display(Name = "Kullanıcı Adı", Prompt = "Kullanıcı adını giriniz.")]
        [Required(ErrorMessage = "Kullanıcı Adı zorunludur.")]
        public string Username { get; set; }

        [Display(Name = "Şifre", Prompt = "Şifrenizi giriniz.")]
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}