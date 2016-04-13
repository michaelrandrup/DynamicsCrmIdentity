using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.DynamicsCrm;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using DefaultMvcWebsite.Models;


namespace DefaultMvcWebsite
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

     
    public class ApplicationUserManager : UserManager<CrmIdentityUser<string>, string>
    {
        public ApplicationUserManager(IUserStore<CrmIdentityUser<string>,string> store)
            : base(store)
        {
            
        }

        public IUserStore<CrmIdentityUser<string>, string> CrmStore
        {
            get
            {
                return this.Store;
            }
        }

        /// <summary>
        /// This static method is setup as a delegate in Startup.Auth and will be called each time a userIdentity is created
        /// </summary>
        /// <param name="userIdentity">the ClaimsIdentity created from CRM</param>
        /// <param name="manager">the user manager used to add claims in the CRM storage</param>
        public static async Task AddCustomUserClaims(System.Security.Claims.ClaimsIdentity userIdentity, UserManager<CrmIdentityUser<string>,string> manager)
        {
            // Here you can add your custom claims to the userIdentity
            // Below is an example of how to add a custom claim:

            // Check if the customClaim has been retrieved from CRM storage
            if (!userIdentity.HasClaim("MyClaimType","MyClaimValue"))
            {
                // Add the claim to the CRM Claim storage
                System.Security.Claims.Claim customClaim = new Claim("MyClaimType","MyClaimValue");
                IdentityResult result = await manager.AddClaimAsync(userIdentity.GetUserId(), customClaim);
                
                // If all goes well, add the claim to the userIdentity. Next time the user logs in 
                if (result.Succeeded)
                {
                    userIdentity.AddClaim(customClaim);
                }
                else
                {
                    // Handle the error
                }
            }
            
        }


        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            
            
            var manager = new ApplicationUserManager((new UserStore<CrmIdentityUser<string>,string>() as IUserStore<CrmIdentityUser<string>, string>));
            
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<CrmIdentityUser<string>,string>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<CrmIdentityUser<string>>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<CrmIdentityUser<string>>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<CrmIdentityUser<string>>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

     
    public class ApplicationSignInManager : SignInManager<CrmIdentityUser<string>, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(CrmIdentityUser<string> user)
        {
            return user.GenerateUserIdentityAsync<string>(UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }


}
