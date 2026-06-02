using Newtonsoft.Json;
using System.Collections.Generic;

namespace teste_cliente.Models.Dto
{
    public class CandidatoPesquisaDTO
    {
        // Mapeia diretamente a propriedade "candidatos" que a API envia
        [JsonProperty("candidatos")]
        public List<CandidatoObjetoDto> Candidatos { get; set; } = new List<CandidatoObjetoDto>();

        public class CandidatoObjetoDto
        {
            public int IdCandidato { get; set; }
            public string Nome { get; set; }
            public string Email { get; set; }
            public int? Telefone { get; set; }

            // Mapeia diretamente o objeto "cv" que a API envia
            [JsonProperty("cv")]
            public CvObjetoDto CV { get; set; }
        }

        public class CvObjetoDto
        {
            public int Concelho { get; set; }
            public int Escolaridade { get; set; }
        }
    }
}