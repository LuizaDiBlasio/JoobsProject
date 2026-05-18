namespace teste_cliente.Models.Dto
{
    public class ResetPasswordDTO
    {
        public string Token { get; set; }

        public string UserId { get; set; }

        public string Password { get; set; }
    }
}
