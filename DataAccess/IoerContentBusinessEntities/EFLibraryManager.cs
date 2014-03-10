using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using AutoMapper;

using EF_LM = IoerContentBusinessEntities.Library_Member;
using EF_LSM = IoerContentBusinessEntities.Library_SectionMember;
using ILPathways.Business;

namespace IoerContentBusinessEntities
{
    public class EFLibraryManager
    {
        static IsleContentEntities ctx = new IsleContentEntities();
        public EFLibraryManager() { }

        #region     Library Member 
        public static int LibraryMember_Create( int libraryId, int userId, int memberTypeId, int createdById, ref string statusMessage )
        {

            Library_Member entity = new Library_Member();
            entity.LibraryId = libraryId;
            entity.UserId = userId;
            entity.MemberTypeId = memberTypeId;
            entity.CreatedById = createdById;
            entity.Created = System.DateTime.Now;
            entity.LastUpdatedById = createdById;
            entity.LastUpdated = System.DateTime.Now;
            entity.RowId = Guid.NewGuid();

            ctx.Library_Member.Add( entity );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                statusMessage = "Successful";
                return entity.Id;
            }
            else
            {
                statusMessage = "Error - LibraryMember_Create failed";
                //?no info on error
                return 0;
            }
        }
        public static bool LibraryMember_Update( int id, int memberTypeId, int updatedById )
        {
            bool action = false;

            Library_Member entity = ctx.Library_Member.SingleOrDefault( s => s.Id == id );
            if ( entity != null )
            {
                entity.MemberTypeId = memberTypeId;
                entity.LastUpdatedById = updatedById;
                entity.LastUpdated = System.DateTime.Now;
                ctx.SaveChanges();

                action = true;
            }

            return action;
        }
        public static bool LibraryMember_Update( int libraryId, int userId, int memberTypeId, int updatedById )
        {
            bool action = false;

            Library_Member entity = ctx.Library_Member.SingleOrDefault( s => s.LibraryId == libraryId && s.UserId == userId );
            if ( entity != null )
            {
                entity.MemberTypeId = memberTypeId;
                entity.LastUpdatedById = updatedById;
                entity.LastUpdated = System.DateTime.Now;
                ctx.SaveChanges();

                action = true;
            }

            return action;
        }
        public static bool LibraryMember_Delete( int id )
        {
            bool action = false;
            Library_Member entity = ctx.Library_Member.SingleOrDefault( s => s.Id == id );
            if ( entity != null )
            {
                ctx.Library_Member.Remove( entity );
                ctx.SaveChanges();

                action = true;
            }
  
            return action;
        }

        public static LibraryMember LibraryMember_Get( int id )
        {
            LibraryMember entity = new LibraryMember();

            Library_Member mbr = ctx.Library_Member.SingleOrDefault( s => s.Id == id);
            if ( mbr != null && mbr.Id > 0 )
            {
                entity = LibraryMember_ToMap( mbr );
            }

            return entity;
        }
        public static LibraryMember LibraryMember_Get( int libraryId, int userId )
        {
            LibraryMember entity = new LibraryMember();

            Library_Member mbr = ctx.Library_Member.SingleOrDefault( s => s.LibraryId == libraryId && s.UserId == userId );
            if ( mbr != null && mbr.Id > 0 )
            {
                entity = LibraryMember_ToMap( mbr );
            }

            return entity;
        }

        public static List<LibraryMember> LibraryMembers_ForLibrary( int libraryId )
        {
            LibraryMember entity = new LibraryMember();

            List<Library_Member> mbrs = ctx.Library_Member
                                .Where( s => s.LibraryId == libraryId )
                                .ToList();

            List<LibraryMember> list = new List<LibraryMember>();
            if ( mbrs.Count > 0 )
            {
                foreach ( Library_Member item in mbrs )
                {
                    entity = new LibraryMember();
                    entity = LibraryMember_ToMap( item );
                    list.Add( entity );

                }
            }
  
            return list;
        }

        public static Library_Member LibraryMember_FromMap( LibraryMember fromEntity )
        {
            //Mapper.CreateMap<LibraryMember, Library_Member>()
            //    .ForMember( dest => dest.LibraryId, opt => opt.MapFrom( src => src.ParentId ) );

            //Library_Member toEntity = Mapper.Map<LibraryMember, Library_Member>( fromEntity );

            Library_Member to = new Library_Member();
            to.Id = fromEntity.Id;
            to.LibraryId = fromEntity.ParentId;
            to.UserId = fromEntity.UserId;
            to.MemberTypeId = fromEntity.MemberTypeId;

            to.Created = fromEntity.Created;
            to.CreatedById = ( int ) fromEntity.CreatedById;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;
            to.RowId = ( Guid ) fromEntity.RowId;

            return to;
        }

