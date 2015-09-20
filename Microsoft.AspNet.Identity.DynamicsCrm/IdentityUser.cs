using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Identity.DynamicsCrm
{
    public class IdentityUser : IUser<Guid>
    {
        #region Crm Properties
        private string _EntityName = "appl_webuser";

        public virtual string EntityName
        {
            get { return _EntityName; }
            set { _EntityName = value; }
        }

        #endregion


        /// <summary>
        /// Default constructor 
        /// </summary>
        public IdentityUser()
        {
            Id = Guid.Empty;
        }

        /// <summary>
        /// Constructor that takes user name as argument
        /// </summary>
        /// <param name="userName"></param>
        public IdentityUser(string userName)
            : this()
        {
            UserName = userName;
        }

        /// <summary>
        /// User ID
        /// </summary>
        public Guid Id { get; set; }

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


        internal static Entity ConvertToEntity(IdentityUser user)
        {
            Entity e = new Entity(user.EntityName, user.Id);
            FillEntity(e,user);
            return e;
        }

        internal static IdentityUser ConvertToIdentityUser(Entity entity)
        {
            IdentityUser user = new IdentityUser() { EntityName = entity.LogicalName, Id = entity.Id };
            FillIdentityUser(user, entity);
            return user;

        }

        public virtual Entity AsEntity()
        {
            return ConvertToEntity(this);
        }

        protected static void FillEntity(Entity entity, IdentityUser user)
        {

        }
        protected static void FillIdentityUser(IdentityUser user,Entity entity)
        {

        }
        
       
    }
}
