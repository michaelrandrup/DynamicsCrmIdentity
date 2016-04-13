using Microsoft.IdentityModel.Claims;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicsCrm.WebsiteIntegration.Core;
using System.Security.Claims;

namespace Microsoft.AspNet.Identity.DynamicsCrm
{


    public class CrmIdentityUser<T> : ICrmIdentityUser<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Default constructor 
        /// </summary>
        public CrmIdentityUser()
        {
            Key = Guid.NewGuid();
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

        private T _Id = default(T);
        public T Id
        {
            get
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)Convert.ChangeType(this.Key.ToString(), typeof(T));
                }
                return _Id;
            }
        }

        #region Crm Properties
        private string _EntityName = "appl_webuser";

        public virtual string EntityName
        {
            get { return _EntityName; }
            set { _EntityName = value; }
        }

        #endregion





        

        public Guid Key { get; set; }
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

        public static Entity ConvertToEntity<TKey>(ICrmIdentityUser<T> user) where TKey : IEquatable<TKey>
        {
            Entity e = new Entity(user.EntityName, user.Key);
            FillEntity(e, user);
            return e;
        }

        public static ICrmIdentityUser<T> ConvertToIdentityUser<TKey>(Entity entity, Action<ICrmIdentityUser<T>, Entity> PopulateUser = null) where TKey : IEquatable<TKey>
        {
            ICrmIdentityUser<T> user = new CrmIdentityUser<T>() { EntityName = entity.LogicalName, Key = entity.Id };
            FillIdentityUser<T>((ICrmIdentityUser<T>)user, entity, PopulateUser);
            return (ICrmIdentityUser<T>)user;

        }

        public virtual Entity AsEntity()
        {
            return ConvertToEntity<T>(this);
        }

        public virtual EntityReference AsEntityReference()
        {
            return new EntityReference("appl_webuser", this.Key);
        }

        protected static void FillEntity<TKey>(Entity entity, ICrmIdentityUser<TKey> user) where TKey : IEquatable<TKey>
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
        protected static void FillIdentityUser<TKey>(ICrmIdentityUser<TKey> user, Entity entity, Action<ICrmIdentityUser<TKey>, Entity> PopulateUser = null) where TKey : IEquatable<TKey>
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
                entity["appl_securitystamp"] = user.SecurityStamp;
                XrmCore.UpdateEntity(entity);
            }
            if (PopulateUser != null)
            {
                PopulateUser(user, entity);
            }
        }



        public delegate Task AddCustomClaimsToIdentityDelegate<T1>(System.Security.Claims.ClaimsIdentity userIdentity, UserManager<CrmIdentityUser<T1>, T1> manager) where T1 : IEquatable<T1>;
        public static AddCustomClaimsToIdentityDelegate<T> AddCustomClaimsToIdentity;


        Entity ICrmIdentityUser<T>.AsEntity()
        {
            throw new NotImplementedException();
        }

        EntityReference ICrmIdentityUser<T>.AsEntityReference()
        {
            throw new NotImplementedException();
        }

        public async Task<System.Security.Claims.ClaimsIdentity> GenerateUserIdentityAsync<T1>(UserManager<CrmIdentityUser<T1>, T1> manager) where T1 : IEquatable<T1>
        {
            var userIdentity = await manager.CreateIdentityAsync((this as CrmIdentityUser<T1>), DefaultAuthenticationTypes.ApplicationCookie);
            if (AddCustomClaimsToIdentity != null)
            {
                await AddCustomClaimsToIdentity(userIdentity, (manager as UserManager<CrmIdentityUser<T>, T>));
            }

            return userIdentity;
        }


    }
}
