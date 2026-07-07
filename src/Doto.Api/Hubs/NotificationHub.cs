using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Doto.Application.Helpers;
using Doto.Application.Interfaces;
using Doto.Domain.Interfaces;
using System.Security.Claims;

namespace Doto.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        
        // Get PersonId from current user and join group
        var scope = Context.GetHttpContext()?.RequestServices?.CreateScope();
        if (scope != null)
        {
            try
            {
                var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
                var personRepository = scope.ServiceProvider.GetRequiredService<IPersonRepository>();
                var person = await PersonHelper.GetCurrentPersonAsync(currentUser, personRepository);
                
                if (person != null)
                {
                    // Join user to their personal group using PersonId
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{person.Id}");
                }
            }
            catch
            {
                // If we can't get person, connection will still work but without targeted notifications
            }
            finally
            {
                scope.Dispose();
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinMemberGroup(string memberId)
    {
        if (string.IsNullOrEmpty(memberId))
            return;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"member_{memberId}");
    }

    public async Task LeaveMemberGroup(string memberId)
    {
        if (string.IsNullOrEmpty(memberId))
            return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"member_{memberId}");
    }
}

