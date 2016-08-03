using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AutoMapper;
using LB = LRWarehouse.Business;
using ILPathways.Business;
using ILPathways.Utilities;
using Isle.DTO;

namespace IOERBusinessEntities
{
    public class AccountManager
    {
        static string thisClassName = "AccountManager";
		static string DEFAULT_GUID = "00000000-0000-0000-0000-000000000000";

		#region Account ============================
		public int Account_Add( LB.Patron entity, ref string statusMessage )
		{
			Patron efEntity = new Patron();
			using ( var context = new ResourceContext() )
			{
				try
				{
					
					if ( string.IsNullOrWhiteSpace( entity.UserName))
						entity.UserName = entity.Email;

					efEntity.UserName = entity.UserName;
					//assuming encrypted by now?
					efEntity.Password = entity.Password;
					efEntity.IsActive = entity.IsActive;

					efEntity.FirstName = entity.FirstName;
					efEntity.LastName = entity.LastName;
					efEntity.Email = entity.Email;
					if (!string.IsNullOrWhiteSpace(entity.ExternalIdentifier))
						efEntity.ExternalIdentifier = entity.ExternalIdentifier;
					else
						efEntity.ExternalIdentifier = null;

					if ( efEntity.RowId== null || efEntity.RowId.ToString() == DEFAULT_GUID )
						efEntity.RowId = Guid.NewGuid();
					efEntity.LastUpdatedById = entity.LastUpdatedById;
					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Patrons.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;
						return efEntity.Id;
					}
					else
					{
						//?no info on error
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = thisClassName + string.Format( ".Account_Add() DbEntityValidationException, Email:{0}, person: {1}", entity.Email, entity.FullName() );
					foreach ( var eve in dbex.EntityValidationErrors )
					{
						message += string.Format( "\rEntity of type \"{0}\" in state \"{1}\" has the following validation errors:",
							eve.Entry.Entity.GetType().Name, eve.Entry.State );
						foreach ( var ve in eve.ValidationErrors )
						{
							message += string.Format( "- Property: \"{0}\", Error: \"{1}\"",
								ve.PropertyName, ve.ErrorMessage );
						}

						LoggingHelper.LogError( message, true );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_Add(), Email:{0}, person: {1}", entity.Email, entity.FullName() )) ;
				}

				return 0;
			}
		}

		public static LB.Patron Account_GetByExternalIdentifier( string identifier )
		{
			LB.Patron entity = new LB.Patron();

			using ( var context = new ResourceContext() )
			{
				Patron item = context.Patrons.SingleOrDefault( s => s.ExternalIdentifier == identifier);
				if (item != null && item.Id > 0)
					Account_ToMap( item, entity );
			}

			return entity;

		}//
		private static void Account_ToMap( Patron from, LB.Patron to )
		{
			to.UserName = from.UserName;
			//assuming encrypted by now?
			to.Password = from.Password;
			to.IsActive = from.IsActive ?? false;

			to.FirstName = from.FirstName;
			to.LastName = from.LastName;
			to.Email = from.Email;
			to.ExternalIdentifier = from.ExternalIdentifier;
			to.RowId = from.RowId;
			to.LastUpdatedById = from.LastUpdatedById ?? 0;
			if ( from.Created != null)
				to.Created = (DateTime) from.Created;
			if ( from.LastUpdated != null )
				to.LastUpdated = ( DateTime ) from.LastUpdated;

			if ( from.Patron_Profile != null )
			{
				to.UserProfile = new LB.PatronProfile();
				to.UserProfile.IsValid = true;

				to.UserProfile.UserId = from.Id;
				to.UserProfile.JobTitle = from.Patron_Profile.JobTitle;

				to.UserProfile.OrganizationId = from.Patron_Profile.OrganizationId ?? 0;
				//to.UserProfile.Organization = GetRowColumn( dr, "Organization", "" );

				to.UserProfile.PublishingRoleId = from.Patron_Profile.PublishingRoleId ?? 0;
				//to.UserProfile.PublishingRole = GetRowColumn( dr, "PublishingRole", "" );
				to.UserProfile.ImageUrl = from.Patron_Profile.ImageUrl ;
				to.UserProfile.RoleProfile = from.Patron_Profile.RoleProfile ;
				to.UserProfile.Created = from.Patron_Profile.Created ;
				to.UserProfile.CreatedById = from.Patron_Profile.CreatedById ?? 0;
				to.UserProfile.LastUpdated = from.Patron_Profile.LastUpdated;
				to.UserProfile.LastUpdatedById = from.Patron_Profile.LastUpdatedId ?? 0;
			}
		}
		#endregion

		#region Community Members =======================
		public static int PersonFollowing_Add( int pFollowingUserId, int FollowedByUserId )
        {

            using ( var context = new ResourceContext() )
            {
                Patron_Following entity = new Patron_Following();
                entity.FollowingUserId = pFollowingUserId;
                entity.FollowedByUserId = FollowedByUserId;
                entity.Created = System.DateTime.Now;

                try
                {
                    context.Patron_Following.Add( entity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        return entity.Id;
                    }
                    else
                    {
                        //?no info on error
                        return 0;
                    }
                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + ".PersonFollowing_Add()" );
                    return 0;
                }
            }
        }

        public static bool PersonFollowing_Delete( int FollowingUserId, int FollowedByUserId )
        {
            bool isSuccessful = false;

            using ( var context = new ResourceContext() )
            {
                Patron_Following item = context.Patron_Following.SingleOrDefault( s => s.FollowingUserId == FollowingUserId && s.FollowedByUserId == FollowedByUserId );

                if ( item != null && item.Id > 0 )
                {
                    context.Patron_Following.Remove( item );
                    context.SaveChanges();
                    isSuccessful = true;
                }
            }
            return isSuccessful;
        }
        public static bool PersonFollowing_IsMember( int pFollowingUserId, int FollowedByUserId )
        {

            bool isMember = false;
            using ( var context = new ResourceContext() )
            {
                Patron_Following item = context.Patron_Following.SingleOrDefault( s => s.FollowingUserId == pFollowingUserId && s.FollowedByUserId == FollowedByUserId );

                if ( item != null && item.Id > 0 )
                {
                    isMember = true;
                }
            }
            return isMember;
        }

        #endregion

    }
}
