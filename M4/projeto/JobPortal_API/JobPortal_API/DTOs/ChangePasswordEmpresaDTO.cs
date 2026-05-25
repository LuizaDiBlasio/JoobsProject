namespace JobPortal_API.DTOs
{
    public class ChangePasswordEmpresaDTO
    {
        /// <summary>
        /// ID da empresa usado para validação cruzada com o Token JWT.
        /// </summary>
        public int IdEmpresa { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
