using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal_API.DTOs
{
    public class NotificationsDTO
    {
        public int IdNotification { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]

        public string? Notification { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
