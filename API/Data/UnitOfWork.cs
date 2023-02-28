
using API.Interfaces;
using AutoMapper;

namespace API.Data
{

    //Unit of work creates unique connection to the database for all repositories
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        public IMapper _mapper { get; }

        public UnitOfWork(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;

        }

        //use user repository
        public IUserRepository UserRepository => new UserRepository(_context, _mapper);

        //use message repository
        public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);

        //use like repository
        public ILikesRepository LikesRepository => new LikesRepository(_context);

        //check whether the database request is processed (saved changes)
        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        //check whether are there some changes that are tracked
        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}