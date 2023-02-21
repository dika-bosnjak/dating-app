using API.Entities;

namespace API.Interfaces
{
    //interface for token manipulation (create token method)
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}