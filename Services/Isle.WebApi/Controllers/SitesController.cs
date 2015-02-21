using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Web.Script.Serialization;

using LB = LRWarehouse.Business;
using Isle.BizServices;
using Isle.DTO;

namespace Isle.WebApi.Controllers
{
    public class SitesController : ApiController
    {
        LB.CodesSite[] sites = new LB.CodesSite[] 
        { 
            new LB.CodesSite { Id = 1, Title = "IOER",  Description = "IOER"  }, 
            new LB.CodesSite { Id = 2, Title = "DisabilityWorks", Description = "DisabilityWorks"}, 
            new LB.CodesSite { Id = 3, Title = "workNet", Description = "workNet"},
            new LB.CodesSite { Id = 4, Title = "Manufacturing", Description = "Manufacturing"}
        };
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public IEnumerable<SiteFiltersDTO> GetAllSites()
        {

            List<SiteFiltersDTO> list = new List<SiteFiltersDTO>();
            SiteFiltersDTO site = new SiteFiltersDTO();
            for ( int i = 1 ; i <= 4 ; i++ )
            {
                site = CodeTableBizService.Site_SelectAsDto( i, false );
                list.Add( site );
            }
            return list;
        }

        /// <summary>
        /// Retrieve filters for provided site using either site id or site name (from code table)
        /// </summary>
        /// <param name="id">If greater than zero, will return site and list of filters</param>
        /// <param name="siteName">If id==0, and siteName provided, will return site and list of filters</param>
        /// <param name="mustHaveResources">True- only codes returned are those with warehouse total > 0, otherwise all</param>
        /// <returns></returns>
        [HttpGet]
        public SiteFiltersDTO Get( int id, string siteName, bool mustHaveResources )
        {
//            string siteName = "";
            SiteFiltersDTO site = new SiteFiltersDTO();
            if ( id > 0 )
            {
                site = CodeTableBizService.Site_SelectAsDto( id, mustHaveResources );

            }
            else if ( siteName != null && siteName.Trim().Length > 0 )
            {
                site = CodeTableBizService.Site_SelectAsDto( siteName, mustHaveResources );
            }
            else
            {
                site.SiteName = "Error - a valid site identifier was not provided.";
            }
            //var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            //json.UseDataContractJsonSerializer = true;

            //string str2 = ImmediateReturn( site, true, "OK", new { totalFilters = site.FilterList.Count() } );
            //string str = Serialize( json, site );
            //return str2;
            return site;
        }
        private string ImmediateReturn( object data, bool isValid, string status, object extra )
        {
            return serializer.Serialize(
              new
              {
                  data = data,
                  isValid = isValid,
                  status = status,
                  extra = extra
              }
            );
        }
        string Serialize<T>( MediaTypeFormatter formatter, T value )
        {
            // Create a dummy HTTP Content.
            Stream stream = new MemoryStream();
            var content = new StreamContent( stream );
            /// Serialize the object.
            formatter.WriteToStreamAsync( typeof( T ), value, stream, content, null ).Wait();
            // Read the serialized string.
            stream.Position = 0;
            return content.ReadAsStringAsync().Result;
        }

        T Deserialize<T>( MediaTypeFormatter formatter, string str ) where T : class
        {
            // Write the serialized string to a memory stream.
            Stream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter( stream );
            writer.Write( str );
            writer.Flush();
            stream.Position = 0;
            // Deserialize to an object of type T
            return formatter.ReadFromStreamAsync( typeof( T ), stream, null, null ).Result as T;
        }

        // POST api/<controller>
        public void Post( [FromBody]string value )
        {
        }

        // PUT api/<controller>/5
        public void Put( int id, [FromBody]string value )
        {
        }

        // DELETE api/<controller>/5
        public void Delete( int id )
        {
        }
    }
}