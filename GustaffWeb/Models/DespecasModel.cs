using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GustaffWeb.Models
{
    public class DespesasModel
    {
        [Key]
        public int DespesasId { get; set; }

        [Required(ErrorMessage = "Titulo é obrigatorio")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "Preço é obrigatorio")]
        [Range(0, double.MaxValue)]
        public double Preco { get; set; }

        public bool Fixo { get; set; } = false;

        public bool Pago { get; set; } = false;

        [Range(1, int.MaxValue)]
        public int NumeroParcelas { get; set; }

        // Define o valor padrão para a data atual
        public DateTime Data { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime Vencimento { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; } = "df";

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }
    } 
}