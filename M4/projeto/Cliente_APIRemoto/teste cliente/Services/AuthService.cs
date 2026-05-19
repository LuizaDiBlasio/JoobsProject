using teste_cliente.DTOs;
using teste_cliente.Models;
using teste_cliente.Models.Dto;
using teste_cliente.Services.IServices;

namespace teste_cliente.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly IHttpClientFactory _clientFactory;
        private string villaUrl;

        public AuthService(IHttpClientFactory clientFactory, IConfiguration configuration) : base(clientFactory)
        {
            _clientFactory = clientFactory;

            // CORRECTED: Looks for a key in appsettings.json, falls back to the hardcoded URL if not found.
            villaUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7211/";
        }

        public Task<T> LoginAsync<T>(LoginRequestDTO obj)
        {
            Console.WriteLine($"Enviando LoginRequest - Username: {obj.UserName}, Password: {obj.Password}");

            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = villaUrl + "api/Auth/login", // Restored to use your variable
            });
        }

        public Task<T> RegisterAsync<T>(RegisterationRequestDTO obj)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = villaUrl + "api/Auth/register", // Restored to use your variable
            });
        }

        public Task<T> GoogleLoginAsync<T>(GoogleLoginDTO dto)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = dto,
                Url = villaUrl + "api/Auth/google-login", // Restored to use your variable
            });
        }
    }
}