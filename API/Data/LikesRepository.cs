using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
        }

        //get the specific user like by source user id and target user id
        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        //get all the likeDTOs (users that liked the member, or users that are liked by the member)
        public async Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams)
        {
            //get all users
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            //get all likes
            var likes = _context.Likes.AsQueryable();

            //check predicate from params - if the liked users are requested
            if (likesParams.Predicate == "liked")
            {
                //get all users that are liked by the logged in user
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like.TargetUser);
            }

            //check predicate from params - if the liked by users are requested
            if (likesParams.Predicate == "likedBy")
            {
                //get all users that liked the logged in user
                likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }

            //select only neccessary information
            var likedUsers = users.Select(user => new LikeDTO
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = user.City,
                Gender = user.Gender,
                Id = user.Id
            });

            //return all users with pagination
            return await PagedList<LikeDTO>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        //get the user by id including the liked users
        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
            .Include(x => x.LikedUsers)
            .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}