using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AutoMapper;
using LB = LRWarehouse.Business;
using ILPathways.Utilities;

namespace IOERBusinessEntities
{
    public class ResourceTaggingManager
    {
        static string thisClassName = "ResourceTaggingManager";

        /// <summary>
        /// Create a resource tag
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Id for the new row (also in the entity)</returns>
        public int ResourceTag_Create (LB.ResourceTag entity ) 
        {

            using ( var context = new ResourceContext() )
            {
                Resource_Tag e = new Resource_Tag();
                e.ResourceIntId = entity.ResourceIntId;
                e.TagValueId = entity.TagValueId;
                e.OriginalValue = entity.OriginalValue;
                e.CreatedById = entity.CreatedById;
                e.Created = System.DateTime.Now;

                context.Resource_Tag.Add( e );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    return e.Id;
                }
                else
                {
                    //?no info on error
                    return 0;
                }
            }
        }

        /// <summary>
        /// Create resource tags from list
        /// Note that caller should only be providing new tags. If the caller can't do this, an alternate method will be needed.
        /// </summary>
        /// <param name="tagList"></param>
        /// <returns></returns>
        public int ResourceTag_Create( List<LB.ResourceTag> tagList )
        {
            int createCount = 0;
            using ( var context = new ResourceContext() )
            {
                foreach ( LB.ResourceTag entity in tagList )
                {
                    Resource_Tag e = new Resource_Tag();
                    e.ResourceIntId = entity.ResourceIntId;
                    e.TagValueId = entity.TagValueId;
                    e.OriginalValue = entity.OriginalValue;
                    e.CreatedById = entity.CreatedById;
                    e.Created = System.DateTime.Now;

                    context.Resource_Tag.Add( e );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        createCount++;
                        //return e.Id;
                    }
                    else
                    {
                        //?no info on error
                        LoggingHelper.LogError( thisClassName + ".ResourceTag_Create(List<>)", true );
                    }

                }
            }
            return createCount;
        }

        public static List<int> Resource_GetTagIds( int resourceId )
        {
            List<int> tagList = new List<int>();
            LB.ResourceTag to = new LB.ResourceTag();

            using ( var context = new ResourceContext() )
            {
                List<Resource_Tag> list = context.Resource_Tag
                            .Where( s => s.ResourceIntId == resourceId )
                            .OrderBy( s => s.Codes_TagValue.CategoryId )
                            .ThenBy( s => s.Codes_TagValue.Title )
                            .ToList();

                if ( list != null && list.Count > 0 )
                {
                    foreach ( Resource_Tag efEntity in list )
                    {
                        tagList.Add( efEntity.TagValueId );
                    }
                }
            }
            return tagList;
        }//

        public static List<LB.ResourceTag> Resource_GetTags( int resourceId )
        {
            List<LB.ResourceTag> tagList = new List<LB.ResourceTag>();
            LB.ResourceTag to = new LB.ResourceTag();
            try
            {
                using ( var context = new ResourceContext() )
                {
                    List<Resource_Tag> list = context.Resource_Tag
                                .Include( "Codes_TagValue" )
                                .Where( s => s.ResourceIntId == resourceId )
                                .OrderBy( s => s.Codes_TagValue.CategoryId )
                                .ThenBy( s => s.Codes_TagValue.Title )
                                .ToList();

                    if ( list != null && list.Count > 0 )
                    {
                        Mapper.CreateMap<Codes_TagValue, LB.CodesTagValue>();

                        foreach ( Resource_Tag efEntity in list )
                        {

                            Mapper.CreateMap<Resource_Tag, LB.ResourceTag>()
                                .ForMember( d => d.Codes_TagValue, opt => opt.MapFrom( source => source.Codes_TagValue ) )
                                .ForMember( d => d.CategoryId, o => o.Ignore() );

                            //                     .ForMember( d => d.Codes_TagValue.Title, opt => opt.MapFrom( source => source.Codes_TagValue.Title ) )
                            //.ForMember( d => d.Codes_TagValue.Id, opt => opt.MapFrom( source => source.Codes_TagValue.Id ) )
                            //.ForMember( d => d.Codes_TagValue.CodeId, opt => opt.MapFrom( source => source.Codes_TagValue.CodeId ) )
                            //.ForMember( d => d.Codes_TagValue.CategoryId, opt => opt.MapFrom( source => source.Codes_TagValue.CategoryId ) )

                            Mapper.AssertConfigurationIsValid();

                            to = Mapper.Map<Resource_Tag, LB.ResourceTag>( efEntity );
                            //just in case
                            to.CategoryId = to.Codes_TagValue.CategoryId;

                            tagList.Add( to );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format(".Resource_GetTags(resourceId ({0}))", resourceId ));
            }
            return tagList;
        }//
    }
}
