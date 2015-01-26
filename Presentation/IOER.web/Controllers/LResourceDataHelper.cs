using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;

using LRWarehouse.DAL;


namespace ILPathways.Controllers
{
    public class LResourceDataHelper
    {
        const string thisClassName = "LResourceDataHelper";

        public LResourceDataHelper()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region helper methods using guid based resource Id

        /// <summary>
        /// Populate a checkbox list using IResourceManager
        /// Must implement SelectedCodes, with columns: Id, Title, and IsSelected
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="iManager">a IResourceManager</param>
        /// <param name="currentList">a CheckBoxList</param>
        /// <returns></returns>
        public DataSet PopulateCheckBoxList( string pResourceId, IResourceManager iManager, CheckBoxList currentList )
        {
            currentList.Items.Clear();

            DataSet ds = iManager.SelectedCodes( pResourceId );

            if ( ds != null && ds.Tables.Count > 0 && ds.Tables[ 0 ].Rows.Count > 0 )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                {
                    ListItem item = new ListItem();
                    item.Value = DatabaseManager.GetRowColumn( dr, "id", "0" );
                    item.Text = dr[ "Title" ].ToString().Trim();
                    bool isSelected = DatabaseManager.GetRowColumn( dr, "IsSelected", false );
                    if ( isSelected == true )
                    {

                    }
                    item.Selected = isSelected;

                    currentList.Items.Add( item );

                } //end foreach
            }

            return ds;
        }


