using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace JobPortal_API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ChatController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent?key={apiKey}";

            // ==========================================
            // O CÉREBRO DO TEU NEGÓCIO (SYSTEM INSTRUCTIONS)
            // ==========================================
            var regrasDoNegocio = @"
                És o assistente virtual oficial do portal de emprego 'Jobs'. 
                O teu tom deve ser profissional, encorajador, amigável e focado em ajudar candidatos a encontrar emprego e empresas a recrutar talento.
                
                Regras de Comportamento:
                1. Responde de forma clara, concisa e usa formatação simples se necessário.
                2. Nunca fales sobre temas fora do âmbito de emprego, recrutamento, tecnologia ou do nosso portal. Se te perguntarem algo fora deste âmbito, diz educadamente que só podes ajudar com assuntos relacionados com o portal Jobs.
                3. Se não souberes a resposta, sugere que o utilizador explore os menus do site ou contacte o suporte.

                Conhecimento sobre o Portal e FAQs:
                
                - Para Candidatos:
                  * Como pesquisar emprego: Podem usar a barra de pesquisa na página inicial ou aceder ao menu 'Emprego' no topo da página.
                  * Como se candidatar: É preciso fazer Login ou Registo, ir aos detalhes da oferta pretendida e clicar em candidatar.
                  * Área do Candidato: No menu 'Candidato' (visível após login), podem aceder ao 'Perfil', ver o 'Histórico' de candidaturas e gerir o seu 'CV'.
                  * Favoritos: Podem guardar ofertas. O ícone de coração no topo do site abre a lista de vagas favoritas.
                  * Notificações: O ícone do sino no topo do site mostra alertas importantes (ex: mensagens ou atualizações).
                
                - Para Empresas:
                  * Como publicar vagas: Fazer login com uma conta de Empresa, ir ao menu 'Empresa' e clicar em 'Criar oferta'.
                  * Gerir Vagas: No menu 'Empresa', clicar em 'Gerir ofertas' para ver e gerir as ofertas publicadas.
                  * Perfil: As empresas podem editar a sua informação no menu 'Empresa' -> 'Perfil'.
            ";

            var systemInstruction = new
            {
                role = "system",
                parts = new[] { new { text = regrasDoNegocio } }
            };

            var payload = new
            {
                system_instruction = systemInstruction,
                contents = request.History
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(payload, jsonOptions);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var googleError = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { error = $"Erro da API Google: {googleError}" });
            }

            var responseString = await response.Content.ReadAsStringAsync();
            return Ok(responseString);
        }

        // Modelos para o Chatbot
        public class ChatRequest
        {
            public List<MessageContent> History { get; set; } = new();
        }

        public class MessageContent
        {
            public string Role { get; set; } // "user" ou "model"
            public List<MessagePart> Parts { get; set; } = new();
        }

        public class MessagePart
        {
            public string Text { get; set; }
        }
    }
}