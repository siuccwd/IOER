using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourceStandardCollection : List<ResourceStandard>
    {
        public ResourceStandardCollection()
        {
        }

        public ResourceStandardCollection( int capacity )
        {
            this.Capacity = capacity;
        }

        public ResourceStandard Search( string property, string stringToFind )
        {
            string holdProperty = property.ToLower();
            if ( holdProperty != "resourceid" && holdProperty != "originalvalue" && holdProperty != "codeid" && holdProperty != "mappedvalue" && holdProperty != "alignmenttypecodeid" && holdProperty != "alignmenttypevalue" )
            {
                ArgumentException ex = new ArgumentException( "Invalid property.  Property must be one of: resourceId, originalValue, codeId, mappedValue, alignmentTypeCodeId, or alignmentTypeValue." );
                throw ex;
            }

            foreach ( ResourceStandard rp in this )
            {
                switch ( holdProperty )
                {
                    case "resourceid":
                        if ( rp.ResourceId.ToString() == stringToFind )
                        {
                            return rp;
                        }
                        break;
                    case "standardnotationcode":
                        if ( rp.StandardNotationCode == stringToFind )
                        {
                            return rp;
                        }
                        break;
                    case "standardid":
                        if ( rp.StandardId == int.Parse( stringToFind ) )
                        {
                            return rp;
                        }
                        break;
                    case "standardurl":
                        if ( rp.StandardUrl == stringToFind )
                        {
                            return rp;
                        }
                        break;
                    case "alignmenttypecodeid":
                        if ( rp.AlignmentTypeCodeId == int.Parse(stringToFind ) )
                        {
                            return rp;
                        }
                        break;
                    case "alignmenttypevalue":
                        if ( rp.AlignmentTypeValue == stringToFind )
                        {
                            return rp;
                        }
                        break;
                }
            } //foreach
            // not found
            return null;
        }

        public ResourceStandardCollection SearchAll( string property, string stringToFind )
        {
            string holdProperty = property.ToLower();
            ResourceStandardCollection collection = new ResourceStandardCollection( this.Count );

            if ( holdProperty != "resourceid" && holdProperty != "originalvalue" && holdProperty != "codeid" && holdProperty != "mappedvalue" && holdProperty != "alignmenttypecodeid" && holdProperty != "alignmenttypevalue" )
            {
                ArgumentException ex = new ArgumentException( "Invalid property.  Property must be one of: resourceId, originalValue, codeId, or mappedValue, alignmentTypeCodeId, or alignmentTypeValue." );
                throw ex;
            }

            foreach ( ResourceStandard rp in this )
            {
                switch ( holdProperty )
                {
                    case "resourceid":
                        if ( rp.ResourceId.ToString() == stringToFind )
                        {
                            collection.Add( rp );
                        }
                        break;
                    case "standardnotationcode":
                        if ( rp.StandardNotationCode == stringToFind )
                        {
                            collection.Add( rp );
                        }
                        break;
                    case "standardid":
                        if ( rp.StandardId == int.Parse( stringToFind ) )
                        {
                            collection.Add( rp );
                        }
                        break;
                    case "standardurl":
                        if ( rp.StandardUrl == stringToFind )
                        {
                            collection.Add( rp );
                        }
                        break;
                    case "alignmenttypecodeid":
                        if ( rp.AlignmentTypeCodeId == int.Parse( stringToFind ) )
                        {
                            collection.Add( rp );
                        }
                        break;
                    case "alignmenttypevalue":
                        if ( rp.AlignmentTypeValue == stringToFind )
                        {
                            collection.Add( rp );
                        }
                        break;
                }
            } //foreach
            if ( collection.Count > 0 )
            {
                return collection;
            }
            else
            {
                return null;
            }
        }

        public ResourceStandardCollection FindAllNew()
        {
            ResourceStandardCollection collection = new ResourceStandardCollection( this.Count );
            Guid defaultGuid = new Guid( "00000000-0000-0000-0000-000000000000" );

            foreach ( ResourceStandard entity in this )
            {
                if ( entity.RowId == null || entity.RowId == defaultGuid )
                {
                    collection.Add( entity );
                }
            }

            return collection;
        }
    }
}
