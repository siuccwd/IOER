
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using LRWarehouse.Business;
using Microsoft.ApplicationBlocks.Data;

namespace LRWarehouse.DAL
{
    public class ResourceAgeRangeManager : BaseDataManager
    {
        const string className = "ResourceAgeRangeManager";
        const string IMPORT_PROC = "[Resource.AgeRangeImport]";
        const string GETBYINTID_PROC = "[Resource.AgeRangeGetByIntId]";
        public string Import(ResourceAgeRange ageRange)
        {
            string retVal = "successful";

            try
            {
                #region parameters
                SqlParameter[] parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@ResourceIntId", ageRange.ResourceIntId);
                parameters[1] = new SqlParameter("@iFromAge", ageRange.FromAge);
                parameters[2] = new SqlParameter("@iToAge", ageRange.ToAge);
                parameters[3] = new SqlParameter("@iOriginalValue", SqlDbType.VarChar);
                parameters[3].Size = 50;
                parameters[3].Value = ageRange.OriginalValue;
                #endregion
                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, IMPORT_PROC, parameters);
            }
            catch (Exception ex)
            {
                LogError(className + ".Import(): " + ex.ToString());
                retVal = ex.Message;
            }

            return retVal;
        }

        public ResourceAgeRange GetByIntId(int resourceIntId)
        {
            try
            {
                #region parameters
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@ResourceIntId", resourceIntId);
                #endregion
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, GETBYINTID_PROC, parameters);
                if (DoesDataSetHaveRows(ds))
                {
                    return Fill(ds.Tables[0].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".GetByIntId(): " + ex.ToString());
            }
            return new ResourceAgeRange();
        }

        private ResourceAgeRange Fill(DataRow dr)
        {
            ResourceAgeRange entity = new ResourceAgeRange();
            entity.Id = GetRowColumn(dr, "Id", 0);
            entity.ResourceIntId = GetRowColumn(dr, "ResourceIntId", 0);
            entity.FromAge = GetRowColumn(dr, "FromAge", 0);
            entity.ToAge = GetRowColumn(dr, "ToAge", 0);
            entity.OriginalValue = GetRowColumn(dr, "OriginalLevel", "");
            entity.Created = GetRowColumn(dr, "Created", DateTime.Now);
            entity.CreatedById = GetRowColumn(dr, "CreatedById", 0);

            return entity;
        }
    }
}
