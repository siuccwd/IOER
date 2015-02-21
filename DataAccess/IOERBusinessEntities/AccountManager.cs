using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AutoMapper;
using LB = LRWarehouse.Business;
using ILPathways.Utilities;
using Isle.DTO;

namespace IOERBusinessEntities
{
    public class AccountManager
    {
        static string thisClassName = "AccountManager";

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
