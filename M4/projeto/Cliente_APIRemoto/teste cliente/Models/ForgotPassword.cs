using System.ComponentModel.DataAnnotations;

namespace teste_cliente.Models
{
    public class ForgotPassword
    {
        [Required]
        public string Email { get; set; }
    }
}
