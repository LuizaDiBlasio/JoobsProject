using JobPortal_API.Models;

namespace JobPortal_API.Utilities.Interfaces
{
    //___________NOVO FICHEIRO________(Serviço de edição e envio de email)
    public interface IMailHelper
    {
        APIResponse SendEmail(string to, string subject, string body);
    }
}
