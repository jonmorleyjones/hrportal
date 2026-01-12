using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Portal.Api.Data;
using Portal.Api.DTOs;
using Portal.Api.Models;
using Portal.Api.Services;

namespace Portal.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", async (int page, int pageSize, AppDbContext dbContext) =>
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;

            var query = dbContext.Users.AsNoTracking();
            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto(u.Id, u.Email, u.Name, u.Role, u.LastLoginAt, u.IsActive))
                .ToListAsync();

            return Results.Ok(new UserListResponse(users, totalCount, page, pageSize));
        })
        .RequireAuthorization()
        .WithName("ListUsers")
        .WithDescription("List all users in the tenant")
        .Produces<UserListResponse>(200);

        group.MapGet("/{id:guid}", async (Guid id, AppDbContext dbContext) =>
        {
            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return Results.NotFound(new { error = "User not found" });
            }

            return Results.Ok(new UserDto(user.Id, user.Email, user.Name, user.Role, user.LastLoginAt, user.IsActive));
        })
        .RequireAuthorization()
        .WithName("GetUser")
        .WithDescription("Get user details by ID")
        .Produces<UserDto>(200)
        .Produces(404);

        group.MapPost("/invite", async (InviteUserRequest request, ITenantContext tenantContext, AppDbContext dbContext, HttpContext httpContext) =>
        {
            // Check if user already exists
            var existingUser = await dbContext.Users
                .AnyAsync(u => u.Email == request.Email);

            if (existingUser)
            {
                return Results.BadRequest(new { error = "User with this email already exists" });
            }

            // Check if pending invitation exists
            var existingInvitation = await dbContext.Invitations
                .AnyAsync(i => i.Email == request.Email && i.AcceptedAt == null && i.ExpiresAt > DateTime.UtcNow);

            if (existingInvitation)
            {
                return Results.BadRequest(new { error = "Pending invitation already exists for this email" });
            }

            // Get current user ID from claims
            var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var invitedById))
            {
                return Results.Unauthorized();
            }

            // Create invitation
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var invitation = new Invitation
            {
                TenantId = tenantContext.TenantId,
                Email = request.Email,
                Role = request.Role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                InvitedBy = invitedById
            };

            dbContext.Invitations.Add(invitation);
            await dbContext.SaveChangesAsync();

            // TODO: Send invitation email

            return Results.Created($"/api/users/invitations/{invitation.Id}", new { message = "Invitation sent successfully", invitationId = invitation.Id });
        })
        .RequireAuthorization("AdminOnly")
        .WithName("InviteUser")
        .WithDescription("Send invitation to new user (admin only)")
        .Produces(201)
        .Produces(400);

        group.MapPut("/{id:guid}/role", async (Guid id, UpdateUserRoleRequest request, AppDbContext dbContext, HttpContext httpContext) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return Results.NotFound(new { error = "User not found" });
            }

            // Prevent self-demotion
            var currentUserId = httpContext.User.FindFirst("sub")?.Value;
            if (currentUserId == id.ToString() && request.Role != UserRole.Admin)
            {
                return Results.BadRequest(new { error = "Cannot demote yourself" });
            }

            user.Role = request.Role;
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Role updated successfully" });
        })
        .RequireAuthorization("AdminOnly")
        .WithName("UpdateUserRole")
        .WithDescription("Update user role (admin only)")
        .Produces(200)
        .Produces(400)
        .Produces(404);

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext dbContext, HttpContext httpContext) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return Results.NotFound(new { error = "User not found" });
            }

            // Prevent self-deletion
            var currentUserId = httpContext.User.FindFirst("sub")?.Value;
            if (currentUserId == id.ToString())
            {
                return Results.BadRequest(new { error = "Cannot deactivate yourself" });
            }

            user.IsActive = false;
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "User deactivated successfully" });
        })
        .RequireAuthorization("AdminOnly")
        .WithName("DeactivateUser")
        .WithDescription("Deactivate user (admin only)")
        .Produces(200)
        .Produces(400)
        .Produces(404);

        // Invitation endpoints
        app.MapPost("/api/invitations/accept", async (AcceptInvitationRequest request, AppDbContext dbContext, IAuthService authService) =>
        {
            var invitation = await dbContext.Invitations
                .FirstOrDefaultAsync(i => i.Token == request.Token && i.AcceptedAt == null && i.ExpiresAt > DateTime.UtcNow);

            if (invitation == null)
            {
                return Results.BadRequest(new { error = "Invalid or expired invitation" });
            }

            // Create user
            var user = new User
            {
                TenantId = invitation.TenantId,
                Email = invitation.Email,
                Name = request.Name,
                PasswordHash = authService.HashPassword(request.Password),
                Role = invitation.Role,
                InvitedBy = invitation.InvitedBy,
                InvitedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(user);
            invitation.AcceptedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Account created successfully", userId = user.Id });
        })
        .WithTags("Users")
        .WithName("AcceptInvitation")
        .WithDescription("Accept invitation and create account")
        .Produces(200)
        .Produces(400);
    }
}
