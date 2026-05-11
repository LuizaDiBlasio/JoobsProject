using JobPortal_API.Models;

namespace JobPortal_API.Utilities.Interfaces
{
    public interface IMailHelper
    {
        APIResponse SendEmail(string to, string subject, string body);
    }
}
