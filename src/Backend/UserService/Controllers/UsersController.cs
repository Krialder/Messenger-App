// ========================================
// PSEUDO-CODE - Sprint 7: User Service
// Status: 🔶 API-Struktur definiert
// Dependencies: Sprint 2 (AuthService)
// ========================================

using MessengerContracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Data;
using UserService.Data.Entities;

namespace UserService.Controllers;

[ApiController, Route("api/users"), Authorize]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        var p = await _context.UserProfiles.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        if (p == null) return NotFound();
        return Ok(new UserDto { Id = p.Id, Username = p.Username, Email = p.Email, DisplayName = p.DisplayName, AvatarUrl = p.AvatarUrl, Bio = p.Bio, EmailVerified = p.EmailVerified, CreatedAt = p.CreatedAt });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile() => await GetProfile(GetUserId());

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] MessengerContracts.DTOs.UpdateProfileRequest req)
    {
        var uid = GetUserId();
        var p = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == uid);
        if (p == null) return NotFound();
        if (req.DisplayName != null) p.DisplayName = req.DisplayName.Trim();
        if (req.AvatarUrl != null) p.AvatarUrl = req.AvatarUrl.Trim();
        if (req.Bio != null) { if (req.Bio.Length > 500) return BadRequest(); p.Bio = req.Bio.Trim(); }
        p.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new UserDto { Id = p.Id, Username = p.Username, Email = p.Email, DisplayName = p.DisplayName, AvatarUrl = p.AvatarUrl, Bio = p.Bio, EmailVerified = p.EmailVerified, CreatedAt = p.CreatedAt });
    }

    [HttpGet("{userId}/salt"), AllowAnonymous]
    public async Task<IActionResult> GetSalt(Guid userId)
    {
        var p = await _context.UserProfiles.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        if (p == null) return NotFound();
        return Ok(new { userId = p.Id, salt = Convert.ToBase64String(p.MasterKeySalt) });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2) return BadRequest();
        var uid = GetUserId();
        if (limit > 50) limit = 50;
        var t = q.ToLower().Trim();
        var ps = await _context.UserProfiles.AsNoTracking().Where(u => u.IsActive && u.Id != uid && (u.Username.ToLower().Contains(t) || u.Email.ToLower().Contains(t) || (u.DisplayName != null && u.DisplayName.ToLower().Contains(t)))).Take(limit).ToListAsync();
        var cids = await _context.Contacts.Where(c => c.UserId == uid).Select(c => c.ContactUserId).ToListAsync();
        return Ok(ps.Select(p => new UserSearchResultDto { Id = p.Id, Username = p.Username, DisplayName = p.DisplayName, AvatarUrl = p.AvatarUrl, Bio = p.Bio, IsContact = cids.Contains(p.Id) }));
    }

    [HttpPost("contacts")]
    public async Task<IActionResult> AddContact([FromBody] MessengerContracts.DTOs.AddContactRequest req)
    {
        var uid = GetUserId();
        if (req.ContactUserId == uid) return BadRequest();
        if (await _context.Contacts.AnyAsync(c => c.UserId == uid && c.ContactUserId == req.ContactUserId)) return Conflict();
        var c = new Contact { Id = Guid.NewGuid(), UserId = uid, ContactUserId = req.ContactUserId, Nickname = req.Nickname, AddedAt = DateTime.UtcNow };
        _context.Contacts.Add(c);
        await _context.SaveChangesAsync();
        return Ok(c);
    }

    [HttpGet("contacts")]
    public async Task<IActionResult> GetContacts()
    {
        var uid = GetUserId();
        var cs = await _context.Contacts.Where(c => c.UserId == uid).ToListAsync();
        var cids = cs.Select(c => c.ContactUserId).ToList();
        var ps = await _context.UserProfiles.Where(u => cids.Contains(u.Id)).ToListAsync();
        var res = cs.Select(c => { var p = ps.FirstOrDefault(x => x.Id == c.ContactUserId); return p == null ? null : new ContactDto { Id = c.Id, ContactUserId = c.ContactUserId, Username = p.Username, DisplayName = p.DisplayName, Nickname = c.Nickname, AvatarUrl = p.AvatarUrl, IsBlocked = c.IsBlocked, AddedAt = c.AddedAt }; }).Where(x => x != null).ToList();
        return Ok(res);
    }

    [HttpDelete("contacts/{contactId}")]
    public async Task<IActionResult> RemoveContact(Guid contactId)
    {
        var uid = GetUserId();
        var c = await _context.Contacts.FirstOrDefaultAsync(x => x.Id == contactId && x.UserId == uid);
        if (c == null) return NotFound();
        _context.Contacts.Remove(c);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount([FromBody] MessengerContracts.DTOs.DeleteAccountRequest req)
    {
        var uid = GetUserId();
        var p = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == uid);
        if (p == null) return NotFound();
        p.DeleteScheduledAt = DateTime.UtcNow.AddDays(30);
        await _context.SaveChangesAsync();
        _logger.LogWarning("User {UserId} scheduled deletion for {Date}", uid, p.DeleteScheduledAt);
        return Ok(new { message = "Account deletion scheduled", scheduledDate = p.DeleteScheduledAt });
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}

// ========================================
// REQUEST/RESPONSE DTOs
// ========================================

public record UpdateProfileRequest(
    string? Email,
    string? DisplayName
);

public record DeleteAccountRequest(
    string Password,
    bool ConfirmDelete
);
