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

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _uow.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

            if (_uow.HasChanges()) await _uow.Complete();

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);
        }

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

            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await _uow.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
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

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller} - {other}" : $"{other} - {caller}";
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _uow.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _uow.Complete())
            {
                return group;
            }

            throw new HubException("Failed to add to the group.");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _uow.MessageRepository.RemoveConnection(connection);

            if (await _uow.Complete())
            {
                return group;
            };
            throw new HubException("Failed to remove from the group");
        }

    }
}