using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class ResourceDataContract : BaseContract
    {

        [DataMember]
        public int ResourceIntId { get; set; }
        //[DataMember]
        //public Guid ResourceId { get; set; }

        [DataMember]
        public string ResourceUrl { get; set; }
        //[DataMember]
        //public Guid ResourceVersionId { get; set; }
        [DataMember]
        public int ResourceVersionIntId { get; set; }


        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }       
        [DataMember]
        public string Publisher { get; set; }
        [DataMember]
        public string Creator { get; set; }
        [DataMember]
        public string Rights { get; set; }
        [DataMember]
        public string AccessRights { get; set; }
        [DataMember]
        public int AccessRightsId { get; set; }

        [DataMember]
        public int LikeCount { get; set; }
        [DataMember]
        public int DislikeCount { get; set; }

        [DataMember]
        public string Submitter { get; set; }
        [DataMember]
        public string TypicalLearningTime { get; set; }
        [DataMember]
        public DateTime Modified { get; set; }
        [DataMember]
        public DateTime Imported { get; set; }

        //[DataMember]
        //public string LRDocId { get; set; }

        //optional
        [DataMember]
        public string Subjects { get; set; }
        [DataMember]
        public string EducationLevels { get; set; }
        [DataMember]
        public string ResourceTypesList { get; set; }
        [DataMember]
        public string Keywords { get; set; }
        [DataMember]
        public string LanguageList { get; set; }
    }
}
