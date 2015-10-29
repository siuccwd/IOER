USE [master]
GO
/****** Object:  Database [LinkChecker]    Script Date: 9/27/2015 11:22:15 AM ******/
CREATE DATABASE [LinkChecker] ON  PRIMARY 
( NAME = N'LinkChecker', FILENAME = N'C:\sql2008Data\Data\LinkChecker.mdf' , SIZE = 36864KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'LinkChecker_log', FILENAME = N'C:\sql2008Data\Logs\LinkChecker_log.ldf' , SIZE = 123648KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [LinkChecker] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [LinkChecker].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [LinkChecker] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [LinkChecker] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [LinkChecker] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [LinkChecker] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [LinkChecker] SET ARITHABORT OFF 
GO
ALTER DATABASE [LinkChecker] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [LinkChecker] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [LinkChecker] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [LinkChecker] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [LinkChecker] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [LinkChecker] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [LinkChecker] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [LinkChecker] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [LinkChecker] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [LinkChecker] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [LinkChecker] SET  DISABLE_BROKER 
GO
ALTER DATABASE [LinkChecker] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [LinkChecker] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [LinkChecker] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [LinkChecker] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [LinkChecker] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [LinkChecker] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [LinkChecker] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [LinkChecker] SET RECOVERY FULL 
GO
ALTER DATABASE [LinkChecker] SET  MULTI_USER 
GO
ALTER DATABASE [LinkChecker] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [LinkChecker] SET DB_CHAINING OFF 
GO
EXEC sys.sp_db_vardecimal_storage_format N'LinkChecker', N'ON'
GO
USE [LinkChecker]
GO
/****** Object:  User [lrReader]    Script Date: 9/27/2015 11:22:16 AM ******/
CREATE USER [lrReader] FOR LOGIN [lrReader] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [lrAdmin]    Script Date: 9/27/2015 11:22:16 AM ******/
CREATE USER [lrAdmin] FOR LOGIN [lrAdmin] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_datareader] ADD MEMBER [lrReader]
GO
/****** Object:  StoredProcedure [dbo].[Known404GetAll]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/12/2015
-- Description:	Gets all Known 404 pages
-- =============================================
CREATE PROCEDURE [dbo].[Known404GetAll]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, Url, IsRegex, Created, CreatedBy, LastUpdated, LastUpdatedBy
	FROM Known404Pages
END

GO
/****** Object:  StoredProcedure [dbo].[KnownBadContentGetAll]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 8/17/2015
-- Description:	Retrieve all known bad content from database
-- =============================================
CREATE PROCEDURE [dbo].[KnownBadContentGetAll]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, HostName, Content, Created, CreatedBy, LastUpdated, LastUpdatedBy
	FROM KnownBadContent
END

GO
/****** Object:  StoredProcedure [dbo].[KnownBadTitleGetAll]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/15/2015
-- Description:	Get all known Bad Titles
-- =============================================
CREATE PROCEDURE [dbo].[KnownBadTitleGetAll]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, HostName, Title, TitleIsRegex, Created, CreatedBy,
		LastUpdated, LastUpdatedBy
	FROM KnownBadTitle
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.CollectionAdd]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/6/2014
-- Description:	Add resource to a collection
-- =============================================
CREATE PROCEDURE [dbo].[Resource.CollectionAdd]
	@ResourceIntId int, @LibrarySectionId int, @Created datetime, @CreatedById int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO [IsleContent].[dbo].[Library.Resource] (ResourceIntId, LibrarySectionId, IsActive, Created, CreatedById)
	VALUES (@ResourceIntId, @LibrarySectionId, 'True', @Created, @CreatedById)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.CollectionGet]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/6/2013
-- Description:	Get list of collections a resource belongs to
-- =============================================
CREATE PROCEDURE [dbo].[Resource.CollectionGet]
	@ResourceIntId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, LibrarySectionId, ResourceIntId, IsActive, Comment, Created, CreatedById
	FROM [IsleContent].[dbo].[Library.Resource]
	WHERE ResourceIntId = @ResourceIntId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LinkAddUpdate]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/11/2013
-- Description:	Update Resource.Links with new and updated resource URLs
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LinkAddUpdate]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Update existing resources
	UPDATE [Resource.Link]
	SET HostName = (
		SELECT value
		FROM Split(res.ResourceUrl,'/')
		WHERE ndx = 3)
	FROM [Resource] res
	WHERE [Resource.Link].ResourceIntId = res.Id AND HostName <> (
		SELECT value
		FROM Split(res.ResourceUrl,'/')
		WHERE ndx = 3)
	
	-- Add new resources
	INSERT INTO [Resource.Link] (ResourceIntId, HostName)
	SELECT Id, (SELECT value
				FROM Split(res.ResourceUrl,'/')
				WHERE ndx = 3)
	FROM [Resource] res
	LEFT JOIN [Resource.Link] rl ON res.Id = rl.ResourceIntId
	WHERE rl.ResourceIntId IS NULL
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LinkGet]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/12/2013
-- Description:	Get a Resource.Link row
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LinkGet]
	@ResourceIntId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ResourceIntId, LastCheckDate, HostName, IsDeleted, NbrDnsErrors, NbrTimeouts, NbrInternalServerErrors, NbrUnableToConnect, ResourceUrl
	FROM [Resource.Link] rl
	INNER JOIN [Resource] res ON rl.ResourceIntId = res.Id
	WHERE ResourceIntId = @ResourceIntId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LinkGetByUrl]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/6/2014
-- Description:	Get ResourceLink by URL
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LinkGetByUrl]
	@Url varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT ResourceIntId, LastCheckDate, HostName, IsDeleted, NbrDnsErrors, NbrTimeouts, NbrInternalServerErrors, NbrUnableToConnect, ResourceUrl
	FROM [Resource.Link] rl
	INNER JOIN [Isle_IOER].dbo.[Resource] res ON rl.ResourceIntId = res.Id
	WHERE res.ResourceUrl = @Url
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LinkSelectForPhase2]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/12/2013
-- Description:	SELECT resources which need a phase 2 check
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LinkSelectForPhase2]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT ResourceIntId, LastCheckDate, HostName, IsDeleted, NbrDnsErrors, NbrTimeouts, NbrInternalServerErrors, NbrUnableToConnect, ResourceUrl
	FROM [Resource.Link] rl
	INNER JOIN [Isle_IOER].dbo.[Resource] res ON rl.ResourceIntId = res.Id
	WHERE rl.IsDeleted <> 'True' AND
		NbrDnsErrors > 0 OR NbrTimeouts > 0 OR NbrInternalServerErrors > 0 OR NbrUnableToConnect > 0
	ORDER BY LastCheckDate DESC, ResourceIntId ASC
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LinkSelectLeastRecentlyChecked]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/12/2013
-- Description:	SELECT TOP x (non-deleted) least recently checked resources
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LinkSelectLeastRecentlyChecked]
	@MaxRows int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP (@MaxRows) ResourceIntId, LastCheckDate, HostName, IsDeleted, NbrDnsErrors, NbrTimeouts, NbrInternalServerErrors, NbrUnableToConnect, ResourceUrl
	FROM [Resource.Link] rl
	INNER JOIN [Isle_IOER].dbo.[Resource] res ON rl.ResourceIntId = res.Id
	WHERE rl.IsDeleted <> 'True'
	ORDER BY LastCheckDate DESC, ResourceIntId ASC
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LinkUpdate]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/12/2013
-- Description:	Update a Resource.Link row
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LinkUpdate]
	@ResourceIntId int,
	@LastCheckDate datetime,
	@IsDeleted int,
	@NbrDnsErrors int,
	@NbrTimeouts int,
	@NbrInternalServerErrors int,
	@NbrUnableToConnect int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE [Resource.Link]
	SET LastCheckDate = @LastCheckDate,
		IsDeleted = @IsDeleted,
		NbrDnsErrors = @NbrDnsErrors,
		NbrTimeouts = @NbrTimeouts,
		NbrInternalServerErrors = @NbrInternalServerErrors,
		NbrUnableToConnect = @NbrUnableToConnect
	WHERE ResourceIntId = @ResourceIntId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.RedirectAdd]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/5/2014
-- Description:	Add row to Resource.Redirect table
-- =============================================
CREATE PROCEDURE [dbo].[Resource.RedirectAdd] 
	@ResourceIntId int, @OldUrl varchar(500), @NewUrl varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @RowCount int
	SELECT @RowCount = count(*)
	FROM [Resource.Redirect]
	WHERE ResourceIntId = @ResourceIntId

	IF @RowCount IS NULL OR @RowCount = 0 BEGIN
		INSERT INTO [Resource.Redirect] (ResourceIntId, OldUrl, NewUrl, Created)
		VALUES (@ResourceIntId, @OldUrl, @NewUrl, getdate())
	END
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.RedirectDelete]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/5/2014
-- Description:	Delete Row from [Resource.Redirect] table
-- =============================================
CREATE PROCEDURE [dbo].[Resource.RedirectDelete]
	@ResourceIntId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DELETE FROM [Resource.Redirect]
	WHERE ResourceIntId = @ResourceIntId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.RedirectSelect]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/5/2014
-- Description:	Select Rows from [Resource.Redirect]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.RedirectSelect] 
	@filter varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Sql varchar(1000)
	SET @Sql = 'SELECT ResourceIntId, OldUrl, NewUrl, Created
FROM [Resource.Redirect]
' + @filter

	PRINT @Sql
	EXEC(@Sql)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.UndoBadLinkCheck]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 12/10/2013
-- Description:	Build list of incorrectly deleted resourceIntIds for readding to ElasticSearch.
--				Modify the WHERE clause of the query used by the ListOfResources cursor to select the resourceIntIds
-- =============================================
CREATE PROCEDURE [dbo].[Resource.UndoBadLinkCheck]
	@DoUndelete bit = 'False'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	CREATE TABLE #Resources (ResourceIds varchar(max))
	DECLARE @ResourceIds varchar(max),@ResourceId int,@ResourceUrl varchar(500),@NbrResources int
	SET @ResourceIds = ''
	SET @NbrResources = 0
	DECLARE ListOfResources CURSOR FOR
		SELECT ResourceIntId, ResourceUrl
		FROM [Resource.Link]
		INNER JOIN [Resource] ON [Resource.Link].ResourceIntId = [Resource].Id
		WHERE IsDeleted='True' AND (ResourceUrl LIKE '%title%' OR ResourceUrl LIKE '%docid%' OR ResourceUrl LIKE '%grade%')
	OPEN ListOfResources
	FETCH NEXT FROM ListOfResources INTO @ResourceId,@ResourceUrl
	WHILE @@FETCH_STATUS = 0 BEGIN
		SET @ResourceIds = @ResourceIds + convert(varchar(10),@ResourceId)+','
		SET @NbrResources = @NbrResources + 1
		IF @NbrResources >= 200 BEGIN
			INSERT INTO #Resources (ResourceIds) VALUES (left(@ResourceIds,len(@ResourceIds)-1))
			SET @ResourceIds = ''
			SET @NbrResources = 0
		END
		IF @DoUndelete = 'True' BEGIN
			UPDATE [Resource.Link]
			SET IsDeleted = 'False'
			WHERE ResourceIntId = @ResourceId
		END
		FETCH NEXT FROM ListOfResources INTO @ResourceId,@ResourceUrl
	END
	IF @NbrResources > 0 BEGIN
		INSERT INTO #Resources (ResourceIds) VALUES (left(@ResourceIds,len(@ResourceIds)-1))
	END
	CLOSE ListOfResources
	DEALLOCATE ListOfResources
	SELECT * FROM #Resources
	DROP TABLE #Resources
END

GO
/****** Object:  UserDefinedFunction [dbo].[Split]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/11/2013
-- Description:	Split a string
-- =============================================
CREATE FUNCTION [dbo].[Split]
(	
	@string varchar(max),
	@delimiter varchar(32)
)
RETURNS @t TABLE (ndx int IDENTITY(1,1), value varchar(max))
AS BEGIN
	DECLARE @lenString int
	WHILE (LEN(@string) > 0) BEGIN
		SET @lenString = 
			(CASE CHARINDEX(@delimiter, @string)
				WHEN 0 THEN LEN(@string)
				ELSE (CHARINDEX(@delimiter, @string) - 1)
			END)
			
		INSERT INTO @t (value)
		SELECT SUBSTRING(@string, 1, @lenString)
		
		SELECT @string = 
			(CASE (LEN(@string) - @lenString)
				WHEN 0 THEN ''
				ELSE RIGHT(@string, LEN(@string) - @lenString - 1)
			END)
	END --WHILE
	
	UPDATE @t
	SET value = NULL
	WHERE len(value) = 0
	
	RETURN
END

GO
/****** Object:  Table [dbo].[Audit.Report]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Audit.Report](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Audit.Report] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Audit.ReportDetail]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.ReportDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ReportId] [int] NULL,
	[URI] [varchar](500) NULL,
	[MessageType] [char](1) NULL,
	[MessageRouting] [varchar](2) NULL,
	[Message] [varchar](200) NULL,
 CONSTRAINT [PK_Audit.ReportDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Known404Pages]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Known404Pages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Url] [varchar](500) NULL,
	[IsRegEx] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
 CONSTRAINT [PK_Known404Pages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[KnownBadContent]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[KnownBadContent](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[HostName] [varchar](500) NULL,
	[Content] [varchar](max) NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
 CONSTRAINT [PK_KnownBadContent] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[KnownBadTitle]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[KnownBadTitle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[HostName] [varchar](500) NULL,
	[Title] [varchar](500) NULL,
	[TitleIsRegex] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
 CONSTRAINT [PK_KnownBadTitle] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.Link]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Link](
	[ResourceIntId] [int] NOT NULL,
	[LastCheckDate] [datetime] NULL CONSTRAINT [DF_Resource.Links_LastCheckDate]  DEFAULT ('2000-01-01'),
	[HostName] [varchar](300) NULL,
	[IsDeleted] [bit] NULL CONSTRAINT [DF_Resource.Links_IsDeleted]  DEFAULT ((0)),
	[NbrDnsErrors] [int] NULL CONSTRAINT [DF_Resource.Links_NbrDnsErrors]  DEFAULT ((0)),
	[NbrTimeouts] [int] NULL CONSTRAINT [DF_Resource.Links_NbrTimeouts]  DEFAULT ((0)),
	[NbrInternalServerErrors] [int] NULL CONSTRAINT [DF_Resource.Links_NbrInternalServerErrors]  DEFAULT ((0)),
	[NbrUnableToConnect] [int] NULL CONSTRAINT [DF_Resource.Link_NbrUnableToConnect]  DEFAULT ((0)),
 CONSTRAINT [PK_Resource.Links] PRIMARY KEY CLUSTERED 
(
	[ResourceIntId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.Redirect]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Redirect](
	[ResourceIntId] [int] NOT NULL,
	[OldUrl] [varchar](500) NULL,
	[NewUrl] [varchar](500) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Resource.Redirect] PRIMARY KEY CLUSTERED 
(
	[ResourceIntId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [dbo].[Resource]    Script Date: 9/27/2015 11:22:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Resource]
AS
SELECT     RowId, ResourceUrl, ViewCount, FavoriteCount, Created, LastUpdated, IsActive, HasPathwayGradeLevel, Id
FROM         Isle_IOER.dbo.Resource

GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[41] 4[21] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Resource (Isle_IOER.dbo)"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 125
               Right = 241
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Resource'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Resource'
GO
USE [master]
GO
ALTER DATABASE [LinkChecker] SET  READ_WRITE 
GO
