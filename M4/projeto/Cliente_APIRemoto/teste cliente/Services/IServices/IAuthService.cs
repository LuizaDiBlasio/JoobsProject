using teste_cliente.DTOs;
using teste_cliente.Models.Dto;


namespace teste_cliente.Services.IServices
{
    public interface IAuthService
    {
        Task<T> LoginAsync<T>(LoginRequestDTO objToCreate);
        Task<T> RegisterAsync<T>(RegisterationRequestDTO objToCreate);
        Task<T> GoogleLoginAsync<T>(string googleToken);
    }
}
