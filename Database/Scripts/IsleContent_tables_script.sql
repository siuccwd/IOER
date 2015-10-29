USE [IsleContent]
GO
/****** Object:  Table [dbo].[Activity.OrgSummary]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Activity.OrgSummary](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[OrgId] [int] NULL,
	[ActivityTypeId] [int] NULL,
	[Activity] [varchar](100) NOT NULL,
	[EventTypeId] [int] NULL,
	[Event] [varchar](100) NOT NULL,
	[ObjectTypeId] [int] NULL,
	[ObjectType] [varchar](100) NOT NULL,
	[ObjectId] [int] NULL,
	[ObjectUrl] [varchar](500) NULL,
	[Comment] [varchar](1000) NULL,
	[IsLoggedIn] [bit] NULL,
	[ActionByUserId] [int] NULL,
	[SessionId] [varchar](50) NULL,
	[IPAddress] [varchar](50) NULL,
	[GeographicLocation] [varchar](100) NULL,
	[Referrer] [varchar](1000) NULL,
 CONSTRAINT [PK_Activity.OrgSummary] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ActivityLog]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ActivityLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetime] NULL CONSTRAINT [DF_ActivityLog_CreatedDate]  DEFAULT (getdate()),
	[ActivityType] [varchar](25) NOT NULL CONSTRAINT [DF_ActivityLog_Type]  DEFAULT ('audit'),
	[Activity] [varchar](100) NOT NULL,
	[Event] [varchar](100) NOT NULL,
	[Comment] [varchar](1000) NULL,
	[TargetUserId] [int] NULL,
	[ActionByUserId] [int] NULL,
	[ActivityObjectId] [int] NULL,
	[ObjectRelatedId] [int] NULL,
	[RelatedImageUrl] [varchar](500) NULL,
	[RelatedTargetUrl] [varchar](500) NULL,
	[SessionId] [varchar](50) NULL,
	[IPAddress] [varchar](50) NULL,
	[TargetObjectId] [int] NULL,
	[Referrer] [varchar](1000) NULL,
 CONSTRAINT [PK_ActivityLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AppGroup]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AppGroup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GroupCode] [varchar](50) NOT NULL,
	[Title] [varchar](100) NOT NULL,
	[Description] [varchar](500) NULL,
	[GroupTypeId] [int] NULL,
	[IsActive] [bit] NULL,
	[ContactId] [int] NULL,
	[OrgId] [int] NULL,
	[ParentGroupId] [int] NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_AppGroup_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_AppGroup_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_AppGroup_RowId]  DEFAULT (newid()),
 CONSTRAINT [PK_AppGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AppGroup.Member]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AppGroup.Member](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[UserRowId] [uniqueidentifier] NULL,
	[OrgId] [int] NULL,
	[Status] [varchar](50) NULL,
	[Category] [varchar](25) NULL,
	[IsActive] [bit] NULL,
	[Comment] [varchar](500) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_AppGroup.Member_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_AppGroup.Member_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_AppGroup.Member] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AppGroup.OrgMember]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppGroup.OrgMember](
	[GroupId] [int] NOT NULL,
	[OrgId] [int] NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IsActive] [bit] NULL CONSTRAINT [DF_AppGroup.OrgMember_IsActive]  DEFAULT ((1)),
	[Created] [datetime] NULL CONSTRAINT [DF_AppGroup.OrgMember_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_AppGroup.OrgMember_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_AppGroup.OrgMember_RowId]  DEFAULT (newid()),
 CONSTRAINT [PK_AppGroup.OrgMember] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AppGroup.Privilege]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppGroup.Privilege](
	[GroupId] [int] NOT NULL,
	[ObjectId] [int] NOT NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatePrivilege] [int] NULL,
	[ReadPrivilege] [int] NULL,
	[WritePrivilege] [int] NULL,
	[DeletePrivilege] [int] NULL,
	[AppendPrivilege] [int] NULL,
	[AppendToPrivilege] [int] NULL,
	[AssignPrivilege] [int] NULL,
	[ApprovePrivilege] [int] NULL,
	[SharePrivilege] [int] NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_AppGroup.Privilege_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_AppGroup.Privilege] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AppGroup.RelatedObject]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppGroup.RelatedObject](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NOT NULL,
	[RelatedObjectId] [int] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_AppGroup.RelatedObject] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ApplicationObject]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ApplicationObject](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ObjectName] [nvarchar](100) NULL,
	[DisplayName] [nvarchar](75) NULL,
	[Description] [nvarchar](250) NULL,
	[Active] [bit] NULL CONSTRAINT [DF_ApplicationObject_Active]  DEFAULT ((1)),
	[ObjectType] [varchar](50) NULL CONSTRAINT [DF_ApplicationObject_ObjectType]  DEFAULT ('Control'),
	[RelatedUrl] [varchar](150) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_ApplicationObject_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_ApplicationObject_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_ApplicationObject] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AppVisitLog]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AppVisitLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SessionId] [varchar](30) NOT NULL,
	[CreatedDate] [datetime] NOT NULL CONSTRAINT [DF_AppVisitLog_CreatedDate]  DEFAULT (getdate()),
	[Application] [varchar](50) NOT NULL,
	[URL] [varchar](300) NOT NULL,
	[Parameters] [varchar](300) NULL,
	[IsPostback] [bit] NULL,
	[Userid] [varchar](50) NULL,
	[Comment] [varchar](500) NULL,
	[RemoteIP] [varchar](25) NULL,
	[ServerName] [varchar](25) NULL,
	[CurrentZip] [varchar](12) NULL,
	[Referrer] [varchar](500) NULL,
 CONSTRAINT [PK_AppVisitLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[BouncedEmails]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[BouncedEmails](
	[E-mail Address] [nvarchar](50) NULL,
	[Date] [varchar](50) NULL,
	[Count] [nvarchar](50) NULL,
	[Bounce Type] [nvarchar](150) NULL,
	[Bounce Reason] [nvarchar](250) NULL,
	[id] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_BouncedEmails] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ActivityEventType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ActivityEventType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Codes.ActivityEventType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ActivityType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ActivityType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.ActivityType_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.ActivityType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.AuthorizationLevel]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.AuthorizationLevel](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](200) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Codes.AuthorizationLevel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ContentPartnerType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ContentPartnerType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[IsActive] [bit] NULL CONSTRAINT [DF_Codes.ContentPartnerType_IsActive]  DEFAULT ((1)),
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.ContentPartnerType_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.ContentPartnerType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ContentPrivilege]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ContentPrivilege](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[StemTitle] [varchar](50) NULL,
	[Description] [varchar](200) NULL,
	[SortOrder] [int] NULL,
	[IsActive] [bit] NULL CONSTRAINT [DF_Codes.ContentPrivilege_IsActive]  DEFAULT ((1)),
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.ContentPrivilege_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.ContentPrivilege] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ContentStatus]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ContentStatus](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.ContentStatus_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.ContentStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ExternalSite]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ExternalSite](
	[Id] [int] NOT NULL,
	[Title] [varchar](100) NULL,
	[Url] [varchar](200) NULL,
 CONSTRAINT [PK_Codes.ExternalSite] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.GroupType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.GroupType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.GroupType_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.GroupType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.LibraryAccessLevel]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.LibraryAccessLevel](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[IsActive] [bit] NULL CONSTRAINT [DF_Codes.LibraryAccessLevel_IsActive]  DEFAULT ((1)),
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.LibraryAccessLevel_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.LibraryAccessLevel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.LibraryMemberType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.LibraryMemberType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.LibraryMemberType_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.LibraryMemberType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ObjectMemberType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ObjectMemberType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.ObjectMemberType_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.ObjectMemberType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ObjectRelationship]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ObjectRelationship](
	[Id] [int] NOT NULL,
	[Code] [varchar](50) NULL,
	[ParentChildTitle] [varchar](50) NULL,
	[ChildParentTitle] [varchar](50) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.ObjectRelationship_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.ObjectRelationship] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.OrgMemberRole]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.OrgMemberRole](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.OrgMemberRole_Created]  DEFAULT (getdate()),
	[IsActive] [bit] NULL CONSTRAINT [DF_Codes.OrgMemberRole_IsActive]  DEFAULT ((1)),
 CONSTRAINT [PK_Codes.OrgMemberRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.OrgMemberType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.OrgMemberType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.OrgMemberType_Created]  DEFAULT (getdate()),
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Codes.OrgMemberType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.OrgType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.OrgType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.OrgType_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.OrgType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.PostingType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.PostingType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.Posting_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.Posting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.PrivilegeDepth]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.PrivilegeDepth](
	[Id] [int] NOT NULL,
	[Title] [varchar](75) NOT NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.PrivilegeDepth_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.PrivilegeDepth] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.PrivilegeType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.PrivilegeType](
	[Id] [int] NOT NULL,
	[Title] [varchar](75) NOT NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.PrivilegeType_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.PrivilegeType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.PublishingRole]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.PublishingRole](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[IsOrgRequired] [bit] NULL,
	[IsActive] [bit] NULL CONSTRAINT [DF_Codes.PublishingRole_IsActive]  DEFAULT ((1)),
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.PublishingRole_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.PublishingRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.StandardUsage]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.StandardUsage](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Codes.StandardUsage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.State]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.State](
	[Id] [int] NOT NULL,
	[StateCode] [varchar](2) NULL,
	[State] [varchar](50) NULL,
 CONSTRAINT [PK_Codes.State] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.SubscriptionType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.SubscriptionType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Codes.SubscriptionType_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Codes.SubscriptionType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CodeTable]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CodeTable](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CodeName] [varchar](50) NOT NULL,
	[LanguageCode] [varchar](10) NOT NULL CONSTRAINT [DF_CodeTable_LanguageCode]  DEFAULT ('en'),
	[StringValue] [varchar](100) NOT NULL,
	[NumericValue] [decimal](12, 4) NULL,
	[IntegerValue] [int] NULL,
	[Description] [varchar](500) NULL,
	[SortOrder] [tinyint] NULL CONSTRAINT [DF_CodeTable_SortOrder]  DEFAULT ((10)),
	[IsActive] [bit] NULL CONSTRAINT [DF_CodeTable_IsActive]  DEFAULT ((1)),
	[Created] [datetime] NULL CONSTRAINT [DF_CodeTable_Created]  DEFAULT (getdate()),
	[Modified] [datetime] NULL CONSTRAINT [DF_CodeTable_Modified]  DEFAULT (getdate()),
 CONSTRAINT [PK_CodeTable] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Community]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Community](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ImageUrl] [varchar](200) NULL,
	[ContactId] [int] NULL,
	[IsActive] [bit] NULL,
	[OrgId] [int] NULL,
	[PublicAccessLevel] [int] NOT NULL CONSTRAINT [DF_Community_PublicAccessLevel]  DEFAULT ((2)),
	[OrgAccessLevel] [int] NOT NULL CONSTRAINT [DF_Community_OrgAccessLevel]  DEFAULT ((4)),
	[IsModerated] [bit] NULL CONSTRAINT [DF_Community_IsModerated]  DEFAULT ((0)),
	[Created] [datetime] NULL CONSTRAINT [DF_Community_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Community_Created1]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Community] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Community.Member]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Community.Member](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CommunityId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[MemberTypeId] [int] NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Community.Member_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Community.Member] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Community.Posting]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Community.Posting](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Message] [nvarchar](max) NULL,
	[PostingTypeId] [int] NULL CONSTRAINT [DF_Community.Posting_PostingTypeId]  DEFAULT ((1)),
	[CreatedById] [int] NOT NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Community.Posting_Created]  DEFAULT (getdate()),
	[RelatedPostingId] [int] NULL,
	[IsApproved] [bit] NOT NULL CONSTRAINT [DF_Community.Posting_IsApproved]  DEFAULT ((0)),
 CONSTRAINT [PK_Community.Posting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Community.PostingDocument]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Community.PostingDocument](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PostingId] [int] NULL,
	[DocumentId] [uniqueidentifier] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Community.PostingDocument] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Community.PostItem]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Community.PostItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CommunityId] [int] NOT NULL,
	[PostingId] [int] NULL,
	[CreatedById] [int] NOT NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Community.PostItem_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Community.PostItem] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Community.PostItem] UNIQUE CLUSTERED 
(
	[CommunityId] ASC,
	[PostingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Content]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Content](
	[Id] [int] IDENTITY(1000,1) NOT NULL,
	[TypeId] [int] NULL CONSTRAINT [DF_Content_TypeId]  DEFAULT ((10)),
	[Title] [varchar](200) NULL,
	[Summary] [varchar](max) NULL,
	[Description] [varchar](max) NULL,
	[StatusId] [int] NULL CONSTRAINT [DF_Table_1_StatusId]  DEFAULT ((1)),
	[PrivilegeTypeId] [int] NULL,
	[ConditionsOfUseId] [int] NULL,
	[IsActive] [bit] NULL CONSTRAINT [DF_Table_1_IsActive]  DEFAULT ((1)),
	[IsPublished] [bit] NULL CONSTRAINT [DF_Content_IsPublished]  DEFAULT ((0)),
	[IsOrgContentOwner] [bit] NULL,
	[OrgId] [int] NULL,
	[ResourceIntId] [int] NULL,
	[ResourceVersionId] [int] NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Table_1_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Table_1_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
	[Approved] [datetime] NULL,
	[ApprovedById] [int] NULL,
	[RowId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Table_1_RowId]  DEFAULT (newid()),
	[UseRightsUrl] [varchar](200) NULL,
	[DocumentRowId] [uniqueidentifier] NULL,
	[DocumentUrl] [varchar](200) NULL,
	[ParentId] [int] NULL,
	[SortOrder] [int] NULL CONSTRAINT [DF_Content_SortOrder]  DEFAULT ((10)),
	[Timeframe] [varchar](100) NULL,
	[ImageUrl] [varchar](200) NULL,
 CONSTRAINT [PK_Table_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Content.Connector]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Content.Connector](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParentId] [int] NOT NULL,
	[ChildId] [int] NULL,
	[SortOrder] [int] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[OldChildId] [int] NULL,
 CONSTRAINT [PK_Content.Connector] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Content.History]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Content.History](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContentId] [int] NOT NULL,
	[Action] [varchar](75) NOT NULL CONSTRAINT [DF_Content.History_Content.History]  DEFAULT ('Project Action'),
	[Description] [varchar](max) NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Content.History_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NOT NULL,
 CONSTRAINT [PK_Content.History] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Content.Like]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Content.Like](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContentId] [int] NULL,
	[IsLike] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Content.Like] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Content.Partner]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Content.Partner](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContentId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[PartnerTypeId] [int] NOT NULL CONSTRAINT [DF_Content.Partner_PartnerTypeId]  DEFAULT ((0)),
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Content.Partner_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL CONSTRAINT [DF_Content.Partner_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Content.Partner] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Content.Partner_LibUser] UNIQUE NONCLUSTERED 
(
	[ContentId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Content.Reference]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Content.Reference](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParentId] [int] NOT NULL,
	[Title] [varchar](200) NOT NULL,
	[Author] [varchar](100) NULL,
	[Publisher] [varchar](100) NULL,
	[ISBN] [varchar](50) NULL,
	[ReferenceUrl] [varchar](200) NULL,
	[AdditionalInfo] [varchar](500) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Content.Reference_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Content.Reference_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Content.Reference] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Content.Resource]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Content.Resource](
	[Id] [int] NOT NULL,
	[ContentId] [int] NOT NULL,
	[ResourceIntId] [int] NOT NULL,
	[SortOrder] [int] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Content.Resource] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Content.Standard]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Content.Standard](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContentId] [int] NOT NULL,
	[StandardId] [int] NOT NULL,
	[AlignmentTypeCodeId] [int] NULL,
	[UsageTypeId] [int] NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Content.Standard_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Content.Standard_Created1]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Content.StandardAlignment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Content.Subscription]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Content.Subscription](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContentId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[SubscriptionTypeId] [int] NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Content.Subscription_Created]  DEFAULT (getdate()),
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Content.Subscription_LastUpdated]  DEFAULT (getdate()),
 CONSTRAINT [PK_Content.Subscription] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Content.Supplement]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Content.Supplement](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParentId] [int] NOT NULL,
	[Title] [varchar](200) NOT NULL,
	[Description] [varchar](max) NULL,
	[ResourceUrl] [varchar](200) NULL,
	[PrivilegeTypeId] [int] NULL,
	[IsActive] [bit] NULL CONSTRAINT [DF_Content.Supplement_IsActive]  DEFAULT ((1)),
	[Created] [datetime] NULL CONSTRAINT [DF_Content.Supplement_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Content.Supplement_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NULL CONSTRAINT [DF_Content.Supplement_RowId]  DEFAULT (newid()),
	[DocumentRowId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Content.Supplement] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Content.Tag]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Content.Tag](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContentId] [int] NOT NULL,
	[TagValueId] [int] NOT NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Content.Tag_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Content.Tag] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Content.UserPrivilege]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Content.UserPrivilege](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContentId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[TypeId] [int] NOT NULL CONSTRAINT [DF_Content.UserPrivilege_TypeId]  DEFAULT ((0)),
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Content.UserPrivilege_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL CONSTRAINT [DF_Content.UserPrivilege_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Content.UserPrivilege] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Content.UserPrivilege_LibUser] UNIQUE NONCLUSTERED 
(
	[ContentId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ContentFileNU]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ContentFileNU](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](200) NOT NULL,
	[Description] [varchar](max) NULL,
	[ResourceUrl] [varchar](200) NOT NULL,
	[OrgId] [int] NULL,
	[ResourceVersionId] [int] NULL,
	[IsActive] [bit] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
	[DocumentRowId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_ContentFile] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ContentTemplate]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ContentTemplate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Template] [varchar](max) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_ContentTemplate_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_ContentTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ContentType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING OFF
GO
CREATE TABLE [dbo].[ContentType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NOT NULL,
	[Description] [varchar](200) NOT NULL,
	[HasApproval] [bit] NULL CONSTRAINT [DF_Content.Type_HasApproval]  DEFAULT ((0)),
	[MaxVersions] [smallint] NULL CONSTRAINT [DF_Content.Type_MaxVersions]  DEFAULT ((1)),
	[IsActive] [bit] NULL CONSTRAINT [DF_Content.Type_IsActive]  DEFAULT ((1)),
	[Created] [datetime] NULL CONSTRAINT [DF_Content.Type_Created]  DEFAULT (getdate()),
	[RowId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Content.Type_RowId]  DEFAULT (newid()),
 CONSTRAINT [PK_Content.Type] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Document.Version]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Document.Version](
	[RowId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Document.Version_RowId]  DEFAULT (newid()),
	[Title] [varchar](200) NOT NULL,
	[Summary] [varchar](500) NULL,
	[Status] [varchar](25) NULL CONSTRAINT [DF_Document.Version_Status]  DEFAULT ('Pending'),
	[FileName] [varchar](150) NULL,
	[FileDate] [datetime] NULL,
	[MimeType] [varchar](150) NULL,
	[Bytes] [bigint] NULL,
	[Data] [varbinary](max) NOT NULL,
	[Url] [varchar](150) NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Document.Version_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NOT NULL,
	[LastUpdated] [datetime] NOT NULL CONSTRAINT [DF_Document.Version_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NOT NULL,
	[FilePath] [varchar](200) NULL,
 CONSTRAINT [PK_Document.Version] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Library]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Library](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](200) NOT NULL,
	[Description] [varchar](500) NULL,
	[LibraryTypeId] [int] NULL,
	[IsDiscoverable] [bit] NULL,
	[PublicAccessLevel] [int] NOT NULL CONSTRAINT [DF_Library_PublicAccessLevel]  DEFAULT ((1)),
	[OrgAccessLevel] [int] NOT NULL CONSTRAINT [DF_Library_OrgAccessLevel]  DEFAULT ((2)),
	[OrgId] [int] NULL,
	[IsActive] [bit] NULL CONSTRAINT [DF_Library_IsActive]  DEFAULT ((1)),
	[ImageUrl] [varchar](200) NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Library_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Library_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Library_RowId]  DEFAULT (newid()),
	[IsPublic] [bit] NULL,
	[AllowJoinRequest] [bit] NULL,
 CONSTRAINT [PK_Library] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Library.Comment]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Library.Comment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LibraryId] [int] NULL,
	[Comment] [varchar](1000) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Library.Comment_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Library.Comment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Library.Content]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Library.Content](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LibrarySectionId] [int] NOT NULL,
	[ContentId] [int] NOT NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Library.Content] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Library.ExternalSection]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Library.ExternalSection](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LibraryId] [int] NOT NULL,
	[ExternalSectionId] [int] NOT NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Library.ExternalSection_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[RowId] [uniqueidentifier] NULL CONSTRAINT [DF_Library.ExternalSection_RowId]  DEFAULT (newid()),
 CONSTRAINT [PK_Library.ExternalSection] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Library.Invitation]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Library.Invitation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LibraryId] [int] NULL,
	[RowId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Library.Invitation_RowId]  DEFAULT (newid()),
	[InvitationType] [varchar](50) NULL CONSTRAINT [DF_Library.Invitation_Type]  DEFAULT ('Individual'),
	[PassCode] [varchar](50) NULL,
	[TargetEmail] [varchar](150) NULL,
	[TargetUserId] [int] NULL,
	[AddToOrgId] [int] NULL,
	[AddAsOrgMemberTypeId] [int] NULL,
	[OrgMbrRoles] [varchar](50) NULL,
	[Subject] [varchar](100) NULL,
	[MessageContent] [varchar](max) NULL,
	[EmailNoticeCode] [varchar](50) NULL,
	[Response] [varchar](50) NULL,
	[ResponseDate] [datetime] NULL,
	[ExpiryDate] [datetime] NULL,
	[IsActive] [bit] NULL CONSTRAINT [DF_Library.Invitation_IsActive]  DEFAULT ((1)),
	[DeleteOnResponse] [bit] NULL CONSTRAINT [DF_Library.Invitation_DeleteOnResponse]  DEFAULT ((0)),
	[Created] [datetime] NULL CONSTRAINT [DF_Table_1_DateSent]  DEFAULT (getdate()),
	[CreatedById] [int] NOT NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Library.Invitation_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
	[LibMemberTypeId] [int] NULL,
	[StartingUrl] [varchar](200) NULL,
 CONSTRAINT [PK_Library.Invitation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Library.Like]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Library.Like](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LibraryId] [int] NOT NULL,
	[IsLike] [bit] NOT NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Library.Like_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NOT NULL,
 CONSTRAINT [PK_Library.Like_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Library.Member]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Library.Member](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LibraryId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[MemberTypeId] [int] NOT NULL CONSTRAINT [DF_Library.Member_MemberTypeId]  DEFAULT ((0)),
	[RowId] [uniqueidentifier] NULL CONSTRAINT [DF_Library.Member_RowId]  DEFAULT (newid()),
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Library.Member_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL CONSTRAINT [DF_Library.Member_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Library.Member] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Library.Member_LibUser] UNIQUE NONCLUSTERED 
(
	[LibraryId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Library.Party]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Library.Party](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParentRowId] [uniqueidentifier] NOT NULL,
	[RelatedRowId] [uniqueidentifier] NOT NULL,
	[Created] [datetime] NOT NULL,
 CONSTRAINT [PK_Library.Party] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Library.PartyComment]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Library.PartyComment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[Comment] [varchar](1000) NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NOT NULL,
 CONSTRAINT [PK_Library.PartyComment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Library.Resource]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Library.Resource](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LibrarySectionId] [int] NOT NULL,
	[ResourceIntId] [int] NOT NULL,
	[IsActive] [bit] NULL CONSTRAINT [DF_Library.Resource_IsActive]  DEFAULT ((1)),
	[Comment] [varchar](500) NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Library.Resource_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Library.Resource] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Library.Section]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Library.Section](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LibraryId] [int] NOT NULL,
	[SectionTypeId] [int] NOT NULL CONSTRAINT [DF_Library.Section_SectionTypeId]  DEFAULT ((3)),
	[Title] [varchar](100) NULL,
	[Description] [varchar](500) NULL,
	[ParentId] [int] NULL,
	[IsDefaultSection] [bit] NULL CONSTRAINT [DF_Library.Section_IsDefaultSection]  DEFAULT ((0)),
	[PublicAccessLevel] [int] NOT NULL CONSTRAINT [DF_Library.Section_PublicAccessLevel]  DEFAULT ((1)),
	[OrgAccessLevel] [int] NOT NULL CONSTRAINT [DF_Library.Section_OrgAccessLevel]  DEFAULT ((2)),
	[AreContentsReadOnly] [bit] NULL CONSTRAINT [DF_Library.Section_AreContentsReadOnly]  DEFAULT ((0)),
	[ImageUrl] [varchar](200) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Library.Section_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Library.Section_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NULL CONSTRAINT [DF_Library.Section_RowId]  DEFAULT (newid()),
	[IsPublic] [bit] NULL CONSTRAINT [DF_Library.Section_IsPublic]  DEFAULT ((1)),
 CONSTRAINT [PK_Library.Section] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Library.SectionComment]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Library.SectionComment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SectionId] [int] NULL,
	[Comment] [varchar](1000) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Library.SectionComment_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Library.SectionComment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Library.SectionLike]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Library.SectionLike](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SectionId] [int] NOT NULL,
	[IsLike] [bit] NOT NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Library.SectionLike_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NOT NULL,
 CONSTRAINT [PK_Library.SectionLike] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Library.SectionMember]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Library.SectionMember](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LibrarySectionId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[MemberTypeId] [int] NOT NULL CONSTRAINT [DF_Library.SectionMember_MemberTypeId]  DEFAULT ((0)),
	[RowId] [uniqueidentifier] NULL CONSTRAINT [DF_Library.SectionMember_RowId]  DEFAULT (newid()),
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Library.SectionMember_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL CONSTRAINT [DF_Library.SectionMember_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Library.SectionMember] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Library.SectionMember_LibUser] UNIQUE NONCLUSTERED 
(
	[LibrarySectionId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Library.SectionSubscription]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Library.SectionSubscription](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SectionId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[SubscriptionTypeId] [int] NULL,
	[PrivilegeId] [int] NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Library.SectionSubscription_Created]  DEFAULT (getdate()),
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Library.SectionSubscription_LastUpdated]  DEFAULT (getdate()),
 CONSTRAINT [PK_Library.SectionSubscription] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Library.SectionType]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Library.SectionType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NOT NULL,
	[AreContentsReadOnly] [bit] NULL CONSTRAINT [DF_Library.SectionType_IsReadOnly]  DEFAULT ((0)),
	[Decription] [varchar](200) NULL,
	[SectionCode] [varchar](25) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Library.SectionType_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Library.SectionType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Library.Subscription]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Library.Subscription](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LibraryId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[SubscriptionTypeId] [int] NULL,
	[PrivilegeId] [int] NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Library.Subscription_Created]  DEFAULT (getdate()),
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Library.Subscription_LastUpdated]  DEFAULT (getdate()),
 CONSTRAINT [PK_Library.Subscription] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Library.Type]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Library.Type](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](300) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Library.Type_Created]  DEFAULT (getdate()),
	[timestampc] [timestamp] NULL,
 CONSTRAINT [PK_Library.Type] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Member.Invitation]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Member.Invitation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InvitationType] [varchar](50) NOT NULL,
	[ParentId] [int] NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[PassCode] [varchar](50) NULL,
	[TargetEmail] [varchar](150) NULL,
	[TargetUserId] [int] NULL,
	[MemberTypeId] [int] NULL,
	[AddToOrgId] [int] NULL,
	[AddAsOrgMemberTypeId] [int] NULL,
	[OrgMbrRoles] [varchar](50) NULL,
	[Subject] [varchar](100) NULL,
	[MessageContent] [varchar](max) NULL,
	[EmailNoticeCode] [varchar](50) NULL,
	[StartingUrl] [varchar](200) NULL,
	[Response] [varchar](50) NULL,
	[ResponseDate] [datetime] NULL,
	[ExpiryDate] [datetime] NULL,
	[IsActive] [bit] NULL,
	[DeleteOnResponse] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NOT NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Member.Invitation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Object.Comment]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Object.Comment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[Comment] [varchar](1000) NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NOT NULL,
 CONSTRAINT [PK_Object.Comment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Object.Connector]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Object.Connector](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParentRowId] [uniqueidentifier] NOT NULL,
	[RelatedRowId] [uniqueidentifier] NOT NULL,
	[ObjectConnectionTypeId] [int] NOT NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Object.Connector_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Object.Connector] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Object.Invitation]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Object.Invitation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ObjectId] [int] NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[TargetEmail] [varchar](150) NULL,
	[TargetUserId] [int] NULL,
	[AddToOrgId] [int] NULL,
	[AddAsOrgMemberTypeId] [int] NULL,
	[Subject] [varchar](100) NULL,
	[MessageContent] [varchar](max) NULL,
	[ResponseDate] [datetime] NULL,
	[ExpiryDate] [datetime] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NOT NULL,
	[MemberTypeId] [int] NULL,
 CONSTRAINT [PK_Object.Invitation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Object.Member]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Object.Member](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[ParentRowId] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[MemberTypeId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Object.Member] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Object.Member_User] UNIQUE NONCLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Object.Related]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Object.Related](
	[RowId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Object.Related_RowId]  DEFAULT (newid()),
	[ObjectId] [int] NOT NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Object.Related_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NOT NULL,
 CONSTRAINT [PK_Object.Related] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Organization]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Organization](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[OrgTypeId] [int] NULL,
	[parentId] [int] NULL,
	[IsActive] [bit] NULL CONSTRAINT [DF_Organization_IsActive]  DEFAULT ((1)),
	[MainPhone] [varchar](20) NULL,
	[MainExtension] [varchar](10) NULL,
	[Fax] [varchar](10) NULL,
	[TTY] [varchar](10) NULL,
	[WebSite] [varchar](100) NULL,
	[Email] [varchar](100) NULL,
	[LogoUrl] [varchar](200) NULL,
	[Address] [varchar](50) NULL,
	[Address2] [varchar](50) NULL,
	[City] [varchar](50) NULL,
	[State] [char](2) NULL,
	[Zipcode] [varchar](10) NULL,
	[Zipcode4] [varchar](4) NULL,
	[K12Identifier] [varchar](50) NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Organization_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Organization_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NULL CONSTRAINT [DF_Organization_RowId]  DEFAULT (newid()),
	[IsIsleMember] [bit] NULL,
	[EmailDomain] [varchar](50) NULL,
 CONSTRAINT [PK_Organization] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Organization.Member]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Organization.Member](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrgId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[OrgMemberTypeId] [int] NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Organization.Member_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_Organization.Member_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Organization.Member] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Organization.MemberRole]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Organization.MemberRole](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrgMemberId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Organization.MemberRole_Created]  DEFAULT (getdate()),
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Organization.MemberRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[OrganizationRequest]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[OrganizationRequest](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[OrgId] [int] NULL,
	[OrganzationName] [varchar](100) NULL,
	[Action] [varchar](100) NULL,
	[IsActive] [bit] NULL CONSTRAINT [DF_OrganizationRequest_IsActive]  DEFAULT ((0)),
	[Created] [datetime] NULL CONSTRAINT [DF_OrganizationRequest_Created]  DEFAULT (getdate()),
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_OrganizationRequest_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_OrganizationRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Person.Following]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Person.Following](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FollowingUserId] [int] NOT NULL,
	[FollowedByUserId] [int] NOT NULL,
	[Created] [datetime] NULL CONSTRAINT [DF_Person.Following_Created]  DEFAULT (getdate()),
 CONSTRAINT [PK_Person.Following] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SqlQuery]    Script Date: 9/27/2015 11:16:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SqlQuery](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](125) NOT NULL,
	[Description] [varchar](500) NULL,
	[QueryCode] [varchar](50) NULL,
	[Category] [varchar](25) NULL,
	[SQL] [varchar](max) NOT NULL,
	[OwnerId] [int] NULL,
	[IsPublic] [bit] NULL CONSTRAINT [DF_SqlQuery_IsPublic]  DEFAULT ((1)),
	[Created] [datetime] NULL CONSTRAINT [DF_SqlQuery_Created]  DEFAULT (getdate()),
	[CreatedBy] [varchar](50) NULL,
	[LastUpdated] [datetime] NULL CONSTRAINT [DF_SqlQuery_LastUpdated]  DEFAULT (getdate()),
	[LastUpdatedBy] [varchar](50) NULL,
 CONSTRAINT [PK_SqlQuery] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[Activity.OrgSummary] ADD  CONSTRAINT [DF_Activity.OrgSummary_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Activity.OrgSummary] ADD  CONSTRAINT [DF_Table_1_ActivityType]  DEFAULT ('audit') FOR [ObjectType]
GO
ALTER TABLE [dbo].[Activity.OrgSummary] ADD  CONSTRAINT [DF_Activity.OrgSummary_IsGuest]  DEFAULT ((0)) FOR [IsLoggedIn]
GO
ALTER TABLE [dbo].[AppGroup.RelatedObject] ADD  CONSTRAINT [DF_AppGroup.RelatedObject_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[AppGroup.RelatedObject] ADD  CONSTRAINT [DF_AppGroup.RelatedObject_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[AppGroup.RelatedObject] ADD  CONSTRAINT [DF_AppGroup.RelatedObject_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Codes.ActivityEventType] ADD  CONSTRAINT [DF_Codes.ActivityEventType_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Codes.AuthorizationLevel] ADD  CONSTRAINT [DF_Codes.AuthorizationLevel_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Community.PostingDocument] ADD  CONSTRAINT [DF_Community.PostingDocument_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Content.Connector] ADD  CONSTRAINT [DF_Content.Connector_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Content.Connector] ADD  CONSTRAINT [DF_Content.Connector_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Content.Like] ADD  CONSTRAINT [DF_Content.Like_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Content.Resource] ADD  CONSTRAINT [DF_Content.Resource_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Content.Resource] ADD  CONSTRAINT [DF_Content.Resource_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[ContentFileNU] ADD  CONSTRAINT [DF_ContentFile_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ContentFileNU] ADD  CONSTRAINT [DF_ContentFile_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[ContentFileNU] ADD  CONSTRAINT [DF_ContentFile_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Library.Content] ADD  CONSTRAINT [DF_Library.Content_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Library.Content] ADD  CONSTRAINT [DF_Library.Content_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Library.Party] ADD  CONSTRAINT [DF_Library.Party_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Library.PartyComment] ADD  CONSTRAINT [DF_Library.PartyComment_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Library.PartyComment] ADD  CONSTRAINT [DF_Library.PartyComment_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Member.Invitation] ADD  CONSTRAINT [DF_Member.Invitation_Type]  DEFAULT ('Library') FOR [InvitationType]
GO
ALTER TABLE [dbo].[Member.Invitation] ADD  CONSTRAINT [DF_Member.Invitation_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Member.Invitation] ADD  CONSTRAINT [DF_Member.Invitation_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Member.Invitation] ADD  CONSTRAINT [DF_Member.Invitation_DeleteOnResponse]  DEFAULT ((0)) FOR [DeleteOnResponse]
GO
ALTER TABLE [dbo].[Member.Invitation] ADD  CONSTRAINT [DF_Member.Invitation_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Object.Comment] ADD  CONSTRAINT [DF_Object.Comment_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Object.Comment] ADD  CONSTRAINT [DF_Object.Comment_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Object.Invitation] ADD  CONSTRAINT [DF_Object.Invitation_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Object.Invitation] ADD  CONSTRAINT [DF_Object.Invitation_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Object.Invitation] ADD  CONSTRAINT [DF_Object.Invitation_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Object.Member] ADD  CONSTRAINT [DF_Object.Member_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Object.Member] ADD  CONSTRAINT [DF_Object.Member_MemberTypeId]  DEFAULT ((0)) FOR [MemberTypeId]
GO
ALTER TABLE [dbo].[Object.Member] ADD  CONSTRAINT [DF_Object.Member_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Object.Member] ADD  CONSTRAINT [DF_Object.Member_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[AppGroup]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup_Organization] FOREIGN KEY([OrgId])
REFERENCES [dbo].[Organization] ([Id])
GO
ALTER TABLE [dbo].[AppGroup] CHECK CONSTRAINT [FK_AppGroup_Organization]
GO
ALTER TABLE [dbo].[AppGroup.Member]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup.Member_AppGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[AppGroup] ([Id])
GO
ALTER TABLE [dbo].[AppGroup.Member] CHECK CONSTRAINT [FK_AppGroup.Member_AppGroup]
GO
ALTER TABLE [dbo].[AppGroup.Member]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup.Member_Organization] FOREIGN KEY([OrgId])
REFERENCES [dbo].[Organization] ([Id])
GO
ALTER TABLE [dbo].[AppGroup.Member] CHECK CONSTRAINT [FK_AppGroup.Member_Organization]
GO
ALTER TABLE [dbo].[AppGroup.OrgMember]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup.OrgMember_AppGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[AppGroup] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AppGroup.OrgMember] CHECK CONSTRAINT [FK_AppGroup.OrgMember_AppGroup]
GO
ALTER TABLE [dbo].[AppGroup.OrgMember]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup.OrgMember_Organization] FOREIGN KEY([OrgId])
REFERENCES [dbo].[Organization] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AppGroup.OrgMember] CHECK CONSTRAINT [FK_AppGroup.OrgMember_Organization]
GO
ALTER TABLE [dbo].[AppGroup.Privilege]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup.Privilege_AppGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[AppGroup] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AppGroup.Privilege] CHECK CONSTRAINT [FK_AppGroup.Privilege_AppGroup]
GO
ALTER TABLE [dbo].[AppGroup.Privilege]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup.Privilege_ApplicationObject] FOREIGN KEY([ObjectId])
REFERENCES [dbo].[ApplicationObject] ([Id])
GO
ALTER TABLE [dbo].[AppGroup.Privilege] CHECK CONSTRAINT [FK_AppGroup.Privilege_ApplicationObject]
GO
ALTER TABLE [dbo].[AppGroup.RelatedObject]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup.RelatedObject_AppGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[AppGroup] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AppGroup.RelatedObject] CHECK CONSTRAINT [FK_AppGroup.RelatedObject_AppGroup]
GO
ALTER TABLE [dbo].[Community]  WITH CHECK ADD  CONSTRAINT [FK_Community_Codes.OrgAccessLevel] FOREIGN KEY([OrgAccessLevel])
REFERENCES [dbo].[Codes.LibraryAccessLevel] ([Id])
GO
ALTER TABLE [dbo].[Community] CHECK CONSTRAINT [FK_Community_Codes.OrgAccessLevel]
GO
ALTER TABLE [dbo].[Community]  WITH CHECK ADD  CONSTRAINT [FK_Community_Codes.PublicAccessLevel] FOREIGN KEY([PublicAccessLevel])
REFERENCES [dbo].[Codes.LibraryAccessLevel] ([Id])
GO
ALTER TABLE [dbo].[Community] CHECK CONSTRAINT [FK_Community_Codes.PublicAccessLevel]
GO
ALTER TABLE [dbo].[Community.Member]  WITH CHECK ADD  CONSTRAINT [FK_Community.Member_Codes.LibraryMemberType] FOREIGN KEY([MemberTypeId])
REFERENCES [dbo].[Codes.LibraryMemberType] ([Id])
GO
ALTER TABLE [dbo].[Community.Member] CHECK CONSTRAINT [FK_Community.Member_Codes.LibraryMemberType]
GO
ALTER TABLE [dbo].[Community.Member]  WITH CHECK ADD  CONSTRAINT [FK_Community.Member_Community] FOREIGN KEY([CommunityId])
REFERENCES [dbo].[Community] ([Id])
GO
ALTER TABLE [dbo].[Community.Member] CHECK CONSTRAINT [FK_Community.Member_Community]
GO
ALTER TABLE [dbo].[Community.Posting]  WITH CHECK ADD  CONSTRAINT [FK_Community.Posting_Codes.PostingType] FOREIGN KEY([PostingTypeId])
REFERENCES [dbo].[Codes.PostingType] ([Id])
GO
ALTER TABLE [dbo].[Community.Posting] CHECK CONSTRAINT [FK_Community.Posting_Codes.PostingType]
GO
ALTER TABLE [dbo].[Community.Posting]  WITH CHECK ADD  CONSTRAINT [FK_Community.Posting_Community.Posting] FOREIGN KEY([RelatedPostingId])
REFERENCES [dbo].[Community.Posting] ([Id])
GO
ALTER TABLE [dbo].[Community.Posting] CHECK CONSTRAINT [FK_Community.Posting_Community.Posting]
GO
ALTER TABLE [dbo].[Community.PostingDocument]  WITH CHECK ADD  CONSTRAINT [FK_Community.PostingDocument_Community.Posting] FOREIGN KEY([PostingId])
REFERENCES [dbo].[Community.Posting] ([Id])
GO
ALTER TABLE [dbo].[Community.PostingDocument] CHECK CONSTRAINT [FK_Community.PostingDocument_Community.Posting]
GO
ALTER TABLE [dbo].[Community.PostingDocument]  WITH CHECK ADD  CONSTRAINT [FK_Community.PostingDocument_Document.Version] FOREIGN KEY([DocumentId])
REFERENCES [dbo].[Document.Version] ([RowId])
GO
ALTER TABLE [dbo].[Community.PostingDocument] CHECK CONSTRAINT [FK_Community.PostingDocument_Document.Version]
GO
ALTER TABLE [dbo].[Community.PostItem]  WITH CHECK ADD  CONSTRAINT [FK_Community.PostItem_Community] FOREIGN KEY([CommunityId])
REFERENCES [dbo].[Community] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Community.PostItem] CHECK CONSTRAINT [FK_Community.PostItem_Community]
GO
ALTER TABLE [dbo].[Community.PostItem]  WITH CHECK ADD  CONSTRAINT [FK_Community.PostItem_Community.Posting] FOREIGN KEY([PostingId])
REFERENCES [dbo].[Community.Posting] ([Id])
ON UPDATE SET DEFAULT
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Community.PostItem] CHECK CONSTRAINT [FK_Community.PostItem_Community.Posting]
GO
ALTER TABLE [dbo].[Content]  WITH CHECK ADD  CONSTRAINT [FK_Content_Codes.ContentPrivilege] FOREIGN KEY([PrivilegeTypeId])
REFERENCES [dbo].[Codes.ContentPrivilege] ([Id])
GO
ALTER TABLE [dbo].[Content] CHECK CONSTRAINT [FK_Content_Codes.ContentPrivilege]
GO
ALTER TABLE [dbo].[Content]  WITH CHECK ADD  CONSTRAINT [FK_Content_Codes.ContentStatus] FOREIGN KEY([StatusId])
REFERENCES [dbo].[Codes.ContentStatus] ([Id])
GO
ALTER TABLE [dbo].[Content] CHECK CONSTRAINT [FK_Content_Codes.ContentStatus]
GO
ALTER TABLE [dbo].[Content]  WITH CHECK ADD  CONSTRAINT [FK_Content_ContentType] FOREIGN KEY([TypeId])
REFERENCES [dbo].[ContentType] ([Id])
GO
ALTER TABLE [dbo].[Content] CHECK CONSTRAINT [FK_Content_ContentType]
GO
ALTER TABLE [dbo].[Content]  WITH CHECK ADD  CONSTRAINT [FK_Content_Document.Version] FOREIGN KEY([DocumentRowId])
REFERENCES [dbo].[Document.Version] ([RowId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Content] CHECK CONSTRAINT [FK_Content_Document.Version]
GO
ALTER TABLE [dbo].[Content.Connector]  WITH CHECK ADD  CONSTRAINT [FK_Content.Connector_ChildContent] FOREIGN KEY([ChildId])
REFERENCES [dbo].[Content] ([Id])
GO
ALTER TABLE [dbo].[Content.Connector] CHECK CONSTRAINT [FK_Content.Connector_ChildContent]
GO
ALTER TABLE [dbo].[Content.Connector]  WITH CHECK ADD  CONSTRAINT [FK_Content.Connector_ParentContent] FOREIGN KEY([ParentId])
REFERENCES [dbo].[Content] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Content.Connector] CHECK CONSTRAINT [FK_Content.Connector_ParentContent]
GO
ALTER TABLE [dbo].[Content.History]  WITH CHECK ADD  CONSTRAINT [FK_Content.History_Content] FOREIGN KEY([ContentId])
REFERENCES [dbo].[Content] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Content.History] CHECK CONSTRAINT [FK_Content.History_Content]
GO
ALTER TABLE [dbo].[Content.Like]  WITH CHECK ADD  CONSTRAINT [FK_Content.Like_Content] FOREIGN KEY([ContentId])
REFERENCES [dbo].[Content] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Content.Like] CHECK CONSTRAINT [FK_Content.Like_Content]
GO
ALTER TABLE [dbo].[Content.Partner]  WITH CHECK ADD  CONSTRAINT [FK_Content.Partner_Codes.ContentPartnerType] FOREIGN KEY([PartnerTypeId])
REFERENCES [dbo].[Codes.ContentPartnerType] ([Id])
GO
ALTER TABLE [dbo].[Content.Partner] CHECK CONSTRAINT [FK_Content.Partner_Codes.ContentPartnerType]
GO
ALTER TABLE [dbo].[Content.Partner]  WITH CHECK ADD  CONSTRAINT [FK_Content.Partner_Content] FOREIGN KEY([ContentId])
REFERENCES [dbo].[Content] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Content.Partner] CHECK CONSTRAINT [FK_Content.Partner_Content]
GO
ALTER TABLE [dbo].[Content.Reference]  WITH CHECK ADD  CONSTRAINT [FK_Content.Reference_Content] FOREIGN KEY([ParentId])
REFERENCES [dbo].[Content] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Content.Reference] CHECK CONSTRAINT [FK_Content.Reference_Content]
GO
ALTER TABLE [dbo].[Content.Resource]  WITH CHECK ADD  CONSTRAINT [FK_Content.Resource_Content] FOREIGN KEY([ContentId])
REFERENCES [dbo].[Content] ([Id])
GO
ALTER TABLE [dbo].[Content.Resource] CHECK CONSTRAINT [FK_Content.Resource_Content]
GO
ALTER TABLE [dbo].[Content.Standard]  WITH CHECK ADD  CONSTRAINT [FK_Content.Standard_Content] FOREIGN KEY([ContentId])
REFERENCES [dbo].[Content] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Content.Standard] CHECK CONSTRAINT [FK_Content.Standard_Content]
GO
ALTER TABLE [dbo].[Content.Subscription]  WITH CHECK ADD  CONSTRAINT [FK_Content.Subscription_Codes.SubscriptionType] FOREIGN KEY([SubscriptionTypeId])
REFERENCES [dbo].[Codes.SubscriptionType] ([Id])
GO
ALTER TABLE [dbo].[Content.Subscription] CHECK CONSTRAINT [FK_Content.Subscription_Codes.SubscriptionType]
GO
ALTER TABLE [dbo].[Content.Subscription]  WITH CHECK ADD  CONSTRAINT [FK_Content.Subscription_Content] FOREIGN KEY([ContentId])
REFERENCES [dbo].[Content] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Content.Subscription] CHECK CONSTRAINT [FK_Content.Subscription_Content]
GO
ALTER TABLE [dbo].[Content.Supplement]  WITH CHECK ADD  CONSTRAINT [FK_Content.Supplement_Codes.ContentPrivilege] FOREIGN KEY([PrivilegeTypeId])
REFERENCES [dbo].[Codes.ContentPrivilege] ([Id])
GO
ALTER TABLE [dbo].[Content.Supplement] CHECK CONSTRAINT [FK_Content.Supplement_Codes.ContentPrivilege]
GO
ALTER TABLE [dbo].[Content.Supplement]  WITH CHECK ADD  CONSTRAINT [FK_Content.Supplement_Content] FOREIGN KEY([ParentId])
REFERENCES [dbo].[Content] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Content.Supplement] CHECK CONSTRAINT [FK_Content.Supplement_Content]
GO
ALTER TABLE [dbo].[Content.Tag]  WITH CHECK ADD  CONSTRAINT [FK_Content.Tag_Content] FOREIGN KEY([ContentId])
REFERENCES [dbo].[Content] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Content.Tag] CHECK CONSTRAINT [FK_Content.Tag_Content]
GO
ALTER TABLE [dbo].[Content.UserPrivilege]  WITH CHECK ADD  CONSTRAINT [FK_Content.UserPrivilege_Codes.ContentPartnerType] FOREIGN KEY([TypeId])
REFERENCES [dbo].[Codes.ContentPartnerType] ([Id])
GO
ALTER TABLE [dbo].[Content.UserPrivilege] CHECK CONSTRAINT [FK_Content.UserPrivilege_Codes.ContentPartnerType]
GO
ALTER TABLE [dbo].[Content.UserPrivilege]  WITH CHECK ADD  CONSTRAINT [FK_Content.UserPrivilege_Content] FOREIGN KEY([ContentId])
REFERENCES [dbo].[Content] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Content.UserPrivilege] CHECK CONSTRAINT [FK_Content.UserPrivilege_Content]
GO
ALTER TABLE [dbo].[Library]  WITH CHECK ADD  CONSTRAINT [FK_Library_Codes.OrgAccessLevel] FOREIGN KEY([OrgAccessLevel])
REFERENCES [dbo].[Codes.LibraryAccessLevel] ([Id])
GO
ALTER TABLE [dbo].[Library] CHECK CONSTRAINT [FK_Library_Codes.OrgAccessLevel]
GO
ALTER TABLE [dbo].[Library]  WITH CHECK ADD  CONSTRAINT [FK_Library_Codes.PublicAccessLevel] FOREIGN KEY([PublicAccessLevel])
REFERENCES [dbo].[Codes.LibraryAccessLevel] ([Id])
GO
ALTER TABLE [dbo].[Library] CHECK CONSTRAINT [FK_Library_Codes.PublicAccessLevel]
GO
ALTER TABLE [dbo].[Library]  WITH CHECK ADD  CONSTRAINT [FK_Library_Library.Type] FOREIGN KEY([LibraryTypeId])
REFERENCES [dbo].[Library.Type] ([Id])
GO
ALTER TABLE [dbo].[Library] CHECK CONSTRAINT [FK_Library_Library.Type]
GO
ALTER TABLE [dbo].[Library.Comment]  WITH CHECK ADD  CONSTRAINT [FK_Library.Comment_Library] FOREIGN KEY([LibraryId])
REFERENCES [dbo].[Library] ([Id])
GO
ALTER TABLE [dbo].[Library.Comment] CHECK CONSTRAINT [FK_Library.Comment_Library]
GO
ALTER TABLE [dbo].[Library.Content]  WITH CHECK ADD  CONSTRAINT [FK_Library.Content_Library.Section] FOREIGN KEY([LibrarySectionId])
REFERENCES [dbo].[Library.Section] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Library.Content] CHECK CONSTRAINT [FK_Library.Content_Library.Section]
GO
ALTER TABLE [dbo].[Library.ExternalSection]  WITH CHECK ADD  CONSTRAINT [FK_Library.ExternalSection_Library] FOREIGN KEY([LibraryId])
REFERENCES [dbo].[Library] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Library.ExternalSection] CHECK CONSTRAINT [FK_Library.ExternalSection_Library]
GO
ALTER TABLE [dbo].[Library.ExternalSection]  WITH CHECK ADD  CONSTRAINT [FK_Table_1_Library.Section_ExtSectionId] FOREIGN KEY([ExternalSectionId])
REFERENCES [dbo].[Library.Section] ([Id])
GO
ALTER TABLE [dbo].[Library.ExternalSection] CHECK CONSTRAINT [FK_Table_1_Library.Section_ExtSectionId]
GO
ALTER TABLE [dbo].[Library.Invitation]  WITH CHECK ADD  CONSTRAINT [FK_Library.Invitation_Library] FOREIGN KEY([LibraryId])
REFERENCES [dbo].[Library] ([Id])
GO
ALTER TABLE [dbo].[Library.Invitation] CHECK CONSTRAINT [FK_Library.Invitation_Library]
GO
ALTER TABLE [dbo].[Library.Like]  WITH CHECK ADD  CONSTRAINT [FK_Library.Like_Library] FOREIGN KEY([LibraryId])
REFERENCES [dbo].[Library] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Library.Like] CHECK CONSTRAINT [FK_Library.Like_Library]
GO
ALTER TABLE [dbo].[Library.Member]  WITH CHECK ADD  CONSTRAINT [FK_Library.Member_Codes.LibraryMemberType] FOREIGN KEY([MemberTypeId])
REFERENCES [dbo].[Codes.LibraryMemberType] ([Id])
GO
ALTER TABLE [dbo].[Library.Member] CHECK CONSTRAINT [FK_Library.Member_Codes.LibraryMemberType]
GO
ALTER TABLE [dbo].[Library.Member]  WITH CHECK ADD  CONSTRAINT [FK_Library.Member_Library] FOREIGN KEY([LibraryId])
REFERENCES [dbo].[Library] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Library.Member] CHECK CONSTRAINT [FK_Library.Member_Library]
GO
ALTER TABLE [dbo].[Library.Resource]  WITH CHECK ADD  CONSTRAINT [FK_Library.Resource_Library.Section] FOREIGN KEY([LibrarySectionId])
REFERENCES [dbo].[Library.Section] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Library.Resource] CHECK CONSTRAINT [FK_Library.Resource_Library.Section]
GO
ALTER TABLE [dbo].[Library.Section]  WITH CHECK ADD  CONSTRAINT [FK_Library.Section_Library] FOREIGN KEY([LibraryId])
REFERENCES [dbo].[Library] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Library.Section] CHECK CONSTRAINT [FK_Library.Section_Library]
GO
ALTER TABLE [dbo].[Library.Section]  WITH CHECK ADD  CONSTRAINT [FK_Library.Section_Library.SectionType] FOREIGN KEY([SectionTypeId])
REFERENCES [dbo].[Library.SectionType] ([Id])
GO
ALTER TABLE [dbo].[Library.Section] CHECK CONSTRAINT [FK_Library.Section_Library.SectionType]
GO
ALTER TABLE [dbo].[Library.SectionComment]  WITH CHECK ADD  CONSTRAINT [FK_Library.SectionComment_LibrarySection] FOREIGN KEY([SectionId])
REFERENCES [dbo].[Library.Section] ([Id])
GO
ALTER TABLE [dbo].[Library.SectionComment] CHECK CONSTRAINT [FK_Library.SectionComment_LibrarySection]
GO
ALTER TABLE [dbo].[Library.SectionLike]  WITH CHECK ADD  CONSTRAINT [FK_Library.SectionLike_LibrarySection] FOREIGN KEY([SectionId])
REFERENCES [dbo].[Library.Section] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Library.SectionLike] CHECK CONSTRAINT [FK_Library.SectionLike_LibrarySection]
GO
ALTER TABLE [dbo].[Library.SectionMember]  WITH CHECK ADD  CONSTRAINT [FK_Library.SectionMember_Codes.LibraryMemberType] FOREIGN KEY([MemberTypeId])
REFERENCES [dbo].[Codes.LibraryMemberType] ([Id])
GO
ALTER TABLE [dbo].[Library.SectionMember] CHECK CONSTRAINT [FK_Library.SectionMember_Codes.LibraryMemberType]
GO
ALTER TABLE [dbo].[Library.SectionMember]  WITH CHECK ADD  CONSTRAINT [FK_Library.SectionMember_Library.Section] FOREIGN KEY([LibrarySectionId])
REFERENCES [dbo].[Library.Section] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Library.SectionMember] CHECK CONSTRAINT [FK_Library.SectionMember_Library.Section]
GO
ALTER TABLE [dbo].[Library.SectionSubscription]  WITH CHECK ADD  CONSTRAINT [FK_Library.SectionSubscription_Codes.SubscriptionType] FOREIGN KEY([SubscriptionTypeId])
REFERENCES [dbo].[Codes.SubscriptionType] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Library.SectionSubscription] CHECK CONSTRAINT [FK_Library.SectionSubscription_Codes.SubscriptionType]
GO
ALTER TABLE [dbo].[Library.SectionSubscription]  WITH CHECK ADD  CONSTRAINT [FK_Library.SectionSubscription_Library.Section] FOREIGN KEY([SectionId])
REFERENCES [dbo].[Library.Section] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Library.SectionSubscription] CHECK CONSTRAINT [FK_Library.SectionSubscription_Library.Section]
GO
ALTER TABLE [dbo].[Library.Subscription]  WITH CHECK ADD  CONSTRAINT [FK_Library.Subscription_Codes.SubscriptionType] FOREIGN KEY([SubscriptionTypeId])
REFERENCES [dbo].[Codes.SubscriptionType] ([Id])
GO
ALTER TABLE [dbo].[Library.Subscription] CHECK CONSTRAINT [FK_Library.Subscription_Codes.SubscriptionType]
GO
ALTER TABLE [dbo].[Library.Subscription]  WITH CHECK ADD  CONSTRAINT [FK_Library.Subscription_Library] FOREIGN KEY([LibraryId])
REFERENCES [dbo].[Library] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Library.Subscription] CHECK CONSTRAINT [FK_Library.Subscription_Library]
GO
ALTER TABLE [dbo].[Object.Member]  WITH CHECK ADD  CONSTRAINT [FK_Object.Member_Codes.ObjectMemberType] FOREIGN KEY([MemberTypeId])
REFERENCES [dbo].[Codes.ObjectMemberType] ([Id])
GO
ALTER TABLE [dbo].[Object.Member] CHECK CONSTRAINT [FK_Object.Member_Codes.ObjectMemberType]
GO
ALTER TABLE [dbo].[Object.Member]  WITH CHECK ADD  CONSTRAINT [FK_Object.Member_Object.Connector] FOREIGN KEY([ParentRowId])
REFERENCES [dbo].[Object.Connector] ([RelatedRowId])
GO
ALTER TABLE [dbo].[Object.Member] CHECK CONSTRAINT [FK_Object.Member_Object.Connector]
GO
ALTER TABLE [dbo].[Organization]  WITH CHECK ADD  CONSTRAINT [FK_Organization_Organization] FOREIGN KEY([Id])
REFERENCES [dbo].[Organization] ([Id])
GO
ALTER TABLE [dbo].[Organization] CHECK CONSTRAINT [FK_Organization_Organization]
GO
ALTER TABLE [dbo].[Organization.Member]  WITH CHECK ADD  CONSTRAINT [FK_Organization.Member_Codes.OrgMemberType] FOREIGN KEY([OrgMemberTypeId])
REFERENCES [dbo].[Codes.OrgMemberType] ([Id])
GO
ALTER TABLE [dbo].[Organization.Member] CHECK CONSTRAINT [FK_Organization.Member_Codes.OrgMemberType]
GO
ALTER TABLE [dbo].[Organization.Member]  WITH CHECK ADD  CONSTRAINT [FK_Organization.Member_Organization] FOREIGN KEY([OrgId])
REFERENCES [dbo].[Organization] ([Id])
GO
ALTER TABLE [dbo].[Organization.Member] CHECK CONSTRAINT [FK_Organization.Member_Organization]
GO
ALTER TABLE [dbo].[Organization.MemberRole]  WITH CHECK ADD  CONSTRAINT [FK_Organization.MemberRole_Codes.OrgMemberRole] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Codes.OrgMemberRole] ([Id])
GO
ALTER TABLE [dbo].[Organization.MemberRole] CHECK CONSTRAINT [FK_Organization.MemberRole_Codes.OrgMemberRole]
GO
ALTER TABLE [dbo].[Organization.MemberRole]  WITH CHECK ADD  CONSTRAINT [FK_Organization.MemberRole_Organization.Member] FOREIGN KEY([OrgMemberId])
REFERENCES [dbo].[Organization.Member] ([Id])
GO
ALTER TABLE [dbo].[Organization.MemberRole] CHECK CONSTRAINT [FK_Organization.MemberRole_Organization.Member]
GO
ALTER TABLE [dbo].[OrganizationRequest]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationRequest_Organization] FOREIGN KEY([OrgId])
REFERENCES [dbo].[Organization] ([Id])
GO
ALTER TABLE [dbo].[OrganizationRequest] CHECK CONSTRAINT [FK_OrganizationRequest_Organization]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'guest or auth' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Activity.OrgSummary', @level2type=N'COLUMN',@level2name=N'IsLoggedIn'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'could be a library, content item, or others' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AppGroup.RelatedObject', @level2type=N'COLUMN',@level2name=N'RelatedObjectId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnHidden', @value=0 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'Id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnOrder', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'Id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnWidth', @value=615 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'Id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnHidden', @value=0 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'ObjectName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnOrder', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'ObjectName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnWidth', @value=4155 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'ObjectName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnHidden', @value=0 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'DisplayName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnOrder', @value=3 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'DisplayName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnWidth', @value=2835 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'DisplayName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnHidden', @value=0 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'Description'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnOrder', @value=6 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'Description'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnWidth', @value=2835 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'Description'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnHidden', @value=0 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'Active'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnOrder', @value=7 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'Active'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnWidth', @value=705 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'Active'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'types include form, channel (path, directory)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'ObjectType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnHidden', @value=0 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'Created'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnOrder', @value=8 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'Created'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnWidth', @value=2400 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'Created'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DefaultView', @value=0x02 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Filter', @value=NULL , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_OrderBy', @value=N'ApplicationObject.id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_OrderByOn', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Orientation', @value=NULL , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_RowHeight', @value=525 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_TableMaxRecords', @value=10000 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK to AppItemType table' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Content', @level2type=N'COLUMN',@level2name=N'TypeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track last user to update an org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Content', @level2type=N'COLUMN',@level2name=N'LastUpdatedById'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track last user to update an org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Content', @level2type=N'COLUMN',@level2name=N'Approved'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track last user to update an org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Content', @level2type=N'COLUMN',@level2name=N'ApprovedById'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK to AppItemType table' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Content.Reference', @level2type=N'COLUMN',@level2name=N'ParentId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track last user to update an org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Content.Reference', @level2type=N'COLUMN',@level2name=N'LastUpdatedById'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK to AppItemType table' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Content.Supplement', @level2type=N'COLUMN',@level2name=N'ParentId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track last user to update an org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Content.Supplement', @level2type=N'COLUMN',@level2name=N'LastUpdatedById'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PK, but set by creator' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ContentType', @level2type=N'COLUMN',@level2name=N'Id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'do we need to distinguish between upload date and actual last mod date of the file?' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Document.Version', @level2type=N'COLUMN',@level2name=N'FileDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Individual or Library' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Library.Invitation', @level2type=N'COLUMN',@level2name=N'InvitationType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Individual or Member' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Member.Invitation', @level2type=N'COLUMN',@level2name=N'InvitationType'
GO
