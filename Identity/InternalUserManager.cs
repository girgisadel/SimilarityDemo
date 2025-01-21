using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SimilarityDemo.Data.Entities;

namespace SimilarityDemo.Identity;

public class InternalUserManager<TUser>(IUserStore<TUser> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<TUser> passwordHasher,
    IEnumerable<IUserValidator<TUser>> userValidators,
    IEnumerable<IPasswordValidator<TUser>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<UserManager<TUser>> logger)
    : UserManager<TUser>(store, 
        optionsAccessor, 
        passwordHasher, 
        userValidators, 
        passwordValidators, 
        keyNormalizer, 
        errors, 
        services, 
        logger) where TUser : User
{
    private IInternalUserStore<TUser> GetInternalUserStore()
    {
        if (Store is not IInternalUserStore<TUser> internalUserStore)
        {
            throw new NotSupportedException("Store does not implement IInternalUserStore<TUser>.");
        }

        return internalUserStore;
    }

    public virtual async Task<bool> IsNameInUseAsync(string userName)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(userName);
        var store = GetInternalUserStore();
        userName = NormalizeName(userName);
        return await store.IsNameInUseAsync(userName, CancellationToken).ConfigureAwait(false);
    }
}
