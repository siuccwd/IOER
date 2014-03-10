using Isle.BizServices.workNetAccountServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Data;

namespace ActivityServicesUnitTests
{
    
    
    /// <summary>
    ///This is a test class for AccountSoapClientTest and is intended
    ///to contain all AccountSoapClientTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AccountSoapClientTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for AccountSoapClient Constructor
        ///</summary>
        [TestMethod()]
        public void AccountSoapClientConstructorTest()
        {
            AccountSoapClient target = new AccountSoapClient();
            Assert.Inconclusive( "TODO: Implement code to verify target" );
        }

        /// <summary>
        ///A test for AccountSoapClient Constructor
        ///</summary>
        [TestMethod()]
        public void AccountSoapClientConstructorTest1()
        {
            string endpointConfigurationName = string.Empty; // TODO: Initialize to an appropriate value
            AccountSoapClient target = new AccountSoapClient( endpointConfigurationName );
            Assert.Inconclusive( "TODO: Implement code to verify target" );
        }

        /// <summary>
        ///A test for AccountSoapClient Constructor
        ///</summary>
        [TestMethod()]
        public void AccountSoapClientConstructorTest2()
        {
            string endpointConfigurationName = string.Empty; // TODO: Initialize to an appropriate value
            string remoteAddress = string.Empty; // TODO: Initialize to an appropriate value
            AccountSoapClient target = new AccountSoapClient( endpointConfigurationName, remoteAddress );
            Assert.Inconclusive( "TODO: Implement code to verify target" );
        }

        /// <summary>
        ///A test for AccountSoapClient Constructor
        ///</summary>
        [TestMethod()]
        public void AccountSoapClientConstructorTest3()
        {
            string endpointConfigurationName = string.Empty; // TODO: Initialize to an appropriate value
            EndpointAddress remoteAddress = null; // TODO: Initialize to an appropriate value
            AccountSoapClient target = new AccountSoapClient( endpointConfigurationName, remoteAddress );
            Assert.Inconclusive( "TODO: Implement code to verify target" );
        }

        /// <summary>
        ///A test for AccountSoapClient Constructor
        ///</summary>
        [TestMethod()]
        public void AccountSoapClientConstructorTest4()
        {
            Binding binding = null; // TODO: Initialize to an appropriate value
            EndpointAddress remoteAddress = null; // TODO: Initialize to an appropriate value
            AccountSoapClient target = new AccountSoapClient( binding, remoteAddress );
            Assert.Inconclusive( "TODO: Implement code to verify target" );
        }

