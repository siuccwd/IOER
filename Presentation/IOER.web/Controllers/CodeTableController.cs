using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using bizService = ILPathways.CodeTableServiceReference;
using CodesContract = ILPathways.CodeTableServiceReference.CodesDataContract;
using ILPathways.Utilities;

namespace ILPathways.Controllers
{
    public class CodeTableController
    {
        bizService.CodeTableServiceClient client = new bizService.CodeTableServiceClient();
        public CodesContract[] codeList;
        //none
        public event EventHandler GetCodesNoComponents;
        public event EventHandler GetCodesSuccess;
        public event EventFailureEventHandler GetCodesFailure;

        private ObservableCollection<CodesContract> _codes2;
        public ObservableCollection<CodesContract> CodesListOC
        {
            get
            {
                if ( this._codes2 == null )
                {
                    this._codes2 = new ObservableCollection<CodesContract>();
                }
                return this._codes2;
            }
            set
            {
                this._codes2 = value;
                // this.OnPropertyChanged( "CodesList" );
            }
        }

        private bizService.CodesDataContract[] _codes;
        public bizService.CodesDataContract[] CodesList
        {
            get
            {
                if ( this._codes == null )
                {
                    this._codes = new bizService.CodesDataContract[ 1 ];
                }
                return this._codes;
            }
            set
            {
                this._codes = value;
                // this.OnPropertyChanged( "CodesList" );
            }
        }
        public string LastTableName = "";
        public CodeTableController()
        {
            client.Endpoint.Address = new EndpointAddress( ServiceReferenceHelper.GetServicesAddress() + "CodeTableService.svc" );
            //client.Endpoint.Behaviors.Add( new ILPathways.AuthenticationBehavior() );

            client.CodeTableSearchCompleted += new EventHandler<bizService.CodeTableSearchCompletedEventArgs>( client_CodeTableSearchCompleted );
        }

        public void CodeTableSearch( string tableName, string valueField, string textField, string sortField, string filter )
        {

            try
            {
                bizService.CodeSearchRequest r = new bizService.CodeSearchRequest();
                r.TableName = tableName;
                r.IdColumn = valueField;
                r.TitleColumn = textField;
                r.OrderBy = sortField;
                r.Filter = filter;
                r.UseWarehouseTotalTitle = false;

                client.CodeTableSearchAsync( r );
                //client.Close();
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "CodeTableController.CodeTableSearch() " );
            }

        }

        public void CodeTableSearch( string tableName, string valueField, string textField, string sortField, bool useWarehouseTotalTitle )
        {
            try
            {
                bizService.CodeSearchRequest r = new bizService.CodeSearchRequest();
                r.TableName = tableName;
                r.IdColumn = valueField;
                r.TitleColumn = textField;
                r.OrderBy = sortField;
                r.UseWarehouseTotalTitle = useWarehouseTotalTitle;

                client.CodeTableSearchAsync( r );
               //client.Close();
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "CodeTableController.CodeTableSearch(bool useWarehouseTotalTitle) " );
            }

        }

        void client_CodeTableSearchCompleted( object sender, bizService.CodeTableSearchCompletedEventArgs e )
        {
            try
            {
                bizService.CodeSearchResponse r = e.Result as bizService.CodeSearchResponse;
                LastTableName = "";
                if ( r.Status == bizService.StatusEnumDataContract.Success )
                {
                    this.CodesList = e.Result.ResultList;
                    if ( CodesList.Length > 0 )
                    {
                        LastTableName = e.Result.TableName;
                        if ( GetCodesSuccess != null )
                            this.GetCodesSuccess( this, null );
                    }
                    else
                    {
                        if ( GetCodesNoComponents != null )
                            this.GetCodesNoComponents( this, null );
                    }
                }
                else
                {
                    this.CodesList = new bizService.CodesDataContract[ 1 ];
                    EventFailureEventArgs args = new EventFailureEventArgs( r.Error.Message );

                    if ( GetCodesFailure != null )
                        this.GetCodesFailure( this, args );
                }
            }
            catch ( Exception ex )
            {
                EventFailureEventArgs args = new EventFailureEventArgs( ex.Message );
                if ( GetCodesFailure != null )
                    this.GetCodesFailure( this, args );

                LoggingHelper.LogError( ex, "CodeTableController.client_CodeTableSearchCompleted() " );
            }
        }//
    }
}