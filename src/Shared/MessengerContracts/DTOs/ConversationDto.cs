using System;
using System.Collections.Generic;

namespace MessengerContracts.DTOs;

// ========================================
// Conversation DTOs
// ========================================

/// <summary>
/// DTO for creating a new group conversation
/// </summary>
public record CreateGroupRequest(
    string Name,
    string? Description,
    List<Guid> MemberIds
);

/// <summary>
/// DTO for conversation details
/// </summary>
public record ConversationDto(
    Guid Id,
    ConversationType Type,
    string? Name,
    string? Description,
    string? AvatarUrl,
    DateTime CreatedAt,
    Guid CreatedBy,
    List<ConversationMemberDto> Members,
    MessageDto? LastMessage,
    int UnreadCount
);

/// <summary>
/// DTO for conversation member
/// </summary>
public record ConversationMemberDto(
    Guid Id,
    Guid UserId,
    string Username,
    MemberRole Role,
    DateTime JoinedAt,
    DateTime? LeftAt,
    string? Nickname,
    bool NotificationsEnabled
);

/// <summary>
/// DTO for adding members to a group
/// </summary>
public record AddMembersRequest(
    List<Guid> UserIds,
    MemberRole Role = MemberRole.Member
);

/// <summary>
/// DTO for updating group settings
/// </summary>
public record UpdateGroupRequest(
    string? Name,
    string? Description,
    string? AvatarUrl
);

/// <summary>
/// DTO for updating member role
/// </summary>
public record UpdateMemberRoleRequest(
    Guid UserId,
    MemberRole NewRole
);

// ========================================
// Enums
// ========================================

/// <summary>
/// Type of conversation
/// </summary>
public enum ConversationType
{
    DirectMessage = 0,
    Group = 1
}

/// <summary>
/// Role of a member in a conversation
/// </summary>
public enum MemberRole
{
    Member = 0,
    Admin = 1,
    Owner = 2
}
