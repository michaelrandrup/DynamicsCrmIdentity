using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Identity.DynamicsCrm
{
    class UserStore : IUserStore<IdentityUser, Guid>, IUserLoginStore<IdentityUser, Guid>, IUserClaimStore<IdentityUser,Guid>, IUserEmailStore<IdentityUser, Guid>
    {

        #region IUserStore implementation

        public Task CreateAsync(IdentityUser user)
        {
            return Task.Factory.StartNew(() => DAL.XrmCore.CreateEntity(user.AsEntity()));
        }

        public Task DeleteAsync(IdentityUser user)
        {
            return Task.Factory.StartNew(() => DAL.XrmCore.DeleteEntity("appl_webuser", user.Id));
        }

        public Task<IdentityUser> FindByIdAsync(Guid userId)
        {
            return Task.Factory.StartNew<IdentityUser>(() =>
            {
                Entity e = DAL.XrmCore.Retrieve("appl_webuser", userId);
                return e == null ? null : IdentityUser.ConvertToIdentityUser(e);
            });
        }

        public Task<IdentityUser> FindByNameAsync(string userName)
        {
            return Task.Factory.StartNew<IdentityUser>(() =>
            {
                EntityCollection col = DAL.XrmCore.RetrieveByAttribute("appl_webuser", "appl_username", userName);
                if (col.Entities.Count > 0)
                {
                    return IdentityUser.ConvertToIdentityUser(col.Entities.First());
                }
                else
                {
                    return null;
                }
            });
        }

        public Task UpdateAsync(IdentityUser user)
        {
            return Task.Factory.StartNew(() => DAL.XrmCore.UpdateEntity(user.AsEntity()));
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IUserLoginStore implementation

        public Task AddLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = new EntityCollection() { EntityName = "appl_webuserlogin" };
                Entity e = new Entity("appl_webuserlogin");
                e["appl_loginprovider"] = login.LoginProvider;
                e["appl_providerkey"] = login.ProviderKey;
                col.Entities.Add(e);
                DAL.XrmCore.AddRelated(new Entity("appl_webuser", user.Id), col, "appl_webuser_appl_webuserlogin");
            });
        }

        public Task<IdentityUser> FindAsync(UserLoginInfo login)
        {
            return Task.Factory.StartNew<IdentityUser>(() =>
            {
                Entity result = DAL.XrmCore.GetWebUserFromLogin(login.LoginProvider, login.ProviderKey);
                if (result != null)
                {
                    return IdentityUser.ConvertToIdentityUser(result);
                }
                else
                {
                    return null;
                }
            });
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user)
        {
            return Task.Factory.StartNew<IList<UserLoginInfo>>(() =>
            {
                List<UserLoginInfo> list = new List<UserLoginInfo>();
                EntityCollection col = DAL.XrmCore.GetRelated(new Entity("appl_webuser", user.Id), "appl_webuserlogin", "appl_webuserid");
                foreach (Entity e in col.Entities)
                {
                    list.Add(new UserLoginInfo(e.GetAttributeValue<string>("appl_loginprovider"), e.GetAttributeValue<string>("appl_providerkey")));
                }
                return list;
            });
        }

        public Task RemoveLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = DAL.XrmCore.GetRelated(new Entity("appl_webuser", user.Id), "appl_webuserlogin", "appl_webuserid");
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

        public Task AddClaimAsync(IdentityUser user, System.Security.Claims.Claim claim)
        {
            return Task.Factory.StartNew(() =>
            {
                EntityCollection col = new EntityCollection() { EntityName = "appl_webuserclaim" };
                Entity e = new Entity("appl_webuserclaim");
                e["appl_claimtype"] = claim.Type;
                e["appl_claimvalue"] = claim.Value;
                col.Entities.Add(e);
                DAL.XrmCore.AddRelated(new Entity("appl_webuser", user.Id), col, "appl_webuser_appl_webuserclaim");
            });
        }

        public Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(IdentityUser user)
        {
            return Task.Factory.StartNew<IList<System.Security.Claims.Claim>>(() =>
            {
                List<System.Security.Claims.Claim> list = new List<System.Security.Claims.Claim>();
                EntityCollection col = DAL.XrmCore.GetRelated(new Entity("appl_webuser", user.Id), "appl_webuserclaim", "appl_webuserid");
                foreach (Entity e in col.Entities)
                {
                    list.Add(new System.Security.Claims.Claim(e.GetAttributeValue<string>("appl_claimtype"), e.GetAttributeValue<string>("appl_claimvalue")));
                }
                return list;
            });
        }

        public Task RemoveClaimAsync(IdentityUser user, System.Security.Claims.Claim claim)
        {
            return Task.Factory.StartNew(() => {
                EntityCollection col = DAL.XrmCore.GetRelated(new Entity("appl_webuser", user.Id), "appl_webuserclaim", "appl_webuserid");
                Entity e = col.Entities.FirstOrDefault(x => x.GetAttributeValue<string>("appl_claimtype").Equals(claim.Type) && x.GetAttributeValue<string>("appl_claimvalue").Equals(claim.Value));
                if (e != null)
                {
                    DAL.XrmCore.DeleteEntity(e);
                }
            });
        }

        #endregion

        #region IUserEmailStore implementation

        public Task<IdentityUser> FindByEmailAsync(string email)
        {
            return Task.Factory.StartNew<IdentityUser>(() =>
            {
                EntityCollection col = DAL.XrmCore.RetrieveByAttribute("appl_webuser", "appl_email", email);
                if (col.Entities.Count > 0)
                {
                    IdentityUser.ConvertToIdentityUser(col.Entities.First());
                }
                else
                {
                    return null;
                }
            });
        }

        public Task<string> GetEmailAsync(IdentityUser user)
        {
            return new Task<string>(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(IdentityUser user)
        {
            return new Task<bool>(user.EmailConfirmed);
        }

        public Task SetEmailAsync(IdentityUser user, string email)
        {
            return Task.Factory.StartNew(() =>
            {
                Entity e = new Entity("appl_webuser", user.Id);
                e["appl_email"] = email;
                DAL.XrmCore.UpdateEntity(e);
                user.Email = email;
            });
        }

        public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed)
        {
            return Task.Factory.StartNew(() =>
            {
                Entity e = new Entity("appl_webuser", user.Id);
                e["appl_emailconfirmed"] = confirmed;
                DAL.XrmCore.UpdateEntity(e);
                user.EmailConfirmed = confirmed;
            });
        }

        #endregion
    }
}
