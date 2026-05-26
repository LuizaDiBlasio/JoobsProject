namespace JobPortal_API.DTOs
{
    public class LogoEmpresaDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// Identificador da Empresa proprietária do Logótipo.
        /// Nota de Fluxo: Em perfis logados como 'Empresa', este ID é injetado e validado de forma automática através das Claims do Token JWT.
        /// </summary>
        public int IdEmpresaFoto { get; set; }
        public byte[] Logo { get; set; }
    }
}
