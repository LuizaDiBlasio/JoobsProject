using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using teste_cliente.Models;
using teste_cliente.Services.IServices;

namespace teste_cliente.Services
{
    public class BaseService : IBaseService
    {
        public APIResponse responseModel { get; set; }
        public IHttpClientFactory httpClient { get; set; }
        public BaseService(IHttpClientFactory httpClient)
        {
            this.responseModel = new();
            this.httpClient = httpClient;
        }
        public async Task<T> SendAsync<T>(APIRequest apiRequest)
        {
            try
            {
                var client = httpClient.CreateClient("MagicAPI");
                HttpRequestMessage message = new HttpRequestMessage();
                message.Headers.Add("Accept", "application/json");
                message.RequestUri = new Uri(apiRequest.Url);

                if (apiRequest.Data != null)
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data),
                        Encoding.UTF8, "application/json");
                }

                switch (apiRequest.ApiType)
                {
                    case SD.ApiType.POST: message.Method = HttpMethod.Post; break;
                    case SD.ApiType.PUT: message.Method = HttpMethod.Put; break;
                    case SD.ApiType.DELETE: message.Method = HttpMethod.Delete; break;
                    default: message.Method = HttpMethod.Get; break;
                }

                if (!string.IsNullOrEmpty(apiRequest.Token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiRequest.Token);
                }

                HttpResponseMessage apiResponse = await client.SendAsync(message);

                // --- LÓGICA DE TRATAMENTO DE ERRO ROBUSTA ---

                // 1) Se for sucesso (2xx), prosseguimos normalmente
                if (apiResponse.IsSuccessStatusCode)
                {
                    var apiContent = await apiResponse.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(apiContent);
                }

                // 2) Se não for sucesso (500, 403, etc.), capturamos o erro de forma genérica
                var errorContent = await apiResponse.Content.ReadAsStringAsync();
                var errorDto = new APIResponse
                {
                    StatusCode = apiResponse.StatusCode,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { $"Erro na API ({apiResponse.StatusCode}): {errorContent}" }
                };

                // Retornamos um APIResponse com erro serializado para não quebrar o fluxo
                var res = JsonConvert.SerializeObject(errorDto);
                return JsonConvert.DeserializeObject<T>(res);
            }
            catch (Exception e)
            {
                // Erro de rede (API em baixo, timeout, etc.)
                var dto = new APIResponse
                {
                    ErrorMessages = new List<string> { $"Erro de rede ou servidor inacessível: {e.Message}" },
                    IsSuccess = false
                };
                var res = JsonConvert.SerializeObject(dto);
                return JsonConvert.DeserializeObject<T>(res);
            }
        }
    }
}
        