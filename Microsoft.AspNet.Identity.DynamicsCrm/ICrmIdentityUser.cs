using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace Microsoft.AspNet.Identity.DynamicsCrm
{
    public interface ICrmIdentityUser<out T> : IUser<T> where T : IEquatable<T>
    {
        int AccessFailedCount { get; set; }
        EntityReference Contact { get; set; }
        Guid ContactId { get; }
        string Email { get; set; }
        bool EmailConfirmed { get; set; }
        string EntityName { get; set; }
        Guid Key { get; set; }
        bool LockoutEnabled { get; set; }
        DateTime? LockoutEndDateUtc { get; set; }
        string PasswordHash { get; set; }
        string PhoneNumber { get; set; }
        bool PhoneNumberConfirmed { get; set; }
        string SecurityStamp { get; set; }
        bool TwoFactorEnabled { get; set; }
        Entity AsEntity();
        EntityReference AsEntityReference();
        Task<ClaimsIdentity> GenerateUserIdentityAsync<T>(UserManager<CrmIdentityUser<T>, T> manager) where T : IEquatable<T>;
    }
}