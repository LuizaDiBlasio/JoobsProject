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
            villaUrl = configuration.GetValue<string>("https://localhost:7211/");

        }

        public Task<T> LoginAsync<T>(LoginRequestDTO obj)
        {
            Console.WriteLine($"Enviando LoginRequest - Username: {obj.UserName}, Password: {obj.Password}");

            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = "https://localhost:7211/api/Auth/login",
                //localhost:7211/api/UsersAuth/login
                //Url = villaUrl + "api/UsersAuth/login",

            });
        }

        public Task<T> RegisterAsync<T>(RegisterationRequestDTO obj)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                //Url = villaUrl + "api/UsersAuth/register",
                Url = "https://localhost:7211/api/Auth/register",

            });
        }
        public Task<T> GoogleLoginAsync<T>(string googleToken)
        {
            return SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = googleToken, // Envia o token como string direta
                Url = "https://localhost:7211/api/Auth/google-login",
            });
        }
    }
}