        public static LibraryMember LibraryMember_ToMap( Library_Member fromEntity )
        {
            
            //Mapper.CreateMap<Library_Member, LibraryMember>()
            //    .ForMember( dest => dest.ParentId, opt => opt.MapFrom( src => src.LibraryId ) );
            //LibraryMember to = Mapper.Map<Library_Member, LibraryMember>( fromEntity );

            LibraryMember to = new LibraryMember();
            to.Id = fromEntity.Id;
            to.ParentId = fromEntity.LibraryId;
            to.UserId = fromEntity.UserId;
            to.MemberTypeId = fromEntity.MemberTypeId;

            to.Created = fromEntity.Created;
            to.CreatedById = (int) fromEntity.CreatedById;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;
            to.RowId = (Guid) fromEntity.RowId;

            return to;
        }

        #endregion

        #region     Library Invitation
        public static int LibraryInvitationCreate( LibraryInvitation entity, ref string statusMessage )
        {

            Library_Invitation ef = LibraryInvitation_FromMap( entity );

            ctx.Library_Invitation.Add( ef );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                statusMessage = "Successful";
                return ef.Id;
            }
            else
            {
                statusMessage = "Error - LibraryInvitation_Create failed";
                //?no info on error
                return 0;
            }
        }
        public static bool LibraryInvitation_Update( LibraryInvitation entity )
        {
            bool action = false;

            Library_Invitation ef = ctx.Library_Invitation.SingleOrDefault( s => s.Id == entity.Id );
            if ( entity != null )
            {
                //hmmm - the passed entity is not likely to be filled eagar? - or make it in these cases
                ef = LibraryInvitation_FromMap( entity );
                ef.LastUpdated = System.DateTime.Now;

                ctx.SaveChanges();

                action = true;
            }

            return action;
        }

        public static bool LibraryInvitation_Delete( int id )
        {
            bool action = false;
            Library_Invitation entity = ctx.Library_Invitation.SingleOrDefault( s => s.Id == id );
            if ( entity != null )
            {
                ctx.Library_Invitation.Remove( entity );
                ctx.SaveChanges();

                action = true;
            }

            return action;
        }

        public static LibraryInvitation LibraryInvitation_Get( int id )
        {
            LibraryInvitation entity = new LibraryInvitation();

            Library_Invitation mbr = ctx.Library_Invitation.SingleOrDefault( s => s.Id == id );
            if ( mbr != null && mbr.Id > 0 )
            {
                entity = LibraryInvitation_ToMap( mbr );
            }

            return entity;
        }

        public static LibraryInvitation LibraryInvitation_Get( int libraryId, int userId )
        {
            LibraryInvitation entity = new LibraryInvitation();

            Library_Invitation mbr = ctx.Library_Invitation.SingleOrDefault( s => s.LibraryId == libraryId && s.TargetUserId == userId );
            if ( mbr != null && mbr.Id > 0 )
            {
                entity = LibraryInvitation_ToMap( mbr );
            }

            return entity;
        }
        public static LibraryInvitation LibraryInvitation_Get( Guid rowId )
        {
            LibraryInvitation entity = new LibraryInvitation();

            Library_Invitation mbr = ctx.Library_Invitation.SingleOrDefault( s => s.RowId == rowId );
            if ( mbr != null && mbr.Id > 0 )
            {
                entity = LibraryInvitation_ToMap( mbr );
            }

            return entity;
        }
        public static LibraryInvitation LibraryInvitation_GetByPasscode( string passcode )
        {
            LibraryInvitation entity = new LibraryInvitation();

            Library_Invitation mbr = ctx.Library_Invitation.SingleOrDefault( s => s.PassCode == passcode );
            if ( entity != null && entity.Id > 0 )
            {
                entity = LibraryInvitation_ToMap( mbr );
            }

            return entity;
        }
        public static Library_Invitation LibraryInvitation_FromMap( LibraryInvitation fromEntity )
        {
            //Mapper.CreateMap<LibraryInvitation, Library_Invitation>();
            //Library_Invitation toEntity = Mapper.Map<LibraryInvitation, Library_Invitation>( fromEntity );

            Library_Invitation toEntity = new Library_Invitation();
            toEntity.LibraryId = fromEntity.ParentId;
            toEntity.InvitationType = fromEntity.InvitationType;
            toEntity.TargetEmail = fromEntity.TargetEmail;
            toEntity.LibMemberTypeId = (int) fromEntity.LibMemberTypeId;
            toEntity.StartingUrl = fromEntity.StartingUrl;
            toEntity.IsActive = fromEntity.IsActive;
            toEntity.TargetUserId = fromEntity.TargetUserId;
            toEntity.Subject = fromEntity.Subject;
            toEntity.ExpiryDate = fromEntity.ExpiryDate;
            if ( fromEntity.IsValidRowId( fromEntity.RowId ) )
                toEntity.RowId = fromEntity.RowId;
            else
                toEntity.RowId = Guid.NewGuid();

            toEntity.CreatedById = fromEntity.CreatedById;
            toEntity.Created = fromEntity.Created;
            toEntity.LastUpdated = fromEntity.LastUpdated;
            toEntity.LastUpdatedById = fromEntity.LastUpdatedById;

            return toEntity;
        }

        public static LibraryInvitation LibraryInvitation_ToMap( Library_Invitation fromEntity )
        {

            //Mapper.CreateMap<Library_Invitation, LibraryInvitation>();
            //LibraryInvitation toEntity = Mapper.Map<Library_Invitation, LibraryInvitation>( fromEntity );

            LibraryInvitation toEntity = new LibraryInvitation();
            toEntity.ParentId = (int)  fromEntity.LibraryId;
            toEntity.InvitationType = toEntity.InvitationType;
            toEntity.TargetEmail = fromEntity.TargetEmail;
            toEntity.LibMemberTypeId = (int) fromEntity.LibMemberTypeId;
            toEntity.StartingUrl = fromEntity.StartingUrl;
            toEntity.IsActive = (bool) fromEntity.IsActive;
            toEntity.TargetUserId = (int) fromEntity.TargetUserId;
            toEntity.Subject = fromEntity.Subject;
            toEntity.ExpiryDate = (System.DateTime) fromEntity.ExpiryDate;
            if ( toEntity.IsValidRowId( fromEntity.RowId ) )
                toEntity.RowId = fromEntity.RowId;
            else
                toEntity.RowId = Guid.NewGuid();

            toEntity.CreatedById = fromEntity.CreatedById;
            toEntity.Created = ( System.DateTime ) fromEntity.Created;
            toEntity.LastUpdated = ( System.DateTime ) fromEntity.LastUpdated;
            toEntity.LastUpdatedById = (int) fromEntity.LastUpdatedById;

            return toEntity;
        }

        #endregion

        #region     Library SectionMember
        public static int LibrarySectionMember_Create( int parentId, int userId, int memberTypeId, int createdById, ref string statusMessage )
        {

            Library_SectionMember entity = new Library_SectionMember();
            entity.LibrarySectionId = parentId;
            entity.UserId = userId;
            entity.MemberTypeId = memberTypeId;
            entity.CreatedById = createdById;
            entity.Created = System.DateTime.Now;
            entity.LastUpdatedById = createdById;
            entity.LastUpdated = System.DateTime.Now;
            entity.RowId = Guid.NewGuid();

            ctx.Library_SectionMember.Add( entity );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                return entity.Id;
            }
            else
            {
                statusMessage = "Error - LibrarySectionMember_Create failed";
                //?no info on error
                return 0;
            }
        }
        public static bool LibrarySectionMember_Update( int id, int memberTypeId, int updatedById )
        {
            bool action = false;

            Library_SectionMember entity = ctx.Library_SectionMember.SingleOrDefault( s => s.Id == id );
            if ( entity != null )
            {
                entity.MemberTypeId = memberTypeId;
                entity.LastUpdatedById = updatedById;
                entity.LastUpdated = System.DateTime.Now;
                ctx.SaveChanges();

                action = true;
            }

            return action;
        }
        public static bool LibrarySectionMember_Update( int parentId, int userId, int memberTypeId, int updatedById )
        {
            bool action = false;

            Library_SectionMember entity = ctx.Library_SectionMember.SingleOrDefault( s => s.LibrarySectionId == parentId && s.UserId == userId );
            if ( entity != null )
            {
                entity.MemberTypeId = memberTypeId;
                entity.LastUpdatedById = updatedById;
                entity.LastUpdated = System.DateTime.Now;
                ctx.SaveChanges();

                action = true;
            }

            return action;
        }
        public static bool LibrarySectionMember_Delete( int id )
        {
            bool action = false;
            Library_SectionMember entity = ctx.Library_SectionMember.SingleOrDefault( s => s.Id == id );
            if ( entity != null )
            {
                ctx.Library_SectionMember.Remove( entity );
                ctx.SaveChanges();

                action = true;
            }

            return action;
        }

        public static LibraryMember LibrarySectionMember_Get( int id )
        {
            LibraryMember entity = new LibraryMember();

            EF_LSM mbr = ctx.Library_SectionMember.SingleOrDefault( s => s.Id == id );
            if ( mbr != null && mbr.Id > 0 )
            {
                entity = LibrarySectionMember_ToMap( mbr );
            }

            return entity;
        }

        public static LibraryMember LibrarySectionMember_Get( int libraryId, int userId )
        {
            // Load one blogs and its related posts 
            //var blog1 = ctx.Library_SectionMembers
            //                    .Where( b => b.UserId == userId )
            //                    .Include( b => b. )
            //                    .FirstOrDefault(); 

            LibraryMember entity = new LibraryMember();

            Library_SectionMember mbr = ctx.Library_SectionMember
                        .Where( s => s.LibrarySectionId == libraryId && s.UserId == userId )
                        .SingleOrDefault( s => s.LibrarySectionId == libraryId && s.UserId == userId );
            if ( mbr != null && mbr.Id > 0 )
            {
                entity = LibrarySectionMember_ToMap( mbr );
            }

            return entity;
        }
        public static Library_SectionMember LibrarySectionMember_FromMap( LibraryMember fromEntity )
        {
            //Mapper.CreateMap<LibraryMember, Library_SectionMember>()
            //    .ForMember(dest => dest.LibrarySectionId, opt => opt.MapFrom(src => src.ParentId));

            //Library_SectionMember toEntity = Mapper.Map<LibraryMember, Library_SectionMember>( fromEntity );

            Library_SectionMember to = new Library_SectionMember();
            to.Id = fromEntity.Id;
            to.LibrarySectionId = fromEntity.ParentId;
            to.UserId = fromEntity.UserId;
            to.MemberTypeId = fromEntity.MemberTypeId;

            to.Created = fromEntity.Created;
            to.CreatedById = ( int )fromEntity.CreatedById;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = ( int )fromEntity.LastUpdatedById;
            to.RowId = ( Guid )fromEntity.RowId;
            return to;
        }

        public static LibraryMember LibrarySectionMember_ToMap( Library_SectionMember fromEntity )
        {

            //Mapper.CreateMap<Library_SectionMember, LibraryMember>()
            //    .ForMember( dest => dest.ParentId, opt => opt.MapFrom( src => src.LibrarySectionId ) );

            //LibraryMember toEntity = Mapper.Map<Library_SectionMember, LibraryMember>( fromEntity );

            LibraryMember to = new LibraryMember();
            to.Id = fromEntity.Id;
            to.ParentId = fromEntity.LibrarySectionId;
            to.UserId = fromEntity.UserId;
            to.MemberTypeId = fromEntity.MemberTypeId;

            to.Created = fromEntity.Created;
            to.CreatedById = ( int )fromEntity.CreatedById;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = ( int )fromEntity.LastUpdatedById;
            to.RowId = ( Guid )fromEntity.RowId;
            return to;
        }

        #endregion

        #region     Library Following
        public static List<ObjectSubscription> LibraryFollowing_ForUser( int userId )
        {
            ObjectSubscription entity = new ObjectSubscription();

            List<Library_Subscription> mbrs = ctx.Library_Subscription
                                .Where( s => s.UserId == userId )
                                .ToList();

            List<ObjectSubscription> list = new List<ObjectSubscription>();
            if ( mbrs.Count > 0 )
            {
                foreach ( Library_Subscription item in mbrs )
                {
                    entity = new ObjectSubscription();
                    entity.Id = item.Id;
                    entity.ParentId = item.LibraryId;
                    entity.UserId = item.UserId;
                    entity.SubscriptionTypeId = (int) item.SubscriptionTypeId;

                    entity.Created = (System.DateTime) item.Created;
                    entity.LastUpdated = ( System.DateTime ) item.LastUpdated;

                    //entity = LibraryMember_ToMap( item );
                    list.Add( entity );

                }
            }


            return list;
        }

        public static List<ObjectSubscription> CollectionFollowing_ForUser( int userId )
        {
            ObjectSubscription entity = new ObjectSubscription();

            List<Library_SectionSubscription> mbrs = ctx.Library_SectionSubscription
                                .Where( s => s.UserId == userId )
                                .ToList();

            List<ObjectSubscription> list = new List<ObjectSubscription>();
            if ( mbrs.Count > 0 )
            {
                foreach ( Library_SectionSubscription item in mbrs )
                {
                    entity = new ObjectSubscription();
                    entity.Id = item.Id;
                    entity.ParentId = item.SectionId;
                    entity.UserId = item.UserId;
                    entity.SubscriptionTypeId = ( int ) item.SubscriptionTypeId;

                    entity.Created = ( System.DateTime ) item.Created;
                    entity.LastUpdated = item.LastUpdated == null 
                                   ? ( System.DateTime ) entity.DefaultDate 
                                   : ( System.DateTime ) item.LastUpdated;
                    list.Add( entity );

                }
            }

            return list;
        }
        #endregion

        public static int LibraryCommentCreate( int libraryId, string comment, int createdById )
        {
            Library_Comment c = new Library_Comment();
            c.Comment = comment;
            c.CreatedById = createdById;
            c.Created = System.DateTime.Now;
            c.LibraryId = libraryId;

            ctx.Library_Comment.Add( c );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                return c.Id;
            }
            else
            {
                //?no info on error
                return 0;
            }
        }
    }
}
