using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ILPathways.Business;
using Isle.BizServices;

namespace LibrariesUnitTests
{
    [TestClass]
    public class LibraryCommentUnitTest
    {
        [TestMethod]
        public void AddLibraryComment()
        {
            int notExpected = 0;
            //arrange
            ObjectComment oc = new ObjectComment();
            oc.ParentId = 1;
            oc.CreatedById = 2;
            oc.Created = System.DateTime.Now;
            oc.Comment = "Comment from unit test";

            //act
            LibraryBizService mgr = new LibraryBizService();
            int id = mgr.LibraryCommentCreate( oc );

            //assert
            string msg = "should be >0. Actual = " + id.ToString();
            Assert.AreNotEqual( notExpected, id, "should not be equal." );

        }

        [TestMethod]
        public void AddLibraryCommentById()
        {
            int notExpected = 0;
            //arrange
            int libraryId = 1;
            int userId = 2;
            string comment = "unit test comment";
    
            //act
            LibraryBizService mgr = new LibraryBizService();
            int id = mgr.LibraryCommentCreate( libraryId, comment, userId );

            //assert
            string msg = "should be >0. Actual = " + id.ToString();
            Assert.AreNotEqual( notExpected, id, "should not be equal." );

        }


        [TestMethod]
        public void AddLibrarySectionComment()
        {
            int notExpected = 0;
            //arrange
            ObjectComment oc = new ObjectComment();
            oc.ParentId = 2;
            oc.CreatedById = 2;
            oc.Created = System.DateTime.Now;
            oc.Comment = "Collection Comment from unit test";

            //act
            LibraryBizService mgr = new LibraryBizService();
            int id = mgr.LibrarySectionCommentCreate( oc );

            //assert
            string msg = "should be >0. Actual = " + id.ToString();
            Assert.AreNotEqual( notExpected, id, "should not be equal." );
        }

        [TestMethod]
        public void AddLibrarySectionById()
        {
            int notExpected = 0;
            //arrange
            int sectionId = 3;
            int userId = 22;
            string comment = "Collection Comment from unit test";

            //act
            LibraryBizService mgr = new LibraryBizService();
            int id = mgr.LibrarySectionCommentCreate( sectionId, comment, userId );

            //assert
            string msg = "should be >0. Actual = " + id.ToString();
            Assert.AreNotEqual( notExpected, id, "should not be equal." );

        }


    }
}
