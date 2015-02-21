using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Isle.DTO;
using Mgr = Isle.Factories.ResourceFactory;

namespace Isle.WebApi.Controllers
{
    public class ResourceController : ApiController
    {
        /// <summary>
        /// Get a list of resources
        /// sample jsonp call:
        /// 
        /// $.ajax({
        ///     url: 'http://myurl.com',
        ///     type: 'GET',
        ///     dataType: 'jsonp',
        ///     success: function (data) {
        ///     alert(data.MyProperty);
        ///     }
        /// })
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        public List<ResourceDTO> Get( string idList)
        {
            List<ResourceDTO> list = new List<ResourceDTO>();
            string[] ids = idList.Split( ',' );
            int resourceId = 0;
            if ( ids.Length > 0 )
            {
                foreach ( string idx in ids )
                {
                    if ( Int32.TryParse( idx, out resourceId ) )
                    {
                        list.Add( Mgr.GetResourceSummary( resourceId ) ); 
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// GET resource
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ResourceDTO Get( int id )
        {
            ResourceDTO entity = new ResourceDTO();
            entity = Mgr.GetResourceSummary( id );
            if ( entity == null || entity.Id == 0 )
                return new ResourceDTO();
            else
                return entity;
        }

    }
}