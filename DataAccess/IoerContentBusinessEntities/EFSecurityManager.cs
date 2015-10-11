using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AutoMapper;
using ILP = ILPathways.Business;
using ILPathways.Common;
using ILPathways.Utilities;
using Isle.DTO;


namespace IoerContentBusinessEntities
{
    public class EFSecurityManager
    {
        static string thisClassName = "EFSecurityManager";
        DateTime DefaultDate = new System.DateTime(1970, 1, 1);

        public static ILP.ApplicationGroupPrivilege ApplicationGroupPrivilege_Get(int id)
        {
            ILP.ApplicationGroupPrivilege toEntity = new ILP.ApplicationGroupPrivilege();
            using (var context = new GatewayContext())
            {
                AppGroup_Privilege fromEntity = context.AppGroup_Privilege.SingleOrDefault(s => s.Id == id);
                if (fromEntity != null && fromEntity.Id > 0)
                {
                    toEntity.Id = fromEntity.Id;
                    toEntity.ObjectId = fromEntity.ObjectId;
                    toEntity.GroupId = fromEntity.GroupId;
                    toEntity.CreatePrivilege = Assign(fromEntity.CreatePrivilege);
                    toEntity.ReadPrivilege = Assign(fromEntity.ReadPrivilege);
                    toEntity.WritePrivilege = Assign(fromEntity.WritePrivilege);
                    toEntity.DeletePrivilege = Assign(fromEntity.DeletePrivilege);

                    toEntity.AppendPrivilege = Assign(fromEntity.AppendPrivilege);
                    toEntity.AppendToPrivilege = Assign(fromEntity.AppendToPrivilege);
                    toEntity.AssignPrivilege = Assign(fromEntity.AssignPrivilege);
                    toEntity.ApprovePrivilege = Assign(fromEntity.ApprovePrivilege);
                    toEntity.SharePrivilege = Assign(fromEntity.SharePrivilege);

                    toEntity.Created = Assign(fromEntity.Created);
                    if (fromEntity.ApplicationObject != null && fromEntity.ApplicationObject.Id > 0)
                    {
                        toEntity.ObjectName = fromEntity.ApplicationObject.DisplayName;
                    }
                    if (fromEntity.AppGroup != null && fromEntity.AppGroup.Id > 0)
                    {
                        toEntity.GroupName = fromEntity.AppGroup.Title;
                    }
                }
            }
            return toEntity;
        }

        public static List<ILP.ApplicationGroupPrivilege> ApplicationGroupPrivilege_Select( int groupId )
        {
            List<ILP.ApplicationGroupPrivilege> list = new List<ILP.ApplicationGroupPrivilege>();
            ILP.ApplicationGroupPrivilege toEntity = new ILP.ApplicationGroupPrivilege();
            using ( var context = new GatewayContext() )
            {
                List<AppGroup_Privilege> items = context.AppGroup_Privilege.Where( s => s.GroupId == groupId )
                                        .OrderBy( s => s.ApplicationObject )
                                        .ToList();

                if ( items.Count > 0 )
                {
                    foreach ( AppGroup_Privilege fromEntity in items )
                    {
                        toEntity = new ILP.ApplicationGroupPrivilege();

                        toEntity.Id = fromEntity.Id;
                        toEntity.ObjectId = fromEntity.ObjectId;
                        toEntity.GroupId = fromEntity.GroupId;
                        toEntity.CreatePrivilege = Assign( fromEntity.CreatePrivilege );
                        toEntity.ReadPrivilege = Assign( fromEntity.ReadPrivilege );
                        toEntity.WritePrivilege = Assign( fromEntity.WritePrivilege );
                        toEntity.DeletePrivilege = Assign( fromEntity.DeletePrivilege );

                        toEntity.AppendPrivilege = Assign( fromEntity.AppendPrivilege );
                        toEntity.AppendToPrivilege = Assign( fromEntity.AppendToPrivilege );
                        toEntity.AssignPrivilege = Assign( fromEntity.AssignPrivilege );
                        toEntity.ApprovePrivilege = Assign( fromEntity.ApprovePrivilege );
                        toEntity.SharePrivilege = Assign( fromEntity.SharePrivilege );

                        toEntity.Created = Assign( fromEntity.Created );
                        if ( fromEntity.ApplicationObject != null && fromEntity.ApplicationObject.Id > 0 )
                        {
                            toEntity.ObjectName = fromEntity.ApplicationObject.DisplayName;
                        }
                        if ( fromEntity.AppGroup != null && fromEntity.AppGroup.Id > 0 )
                        {
                            toEntity.GroupName = fromEntity.AppGroup.Title;
                        }
                        list.Add( toEntity );

                    }
                }

            }
            return list;
        }

        public static List<CodeItem> Codes_PrivilegeDepth_Select()
        {
            CodeItem entity = new CodeItem();
            List<CodeItem> list = new List<CodeItem>();

            using (var context = new GatewayContext())
            {
                List<Codes_PrivilegeDepth> items = context.Codes_PrivilegeDepth.OrderBy(s => s.Id).ToList();

                if (items != null && items.Count > 0)
                {
                    foreach (Codes_PrivilegeDepth item in items)
                    {
                        entity.Id = item.Id;
                        entity.Title = item.Title;
                        list.Add(entity);
                    }
                }
            }
            return list;
        }

        private static int Assign(int? value)
        {
            if (value != null)
                return (int)value;
            else
                return 0;
        }
        private static DateTime Assign(DateTime? value)
        {
            if (value != null)
                return (DateTime)value;
            else
                return DateTime.Now;
        }
    }
}
