using System;
using System.Collections.Generic;
using System.Text;

namespace LRWarehouse.Business
{
    /// <summary>
    /// Represents an object that describes a ResourceProperty
    /// </summary>
    [Serializable]
    public class ResourceProperty : BaseBusinessDataEntity
    {
        #region Properties
        private Guid _resourceId;
		public Guid ResourceId
		{
			get { return this._resourceId; }
			set { this._resourceId = value; }
		}

		/// <summary>
		/// future???
		/// </summary>
		private int _propertyTypeId;
		public int PropertyTypeId
		{
			get { return this._propertyTypeId; }
			set { this._propertyTypeId = value; }
		}

		public string Name
		{
			get { return this._propertyType; }
			set { this._propertyType = value; }
		}

		private string _value;
		public string Value
		{
			get { return this._value; }
			set { this._value = value; }
		}


        public DateTime Imported
        {
            get { return this.Created; }
            set { this.Created = value; }
        }

        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }

        #endregion

        #region External Properties

        private string _propertyType;
        public string PropertyType
        {
            get { return this._propertyType; }
            set { this._propertyType = value; }
        }

        #endregion
	}
}
