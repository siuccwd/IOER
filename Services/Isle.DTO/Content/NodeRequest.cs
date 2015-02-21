using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
    public class NodeRequest
    {

        public int ContentId { get; set; }

        public bool IsEditView { get; set; }
        public bool DoCompleteFill { get; set; }
        public bool AllowCaching { get; set; }

        public bool IncludeStandards { get; set; }

        public bool IncludeChildNodes { get; set; }
        public bool IncludeConnectorNodes { get; set; }
        public bool IncludeDocumentNodes { get; set; }

    }
}
