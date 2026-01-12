using Portal.Api.Models;

namespace Portal.Api.DTOs;

public record InviteUserRequest(string Email, UserRole Role);

public record UpdateUserRoleRequest(UserRole Role);

public record AcceptInvitationRequest(string Token, string Name, string Password);

public record UserListResponse(
    List<UserDto> Users,
    int TotalCount,
    int Page,
    int PageSize
);

public record InvitationDto(
    Guid Id,
    string Email,
    UserRole Role,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    string InvitedByName
);
