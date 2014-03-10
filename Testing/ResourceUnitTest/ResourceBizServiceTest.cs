using Isle.BizServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using LRWarehouse.Business;
using System.Collections.Generic;

namespace ResourceUnitTest
{
    
    
    /// <summary>
    ///This is a test class for ResourceBizServiceTest and is intended
    ///to contain all ResourceBizServiceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ResourceBizServiceTest
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
        ///A test for DoesUrlExist
        ///</summary>
        [TestMethod()]
        public void DoesUrlExistTest()
        {
            string url = "http://hereisathing.com";
            bool expected = true;
            bool actual;
            actual = ResourceBizService.DoesUrlExist( url );
            Assert.AreEqual( expected, actual );
        }

        [TestMethod()]
        public void DoesUrlExistTest_False()
        {
            string url = "http://hereisathing.com/NotFound";
            bool expected = false;
            bool actual;
            actual = ResourceBizService.DoesUrlExist( url );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///A test for ResourceVersion_GetByUrl
        ///</summary>
        [TestMethod()]
        public void ResourceVersion_GetByUrlTest()
        {
            string url = "http://hereisathing.com";
            ResourceVersion rv = new ResourceVersion();
            rv.Id = 455894;
            List<ResourceVersion> expected = new List<ResourceVersion>();
            expected.Add( rv );
            List<ResourceVersion> actual;
            actual = ResourceBizService.ResourceVersion_GetByUrl( url );
            Assert.AreEqual( expected[0].Id, actual[0].Id );
            
        }
    }
}
