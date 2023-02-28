using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        public DataContext _context { get; }
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        //add the message in the database
        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        //delete the message from the database
        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }


        //get the message by id
        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }


        //get the messages for the specific user
        public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
        {
            //get the messages in the desc order
            var query = _context.Messages
                 .OrderByDescending(x => x.MessageSent)
                 .AsQueryable();

            //get the inbox or outbox messages (all except the deleted ones)
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username && u.SenderDeleted == false),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false && u.DateRead == null)
            };

            //project from a queryable to a mapper object
            var messages = query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);

            //return paged list
            return await PagedList<MessageDTO>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }


        //get messages thread for realtime chatting
        public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            //get all the messages between two users
            var query = _context.Messages
                .Where(
                    m => m.RecipientUsername == currentUserName && m.RecipientDeleted == false && m.SenderUsername == recipientUserName ||
                         m.RecipientUsername == recipientUserName && m.SenderUsername == currentUserName && m.SenderDeleted == false
                )
                .OrderBy(m => m.MessageSent).AsQueryable();

            //get the unread messages for the logged in user
            var unreadMessages = query.Where(m => m.DateRead == null && m.RecipientUsername == currentUserName).ToList();

            //update dateread in all unread messages
            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            //return all messages in the thread
            return await query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider).ToListAsync();

        }

        //add new chat group
        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }


        //get the chat group (with connections) by the groupname
        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
        }


        //get the connection by the connection id
        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }


        //search for the chat group by the specific conenction id
        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
            .Include(x => x.Connections)
            .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
        }


        //delete the connection
        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}