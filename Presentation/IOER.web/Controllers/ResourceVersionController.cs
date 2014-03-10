using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Web;
using ILPathways.ResourceServiceReference;
using ILPathways;

namespace ILPathways.Controllers
{
    public class ResourceVersionController 
    {
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged( string propertyName )
        //{
        //    if ( PropertyChanged != null )
        //    {
        //        //App.IsCurrentViewDirty = true;
        //        PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        //    }
        //}

        ResourceServiceClient client = new ResourceServiceClient();
        public ILPathways.ResourceServiceReference.ResourceDataContract[] ResourceList23;
        private ILPathways.ResourceServiceReference.ResourceDataContract[] _resources2;
        public ILPathways.ResourceServiceReference.ResourceDataContract[] ResourceList2
        {
            get
            {
                if ( this._resources2 == null )
                {
                    this._resources2 = new ILPathways.ResourceServiceReference.ResourceDataContract[1];
                }
                return this._resources2;
            }
            set
            {
                this._resources2 = value;
                // this.OnPropertyChanged( "ResourceList2" );
            }
        }

       
        //none
        public event EventHandler GetResourcesNoComponents;
        //get
        public event EventHandler GetResourcesSuccess;
        public event EventFailureEventHandler GetResourcesFailure;


        private ObservableCollection<ResourceDataContract> _resources;
        public ObservableCollection<ResourceDataContract> ResourceList
        {
            get
            {
                if ( this._resources == null )
                {
                    this._resources = new ObservableCollection<ResourceDataContract>();
                }
                return this._resources;
            }
            set
            {
                this._resources = value;
                // this.OnPropertyChanged( "ResourceList" );
            }
        }


        public ResourceVersionController()
        {
            client.Endpoint.Address = new EndpointAddress( ServiceReferenceHelper.GetServicesAddress() + "ResourceService.svc" );
            client.Endpoint.Behaviors.Add( new ILPathways.AuthenticationBehavior() );
            client.ResourceSearchCompleted += new EventHandler<ResourceSearchCompletedEventArgs>(client_ResourceSearchCompleted);

        }

        public void ResourceSearch( string filter, string keywords, int pageNbr, int pageSize)
        {

            ResourceSearchRequest r = new ResourceSearchRequest();
            r.Filter = filter;
            r.PageSize = pageSize;
            r.SortOrder = "";
            r.StartingPageNbr = pageNbr;
            r.Keywords = keywords;

            client.ResourceSearchAsync( r );
        }
        void client_ResourceSearchCompleted( object sender, ResourceSearchCompletedEventArgs e )
        {
            ResourceSearchResponse r = e.Result as ResourceSearchResponse;

            if ( r.Status == StatusEnumDataContract.Success )
            {
                this.ResourceList2 = e.Result.ResourceList;


                if ( ResourceList.Count > 0 )
                {
                    if ( GetResourcesSuccess != null )
                        this.GetResourcesSuccess( this, null );
                }
                else
                {
                    if ( GetResourcesNoComponents != null )
                        this.GetResourcesNoComponents( this, null );
                }
            }
            else
            {
                //this.ResourceList2 = new ObservableCollection<ResourceDataContract>();
                this.ResourceList2 = new ILPathways.ResourceServiceReference.ResourceDataContract[1];
                EventFailureEventArgs args = new EventFailureEventArgs( r.Error.Message );
    
                if ( GetResourcesFailure != null )
                    this.GetResourcesFailure( this, args );
            }
        }
    }
}