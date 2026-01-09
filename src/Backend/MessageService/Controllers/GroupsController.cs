using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerContracts.DTOs;
using MessageService.Data;
using MessageService.Data.Entities;
using DtoConversationType = MessengerContracts.DTOs.ConversationType;
using DtoMemberRole = MessengerContracts.DTOs.MemberRole;

namespace MessageService.Controllers;

/// <summary>
/// API Controller for managing group conversations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly MessageDbContext _context;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(MessageDbContext context, ILogger<GroupsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new group conversation
    /// POST /api/groups
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        var userId = GetCurrentUserId();
        
        // Validation
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Group name is required");
        
        if (request.MemberIds == null || request.MemberIds.Count < 1)
            return BadRequest("At least one other member is required");
        
        if (request.MemberIds.Count > 256)
            return BadRequest("Maximum 256 members per group");

        try
        {
            // 1. Create Conversation
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = EntityConversationType.Group,
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsActive = true
            };

            // 2. Add creator as Owner
            conversation.Members.Add(new ConversationMember
            {
                ConversationId = conversation.Id,
                UserId = userId,
                Role = EntityMemberRole.Owner,
                JoinedAt = DateTime.UtcNow
            });

            // 3. Add other members as regular Members
            foreach (var memberId in request.MemberIds.Distinct())
            {
                if (memberId == userId) continue; // Skip creator (already added)

                conversation.Members.Add(new ConversationMember
                {
                    ConversationId = conversation.Id,
                    UserId = memberId,
                    Role = EntityMemberRole.Member,
                    JoinedAt = DateTime.UtcNow
                });
            }

            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Group created: {GroupId} by {UserId}, Members: {MemberCount}",
                conversation.Id, userId, conversation.Members.Count);

            return CreatedAtAction(
                nameof(GetGroup),
                new { id = conversation.Id },
                new ConversationDto(
                    conversation.Id,
                    DtoConversationType.Group,
                    conversation.Name,
                    conversation.Description,
                    conversation.AvatarUrl,
                    conversation.CreatedAt,
                    conversation.CreatedBy,
                    conversation.Members.Select(m => new ConversationMemberDto(
                        m.Id,
                        m.UserId,
                        "Unknown",
                        (DtoMemberRole)m.Role,
                        m.JoinedAt,
                        m.LeftAt,
                        m.CustomNickname,
                        !m.IsMuted
                    )).ToList(),
                    null, // TODO: Map last message
                    0 // TODO: Calculate unread count
                ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            return StatusCode(500, "Error creating group");
        }
    }

    /// <summary>
    /// Get group details
    /// GET /api/groups/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroup(Guid id)
    {
        var userId = GetCurrentUserId();

        var conversation = await _context.Conversations
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id && c.Type == EntityConversationType.Group);

        if (conversation == null)
            return NotFound("Group not found");

        // Check if user is a member
        var membership = conversation.Members.FirstOrDefault(m => m.UserId == userId && m.LeftAt == null);
        if (membership == null)
            return Forbid();

        // Map to DTO
        var groupDto = new ConversationDto(
            conversation.Id,
            DtoConversationType.Group,
            conversation.Name,
            conversation.Description,
            conversation.AvatarUrl,
            conversation.CreatedAt,
            conversation.CreatedBy,
            conversation.Members.Select(m => new ConversationMemberDto(
                m.Id,
                m.UserId,
                "Unknown", // TODO: Fetch username from UserService
                (DtoMemberRole)m.Role,
                m.JoinedAt,
                m.LeftAt,
                m.CustomNickname,
                m.IsMuted
            )).ToList(),
            null, // TODO: Map last message
            0 // TODO: Calculate unread count
        );

        return Ok(groupDto);
    }

    /// <summary>
    /// Add members to a group
    /// POST /api/groups/{id}/members
    /// </summary>
    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMembers(Guid id, [FromBody] AddMembersRequest request)
    {
        var userId = GetCurrentUserId();

        var conversation = await _context.Conversations
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id && c.Type == EntityConversationType.Group);

        if (conversation == null)
            return NotFound("Group not found");

        // Check if user is Admin or Owner
        var currentMember = conversation.Members.FirstOrDefault(m => m.UserId == userId && m.LeftAt == null);
        if (currentMember == null)
            return Forbid();
        
        if (currentMember.Role == EntityMemberRole.Member)
            return Forbid("Only admins can add members");

        // Check max members
        if (conversation.Members.Count + request.UserIds.Count > 256)
            return BadRequest("Maximum 256 members per group");

        try
        {
            // Add new members
            foreach (var newUserId in request.UserIds.Distinct())
            {
                // Check if already a member
                if (conversation.Members.Any(m => m.UserId == newUserId && m.LeftAt == null))
                    continue;

                conversation.Members.Add(new ConversationMember
                {
                    ConversationId = id,
                    UserId = newUserId,
                    Role = (EntityMemberRole)request.Role,
                    JoinedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Members added to group {GroupId}: {MemberIds}",
                id, string.Join(", ", request.UserIds));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding members to group {GroupId}", id);
            return StatusCode(500, "Error adding members");
        }
    }

    /// <summary>
    /// Remove a member from the group
    /// DELETE /api/groups/{id}/members/{userId}
    /// </summary>
    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        var currentUserId = GetCurrentUserId();

        var conversation = await _context.Conversations
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id && c.Type == EntityConversationType.Group);

        if (conversation == null)
            return NotFound("Group not found");

        var currentMember = conversation.Members.FirstOrDefault(m => m.UserId == currentUserId && m.LeftAt == null);
        if (currentMember == null)
            return Forbid();

        // Only admins/owners can remove others, or user can remove themselves
        if (userId != currentUserId && currentMember.Role == EntityMemberRole.Member)
            return Forbid("Only admins can remove other members");

        var memberToRemove = conversation.Members.FirstOrDefault(m => m.UserId == userId && m.LeftAt == null);
        if (memberToRemove == null)
            return NotFound("Member not found in group");

        // Cannot remove owner
        if (memberToRemove.Role == EntityMemberRole.Owner)
            return BadRequest("Cannot remove group owner");

        try
        {
            // Soft delete: Set LeftAt timestamp
            memberToRemove.LeftAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Member {UserId} removed from group {GroupId} by {CurrentUserId}",
                userId, id, currentUserId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from group {GroupId}", id);
            return StatusCode(500, "Error removing member");
        }
    }

    /// <summary>
    /// Update group settings (name, description, avatar)
    /// PATCH /api/groups/{id}
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateGroupRequest request)
    {
        var userId = GetCurrentUserId();

        var conversation = await _context.Conversations
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id && c.Type == EntityConversationType.Group);

        if (conversation == null)
            return NotFound("Group not found");

        // Only admins/owners can update group settings
        var member = conversation.Members.FirstOrDefault(m => m.UserId == userId && m.LeftAt == null);
        if (member == null || member.Role == EntityMemberRole.Member)
            return Forbid("Only admins can update group settings");

        try
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
                conversation.Name = request.Name;
            
            if (request.Description != null)
                conversation.Description = request.Description;
            
            if (request.AvatarUrl != null)
                conversation.AvatarUrl = request.AvatarUrl;

            conversation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Group {GroupId} updated by {UserId}", id, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group {GroupId}", id);
            return StatusCode(500, "Error updating group");
        }
    }

    /// <summary>
    /// Leave a group
    /// POST /api/groups/{id}/leave
    /// </summary>
    [HttpPost("{id}/leave")]
    public async Task<IActionResult> LeaveGroup(Guid id)
    {
        var userId = GetCurrentUserId();

        var conversation = await _context.Conversations
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id && c.Type == EntityConversationType.Group);

        if (conversation == null)
            return NotFound("Group not found");

        var member = conversation.Members.FirstOrDefault(m => m.UserId == userId && m.LeftAt == null);
        if (member == null)
            return NotFound("You are not a member of this group");

        // Owner cannot leave (must transfer ownership first)
        if (member.Role == EntityMemberRole.Owner)
            return BadRequest("Owner cannot leave group. Transfer ownership first.");

        try
        {
            member.LeftAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} left group {GroupId}", userId, id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving group {GroupId}", id);
            return StatusCode(500, "Error leaving group");
        }
    }

    // ========================================
    // Helper Methods
    // ========================================

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        return Guid.Parse(userIdClaim!);
    }
}
