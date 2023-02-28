using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        public IUnitOfWork _uow { get; }
        public MessageHub(IMapper mapper, IHubContext<PresenceHub> presenceHub, IUnitOfWork uow)
        {
            _uow = uow;
            _presenceHub = presenceHub;
            _mapper = mapper;

        }

        //on user connection
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            //get the user from the request
            var otherUser = httpContext.Request.Query["user"];
            //get the group name between two users
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            //add the connection to the chat group
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            //get the group
            var group = await AddToGroup(groupName);

            //send message that the group is updated
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            //get message thread between two users
            var messages = await _uow.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

            //if there is any change, update
            if (_uow.HasChanges()) await _uow.Complete();
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        //on user disconnection, remove his connection
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);
        }

        //send the message
        public async Task SendMessage(CreateMessageDTO createMessageDTO)
        {
            //get the sender username
            var username = Context.User.GetUsername();
            if (username == createMessageDTO.RecipientUsername.ToLower())
                throw new HubException("You cannot send messages to yourself.");

            //define sender and recipient
            var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);

            //if there is no recipient, return not found
            if (recipient == null) throw new HubException("Not found user");

            //create new message object
            var message = new Message
            {
                Sender = sender,
                SenderUsername = sender.UserName,

                Recipient = recipient,
                RecipientUsername = recipient.UserName,

                Content = createMessageDTO.Content
            };

            //get the group name between the users
            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            //get the chat group info
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);

            //if there is registered recipient connection right now, set the date read
            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                //else, send the notification about the new message in the inbox
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            //add message in the database, return response
            _uow.MessageRepository.AddMessage(message);
            if (await _uow.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDTO>(message));
            }

        }


        //get the group name for the chat between two users
        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller} - {other}" : $"{other} - {caller}";
        }

        //add the connection to the chat group
        private async Task<Group> AddToGroup(string groupName)
        {
            //get the chat group by groupname
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);

            //create the connection for  the logged in user
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            //if there is no group, create the new one
            if (group == null)
            {
                group = new Group(groupName);
                _uow.MessageRepository.AddGroup(group);
            }

            //add the connection in the group
            group.Connections.Add(connection);

            //return the chat group info
            if (await _uow.Complete())
            {
                return group;
            }
            throw new HubException("Failed to add to the group.");
        }


        //remove the connection from the chat group
        private async Task<Group> RemoveFromMessageGroup()
        {
            //get the group for the exact connection
            var group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);

            //get the conenction data from the database
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            //delete the connection
            _uow.MessageRepository.RemoveConnection(connection);

            //return the connection group
            if (await _uow.Complete())
            {
                return group;
            };
            throw new HubException("Failed to remove from the group");
        }

    }
}