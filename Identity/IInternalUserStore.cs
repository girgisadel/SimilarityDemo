using Microsoft.AspNetCore.Identity;
using SimilarityDemo.Data.Entities;

namespace SimilarityDemo.Identity;

public interface IInternalUserStore<TUser> : IUserStore<TUser>
    where TUser : User
{
    Task<bool> IsNameInUseAsync(string normalizedUserName, CancellationToken cancellationToken);
}
