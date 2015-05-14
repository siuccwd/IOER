using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;

using ILPathways.Utilities;

namespace IoerContentBusinessEntities
{
    /// <summary>
    /// Instantiate IsleContent context withOUT lazy loading
    /// </summary>
    public class IsleContentContext : IsleContentEntities
    {
        public IsleContentContext()
        {
            this.Configuration.LazyLoadingEnabled = false;

            ///workaround to force EntityFramework.SqlServer to be included when publishing
            bool instanceExists = System.Data.Entity.SqlServer.SqlProviderServices.Instance != null;
        }


        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch ( DbEntityValidationException ex )
            {
                // Retrieve the error messages as a list of strings.
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany( x => x.ValidationErrors )
                        .Select( x => x.ErrorMessage );

                // Join the list to a single string.
                var fullErrorMessage = string.Join( "; ", errorMessages );

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat( ex.Message, " The validation errors are: ", fullErrorMessage );

                LoggingHelper.LogError( "IsleContentContext.SaveChanges(). " + exceptionMessage );

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException( exceptionMessage, ex.EntityValidationErrors );
            }
        }
    }
}
