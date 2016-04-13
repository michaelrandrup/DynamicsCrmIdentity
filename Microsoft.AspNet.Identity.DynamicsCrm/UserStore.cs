using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicsCrm.WebsiteIntegration.Core;

namespace Microsoft.AspNet.Identity.DynamicsCrm
{
    public class UserStore<T, S> : IUserStore<T,S>, IUserLoginStore<T, S>, IUserClaimStore<T, S>, IUserEmailStore<T, S>, IUserLockoutStore<T, S>, IUserPasswordStore<T,S>, IUserTwoFactorStore<T, S>, IUserSecurityStampStore<T,S>, IUserPhoneNumberStore<T,S>, IUserRoleStore<T,S>
        where T : CrmIdentityUser<S>, new()
        where S : IEquatable<S>
                
    {

        #region IUserStore implementation

        

        public Task CreateAsync(T user)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = XrmCore.RetrieveByAttribute("contact", "emailaddress1", user.Email);
                if (col.Entities.Count > 0)
                {
                    user.Contact = col.Entities.First().ToEntityReference();
                }
                XrmCore.CreateEntity(user.AsEntity());
            });
        }

        public Task DeleteAsync(T user)
        {
            return Task.Factory.StartNew(() => XrmCore.DeleteEntity("appl_webuser", user.Key));
        }

        public Task<T> FindByIdAsync(S userId)
        {
            return Task.Factory.StartNew<T>(() =>
            {
                Entity e = null;
                if (typeof(S) == typeof(Guid) || typeof(S) == typeof(string))
                {
                    e = XrmCore.Retrieve("appl_webuser", Guid.Parse(Convert.ToString(userId)));
                }
                else if (typeof(S) == typeof(int))
                {
                    EntityCollection col = XrmCore.RetrieveByAttribute("appl_webuser", "appl_userid", Convert.ToString(userId), CacheResults: false);
                    e = col.Entities.FirstOrDefault();
                }
                
                return e == null ? default(T): (T)CrmIdentityUser<S>.ConvertToIdentityUser<S>(e);
            });
        }

        public Task<T> FindByNameAsync(string userName)
        {
            return Task.Factory.StartNew<T>(() =>
            {
                EntityCollection col = XrmCore.RetrieveByAttribute("appl_webuser", "appl_username", userName);
                if (col.Entities.Count > 0)
                {
                    return (T)CrmIdentityUser<S>.ConvertToIdentityUser<S>(col.Entities.First());
                }
                else
                {
                    return default(T);
                }
            });
        }

        public Task UpdateAsync(T user)
        {
            return Task.Factory.StartNew(() => XrmCore.UpdateEntity(user.AsEntity()));
        }

        public void Dispose()
        {
            XrmConnection.Connection = null;
            // throw new NotImplementedException();
        }

        public void HashAllPasswords(Func<string, string> PasswordHasher)
        {
            XrmCore.HashAllPasswords(PasswordHasher);
        }

        #endregion

        #region IUserLoginStore implementation

        public Task AddLoginAsync(T user, UserLoginInfo login)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = new EntityCollection() { EntityName = "appl_webuserlogin" };
                Entity e = new Entity("appl_webuserlogin");
                e["appl_loginprovider"] = login.LoginProvider;
                e["appl_providerkey"] = login.ProviderKey;
                col.Entities.Add(e);
                XrmCore.AddRelated(new Entity("appl_webuser", user.Key), col, "appl_webuser_appl_webuserlogin");
            });
        }

        public Task<T> FindAsync(UserLoginInfo login)
        {
            return Task.Factory.StartNew<T>(() =>
            {
                Entity result = XrmCore.GetWebUserFromLogin(login.LoginProvider, login.ProviderKey);
                if (result != null)
                {
                    return (T)CrmIdentityUser<S>.ConvertToIdentityUser<S>(result);
                }
                else
                {
                    return default(T);
                }
            });
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(T user)
        {
            return Task.Factory.StartNew<IList<UserLoginInfo>>(() =>
            {
                List<UserLoginInfo> list = new List<UserLoginInfo>();
                EntityCollection col = XrmCore.GetRelated(new Entity("appl_webuser", user.Key), "appl_webuserlogin", "appl_webuserid");
                foreach (Entity e in col.Entities)
                {
                    list.Add(new UserLoginInfo(e.GetAttributeValue<string>("appl_loginprovider"), e.GetAttributeValue<string>("appl_providerkey")));
                }
                return list;
            });
        }

        public Task RemoveLoginAsync(T user, UserLoginInfo login)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = XrmCore.GetRelated(new Entity("appl_webuser", user.Key), "appl_webuserlogin", "appl_webuserid");
                Entity e = col.Entities.FirstOrDefault(x => x.GetAttributeValue<string>("appl_loginprovider").Equals(login.LoginProvider, StringComparison.OrdinalIgnoreCase) &&
                    x.GetAttributeValue<string>("appl_providerkey").Equals(login.ProviderKey));
                if (e != null)
                {
                    XrmCore.DeleteEntity(e);
                }
            });
        }

        #endregion

        #region IUserClaimStore implementation

        public Task AddClaimAsync(T user, System.Security.Claims.Claim claim)
        {
            return Task.Factory.StartNew(() =>
            {
                Entity e = new Entity("appl_webuserclaim",Guid.NewGuid());
                e["appl_claimtype"] = claim.Type;
                e["appl_claimvalue"] = claim.Value;
                e["appl_webuserid"] = user.AsEntityReference();
                XrmCore.CreateEntity(e);
            });
        }

        public Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(T user)
        {
            return Task.Factory.StartNew<IList<System.Security.Claims.Claim>>(() =>
            {
                List<System.Security.Claims.Claim> list = new List<System.Security.Claims.Claim>();
                EntityCollection col = XrmCore.GetRelated(new Entity("appl_webuser", user.Key), "appl_webuserclaim", "appl_webuserid");
                foreach (Entity e in col.Entities)
                {
                    list.Add(new System.Security.Claims.Claim(e.GetAttributeValue<string>("appl_claimtype"), e.GetAttributeValue<string>("appl_claimvalue")));
                }
                return list;
            });
        }

        public Task RemoveClaimAsync(T user, System.Security.Claims.Claim claim)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = XrmCore.GetRelated(new Entity("appl_webuser", user.Key), "appl_webuserclaim", "appl_webuserid");
                Entity e = col.Entities.FirstOrDefault(x => x.GetAttributeValue<string>("appl_claimtype").Equals(claim.Type) && x.GetAttributeValue<string>("appl_claimvalue").Equals(claim.Value));
                if (e != null)
                {
                    XrmCore.DeleteEntity(e);
                }
            });
        }

        #endregion

        #region IUserEmailStore implementation

        public Task<T> FindByEmailAsync(string email)
        {
            return Task.Factory.StartNew<T>(() =>
            {
                EntityCollection col = XrmCore.RetrieveByAttribute("appl_webuser", "appl_email", email);
                if (col.Entities.Count > 0)
                {
                    return (T)CrmIdentityUser<S>.ConvertToIdentityUser<S>(col.Entities.First());
                }
                else
                {
                    return default(T);
                }
            });
        }

        public Task<string> GetEmailAsync(T user)
        {
            return Task.Factory.StartNew<string>(() => user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(T user)
        {
            return Task.Factory.StartNew<bool>(() => user.EmailConfirmed);
        }

        public Task SetEmailAsync(T user, string email)
        {
            return Task.Factory.StartNew(() =>
            {
                user.Email = email;
            });
        }

        public Task SetEmailConfirmedAsync(T user, bool confirmed)
        {
            return Task.Factory.StartNew(() =>
            {
                user.EmailConfirmed = confirmed;
            });
        }

        #endregion

        #region IUserLockoutStore

        public Task<int> GetAccessFailedCountAsync(T user)
        {
            return Task.Factory.StartNew<int>(() => user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(T user)
        {
            return Task.Factory.StartNew<bool>(() => user.LockoutEnabled);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(T user)
        {
            return Task.Factory.StartNew<DateTimeOffset>(() =>
            {
                return user.LockoutEndDateUtc.HasValue ? new DateTimeOffset(user.LockoutEndDateUtc.Value) : DateTimeOffset.MinValue;
            });
        }

        public Task<int> IncrementAccessFailedCountAsync(T user)
        {
            return Task.Factory.StartNew<int>(() =>
            {
                user.AccessFailedCount += 1;
                XrmCore.UpdateEntity(user.AsEntity());
                return user.AccessFailedCount;
            });
        }

        public Task ResetAccessFailedCountAsync(T user)
        {
            return Task.Factory.StartNew<int>(() =>
            {
                user.AccessFailedCount = 0;
                XrmCore.UpdateEntity(user.AsEntity());
                return user.AccessFailedCount;
            });
        }

        public Task SetLockoutEnabledAsync(T user, bool enabled)
        {
            return Task.Factory.StartNew(() =>
            {
                user.LockoutEnabled = enabled;
                // XrmCore.UpdateEntity(user.AsEntity());
            });
        }

        public Task SetLockoutEndDateAsync(T user, DateTimeOffset lockoutEnd)
        {
            return Task.Factory.StartNew(() =>
            {
                user.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
                // XrmCore.UpdateEntity(user.AsEntity());
            });
        }

        #endregion

        #region IUserPasswordStore implementation

        public Task<string> GetPasswordHashAsync(T user)
        {
            return Task.Factory.StartNew<string>(() => { return user.PasswordHash; });
        }

        public Task<bool> HasPasswordAsync(T user)
        {
            return Task.Factory.StartNew<bool>(() => { return !string.IsNullOrEmpty(user.PasswordHash); });
        }

        public Task SetPasswordHashAsync(T user, string passwordHash)
        {
            return Task.Factory.StartNew(() =>
            {
                user.PasswordHash = passwordHash;
            });
        }

        #endregion

        #region IUserTwoFactorStore implementation

        public Task<bool> GetTwoFactorEnabledAsync(T user)
        {
            return Task.Factory.StartNew<bool>(() => { return user.TwoFactorEnabled; });
        }

        public Task SetTwoFactorEnabledAsync(T user, bool enabled)
        {
            return Task.Factory.StartNew(() => { user.TwoFactorEnabled = enabled; });
        }

        #endregion

        #region IUserSecurityStampStore implementation

        public Task<string> GetSecurityStampAsync(T user)
        {
            return Task.Factory.StartNew<string>(() => { return user.SecurityStamp; });
        }

        public Task SetSecurityStampAsync(T user, string stamp)
        {
            return Task.Factory.StartNew(() => { user.SecurityStamp = stamp; });
        }

        #endregion

        #region IUserPhoneNumberStore implementation

        public Task<string> GetPhoneNumberAsync(T user)
        {
            return Task.Factory.StartNew<string>(() => { return user.PhoneNumber; });
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(T user)
        {
            return Task.Factory.StartNew<bool>(() => { return user.PhoneNumberConfirmed; });
        }

        public Task SetPhoneNumberAsync(T user, string phoneNumber)
        {
            return Task.Factory.StartNew(() => { user.PhoneNumber = phoneNumber; });
        }

        public Task SetPhoneNumberConfirmedAsync(T user, bool confirmed)
        {
            return Task.Factory.StartNew(() => { user.PhoneNumberConfirmed = confirmed; });
        }

        #endregion

        #region IUserRoleStore implementation

        public Task AddToRoleAsync(T user, string roleName)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = XrmCore.GetRelated(new Entity("appl_webuser", user.Key), "appl_webuserrole", "appl_webuserid");
                if (col.Entities.Count(x => x.GetAttributeValue<string>("appl_name").Equals(roleName, StringComparison.OrdinalIgnoreCase)) == 0)
                {
                    Entity e = new Entity("appl_webuserrole", Guid.NewGuid());
                    e["appl_name"] = roleName;
                    e["appl_webuserid"] = new EntityReference("appl_webuser", user.Key);
                    XrmCore.CreateEntity(e);
                }
            });

        }

        public Task<IList<string>> GetRolesAsync(T user)
        {
            EntityCollection col = XrmCore.GetRelated(new Entity("appl_webuser", user.Key), "appl_webuserrole", "appl_webuserid");
            return Task.FromResult<IList<string>>(col.Entities.Select(x => x.GetAttributeValue<string>("appl_name")).ToList());
        }

        public async Task<bool> IsInRoleAsync(T user, string roleName)
        {
            IList<string> roles = await GetRolesAsync(user);
            return roles.Contains(roleName);
        }

        public Task RemoveFromRoleAsync(T user, string roleName)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = XrmCore.GetRelated(new Entity("appl_webuser", user.Key), "appl_webuserrole", "appl_webuserid");
                Entity e = col.Entities.FirstOrDefault(x => x.GetAttributeValue<string>("appl_name").Equals(roleName, StringComparison.OrdinalIgnoreCase));
                if (e != null)
                {
                    XrmCore.DeleteEntity(e);
                }
            });
            


        }


        #endregion
    }
}
