using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        public IMapper _mapper { get; }
        public IUnitOfWork _uow { get; }
        public UsersController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        //get users with query params
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDTO>>> GetUsers([FromQuery] QueryParams queryParams)
        {
            //get the loggedin user
            var gender = await _uow.UserRepository.GetUserGender(User.GetUsername());
            queryParams.CurrentUsername = User.GetUsername();

            //if there is no gender in query params, set the default gender as the opposite one from the loggedin user
            if (string.IsNullOrEmpty(queryParams.Gender))
            {
                queryParams.Gender = gender == "male" ? "female" : "male";
            }

            //get the members from the db
            var users = await _uow.UserRepository.GetMembersAsync(queryParams);

            //aff the pagination header to the response
            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            //return the users
            return Ok(users);
        }

        //get the user with the specific username
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            return await _uow.UserRepository.GetMemberAsync(username);
        }

        //update user
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            //get the loggedin user
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            //map the updateDTO to the user
            _mapper.Map(memberUpdateDTO, user);

            //save the changes, return status
            if (await _uow.Complete()) return NoContent();
            return BadRequest("Failed to update the user");
        }

        //upload user photo
        [HttpPost("user-photo")]
        public async Task<ActionResult<PhotoDTO>> UploadImage(PhotoUploadDTO uploadedPhotoDTO)
        {
            //get the loggedin user
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            //create new Photo object (using entity)
            var photo = new Photo
            {
                Url = uploadedPhotoDTO.imageUrl
            };

            //check whether it is the first photo, if is, set as main
            if (user.Photos.Count == 0) photo.IsMain = true;

            //add new photo object
            user.Photos.Add(photo);

            //return the photo object or status
            if (await _uow.Complete()) return _mapper.Map<PhotoDTO>(photo);
            return BadRequest("Photo could not be added");

        }

        //set the photo as the main photo
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            //get the loggedin user
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            //get the photo
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return NotFound();

            //check if the photo is already the main photo
            if (photo.IsMain) return BadRequest("This is already your main photo.");

            //get the current main photo, and make it regular photo
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;

            //set the new photo as the main
            photo.IsMain = true;

            //return the status
            if (await _uow.Complete()) return NoContent();
            return BadRequest("An error occurred while setting the main photo.");
        }


        //delete the user photo using photoId
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            //get the loggedin user
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            //get the photo
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return NotFound("Image not found");

            //check if the photo is main photo
            if (photo.IsMain) return BadRequest("You cannot delete your main photo.");

            //delete the photo
            user.Photos.Remove(photo);

            //return Ok status or error
            if (await _uow.Complete()) return Ok();
            return BadRequest("Error occurred on deleting photo.");

        }
    }
}