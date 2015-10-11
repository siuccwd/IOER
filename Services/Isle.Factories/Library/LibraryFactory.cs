using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILPathways.Business;
using ILPathways.Utilities;

using Isle.DTO;
using Isle.DTO.Common;
using Isle.BizServices;
using MyMgr = Isle.BizServices.LibraryBizService;

namespace Isle.Factories
{
    public class LibraryFactory
    {

        public static LibraryDTO Get( int id )
        {
            string site = UtilityManager.GetAppKeyValue( "siteRoot" );

            MyMgr mgr = new MyMgr();
            LibraryDTO dto = new LibraryDTO();
            Library entity = mgr.Get( id );
            if ( entity != null && entity.Id > 0 )
            {
                dto.Id = entity.Id;
                dto.Title = entity.Title;
                dto.Description = entity.Description;

                //need to provide current domain name
                dto.ImageUrl = site + entity.ImageUrl;
                dto.Url = site + LibraryBizService.GetLibraryFriendlyUrl( entity ); 

                //?? arbitrarily return list of collections??
                //public only until can have means to verify privileges
                List<LibrarySection> list = mgr.LibrarySectionsSelectList( entity.Id, 1 );
                foreach ( LibrarySection item in list )
                {
                    CollectionDTO coll = new CollectionDTO();
                    coll.Id = item.Id;
                    coll.Title = item.Title;
                    coll.Description = item.Description;
                    coll.ImageUrl = site + item.ImageUrl;
                    coll.Url = site + item.FriendlyUrl;

                    dto.collections.Add( coll );
                }
            }

            return dto;
        }

        //Probably a simpler way of doing this...
        public AJAXResponse AddToDefaultLibCol( int resourceID, string userGuid )
        {
          var user = new AccountServices().GetByRowId( userGuid );
          if ( user == null || user.Id == 0 )
          {
            return new AJAXResponse() { data = null, valid = false, status = "You must be logged in to do this!", extra = null };
          }

          var libService = new LibraryBizService();
          var libID = libService.GetMyLibrary( user ).Id;
          var section = libService.GetLibrarySection_Default( libID, false );
          if ( section == null || section.Id == 0 )
          {
            return new AJAXResponse() { data = null, valid = false, status = "You must have a Library and a default Collection within it to do this!", extra = new { lib = user.CurrentLibraryId, col = (section != null ? section.Id : 0) } };
          }

          var status = "";
          var test = libService.LibraryResourceCreate( section.Id, resourceID, user.Id, ref status );

          if ( test == 0 || status.ToLower().IndexOf("error") > -1 )
          {
            return new AJAXResponse() { data = null, valid = false, status = "This Resource is already in your default Library's default Collection.", extra = null };
          }

          return new AJAXResponse() { data = null, valid = true, status = status, extra = null };

        }
    }
}