        /// <summary>
        /// Perform database updates from a checkboxlist using IResourceManager
        /// Must implement ApplyChanges, and SelectedCodes methods
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="iManager"></param>
        /// <param name="currentTarget"></param>
        /// <param name="currentList"></param>
        /// <param name="userId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool CheckBoxListUpdateItem( string pResourceId, 
                        IResourceManager iManager, 
                        DataSet currentTarget, 
                        CheckBoxList currentList, 
                        int userId,
                        ref string statusMessage )
        {
            bool isValid = true;
            string messages = "";
            statusMessage = "";

            try
            {
                DataSet ds = new DataSet();
                //Retrieve tags list from session
                //First time, there won't be any tags retrieved!
                if ( currentTarget != null )
                {
                    ds = currentTarget;
                    StringBuilder addedItems = new StringBuilder( "" );
                    StringBuilder deletedItems = new StringBuilder( "" );
                    int counter = 0;

                    //Might be okay
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        //Build the added and deleted strings
                        if ( currentList.Items[ counter ].Selected && DatabaseManager.GetRowColumn( dr, "IsSelected", false ) == false )
                        {
                            addedItems.Append( dr[ "id" ] + "|" );
                        }
                        else if ( !currentList.Items[ counter ].Selected && DatabaseManager.GetRowColumn( dr, "IsSelected", false ) == true )
                        {
                            deletedItems.Append( dr[ "id" ] + "|" );
                        }
                        counter++;
                    }


                    //Apply the changes
                    iManager.ApplyChanges( pResourceId, userId, addedItems.ToString(), deletedItems.ToString() );
                }
                else
                {
                    foreach ( ListItem li in currentList.Items )
                    {
                        if ( li.Selected == true )
                        {
                            iManager.Insert( pResourceId, int.Parse( li.Value ), userId, ref statusMessage );
                        }
                        if ( !isValid )
                        {
                            statusMessage += messages + "<br/>";
                        }
                    }
                    if ( statusMessage.Length > 0 )
                    {
                        isValid = false;
                    }
                }

            }// end try
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".CheckBoxListUpdateItem(IResourceManager)" );

                isValid = false;
            }// end catch

            //refresh the saved code list
            DataSet ds2 = iManager.SelectedCodes( pResourceId );
            currentTarget = ds2;

            return isValid;
        }

        public DataSet CheckBoxListUpdateItem2( string pResourceId,
                        IResourceManager iManager,
                        DataSet currentTarget,
                        CheckBoxList currentList,
                        int userId,
                        ref string statusMessage )
        {
            bool isValid = true;
            string messages = "";
            statusMessage = "";

            try
            {
                DataSet ds = new DataSet();
                //Retrieve tags list from session
                //First time, there won't be any tags retrieved!
                if ( currentTarget != null )
                {
                    ds = currentTarget;
                    StringBuilder addedItems = new StringBuilder( "" );
                    StringBuilder deletedItems = new StringBuilder( "" );
                    int counter = 0;

                    //Might be okay
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        //Build the added and deleted strings
                        if ( currentList.Items[ counter ].Selected && DatabaseManager.GetRowColumn( dr, "IsSelected", false ) == false )
                        {
                            addedItems.Append( dr[ "id" ] + "|" );
                        }
                        else if ( !currentList.Items[ counter ].Selected && DatabaseManager.GetRowColumn( dr, "IsSelected", false ) == true )
                        {
                            deletedItems.Append( dr[ "id" ] + "|" );
                        }
                        counter++;
                    }


                    //Apply the changes
                    iManager.ApplyChanges( pResourceId, userId, addedItems.ToString(), deletedItems.ToString() );
                }
                else
                {
                    foreach ( ListItem li in currentList.Items )
                    {
                        if ( li.Selected == true )
                        {
                            iManager.Insert( pResourceId, int.Parse( li.Value ), userId, ref statusMessage );
                        }
                        if ( !isValid )
                        {
                            statusMessage += messages + "<br/>";
                        }
                    }
                    if ( statusMessage.Length > 0 )
                    {
                        isValid = false;
                    }
                }

            }// end try
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".CheckBoxListUpdateItem(IResourceManager)" );

                isValid = false;
            }// end catch

            //refresh the saved code list
            DataSet ds2 = iManager.SelectedCodes( pResourceId );
            currentTarget = ds2;

            return ds2;
        }
        #endregion


        #region helper methods using INT based resource Id

        /// <summary>
        /// Populate a checkbox list using IResourceManager
        /// Must implement SelectedCodes, with columns: Id, Title, and IsSelected
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="iManager">a IResourceManager</param>
        /// <param name="currentList">a CheckBoxList</param>
        /// <returns></returns>
        public DataSet PopulateCheckBoxList( int pResourceId, IResourceIntManager iManager, CheckBoxList currentList )
        {
            currentList.Items.Clear();

            DataSet ds = iManager.SelectedCodes( pResourceId );

            if ( ds != null && ds.Tables.Count > 0 && ds.Tables[ 0 ].Rows.Count > 0 )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                {
                    ListItem item = new ListItem();
                    item.Value = DatabaseManager.GetRowColumn( dr, "id", "0" );
                    item.Text = dr[ "Title" ].ToString().Trim();
                    bool isSelected = DatabaseManager.GetRowColumn( dr, "IsSelected", false );
                    if ( isSelected == true )
                    {

                    }
                    item.Selected = isSelected;

                    currentList.Items.Add( item );

                } //end foreach
            }

            return ds;
        }


        /// <summary>
        /// Perform database updates from a checkboxlist using IResourceManager
        /// Must implement ApplyChanges, and SelectedCodes methods
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="iManager"></param>
        /// <param name="currentTarget"></param>
        /// <param name="currentList"></param>
        /// <param name="userId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool CheckBoxListUpdateItem( int pResourceId,
                        IResourceIntManager iManager,
                        DataSet currentTarget,
                        CheckBoxList currentList,
                        int userId,
                        ref string statusMessage )
        {
            bool isValid = true;
            string messages = "";
            statusMessage = "";

            try
            {
                DataSet ds = new DataSet();
                //Retrieve tags list from session
                //First time, there won't be any tags retrieved!
                if ( currentTarget != null )
                {
                    ds = currentTarget;
                    StringBuilder addedItems = new StringBuilder( "" );
                    StringBuilder deletedItems = new StringBuilder( "" );
                    int counter = 0;

                    //Might be okay
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        //Build the added and deleted strings
                        if ( currentList.Items[ counter ].Selected && DatabaseManager.GetRowColumn( dr, "IsSelected", false ) == false )
                        {
                            addedItems.Append( dr[ "id" ] + "|" );
                        }
                        else if ( !currentList.Items[ counter ].Selected && DatabaseManager.GetRowColumn( dr, "IsSelected", false ) == true )
                        {
                            deletedItems.Append( dr[ "id" ] + "|" );
                        }
                        counter++;
                    }


                    //Apply the changes
                    iManager.ApplyChanges( pResourceId, userId, addedItems.ToString(), deletedItems.ToString() );
                }
                else
                {
                    foreach ( ListItem li in currentList.Items )
                    {
                        if ( li.Selected == true )
                        {
                            iManager.Insert( pResourceId, int.Parse( li.Value ), userId, ref statusMessage );
                        }
                        if ( !isValid )
                        {
                            statusMessage += messages + "<br/>";
                        }
                    }
                    if ( statusMessage.Length > 0 )
                    {
                        isValid = false;
                    }
                }

            }// end try
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".CheckBoxListUpdateItem(IResourceIntManager)" );
                isValid = false;
            }// end catch

            //refresh the saved code list
            DataSet ds2 = iManager.SelectedCodes( pResourceId );
            currentTarget = ds2;

            return isValid;
        }

        public DataSet CheckBoxListUpdateApply( int pResourceId,
                        IResourceIntManager iManager,
                        DataSet currentTarget,
                        CheckBoxList currentList,
                        int userId,
                        ref string statusMessage )
        {
            bool isValid = true;
            string messages = "";
            statusMessage = "";

            try
            {
                DataSet ds = new DataSet();
                //Retrieve tags list from session
                //First time, there won't be any tags retrieved!
                if ( currentTarget != null )
                {
                    ds = currentTarget;
                    StringBuilder addedItems = new StringBuilder( "" );
                    StringBuilder deletedItems = new StringBuilder( "" );
                    int counter = 0;

                    //Might be okay
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        //Build the added and deleted strings
                        if ( currentList.Items[ counter ].Selected && DatabaseManager.GetRowColumn( dr, "IsSelected", false ) == false )
                        {
                            addedItems.Append( dr[ "id" ] + "|" );
                        }
                        else if ( !currentList.Items[ counter ].Selected && DatabaseManager.GetRowColumn( dr, "IsSelected", false ) == true )
                        {
                            deletedItems.Append( dr[ "id" ] + "|" );
                        }
                        counter++;
                    }


                    //Apply the changes
                    iManager.ApplyChanges( pResourceId, userId, addedItems.ToString(), deletedItems.ToString() );
                }
                else
                {
                    foreach ( ListItem li in currentList.Items )
                    {
                        if ( li.Selected == true )
                        {
                            iManager.Insert( pResourceId, int.Parse( li.Value ), userId, ref statusMessage );
                        }
                        if ( !isValid )
                        {
                            statusMessage += messages + "<br/>";
                        }
                    }
                    if ( statusMessage.Length > 0 )
                    {
                        isValid = false;
                    }
                }

            }// end try
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".CheckBoxListUpdateItem(IResourceIntManager)" );
                isValid = false;
            }// end catch

            //refresh the saved code list
            DataSet ds2 = iManager.SelectedCodes( pResourceId );
            currentTarget = ds2;

            return ds2;
        }
        #endregion
    }
}