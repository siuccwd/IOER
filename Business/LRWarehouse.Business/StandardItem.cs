using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class StandardItem : BaseBusinessDataEntity
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public StandardItem()
        {
            EducationLevels = new ArrayList();
        }

        /// <summary>
        /// Get/Set ParentId
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// Get/Set LevelType:
        /// - domain, cluster, standard, component
        /// </summary>
        public string LevelType { get; set; }


        public string Description { get; set; }

        public string Language { get; set; }

        /// <summary>
        /// Standard Url
        /// </summary>
        public string StandardUrl { get; set; }

        public string NotationCode { get; set; }
        /// <summary>
        /// Get/Set GradeLevels
        /// Grade levels are inline for NGSS
        /// </summary>
        public string GradeLevels { get; set; }

        public string EducationLevelStart { get; set; }
        public string EducationLevelEnd { get; set; }
        public ArrayList EducationLevels { get; set; }


        /// <summary>
        /// Standard Guid
        /// <skos:exactMatch rdf:resource="urn:guid:C9B1E6F9029B46BF91FA3D7AD3424D81" />
        /// </summary>
        public string StandardGuid { get; set; }


        //??????????????
        public string Title { get; set; }
        public string Subject { get; set; }
        public string AltUrl { get; set; }
    }
}
