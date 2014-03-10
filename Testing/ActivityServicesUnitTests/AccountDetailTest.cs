using Isle.BizServices.workNetAccountServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ActivityServicesUnitTests
{
    
    
    /// <summary>
    ///This is a test class for AccountDetailTest and is intended
    ///to contain all AccountDetailTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AccountDetailTest
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
        ///A test for AccountDetail Constructor
        ///</summary>
        [TestMethod()]
        public void AccountDetailConstructorTest()
        {
            AccountDetail target = new AccountDetail();
            Assert.Inconclusive( "TODO: Implement code to verify target" );
        }

        /// <summary>
        ///A test for email
        ///</summary>
        [TestMethod()]
        public void emailTest()
        {
            AccountDetail target = new AccountDetail(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.email = expected;
            actual = target.email;
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for firstName
        ///</summary>
        [TestMethod()]
        public void firstNameTest()
        {
            AccountDetail target = new AccountDetail(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.firstName = expected;
            actual = target.firstName;
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for fullName
        ///</summary>
        [TestMethod()]
        public void fullNameTest()
        {
            AccountDetail target = new AccountDetail(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.fullName = expected;
            actual = target.fullName;
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for lastName
        ///</summary>
        [TestMethod()]
        public void lastNameTest()
        {
            AccountDetail target = new AccountDetail(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.lastName = expected;
            actual = target.lastName;
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for rowId
        ///</summary>
        [TestMethod()]
        public void rowIdTest()
        {
            AccountDetail target = new AccountDetail(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.rowId = expected;
            actual = target.rowId;
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for statusMessage
        ///</summary>
        [TestMethod()]
        public void statusMessageTest()
        {
            AccountDetail target = new AccountDetail(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.statusMessage = expected;
            actual = target.statusMessage;
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for userName
        ///</summary>
        [TestMethod()]
        public void userNameTest()
        {
            AccountDetail target = new AccountDetail(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.userName = expected;
            actual = target.userName;
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for worknetId
        ///</summary>
        [TestMethod()]
        public void worknetIdTest()
        {
            AccountDetail target = new AccountDetail(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.worknetId = expected;
            actual = target.worknetId;
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }
    }
}
