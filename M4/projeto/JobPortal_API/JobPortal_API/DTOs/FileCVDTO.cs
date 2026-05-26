namespace JobPortal_API.DTOs
{
    public class FileCVDTO
    {
        public int IdFile { get; set; }
        public byte[] File { get; set; }
        /// <summary>
        /// Identificador do Candidato dono do ficheiro de currículo.
        /// Nota de Fluxo: Parâmetro obrigatório na rota (Path) para GET, PUT e DELETE. No POST via token, o ID do candidato logado é extraído das Claims por segurança.
        /// </summary>
        public int IdCandidatoFile { get; set; }
    }
}
