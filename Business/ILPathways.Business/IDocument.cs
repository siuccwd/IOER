using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    public interface IDocument
    {
        int Id { get; set; }
        Guid RowId { get; set; }

        string Title { get; set; }
        string FileName { get; set; }
        DateTime FileDate { get; set; }

        string FilePath { get; set; }
        string ResourceUrl { get; set; }
        long ResourceBytes { get; }
        byte[] ResourceData { get;  }
        int CreatedById { get; set; }
        void SetResourceData( long bytes, byte[] resourceData );
    }
}
