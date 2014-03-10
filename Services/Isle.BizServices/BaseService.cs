using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Isle.DataContracts;
//using IllinoisPathways.Framework.Exception;
//using IllinoisPathways.Framework.Logging;
using LRWarehouse.Business;
using ILPathways.Business;

namespace Isle.BusinessServices
{
    public abstract class BaseService
    {
        ErrorDataContract error = default( ErrorDataContract);

        public ErrorDataContract ManageExceptionDataContract(Exception exception)
        {
            ExceptionLogBEO exceptionLogBEO = new ExceptionLogBEO();

            //if (exception.GetBaseException() is TheCommunityNetException)
            //{
            //    error = new ErrorDataContract { MessageId = ((TheCommunityNetException)exception.GetBaseException()).MessageId, Message = ((TheCommunityNetException)exception.GetBaseException()).ErrorMessage };
            //    exceptionLogBEO.MessageId = error.MessageId;
            //    exceptionLogBEO.MessageText = error.Message;
            //    if (((TheCommunityNetException)exception.GetBaseException()).RootException != null)
            //    {
            //        exceptionLogBEO.ExceptionMessage = ((TheCommunityNetException)exception.GetBaseException()).RootException.Message;
            //    }
            //    exceptionLogBEO.StackTrace = ((TheCommunityNetException)exception.GetBaseException()).StackTrace;
            //}
            //else
            //{
            error = new ErrorDataContract { Message = exception.Message };
            exceptionLogBEO.ExceptionMessage = exception.Message;
            exceptionLogBEO.StackTrace = exception.StackTrace;
            //}


            LogManager.LogException(exceptionLogBEO);
            return error;
        }

        
    }
}
