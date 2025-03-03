using System.ComponentModel.DataAnnotations;

namespace GustaffWeb.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Nome de usuário é obrigatório")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirme a senha")]
        [Compare("Password", ErrorMessage = "As senhas não conferem")]
        public string? ConfirmPassword { get; set; }
    }
}
