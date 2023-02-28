using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        public PresenceTracker _tracker { get; }

        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;

        }

        //on user connection
        public override async Task OnConnectedAsync()
        {
            //check whether the user is just conencted
            var isOnline = await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            //send the info that the new user is connected
            if (isOnline)
            {
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
            }

            //get online users and send info about online users
            var currentUsers = await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        //on user disconnection
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            //check whether the user is just disconnected
            var isOffline = await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

            //send the info that the user disconnected
            if (isOffline)
            {
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}