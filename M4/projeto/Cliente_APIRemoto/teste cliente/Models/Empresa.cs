using System.ComponentModel.DataAnnotations;

namespace teste_cliente.Models
{
    public class Empresa
    {
        [Key]
        public int IdEmpresa { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        public string Email { get; set; }

        [Required(ErrorMessage = "A localidade é obrigatória.")]
        public string? Localidade { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "O telefone deve conter 9 números.")]
        public int? Telefone { get; set; }

        [Required(ErrorMessage = "O número de funcionários é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Valor inválido.")]
        public int? NoFuncionarios { get; set; }

        [Required(ErrorMessage = "A zona de atuação é obrigatória.")]
        public string? ZonaAtuacao { get; set; }

        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }

        public virtual ICollection<LogoEmpresa>? LogoEmpresa { get; set; }
        public virtual ICollection<OfertaEmprego>? OfertaEmprego { get; set; }
    }
}
