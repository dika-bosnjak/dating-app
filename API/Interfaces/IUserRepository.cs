using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    //interface for user manipulation in the db
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsernameAsync(string username);
        Task<PagedList<MemberDTO>> GetMembersAsync(QueryParams queryParams);
        Task<MemberDTO> GetMemberAsync(string username);
        Task<string> GetUserGender(string username);
    }
}