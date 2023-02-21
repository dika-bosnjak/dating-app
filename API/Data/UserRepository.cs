using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        public readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        //GetMemberAsync function returns the member info using his username
        public async Task<MemberDTO> GetMemberAsync(string username)
        {
            return await _context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        }

        //GetMembersAsync function returns the members info using queries
        public async Task<PagedList<MemberDTO>> GetMembersAsync(QueryParams queryParams)
        {
            //create new query
            var query = _context.Users.AsQueryable();

            //query all users without the logged one
            query = query.Where(u => u.UserName != queryParams.CurrentUsername);

            //query all users with the specific gender
            query = query.Where(u => u.Gender == queryParams.Gender);

            //calculate min age and max age using query params
            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-queryParams.MaxAge - 1));
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-queryParams.MinAge));
            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            //sort the results
            query = queryParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };

            //return pagedlist (with pagination)
            return await PagedList<MemberDTO>.CreateAsync(
                query.AsNoTracking().ProjectTo<MemberDTO>(_mapper.ConfigurationProvider),
                queryParams.PageNumber,
                queryParams.PageSize);
        }

        //GetUserByIdAsync function returns the user with the specific id (and his images)
        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context
            .Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.Id == id);
        }

        //GetUserByUsernameAsync function returns the user with a specific username (and his images)
        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context
            .Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username);
        }

        //GetUsersAsync function returns users with theirs images as a list
        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context
            .Users
            .Include(p => p.Photos)
            .ToListAsync();
        }


        //SaveAllAsync function saves all changes on the entity to the db (returns bool - number of entries that are saved/modified > 0)
        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        //Update funstion sets the entry state to modified
        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}