using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;

using Isle.DataContracts;
using Isle.BizServices;
using LRWarehouse.Business;
//.use alias for easy change to Gateway
//using ThisUser = ILPathways.Business.AppUser;


using ThisUser = LRWarehouse.Business.Patron;
//using PatronMgr = LRWarehouse.DAL.PatronManager;

namespace Isle.RESTServices
{
    [AspNetCompatibilityRequirements( RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed )]
    public class UserDataService : IUserDataService
    {

        AccountServices userBizService = new AccountServices();

        public UserLoginResponse Login( UserLoginRequest request )
        {
            UserLoginResponse response = new UserLoginResponse();
            // response.Account = new AccountDataContract();
            AccountServices services = new AccountServices();
            string statusMessage = "";

            //if ( ValidateServiceAcct( request.ServiceAccount ) == false )
            //{
            //    response.MessageList.Add( new ErrorDataContract( "1", "Invalid service request (account) " ) );
            //    response.Status = StatusEnumDataContract.Failure.ToString();
            //    return response;
            //}

            if ( request.IsQuickLoginType )
            {
                //return QuickLogin(request.ServiceAccount, request.UniqueId, "");
                return QuickLogin( request );
            }
            // request.IsQuickLoginType is FALSE
            ThisUser user = services.Login( request.UserName, request.Password, ref statusMessage );

            if ( user != null && user.IsValid )
            {
                response.Status = StatusEnumDataContract.Success;
                response.UserName = user.UserName;
                response.FirstName = user.FirstName;
                response.LastName = user.LastName;
                response.Email = user.Email;
                response.Zipcode = user.ZipCode;
                response.RowId = user.RowId.ToString();
            }
            else
            {
                response.Status = StatusEnumDataContract.Failure;
                response.MessageList.Add( new ErrorDataContract( "1", statusMessage ) );
            }
            return response;
        }


        //public UserLoginResponse QuickLogin( string serviceAccount, string id, string mobilePhoneNbr )
        public UserLoginResponse QuickLogin( UserLoginRequest request )
        {
            UserLoginResponse response = new UserLoginResponse();
            // response.Account = new AccountDataContract();
            AccountServices services = new AccountServices();

            if ( ValidateServiceAcct( request.ServiceAccount ) == false )
            {
                response.MessageList.Add( new ErrorDataContract( "1", "Invalid service request (account) " ) );
                response.Status = StatusEnumDataContract.Failure;
                return response;
            }

            string statusMessage = "";
            ThisUser user = services.GetByRowId( request.UniqueId, ref statusMessage );

            if ( user != null && user.IsValid )
            {
                response.Status = StatusEnumDataContract.Success;
                response.UserName = user.Username;
                response.FirstName = user.FirstName;
                response.LastName = user.LastName;
                response.Email = user.Email;
                response.Zipcode = user.ZipCode;
                response.RowId = user.RowId.ToString();
            }
            else
            {
                response.Status = StatusEnumDataContract.Failure;
                response.Error.Message = statusMessage;
                response.MessageList.Add( new ErrorDataContract( "1", statusMessage ) );
            }

            return response;
        }

        public string GetDateTime()
        {
            return System.DateTime.Now.ToString();
        }

        private bool ValidateServiceAcct( string account )
        {
            bool isValid = true;
            if ( account.ToLower() != "isle" )
                isValid = false;
            return isValid;
        }

        private bool ValidateDate( string value, ref DateTime validDate )
        {
            bool isValid = true;
            validDate = new DateTime();
            DateTime minDate = System.DateTime.Today.AddYears( -12 );
            DateTime maxDate = System.DateTime.Today.AddYears( -100 );

            if ( ServiceHelper.StringToDate( value, ref validDate ) == false )
                isValid = false;
            else if ( validDate > minDate || validDate < maxDate )
                isValid = false;

            return isValid;
        }

    }

}
