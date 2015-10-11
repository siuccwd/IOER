using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using Entity = LearningRegistryCache2.App_Code.Classes.Submitter;

namespace LearningRegistryCache2.App_Code.DataManagers
{
  
  public class SubmitterManager:BaseDataManager
  {
    public SubmitterManager()
    {
    }

    public Entity Get(Guid rowId, string submitterName, ref string status)
    {
      status = "successful";
      try
      {
        SqlParameter[] arParms = new SqlParameter[2];
        arParms[0] = new SqlParameter("@RowId", SqlDbType.UniqueIdentifier);
        arParms[0].Value = rowId;
        arParms[1] = new SqlParameter("@SubmitterName", SqlDbType.VarChar);
        arParms[1].Size = 100;
        arParms[1].Value = submitterName;

        DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "SubmitterGet", arParms);
        if (DoesDataSetHaveRows(ds))
        {
          Entity entity = Fill(ds);
          return entity;
        }
        else
        {
          return null;
        }
      }
      catch (Exception ex)
      {
        LogError(ex, "SubmitterManager.Get(): ");
        status = ex.Message;
        return null;
      }
    }

    public Guid Create(string submitterName, ref string status)
    {
      status = "successful";
      try
      {
        SqlParameter[] arParms = new SqlParameter[1];
        arParms[0] = new SqlParameter("@SubmitterName", SqlDbType.VarChar);
        arParms[0].Size = 100;
        arParms[0].Value = submitterName;

        DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "SubmitterInsert", arParms);
        if (DoesDataSetHaveRows(ds))
        {
          Guid retVal = new Guid(GetRowColumn(ds.Tables[0].Rows[0], "RowId", DEFAULT_GUID));
          return retVal;
        }
        else
        {
          return new Guid(DEFAULT_GUID);
        }
      }
      catch (Exception ex)
      {
        LogError(ex, "SubmitterManager.Create(): ");
        status = ex.Message;
        return new Guid(DEFAULT_GUID);
      }
    }

    public Entity Fill(DataSet ds)
    {
      Entity entity = new Entity();
      entity.RowId = new Guid(GetRowColumn(ds.Tables[0].Rows[0], "RowId", DEFAULT_GUID));
      entity.SubmitterName = GetRowColumn(ds.Tables[0].Rows[0], "SubmitterName", "");

      return entity;
    }

  }
}
