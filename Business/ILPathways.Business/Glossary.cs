using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class Glossary : BaseBusinessDataEntity
    {
        ///<summary>
        ///Initializes a new instance of the ILPathways.Business.Glossary class.
        ///</summary>
        public Glossary() { }

        #region Properties created from dictionary for Glossary
        private string _term = "";
        /// <summary>
        /// Gets/Sets Term
        /// </summary>
        public string Term
        {
            get
            {
                return this._term;
            }
            set
            {
                if ( this._term == value )
                {
                    //Ignore set
                }
                else
                {
                    this._term = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _description = "";
        /// <summary>
        /// Gets/Sets Description
        /// </summary>
        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                if ( this._description == value )
                {
                    //Ignore set
                }
                else
                {
                    this._description = value.Trim();
                    HasChanged = true;
                }
            }
        }


        private string _library = "";
        /// <summary>
        /// Gets/Sets Library. The library is used to partition domain or program related libraries 
        /// from each other.
        /// TODO - probably need to 
        /// </summary>
        public string Library
        {
            get
            {
                return this._library;
            }
            set
            {
                if ( this._library == value )
                {
                    //Ignore set
                }
                else
                {
                    this._library = value.Trim();
                    HasChanged = true;
                }
            }
        }


        private bool _isAlias = false;
        /// <summary>
        /// Gets/Sets IsAlias - if true, this term is an alias for another term
        /// </summary>
        public bool IsAlias
        {
            get
            {
                return this._isAlias;
            }
            set
            {
                if ( this._isAlias == value )
                {
                    //Ignore set
                }
                else
                {
                    this._isAlias = value;
                    HasChanged = true;
                }
            }
        }

        private bool _isAcronym = false;
        /// <summary>
        /// Gets/Sets IsAcronym - if true, this term is an acronym for another term
        /// </summary>
        public bool IsAcronym
        {
            get
            {
                return this._isAcronym;
            }
            set
            {
                if ( this._isAcronym == value )
                {
                    //Ignore set
                }
                else
                {
                    this._isAcronym = value;
                    HasChanged = true;
                }
            }
        }

        private int _parentTermId;
        /// <summary>
        /// Gets/Sets the ParentTermId
        /// </summary>
        public int ParentTermId
        {
            get
            {
                return this._parentTermId;
            }
            set
            {
                if ( this._parentTermId != value )
                {
                    this._parentTermId = value;
                    HasChanged = true;
                }
            }
        }//
        #endregion

        #region External properties
        private string _parentTerm = "";
        /// <summary>
        /// Gets/Sets ParentTerm - blank unless this term is an alias
        /// </summary>
        public string ParentTerm
        {
            get
            {
                return this._parentTerm;
            }
            set
            {
                this._parentTerm = value.Trim();
            }
        }

        private string _parentDescription = "";
        /// <summary>
        /// Gets/Sets ParentDescription
        /// </summary>
        public string ParentDescription
        {
            get
            {
                return this._parentDescription;
            }
            set
            {
                this._parentDescription = value.Trim();
            }
        }

        /// <summary>
        /// Return public version of term description, with handling for an alias
        /// </summary>
        public string TermDefinition
        {
            get
            {
                if ( IsAlias == true )
                {
                    string display = ParentTerm + "<br/>" + ParentDescription;
                    if ( display.Length > 0 )
                        return display;
                    else
                        return this.Description;

                }
                else
                {
                    return this.Description;
                }
            }
        }
        #endregion

    } // end class 
}