        /// <summary>
        ///A test for GetCustomerByRowId
        ///</summary>
        [TestMethod()]
        public void GetCustomerByRowIdTest()
        {
            AccountSoapClient target = new AccountSoapClient(); // TODO: Initialize to an appropriate value
            string serviceCode = string.Empty; // TODO: Initialize to an appropriate value
            string applicationPassword = string.Empty; // TODO: Initialize to an appropriate value
            string rowId = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            DataSet expected = null; // TODO: Initialize to an appropriate value
            DataSet actual;
            actual = target.GetCustomerByRowId( serviceCode, applicationPassword, rowId, password );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for JobClubRegister
        ///</summary>
        [TestMethod()]
        public void JobClubRegisterTest()
        {
            AccountSoapClient target = new AccountSoapClient(); // TODO: Initialize to an appropriate value
            string email = string.Empty; // TODO: Initialize to an appropriate value
            string firstName = string.Empty; // TODO: Initialize to an appropriate value
            string lastName = string.Empty; // TODO: Initialize to an appropriate value
            string employmentStatus = string.Empty; // TODO: Initialize to an appropriate value
            string applicationPassword = string.Empty; // TODO: Initialize to an appropriate value
            AccountDetail expected = null; // TODO: Initialize to an appropriate value
            AccountDetail actual;
            actual = target.JobClubRegister( email, firstName, lastName, employmentStatus, applicationPassword );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for Login
        ///</summary>
        [TestMethod()]
        public void LoginTest()
        {
            AccountSoapClient target = new AccountSoapClient(); // TODO: Initialize to an appropriate value
            string serviceCode = string.Empty; // TODO: Initialize to an appropriate value
            string applicationPassword = string.Empty; // TODO: Initialize to an appropriate value
            string loginName = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            AccountDetail expected = null; // TODO: Initialize to an appropriate value
            AccountDetail actual;
            actual = target.Login( serviceCode, applicationPassword, loginName, password );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for NewsSubscriptionCreate
        ///</summary>
        [TestMethod()]
        public void NewsSubscriptionCreateTest()
        {
            AccountSoapClient target = new AccountSoapClient(); // TODO: Initialize to an appropriate value
            string email = string.Empty; // TODO: Initialize to an appropriate value
            string newsCategory = string.Empty; // TODO: Initialize to an appropriate value
            int Frequency = 0; // TODO: Initialize to an appropriate value
            bool isValidated = false; // TODO: Initialize to an appropriate value
            string applicationPassword = string.Empty; // TODO: Initialize to an appropriate value
            DataSet expected = null; // TODO: Initialize to an appropriate value
            DataSet actual;
            actual = target.NewsSubscriptionCreate( email, newsCategory, Frequency, isValidated, applicationPassword );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for NewsSubscriptionDelete
        ///</summary>
        [TestMethod()]
        public void NewsSubscriptionDeleteTest()
        {
            AccountSoapClient target = new AccountSoapClient(); // TODO: Initialize to an appropriate value
            string email = string.Empty; // TODO: Initialize to an appropriate value
            string newsCategory = string.Empty; // TODO: Initialize to an appropriate value
            string applicationPassword = string.Empty; // TODO: Initialize to an appropriate value
            DataSet expected = null; // TODO: Initialize to an appropriate value
            DataSet actual;
            actual = target.NewsSubscriptionDelete( email, newsCategory, applicationPassword );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for NewsSubscriptionGet
        ///</summary>
        [TestMethod()]
        public void NewsSubscriptionGetTest()
        {
            AccountSoapClient target = new AccountSoapClient(); // TODO: Initialize to an appropriate value
            string email = string.Empty; // TODO: Initialize to an appropriate value
            string newsCategory = string.Empty; // TODO: Initialize to an appropriate value
            string applicationPassword = string.Empty; // TODO: Initialize to an appropriate value
            DataSet expected = null; // TODO: Initialize to an appropriate value
            DataSet actual;
            actual = target.NewsSubscriptionGet( email, newsCategory, applicationPassword );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for NewsSubscriptionUpdate
        ///</summary>
        [TestMethod()]
        public void NewsSubscriptionUpdateTest()
        {
            AccountSoapClient target = new AccountSoapClient(); // TODO: Initialize to an appropriate value
            string email = string.Empty; // TODO: Initialize to an appropriate value
            string newsCategory = string.Empty; // TODO: Initialize to an appropriate value
            int Frequency = 0; // TODO: Initialize to an appropriate value
            bool isValidated = false; // TODO: Initialize to an appropriate value
            string applicationPassword = string.Empty; // TODO: Initialize to an appropriate value
            DataSet expected = null; // TODO: Initialize to an appropriate value
            DataSet actual;
            actual = target.NewsSubscriptionUpdate( email, newsCategory, Frequency, isValidated, applicationPassword );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for NewsletterSubscriptionCreate
        ///</summary>
        [TestMethod()]
        public void NewsletterSubscriptionCreateTest()
        {
            AccountSoapClient target = new AccountSoapClient(); // TODO: Initialize to an appropriate value
            int userId = 0; // TODO: Initialize to an appropriate value
            int newsLetterId = 0; // TODO: Initialize to an appropriate value
            bool isValidated = false; // TODO: Initialize to an appropriate value
            string applicationPassword = string.Empty; // TODO: Initialize to an appropriate value
            DataSet expected = null; // TODO: Initialize to an appropriate value
            DataSet actual;
            actual = target.NewsletterSubscriptionCreate( userId, newsLetterId, isValidated, applicationPassword );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for NewsletterSubscriptionGet
        ///</summary>
        [TestMethod()]
        public void NewsletterSubscriptionGetTest()
        {
            AccountSoapClient target = new AccountSoapClient(); // TODO: Initialize to an appropriate value
            int userId = 0; // TODO: Initialize to an appropriate value
            int newsLetterId = 0; // TODO: Initialize to an appropriate value
            string applicationPassword = string.Empty; // TODO: Initialize to an appropriate value
            DataSet expected = null; // TODO: Initialize to an appropriate value
            DataSet actual;
            actual = target.NewsletterSubscriptionGet( userId, newsLetterId, applicationPassword );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for NewsletterSubscriptionUpdate
        ///</summary>
        [TestMethod()]
        public void NewsletterSubscriptionUpdateTest()
        {
            AccountSoapClient target = new AccountSoapClient(); // TODO: Initialize to an appropriate value
            int userId = 0; // TODO: Initialize to an appropriate value
            int newsLetterId = 0; // TODO: Initialize to an appropriate value
            int Frequency = 0; // TODO: Initialize to an appropriate value
            bool isValidated = false; // TODO: Initialize to an appropriate value
            string applicationPassword = string.Empty; // TODO: Initialize to an appropriate value
            DataSet expected = null; // TODO: Initialize to an appropriate value
            DataSet actual;
            actual = target.NewsletterSubscriptionUpdate( userId, newsLetterId, Frequency, isValidated, applicationPassword );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for Register
        ///</summary>
        [TestMethod()]
        public void RegisterTest()
        {
            AccountSoapClient target = new AccountSoapClient(); // TODO: Initialize to an appropriate value
            string serviceCode = string.Empty; // TODO: Initialize to an appropriate value
            string applicationPassword = string.Empty; // TODO: Initialize to an appropriate value
            string email = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            string firstName = string.Empty; // TODO: Initialize to an appropriate value
            string lastName = string.Empty; // TODO: Initialize to an appropriate value
            string accountType = string.Empty; // TODO: Initialize to an appropriate value
            AccountDetail expected = null; // TODO: Initialize to an appropriate value
            AccountDetail actual;
            actual = target.Register( serviceCode, applicationPassword, email, password, firstName, lastName, accountType );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }
    }
}
