using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class StandardSubject : BaseBusinessDataEntity
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public StandardSubject()
        {
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Language { get; set; }

        public string EducationLevelStart { get; set; }
        public string EducationLevelEnd { get; set; }

        /// <summary>
        /// Get/Set StandardBodyId
        /// </summary>
        public int StandardBodyId { get; set; }

    } //

    public class Standard_SubjectStandardConnector : BaseBusinessDataEntity
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public Standard_SubjectStandardConnector()
        {
        }
        /// <summary>
        /// Get/Set StandardSubjectId
        /// </summary>
        public int StandardSubjectId { get; set; }

        /// <summary>
        /// Get/Set DomainNodeId
        /// </summary>
        public int DomainNodeId { get; set; }

    }
}
