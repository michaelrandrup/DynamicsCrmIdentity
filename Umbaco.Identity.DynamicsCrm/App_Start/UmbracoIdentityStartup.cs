using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using UmbracoIdentity;
using Umbaco.Identity.DynamicsCrm.Models.UmbracoIdentity;
using Umbaco.Identity.DynamicsCrm;
using Owin;
using Umbraco.Web;
using Umbraco.Web.Security.Identity;

[assembly: OwinStartup("UmbracoIdentityStartup", typeof(UmbracoIdentityStartup))]

namespace Umbaco.Identity.DynamicsCrm
{
   
    /// <summary>
    /// OWIN Startup class for UmbracoIdentity 
    /// </summary>
    public class UmbracoIdentityStartup : UmbracoDefaultOwinStartup
    {
        /// <summary>
        /// Configures services to be created in the OWIN context (CreatePerOwinContext)
        /// </summary>
        /// <param name="app"/>
        protected override void ConfigureServices(IAppBuilder app)
        {
            base.ConfigureServices(app);

            //Single method to configure the Identity user manager for use with Umbraco
            app.ConfigureUserManagerForUmbracoMembers<UmbracoApplicationMember>();

            

            



            //Single method to configure the Identity user manager for use with Umbraco
            app.ConfigureRoleManagerForUmbracoMembers<UmbracoApplicationRole>();
        }

        /// <summary>
        /// Configures middleware to be used (i.e. app.Use...)
        /// </summary>
        /// <param name="app"/>
        protected override void ConfigureMiddleware(IAppBuilder app)
        {
            //Ensure owin is configured for Umbraco back office authentication. If you have any front-end OWIN
            // cookie configuration, this must be declared after it.
            app
                .UseUmbracoBackOfficeCookieAuthentication(ApplicationContext, PipelineStage.Authenticate)
                .UseUmbracoBackOfficeExternalCookieAuthentication(ApplicationContext, PipelineStage.Authenticate);                

            // Enable the application to use a cookie to store information for the 
            // signed in user and to use a cookie to temporarily store information 
            // about a user logging in with a third party login provider 
            // Configure the sign in cookie
            app.UseCookieAuthentication(
                //You can modify these options for any customizations you'd like
                new FrontEndCookieAuthenticationOptions(),
                PipelineStage.Authenticate);
            
            // Uncomment the following lines to enable logging in with third party login providers

            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            //app.UseMicrosoftAccountAuthentication(
            //  clientId: "",
            //  clientSecret: "");

            //app.UseTwitterAuthentication(
            //  consumerKey: "",
            //  consumerSecret: "");

            //app.UseFacebookAuthentication(
            //  appId: "",
            //  appSecret: "");

            //app.UseGoogleAuthentication(
            //  clientId: "",
            //  clientSecret: ""); 




            //Lasty we need to ensure that the preview Middleware is registered, this must come after
            // all of the authentication middleware:
            app.UseUmbracoPreviewAuthentication(ApplicationContext, PipelineStage.Authorize);
        }
        
    }
}

