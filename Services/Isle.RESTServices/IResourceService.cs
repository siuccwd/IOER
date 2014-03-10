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

    [ServiceContract]
    public interface IResourceService
    {
        //[WebGet( UriTemplate = "/ResourceGet" )]
        //[OperationContract]
        //[AuthenticationBehavior]
        //[WebInvoke( Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        //ResourceGetResponse ResourceGet( ResourceGetRequest request );

        [WebGet( UriTemplate = "/ResourceGetId?id={id}", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        [OperationContract]
        ResourceGetResponse ResourceGetId( int id );

        //[WebGet( UriTemplate = "/ResourceSearch" )]
        //[OperationContract]
        //[AuthenticationBehavior]
        //[WebInvoke( Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        //ResourceSearchResponse ResourceSearch( ResourceSearchRequest request );


        [WebGet( UriTemplate = "/Search?pageNbr={pageNbr}&filter={filter}", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        [OperationContract]
        ResourceSearchResponse Search( string pageNbr, string filter );



        [WebGet( UriTemplate = "/Search2?pageNbr={pageNbr}&clusters={clusters}&accessType={accessType}", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        [OperationContract]
        ResourceSearchResponse Search2( string pageNbr, string clusters, string accessType );
    }
}
