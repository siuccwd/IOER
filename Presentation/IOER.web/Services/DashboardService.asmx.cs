using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

using LRWarehouse.Business;
using LRWarehouse.DAL;
using IB = ILPathways.Business;
//using ILPathways.DAL;
using MyLibraryManager = Isle.BizServices.LibraryBizService;
using OrganizationManager = Isle.BizServices.OrganizationBizService;
using Isle.BizServices;
using Patron = LRWarehouse.Business.Patron;

namespace IOER.Services
{

    /// <summary>
    /// Summary description for DashboardService
    /// </summary>
    [WebService( Namespace = "http://tempuri.org/" )]
    [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
    [System.ComponentModel.ToolboxItem( false )]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class DashboardService : System.Web.Services.WebService
    {
        UtilityService utilService = new UtilityService();

        [WebMethod]
        public string ToggleFollowing( int followingUserId, int followedByUserId )
        {
            bool isValid = false;
            string status = "";

            bool isFollowingNow = ToggleFollowing( followingUserId, followedByUserId, ref isValid, ref status );

            return utilService.ImmediateReturn( isFollowingNow, isValid, status, null );
        }
        public bool ToggleFollowing( int followingUserId, int followedByUserId, ref bool isValid, ref string status )
        {
            var output = false;

            //Validate user
            if ( followedByUserId == 0  )
            {
                isValid = false;
                status = "You must login to follow!";
                return output;
            }
            if ( followingUserId == 0 )
            {
                isValid = false;
                status = "A following user must be selected!";
                return output;
            }
            //Toggle the following
            if ( AccountServices.Person_FollowingIsMember( followingUserId, followedByUserId ) )
            {
                AccountServices.Person_FollowingDelete( followingUserId, followedByUserId );
                output = false;
            }
            else
            {
                AccountServices.Person_FollowingAdd( followingUserId, followedByUserId );
                output = true;
            }

            isValid = true;
            status = "okay";
            return output;
        }


        #region Get methods
        [WebMethod]
        public UserProfile GetProfile(InputEnvelope input)
        {
            //return GetUserData( new Guid() ); //Temporary
            return (UserProfile) GetViaGUID( GetUserData, input.user );
        }

        [WebMethod]
        public LibraryProfile GetLibrary( InputEnvelope input )
        {
            return GetLibraryData( new Guid() ); //Temporary
            //return ( LibraryProfile ) GetViaGUID( GetLibraryData, input.user );
        }

        [WebMethod]
        public List<CollectionItem> GetCollectionItems( InputEnvelope input )
        {
            return GetCollectionData( new Guid() ); //Temporary
           // return ( List<CollectionItem> ) GetViaGUID( GetCollectionData, input.user );
        }

        [WebMethod]
        public List<FollowedItem> GetFollowedItems( InputEnvelope input )
        {
            return GetFollowedData( new Guid() ); //Temporary
            //return ( List<FollowedItem> ) GetViaGUID( GetFollowedData, input.user );
        }

        [WebMethod]
        public List<ResourceItem> GetResourceItems( InputEnvelope input )
        {
            return GetResourceData( new Guid() ); //Temporary
           // return ( List<ResourceItem> ) GetViaGUID( GetResourceData, input.user );
        }

        [WebMethod]
        public UserProfile UpdateProfile( InputEnvelope input )
        {
            try
            {
                Guid userGUID = new Guid();
                if ( Authenticate( input.user, ref userGUID ) )
                {
                    //Load the User Profile object from the input
                    UserProfile updatedProfile = ( UserProfile ) input.data;

                    //Validation
                    string[] testItems = new string[] { updatedProfile.name, updatedProfile.organization, updatedProfile.role };
                    foreach ( string test in testItems )
                    {
                        if ( !ValidateString( test ) )
                        {
                            return null;
                        }
                    }

                    //Update the database


                    //Retrieve the data again
                    return GetUserData( userGUID );
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        [WebMethod]
        public LibraryProfile UpdateLibrary( InputEnvelope input )
        {
            try
            {
                Guid userGUID = new Guid();
                if ( Authenticate( input.user, ref userGUID ) )
                {
                    //Load the User Profile object from the input
                    LibraryProfile updatedProfile = ( LibraryProfile ) input.data;

                    //Validation
                    string[] testItems = new string[] { updatedProfile.name, updatedProfile.description };
                    foreach ( string test in testItems )
                    {
                        if ( !ValidateString( test ) )
                        {
                            return null;
                        }
                    }

                    //Update the database


                    //Retrieve the data again
                    return GetLibraryData( userGUID );
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        [WebMethod]
        public List<FollowedItem> UpdateFollowing( InputEnvelope input )
        {
            try
            {
                Guid userGUID = new Guid();
                if ( Authenticate( input.user, ref userGUID ) )
                {
                    //Load the following list form the input
                    FollowedItem[] updatedItems = ( FollowedItem[] ) input.data;

                    foreach ( FollowedItem item in updatedItems )
                    {
                        int followingMode = item.following;

                        //Update the database as needed. 0 = unsubscribe, 1 = daily, 2 = weekly.
                    }

                    //Retrieve the updated data
                    return GetFollowedData( userGUID );
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region helper methods
        protected bool ValidateString( string input )
        {
            string[] invalidCharacters = "\"<>!/\\@&%$#^*".Split();
            foreach ( string test in invalidCharacters )
            {
                if ( input.Length < 3 )
                {
                    return false;
                }
                if ( input.IndexOf( test ) > 0 )
                {
                    return false;
                }
            }
            return true;
        }
        protected bool Authenticate( string input, ref Guid userGUID )
        {
            try
            {
                userGUID = Guid.Parse( input );
                //Authenticate user via GUID
                if ( true ) //Temporary
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        protected object GetViaGUID( Func<Guid,object> targetFunction, string user )
        {
            try
            {
                Guid userGUID = new Guid();
                if ( Authenticate( user, ref userGUID ) )
                {
                    return targetFunction( userGUID );
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        protected UserProfile GetUserData(Guid userGUID)
        {
            //Get User data via GUID
            Patron webUser = new LRWarehouse.DAL.PatronManager().GetByRowId( userGUID.ToString() );
            IB.Organization organization = OrganizationManager.EFGet( webUser.OrgId );

            UserProfile profile = new UserProfile();
            profile.name = webUser.FullName();
            profile.organization = organization.Name;
            profile.role = webUser.UserProfile.PublishingRole;
            profile.avatar = webUser.UserProfile.ImageUrl; // "/images/ioer_med.png"; //Temporary until user avatars are implemented
            return profile;
        }
        protected LibraryProfile GetLibraryData( Guid userGUID )
        {
            //Get Library data via User's GUID
            Patron webUser = new LRWarehouse.DAL.PatronManager().GetByRowId( userGUID.ToString() );
			IB.Library myLibrary = new MyLibraryManager().GetMyLibrary(webUser);

            LibraryProfile profile = new LibraryProfile();
            profile.name = myLibrary.Title;
            profile.description = myLibrary.Description;
            profile.avatar = myLibrary.ImageUrl;
            return profile;
        }
        protected List<CollectionItem> GetCollectionData( Guid userGUID )
        {
            List<CollectionItem> items = new List<CollectionItem>();
            //Get the collections for a user via User's GUID
            Patron webUser = new LRWarehouse.DAL.PatronManager().GetByRowId( userGUID.ToString() );
			IB.Library myLibrary = new MyLibraryManager().GetMyLibrary(webUser);
            DataSet ds = new MyLibraryManager().LibrarySectionsSelect( myLibrary.Id, 2 ); 

            if ( LRWarehouse.DAL.DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    //if ( GetIntField( dr, "LibraryId" ) == webUser.CurrentLibraryId )
                    //{
                        CollectionItem item = new CollectionItem();
                        item.id = GetIntField( dr, "Id" );
                        item.href = "/My/Library.aspx?sId=" + item.id;
                        item.name = GetField( dr, "Title" );
                        item.resources = GetIntField(dr, "SectionResourceCount");
                        item.image = GetField(dr, "LibraryImageUrl");

                        items.Add( item );
                    //}
                }
            }

            /*
            //Temporary
            Random randomizer = new Random();
            for ( int i = 1 ; i < randomizer.Next(10, 30) ; i++ )
            {
                CollectionItem item = new CollectionItem();
                item.id = i;
                item.href = "#";
                item.name = "Collection " + i;
                item.resources = randomizer.Next(100);
                item.image = "/images/ioer_med.png";

                items.Add( item );
            }//
            */

            return items;
        }
        protected List<FollowedItem> GetFollowedData( Guid userGUID )
        {
            List<FollowedItem> items = new List<FollowedItem>();
            //Get the followed libraries for a user via the User's GUID

            //Temporary
            Random randomizer = new Random();
            for ( int i = 1 ; i < randomizer.Next(10, 40) ; i++ )
            {
                FollowedItem item = new FollowedItem();
                item.id = i;
                item.href = "#";
                item.name = "Library " + i;
                item.resources = randomizer.Next( 200 );
                item.image = "/images/ioer_med.png";
                item.following = randomizer.Next( 1, 3 );

                items.Add( item );
            }//

            return items;
        }
        protected List<ResourceItem> GetResourceData( Guid userGUID )
        {
            List<ResourceItem> items = new List<ResourceItem>();
            //Get a list of resources the user has created, via the user's GUID

            //Temporary
            Random randomizer = new Random();
            for ( int i = 1 ; i < randomizer.Next( 10, 90 ) ; i++ )
            {
                ResourceItem item = new ResourceItem();
                item.id = i;
                item.href = "#";
                item.name = "My Resource #" + i;
                item.image = "/images/ioer_med.png";

                items.Add( item );
            }//

            return items;
        }
        protected int GetIntField( DataRow dr, string column )
        {
            try 
            {
                return int.Parse( LRWarehouse.DAL.DatabaseManager.GetRowColumn( dr, column ) );
            }
            catch 
            {
                return 0;
            }
        }
        protected string GetField( DataRow dr, string column )
        {
            return LRWarehouse.DAL.DatabaseManager.GetRowColumn( dr, column );
        }
        #endregion

        #region subclasses
        public class InputEnvelope 
        {
            public string user;
            public object data;
        }
        public class UserProfile
        {
            public string name;
            public string role;
            public string organization;
            public string avatar;
        }
        public class LibraryProfile
        {
            public string name;
            public string description;
            public string avatar;
        }
        public class BasicItem
        {
            public int id;         //Item ID
            public string href;    //link to view item
            public string name;    //Title of item
            public string image;   //Avatar of item
        }
        public class CollectionItem : BasicItem //Equivalent to a single collection. A list of these is returned
        {
            public int resources;  //Count of resources in the collection
        }
        public class FollowedItem : BasicItem   //Equivalent to a single followed library. A list of these is returned
        {
            public int resources;   //Count of resources in the library
            public int following;   //Integer representing the following mode. 0 = unsubscribe, 1 = daily, 2 = weekly
        }
        public class ResourceItem : BasicItem  //Equivalent to a single Resource. A list of these is returned
        {
            //No additional fields at the moment. May need some in the future.
        }
        #endregion
    }
}
