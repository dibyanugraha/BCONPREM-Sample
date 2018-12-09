using System.Security.Principal;
using Microsoft.Dynamics.Framework.UI.Client;
using Microsoft.Dynamics.Nav.UserSession;
using Microsoft.VisualStudio.TestTools.LoadTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Dynamics.Nav.TestUtilities
{
    /// <summary>
    /// WindowsUserContextManager manages user contexts for a given tenant/company/user
    /// All virtual users use the current Windows Identity 
    /// </summary>
    public class WindowsUserContextManager : UserContextManager
    {
        /// <summary>
        /// Creates the WindowsUserContextManager for a given tenant/company/user
        /// </summary>
        /// <param name="navServerUrl">URL for NAV ClientService</param>
        /// <param name="defaultTenantId">Default Tenant Id</param>
        /// <param name="defaultCompanyName">Company</param>
        /// <param name="roleCenterId">Role Center to use for the users</param>
        public WindowsUserContextManager(string navServerUrl, string defaultTenantId, string defaultCompanyName, int? roleCenterId)
            : base(navServerUrl, defaultTenantId, defaultCompanyName, roleCenterId) {}


        protected override UserContext CreateUserContext(TestContext testContext)
        {
            string tenantId;
            string companyName;
            string userName;
            GetTenantAndUserName(testContext, out tenantId, out companyName, out userName);
            return new UserContext(tenantId, companyName, AuthenticationScheme.Windows, userName);
        }

        protected override void GetTenantAndUserName(TestContext testContext, out string tenantId, out string companyName, out string userName)
        {
            LoadTestUserContext loadTestUserContext = testContext.GetLoadTestUserContext();

            tenantId = DefaultTenantId;
            companyName = DefaultCompanyName;
            userName = WindowsIdentity.GetCurrent().Name;

            if (loadTestUserContext != null)
            {
                // add the load test user id as a suffix to the default tenant name
                // tenantId = String.Format("{0}{1}", DefaultTenantId, loadTestUserContext.UserId);
            }

        }
    }
}
