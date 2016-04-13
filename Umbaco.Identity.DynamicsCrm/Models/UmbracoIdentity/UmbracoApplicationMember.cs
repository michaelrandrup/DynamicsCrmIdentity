using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using UmbracoIdentity;
using UmbracoIdentity.Models;
using Umbraco.Core;
using System;
using System.Collections.Generic;

namespace Umbaco.Identity.DynamicsCrm.Models.UmbracoIdentity
{
    public class UmbracoApplicationMember : UmbracoIdentityMember
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<UmbracoApplicationMember, int> manager)
        {
            // Note the authenticationType must match the one 
            // defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            
            // Add custom user claims here
            return userIdentity;
        }

        
    }
    public class CrmExternalLoginStore : IExternalLoginStore
    {
        public void DeleteUserLogins(int memberId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> Find(UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IdentityMemberLogin<int>> GetAll(int userId)
        {
            throw new NotImplementedException();
        }

        public void SaveUserLogins(int memberId, IEnumerable<UserLoginInfo> logins)
        {
            throw new NotImplementedException();
        }
    }
    public class CrmUserStore : UmbracoMembersUserStore<UmbracoApplicationMember>
    {
        public CrmUserStore(ApplicationContext context)
            : base(context.Services.MemberService,context.Services.MemberTypeService,context.Services.MemberGroupService,null,new CrmExternalLoginStore())
        {

        }
        public override Task CreateAsync(UmbracoApplicationMember user)
        {
            return base.CreateAsync(user);
        }

        public override Task UpdateAsync(UmbracoApplicationMember user)
        {
            return base.UpdateAsync(user);
        }

        


    }
}
