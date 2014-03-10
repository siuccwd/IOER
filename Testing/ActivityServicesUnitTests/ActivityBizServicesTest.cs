using Isle.BizServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ILPathways.Business;
using System.Collections.Generic;

namespace ActivityServicesUnitTests
{
    
    
    /// <summary>
    ///This is a test class for ActivityBizServicesTest and is intended
    ///to contain all ActivityBizServicesTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ActivityBizServicesTest
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
        ///A test for UserAddsLibraryComment
        ///</summary>
        [TestMethod()]
        public void UserAddsLibraryCommentTest()
        {
            ActivityBizServices target = new ActivityBizServices(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.UserAddsLibraryComment(2,1);
            Assert.AreNotEqual( expected, actual );
            //Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for ObjectActivity_RecentList
        ///</summary>
        [TestMethod()]
        public void ObjectActivity_RecentListTest()
        {
            int forDays = 30; 
            int pMaximumRows = 100; 
            List<ObjectActivity> expected = new List<ObjectActivity>(); 
            expected.Add( new ObjectActivity() { Actor="Person", ActorId=2, ActorTypeId=2, Activity="Add something", ObjectType="Object"});

            List<ObjectActivity> actual;
            actual = ActivityBizServices.ObjectActivity_RecentList( forDays, pMaximumRows );
            Assert.AreNotEqual( expected.Count, actual.Count );
            
        }

        /// <summary>
        ///A test for AddActivity
        ///</summary>
        [TestMethod()]
        public void AddActivityTest()
        {
            ActivityBizServices target = new ActivityBizServices(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.AddActivityTest();
            Assert.AreNotEqual( expected, actual );
           // Assert.Inconclusive( "Verify the correctness of this test method." );
        }
    }
}
