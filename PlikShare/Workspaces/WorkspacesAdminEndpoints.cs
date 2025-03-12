using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PlikShare.Core.Authorization;
using PlikShare.Core.Utils;
using PlikShare.Users.Cache;
using PlikShare.Workspaces.Cache;
using PlikShare.Workspaces.ChangeOwner;
using PlikShare.Workspaces.ChangeOwner.Contracts;
using PlikShare.Workspaces.Validation;

namespace PlikShare.Workspaces;

public static class WorkspacesAdminEndpoints
{
    public static void MapWorkspacesAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/workspaces")
            .WithTags("Workspaces Admin")
            .RequireAuthorization(new AuthorizeAttribute
            {
                Policy = AuthPolicy.Internal,
                Roles = $"{Roles.Admin}"
            });

        group.MapPatch("/{workspaceExternalId}/owner", UpdateWorkspaceOwner)
            .WithName("UpdateWorkspaceOwner")
            .AddEndpointFilter<ValidateWorkspaceFilter>();
    }

    private static async Task<Results<Ok, NotFound<HttpError>>> UpdateWorkspaceOwner(
        [FromBody] ChangeWorkspaceOwnerRequestDto request,
        HttpContext httpContext,
        UserCache userCache,
        WorkspaceCache workspaceCache,
        ChangeWorkspaceOwnerQuery changeWorkspaceOwnerQuery,
        CancellationToken cancellationToken)
    {
        var workspaceMembership = httpContext.GetWorkspaceMembershipDetails();
        var newOwner = await userCache.TryGetUser(
            request.NewOwnerExternalId,
            cancellationToken: cancellationToken);

        if (newOwner is null)
            return HttpErrors.User.NotFound(request.NewOwnerExternalId);

        if (newOwner.ExternalId == workspaceMembership.Workspace.Owner.ExternalId)
            return TypedResults.Ok();

        await changeWorkspaceOwnerQuery.Execute(
            workspace: workspaceMembership.Workspace,
            newOwner: newOwner,
            cancellationToken: cancellationToken);

        await workspaceCache.InvalidateEntry(
            workspaceMembership.Workspace.ExternalId,
            cancellationToken: cancellationToken);

        return TypedResults.Ok();
    }
}