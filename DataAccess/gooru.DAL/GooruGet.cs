using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Isle.DTO;

namespace gooru.DAL
{
    public class GooruGet
    {
        LibraryDTO library = new LibraryDTO();
        static async Task PopulateGooruItem( string gooruId )
        {
            using ( var client = new HttpClient() )
            {
                client.BaseAddress = new Uri( "http://localhost:9000/" );
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );

                // New code:
                HttpResponseMessage response = await client.GetAsync( "api/products/1" );
                if ( response.IsSuccessStatusCode )
                {
                   // library = await response.Content.ReadAsAsync<LibraryDTO>();

                   // Console.WriteLine( "{0}\t${1}\t{2}", library.Title, library.ImageUrl, library.Url );
                }
            }
        }
    }
}
