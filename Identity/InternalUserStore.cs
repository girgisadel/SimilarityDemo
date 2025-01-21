using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimilarityDemo.Data.Contexts;
using SimilarityDemo.Data.Entities;

namespace SimilarityDemo.Identity;

public class InternalUserStore<TUser>(IdentityDatabase context,
    IdentityErrorDescriber? describer = null)
    : UserStore<TUser, Role, IdentityDatabase, long>(context, describer), IInternalUserStore<TUser>
    where TUser : User
{
    public async Task<bool> IsNameInUseAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var user = await Users
            .AsNoTracking()
            .Select(u => u.NormalizedUserName)
            .FirstOrDefaultAsync(n => n == normalizedUserName, cancellationToken);

        return !string.IsNullOrEmpty(user);
    }
}
