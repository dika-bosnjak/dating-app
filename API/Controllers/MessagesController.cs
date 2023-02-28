using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        public readonly IMapper _mapper;
        public IUnitOfWork _uow { get; }
        public MessagesController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;

        }

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            //get the sender username
            var username = User.GetUsername();
            if (username == createMessageDTO.RecipientUsername.ToLower())
                return BadRequest("You cannot send messages to yourself.");

            //define sender and recipient
            var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);

            //if there is no recipient, return not found
            if (recipient == null) return NotFound();

            //create new message object
            var message = new Message
            {
                Sender = sender,
                SenderUsername = sender.UserName,

                Recipient = recipient,
                RecipientUsername = recipient.UserName,

                Content = createMessageDTO.Content
            };

            //add message in the database, return response
            _uow.MessageRepository.AddMessage(message);
            if (await _uow.Complete()) return Ok(_mapper.Map<MessageDTO>(message));
            return BadRequest("Failed to send message.");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            //set query param username (get messages for the logged in user)
            messageParams.Username = User.GetUsername();

            //get the messages
            var messages = await _uow.MessageRepository.GetMessagesForUser(messageParams);

            //add pagination header
            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

            //return messages
            return messages;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            //get the username of logged in user
            var username = User.GetUsername();

            //get the message id that should be deleted
            var message = await _uow.MessageRepository.GetMessage(id);

            //only sender or the receiver can delete the message in their inbox
            if (message.SenderUsername != username && message.RecipientUsername != username)
                return Unauthorized();

            //if the sender deleted message, mark the field
            if (message.SenderUsername == username) message.SenderDeleted = true;
            //if the receiver deleted message, mark the field
            if (message.RecipientUsername == username) message.RecipientDeleted = true;

            //if both users deleted the message, delete it in the database too
            if (message.SenderDeleted && message.RecipientDeleted)
                _uow.MessageRepository.DeleteMessage(message);

            //return response
            if (await _uow.Complete()) return Ok();
            return BadRequest("Error occurred on deleting the message");
        }

    }
}