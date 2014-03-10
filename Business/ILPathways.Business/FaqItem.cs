using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
     ///<summary>
    ///Represents an object that describes a FaqItem which is a subclass of AppItem
    ///</summary>
    [Serializable]
    public class FaqItem : AppItem
    {

        ///<summary>
        ///Initializes a new instance of the workNet.BusObj.Entity.FaqItem class.
        ///</summary>
        public FaqItem()
        {
            this.TypeId = FAQItemType;

        }

        #region Properties
        private int _pathwayId;
        /// <summary>
        /// Gets/Sets PathwayId ==> OBSOLETE
        /// </summary>
        public int PathwayId
        {
            get
            {
                return this._pathwayId;
            }
            set
            {
                if ( this._pathwayId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._pathwayId = value;
                    HasChanged = true;
                }
            }
        }

        private int _categoryId;
        /// <summary>
        /// Gets/Sets CategoryId
        /// </summary>
        public int CategoryId
        {
            get
            {
                return this._categoryId;
            }
            set
            {
                if ( this._categoryId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._categoryId = value;
                    HasChanged = true;
                }
            }
        }

        private int _subcategoryId;
        /// <summary>
        /// Gets/Sets SubcategoryId
        /// </summary>
        public int SubcategoryId
        {
            get
            {
                return this._subcategoryId;
            }
            set
            {
                if ( this._subcategoryId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._subcategoryId = value;
                    HasChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets/Sets FaqCode
        /// </summary>
        public string FaqCode
        {
            get
            {
                return this.AppItemCode;
            }
            set
            {
                if ( this.AppItemCode == value )
                {
                    //Ignore set
                }
                else
                {
                    this.AppItemCode = value.Trim();
                    HasChanged = true;
                }
            }
        }

        /// <summary>
        /// temp - will be deleted, once new Faq is in place
        /// </summary>
        public string ItemCode
        {
            get
            {
                return this.AppItemCode;
            }
            set
            {
                if ( this.AppItemCode == value )
                {
                    //Ignore set
                }
                else
                {
                    this.AppItemCode = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private int _hits;
        /// <summary>
        /// Gets/Sets Hits
        /// </summary>
        public int Hits
        {
            get
            {
                return this._hits;
            }
            set
            {
                if ( this._hits == value )
                {
                    //Ignore set
                }
                else
                {
                    this._hits = value;
                    HasChanged = true;
                }
            }
        }



        #endregion
        #region Joined Properties
        private string _sitePathName = "";
        /// <summary>
        /// Gets/Sets SitePathName
        /// </summary>
        public string SitePathName
        {
            get
            {
                return this._sitePathName;
            }
            set
            {
                if ( this._sitePathName == value )
                {
                    //Ignore set
                }
                else
                {
                    this._sitePathName = value.Trim();
                }
            }
        }//
        #endregion
    }
}
