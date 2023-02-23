using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        public readonly IMapper _mapper;
        public MessagesController(IUserRepository userRepository, IMessageRepository MessageRepository, IMapper mapper)
        {
            _mapper = mapper;
            _messageRepository = MessageRepository;
            _userRepository = userRepository;

        }

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            //get the sender username
            var username = User.GetUsername();
            if (username == createMessageDTO.RecipientUsername.ToLower())
                return BadRequest("You cannot send messages to yourself.");

            //define sender and recipient
            var sender = await _userRepository.GetUserByUsernameAsync(username);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);

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
            _messageRepository.AddMessage(message);
            if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDTO>(message));
            return BadRequest("Failed to send message.");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            //set query param username (get messages for the logged in user)
            messageParams.Username = User.GetUsername();

            //get the messages
            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            //add pagination header
            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

            //return messages
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string username)
        {
            var currentUserName = User.GetUsername();
            return Ok(await _messageRepository.GetMessageThread(currentUserName, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();

            var message = await _messageRepository.GetMessage(id);

            if (message.SenderUsername != username && message.RecipientUsername != username)
                return Unauthorized();

            if (message.SenderUsername == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                _messageRepository.DeleteMessage(message);

            if (await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Error occurred on deleting the message");


        }

    }
}