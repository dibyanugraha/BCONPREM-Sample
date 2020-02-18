using System.Globalization;
using Microsoft.Dynamics.Framework.UI.Client;
using Microsoft.Dynamics.Framework.UI.Client.Interactions;
using Microsoft.Dynamics.Nav.LoadTest.Properties;
using Microsoft.Dynamics.Nav.TestUtilities;
using Microsoft.Dynamics.Nav.UserSession;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System;
namespace Microsoft.Dynamics.Nav.LoadTest
{
    /// <summary>
    /// Summary description for PurchasingAgentScenarios
    /// </summary>
    [TestClass]
    public class PurchasingAgentScenarios
    {
        public TestContext TestContext { get; set; }

        private const int PurchasingAgentRoleCenterId = 9007;
        private const int VendorListPageId = 27;
        private const int ItemListPageId = 31;
        private const int PurchaseOrderListPageId = 9307;
        private const int PurchaseOrderPageId = 50;

        private static UserContextManager purchasingAgentUserContextManager;
        public PurchasingAgentScenarios()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public UserContextManager PurchasingAgentUserContextManager
        {
            get
            {
                return purchasingAgentUserContextManager ?? CreateUserContextManager();
            }
        }

        private UserContextManager CreateUserContextManager()
        {
            switch (Authentication)
            {
                case AuthenticationScheme.Windows:
                    // Use the current windows user 
                    purchasingAgentUserContextManager = new WindowsUserContextManager(
                        ClientServiceUrl,
                        null,
                        null,
                        PurchasingAgentRoleCenterId);
                    break;
                case AuthenticationScheme.UserNamePassword:
                    // Use Username / Password authentication
                    purchasingAgentUserContextManager = new NAVUserContextManager(
                        ClientServiceUrl,
                        null,
                        null,
                        PurchasingAgentRoleCenterId,
                        Username,
                        Password);
                    break;
                case AuthenticationScheme.AzureActiveDirectory:
                    // Use Username / Password authentication
                    purchasingAgentUserContextManager = new AADUserContextManager(
                        ClientServiceUrl,
                        Settings.Default.Tenant,
                        null,
                        PurchasingAgentRoleCenterId,
                        Username,
                        Password,
                        Settings.Default.Authority,
                        Settings.Default.Resource,
                        Settings.Default.ClientId,
                        Settings.Default.ClientSecret);
                    break;
                default:
                    throw new Exception("Unknown authentication scheme");
            }
            return purchasingAgentUserContextManager;
        }

        public string Password
        {
            get
            {
                return Settings.Default.Password;
            }
        }

        public string Username
        {
            get
            {
                return Settings.Default.UserName;
            }
        }

        public string ClientServiceUrl
        {
            get
            {
                return Settings.Default.ClientServiceUrl;
            }
        }

        public AuthenticationScheme Authentication
        {
            get
            {
                return Settings.Default.Authentication;
            }
        }

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            if (ServicePointManager.ServerCertificateValidationCallback == null)
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (
                Object obj, X509Certificate certificate, X509Chain chain,
                SslPolicyErrors errors)
                {
                    return true;
                };
            }
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            if (purchasingAgentUserContextManager != null)
            {
                purchasingAgentUserContextManager.CloseAllSessions();
            }
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [TestMethod]
        public void OpenPurchaseOrderList()
        {
            // Open Page "Purchase Order List" which contains a list of all Purchase orders
            TestScenario.Run(PurchasingAgentUserContextManager, TestContext,
                userContext => TestScenario.RunPageAction(TestContext, userContext, PurchaseOrderListPageId));
        }

        [TestMethod]
        public void OpenVendorList()
        {
            // Open Vendors
            TestScenario.Run(PurchasingAgentUserContextManager, TestContext,
                userContext => TestScenario.RunPageAction(TestContext, userContext, VendorListPageId));
        }

        [TestMethod]
        public void OpenItemList()
        {
            // Open Vendors
            TestScenario.Run(PurchasingAgentUserContextManager, TestContext,
                userContext => TestScenario.RunPageAction(TestContext, userContext, ItemListPageId));
        }

        [TestMethod]
        public void LookupRandomVendor()
        {
            TestScenario.Run(PurchasingAgentUserContextManager, TestContext,
                userContext =>
                {
                    string custNo = TestScenario.SelectRandomRecordFromListPage(this.TestContext, VendorListPageId, userContext, "No.");
                    Assert.IsNotNull(custNo, "No Vendor selected");
                });
        }

