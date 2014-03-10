using Isle.BizServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ILPathways.Business;
using System.Collections.Generic;

namespace LibrariesUnitTests
{
    
    
    /// <summary>
    ///This is a test class for LibraryBizServiceTest and is intended
    ///to contain all LibraryBizServiceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LibraryBizServiceTest
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

        #region Subscriptions
        /// <summary>
        ///A test for IsSubcribedtoLibrary
        ///</summary>
        [TestMethod()]
        public void IsSubcribedtoLibraryTest_True()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 2; 
            int userId = 25; 
            bool expected = false; 
            bool actual;
            actual = target.IsSubcribedtoLibrary( libraryId, userId );
            Assert.AreEqual( expected, actual );

        }
        /// <summary>
        ///A test for IsSubcribedtoLibrary
        ///</summary>
        [TestMethod()]
        public void IsSubcribedtoLibraryTest_False()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 1; 
            int userId = 40; 
            bool expected = false; 
            bool actual;
            actual = target.IsSubcribedtoLibrary( libraryId, userId );
            Assert.AreEqual( expected, actual );

        }

        [TestMethod()]
        public void LibrarySubscriptionGetTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 25; 
            int userId = 29; 
            ObjectSubscription expected = new ObjectSubscription();
            expected.Id = 7;
            ObjectSubscription actual;
            actual = target.LibrarySubscriptionGet( libraryId, userId );
            Assert.AreEqual( expected.Id, actual.Id );
            
        }
        [TestMethod()]
        public void LibrarySubscriptionGetByIdTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 25; 
            int userId = 29; 
            ObjectSubscription expected = new ObjectSubscription();
            expected.Id = 7;
            ObjectSubscription actual;
            actual = target.LibrarySubscriptionGet( libraryId, userId );
            Assert.AreEqual( expected.Id, actual.Id );

        }
        /// <summary>
        ///A test for IsSubcribedtoCollection
        ///</summary>
        [TestMethod()]
        public void IsSubcribedtoCollectionTest_False()
        {
            LibraryBizService target = new LibraryBizService(); 
            int collectionId = 2; 
            int userId = 2; 
            bool expected = false; 
            bool actual;
            actual = target.IsSubcribedtoCollection( collectionId, userId );
            Assert.AreEqual( expected, actual );
        }

        [TestMethod()]
        public void IsSubcribedtoCollectionTest_True()
        {
            LibraryBizService target = new LibraryBizService(); 
            int sectionId = 1;      
            int userId = 22;        
            bool expected = false;  
            bool actual;
            actual = target.IsSubcribedtoCollection( sectionId, userId );
            Assert.AreEqual( expected, actual );
            
        }


        /// <summary>
        ///A test for CollectionSubscriptionCreate
        ///</summary>
        [TestMethod()]
        public void CollectionSubscriptionCreateTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int collectionId = 0; 
            int userId = 0; 
            int typeId = 0; 
            string statusMessage = string.Empty; 
            string statusMessageExpected = string.Empty; 
            int expected = 0; 
            int actual;
            actual = target.CollectionSubscriptionCreate( collectionId, userId, typeId, ref statusMessage );
            Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for CollectionSubscriptionGet
        ///</summary>
        [TestMethod()]
        public void CollectionSubscriptionGetTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int collectionId = 0; 
            int userId = 0; 
            ObjectSubscription expected = null; 
            ObjectSubscription actual;
            actual = target.CollectionSubscriptionGet( collectionId, userId );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for HasLibrarySubScriptions
        ///</summary>
        [TestMethod()]
        public void HasLibrarySubScriptionsTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int userId = 1; 
            bool expected = true; 
            bool actual;
            actual = target.HasLibrarySubScriptions( userId );
            Assert.AreEqual( expected, actual );

        }


        #endregion

        #region Filter codes =========================
        /// <summary>
        ///A test for LibraryCommentCreate
        ///</summary>
        [TestMethod()]
        public void AvailableFiltersForLibraryTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 1; 
            string pCodeTableSuffix = "GroupType"; 
            int expected = 6; 
            List<DataItem> actual = target.AvailableFiltersForLibrary( libraryId, pCodeTableSuffix );

            Assert.AreEqual( expected, actual.Count );
            //===========================================================

            libraryId = 1;
            pCodeTableSuffix = "AccessRights";
            expected = 2;
            actual = target.AvailableFiltersForLibrary( libraryId, pCodeTableSuffix );
            Assert.AreEqual( expected, actual.Count );
        }

        [TestMethod()]
        public void AvailableFiltersForCollectionTest()
        {
            LibraryBizService target = new LibraryBizService();
            int sectionId = 96;
            string pCodeTableSuffix = "GroupType";
            int expected = 6; 
            List<DataItem> actual = target.AvailableFiltersForCollection( sectionId, pCodeTableSuffix );
            Assert.AreEqual( expected, actual.Count );
            //===========================================================

            sectionId = 1;
            pCodeTableSuffix = "CareerCluster";
            expected = 3;
            actual = target.AvailableFiltersForCollection( sectionId, pCodeTableSuffix );
            Assert.AreEqual( expected, actual.Count );
            //===========================================================

            sectionId = 96;
            pCodeTableSuffix = "EducationalUse";
            expected = 2;
            actual = target.AvailableFiltersForCollection( sectionId, pCodeTableSuffix );
            Assert.AreEqual( expected, actual.Count );
        }

        #endregion

        #region Likes ==========================
        /// <summary>
        ///A test for Library_LikeSummary
        ///</summary>
        [TestMethod()]
        public void Library_LikeSummaryTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 1; 
            List<DataItem> expected = new List<DataItem>();
            expected.Add( new DataItem() { Id = 1, CreatedById = 2, ParentId = 1, Created = System.DateTime.Now } );
            List<DataItem> actual;
            actual = target.Library_LikeSummary( libraryId );
            Assert.AreNotEqual( expected.Count, actual.Count );

        }
        /// <summary>
        ///A test for LibraryLikeCreateTest - true
        ///</summary>
        [TestMethod()]
        public void LibraryLikeCreateTest1()
        {
            LibraryBizService target = new LibraryBizService(); 
            int sectionId = 2; 
            bool IsLike = false; 
            int createdById = 22; 
            int expected = 0; 
            int actual;
            actual = target.LibrarySectionLikeCreate( sectionId, IsLike, createdById );
            Assert.AreNotEqual( expected, actual );

        }
        /// <summary>
        ///A test for LibraryLikeCreateTest - false
        ///</summary>
        [TestMethod()]
        public void LibraryLikeCreateTestFalse()
        {
            LibraryBizService target = new LibraryBizService(); 
            int sectionId = 2; 
            bool IsLike = true; 
            int createdById = 23; 
            int expected = 0; 
            int actual;
            actual = target.LibrarySectionLikeCreate( sectionId, IsLike, createdById );
            Assert.AreNotEqual( expected, actual );
        }
        [TestMethod()]
        public void Library_GetLikeTestTrue()
        {
            LibraryBizService target = new LibraryBizService();
            int id = 1;
            int userId = 22;
            ObjectLike expected = new ObjectLike();
            expected.HasLikeEntry = true;
            ObjectLike actual;
            actual = target.Library_GetLike( id, userId );
            Assert.AreEqual( expected.HasLikeEntry, actual.HasLikeEntry );

        }

        /// <summary>
        ///A test for LibrarySectionLikeCreate
        ///</summary>
        [TestMethod()]
        public void LibrarySectionLikeCreateTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int sectionId = 2; 
            bool IsLike = false; 
            int createdById = 22; 
            int expected = 0; 
            int actual;
            actual = target.LibrarySectionLikeCreate( sectionId, IsLike, createdById );
            Assert.AreNotEqual( expected, actual );

        }
        /// <summary>
        ///A test for LibrarySectionLikeCreate
        ///</summary>
        [TestMethod()]
        public void LibrarySectionDislikeCreateTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int sectionId = 2; 
            bool IsLike = true; 
            int createdById = 23; 
            int expected = 0; 
            int actual;
            actual = target.LibrarySectionLikeCreate( sectionId, IsLike, createdById );
            Assert.AreNotEqual( expected, actual );

        }

        /// <summary>
        ///A test for LibrarySection_GetLike
        ///</summary>
        [TestMethod()]
        public void LibrarySection_GetLikeTestTrue()
        {
            LibraryBizService target = new LibraryBizService(); 
            int collectionId = 2; 
            int userId = 23;
            ObjectLike expected = new ObjectLike();
            expected.HasLikeEntry = true;
            ObjectLike actual;
            actual = target.LibrarySection_GetLike( collectionId, userId );
            Assert.AreEqual( expected.HasLikeEntry, actual.HasLikeEntry );

        }

        public void LibrarySection_GetLikeTestFalse()
        {
            LibraryBizService target = new LibraryBizService(); 
            int collectionId = 2; 
            int userId = 99;
            ObjectLike expected = new ObjectLike();
            expected.HasLikeEntry = false;
            ObjectLike actual;
            actual = target.LibrarySection_GetLike( collectionId, userId );
            Assert.AreEqual( expected.HasLikeEntry, actual.HasLikeEntry );
        }
        #endregion

        #region Library =========================

        /// <summary>
        ///A test for Get
        ///</summary>
        [TestMethod()]
        public void GetLibraryTest()
        {
            LibraryBizService target = new LibraryBizService();
            int pId = 1; 
            Library expected = new Library();
            expected.Id = 1;
            Library actual;
            actual = target.Get( pId );
            Assert.AreEqual( expected.Id, actual.Id );

        }

        /// <summary>
        ///A test for IsLibraryEmpty
        ///</summary>
        [TestMethod()]
        public void IsLibraryEmptyTest_False()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 1; 
            bool expected = false; 
            bool actual;
            actual = target.IsLibraryEmpty( libraryId );
            Assert.AreEqual( expected, actual );

        }
        [TestMethod()]
        public void IsLibraryEmptyTest_True()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 0; 
            bool expected = false; 
            bool actual;
            actual = target.IsLibraryEmpty( libraryId );
            Assert.AreNotEqual( expected, actual );

        }

        /// <summary>
        ///A test for Library_SelectListWithEditAccess
        ///</summary>
        [TestMethod()]
        public void Library_SelectListWithEditAccessTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int userId = 0; 
            List<Library> expected = null; 
            List<Library> actual;
            actual = target.Library_SelectListWithEditAccess( userId );
            Assert.AreNotEqual( expected, actual );

        }

        #endregion

        #region Collections =========================

        /// <summary>
        ///A test for LibrarySectionCopy
        ///</summary>
        [TestMethod()]
        public void LibrarySectionCopyTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int fromSectionId = 1; 
            int toLibraryId = 22; 
            int createdById = 2;
            string statusMessage = "";
            string statusMessageExpected = "various";
            int expected = 0; 
            int actual;
            actual = target.LibrarySectionCopy( fromSectionId, toLibraryId, createdById, ref statusMessage );
            expected = actual;

            //Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreEqual( expected, actual );
            
        }

        /// <summary>
        ///A test for LibrarySection_Delete
        ///</summary>
        [TestMethod()]
        public void LibrarySection_DeleteTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int sectionId = 0; 
            string statusMessage = string.Empty; 
            string statusMessageExpected = string.Empty; 
            bool expected = false; 
            bool actual;
            actual = target.LibrarySection_Delete( sectionId, ref statusMessage );
            Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreEqual( expected, actual );

        }

        #endregion

        #region Library resources =========================
        /// <summary>
        ///A test for IsResourceInLibrary
        ///</summary>
        [TestMethod()]
        public void IsResourceInLibraryTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 0; 
            int resourceIntId = 0; 
            bool expected = false; 
            bool actual;
            actual = target.IsResourceInLibrary( libraryId, resourceIntId );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for LibraryResourceCreate
        ///</summary>
        [TestMethod()]
        public void LibraryResourceCreateTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int collectionId = 0; 
            int resourceIntId = 0; 
            int userId = 0; 
            string statusMessage = string.Empty; 
            string statusMessageExpected = string.Empty; 
            int expected = 0; 
            int actual;
            actual = target.LibraryResourceCreate( collectionId, resourceIntId, userId, ref statusMessage );
            Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for LibraryResourceDelete
        ///</summary>
        [TestMethod()]
        public void LibraryResourceDeleteTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int fromCollectionId = 41;
            int resourceIntId = 196145;
            string statusMessage = "successful";
            string statusMessageExpected = "successful";
            bool expected = true; 
            bool actual;
            actual = target.LibraryResourceDelete( fromCollectionId, resourceIntId, ref statusMessage );
            Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreEqual( expected, actual );
            
        }



        /// <summary>
        ///A test for ResourceCopy
        ///</summary>
        [TestMethod()]
        public void ResourceCopyTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int resourceIntId = 0; 
            int toCollectionId = 0; 
            int userId = 0; 
            string statusMessage = string.Empty; 
            string statusMessageExpected = string.Empty; 
            int expected = 0; 
            int actual;
            actual = target.ResourceCopy( resourceIntId, toCollectionId, userId, ref statusMessage );
            Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for ResourceCopyById
        ///</summary>
        [TestMethod()]
        public void ResourceCopyByIdTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryResourceId = 0; 
            int toCollectionId = 0; 
            int userId = 0; 
            string statusMessage = string.Empty; 
            string statusMessageExpected = string.Empty; 
            int expected = 0; 
            int actual;
            actual = target.ResourceCopyById( libraryResourceId, toCollectionId, userId, ref statusMessage );
            Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for ResourceMove
        ///</summary>
        [TestMethod()]
        public void ResourceMoveTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int fromCollectionId = 0; 
            int resourceIntId = 0; 
            int toCollectionId = 0; 
            int userId = 0; 
            string statusMessage = string.Empty; 
            string statusMessageExpected = string.Empty; 
            string expected = string.Empty; 
            string actual;
            actual = target.ResourceMove( fromCollectionId, resourceIntId, toCollectionId, userId, ref statusMessage );
            Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for ResourceMoveById
        ///</summary>
        [TestMethod()]
        public void ResourceMoveByIdTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryResourceId = 0; 
            int toCollectionId = 0; 
            int userId = 0; 
            string statusMessage = string.Empty; 
            string statusMessageExpected = string.Empty; 
            string expected = string.Empty; 
            string actual;
            actual = target.ResourceMoveById( libraryResourceId, toCollectionId, userId, ref statusMessage );
            Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for GetAllLibrariesWithResource
        ///</summary>
        [TestMethod()]
        public void GetAllLibrariesWithResourceTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int resourceIntId = 21632; 
            List<Library> expected = new List<Library>(); 
            int excpectedCount = 3;
            List<Library> actual;
            actual = target.GetAllLibrariesWithResource( resourceIntId );
            Assert.AreEqual( excpectedCount, actual.Count );

            //==============================================
            resourceIntId = 21632;
            expected = new List<Library>();
            excpectedCount = 3;
            
            actual = target.GetAllLibrariesWithResource( resourceIntId );
            Assert.AreEqual( excpectedCount, actual.Count );
           
        }

        [TestMethod()]
        public void LibraryResource_UpdateElasticSearchForAllLibrariesAndSectionsForResource()
        {
            LibraryBizService target = new LibraryBizService(); 
            int resourceIntId = 87451; // in 3 libraries, 3 collections
            
            bool expected = true;

            bool actual = target.LibraryResource_UpdateElasticSearchForAllLibrariesAndSectionsForResource( resourceIntId );
            Assert.AreEqual( expected, actual );

            resourceIntId = 62959; // in 1 lib, 4 collections
            actual = target.LibraryResource_UpdateElasticSearchForAllLibrariesAndSectionsForResource( resourceIntId );
            Assert.AreEqual( expected, actual );
        }


        #endregion


        #region         Library Members
        [TestMethod()]
        public void RunTests()
        {
            Library_GetLikeTestTrue();
            LibrarySection_GetLikeTestTrue();
            LibrarySection_GetLikeTestFalse();

           // IsLibraryMemberTest();

           // IsLibraryMemberTest_False();

           // //LibraryMember_CreateTest();

           //// LibraryMember_UpdateTest();

           // LibraryMember_GetTest();

           // LibraryInvitation_CreateTest();
        }
        /// <summary>
        ///A test for IsLibraryMember
        ///</summary>
        [TestMethod()]
        public void IsLibraryMemberTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 2; 
            int userId = 2; 
            bool expected = true; 
            bool actual;
            actual = target.IsLibraryMember( libraryId, userId );
            Assert.AreEqual( expected, actual );

        }
        [TestMethod()]
        public void IsLibraryMemberTest_False()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 1; 
            int userId = 22; 
            bool expected = false; 
            bool actual;
            actual = target.IsLibraryMember( libraryId, userId );
            Assert.AreEqual( expected, actual );
        }


        /// <summary>
        ///A test for LibraryMember_Create
        ///</summary>
        [TestMethod()]
        public void LibraryMember_CreateTest()
        {
            LibraryBizService target = new LibraryBizService();
            int libraryId = 51;
            int userId = 2;
            int memberTypeId = 1;
            int createdById = 2;
            string statusMessage = "";
            string statusMessageExpected = "Successful";
            int expected = 41; //+
            int actual;
            actual = target.LibraryMember_Create( libraryId, userId, memberTypeId, createdById, ref statusMessage );
            Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreNotSame( expected, actual );

        }

        /// <summary>
        ///A test for LibraryMember_Update
        ///</summary>
        [TestMethod()]
        public void LibraryMember_UpdateTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int id = 1; 
            int memberTypeId = 3; 
            int lastUpdatedById = 22; 
            bool expected = true; 
            bool actual;
            actual = target.LibraryMember_Update( id, memberTypeId, lastUpdatedById );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for LibraryMember_Update
        ///</summary>
        [TestMethod()]
        public void LibraryMember_UpdateTest1()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 10; 
            int userId = 2; 
            int memberTypeId = 2; 
            int lastUpdatedById = 2; 
            bool expected = true; 
            bool actual;
            actual = target.LibraryMember_Update( libraryId, userId, memberTypeId, lastUpdatedById );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for LibraryMember_Get
        ///</summary>
        [TestMethod()]
        public void LibraryMember_GetTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int id = 11;
            LibraryMember expected = new LibraryMember();
            expected.Id = 11;
            LibraryMember actual;
            actual = target.LibraryMember_Get( id );
            Assert.AreEqual( expected.Id, actual.Id );
            
        }

        /// <summary>
        ///A test for LibraryMember_Get
        ///</summary>
        [TestMethod()]
        public void LibraryMember_GetTest1()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 3; 
            int userId = 2;
            LibraryMember expected = new LibraryMember();
            expected.Id = 12;

            LibraryMember actual;
            actual = target.LibraryMember_Get( libraryId, userId );
            Assert.AreEqual( expected.Id, actual.Id );
        }

        /// <summary>
        ///A test for LibraryMember_Delete
        ///</summary>
        [TestMethod()]
        public void LibraryMember_DeleteTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int id = 0; 
            bool expected = false; 
            bool actual;
            actual = target.LibraryMember_Delete( id );
            Assert.AreEqual( expected, actual );

        }

        #endregion

        #region  Library invitations

        /// <summary>
        ///A test for LibraryInvitation_Create
        ///</summary>
        [TestMethod()]
        public void LibraryInvitation_CreateTest()
        {
            LibraryBizService target = new LibraryBizService();
            LibraryInvitation entity = new LibraryInvitation();
            entity.TargetUserId = 2;
            entity.Subject = "Unit test invitation";
            entity.TargetEmail = "mparsons@siuccwd.com";
            entity.InvitationType = "personal";

            string statusMessage = string.Empty;
            string statusMessageExpected = "Successful";
            int expected = 0;
            int actual;
            actual = target.LibraryInvitation_Create( entity, ref statusMessage );
            Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreNotEqual( expected, actual );

        }
        /// <summary>
        ///A test for LibraryInvitation_GetByPasscode
        ///</summary>
        [TestMethod()]
        public void LibraryInvitation_GetByPasscodeTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            string pPassCode = string.Empty; 
            LibraryInvitation expected = null; 
            LibraryInvitation actual;
            actual = target.LibraryInvitation_GetByPasscode( pPassCode );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for LibraryInvitation_Get by rowId
        ///</summary>
        [TestMethod()]
        public void LibraryInvitation_GetTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            string rowId = string.Empty; 
            LibraryInvitation expected = null; 
            LibraryInvitation actual;
            actual = target.LibraryInvitation_GetByGuid( rowId );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for LibraryInvitation_Get
        ///</summary>
        [TestMethod()]
        public void LibraryInvitation_GetTest1()
        {
            LibraryBizService target = new LibraryBizService(); 
            int id = 0; 
            LibraryInvitation expected = null; 
            LibraryInvitation actual;
            actual = target.LibraryInvitation_Get( id );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for LibraryInvitation_Get
        ///</summary>
        [TestMethod()]
        public void LibraryInvitation_GetTest2()
        {
            LibraryBizService target = new LibraryBizService(); 
            int libraryId = 0; 
            int userId = 0; 
            LibraryInvitation expected = null; 
            LibraryInvitation actual;
            actual = target.LibraryInvitation_Get( libraryId, userId );
            Assert.AreEqual( expected, actual );

        }

        /// <summary>
        ///A test for LibraryInvitation_Delete
        ///</summary>
        [TestMethod()]
        public void LibraryInvitation_DeleteTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int id = 0; 
            bool expected = false; 
            bool actual;
            actual = target.LibraryInvitation_Delete( id );
            Assert.AreEqual( expected, actual );
        }

        #endregion

        #region Library Section members
        /// <summary>
        ///A test for LibraryCollectionMember_Update
        ///</summary>
        [TestMethod()]
        public void LibraryCollectionMember_UpdateTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int id = 0; 
            int memberTypeId = 0; 
            int lastUpdatedById = 0; 
            bool expected = false; 
            bool actual;
            actual = target.LibraryCollectionMember_Update( id, memberTypeId, lastUpdatedById );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///A test for LibraryCollectionMember_Get
        ///</summary>
        [TestMethod()]
        public void LibraryCollectionMember_GetTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int parentId = 0; 
            int userId = 0; 
            LibraryMember expected = null; 
            LibraryMember actual;
            actual = target.LibraryCollectionMember_Get( parentId, userId );
            Assert.AreEqual( expected, actual );
            
        }

        /// <summary>
        ///A test for LibraryCollectionMember_Get
        ///</summary>
        [TestMethod()]
        public void LibraryCollectionMember_GetTest1()
        {
            LibraryBizService target = new LibraryBizService(); 
            int id = 0; 
            LibraryMember expected = null; 
            LibraryMember actual;
            actual = target.LibraryCollectionMember_Get( id );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///A test for LibraryCollectionMember_Delete
        ///</summary>
        [TestMethod()]
        public void LibraryCollectionMember_DeleteTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int id = 0; 
            bool expected = false; 
            bool actual;
            actual = target.LibraryCollectionMember_Delete( id );
            Assert.AreEqual( expected, actual );
        }

        /// <summary>
        ///A test for LibraryCollectionMember_Create
        ///</summary>
        [TestMethod()]
        public void LibraryCollectionMember_CreateTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int parentId = 0; 
            int userId = 0; 
            int memberTypeId = 0; 
            int createdById = 0; 
            string statusMessage = string.Empty; 
            string statusMessageExpected = string.Empty; 
            int expected = 0; 
            int actual;
            actual = target.LibraryCollectionMember_Create( parentId, userId, memberTypeId, createdById, ref statusMessage );
            Assert.AreEqual( statusMessageExpected, statusMessage );
            Assert.AreEqual( expected, actual );
        }
        #endregion


        /// <summary>
        ///A test for Library_SelectLibrariesFollowing
        ///</summary>
        [TestMethod()]
        public void Library_SelectLibrariesFollowingTest()
        {
            LibraryBizService target = new LibraryBizService(); 
            int userId = 2;
            FollowingCollection expected = new FollowingCollection();
            expected.LibraryIds.Add( 1 );
            FollowingCollection actual;
            actual = target.Library_SelectLibrariesFollowing( userId );
            Assert.AreNotEqual( expected.LibraryIds.Count, actual.LibraryIds.Count );

        }
    }
}
