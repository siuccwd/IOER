using System;
using System.Collections.Generic;
using System.Text;

namespace LearningRegistryCache2.App_Code.Classes
{
  public class Submitter
  {
    public Submitter()
    {
    }

    private Guid _rowId;
    public Guid RowId
    {
      get { return this._rowId; }
      set { this._rowId = value; }
    }

    private string _submitterName;
    public string SubmitterName
    {
      get { return this._submitterName; }
      set { this._submitterName = value; }
    }

  }
}
