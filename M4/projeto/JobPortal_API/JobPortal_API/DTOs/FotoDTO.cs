using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace JobPortal_API.DTOs
{
    public class FotoDTO
    {
        /// <summary>
        /// Identificador único do registo de Foto na base de dados.
        /// Nota de Fluxo: Gerado de forma incremental. Obrigatório como parâmetro de rota no PUT.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador do Candidato associado à foto de perfil.
        /// Nota de Fluxo: Nos endpoints de mutação protegidos (POST), o sistema valida as Claims do utilizador e vincula o ficheiro ao ID do Candidato logado por questões de segurança.
        /// </summary>
        public int IdCandidatoFoto { get; set; }
        public byte[] FotoPerfil { get; set; }
    }
}
