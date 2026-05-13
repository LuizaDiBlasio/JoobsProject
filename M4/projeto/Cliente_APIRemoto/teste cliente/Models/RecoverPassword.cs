using System.ComponentModel.DataAnnotations;

namespace teste_cliente.Models
{
    public class RecoverPassword
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string? Password { get; set; } //nova password


        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name = "Confirm password")]
        public string? ConfirmPassword { get; set; }


        [Required]
        public string Token { get; set; }

        public string UserId { get; set; }
 
    }
}
