using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal_API.Models
{
    public class Notifications
    {
        [Key]
        public int NotificationId { get; set; }


        [ForeignKey("UserId")]
        public string UserId { get; set; }
        
        public string? Notification { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }
    }
}