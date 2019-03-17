using System;
using Microsoft.Dynamics.Framework.UI.Client;
using Microsoft.Dynamics.Nav.UserSession;
using Microsoft.VisualStudio.TestTools.LoadTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Dynamics.Nav.TestUtilities
{
    /// <summary>
    /// UserContextManager user contexts for a given tenant/company/user
    /// This allows tests to reuse sessions for a given user
    /// to use this class you need to first create a default NAVUser and enough NAVUsers for each virtual test user eg. User, User1, User2...
    /// For simplicity all NAV Users share the same password
    /// </summary>
    public class AADUserContextManager : UserContextManager
    {
        public string DefaultUserName { get; private set; }
        public string DefaultPassword { get; private set; }

        public string Authority { get; private set; }
        public string Resource { get; private set; }
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="navServerUrl">URL for NAV ClientService</param>
        /// <param name="defaultTenantId">Default Tenant Id</param>
        /// <param name="defaultCompanyName">Company</param>
        /// <param name="authenticationScheme">Authentication Scheme</param>
        /// <param name="roleCenterId">Role Center to use for the users</param>
        /// <param name="defaultUserName">Default User Name</param>
        /// <param name="defaultPassword">Default Password</param>
        public AADUserContextManager(string navServerUrl, string defaultTenantId, string defaultCompanyName, int? roleCenterId, string defaultUserName, string defaultPassword, string authority, string resource, string clientId, string clientSecret)
            : base(navServerUrl, defaultTenantId, defaultCompanyName, roleCenterId)
        {
            this.Authority = authority;
            this.Resource = resource;
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.DefaultUserName = defaultUserName;
            this.DefaultPassword = defaultPassword;
        }

        protected override UserContext CreateUserContext(TestContext testContext)
        {
            string tenantId;
            string companyName;
            string userName;
            GetTenantAndUserName(testContext, out tenantId, out companyName, out userName);
            return new UserContext(tenantId, companyName, AuthenticationScheme.AzureActiveDirectory, userName, DefaultPassword, this.Authority, this.Resource, this.ClientId, this.ClientSecret);
        }

        protected override void GetTenantAndUserName(TestContext testContext, out string tenantId, out string companyName, out string userName)
        {
            LoadTestUserContext loadTestUserContext = testContext.GetLoadTestUserContext();
            tenantId = DefaultTenantId;
            companyName = DefaultCompanyName;
            userName = DefaultUserName;
        }
    }
}
