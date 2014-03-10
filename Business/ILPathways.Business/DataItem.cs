using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
/*
Modifications
05-08-09	mparsons - added _sortValue - used to hold a sort value for use with for example the Sorter class
08-02-08	mparsons - added code to allow for comparison sorting. For future use!
10-03-02	mparsons - moved from utilityClasses to bus library
*/
namespace ILPathways.Business
{
    /// <summary>
    /// Class for generic data item object
    /// </summary>
    [Serializable]
    public class DataItem : BaseBusinessDataEntity, System.IComparable
    {

        string _displayName = "";
        string _description = "";
        string _url = "";
        string _path = "";
        string _param1 = "";
        string _param2 = "";
        string _param3 = "";
        string _param4 = "";
        string _param5 = "";
        string _param6 = "";
        string _param7 = "";
        string _param8 = "";
        string _param9 = "";
        string _param10 = "";
        string _param11 = "";
        string _param12 = "";
        string _sortValue = "";

        string _mainChName = "";
        string _Sub1ChName = "";
        string _Sub2ChName = "";
        string _cssClass = "";
        string _cssClass2 = "";
        string _onclick = "";


        /// <summary>
        /// default constructor
        /// </summary>
        public DataItem()
        { }

        /// <summary>
        /// enum for sorting
        /// </summary>
        public enum ESortBy { param1, param2, param3, param4, param5, param6, param7, param8, param9 }

        /// <summary>
        /// Get/Set Title (displayName)
        /// </summary>
        public string Title
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        /// <summary>
        /// Get/Set description
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Get/Set ParentId
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// Get/Set Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Get/Set Language
        /// </summary>
        public string Language { get; set; }
        public bool IsSelected { get; set; } 
        public int Int1 { get; set; }
        public int Int2 { get; set; }
        public int Int3 { get; set; } 

        /// <summary>
        /// Get/Set url
        /// </summary>
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }
        /// <summary>
        /// Get/Set path
        /// </summary>
        public string path
        {
            get { return _path; }
            set { _path = value; }
        }
        /// <summary>
        /// Get/Set param1
        /// </summary>
        public string param1
        {
            get { return _param1; }
            set { _param1 = value; }
        }
        /// <summary>
        /// Get/Set param2
        /// </summary>
        public string param2
        {
            get { return _param2; }
            set { _param2 = value; }
        }
        /// <summary>
        /// Get/Set param3
        /// </summary>
        public string param3
        {
            get { return _param3; }
            set { _param3 = value; }
        }
        /// <summary>
        /// Get/Set param4
        /// </summary>
        public string param4
        {
            get { return _param4; }
            set { _param4 = value; }
        }
        /// <summary>
        /// Get/Set param5
        /// </summary>
        public string param5
        {
            get { return _param5; }
            set { _param5 = value; }
        }
        /// <summary>
        /// Get/Set param6
        /// </summary>
        public string param6
        {
            get { return _param6; }
            set { _param6 = value; }
        }
        /// <summary>
        /// Get/Set param7
        /// </summary>
        public string param7
        {
            get { return _param7; }
            set { _param7 = value; }
        }
        /// <summary>
        /// Get/Set param8
        /// </summary>
        public string param8
        {
            get { return _param8; }
            set { _param8 = value; }
        }
        /// <summary>
        /// Get/Set param9
        /// </summary>
        public string param9
        {
            get { return _param9; }
            set { _param9 = value; }
        }
        /// <summary>
        /// Get/Set param10
        /// </summary>
        public string param10
        {
            get { return _param10; }
            set { _param10 = value; }
        }
        /// <summary>
        /// Get/Set param11
        /// </summary>
        public string param11
        {
            get { return _param11; }
            set { _param11 = value; }
        }
        /// <summary>
        /// Get/Set param12
        /// </summary>
        public string param12
        {
            get { return _param12; }
            set { _param12 = value; }
        }
        /// <summary>
        /// Get/Set SortValue
        /// </summary>
        public string SortValue
        {
            get { return _sortValue; }
            set { _sortValue = value; }
        }
        /// <summary>
        /// Get/Set MainChName
        /// </summary>
        public string MainChName
        {
            get { return _mainChName; }
            set { _mainChName = value; }
        }
        /// <summary>
        /// Get/Set Sub1ChName
        /// </summary>
        public string Sub1ChName
        {
            get { return _Sub1ChName; }
            set { _Sub1ChName = value; }
        }
        /// <summary>
        /// Get/Set Sub2ChName
        /// </summary>
        public string Sub2ChName
        {
            get { return _Sub2ChName; }
            set { _Sub2ChName = value; }
        }
        /// <summary>
        /// Get/Set cssClass
        /// </summary>
        public string cssClass
        {
            get { return _cssClass; }
            set { _cssClass = value; }
        }
        /// <summary>
        /// Get/Set cssClass2
        /// </summary>
        public string cssClass2
        {
            get { return _cssClass2; }
            set { _cssClass2 = value; }
        }
        /// <summary>
        /// Get/Set OnClick
        /// </summary>
        public string OnClick
        {
            get { return _onclick; }
            set { _onclick = value; }
        }
        private static ESortBy m_SortBy = ESortBy.param1;
        /// <summary>
        /// Get/Set SortBy
        /// </summary>
        public static ESortBy SortBy
        {
            get { return m_SortBy; }
            set { m_SortBy = value; }
        }


        /// <summary>
        /// method to handle sorting a DataItem
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo( object obj )
        {
            switch ( SortBy )
            {
                case ESortBy.param1:
                    return this.param1.CompareTo( ( ( DataItem ) obj ).param1 );
                case ESortBy.param2:
                    return this.param2.CompareTo( ( ( DataItem ) obj ).param2 );
                case ESortBy.param3:
                    return this.param3.CompareTo( ( ( DataItem ) obj ).param3 );
                case ESortBy.param4:
                    return this.param4.CompareTo( ( ( DataItem ) obj ).param4 );
                case ESortBy.param5:
                    return this.param5.CompareTo( ( ( DataItem ) obj ).param5 );
                case ESortBy.param6:
                    return this.param6.CompareTo( ( ( DataItem ) obj ).param6 );
                case ESortBy.param7:
                    return this.param7.CompareTo( ( ( DataItem ) obj ).param7 );
                case ESortBy.param8:
                    return this.param8.CompareTo( ( ( DataItem ) obj ).param8 );
                case ESortBy.param9:
                    return this.param9.CompareTo( ( ( DataItem ) obj ).param9 );

                default:
                    return this.param1.CompareTo( ( ( DataItem ) obj ).param1 );
            }
        }

    }
}