        [TestMethod]
        public void CreateAndPostPurchaseOrder()
        {
            TestScenario.Run(PurchasingAgentUserContextManager, TestContext, RunCreateAndPostPurchaseOrder);
        }

        public void RunCreateAndPostPurchaseOrder(UserContext userContext)
        {
            // select a random Vendor
            var vendorNo = TestScenario.SelectRandomRecordFromListPage(TestContext, VendorListPageId, userContext, "No.");

            // Invoke using the new Purchase order action on Role Center
            var newPurchaseOrderPage = userContext.EnsurePage(PurchaseOrderPageId, userContext.RoleCenterPage.Action("Purchase Order").InvokeCatchForm());

            // Start in the No. field
            newPurchaseOrderPage.Control("No.").Activate();

            // Navigate to Vendor field in order to create record
            newPurchaseOrderPage.Control("Vendor No.").Activate();

            var newPurchaseOrderNo = newPurchaseOrderPage.Control("No.").StringValue;
            TestContext.WriteLine("Created Purchase Order No. {0}", newPurchaseOrderNo);

            // Set Vendor to a Random Vendor and ignore any credit warning
            TestScenario.SaveValueAndIgnoreWarning(TestContext, userContext, newPurchaseOrderPage.Control("Vendor No."), vendorNo);

            TestScenario.SaveValueWithDelay(newPurchaseOrderPage.Control("Vendor Invoice No."), vendorNo);

            userContext.ValidateForm(newPurchaseOrderPage);

            // Add a random number of lines between 2 and 5
            int noOfLines = SafeRandom.GetRandomNext(2, 6);
            for (int line = 0; line < noOfLines; line++)
            {
                AddPurchaseOrderLine(userContext, newPurchaseOrderPage, line);
            }

            // Check Validation errors
            userContext.ValidateForm(newPurchaseOrderPage);

            PostPurchaseOrder(userContext, newPurchaseOrderPage);

            // Close the page
            TestScenario.ClosePage(TestContext, userContext, newPurchaseOrderPage);
        }

        private void PostPurchaseOrder(UserContext userContext, ClientLogicalForm newPurchaseOrderPage)
        {
            ClientLogicalForm postConfirmationDialog;
            using (new TestTransaction(TestContext, "Post"))
            {
                postConfirmationDialog = newPurchaseOrderPage.Action("Post...").InvokeCatchDialog();
            }

            if (postConfirmationDialog == null)
            {
                userContext.ValidateForm(newPurchaseOrderPage);
                Assert.Inconclusive("Post dialog can't be found");
            }

            using (new TestTransaction(TestContext, "ConfirmReceiptAndInvoice"))
            {
                ClientLogicalForm dialog = userContext.CatchDialog(postConfirmationDialog.Action("OK").Invoke);
                if (dialog != null)
                {
                    // The order has been posted and moved to the posted invoices tab, do you want to open...
                    dialog.Action("No").Invoke();
                }
            }
        }

        private void AddPurchaseOrderLine(UserContext userContext, ClientLogicalForm newPurchaseOrderPage, int line)
        {
            // Get Line
            var itemsLine = newPurchaseOrderPage.Repeater().DefaultViewport[line];

            // Activate Type field
            itemsLine.Control("Type").Activate();

            // set Type = Item
            TestScenario.SaveValueWithDelay(itemsLine.Control("Type"), "Item");

            // Set Item No.
            var itemNo = TestScenario.SelectRandomRecordFromListPage(TestContext, ItemListPageId, userContext, "No.");
            TestScenario.SaveValueWithDelay(itemsLine.Control("No."), itemNo);

            string qtyToOrder = SafeRandom.GetRandomNext(1, 10).ToString(CultureInfo.InvariantCulture);

            TestScenario.SaveValueAndIgnoreWarning(TestContext, userContext, itemsLine.Control("Quantity"), qtyToOrder);

            TestScenario.SaveValueAndIgnoreWarning(TestContext, userContext, itemsLine.Control("Qty. to Receive"), qtyToOrder, "OK");

            // Look at the line for 1 seconds.
            DelayTiming.SleepDelay(DelayTiming.ThinkDelay);
        }
    }
}
