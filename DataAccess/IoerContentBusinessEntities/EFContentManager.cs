using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;

using ILPContentFile = ILPathways.Business.ContentFile;

namespace IoerContentBusinessEntities
{
    public class EFContentManager
    {
        static IsleContentEntities ctx = new IsleContentEntities();

        #region     ContentFile
        public static int ContentFile_Create( ILPContentFile fromEntity, ref string statusMessage )
        {

            ContentFile entity = entity = ILPContentFile_FromMap( fromEntity );
            entity.IsActive = true;
            entity.Created = System.DateTime.Now;
            entity.LastUpdated = System.DateTime.Now;

            ctx.ContentFiles.Add( entity );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                statusMessage = "Successful";
                return entity.Id;
            }
            else
            {
                statusMessage = "Error - ContentFile_Create failed";
                //?no info on error
                return 0;
            }
        }

        public static bool ContentFile_UpdateVersionId( int contentFileId, int versionId )
        {

            bool action = false;

            ContentFile entity = ctx.ContentFiles.SingleOrDefault( s => s.Id == contentFileId );
            if ( entity != null )
            {
                entity.ResourceVersionId = contentFileId;
                entity.LastUpdated = System.DateTime.Now;
                ctx.SaveChanges();

                action = true;
            }

            return action;
        }

        public static ContentFile ILPContentFile_FromMap( ILPContentFile fromEntity )
        {
            Mapper.CreateMap<ILPContentFile, ContentFile>();

            ContentFile toEntity = Mapper.Map<ILPContentFile, ContentFile>( fromEntity );

            return toEntity;
        }

        public static ILPContentFile ContentFile_ToMap( ContentFile fromEntity )
        {

            Mapper.CreateMap<ContentFile, ILPContentFile>();
            ILPContentFile toEntity = Mapper.Map<ContentFile, ILPContentFile>( fromEntity );

            return toEntity;
        }
        #endregion
    }
}
