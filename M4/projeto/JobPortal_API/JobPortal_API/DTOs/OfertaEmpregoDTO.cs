using System.Text.Json.Serialization;

namespace JobPortal_API.DTOs
{
    public class OfertaEmpregoDTO
    {
        /// <summary>
        /// Identificador único da Oferta de Emprego.
        /// Nota de Fluxo: Gerado automaticamente pela base de dados. Deve ser omitido no POST, mas é obrigatório na rota do PUT.
        /// </summary>
        public int IdOferta { get; set; }
        /// <summary>
        /// Identificador da Empresa que publicou a vaga.
        /// Nota de Fluxo: Em operações de escrita (POST/PUT), o sistema ignora o valor enviado pelo cliente e injeta de forma segura o ID extraído das Claims do Token JWT do utilizador autenticado.
        /// </summary>
        public int IdEmpresa { get; set; }
        public string Titulo { get; set; }
        public float? Salario { get; set; }
        public string? Jornada { get; set; }
        public string? Localização { get; set; }
        public string? RegimeTrabalho { get; set; }
        public string? TipoContrato { get; set; }
        public string? Requisitos { get; set; }
        public bool? VagaDisponivel { get; set; }
        public string? Descricao { get; set; }
        public int Contagem { get; set; } = 0;
    }
}
