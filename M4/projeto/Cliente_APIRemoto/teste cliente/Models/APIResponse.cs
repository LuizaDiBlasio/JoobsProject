using System.Net;

namespace teste_cliente.Models
{
    public class APIResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<string>? ErrorMessages { get; set; } = new List<string>();
        public string Message { get; set; }

        public object Result { get; set; }

        public string PassToken { get; set; }
    }
}
