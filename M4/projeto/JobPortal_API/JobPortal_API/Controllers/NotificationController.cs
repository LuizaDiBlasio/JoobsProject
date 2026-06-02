using JobPortal_API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase

{
    private readonly ApplicationDbContext _context;

    public NotificationController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetRecent()

    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var notifications = await _context.Notifications

        .Where(n => n.UserId == userId)

        .OrderByDescending(n => n.CreatedAt)

        .Take(10)

        .ToListAsync();

        return Ok(notifications);
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> MarkAsRead(int id)

    {

        var notification = await _context.Notifications.FindAsync(id);

        if (notification == null) return NotFound();

        notification.IsRead = true;

        await _context.SaveChangesAsync();

        return Ok();

    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()

    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var unreadNotifications = await _context.Notifications

        .Where(n => n.UserId == userId && !n.IsRead)

        .ToListAsync();

        if (unreadNotifications.Any())

        {
            foreach (var notification in unreadNotifications)

            {
                notification.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var notification = await _context.Notifications.FindAsync(id);

        if (notification == null)

            return NotFound();

        if (notification.UserId != userId)

            return Forbid();

        _context.Notifications.Remove(notification);

        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("clear-all")]
    public async Task<IActionResult> ClearAllNotifications()

    {

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var userNotifications = await _context.Notifications

        .Where(n => n.UserId == userId)

        .ToListAsync();

        if (userNotifications.Any())

        {
            _context.Notifications.RemoveRange(userNotifications);

            await _context.SaveChangesAsync();

        }

        return Ok();
    }
}
