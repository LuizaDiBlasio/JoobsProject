using System.ComponentModel.DataAnnotations;

namespace teste_cliente.DTOs
{
    public class RegisterationRequestDTO
    {
        [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O username de usuário é obrigatório.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Digite um e-mail válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Selecione uma role.")]
        public string Role { get; set; }
    }
}
