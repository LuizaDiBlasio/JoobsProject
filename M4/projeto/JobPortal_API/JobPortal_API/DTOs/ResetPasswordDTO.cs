namespace JobPortal_API.DTOs
{
    public class ResetPasswordDTO
    {
        public string Token { get; set; }

        public string UserId { get; set; }

        public string Password { get; set; }
    }
}
