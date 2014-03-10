using Isle.BizServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using ILPathways.Business;
using System.Collections.Generic;

namespace ContentServicesUnitTests
{
    
    
    /// <summary>
    ///This is a test class for ContentServicesTest and is intended
    ///to contain all ContentServicesTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ContentServicesTest
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
        ///A test for ContentPrivilegeCodes_Select
        ///</summary>
        [TestMethod()]
        public void ContentPrivilegeCodes_SelectTest()
        {
            ContentServices target = new ContentServices(); // TODO: Initialize to an appropriate value
            DataSet expected = null; // TODO: Initialize to an appropriate value
            DataSet actual;
            actual = target.ContentPrivilegeCodes_Select();
            Assert.IsNotNull( actual );
            
            //Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for ContentReferencesSelectList
        ///</summary>
        [TestMethod()]
        public void ContentReferencesSelectListTest()
        {
            ContentServices target = new ContentServices(); // TODO: Initialize to an appropriate value
            int parentId = 0; // TODO: Initialize to an appropriate value
            List<ContentReference> expected = null; // TODO: Initialize to an appropriate value
            List<ContentReference> actual;
            actual = target.ContentReferencesSelectList( parentId );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for ContentSupplementGet
        ///</summary>
        [TestMethod()]
        public void ContentSupplementGetTest()
        {
            ContentServices target = new ContentServices(); // TODO: Initialize to an appropriate value
            int id = 0; // TODO: Initialize to an appropriate value
            ContentSupplement expected = null; // TODO: Initialize to an appropriate value
            ContentSupplement actual;
            actual = target.ContentSupplementGet( id );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for ContentSupplementsSelectList
        ///</summary>
        [TestMethod()]
        public void ContentSupplementsSelectListTest()
        {
            ContentServices target = new ContentServices(); // TODO: Initialize to an appropriate value
            int parentId = 0; // TODO: Initialize to an appropriate value
            List<ContentSupplement> expected = null; // TODO: Initialize to an appropriate value
            List<ContentSupplement> actual;
            actual = target.ContentSupplementsSelectList( parentId );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for Create
        ///</summary>
        [TestMethod()]
        public void CreateTest()
        {
            ContentServices target = new ContentServices(); // TODO: Initialize to an appropriate value
            ContentItem entity = null; // TODO: Initialize to an appropriate value
            string statusMessage = string.Empty; // TODO: Initialize to an appropriate value
            string statusMessageExpected = string.Empty; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.Create( entity, ref statusMessage );
            Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for DoQuery
        ///</summary>
        [TestMethod()]
        public void DoQueryTest()
        {
            string sql = string.Empty; // TODO: Initialize to an appropriate value
            DataSet expected = null; // TODO: Initialize to an appropriate value
            DataSet actual;
            actual = ContentServices.DoQuery( sql );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for SelectOrgTemplates
        ///</summary>
        [TestMethod()]
        public void SelectOrgTemplatesTest()
        {
            ContentServices target = new ContentServices(); // TODO: Initialize to an appropriate value
            int orgId = 0; // TODO: Initialize to an appropriate value
            DataSet expected = null; // TODO: Initialize to an appropriate value
            DataSet actual;
            actual = target.SelectOrgTemplates( orgId );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }

        /// <summary>
        ///A test for Update
        ///</summary>
        [TestMethod()]
        public void UpdateTest()
        {
            ContentServices target = new ContentServices(); // TODO: Initialize to an appropriate value
            ContentItem entity = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Update( entity );
            Assert.AreEqual( expected, actual );
            Assert.Inconclusive( "Verify the correctness of this test method." );
        }
    }
}
