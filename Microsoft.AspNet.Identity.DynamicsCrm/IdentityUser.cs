using Microsoft.IdentityModel.Claims;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Identity.DynamicsCrm
{
    

    public class CrmIdentityUser : IUser<string>, IUser
    {
        /// <summary>
        /// Default constructor 
        /// </summary>
        public CrmIdentityUser()
        {
            Id = Guid.NewGuid().ToString();
            SecurityStamp = Guid.NewGuid().ToString();
        }
        /// <summary>
        /// Constructor that takes user name as argument
        /// </summary>
        /// <param name="userName"></param>
        public CrmIdentityUser(string userName)
            : this()
        {
            UserName = userName;
        }



        #region Crm Properties
        private string _EntityName = "appl_webuser";

        public virtual string EntityName
        {
            get { return _EntityName; }
            set { _EntityName = value; }
        }

        #endregion


        

        

        /// <summary>
        /// User ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User's name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Email
        /// </summary>
        public virtual string Email { get; set; }

        /// <summary>
        ///     True if the email is confirmed, default is false
        /// </summary>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        ///     The salted/hashed form of the user password
        /// </summary>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        ///     A random value that should change whenever a users credentials have changed (password changed, login removed)
        /// </summary>
        public virtual string SecurityStamp { get; set; }

        /// <summary>
        ///     PhoneNumber for the user
        /// </summary>
        public virtual string PhoneNumber { get; set; }

        /// <summary>
        ///     True if the phone number is confirmed, default is false
        /// </summary>
        public virtual bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        ///     Is two factor enabled for the user
        /// </summary>
        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        ///     DateTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public virtual DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        ///     Is lockout enabled for this user
        /// </summary>
        public virtual bool LockoutEnabled { get; set; }

        /// <summary>
        ///     Used to record failures for the purposes of lockout
        /// </summary>
        public virtual int AccessFailedCount { get; set; }

        public EntityReference Contact { get; set; }

        public Guid ContactId
        {
            get
            {
                if (Contact != null)
                {
                    return Contact.Id;
                }
                else
                {
                    return Guid.Empty;
                }
            }
        }

        internal static Entity ConvertToEntity(CrmIdentityUser user)
        {
            Entity e = new Entity(user.EntityName, new Guid(user.Id));
            FillEntity(e, user);
            return e;
        }

        internal static CrmIdentityUser ConvertToIdentityUser(Entity entity)
        {
            CrmIdentityUser user = new CrmIdentityUser() { EntityName = entity.LogicalName, Id = entity.Id.ToString() };
            FillIdentityUser(user, entity);
            return user;

        }

        public virtual Entity AsEntity()
        {
            return ConvertToEntity(this);
        }

        public virtual EntityReference AsEntityReference()
        {
            return new EntityReference("appl_webuser", new Guid(this.Id));
        }

        protected static void FillEntity(Entity entity, CrmIdentityUser user)
        {
            entity["appl_contactid"] = user.Contact;
            entity["appl_username"] = user.UserName;
            entity["appl_passwordhash"] = user.PasswordHash;
            entity["appl_email"] = user.Email;
            entity["appl_emailconfirmed"] = user.EmailConfirmed;
            entity["appl_phonenumber"] = user.PhoneNumber;
            entity["appl_phonenumberconfirmed"] = user.PhoneNumberConfirmed;
            entity["appl_twofactorenabled"] = user.TwoFactorEnabled;
            entity["appl_lockoutenabled"] = user.LockoutEnabled;
            entity["appl_lockoutenddateutc"] = user.LockoutEndDateUtc.HasValue ? user.LockoutEndDateUtc.Value.ToUniversalTime() : (DateTime?)null;
            entity["appl_accessfailedcount"] = user.AccessFailedCount;
            entity["appl_securitystamp"] = user.SecurityStamp;



        }
        protected static void FillIdentityUser(CrmIdentityUser user, Entity entity)
        {
            user.AccessFailedCount = entity.GetAttributeValue<int>("appl_accessfailedcount");
            user.Email = entity.GetAttributeValue<string>("appl_email");
            user.EmailConfirmed = entity.GetAttributeValue<bool>("appl_emailconfirmed");
            user.LockoutEnabled = entity.GetAttributeValue<bool>("appl_lockoutenabled");
            user.LockoutEndDateUtc = entity.GetAttributeValue<DateTime?>("appl_lockoutenddateutc");
            user.PasswordHash = entity.GetAttributeValue<string>("appl_passwordhash");
            user.PhoneNumber = entity.GetAttributeValue<string>("appl_phonenumber");
            user.PhoneNumberConfirmed = entity.GetAttributeValue<bool>("appl_phonenumberconfirmed");
            user.TwoFactorEnabled = entity.GetAttributeValue<bool>("appl_twofactorenabled");
            user.UserName = entity.GetAttributeValue<string>("appl_username");
            user.Contact = entity.Contains("appl_contactid") ? entity.GetAttributeValue<EntityReference>("appl_contactid") : null;
            user.SecurityStamp = entity.GetAttributeValue<string>("appl_securitystamp");
            if (string.IsNullOrEmpty(user.SecurityStamp))
            {
                // Always ensure the security stamp is present. If not, create a new one and save it to the backend.
                user.SecurityStamp = Guid.NewGuid().ToString();
                entity["appl_securitystame"] = user.SecurityStamp;
                DAL.XrmCore.UpdateEntity(entity);

            }
        }

        
        
        public delegate Task AddCustomClaimsToIdentityDelegate(System.Security.Claims.ClaimsIdentity userIdentity, UserManager<CrmIdentityUser> manager);
        public static AddCustomClaimsToIdentityDelegate AddCustomClaimsToIdentity = null;
        

        public virtual async Task<System.Security.Claims.ClaimsIdentity> GenerateUserIdentityAsync(UserManager<CrmIdentityUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            if (AddCustomClaimsToIdentity != null)
            {
                await AddCustomClaimsToIdentity(userIdentity, manager);
            }
            
            return userIdentity;
        }


        string IUser<string>.Id
        {
            get { return this.Id.ToString(); }
        }
    }



    //public class IdentityUserLogin
    //{
    //    public IdentityUserLogin();

    //    // Summary:
    //    //     The login provider for the login (i.e. facebook, google)
    //    public virtual string LoginProvider { get; set; }
    //    //
    //    // Summary:
    //    //     Key representing the login for the provider
    //    public virtual string ProviderKey { get; set; }
    //    //
    //    // Summary:
    //    //     User Id for the user who owns this login
    //    public virtual string UserId { get; set; }
    //}

    //// Summary:
    ////     EntityType that represents a user belonging to a role
    ////
    //// Type parameters:
    ////   TKey:
    //public class IdentityUserRole<TKey>
    //{
    //    public IdentityUserRole();

    //    // Summary:
    //    //     RoleId for the role
    //    public virtual TKey RoleId { get; set; }
    //    //
    //    // Summary:
    //    //     UserId for the user that is in the role
    //    public virtual TKey UserId { get; set; }
    //}


    //// Summary:
    ////     EntityType that represents one specific user claim
    ////
    //// Type parameters:
    ////   TKey:
    //public class IdentityUserClaim<TKey>
    //{
    //    public IdentityUserClaim()
    //    {

    //    }

    //    // Summary:
    //    //     Claim type
    //    public virtual string ClaimType { get; set; }
    //    //
    //    // Summary:
    //    //     Claim value
    //    public virtual string ClaimValue { get; set; }
    //    //
    //    // Summary:
    //    //     Primary key
    //    public virtual int Id { get; set; }
    //    //
    //    // Summary:
    //    //     User Id for the user who owns this login
    //    public virtual TKey UserId { get; set; }
    //}



}
