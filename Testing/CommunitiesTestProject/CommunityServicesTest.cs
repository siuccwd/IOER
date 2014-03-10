using Isle.BizServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ILPathways.Business;
using System.Collections.Generic;

namespace CommunitiesTestProject
{
    
    
    /// <summary>
    ///This is a test class for CommunityServicesTest and is intended
    ///to contain all CommunityServicesTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CommunityServicesTest
    {

        int lastCommunity = 0;
        int lastPostingId = 0;

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
        ///A test for CommunityServices Constructor
        ///</summary>
        [TestMethod()]
        public void CommunityServicesConstructorTest()
        {
            CommunityServices target = new CommunityServices();
        }

        [TestMethod()]
        public void CommunityTestLIst()
        {
            Community_GetTest();

            Posting_GetTest();

            PostingDocumentAddTest();

        }


        /// <summary>
        ///A test for Community_Get
        ///</summary>
        [TestMethod()]
        public void Community_GetTest()
        {
            int communityId = 1; 
            Community expected = new Community();
            expected.Id = 1;
            Community actual;
            actual = CommunityServices.Community_Get( communityId );
            Assert.AreEqual( expected.Id, actual.Id );
        }

        /// <summary>
        ///A test for Community_SelectAll
        ///</summary>
        [TestMethod()]
        public void Community_SelectAllTest()
        {
            List<Community> expected = new List<Community>();
            expected.Add( new Community() { Id=1, Title = "one", Description = "test"});
            List<Community> actual;
            actual =  new CommunityServices().Community_SelectAll();
            Assert.AreNotEqual( expected.Count, actual.Count );
        }

        /// <summary>
        ///A test for PostingAdd
        ///</summary>
        [TestMethod()]
        public void PostingAddTest()
        {
            int pCommunityId = 1;
            string comment = "unit test post";
            int pCreatedById = 2;
            int expected = 0; 
            int actual;
            actual = CommunityServices.PostingAdd( pCommunityId, comment, pCreatedById );
            lastPostingId = actual;
            Assert.AreNotEqual( expected, actual );
        }

        /// <summary>
        ///A test for PostingAdd
        ///</summary>
        [TestMethod()]
        public void PostingAddTest1()
        {
            int pCommunityId = 1;
            string comment = "post response to a post";
            int pCreatedById = 2;
            int relatedPostingId = 1;
            int expected = 0; 
            int actual;
            actual = CommunityServices.PostingAdd( pCommunityId, comment, pCreatedById, relatedPostingId );
            lastPostingId = actual;
            Assert.AreNotEqual( expected, actual );
        }

        /// <summary>
        ///A test for Posting_Get
        ///</summary>
        [TestMethod()]
        public void Posting_GetTest()
        {
            int id = 2;
            CommunityPosting expected = new CommunityPosting();
            expected.Id = 2;
            CommunityPosting actual;
            actual = CommunityServices.Posting_Get( id );
            Assert.AreEqual( expected.Id, actual.Id );
        }

        /// <summary>
        ///A test for Posting_Delete
        ///</summary>
        [TestMethod()]
        public void Posting_DeleteTest()
        {
            int id = 4;
            bool expected = true;
            bool actual;
            actual = CommunityServices.Posting_Delete( id );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///A test for PostingDocumentAdd
        ///</summary>
        [TestMethod()]
        public void PostingDocumentAddTest()
        {
            int pPostingId = 2;
            if ( lastPostingId > 0 )
                pPostingId = lastPostingId;

            Guid docId = new Guid( "A5823EA6-F3D9-4121-AE42-0178C817D8E7" ); 
            int pCreatedById = 2; 
            int expected = 0; 
            int actual;
            actual = CommunityServices.PostingDocumentAdd( pPostingId, docId, pCreatedById );
            Assert.AreNotEqual( expected, actual );
        }

        /// <summary>
        ///A test for PostingSearch
        ///</summary>
        [TestMethod()]
        public void PostingSearchTest()
        {
            int pCommunityId = 1; // 
            int selectedPageNbr = 1; 
            int pageSize = 50; 
            int pTotalRows = 0; 
            int pTotalRowsExpected = 0; 
            List<CommunityPosting> expected = new List<CommunityPosting>();
            List<CommunityPosting> actual;
            actual = CommunityServices.PostingSearch( pCommunityId, selectedPageNbr, pageSize, ref pTotalRows );
            Assert.AreNotEqual( pTotalRowsExpected, pTotalRows );
            Assert.AreNotEqual( expected, actual );
        }


    }
}
