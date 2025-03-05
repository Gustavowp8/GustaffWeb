using System.ComponentModel.DataAnnotations;

namespace GustaffWeb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Nome de usuario e obrigatorio")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Entre com a senha")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Lembrar-me")]
        public bool RememberMe { get; set; }
    } 
}
