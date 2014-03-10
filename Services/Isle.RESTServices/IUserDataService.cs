using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using Isle.DataContracts;
//using IllinoisPathways.WCFServices.Behavior;

namespace Isle.RESTServices
{

    [ServiceContract]
    public interface IUserDataService
    {
        [OperationContract]
        [WebInvoke( UriTemplate = "/Login", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST" )]
        UserLoginResponse Login( UserLoginRequest request );

        [OperationContract]
        [WebInvoke( UriTemplate = "/QuickLogin", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST" )]
        UserLoginResponse QuickLogin( UserLoginRequest request );


        //[OperationContract]
        //[AuthenticationBehavior]
        //[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        //UpdateUserResponse ChangePassword(UpdateUserRequest updateUserRequest);

        //[WebGet( UriTemplate = "/GetUserDetail", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        //[OperationContract]
        //[AuthenticationBehavior]
        //PatronGetResponse GetUserDetail( PatronGetRequest request );


        [WebGet( UriTemplate = "/GetDateTime", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json )]
        [OperationContract]
        string GetDateTime();
    }
}
