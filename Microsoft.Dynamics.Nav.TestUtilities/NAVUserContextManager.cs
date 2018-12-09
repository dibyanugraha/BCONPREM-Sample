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
    public class NAVUserContextManager : UserContextManager
    {
        public string DefaultNAVUserName { get; private set; }
        public string DefaultNAVPassword { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="navServerUrl">URL for NAV ClientService</param>
        /// <param name="defaultTenantId">Default Tenant Id</param>
        /// <param name="defaultCompanyName">Company</param>
        /// <param name="authenticationScheme">Authentication Scheme</param>
        /// <param name="roleCenterId">Role Center to use for the users</param>
        /// <param name="defaultNAVUserName">Default User Name</param>
        /// <param name="defaultNAVPassword">Default Password</param>
        public NAVUserContextManager(string navServerUrl, string defaultTenantId, string defaultCompanyName, int? roleCenterId, string defaultNAVUserName, string defaultNAVPassword)
            : base(navServerUrl, defaultTenantId, defaultCompanyName, roleCenterId)
        {
            this.DefaultNAVUserName = defaultNAVUserName;
            this.DefaultNAVPassword = defaultNAVPassword;
        }

        protected override UserContext CreateUserContext(TestContext testContext)
        {
            string tenantId;
            string companyName;
            string userName;
            GetTenantAndUserName(testContext, out tenantId, out companyName, out userName);
            return new UserContext(tenantId, companyName, AuthenticationScheme.UserNamePassword, userName, DefaultNAVPassword);
        }

        protected override void GetTenantAndUserName(TestContext testContext, out string tenantId, out string companyName, out string userName)
        {
            LoadTestUserContext loadTestUserContext = testContext.GetLoadTestUserContext();
            tenantId = DefaultTenantId;
            companyName = DefaultCompanyName;
            userName = DefaultNAVUserName;

            if (loadTestUserContext != null)
            {
                // add the load test user id as a suffix to the default user name
                userName = String.Format("{0}{1}", DefaultNAVUserName, loadTestUserContext.UserId);

                // add the load test user id as a suffix to the default tenant name
                // tenantId = String.Format("{0}{1}", DefaultTenantId, loadTestUserContext.UserId);
            }
        }
    }
}
