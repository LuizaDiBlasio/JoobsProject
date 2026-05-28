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

            var systemInstruction = new
            {
                role = "system",
                parts = new[] { new { text = "És um assistente virtual útil e amigável. O teu objetivo é ajudar os utilizadores a navegar no site e responder a perguntas frequentes." } }
            };

            var payload = new
            {
                system_instruction = systemInstruction,
                contents = request.History
            };

            // A SOLUÇÃO ESTÁ AQUI: Obrigar o C# a enviar as propriedades em formato camelCase (ex: 'Role' passa a 'role')
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(payload, jsonOptions);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                // Se a Google der erro, vamos ler o erro exato que a Google devolve para nos ajudar a fazer debug
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