using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Identity.DynamicsCrm
{
    public class UserStore : IUserStore<CrmIdentityUser>, IUserLoginStore<CrmIdentityUser, string>, IUserClaimStore<CrmIdentityUser, string>, IUserEmailStore<CrmIdentityUser, string>, IUserLockoutStore<CrmIdentityUser, string>, IUserPasswordStore<CrmIdentityUser>, IUserTwoFactorStore<CrmIdentityUser, string>, IUserSecurityStampStore<CrmIdentityUser>, IUserPhoneNumberStore<CrmIdentityUser>, IUserRoleStore<CrmIdentityUser>
    {

        #region IUserStore implementation

        public Task CreateAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = DAL.XrmCore.RetrieveByAttribute("contact", "emailaddress1", user.Email);
                if (col.Entities.Count > 0)
                {
                    user.ContactId = col.Entities.First().ToEntityReference();
                }
                DAL.XrmCore.CreateEntity(user.AsEntity());
            });
        }

        public Task DeleteAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew(() => DAL.XrmCore.DeleteEntity("appl_webuser", new Guid(user.Id)));
        }

        public Task<CrmIdentityUser> FindByIdAsync(string userId)
        {
            return Task.Factory.StartNew<CrmIdentityUser>(() =>
            {
                Entity e = DAL.XrmCore.Retrieve("appl_webuser", new Guid(userId));
                return e == null ? null : CrmIdentityUser.ConvertToIdentityUser(e);
            });
        }

        public Task<CrmIdentityUser> FindByNameAsync(string userName)
        {
            return Task.Factory.StartNew<CrmIdentityUser>(() =>
            {
                EntityCollection col = DAL.XrmCore.RetrieveByAttribute("appl_webuser", "appl_username", userName);
                if (col.Entities.Count > 0)
                {
                    return CrmIdentityUser.ConvertToIdentityUser(col.Entities.First());
                }
                else
                {
                    return null;
                }
            });
        }

        public Task UpdateAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew(() => DAL.XrmCore.UpdateEntity(user.AsEntity()));
        }

        public void Dispose()
        {
            DAL.XrmConnection.Connection = null;
            // throw new NotImplementedException();
        }

        #endregion

        #region IUserLoginStore implementation

        public Task AddLoginAsync(CrmIdentityUser user, UserLoginInfo login)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = new EntityCollection() { EntityName = "appl_webuserlogin" };
                Entity e = new Entity("appl_webuserlogin");
                e["appl_loginprovider"] = login.LoginProvider;
                e["appl_providerkey"] = login.ProviderKey;
                col.Entities.Add(e);
                DAL.XrmCore.AddRelated(new Entity("appl_webuser", new Guid(user.Id)), col, "appl_webuser_appl_webuserlogin");
            });
        }

        public Task<CrmIdentityUser> FindAsync(UserLoginInfo login)
        {
            return Task.Factory.StartNew<CrmIdentityUser>(() =>
            {
                Entity result = DAL.XrmCore.GetWebUserFromLogin(login.LoginProvider, login.ProviderKey);
                if (result != null)
                {
                    return CrmIdentityUser.ConvertToIdentityUser(result);
                }
                else
                {
                    return null;
                }
            });
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<IList<UserLoginInfo>>(() =>
            {
                List<UserLoginInfo> list = new List<UserLoginInfo>();
                EntityCollection col = DAL.XrmCore.GetRelated(new Entity("appl_webuser", new Guid(user.Id)), "appl_webuserlogin", "appl_webuserid");
                foreach (Entity e in col.Entities)
                {
                    list.Add(new UserLoginInfo(e.GetAttributeValue<string>("appl_loginprovider"), e.GetAttributeValue<string>("appl_providerkey")));
                }
                return list;
            });
        }

        public Task RemoveLoginAsync(CrmIdentityUser user, UserLoginInfo login)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = DAL.XrmCore.GetRelated(new Entity("appl_webuser", new Guid(user.Id)), "appl_webuserlogin", "appl_webuserid");
                Entity e = col.Entities.FirstOrDefault(x => x.GetAttributeValue<string>("appl_loginprovider").Equals(login.LoginProvider, StringComparison.OrdinalIgnoreCase) &&
                    x.GetAttributeValue<string>("appl_providerkey").Equals(login.ProviderKey));
                if (e != null)
                {
                    DAL.XrmCore.DeleteEntity(e);
                }
            });
        }

        #endregion

        #region IUserClaimStore implementation

        public Task AddClaimAsync(CrmIdentityUser user, System.Security.Claims.Claim claim)
        {
            return Task.Factory.StartNew(() =>
            {
                Entity e = new Entity("appl_webuserclaim",Guid.NewGuid());
                e["appl_claimtype"] = claim.Type;
                e["appl_claimvalue"] = claim.Value;
                e["appl_webuserid"] = user.AsEntityReference();
                
                
                DAL.XrmCore.CreateEntity(e);
            });
        }

        public Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<IList<System.Security.Claims.Claim>>(() =>
            {
                List<System.Security.Claims.Claim> list = new List<System.Security.Claims.Claim>();
                EntityCollection col = DAL.XrmCore.GetRelated(new Entity("appl_webuser", new Guid(user.Id)), "appl_webuserclaim", "appl_webuserid");
                foreach (Entity e in col.Entities)
                {
                    list.Add(new System.Security.Claims.Claim(e.GetAttributeValue<string>("appl_claimtype"), e.GetAttributeValue<string>("appl_claimvalue")));
                }
                return list;
            });
        }

        public Task RemoveClaimAsync(CrmIdentityUser user, System.Security.Claims.Claim claim)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = DAL.XrmCore.GetRelated(new Entity("appl_webuser", new Guid(user.Id)), "appl_webuserclaim", "appl_webuserid");
                Entity e = col.Entities.FirstOrDefault(x => x.GetAttributeValue<string>("appl_claimtype").Equals(claim.Type) && x.GetAttributeValue<string>("appl_claimvalue").Equals(claim.Value));
                if (e != null)
                {
                    DAL.XrmCore.DeleteEntity(e);
                }
            });
        }

        #endregion

        #region IUserEmailStore implementation

        public Task<CrmIdentityUser> FindByEmailAsync(string email)
        {
            return Task.Factory.StartNew<CrmIdentityUser>(() =>
            {
                EntityCollection col = DAL.XrmCore.RetrieveByAttribute("appl_webuser", "appl_email", email);
                if (col.Entities.Count > 0)
                {
                    CrmIdentityUser.ConvertToIdentityUser(col.Entities.First());
                }
                else
                {
                    return null;
                }
                return null;
            });
        }

        public Task<string> GetEmailAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<string>(() => user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<bool>(() => user.EmailConfirmed);
        }

        public Task SetEmailAsync(CrmIdentityUser user, string email)
        {
            return Task.Factory.StartNew(() =>
            {
                //Entity e = new Entity("appl_webuser", new Guid(user.Id));
                //e["appl_email"] = email;
                //DAL.XrmCore.UpdateEntity(e);
                user.Email = email;
            });
        }

        public Task SetEmailConfirmedAsync(CrmIdentityUser user, bool confirmed)
        {
            return Task.Factory.StartNew(() =>
            {
                //Entity e = new Entity("appl_webuser", new Guid(user.Id));
                //e["appl_emailconfirmed"] = confirmed;
                //DAL.XrmCore.UpdateEntity(e);
                user.EmailConfirmed = confirmed;
            });
        }

        #endregion

        public Task<int> GetAccessFailedCountAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<int>(() => user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<bool>(() => user.LockoutEnabled);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<DateTimeOffset>(() =>
            {
                return user.LockoutEndDateUtc.HasValue ? new DateTimeOffset(user.LockoutEndDateUtc.Value) : DateTimeOffset.MinValue;
            });
        }

        public Task<int> IncrementAccessFailedCountAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<int>(() =>
            {
                user.AccessFailedCount += 1;
                DAL.XrmCore.UpdateEntity(user.AsEntity());
                return user.AccessFailedCount;
            });
        }

        public Task ResetAccessFailedCountAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<int>(() =>
            {
                user.AccessFailedCount = 0;
                DAL.XrmCore.UpdateEntity(user.AsEntity());
                return user.AccessFailedCount;
            });
        }

        public Task SetLockoutEnabledAsync(CrmIdentityUser user, bool enabled)
        {
            return Task.Factory.StartNew(() =>
            {
                user.LockoutEnabled = enabled;
                // DAL.XrmCore.UpdateEntity(user.AsEntity());
            });
        }

        public Task SetLockoutEndDateAsync(CrmIdentityUser user, DateTimeOffset lockoutEnd)
        {
            return Task.Factory.StartNew(() =>
            {
                user.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
                // DAL.XrmCore.UpdateEntity(user.AsEntity());
            });
        }

        #region IUserPasswordStore implementation

        public Task<string> GetPasswordHashAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<string>(() => { return user.PasswordHash; });
        }

        public Task<bool> HasPasswordAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<bool>(() => { return !string.IsNullOrEmpty(user.PasswordHash); });
        }

        public Task SetPasswordHashAsync(CrmIdentityUser user, string passwordHash)
        {
            return Task.Factory.StartNew(() =>
            {
                user.PasswordHash = passwordHash;
            });
        }

        #endregion

        #region IUserTwoFactorStore implementation

        public Task<bool> GetTwoFactorEnabledAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<bool>(() => { return user.TwoFactorEnabled; });
        }

        public Task SetTwoFactorEnabledAsync(CrmIdentityUser user, bool enabled)
        {
            return Task.Factory.StartNew(() => { user.TwoFactorEnabled = enabled; });
        }

        #endregion

        #region IUserSecurityStampStore implementation

        public Task<string> GetSecurityStampAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<string>(() => { return user.SecurityStamp; });
        }

        public Task SetSecurityStampAsync(CrmIdentityUser user, string stamp)
        {
            return Task.Factory.StartNew(() => { user.SecurityStamp = stamp; });
        }

        #endregion

        #region IUserPhoneNumberStore implementation

        public Task<string> GetPhoneNumberAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<string>(() => { return user.PhoneNumber; });
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(CrmIdentityUser user)
        {
            return Task.Factory.StartNew<bool>(() => { return user.PhoneNumberConfirmed; });
        }

        public Task SetPhoneNumberAsync(CrmIdentityUser user, string phoneNumber)
        {
            return Task.Factory.StartNew(() => { user.PhoneNumber = phoneNumber; });
        }

        public Task SetPhoneNumberConfirmedAsync(CrmIdentityUser user, bool confirmed)
        {
            return Task.Factory.StartNew(() => { user.PhoneNumberConfirmed = confirmed; });
        }

        #endregion

        #region IUserRoleStore implementation

        public Task AddToRoleAsync(CrmIdentityUser user, string roleName)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = DAL.XrmCore.GetRelated(new Entity("appl_webuser", new Guid(user.Id)), "appl_webuserrole", "appl_webuserid");
                if (col.Entities.Count(x => x.GetAttributeValue<string>("appl_name").Equals(roleName, StringComparison.OrdinalIgnoreCase)) == 0)
                {
                    Entity e = new Entity("appl_webuserrole", Guid.NewGuid());
                    e["appl_name"] = roleName;
                    e["appl_webuserid"] = new EntityReference("appl_webuser", new Guid(user.Id));
                    DAL.XrmCore.CreateEntity(e);
                }
            });

        }

        public Task<IList<string>> GetRolesAsync(CrmIdentityUser user)
        {
            EntityCollection col = DAL.XrmCore.GetRelated(new Entity("appl_webuser", new Guid(user.Id)), "appl_webuserrole", "appl_webuserid");
            return Task.FromResult<IList<string>>(col.Entities.Select(x => x.GetAttributeValue<string>("appl_name")).ToList());
        }

        public async Task<bool> IsInRoleAsync(CrmIdentityUser user, string roleName)
        {
            IList<string> roles = await GetRolesAsync(user);
            return roles.Contains(roleName);
        }

        public Task RemoveFromRoleAsync(CrmIdentityUser user, string roleName)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = DAL.XrmCore.GetRelated(new Entity("appl_webuser", new Guid(user.Id)), "appl_webuserrole", "appl_webuserid");
                Entity e = col.Entities.FirstOrDefault(x => x.GetAttributeValue<string>("appl_name").Equals(roleName, StringComparison.OrdinalIgnoreCase));
                if (e != null)
                {
                    DAL.XrmCore.DeleteEntity(e);
                }
            });
            


        }

        #endregion
    }
}
