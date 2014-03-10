using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using Isle.DataContracts;

namespace Isle.RESTServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ILibraryService" in both code and config file together.
    [ServiceContract]
    public interface ILibraryService
    {
        [OperationContract]
        [WebInvoke( UriTemplate = "/Search", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        LibraryResourceSearchResponse Search( LibraryResourceSearchRequest request );

        [OperationContract]
        [WebGet( UriTemplate = "/MyLibrary/{userName}/{pageNbr}?filter={filter}", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        LibraryResourceSearchResponse MyLibrary( string userName, string pageNbr, string filter );

        [OperationContract]
        [WebGet( UriTemplate = "/{libraryId}/{pageNbr}?filter={filter}", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        LibraryResourceSearchResponse Library( string libraryId, string pageNbr, string filter );


        [OperationContract]
        [WebGet( UriTemplate = "/{libraryId}", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        LibraryResourceSearchResponse LibraryList( string libraryId);


        [OperationContract]
        [WebGet( UriTemplate = "/Collection/{collectionId}/{pageNbr}?filter={filter}", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        LibraryResourceSearchResponse Collection( string collectionId, string pageNbr, string filter );


        [OperationContract]
        [WebGet( UriTemplate = "/Test",  BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        LibraryResourceSearchResponse Test();
    }
}
