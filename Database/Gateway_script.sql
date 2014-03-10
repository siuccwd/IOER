USE [Gateway]
GO
/****** Object:  User [lrAdmin]    Script Date: 3/9/2014 10:00:58 PM ******/
CREATE USER [lrAdmin] FOR LOGIN [lrAdmin] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [lrReader]    Script Date: 3/9/2014 10:00:58 PM ******/
CREATE USER [lrReader] FOR LOGIN [lrReader] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [lrAdmin]
GO
ALTER ROLE [db_datareader] ADD MEMBER [lrReader]
GO
/****** Object:  StoredProcedure [dbo].[AppGroup.MemberDelete]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AppGroup.MemberDelete]
        @id int
As
DELETE FROM [AppGroup.Member]
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.MemberGet]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[AppGroup.MemberGet] 1, 0, 0, ''

[AppGroup.MemberGet] 0, 0 ,0 , ''
*/
CREATE PROCEDURE [dbo].[AppGroup.MemberGet]
    @id int,
    @UserId int,
    @GroupId int,
    @GroupCode varchar(50)
As
if @id= 0 set @id = NULL
if @UserId= 0 set @UserId = NULL
if @GroupId= 0 set @GroupId = NULL
if @GroupCode= '' set @GroupCode = NULL

SELECT 
    base.id, 
    base.GroupId, 
    grp.Title As GroupName,
    base.UserId, base.OrgId, 
    base.Status,
    base.Category, 
    base.Comment,
    base.IsActive,
    base.Created, base.CreatedById, base.LastUpdated, base.LastUpdatedById,
    grp.GroupCode, grp.title as GroupName,
    users.FullName, users.SortName, users.Email As UserEmail, 
    users.OrganizationId
    ,isnull(orgs.Name, '') As gmOrganization
    ,isnull(orgs.ParentId, -1) as gmParentOrgId
    

FROM [AppGroup.Member] base
inner join [dbo].AppGroup grp on base.GroupId = grp.Id
INNER JOIN dbo.[LR.PatronOrgSummary] users ON base.UserId = users.UserId
Left Join Organization_Summary orgs on base.OrgId = orgs.Id
WHERE 
    (base.id = @id or @Id is null)
AND (base.UserId = @UserId or @UserId is null)    
AND (base.GroupId = @GroupId or @GroupId is null)
AND (grp.GroupCode = @GroupCode or @GroupCode is null)

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.MemberInsert]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AppGroup.MemberInsert]
            @GroupId int, 
            @UserId int, 
            @UserRowId varchar(50), 
            @OrgId int, 
            @Status varchar(50), 
            @Category varchar(25), 
            @IsActive bit, 
            @Comment varchar(500), 
            @CreatedById int

As

If @UserRowId = ''   SET @UserRowId = NULL 
If @OrgId = 0   SET @OrgId = NULL 
If @Status = ''   SET @Status = NULL 
If @Category = ''   SET @Category = NULL 
If @IsActive = 0   SET @IsActive = NULL 
If @Comment = ''   SET @Comment = NULL 
If @CreatedById = 0   SET @CreatedById = NULL 

INSERT INTO [AppGroup.Member] (

    GroupId, 
    UserId, 
    UserRowId, 
    OrgId, 
    Status, 
    Category, 
    IsActive, 
    Comment, 
    Created, 
    CreatedById, 
    LastUpdated, 
    LastUpdatedById
)
Values (

    @GroupId, 
    @UserId, 
    @UserRowId, 
    @OrgId, 
    @Status, 
    @Category, 
    @IsActive, 
    @Comment, 
    getdate(), 
    @CreatedById, 
    getdate(), 
    @CreatedById
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.MemberSearch]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
sp_who


-- ===========================================================

DECLARE @RC int, @Filter varchar(500),@SortOrder       varchar(200)
DECLARE @StartPageIndex int, @PageSize int, @totalRows int
set @SortOrder = ''
set @Filter = ' where groupId = 62 '
set @Filter = ' where GroupCode = ''OrgApprovers'' '

set @StartPageIndex = 1
set @PageSize = 15

EXECUTE @RC = [AppGroup.MemberSearch] 
   @Filter  ,@SortOrder, @StartPageIndex  ,@PageSize  ,@totalRows OUTPUT

select 'total rows = ' + convert(varchar,@totalRows)


*/
/* =================================================
= Get group members - 
=	Options:
= - all records for a group (by id or code)
= - all records for a contact (that is all groups of which the contact is a member)
= @StartPageIndex - starting page number. If interface is at 20 when next page is requested, this would be set to 21?
= @PageSize - number of records on a page
= @totalRows OUTPUT - total available rows. Used by interface to build a custom pager
= ------------------------------------------------------
= Modifications
= 13-05-27 mparsons - copies from VOS
-- ================================================= */
CREATE PROCEDURE [dbo].[AppGroup.MemberSearch]

    @Filter				varchar(500)
    ,@SortOrder       varchar(200)
		,@StartPageIndex int
		,@PageSize		int
		,@TotalRows			int OUTPUT
AS 

	SET NOCOUNT ON;
-- paging
DECLARE 
	@first_id			int
	,@startRow		int
	,@debugLevel	int
	,@SQL					varchar(1000)
	,@OrderBy			varchar(100)

-- =================================
Set @debugLevel = 4
if len(isnull(@SortOrder,'')) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by grp.Title, mbr.SortName '

CREATE TABLE #tempSearchTable(
	RowNumber int PRIMARY KEY IDENTITY(1,1) NOT NULL,
	GroupId int NOT NULL,
	GroupMemberId int NOT NULL,
	SortName varchar(100)
)

-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))

	set @SQL = 'SELECT gm.GroupId, gm.ID, mbr.SortName
	FROM [AppGroup.Member] gm
			Inner Join [AppGroup] grp on gm.GroupId = grp.Id
			Inner Join [LR.PatronOrgSummary] mbr on gm.UserId = mbr.UserId ' 
		+ @Filter

	if charindex( 'order by', lower(@Filter) ) = 0 
		set @SQL = 	@SQL + @OrderBy
		
  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL
  
	INSERT INTO #tempSearchTable (GroupId, GroupMemberId, SortName)
	exec (@sql)
	
 --print 'rows: ' + convert(varchar, @@ROWCOUNT)
  SELECT @TotalRows = @@ROWCOUNT
-- =================================

print 'added to temp table: ' + convert(varchar,@@rowcount)
if @debugLevel > 7 begin
	select * from #tempSearchTable
	end

if @StartPageIndex < 1		set @StartPageIndex = 1

-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex = 0		SET @StartPageIndex = 1
PRINT '@StartPageIndex = ' + convert(varchar,@StartPageIndex)

SET ROWCOUNT @StartPageIndex
SELECT @first_id = RowNumber FROM #tempSearchTable   ORDER BY RowNumber
PRINT '@first_id = ' + convert(varchar,@first_id)
if @first_id = 1 set @first_id = 0

--set max to return
SET ROWCOUNT @PageSize

SELECT     Distinct
RowNumber,
    grp.Title As GroupName, 
    grp.GroupCode,
    gm.GroupId, 
    gm.UserId, gm.OrgId, mbr.Organization,
    gm.ID,     gm.ID As GroupMemberId, 
    gm.Status, 
    gm.Category, 
    gm.Comment
-- Customer
      ,mbr.Lastname
      ,mbr.Fullname
      ,mbr.SortName
			,mbr.Email
			,mbr.Email As UserEmail 

From #tempSearchTable work
Inner Join [AppGroup.Member] gm on work.GroupMemberId = gm.Id
			Inner Join [AppGroup] grp on gm.GroupId = grp.Id
			Inner Join [LR.PatronOrgSummary] mbr on gm.UserId = mbr.UserId
			
WHERE RowNumber > @first_id 
order by RowNumber		

SET ROWCOUNT 0

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.MemberSelect]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[AppGroup.MemberSelect] 1, 0, '', 0

[AppGroup.MemberSelect] 0, 0, '', 0
*/
Create PROCEDURE [dbo].[AppGroup.MemberSelect]
        @GroupId    int,
        @UserId     int,
        @GroupCode  varchar(50),
        @CreatedById int
As

if @UserId= 0 set @UserId = NULL
if @GroupId= 0 set @GroupId = NULL
if @CreatedById= 0 set @CreatedById = NULL
if @GroupCode= '' set @GroupCode = NULL


SELECT 
    base.id, 
    base.GroupId, 
    grp.Title As GroupName,
    base.UserId, base.OrgId, 
    base.Status,
    base.Category, 
    base.Comment,
    base.IsActive,
    base.Created, base.CreatedById, base.LastUpdated, base.LastUpdatedById,
    grp.GroupCode, grp.title as GroupName,
    users.FullName, users.SortName, users.Email As UserEmail, 
    users.OrganizationId
    ,isnull(orgs.Name, '') As gmOrganization
    ,isnull(orgs.ParentId, -1) as gmParentOrgId
    

FROM [AppGroup.Member] base
inner join [dbo].AppGroup grp on base.GroupId = grp.Id
INNER JOIN dbo.[LR.PatronOrgSummary] users ON base.UserId = users.UserId
Left Join Organization_Summary orgs on base.OrgId = orgs.Id
WHERE 
    (base.GroupId = @GroupId or @GroupId is null)
AND (base.UserId = @UserId or @UserId is null)    
AND (base.CreatedById = @CreatedById or @CreatedById is null)
AND (grp.GroupCode = @GroupCode or @GroupCode is null)

order by grp.Title, users.FirstName, users.LastName

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.MemberUpdate]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Update Procedure for [AppGroup.Member] ---
CREATE PROCEDURE [dbo].[AppGroup.MemberUpdate]
        @id int,
        @Status varchar(50), 
        @Category varchar(25), 
        @IsActive bit, 
        @Comment varchar(500), 
        @LastUpdatedById int
As

If @Status = ''   SET @Status = NULL 
If @Category = ''   SET @Category = NULL 
If @IsActive = 0   SET @IsActive = NULL 
If @Comment = ''   SET @Comment = NULL 
If @LastUpdatedById = 0   SET @LastUpdatedById = NULL 

UPDATE [AppGroup.Member] 
SET 
    Status = @Status, 
    Category = @Category, 
    IsActive = @IsActive, 
    Comment = @Comment, 
    LastUpdated = getdate(), 
    LastUpdatedById = @LastUpdatedById
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.OrgApproversSelect]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
[AppGroup.OrgApproversSelect] 3

[AppGroup.OrgApproversSelect] 1
*/
Create PROCEDURE [dbo].[AppGroup.OrgApproversSelect]
    @OrgId int
As
Declare @parentId int

select @parentId = isnull(parentId,0) from Organization where Id = @OrgId
--if @parentId = 0  set @parentId = NULL
print 'parentId = ' + convert(varchar, isnull(@parentId,0))

SELECT 
    base.id, 
    base.GroupId, 
    grp.Title As GroupName,
    base.UserId, base.OrgId, 
    base.Status,
    base.Category, 
    base.Comment,
    base.IsActive,
    base.Created, base.CreatedById, base.LastUpdated, base.LastUpdatedById,
    grp.GroupCode, grp.title as GroupName,
    users.FullName, users.SortName, users.Email As UserEmail, 
    users.OrganizationId

FROM [AppGroup.Member] base
inner join [dbo].AppGroup grp on base.GroupId = grp.Id
INNER JOIN dbo.[LR.PatronOrgSummary] users ON base.UserId = users.UserId
WHERE grp.GroupCode = 'OrgApprovers'
And (
     users.OrganizationId = @OrgId 
      OR users.OrganizationId = @parentId
    )

order by users.SortName


GO
/****** Object:  StoredProcedure [dbo].[AppGroup.OrgMemberSearch]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



/*
sp_who


-- ===========================================================

DECLARE @RC int, @Filter varchar(500),@SortOrder       varchar(200)
DECLARE @StartPageIndex int, @PageSize int, @totalRows int
set @SortOrder = ''
set @Filter = ' where groupId = 62 '
set @Filter = ' where GroupCode = ''OrgApprovers'' '
set @Filter = ' '
set @StartPageIndex = 1
set @PageSize = 15

EXECUTE @RC = [AppGroup.OrgMemberSearch] 
   @Filter  ,@SortOrder, @StartPageIndex  ,@PageSize  ,@totalRows OUTPUT

select 'total rows = ' + convert(varchar,@totalRows)


*/
/* =================================================
= Get group org members - 
=	Options:
= - all records for a group (by id or code)
= - all records for a contact (that is all groups of which the contact is a member)
= @StartPageIndex - starting page number. If interface is at 20 when next page is requested, this would be set to 21?
= @PageSize - number of records on a page
= @totalRows OUTPUT - total available rows. Used by interface to build a custom pager
= ------------------------------------------------------
= Modifications
= 14-02-06 mparsons - created
-- ================================================= */
CREATE PROCEDURE [dbo].[AppGroup.OrgMemberSearch]

    @Filter				varchar(500)
    ,@SortOrder       varchar(200)
		,@StartPageIndex int
		,@PageSize		int
		,@TotalRows			int OUTPUT
AS 

	SET NOCOUNT ON;
-- paging
DECLARE 
	@first_id			int
	,@startRow		int
	,@debugLevel	int
	,@SQL					varchar(1000)
	,@OrderBy			varchar(100)

-- =================================
Set @debugLevel = 4
if len(isnull(@SortOrder,'')) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by grp.Title, mbr.Name '

CREATE TABLE #tempSearchTable(
	RowNumber int PRIMARY KEY IDENTITY(1,1) NOT NULL,
	GroupId int NOT NULL,
	GroupMemberId int NOT NULL,
	OrgId int NOT NULL,
	OrgName varchar(100)
)

-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))

	set @SQL = 'SELECT gm.GroupId, gm.ID, gm.OrgId, mbr.Name FROM [AppGroup.OrgMember] gm
			Inner Join [AppGroup] grp on gm.GroupId = grp.Id
			Inner Join [Organization_Summary] mbr on gm.OrgId = mbr.Id ' 
		+ @Filter

	if charindex( 'order by', lower(@Filter) ) = 0 
		set @SQL = 	@SQL + @OrderBy
		
  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL
  
	INSERT INTO #tempSearchTable (GroupId, GroupMemberId, OrgId, OrgName)
	exec (@sql)
	
 --print 'rows: ' + convert(varchar, @@ROWCOUNT)
  SELECT @TotalRows = @@ROWCOUNT
-- =================================

print 'added to temp table: ' + convert(varchar,@@rowcount)
if @debugLevel > 7 begin
	select * from #tempSearchTable
	end

if @StartPageIndex < 1		set @StartPageIndex = 1

-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex = 0		SET @StartPageIndex = 1
PRINT '@StartPageIndex = ' + convert(varchar,@StartPageIndex)

SET ROWCOUNT @StartPageIndex
SELECT @first_id = RowNumber FROM #tempSearchTable   ORDER BY RowNumber
PRINT '@first_id = ' + convert(varchar,@first_id)
if @first_id = 1 set @first_id = 0

--set max to return
SET ROWCOUNT @PageSize

SELECT     Distinct
RowNumber,
    grp.Title As GroupName, 
    grp.GroupCode,
    gm.GroupId, 
    gm.OrgId, mbr.Name as Organization,
    gm.ID,     
	gm.ID As GroupMemberId, 
	gm.Created,
	gm.CreatedById 

From #tempSearchTable work
Inner Join [AppGroup.OrgMember] gm on work.GroupMemberId = gm.Id
			Inner Join [AppGroup] grp on gm.GroupId = grp.Id
			Inner Join Organization_Summary mbr on gm.OrgId = mbr.id
			
WHERE RowNumber > @first_id 
order by RowNumber		

SET ROWCOUNT 0


GO
/****** Object:  StoredProcedure [dbo].[AppGroup.PrivilegeDelete]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AppGroup.PrivilegeDelete]
        @Id int
As
DELETE FROM [AppGroup.Privilege]
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.PrivilegeGet]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AppGroup.PrivilegeGet]
    @Id int
As
SELECT     GroupId, 
    ObjectId, 
    Id, 
    CreatePrivilege, 
    ReadPrivilege, 
    WritePrivilege, 
    DeletePrivilege, 
    AppendPrivilege, 
    AppendToPrivilege, 
    AssignPrivilege, 
    ApprovePrivilege, 
    SharePrivilege, 
    Created
FROM [AppGroup.Privilege]
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.PrivilegeInsert]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AppGroup.PrivilegeInsert]
            @GroupId int, 
            @ObjectId int, 
            @CreatePrivilege int, 
            @ReadPrivilege int, 
            @WritePrivilege int, 
            @DeletePrivilege int, 
            @AppendPrivilege int, 
            @AppendToPrivilege int, 
            @AssignPrivilege int, 
            @ApprovePrivilege int, 
            @SharePrivilege int
As

If @CreatePrivilege = 0   SET @CreatePrivilege = NULL 
If @ReadPrivilege = 0   SET @ReadPrivilege = NULL 
If @WritePrivilege = 0   SET @WritePrivilege = NULL 
If @DeletePrivilege = 0   SET @DeletePrivilege = NULL 
If @AppendPrivilege = 0   SET @AppendPrivilege = NULL 
If @AppendToPrivilege = 0   SET @AppendToPrivilege = NULL 
If @AssignPrivilege = 0   SET @AssignPrivilege = NULL 
If @ApprovePrivilege = 0   SET @ApprovePrivilege = NULL 
If @SharePrivilege = 0   SET @SharePrivilege = NULL 

INSERT INTO [AppGroup.Privilege] (

    GroupId, 
    ObjectId, 
    CreatePrivilege, 
    ReadPrivilege, 
    WritePrivilege, 
    DeletePrivilege, 
    AppendPrivilege, 
    AppendToPrivilege, 
    AssignPrivilege, 
    ApprovePrivilege, 
    SharePrivilege, 
    Created
)
Values (

    @GroupId, 
    @ObjectId, 
    @CreatePrivilege, 
    @ReadPrivilege, 
    @WritePrivilege, 
    @DeletePrivilege, 
    @AppendPrivilege, 
    @AppendToPrivilege, 
    @AssignPrivilege, 
    @ApprovePrivilege, 
    @SharePrivilege, 
    getdate()
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.PrivilegeSelect]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AppGroup.PrivilegeSelect]
  @GroupId int
As
SELECT 
    GroupId, 
    ObjectId, 
    Id, 
    CreatePrivilege, 
    ReadPrivilege, 
    WritePrivilege, 
    DeletePrivilege, 
    AppendPrivilege, 
    AppendToPrivilege, 
    AssignPrivilege, 
    ApprovePrivilege, 
    SharePrivilege, 
    Created
FROM [AppGroup.Privilege]
WHERE GroupId = @GroupId

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.PrivilegeUpdate]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Update Procedure for [AppGroup.Privilege] ---
CREATE PROCEDURE [dbo].[AppGroup.PrivilegeUpdate]
        @Id int, 
        @CreatePrivilege int, 
        @ReadPrivilege int, 
        @WritePrivilege int, 
        @DeletePrivilege int, 
        @AppendPrivilege int, 
        @AppendToPrivilege int, 
        @AssignPrivilege int, 
        @ApprovePrivilege int, 
        @SharePrivilege int
        
As

If @CreatePrivilege = 0   SET @CreatePrivilege = NULL 
If @ReadPrivilege = 0   SET @ReadPrivilege = NULL 
If @WritePrivilege = 0   SET @WritePrivilege = NULL 
If @DeletePrivilege = 0   SET @DeletePrivilege = NULL 
If @AppendPrivilege = 0   SET @AppendPrivilege = NULL 
If @AppendToPrivilege = 0   SET @AppendToPrivilege = NULL 
If @AssignPrivilege = 0   SET @AssignPrivilege = NULL 
If @ApprovePrivilege = 0   SET @ApprovePrivilege = NULL 
If @SharePrivilege = 0   SET @SharePrivilege = NULL 

UPDATE [AppGroup.Privilege] 
SET 
    CreatePrivilege = @CreatePrivilege, 
    ReadPrivilege = @ReadPrivilege, 
    WritePrivilege = @WritePrivilege, 
    DeletePrivilege = @DeletePrivilege, 
    AppendPrivilege = @AppendPrivilege, 
    AppendToPrivilege = @AppendToPrivilege, 
    AssignPrivilege = @AssignPrivilege, 
    ApprovePrivilege = @ApprovePrivilege, 
    SharePrivilege = @SharePrivilege
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.TeamMemberDelete]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AppGroup.TeamMemberDelete]
        @id int
As
DELETE FROM [AppGroup.TeamMember]
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.TeamMemberGet]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AppGroup.TeamMemberGet]
    @id int
As
SELECT     id, 
    GroupId, 
    UserId, 
    UserRowId, 
    CreatePrivilege, 
    ReadPrivilege, 
    WritePrivilege, 
    DeletePrivilege, 
    AppendPrivilege, 
    AppendToPrivilege, 
    AssignPrivilege, 
    ApprovePrivilege, 
    SharePrivilege, 
    Created, 
    CreatedById, 
    LastUpdated, 
    LastUpdatedById
FROM [AppGroup.TeamMember]
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.TeamMemberInsert]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AppGroup.TeamMemberInsert]
            @GroupId int, 
            @UserId int ,
            @CreatePrivilege int, 
            @ReadPrivilege int, 
            @WritePrivilege int, 
            @DeletePrivilege int, 
            @AppendPrivilege int, 
            @AppendToPrivilege int, 
            @AssignPrivilege int, 
            @ApprovePrivilege int, 
            @SharePrivilege int, 
            @CreatedById int,
            @UserRowId varchar(50)
As

If @UserRowId = ''   SET @UserRowId = NULL 
If @CreatePrivilege = 0   SET @CreatePrivilege = NULL 
If @ReadPrivilege = 0   SET @ReadPrivilege = NULL 
If @WritePrivilege = 0   SET @WritePrivilege = NULL 
If @DeletePrivilege = 0   SET @DeletePrivilege = NULL 
If @AppendPrivilege = 0   SET @AppendPrivilege = NULL 
If @AppendToPrivilege = 0   SET @AppendToPrivilege = NULL 
If @AssignPrivilege = 0   SET @AssignPrivilege = NULL 
If @ApprovePrivilege = 0   SET @ApprovePrivilege = NULL 
If @SharePrivilege = 0   SET @SharePrivilege = NULL 
If @CreatedById = 0   SET @CreatedById = NULL 

INSERT INTO [AppGroup.TeamMember] (

    GroupId, 
    UserId, 
    UserRowId, 
    CreatePrivilege, 
    ReadPrivilege, 
    WritePrivilege, 
    DeletePrivilege, 
    AppendPrivilege, 
    AppendToPrivilege, 
    AssignPrivilege, 
    ApprovePrivilege, 
    SharePrivilege, 
    Created, 
    CreatedById, 
    LastUpdated, 
    LastUpdatedById
)
Values (

    @GroupId, 
    @UserId, 
    @UserRowId, 
    @CreatePrivilege, 
    @ReadPrivilege, 
    @WritePrivilege, 
    @DeletePrivilege, 
    @AppendPrivilege, 
    @AppendToPrivilege, 
    @AssignPrivilege, 
    @ApprovePrivilege, 
    @SharePrivilege, 
    getdate(), 
    @CreatedById, 
    getdate(), 
    @CreatedById
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.TeamMemberSearch]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
SELECT     
    base.id, 
    base.Name, 
    OrgTypeId, orgt.Title as OrgType,
    parentId, 
    IsActive, 
    MainPhone, MainExtension,Fax, TTY, 
    WebSite, 
    Email, 
    LogoUrl, 
    Address,  Address2,  City, State, Zipcode, 
    base.Created, base.CreatedById, 
    base.LastUpdated,  base.LastUpdatedById, 
    base.RowId
FROM [Organization] base
inner join [Codes.OrgType] orgt on base.OrgTypeId = orgt.Id

GO



--=====================================================

DECLARE @RC int,@SortOrder varchar(100),@Filter varchar(5000)
DECLARE @StartPageIndex int, @PageSize int, @TotalRows int
--
set @SortOrder = ''

-- blind search 
set @Filter = ''
set @Filter = ' OrgTypeId= 2'

set @Filter = ' LastName like ''p%'' '

set @StartPageIndex = 1
set @PageSize = 55
--set statistics time on       
EXECUTE @RC = [AppGroup.TeamMemberSearch]
     @Filter,@SortOrder  ,@StartPageIndex  ,@PageSize, @TotalRows OUTPUT

select 'total rows = ' + convert(varchar,@TotalRows)

--set statistics time off       


*/


/* =============================================
      Description:      Org search
  Uses custom paging to only return enough rows for one page
     Options:

  @StartPageIndex - starting page number. If interface is at 20 when next
page is requested, this would be set to 21?
  @PageSize - number of records on a page
  @TotalRows OUTPUT - total available rows. Used by interface to build a
custom pager
  ------------------------------------------------------
Modifications
13-04-02 mparsons - new

*/

CREATE PROCEDURE [dbo].[AppGroup.TeamMemberSearch]
		@Filter           varchar(5000)
		,@SortOrder       varchar(200)
		,@StartPageIndex  int
		,@PageSize        int
		,@TotalRows       int OUTPUT

As

SET NOCOUNT ON;
-- paging
DECLARE
      @first_id               int
      ,@startRow        int
      ,@debugLevel      int
      ,@SQL             varchar(5000)
      ,@OrderBy         varchar(100)


-- =================================

Set @debugLevel = 4

if len(@SortOrder) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
       set @OrderBy = ' Order by grp.Title, mbr.SortName '

--===================================================
-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1

 
-- =================================
CREATE TABLE #tempSearchTable(
	RowNumber int PRIMARY KEY IDENTITY(1,1) NOT NULL,
	GroupId int NOT NULL,
	GroupMemberId int NOT NULL,
	SortName varchar(100)
)

-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))
	set @SQL = 'SELECT gm.GroupId, gm.ID, mbr.SortName
	FROM [AppGroup.TeamMember] gm
			Inner Join [AppGroup] grp on gm.GroupId = grp.Id
			Inner Join [LR.PatronOrgSummary] mbr on gm.UserId = mbr.UserId ' 
		+ @Filter
        
  if charindex( 'order by', lower(@Filter) ) = 0
    set @SQL = @SQL + ' ' + @OrderBy

  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL

  INSERT INTO #tempSearchTable (GroupId, GroupMemberId, SortName)
  exec (@SQL)
  --print 'rows: ' + convert(varchar, @@ROWCOUNT)
  SELECT @TotalRows = @@ROWCOUNT
-- =================================

print 'added to temp table: ' + convert(varchar,@TotalRows)
if @debugLevel > 7 begin
  select * from #tempSearchTable
  end

-- Calculate the range
--===================================================
PRINT '@StartPageIndex = ' + convert(varchar,@StartPageIndex)

SET ROWCOUNT @StartPageIndex
--SELECT @first_id = RowNumber FROM #tempSearchTable   ORDER BY RowNumber
SELECT @first_id = @StartPageIndex
PRINT '@first_id = ' + convert(varchar,@first_id)

if @first_id = 1 set @first_id = 0
--set max to return
SET ROWCOUNT @PageSize

-- ================================= 
SELECT     Distinct
RowNumber,
    grp.Title As GroupName, 
    grp.GroupCode,
    gm.GroupId, 
    gm.UserId, mbr.Organization,
    gm.ID,     gm.ID As GroupMemberId

-- Customer
      ,mbr.Lastname
      ,mbr.Fullname
      ,mbr.SortName
			,mbr.Email

From #tempSearchTable work
Inner Join [AppGroup.TeamMember] gm on work.GroupMemberId = gm.Id
			Inner Join [AppGroup] grp on gm.GroupId = grp.Id
			Inner Join [LR.PatronOrgSummary] mbr on gm.UserId = mbr.UserId
			
WHERE RowNumber > @first_id 
order by RowNumber		

SET ROWCOUNT 0

GO
/****** Object:  StoredProcedure [dbo].[AppGroup.TeamMemberUpdate]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Update Procedure for [AppGroup.TeamMember] ---
CREATE PROCEDURE [dbo].[AppGroup.TeamMemberUpdate]
        @id int,
        @CreatePrivilege int, 
        @ReadPrivilege int, 
        @WritePrivilege int, 
        @DeletePrivilege int, 
        @AppendPrivilege int, 
        @AppendToPrivilege int, 
        @AssignPrivilege int, 
        @ApprovePrivilege int, 
        @SharePrivilege int, 
        @LastUpdatedById int
As

If @CreatePrivilege = 0   SET @CreatePrivilege = NULL 
If @ReadPrivilege = 0   SET @ReadPrivilege = NULL 
If @WritePrivilege = 0   SET @WritePrivilege = NULL 
If @DeletePrivilege = 0   SET @DeletePrivilege = NULL 
If @AppendPrivilege = 0   SET @AppendPrivilege = NULL 
If @AppendToPrivilege = 0   SET @AppendToPrivilege = NULL 
If @AssignPrivilege = 0   SET @AssignPrivilege = NULL 
If @ApprovePrivilege = 0   SET @ApprovePrivilege = NULL 
If @SharePrivilege = 0   SET @SharePrivilege = NULL 
If @LastUpdatedById = 0   SET @LastUpdatedById = NULL 

UPDATE [AppGroup.TeamMember] 
SET 
    CreatePrivilege = @CreatePrivilege, 
    ReadPrivilege = @ReadPrivilege, 
    WritePrivilege = @WritePrivilege, 
    DeletePrivilege = @DeletePrivilege, 
    AppendPrivilege = @AppendPrivilege, 
    AppendToPrivilege = @AppendToPrivilege, 
    AssignPrivilege = @AssignPrivilege, 
    ApprovePrivilege = @ApprovePrivilege, 
    SharePrivilege = @SharePrivilege, 
    LastUpdated = getdate(), 
    LastUpdatedById = @LastUpdatedById
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[AppGroupDelete]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AppGroupDelete]
        @id int
As
DELETE FROM [AppGroup]
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[AppGroupGet]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*

*/
CREATE PROCEDURE [dbo].[AppGroupGet]
    @id int,
    @GroupCode varchar(50)
As
 
If @GroupCode = ''   SET @GroupCode = NULL 
If @id = 0   SET @id = NULL 

SELECT     
ap.id, GroupCode,
    ApplicationId,  
    ap.Title, 
    ap.Description, 
    ap.GroupTypeId, codes.Title as GroupType,
    ap.IsActive, 
    ContactId, OrgId,
    ParentGroupId, 
    ap.Created, 
    ap.CreatedById, 
    ap.LastUpdated, 
    ap.LastUpdatedById
FROM [AppGroup] ap
Inner join [Codes.GroupType] codes on ap.GroupTypeId = codes.id
WHERE 
    (ap.id = @id OR @Id Is Null)
And (GroupCode = @GroupCode or @GroupCode Is Null)


GO
/****** Object:  StoredProcedure [dbo].[AppGroupInsert]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AppGroupInsert]
            @GroupCode varchar(50),  
            @Title varchar(100), 
            @Description varchar(500), 
            @IsActive bit,
            @GroupTypeId int, 
            @ContactId int, 
            @OrgId int,
            @ParentGroupId int, 
            @CreatedById int
As
            --@ApplicationId int, 
declare @ApplicationId int 
--If @ApplicationId = 0   
SET @ApplicationId = 1 
If @GroupCode = ''   SET @GroupCode = newId() 
If @Title = ''   SET @Title = 'Noname group' 
If @Description = ''   SET @Description = NULL 
If @GroupTypeId = 0   SET @GroupTypeId = 1 
If @ContactId = 0   SET @ContactId = NULL 
If @OrgId = 0   SET @OrgId = NULL 
If @ParentGroupId = 0   SET @ParentGroupId = NULL 
If @CreatedById = 0   SET @CreatedById = NULL 

INSERT INTO [AppGroup] (

    ApplicationId, 
    GroupCode, 
    Title, 
    Description, 
    IsActive, GroupTypeId,
    ContactId, OrgId,
    ParentGroupId, 
    Created, 
    CreatedById, 
    LastUpdated, 
    LastUpdatedById
)
Values (

    @ApplicationId, 
    @GroupCode, 
    @Title, 
    @Description, 
    @IsActive, @GroupTypeId,
    @ContactId, @OrgId,
    @ParentGroupId, 
    getdate(), 
    @CreatedById, 
    getdate(), 
    @CreatedById
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[AppGroupSearch]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
sp_who

select * 
from Group

  (grp.ContactId = 566)  AND   (GroupType = 'PersonalGroup' OR GroupType = 'PersonalGroup_Archived' )

-- ===========================================================
DECLARE @RC int,@Filter varchar(500), @StartPageIndex int, @PageSize int, @totalRows int,@SortOrder varchar(100)
set @SortOrder = '' --' grp.id desc '
set @Filter = ' where groupId = 14 '
set @Filter = '  Where   (GroupType like ''Personal%'' )'
set @Filter = '  Where   (GroupType = ''PersonalGroup'' OR GroupType = ''PersonalGroup_Archived'' )'
set @Filter = '  (grp.ContactId = 566)  AND  (GroupType = ''PersonalGroup'' OR GroupType = ''PersonalGroup_Archived'' )'
set @Filter = ' '
                 
set @StartPageIndex = 1
set @PageSize = 15

exec [AppGroupSearch] @Filter, @SortOrder, @StartPageIndex  ,@PageSize  ,@totalRows OUTPUT

select 'total rows = ' + convert(varchar,@totalRows)


*/
/* =================================================
= Groups search
= Uses custom paging to only return enough rows for one page
=		@StartPageIndex - starting page number. If interface is at 20 when next page is requested, this would be set to 21?
=		@PageSize - number of records on a page
=		@totalRows OUTPUT - total available rows. Used by interface to build a custom pager
= ------------------------------------------------------
= Modifications
= 11-09-12 mparsons - Created 
-- ================================================= */
Create PROCEDURE [dbo].[AppGroupSearch]
    @Filter				    varchar(500)
    ,@SortOrder	      varchar(500)
		,@StartPageIndex  int
		,@PageSize		    int
		,@TotalRows			  int OUTPUT
AS 
DECLARE 
	@first_id			int
	,@startRow		int
	,@debugLevel	int
	,@SQL             varchar(5000)
  ,@OrderBy         varchar(100)

	SET NOCOUNT ON;

-- ==========================================================
Set @debugLevel = 4
if len(@SortOrder) > 0
	set @OrderBy = ' Order by ' + @SortOrder
else 
  set @OrderBy = ' Order by Title '
--===================================================
-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1

 
-- =================================
CREATE TABLE #tempWorkTable(
	RowNumber int PRIMARY KEY IDENTITY(1,1) NOT NULL,
	GroupId int NOT NULL,
	Title varchar(100)
)
-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end
 
set @SQL = 'SELECT grp.Id, grp.Title FROM [AppGroup]  grp
	  Left Join [LR.PatronOrgSummary] contact on grp.ContactId = contact.UserId 
	  Inner join [Codes.GroupType] codes on grp.GroupTypeId = codes.id
	  Left Join [AppGroup.TeamMember] atm on grp.Id = atm.GroupId ' 
	  + @Filter

if charindex( 'order by', lower(@Filter) ) = 0 
	set @SQL = 	@SQL + @OrderBy
if @debugLevel > 3 begin
  print '@SQL len: '  +  convert(varchar,len(@SQL))
	print @SQL
	end
	
INSERT INTO #tempWorkTable (GroupId, Title)
exec (@sql)
   SELECT @TotalRows = @@ROWCOUNT
-- =================================

print 'added to temp table: ' + convert(varchar,@TotalRows)
if @debugLevel > 7 begin
  select * from #tempWorkTable
  end

-- Show the StartPageIndex
--===================================================
PRINT '@StartPageIndex = ' + convert(varchar,@StartPageIndex)

SET ROWCOUNT @StartPageIndex
--SELECT @first_id = RowNumber FROM #tempWorkTable   ORDER BY RowNumber
SELECT @first_id = @StartPageIndex
PRINT '@first_id = ' + convert(varchar,@first_id)

if @first_id = 1 set @first_id = 0
--set max to return
SET ROWCOUNT @PageSize

SELECT     Distinct
		RowNumber,
    grp.Id, 
    grp.GroupCode, 		grp.GroupTypeId, codes.Title as GroupType, 
    grp.Title,
		case when isnull(TotalMembers,0) > 0 then
			grp.Title + ' (' + convert(varchar, isnull(TotalMembers,0)) + ')'
		else grp.Title
		end as  TitleWithTotals,

    grp.Description, 
    grp.IsActive, 
    ContactId, 
    grp.OrgId, 
		isnull(TotalMembers,0) As TotalMembers,
    grp.Created,  grp.CreatedById, 
    grp.LastUpdated, grp.LastUpdatedById
		,ParentGroupId

		,case 
			when contact.userid is not null Then contact.FirstName + ' ' + contact.LastName  
			else '' End As FullName
		,case 
			when contact.userid is not null Then contact.LastName + ', ' + contact.FirstName
			else '' End As SortName    

From #tempWorkTable temp
    Inner Join [AppGroup] grp on temp.GroupId = grp.Id
    Inner join [Codes.GroupType] codes on grp.GroupTypeId = codes.id
	  Left Join [LR.PatronOrgSummary] contact on grp.ContactId = contact.UserId 
	  Left Join [AppGroup.TeamMember] atm on grp.Id = atm.GroupId

	  left join (SELECT GroupId, count(*) As TotalMembers
		  FROM [AppGroup.Member] group by GroupId) As gmt on grp.id = gmt.GroupId

WHERE RowNumber > @first_id 
order by RowNumber		

SET ROWCOUNT 0




GO
/****** Object:  StoredProcedure [dbo].[AppGroupUpdate]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Update Procedure for [AppGroup] ---
CREATE PROCEDURE [dbo].[AppGroupUpdate]
        @id int,
        @Title varchar(100), 
        @Description varchar(500), 
        @IsActive bit, 
        @ContactId int, 
        @GroupTypeId int, 
        @LastUpdatedById int

As
If @Title = ''   SET @Title = 'Noname group' 
If @Description = ''   SET @Description = NULL 
If @ContactId = 0   SET @ContactId = NULL 
If @GroupTypeId = 0   SET @GroupTypeId = 1 
If @LastUpdatedById = 0   SET @LastUpdatedById = NULL 

UPDATE [AppGroup] 
SET 
    Title = @Title, 
    Description = @Description, 
    IsActive = @IsActive, 
    ContactId = @ContactId, 
    GroupTypeId = @GroupTypeId, 
    LastUpdated = getdate(), 
    LastUpdatedById = @LastUpdatedById
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[ApplicationObjectDelete]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ApplicationObjectDelete]
        @id int
As
DELETE FROM [ApplicationObject]
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[ApplicationObjectGet]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ApplicationObjectGet]
    @id int
As
SELECT     id, 
    ObjectName, 
    DisplayName, 
    Description, 
    Active, 
    ObjectType, 
    RelatedUrl, 
    Created, 
    CreatedById, 
    LastUpdated, 
    LastUpdatedById
FROM [ApplicationObject]
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[ApplicationObjectInsert]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ApplicationObjectInsert]
            @ObjectName nvarchar(100), 
            @DisplayName nvarchar(75), 
            @Description nvarchar(250), 
            @Active bit, 
            @ObjectType varchar(50), 
            @RelatedUrl varchar(150), 
            @CreatedById int
As
If @Description = ''   SET @Description = NULL 

If @ObjectType = ''   SET @ObjectType = NULL 
If @RelatedUrl = ''   SET @RelatedUrl = NULL 
If @CreatedById = 0   SET @CreatedById = NULL 

INSERT INTO [ApplicationObject] (

    ObjectName, 
    DisplayName, 
    Description, 
    Active, 
    ObjectType, 
    RelatedUrl, 
    Created, 
    CreatedById, 
    LastUpdated, 
    LastUpdatedById
)
Values (

    @ObjectName, 
    @DisplayName, 
    @Description, 
    @Active, 
    @ObjectType, 
    @RelatedUrl, 
    getdate(), 
    @CreatedById, 
    getdate(), 
    @CreatedById
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[ApplicationObjectSelect]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ApplicationObjectSelect]
As
SELECT 
    id, 
    ObjectName, 
    DisplayName, 
    Description, 
    Active, 
    ObjectType, 
    RelatedUrl, 
    Created, 
    CreatedById, 
    LastUpdated, 
    LastUpdatedById
FROM [ApplicationObject]
order by DisplayName


GO
/****** Object:  StoredProcedure [dbo].[ApplicationObjectUpdate]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Update Procedure for [ApplicationObject] ---
CREATE PROCEDURE [dbo].[ApplicationObjectUpdate]
        @id int,
        @ObjectName nvarchar(100), 
        @DisplayName nvarchar(75), 
        @Description nvarchar(250), 
        @Active bit, 
        @ObjectType varchar(50), 
        @RelatedUrl varchar(150), 
        @LastUpdatedById int

As

If @Description = ''   SET @Description = NULL 
If @ObjectType = ''   SET @ObjectType = NULL 
If @RelatedUrl = ''   SET @RelatedUrl = NULL 
If @LastUpdatedById = 0   SET @LastUpdatedById = NULL 
UPDATE [ApplicationObject] 
SET 
    ObjectName = @ObjectName, 
    DisplayName = @DisplayName, 
    Description = @Description, 
    Active = @Active, 
    ObjectType = @ObjectType, 
    RelatedUrl = @RelatedUrl, 
    LastUpdated = getdate(), 
    LastUpdatedById = @LastUpdatedById
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[ApplicationRolePrivilegeSelect]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- - Returns all privileges for a roles and object
/*
ApplicationRolePrivilegeSelect 0, 0, 10, 'SiteManagement'
ApplicationRolePrivilegeSelect 1, 0,0,''
ApplicationRolePrivilegeSelect 0, 20,0,''


*/
CREATE PROCEDURE [dbo].[ApplicationRolePrivilegeSelect] (
        @RoleId         int,
        @ObjectId       int,
        @RoleLevel			int,
        @ObjectName			nvarchar(200)

)
As
Declare
  @RowCount   int
  ,@DebugLevel int

set @DebugLevel = 8
set nocount on

if @RoleId = 0        set @RoleId = null
if @ObjectId = 0      set @ObjectId = null
if @RoleLevel = 0     set @RoleLevel = null
if len(@ObjectName) = 0    set @ObjectName = null

Select @RowCount = isnull(Count(*),0) FROM ApplicationPrivileges
Where
 	  (RoleId	= @RoleId or @RoleId is null) 
And (ObjectId = @ObjectId or @ObjectId is null)
--And (RoleLevel = @RoleLevel or @RoleLevel is null)
And (ObjectName = @ObjectName or @ObjectName is null)

if @DebugLevel > 5 print @RowCount
--if @RowCount = 0 Begin
--
--  End
--Else Begin
--End
SELECT [RoleId]
      --,[Name]
      --,RoleLevel
      ,[ObjectId]
      ,[ObjectName]
      ,[DisplayName]
     --,[Sequence]
      ,[CreatePrivilege]
      ,[CreateTitle]
      ,[ReadPrivilege]
      ,[ReadTitle]
      ,[UpdatePrivilege]
      ,[UpdateTitle]
			,WritePrivilege
      ,[DeletePrivilege]
      ,[DeleteTitle]
      ,[AppendPrivilege]
      ,[AppendTitle]
      ,[ApprovePrivilege]
      ,[ApproveTitle]
      ,[AssignPrivilege]
      ,[AssignTitle]
      ,[SharePrivilege]
      ,[ShareTitle]
      ,[AppendToPrivilege]
      ,[AppendToTitle]
      ,[Created]
      --,[SubObjectId]
      --,[SubObjectName]
			,description
  FROM ApplicationPrivileges
Where
 	  (RoleId	= @RoleId or @RoleId is null) 
And (ObjectId = @ObjectId or @ObjectId is null)
--And (RoleLevel = @RoleLevel or @RoleLevel is null)
And (ObjectName = @ObjectName or @ObjectName is null)

Order by 
--[Name], 
[DisplayName]


GO
/****** Object:  StoredProcedure [dbo].[AppObject_Group_OrgPrivileges_Select]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
AppObject_Group_OrgPrivileges_Select null, 'ILPathways.LRW.controls.Authoring'

AppObject_Group_OrgPrivileges_Select 1, null


*/
/* =======================================================
	Determine if userid has group access privileges for the 
	passed object
	 =======================================================*/ 
CREATE PROCEDURE [dbo].[AppObject_Group_OrgPrivileges_Select] (
	@OrgId				int = null
  ,@ObjectName			nvarchar(200)
-- ,@GroupId			  int
)
As
  If @OrgId = 0		SET @OrgId = NULL 
  If @ObjectName = ''		SET @ObjectName = NULL 

Declare
  @RowCount   int
  ,@DebugLevel int

set @DebugLevel = 8
set nocount on

Select @RowCount = isnull(Count(*),0) FROM [AppObject_Group_OrgPrivilegesSummary]
Where
 	  (ObjectName	= @ObjectName or @ObjectName is null) 
And (OrgId = @OrgId or @OrgId is null)

if @DebugLevel > 5 print @RowCount
--
SELECT [ObjectName]
      ,[DisplayName]
      ,[GroupCode]
      ,Title As [GroupName]
      ,[GroupId]
      ,OrgId
      ,[ObjectId]
      ,isnull([CreatePrivilege], 0)		As CreatePrivilege
      ,isnull([ReadPrivilege], 0)			As ReadPrivilege
      ,isnull([WritePrivilege], 0)		As WritePrivilege
      ,isnull([DeletePrivilege], 0)		As DeletePrivilege

      ,isnull([AppendPrivilege], 0)		As AppendPrivilege
      ,isnull([AppendToPrivilege], 0) As AppendToPrivilege
      ,isnull([AssignPrivilege], 0)		As AssignPrivilege
      ,isnull([ApprovePrivilege], 0)	As ApprovePrivilege
      ,isnull([SharePrivilege], 0)		As SharePrivilege
			,Description
  FROM AppObject_Group_OrgPrivilegesSummary
Where
 	  (ObjectName	= @ObjectName or @ObjectName is null) 
And (OrgId = @OrgId or @OrgId is null)


GO
/****** Object:  StoredProcedure [dbo].[AppObject_Group_UserPrivileges_Select]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
AppObject_Group_UserPrivileges_Select null, 'ILPathways.Controls.ResourcePublisher'

AppObject_Group_UserPrivileges_Select 1, null


*/
/* =======================================================
	Determine if userid has group access privileges for the 
	passed object
	 =======================================================*/ 
CREATE PROCEDURE [dbo].[AppObject_Group_UserPrivileges_Select] (
	@UserId				int = null
  ,@ObjectName			nvarchar(200)
-- ,@GroupId			  int
)
As
  If @UserId = 0		SET @UserId = NULL 
  If @ObjectName = ''		SET @ObjectName = NULL 

Declare
  @RowCount   int
  ,@DebugLevel int

set @DebugLevel = 8
set nocount on

Select @RowCount = isnull(Count(*),0) FROM [AppObject_Group_UserPrivileges]
Where
 	  (ObjectName	= @ObjectName or @ObjectName is null) 
And (UserId = @UserId or @UserId is null)

if @DebugLevel > 5 print @RowCount
--
SELECT [ObjectName]
      ,[DisplayName]
      ,[GroupCode]
      ,Title As [GroupName]
      ,[GroupId]
      ,UserId
      ,[ObjectId]
      ,isnull([CreatePrivilege], 0)		As CreatePrivilege
      ,isnull([ReadPrivilege], 0)			As ReadPrivilege
      ,isnull([WritePrivilege], 0)		As WritePrivilege
      ,isnull([DeletePrivilege], 0)		As DeletePrivilege

      ,isnull([AppendPrivilege], 0)		As AppendPrivilege
      ,isnull([AppendToPrivilege], 0) As AppendToPrivilege
      ,isnull([AssignPrivilege], 0)		As AssignPrivilege
      ,isnull([ApprovePrivilege], 0)	As ApprovePrivilege
      ,isnull([SharePrivilege], 0)		As SharePrivilege
			,Description
  FROM [AppObject_Group_UserPrivileges]
Where
 	  (ObjectName	= @ObjectName or @ObjectName is null) 
And (UserId = @UserId or @UserId is null)


GO
/****** Object:  StoredProcedure [dbo].[AppUser.ExternalAccountGet]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AppUser.ExternalAccountGet]
    @Id int,
    @UserId int, 
    @ExternalSiteId int,
    @Token  varchar(50) 
As
if @Id = 0 set @Id = null
if @UserId = 0 set @UserId = null
if @ExternalSiteId = 0 set @ExternalSiteId = null
if len(@Token) = 0 set @Token = null

if @id is null And @UserId is null And @ExternalSiteId is null begin
  print 'no id provided'
  RAISERROR(' A valid id or rowId must be supplied', 18, 1)    
  RETURN -1 
  end
  
SELECT     
    UserId, 
    Id, 
    ExternalSiteId, 
    LoginId, 
    Token
FROM [AppUser.ExternalAccount]
WHERE 
    (Id = @Id OR @Id is null)
And (UserId = @UserId OR @UserId is null)    
And (ExternalSiteId = @ExternalSiteId OR @ExternalSiteId is null)
And (Token = @Token OR @Token is null)


GO
/****** Object:  StoredProcedure [dbo].[AppUser.ExternalAccountInsert]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[AppUser.ExternalAccountInsert]
            @UserId int,  
            @ExternalSiteId int, 
            @LoginId varchar(100), 
            @Token varchar(50)
As
If @UserId = 0   SET @UserId = NULL 
If @ExternalSiteId = 0   SET @ExternalSiteId = NULL 
If @LoginId = ''   SET @LoginId = NULL 
--If @Password = ''   SET @Password = NULL 
If @Token = ''   SET @Token = NULL 

INSERT INTO [AppUser.ExternalAccount] (

    UserId, 
    ExternalSiteId, 
    LoginId, 
    Token
)
Values (

    @UserId, 
    @ExternalSiteId, 
    @LoginId, 
    @Token
)
 
select SCOPE_IDENTITY() as Id


GO
/****** Object:  StoredProcedure [dbo].[AppUser.GetByExtAccount]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
AppUserGet 'IllinoisPathways','77BAE02B-9E76-4877-A90C-BFFFE9B3464E'
*/
CREATE PROCEDURE [dbo].[AppUser.GetByExtAccount]
    @ExternalSiteId int,
    @LoginId  varchar(100), 
    @Token    varchar(50) 

As
if @ExternalSiteId = 0 set @ExternalSiteId = null
if len(@LoginId) = 0 set @LoginId = null
if len(@Token) = 0 set @Token = null

if @ExternalSiteId is null And @LoginId is null And @Token is null begin
  print 'no id provided'
  RAISERROR(' A valid id or rowId must be supplied', 18, 1)    
  RETURN -1 
  end

SELECT 
    base.Id, 
    base.UserName,  
    base.FirstName, 
    base.LastName, 
    base.Email, 
    base.SecretQuestionId, 
    base.SecretAnswer, 
    base.Created, 
    --base.CreatedById, 
    base.LastUpdated,     base.LastUpdatedById
    ,ExternalSiteId, LoginId, Token
FROM [AppUser] base
inner join [AppUser.ExternalAccount] ext on base.Id = ext.UserId

WHERE	
    (LoginId = @LoginId OR @LoginId is null)
And (Token = @Token OR @Token is null)    
And (ExternalSiteId = @ExternalSiteId OR @ExternalSiteId is null)


GO
/****** Object:  StoredProcedure [dbo].[AppUser.ProfileGet]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AppUser.ProfileGet]
    @UserId int
As
SELECT     UserId, 
    MainPhone, 
    JobTitle, 
    PublishingRoleId, 
    RoleProfile, 
    OrganizationId, 
    '' AS Organization,
    Created, 
    CreatedById, 
    LastUpdated, 
    LastUpdatedId
FROM [AppUser.Profile]
WHERE UserId = @UserId


GO
/****** Object:  StoredProcedure [dbo].[AppUser.ProfileInsert]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AppUser.ProfileInsert]
            @UserId int, 
            @JobTitle varchar(50), 
            @PublishingRoleId int, 
            @RoleProfile varchar(500), 
            @OrganizationId int
As

If @JobTitle = ''   SET @JobTitle = NULL 
If @PublishingRoleId = 0   SET @PublishingRoleId = NULL 
If @RoleProfile = ''   SET @RoleProfile = NULL 
If @OrganizationId = 0   SET @OrganizationId = NULL 

INSERT INTO [AppUser.Profile] (
    UserId,
    JobTitle, 
    PublishingRoleId, 
    RoleProfile, 
    OrganizationId, 
    CreatedById, 
    LastUpdatedId
)
Values (
    @UserId,
    @JobTitle, 
    @PublishingRoleId, 
    @RoleProfile, 
    @OrganizationId, 
    @UserId, 
    @UserId
)
 
select @@RowCount as Id


GO
/****** Object:  StoredProcedure [dbo].[AppUser.ProfileUpdate]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--- Update Procedure for [AppUser.Profile] ---
CREATE PROCEDURE [dbo].[AppUser.ProfileUpdate]
        @UserId int,
        @JobTitle varchar(50), 
        @PublishingRoleId int, 
        @RoleProfile varchar(500), 
        @OrganizationId int, 
        @LastUpdatedId int
As

If @JobTitle = ''   SET @JobTitle = NULL 
If @PublishingRoleId = 0   SET @PublishingRoleId = NULL 
If @RoleProfile = ''   SET @RoleProfile = NULL 
If @OrganizationId = 0   SET @OrganizationId = NULL 
If @LastUpdatedId = 0   SET @LastUpdatedId = NULL 

UPDATE [AppUser.Profile] 
SET 
    JobTitle = @JobTitle, 
    PublishingRoleId = @PublishingRoleId, 
    RoleProfile = @RoleProfile, 
    OrganizationId = @OrganizationId, 
    LastUpdated = getdate(), 
    LastUpdatedId = @LastUpdatedId
WHERE UserId = @UserId

GO
/****** Object:  StoredProcedure [dbo].[AppUser.RecoverPassword]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[AppUser.RecoverPassword] 'IllinoisPathways'


[AppUser.RecoverPassword] 'mparsons'

*/
CREATE PROCEDURE [dbo].[AppUser.RecoverPassword]
		@Lookup varchar(100)


As
if len(@Lookup) = 0    set @Lookup = null

if @Lookup is null begin
  print 'no @Lookup provided'
  RAISERROR(' A valid lookup must be supplied', 18, 1)    
  RETURN -1 
  end

SELECT 
    Id, 
    UserName,  
    FirstName, 
    LastName, 
    Email, 
    SecretQuestionId, 
    SecretAnswer, 
    Created, 
    LastUpdated, 
    LastUpdatedById,
    RowId
FROM [AppUser]

WHERE	
    (UserName = @Lookup OR Email = @Lookup)


GO
/****** Object:  StoredProcedure [dbo].[AppUserAuthorize]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
AppUserAuthorize 'IllinoisPathways','77BAE02B-9E76-4877-A90C-BFFFE9B3464E'
*/
CREATE PROCEDURE [dbo].[AppUserAuthorize]
		@UserName varchar(50),
		@Password varchar(50)

As

--if NOT exists( SELECT * FROM [AppUser] WHERE UserName = @UserName AND Password = @Password ) begin
--                print '—internal test message'
--                RAISERROR(' Invalid Username or Password ', 18, 1)    
--                RETURN -1 
--                End


SELECT 
    Id, 
    UserName,  
    FirstName, 
    Password,
    LastName, 
    Email, 
    SecretQuestionId, 
    SecretAnswer, 
    Created, 
    LastUpdated, 
    LastUpdatedById,
    RowId
FROM [AppUser]

WHERE	UserName = @UserName AND
		Password = @Password



GO
/****** Object:  StoredProcedure [dbo].[AppUserGet]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
AppUserGet 0, 'IllinoisPathways','', ''

AppUserGet 0, '','info@illinoisworknet.com', ''

AppUserGet 'IllinoisPathways','77BAE02B-9E76-4877-A90C-BFFFE9B3464E'
AppUserGet 1, '', '', ''

*/
CREATE PROCEDURE [dbo].[AppUserGet]
		@Id int,
		@UserName varchar(50),
		@Email varchar(100),
		@RowId varchar(40)


As
if @Id = 0            set @Id = null
if len(@RowId) = 0    set @RowId = null
if len(@Email) = 0    set @Email = null
if len(@UserName) = 0 set @UserName = null

if @id is null And @RowId is null And @Email is null And @UserName is null begin
  print 'no id provided'
  RAISERROR(' A valid id or rowId must be supplied', 18, 1)    
  RETURN -1 
  end

SELECT 
    Id, 
    UserName,  
    FirstName, 
    LastName, 
    Email, 
    SecretQuestionId, 
    SecretAnswer, 
    Created, 
    LastUpdated, 
    LastUpdatedById,
    RowId
FROM [AppUser]

WHERE	
    (Id = @Id OR @Id is null)
And (UserName = @UserName OR @UserName is null)
And (Email = @Email OR @Email is null)
And (RowId = @RowId OR @RowId is null)


GO
/****** Object:  StoredProcedure [dbo].[AppUserInsert]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AppUserInsert]
            @UserName varchar(50), 
            @Password varchar(50), 
            @FirstName varchar(50), 
            @LastName varchar(50), 
            @Email varchar(100), 
            @SecretQuestionId int, 
            @SecretAnswer varchar(50)
As
If @UserName = ''   SET @UserName = NULL 
If @Password = ''   SET @Password = NULL 
If @FirstName = ''   SET @FirstName = NULL 
If @LastName = ''   SET @LastName = NULL 
If @Email = ''   SET @Email = NULL 
If @SecretQuestionId = 0   SET @SecretQuestionId = NULL 
If @SecretAnswer = ''   SET @SecretAnswer = NULL 

INSERT INTO [AppUser] (

    UserName, 
    Password, 
    FirstName, 
    LastName, 
    Email, 
    SecretQuestionId, 
    SecretAnswer
)
Values (

    @UserName, 
    @Password, 
    @FirstName, 
    @LastName, 
    @Email, 
    @SecretQuestionId, 
    @SecretAnswer 

)
 
select SCOPE_IDENTITY() as Id


GO
/****** Object:  StoredProcedure [dbo].[AppUserSelect]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AppUserSelect]
As
SELECT 
    Id, 
    UserName,  
    FirstName, 
    LastName, 
    Email, 
    SecretQuestionId, 
    SecretAnswer, 
    Created, 
    LastUpdated, 
    LastUpdatedById
FROM [AppUser]
ORDER BY LastName, FirstName


GO
/****** Object:  StoredProcedure [dbo].[AppUserUpdate]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--- Update Procedure for [AppUser] ---
CREATE PROCEDURE [dbo].[AppUserUpdate]
		@Id int,
        @UserName varchar(50), 
        @Password varchar(50), 
        @FirstName varchar(50), 
        @LastName varchar(50), 
        @Email varchar(100), 
        @SecretQuestionId int, 
        @SecretAnswer varchar(50)

As
If @UserName = ''   SET @UserName = NULL 
If @Password = ''   SET @Password = NULL 
If @FirstName = ''   SET @FirstName = NULL 
If @LastName = ''   SET @LastName = NULL 
If @Email = ''   SET @Email = NULL 
If @SecretQuestionId = 0   SET @SecretQuestionId = NULL 
If @SecretAnswer = ''   SET @SecretAnswer = NULL 

UPDATE [AppUser] 
SET 
    UserName = @UserName, 
    Password = @Password, 
    FirstName = @FirstName, 
    LastName = @LastName, 
    Email = @Email, 
    SecretQuestionId = @SecretQuestionId, 
    SecretAnswer = @SecretAnswer
WHERE Id = @Id


GO
/****** Object:  StoredProcedure [dbo].[aspCreateProcedure_Delete]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
Exec aspCreateProcedure_Delete 'Contact', 1

Exec aspCreateProcedure_Delete 'Contact', 1, 0, 'pre', '_suf', 'DeleteName', 'GrantToMtce'

Exec aspCreateProcedure_Delete 'WiaContract.Action', 1, 0, '', '_Delete','', 'GrantToMtce'
*/
-- =========================================================================
-- 12/09/13 mparsons - updated to handle tables with dot notation (ex.Policy.Version)
-- =========================================================================
create  Procedure [dbo].[aspCreateProcedure_Delete]
		@tableName 		varchar(128) 
		,@print 		bit 
		,@tableFirst	bit = 1
		,@Prefix		varchar(20) = '' 
		,@Suffix		varchar(20) = '' 
		,@ProcName		varchar(128) = 'Delete' 
		,@GrantGroup	varchar(128) = '' 
AS

Declare 
	@SQLStatement 		varchar(8000), --Actual Delete Procedure string
	@parameters 		varchar(8000), -- Parameters to be passed to the Stored Procedure
	@deleteStatement 	varchar(8000), -- To Store the Delete SQL Statement
	@procedurename 		varchar(128), -- To store the procedure name
	@DropProcedure 		varchar(1000), --Drop procedure sql statement
	@GrantProcedure 	varchar(1000) 	--To Store Grant execute Procedure SQL Statement

-- Initialize Variables
SET @parameters = ''
SET @deleteStatement = ''

--Get Parameters and Delete Where Clause needed for the Delete Procedure.
SELECT	@parameters = @parameters + 
			Case When @parameters = '' Then ''
					Else ', ' + Char(13) + Char(10) 
					End + '        @' + + INFORMATION_SCHEMA.Columns.COLUMN_NAME + ' ' + 
					DATA_TYPE + 
					Case When CHARACTER_MAXIMUM_LENGTH is not null Then 
						'(' + Cast(CHARACTER_MAXIMUM_LENGTH as varchar(4)) + ')' 
						Else '' 
					End,
	@deleteStatement = @deleteStatement + Case When @deleteStatement = '' Then ''
					Else ' AND ' + Char(13) + Char(10) 
					End + INFORMATION_SCHEMA.Columns.COLUMN_NAME + ' = @' + + INFORMATION_SCHEMA.Columns.COLUMN_NAME
FROM	
	INFORMATION_SCHEMA.Columns,
	INFORMATION_SCHEMA.TABLE_CONSTRAINTS,
	INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE
WHERE 	INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME = INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.CONSTRAINT_NAME AND
	INFORMATION_SCHEMA.Columns.Column_name = INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.Column_name AND
	INFORMATION_SCHEMA.Columns.table_name = INFORMATION_SCHEMA.TABLE_CONSTRAINTS.TABLE_NAME AND
	INFORMATION_SCHEMA.TABLE_CONSTRAINTS.table_name = @tableName AND 
	CONSTRAINT_TYPE = 'PRIMARY KEY'

-- the following logic can be changed as per your standards. 
SET @procedurename = @ProcName	--'Delete'

If @tableFirst = 1 Begin
	-- Use syntax of prefix + TableName + ProcedureType + Suffix
	--	ex.	ContactSelect, uspContactSelect, ContactSelect_usp
	If Left(@tableName, 3) = 'tbl' Begin
		SET @procedurename = @Prefix +  + SubString(@tableName, 4, Len(@tableName))  + @procedurename  + @Suffix
	End
	Else Begin
		-- In case none of the above standards are followed then just get the table name.
		SET @procedurename = @Prefix + @tableName + @procedurename  + @Suffix
	End
End
Else Begin
	If Left(@tableName, 3) = 'tbl' Begin
		SET @procedurename = @Prefix + @procedurename + SubString(@tableName, 4, Len(@tableName))  + @Suffix
	End
	Else Begin
		-- In case none of the above standards are followed then just get the table name.
		SET @procedurename = @Prefix + @procedurename + @tableName + @Suffix
	End
End


--Stores DROP Procedure Statement
SET @DropProcedure = 'if exists (select * from dbo.sysobjects where id = object_id(N''[dbo].[' + @procedurename + ']'') and OBJECTPROPERTY(id, N''IsProcedure'') = 1)' + Char(13) + Char(10) +
				'Drop Procedure [' + @procedurename + ']'

--Stores grant procedure statement
if len(@GrantGroup) > 0 
	SET @GrantProcedure = 'grant execute on [' + @procedurename + ']  to ' + @GrantGroup
Else
	SET @GrantProcedure = ''

-- In case you want to create the procedure pass in 0 for @print else pass in 1 and stored procedure will be displayed in results pane.
If @print = 0
Begin
	-- Create the final procedure and store it..
	Exec (@DropProcedure)
	SET @SQLStatement = 'CREATE PROCEDURE [' + @procedurename + '] ' +  Char(13) + Char(10) + @parameters + Char(13) + Char(10) + ' AS ' + 
				 + Char(13) + Char(10) + ' Delete FROM [' + @tableName + '] ' + ' WHERE ' + @deleteStatement + Char(13) + Char(10) 

	-- Execute the SQL Statement to create the procedure
	--Print @SQLStatement
	Exec (@SQLStatement)

	-- Do the grant
	if len(@GrantProcedure) > 0 Begin
		Exec (@GrantProcedure)
	End	
End
Else
Begin
	--Print the Procedure to Results pane
	Print ''
	Print ''
	Print ''
	Print '--- Delete Procedure for [' + @tableName + '] ---'
	Print @DropProcedure
	Print 'GO'
	Print 'CREATE PROCEDURE [' + @procedurename + ']'
	Print @parameters
	Print 'As'
	Print 'DELETE FROM [' + @tableName + ']'
	Print 'WHERE ' + @deleteStatement
	Print 'GO'
	Print @GrantProcedure
	Print 'Go'
End


GO
/****** Object:  StoredProcedure [dbo].[aspCreateProcedure_Get]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
Exec aspCreateProcedure_Get 'Contact', 1

Exec aspCreateProcedure_Get 'Contact', 1, 0, 'pre', 'suf', 'GetName'

Exec aspCreateProcedure_Get 'WiaContract.Action', 1, 0, '', '_Select', ''
*/
-- =========================================================================
-- 12/09/13 mparsons - updated to handle tables with dot notation (ex.Policy.Version)
-- =========================================================================
create  Procedure [dbo].[aspCreateProcedure_Get] 
		@tableName 		varchar(128) 
		,@print 		bit 
		,@tableFirst	bit = 1
		,@Prefix		varchar(20) = '' 
		,@Suffix		varchar(20) = '' 
		,@ProcName		varchar(128) = 'Select' 

AS

Declare 
	@SQLStatement 		varchar(8000), 	--Actual Get Procedure string
	@SelectStatement 	varchar(8000), 	--Actual Select Statement that returns result set.
	@procedurename 		varchar(128), 	-- To store the procedure name
	@DropProcedure 		varchar(1000), --To Store Drop Procedure SQL Statement
	@GrantProcedure 	varchar(1000) 	--To Store Grant execute Procedure SQL Statement

SET @SelectStatement = ''

-- Get Columns from the table to be displayed.
SELECT @SelectStatement = @SelectStatement + 
				Case When @SelectStatement = '' Then ''  + Char(13) + Char(10) 
					Else ', ' + Char(13) + Char(10) 
				End + '    ' + COLUMN_NAME
FROM INFORMATION_SCHEMA.Columns
WHERE	table_name = @tableName

-- the following logic can be changed as per your standards. In our case tbl is for tables and tlkp is for lookup tables. Needed to remove tbl and tlkp...
SET @procedurename = @ProcName	--'Select'

If @tableFirst = 1 Begin
	-- Use syntax of prefix + TableName + ProcedureType + Suffix
	--	ex.	ContactSelect, uspContactSelect, ContactSelect_usp
	If Left(@tableName, 3) = 'tbl' Begin
		SET @procedurename = @Prefix +  + SubString(@tableName, 4, Len(@tableName))  + @procedurename  + @Suffix
	End
	Else Begin
		-- In case none of the above standards are followed then just get the table name.
		SET @procedurename = @Prefix + @tableName + @procedurename  + @Suffix
	End
End
Else Begin
	If Left(@tableName, 3) = 'tbl' Begin
		SET @procedurename = @Prefix + @procedurename + SubString(@tableName, 4, Len(@tableName))  + @Suffix
	End
	Else Begin
		-- In case none of the above standards are followed then just get the table name.
		SET @procedurename = @Prefix + @procedurename + @tableName + @Suffix
	End
End

--Stores DROP Procedure Statement
SET @DropProcedure = 'if exists (select * from dbo.sysobjects where id = object_id(N''[' + @procedurename + ']'') and OBJECTPROPERTY(id, N''IsProcedure'') = 1)' + Char(13) + Char(10) +
				'Drop Procedure [' + @procedurename + ']'

--Stores grant procedure statement
SET @GrantProcedure = 'grant execute on [' + @procedurename + '] to public ' 

-- In case you want to create the procedure pass in 0 for @print else pass in 1 and stored procedure will be displayed in results pane.
If @print = 0
Begin
	-- Drop the current procedure.
	Exec (@DropProcedure)
	-- Create the final procedure and store it..
	SET @SQLStatement = 	
				'CREATE PROCEDURE [' + @procedurename + '] ' +  Char(13) + Char(10) + ' AS ' + Char(13) + Char(10) +
				'SELECT ' + @SelectStatement + Char(13) + Char(10) + 
				'FROM [' + @tableName + '] ' + Char(13) + Char(10) 

	-- Execute the SQL Statement to create the procedure	
	Exec (@SQLStatement)
	-- Do the egrant
	Exec (@GrantProcedure)
End
Else
Begin
	--Print the Procedure to Results pane
	Print ''
	Print ''
	Print ''
	Print '--- Get Procedure for [' + @tableName + '] ---'
	Print @DropProcedure
	Print 'Go'
	Print 'CREATE PROCEDURE [' + @procedurename + ']'
	Print 'As'
	Print 'SELECT ' + @SelectStatement 
	Print 'FROM [' + @tableName + ']'
	Print 'GO'
	Print @GrantProcedure
	Print 'Go'
End


GO
/****** Object:  StoredProcedure [dbo].[aspCreateProcedure_GetSingle]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
Exec aspCreateProcedure_GetSingle 'WiaContract.Action', 1

Exec aspCreateProcedure_GetSingle 'WiaContract.Action', 1, 0, 'pre', '_suf', 'GetSingle', 'GrantToPublic'

Exec aspCreateProcedure_GetSingle 'WiaContract.Action', 1, 0, '', '_Select', '', 'GrantToPublic'
*/
-- =========================================================================
-- 12/09/13 mparsons - updated to handle tables with dot notation (ex.Policy.Version)
-- =========================================================================
create   Procedure [dbo].[aspCreateProcedure_GetSingle] 
		@tableName 		varchar(128) 
		,@print 		bit 
		,@tableFirst	bit = 1
		,@Prefix		varchar(20) = '' 
		,@Suffix		varchar(20) = '' 
		,@ProcName		varchar(128) = 'Get' 
		,@GrantGroup	varchar(128) = '' 
AS

Declare @SQLStatement varchar(8000), 	-- Actual GetSingle Procedure string
	@parameters varchar(8000), 			-- To store parameter list.
	@SelectStatement varchar(8000), 	-- Actual Select Statement that returns result set.
	@WhereStatement varchar(8000), 		-- Where clause to pick the single record
	@procedurename varchar(128), 		-- To store the procedure name
	@DropProcedure 		varchar(1000), -- Drop procedure sql statement
	@GrantProcedure 	varchar(1000) 	-- To Store Grant execute Procedure SQL Statement

SET @parameters = ''
SET @SelectStatement = ''
SET @WhereStatement = ''

--Build parameter list and where clause
SELECT	@parameters = @parameters + Case When @parameters = '' Then ''
					Else ', ' + Char(13) + Char(10) 
					End + '    @' + INFORMATION_SCHEMA.Columns.COLUMN_NAME + ' ' + 
					DATA_TYPE + 
					Case When CHARACTER_MAXIMUM_LENGTH is not null Then 
						'(' + Cast(CHARACTER_MAXIMUM_LENGTH as varchar(4)) + ')' 
						Else '' 
					End,
	@WhereStatement = @WhereStatement + Case When @WhereStatement = '' Then ''
					Else ' AND ' + Char(13) + Char(10) 
					End + INFORMATION_SCHEMA.Columns.COLUMN_NAME + ' = @' + + INFORMATION_SCHEMA.Columns.COLUMN_NAME
FROM	
	INFORMATION_SCHEMA.Columns,
	INFORMATION_SCHEMA.TABLE_CONSTRAINTS,
	INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE
WHERE 	
	INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME = INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.CONSTRAINT_NAME AND
	INFORMATION_SCHEMA.Columns.Column_name = INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.Column_name AND
	INFORMATION_SCHEMA.Columns.table_name = INFORMATION_SCHEMA.TABLE_CONSTRAINTS.TABLE_NAME AND
	INFORMATION_SCHEMA.TABLE_CONSTRAINTS.table_name = @tableName AND 
	CONSTRAINT_TYPE = 'PRIMARY KEY'

--Store column names from the select statement
SELECT	@SelectStatement = @SelectStatement + Case When @SelectStatement = '' Then ''
					Else ', ' + Char(13) + Char(10) 
					End + '    ' + COLUMN_NAME
FROM	INFORMATION_SCHEMA.Columns
WHERE	table_name = @tableName

-- the following logic can be changed as per your standards. 
SET @procedurename = @ProcName	--'GetSingle'

If @tableFirst = 1 Begin
	-- Use syntax of prefix + TableName + ProcedureType + Suffix
	--	ex.	ContactSelect, uspContactSelect, ContactSelect_usp
	If Left(@tableName, 3) = 'tbl' Begin
		SET @procedurename = @Prefix +  + SubString(@tableName, 4, Len(@tableName))  + @procedurename  + @Suffix
	End
	Else Begin
		-- In case none of the above standards are followed then just get the table name.
		SET @procedurename = @Prefix + @tableName + @procedurename  + @Suffix
	End
End
Else Begin
	If Left(@tableName, 3) = 'tbl' Begin
		SET @procedurename = @Prefix + @procedurename + SubString(@tableName, 4, Len(@tableName))  + @Suffix
	End
	Else Begin
		-- In case none of the above standards are followed then just get the table name.
		SET @procedurename = @Prefix + @procedurename + @tableName + @Suffix
	End
End

--Stores DROP Procedure Statement
SET @DropProcedure = 'if exists (select * from dbo.sysobjects where id = object_id(N''[' + @procedurename + ']'') and OBJECTPROPERTY(id, N''IsProcedure'') = 1)' + Char(13) + Char(10) +
					'Drop Procedure [' + @procedurename + ']'

--Stores grant procedure statement
if len(@GrantGroup) > 0 
	SET @GrantProcedure = 'grant execute on [' + @procedurename + '] to ' + @GrantGroup
Else
	SET @GrantProcedure = ''

-- In case you want to create the procedure pass in 0 for @print else pass in 1 and stored procedure will be displayed in results pane.
If @print = 0
Begin
	-- Drop the current procedure.
	Exec (@DropProcedure)
	-- Create the final procedure and store it..	
	SET @SQLStatement = 'CREATE PROCEDURE [' + @procedurename + '] ' +  Char(13) + Char(10) + @parameters + Char(13) + Char(10) + ' AS ' + 
				'SELECT ' + @SelectStatement + Char(13) + Char(10) + 
				'FROM [' + @tableName + '] ' + Char(13) + Char(10) +
				'WHERE ' + @WhereStatement
	-- Execute the SQL Statement to create the procedure
	Exec(@SQLStatement)

	-- Do the grant
	if len(@GrantProcedure) > 0 Begin
		Exec (@GrantProcedure)
	End	
End
Else
Begin
	--Print the Procedure to Results pane
	Print ''
	Print ''
	Print ''
	Print '--- Get Single Procedure for [' + @tableName + '] ---'
	Print @DropProcedure
	Print 'Go'
	Print 'CREATE PROCEDURE [' + @procedurename + ']'
	Print @parameters
	Print 'As'
	Print 'SELECT ' + @SelectStatement 
	Print 'FROM [' + @tableName + ']'
	Print 'WHERE ' + @WhereStatement
	Print 'GO'
	Print @GrantProcedure
	Print 'Go'
End



GO
/****** Object:  StoredProcedure [dbo].[aspCreateProcedure_Insert]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/****** Object:  Stored Procedure dbo.aspCreateProcedure_Insert    Script Date: 7/8/2005 11:16:33 AM ******/
/*
Exec aspCreateProcedure_Insert 'Building', 0
Exec aspCreateProcedure_Insert 'Contact', 1

Exec aspCreateProcedure_Insert 'Contact', 1, 0, 'pre', 'suf', 'CreateName', 'GrantToMtce'

Exec aspCreateProcedure_Insert 'WiaContract.Action', 1, 0, '', '_Insert', '', 'GrantToMtce'

*/
-- =========================================================================
-- 08/05/28 mparsons - added code to handle varchar(MAX) - length is equal to -1 in CHARACTER_MAXIMUM_LENGTH
-- 12/09/13 mparsons - updated to handle tables with dot notation (ex.Policy.Version)
-- =========================================================================
create   Procedure [dbo].[aspCreateProcedure_Insert]
		@tableName 		varchar(128) 
		,@print 		bit 
		,@tableFirst	bit = 1
		,@Prefix		varchar(20) = '' 
		,@Suffix		varchar(20) = '' 
		,@ProcName		varchar(128) = 'Insert' 
		,@GrantGroup		varchar(128) = '' 
AS

-- =============================================================
-- = TODO
-- =	- consider trying to handle nullable fields on input parms 
-- =	  (i.e. default to null, so don't have to be provided)
-- =
-- =============================================================
Declare 
	@SQLStatement 		varchar(8000), --Actual Delete Procedure string
	@parameters 		varchar(8000), -- Parameters to be passed to the Stored Procedure
	@InsertStatement 	varchar(8000), --To store Insert Clause.
	@ValuesStatement 	varchar(8000), -- To store Values Clause
	@NullStatement 		varchar(8000), --To store special handling of null values.
	@procedurename 		varchar(128), -- To store the procedure name
	@DropProcedure 		varchar(1000), --Drop procedure sql statement
	@GrantProcedure 	varchar(1000) 	--To Store Grant execute Procedure SQL Statement
	--TODO add process to do a grant - ex to a passed role
	,@KeyCount			int

-- Initialize Variables
SET @parameters = ''
SET @InsertStatement = ''
SET @ValuesStatement = ''
SET @NullStatement = ''

-- Do check for multiple primary key columns
SELECT @KeyCount = count(*) 
			FROM 	INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
				JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE 
					ON INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME = INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.CONSTRAINT_NAME
			WHERE	
-- 				INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.COLUMN_NAME = INFORMATION_SCHEMA.Columns.COLUMN_NAME 
-- 			AND	
				INFORMATION_SCHEMA.TABLE_CONSTRAINTS.table_name = @tableName 
			AND CONSTRAINT_TYPE = 'PRIMARY KEY'

if @KeyCount > 1 begin
	print '******************************************************************'
	print '  Warning requested table has multiple primary keys. '
	print '  You may need to manually add these to the create procedure if  '
	print '  they are not of the autogenerate type!				'
	print '******************************************************************'
end

-- Get Parameters, insert, values and Null statements.

SELECT	@parameters = @parameters + 
		Case When @parameters = '' Then '    ' 
			Else ', ' + Char(13) + Char(10) + '    ' 
			End + '        @' + + INFORMATION_SCHEMA.Columns.COLUMN_NAME + ' ' + 
			DATA_TYPE + 
			Case 
				When CHARACTER_MAXIMUM_LENGTH = -1 Then 
					'(MAX)' 
				When CHARACTER_MAXIMUM_LENGTH is not null Then 
					'(' + Cast(CHARACTER_MAXIMUM_LENGTH as varchar(4)) + ')' 
				Else '' 
			End,
	@InsertStatement = @InsertStatement + 
		Case When @InsertStatement = '' Then ''
			Else ', ' 
			End + Char(13) + Char(10) + '    ' +
			COLUMN_NAME,
	@ValuesStatement = @ValuesStatement + Case When @ValuesStatement = '' Then ''
			Else ', ' 
			End + Char(13) + Char(10) + '    @' +
			COLUMN_NAME,
	@NullStatement = @NullStatement + Case When @NullStatement = '' Then '' 
			Else Char(13) + Char(10)
			End + 
			CASE WHEN DATA_TYPE = 'int' OR DATA_TYPE = 'smallint' OR 
				DATA_TYPE = 'tinyint' OR DATA_TYPE = 'real' OR 
				DATA_TYPE = 'float' OR DATA_TYPE = 'decimal' OR 
				DATA_TYPE = 'bit' OR DATA_TYPE = 'numeric' OR DATA_TYPE = 'bigint' Then 
				'If @' + COLUMN_NAME + ' = 0 ' 
			Else 
				'If @' + COLUMN_NAME + ' = '''' ' 
			End + '  SET @' + COLUMN_NAME + ' = NULL '
--  select *
FROM	
	INFORMATION_SCHEMA.Columns
WHERE	
	table_name = @tableName AND 
	NOT EXISTS(SELECT * 
			FROM 	INFORMATION_SCHEMA.TABLE_CONSTRAINTS JOIN
				INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ON INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME = INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.CONSTRAINT_NAME
			WHERE	INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.COLUMN_NAME = INFORMATION_SCHEMA.Columns.COLUMN_NAME AND
				INFORMATION_SCHEMA.TABLE_CONSTRAINTS.table_name = @tableName AND 
				CONSTRAINT_TYPE = 'PRIMARY KEY')
				
-- the following logic can be changed as per your standards. In our case tbl is for tables and tlkp is for lookup tables. Needed to remove tbl and tlkp...
SET @procedurename = @ProcName	--'Create'

If @tableFirst = 1 Begin
	-- Use syntax of prefix + TableName + ProcedureType + Suffix
	--	ex.	ContactSelect, uspContactSelect, ContactSelect_usp
	If Left(@tableName, 3) = 'tbl' Begin
		SET @procedurename = @Prefix +  + SubString(@tableName, 4, Len(@tableName))  + @procedurename  + @Suffix
	End
	Else Begin
		-- In case none of the above standards are followed then just get the table name.
		SET @procedurename = @Prefix + @tableName + @procedurename  + @Suffix
	End
End
Else Begin
	If Left(@tableName, 3) = 'tbl' Begin
		SET @procedurename = @Prefix + @procedurename + SubString(@tableName, 4, Len(@tableName))  + @Suffix
	End
	Else Begin
		-- In case none of the above standards are followed then just get the table name.
		SET @procedurename = @Prefix + @procedurename + @tableName + @Suffix
	End
End

--Stores DROP Procedure Statement
SET @DropProcedure = 'if exists (select * from dbo.sysobjects where id = object_id(N''[' + @procedurename + ']'') and OBJECTPROPERTY(id, N''IsProcedure'') = 1)' + Char(13) + Char(10) +
				'Drop Procedure [' + @procedurename + ']'

--Stores grant procedure statement
if len(@GrantGroup) > 0 
	SET @GrantProcedure = 'grant execute on [' + @procedurename + '] to ' + @GrantGroup
Else
	SET @GrantProcedure = ''

-- In case you want to create the procedure pass in 0 for @print else pass in 1 and stored procedure will be displayed in results pane.
If @print = 0
Begin
	--print 'Doing drop'
	-- Drop the current procedure.
	Exec (@DropProcedure)
	--print 'Doing create'
	-- Create the final procedure 
	SET @SQLStatement = 'CREATE PROCEDURE [' + @procedurename + '] ' +  Char(13) + Char(10) + @parameters + Char(13) + Char(10) + 'AS' + Char(13) + Char(10) +
				@NullStatement + Char(13) + Char(10) + 'INSERT INTO [' + @tableName + '] (' + @InsertStatement + ')' + Char(13) + Char(10) + 
				'Values (' + @ValuesStatement + ')' + Char(13) + Char(10) + 'select SCOPE_IDENTITY()'
	--print str(len(@SQLStatement)) + @SQLStatement 
	-- Execute the SQL Statement to create the procedure
	Exec(@SQLStatement)

	-- Do the grant
	if len(@GrantProcedure) > 0 Begin
		Exec (@GrantProcedure)
	End	

End
Else
Begin
	--Print the Procedure to Results pane
	Print ''
	Print ''
	Print ''
	Print '--- Insert Procedure for [' + @tableName + '] ---'
	Print @DropProcedure
	Print 'Go'
	Print 'CREATE PROCEDURE [' + @procedurename + ']'
	Print @parameters
	Print 'As'
	Print @NullStatement
	Print 'INSERT INTO [' + @tableName + '] ('
	Print @InsertStatement
	Print ')'
	Print 'Values ('
	Print @ValuesStatement
	Print ')'
	Print ''
	Print 'select SCOPE_IDENTITY() as Id'
	Print 'GO'
	Print @GrantProcedure
	Print 'Go'
End



GO
/****** Object:  StoredProcedure [dbo].[aspCreateProcedure_Update]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
Exec aspCreateProcedure_Update 'Contact', 1

Exec aspCreateProcedure_Update 'Contact', 1, 0, '', '_usp', 'Update', 'GrantToMtce'

Exec aspCreateProcedure_Update 'WiaContract.Action', 1, 0, '', '_Insert', '', 'GrantToMtce'
*/
-- =========================================================================
-- 08/05/28 mparsons - added code to handle varchar(MAX) - length is equal to -1 in CHARACTER_MAXIMUM_LENGTH
-- 12/09/13 mparsons - updated to handle tables with dot notation (ex.Policy.Version)
-- =========================================================================
create  Procedure [dbo].[aspCreateProcedure_Update] 
		@tableName 		varchar(128) 
		,@print 		bit 
		,@tableFirst	bit = 1
		,@Prefix		varchar(20) = '' 
		,@Suffix		varchar(20) = '' 
		,@ProcName		varchar(128) = 'Update' 
		,@GrantGroup	varchar(128) = '' 
AS
-- =============================================================
-- = TODO
-- =	- consider trying to handle nullable fields on input parms 
-- =	  (i.e. default to null, so don't have to be provided)
-- =
-- =============================================================
Declare 
	@SQLStatement 		varchar(8000), --Actual Delete Procedure string
	@parameters 		varchar(8000), -- Parameters to be passed to the Stored Procedure
	@updateStatement 	varchar(8000), -- To Store the update SQL Statement
	@NullStatement 		varchar(8000), -- To Store Null handling for null allowed columns.
	@procedurename 		varchar(128), -- To store the procedure name
	@WhereClause 		varchar(8000), -- To Store Where clause information for the update statement.
	@DropProcedure 		varchar(1000), --Drop procedure sql statement
	@GrantProcedure 	varchar(1000) 	--To Store Grant execute Procedure SQL Statement
	--TODO add process to do a grant - ex to a passed role

-- Initialize Variables
SET @parameters = ''
SET @updateStatement = ''
SET @NullStatement = ''
SET @WhereClause = ''
SET @DropProcedure = ''

-- Build Parameters, Update and Null Statements

SELECT	@parameters = @parameters + Case When @parameters = '' Then ''
					Else ', ' + Char(13) + Char(10) 
					End + '        @' + + INFORMATION_SCHEMA.Columns.COLUMN_NAME + ' ' + 
					DATA_TYPE + 
					Case 
						When CHARACTER_MAXIMUM_LENGTH = -1 Then 
							'(MAX)' 
						When CHARACTER_MAXIMUM_LENGTH is not null Then 
						'(' + Cast(CHARACTER_MAXIMUM_LENGTH as varchar(4)) + ')' 
						Else '' 
					End,
	@updateStatement = @updateStatement + Case When @updateStatement = '' Then ''
					Else ', ' 
					End + Char(13) + Char(10) + '    ' + COLUMN_NAME + ' = @' +
					COLUMN_NAME,
	@NullStatement = @NullStatement + Case When @NullStatement = '' Then '' 
					Else Char(13) + Char(10)
					End + 
					CASE WHEN DATA_TYPE = 'int' OR DATA_TYPE = 'smallint' OR 
						DATA_TYPE = 'tinyint' OR DATA_TYPE = 'real' OR 
						DATA_TYPE = 'float' OR DATA_TYPE = 'decimal' OR 
						DATA_TYPE = 'bit' OR DATA_TYPE = 'numeric' OR DATA_TYPE = 'bigint' Then 
						'If @' + COLUMN_NAME + ' = 0 ' 
					Else 
						'If @' + COLUMN_NAME + ' = '''' ' 
					End + '  SET @' + COLUMN_NAME + ' = NULL '
FROM	
	INFORMATION_SCHEMA.Columns
WHERE	
	table_name = @tableName AND 
	NOT EXISTS(SELECT * 
			FROM 	INFORMATION_SCHEMA.TABLE_CONSTRAINTS JOIN
				INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ON INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME = INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.CONSTRAINT_NAME
			WHERE	INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.COLUMN_NAME = INFORMATION_SCHEMA.Columns.COLUMN_NAME AND
				INFORMATION_SCHEMA.TABLE_CONSTRAINTS.table_name = @tableName AND 
				CONSTRAINT_TYPE = 'PRIMARY KEY')

-- Build Parameters and Where Clause with Primary key

SELECT	@parameters = @parameters + Case When @parameters = '' Then ''
					Else ', ' + Char(13) + Char(10) 
					End + '@' + + INFORMATION_SCHEMA.Columns.COLUMN_NAME + ' ' + 
					DATA_TYPE + 
					Case When CHARACTER_MAXIMUM_LENGTH is not null Then 
						'(' + Cast(CHARACTER_MAXIMUM_LENGTH as varchar(4)) + ')' 
						Else '' 
					End,
	@WhereClause = @WhereClause + Case When @WhereClause = '' Then ''
					Else ' AND ' + Char(13) + Char(10) 
					End + INFORMATION_SCHEMA.Columns.COLUMN_NAME + ' = @' + + INFORMATION_SCHEMA.Columns.COLUMN_NAME
FROM	
	INFORMATION_SCHEMA.Columns,
	INFORMATION_SCHEMA.TABLE_CONSTRAINTS,
	INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE
WHERE 	
	INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME = INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.CONSTRAINT_NAME AND
	INFORMATION_SCHEMA.Columns.Column_name = INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.Column_name AND
	INFORMATION_SCHEMA.Columns.table_name = INFORMATION_SCHEMA.TABLE_CONSTRAINTS.TABLE_NAME AND
	INFORMATION_SCHEMA.TABLE_CONSTRAINTS.table_name = @tableName AND 
	CONSTRAINT_TYPE = 'PRIMARY KEY'

-- the following logic can be changed as per your standards. 
SET @procedurename = @ProcName	--'Update'

If @tableFirst = 1 Begin
	-- Use syntax of prefix + TableName + ProcedureType + Suffix
	--	ex.	ContactSelect, uspContactSelect, ContactSelect_usp
	If Left(@tableName, 3) = 'tbl' Begin
		SET @procedurename = @Prefix +  + SubString(@tableName, 4, Len(@tableName))  + @procedurename  + @Suffix
	End
	Else Begin
		-- In case none of the above standards are followed then just get the table name.
		SET @procedurename = @Prefix + @tableName + @procedurename  + @Suffix
	End
End
Else Begin
	If Left(@tableName, 3) = 'tbl' Begin
		SET @procedurename = @Prefix + @procedurename + SubString(@tableName, 4, Len(@tableName))  + @Suffix
	End
	Else Begin
		-- In case none of the above standards are followed then just get the table name.
		SET @procedurename = @Prefix + @procedurename + @tableName + @Suffix
	End
End


--Stores DROP Procedure Statement
SET @DropProcedure = 'if exists (select * from dbo.sysobjects where id = object_id(N''[' + @procedurename + ']'') and OBJECTPROPERTY(id, N''IsProcedure'') = 1)' + Char(13) + Char(10) +
				'Drop Procedure [' + @procedurename + ']'

--Stores grant procedure statement
if len(@GrantGroup) > 0 
	SET @GrantProcedure = 'grant execute on [' + @procedurename + '] to ' + @GrantGroup
Else
	SET @GrantProcedure = ''

If @print = 0
Begin
	-- Drop the current procedure.
	Exec (@DropProcedure)
	-- Create the final procedure 
	SET @SQLStatement = 'CREATE PROCEDURE [' + @procedurename + '] ' +  Char(13) + Char(10) + @parameters + Char(13) + Char(10) + 'AS ' + Char(13) + Char(10) +
				@NullStatement + Char(13) + Char(10) + 'Update ' + @tableName + '] ' + Char(13) + Char(10) + 'SET ' + @UpdateStatement + Char(13) + Char(10) +
				'WHERE ' + @WhereClause
	-- Execute the SQL Statement to create the procedure
	--Print @SQLStatement
	Exec (@SQLStatement)

	-- Do the grant
	if len(@GrantProcedure) > 0 Begin
		Exec (@GrantProcedure)
	End	

End
Else
Begin
	--Print the Procedure to Results pane
	Print ''
	Print ''
	Print ''
	Print @DropProcedure
	Print 'Go'
	Print '--- Update Procedure for [' + @tableName + '] ---'
	Print 'CREATE PROCEDURE [' + @procedurename + ']'
	Print @parameters
	Print 'As'
	Print @NullStatement
	Print 'UPDATE [' + @tableName + '] '
	Print 'SET ' + @UpdateStatement
	Print 'WHERE ' + @WhereClause
	Print 'GO'
	Print @GrantProcedure
	Print 'Go'
End


GO
/****** Object:  StoredProcedure [dbo].[aspCreateProcedures]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

exec aspCreateProcedures 1,1,1,1,1,'|WorkNetGroupPrivilege|',1,'public'

exec aspCreateProcedures 1,1,1,1,1,'|GroupTeamMember|',1,'public'

exec aspCreateProcedures 1,1,1,1,1,'|Contact|',1,'public'
exec aspCreateProcedures 1,1,1,1,1,'|Organization|',1,'public'
exec aspCreateProcedures 1,1,1,1,1,'|Apartment|',0,'public'

exec aspCreateProcedures 1,1,1,1,1,'|SiteLandlord|',0,'public'

Exec aspCreateProcedure_Insert 'Building', 0
Exec aspCreateProcedure_Update 'Building', 0

Exec aspCreateProcedure_Get 'Contact', 1
Exec aspCreateProcedure_GetSingle 'Contact', 1

Exec aspCreateProcedure_Insert 'Contact', 0
Exec aspCreateProcedure_Update 'Contact', 0
Exec aspCreateProcedure_Delete 'Contact', 1
*/

Create   PROCEDURE [dbo].[aspCreateProcedures] 
	@insertProc 	bit,
	@updateProc 	bit,
	@selectProc 		bit,
	@getSingleProc 	bit,
	@deleteProc 	bit,
	@tables 		varchar(2000),
	@printtoFile 	bit
	,@DefaultRole varchar(50) = 'public'
AS
/* 
	Usage:
		aspCreateProcedures
				@insertProc 	  1 to create an insert proc, 0 to skip,
				@updateProc 	  1 to create an update proc, 0 to skip,
				@selectProc 		1 to create a select proc, 0 to skip,
				@getSingleProc 	1 to create a getSingle proc, 0 to skip,
				@deleteProc 	  1 to create a delete proc, 0 to skip,
				@tables 		    list of tables, blank for all
				@printtoFile 	  1 to preview the SQL, 0 to actually create
				@DefaultRole	  default role (grants execute to this role

	User Can pass in multiple table names separated by |. The following cursor will get 
	the right table names and then run procedures for each tables. 

	EX: 
		exec aspCreateProcedures 0,0,1,0,0,'|Contact|Account|', 0	
	Would create: get only for Account and Contact

	If creating for full database just pass in '' for @tables.

	TODO:
	- consider supplying parms to dictate naming format (ex for usp prefix, suffix, etc.)
*/
Declare
	@Prefix				varchar(10)
	,@Suffix			varchar(10)
	,@SelectProcName		varchar(50) 
	,@GetSingleProcName	varchar(50) 
	,@CreateProcName	varchar(50) 
	,@UpdateProcName	varchar(50) 
	,@DeleteProcName	varchar(50) 
	,@GetSingleGrantRole	varchar(50) 
	,@CreateGrantRole	varchar(50) 
	,@UpdateGrantRole	varchar(50) 
	,@DeleteGrantRole	varchar(50) 
	,@tableFirst		bit

-- Set defaults for the current database /application 
set @tableFirst 	= 1		-- 1 to place table name before the procedure type (ContactInsert), 0 for after (InsertContact)
set @Prefix 		= ''	-- use to add a prefix to the procedure name )
set @Suffix 		= ''	-- use to add a suffix to the procedure name
-- Default procedure names
set @SelectProcName 	= 'Select' 
set @CreateProcName = 'Insert'
set @UpdateProcName = 'Update'
set @DeleteProcName = 'Delete'
set @GetSingleProcName 	= 'Get' 

-- Default grant role (can have multiplies, comma separated) 
set @CreateGrantRole = @DefaultRole	--'Maintenance'
set @UpdateGrantRole = @DefaultRole	--'Maintenance'
set @DeleteGrantRole = @DefaultRole	--'Maintenance'
set @GetSingleGrantRole = 'Public' 

Declare curTables CURSOR
FOR
	SELECT TABLE_NAME
	FROM INFORMATION_SCHEMA.TABLES 
	WHERE 
		TABLE_TYPE = 'BASE TABLE' 
	AND
		(CharIndex('|' + TABLE_NAME + '|', @tables) > 0 OR
		LTrim(RTrim(@tables)) = '')

Open curTables
Declare @Table_name varchar(128)
FETCH NEXT FROM curTables INTO @table_name

WHILE @@FETCH_STATUS = 0
Begin
	-- Check if insert procedure needs to be created. 
	If @insertProc = 1
		Exec aspCreateProcedure_Insert @table_name, @printtoFile, @tableFirst, @Prefix, @Suffix, @CreateProcName, @CreateGrantRole

	-- Check if update procedure needs to be created. 
	If @updateProc = 1
		Exec aspCreateProcedure_Update @table_name, @printtoFile, @tableFirst, @Prefix, @Suffix, @UpdateProcName, @UpdateGrantRole

	-- Check if get procedure needs to be created. 
	If @SelectProc = 1
		Exec aspCreateProcedure_Get @table_name, @printtoFile, @tableFirst, @Prefix, @Suffix, @SelectProcName
			 
	-- Check if get single procedure needs to be created. 
	If @getSingleProc = 1
		Exec aspCreateProcedure_GetSingle @table_name, @printtoFile, @tableFirst, @Prefix, @Suffix, @GetSingleProcName, @GetSingleGrantRole

	-- Check if delete procedure needs to be created. 
	If @deleteProc = 1
		Exec aspCreateProcedure_Delete @table_name, @printtoFile, @tableFirst, @Prefix, @Suffix, @DeleteProcName, @DeleteGrantRole

	FETCH NEXT FROM curTables INTO @table_name
End
Close curTables
Deallocate curTables


GO
/****** Object:  StoredProcedure [dbo].[aspGenerateColumnDef]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--*** need to update to handle varchar(max)!!!

-- =========================================================================
-- generate columns in a format for use by the dictionary program
-- Loops through all tables
--	- add a filter to retrieve a subset
-- raw select without any joins - for updates
-- Outputs to a local table called _Dictionary
-- 05/03/02 mparsons - added column desc
-- 06/10/27 mparsons - removed column description as the latter doesn't exist under ss2005
-- 08/05/21 mparsons - added code to handle varchar(MAX) - length is equal to -1 in syscolumns
-- 10/11/10 mparsons - added code to handle table names containing a period
-- =========================================================================
/*	
	drop table _Dictionary
 aspGenerateColumnDef @TableFilter = NULL, @TypeFilter='table', @Debug=8

 aspGenerateColumnDef @TableFilter = 'bus%', @TypeFilter='table'
*/
Create     PROCEDURE [dbo].[aspGenerateColumnDef]
	@TableFilter	varchar(50) = null,
	@TypeFilter		varchar(25) = null,
	@Debug 			int = 0
   	with recompile
As

Declare 
	@tablename 	nvarchar(128), 
	@entityType nvarchar(25), 
	@EntityFilter nvarchar(25), 
	@flags 		int , 
	@orderby 	nvarchar(10),
	@flags2 	int ,
	@AllColumns int,
    @msg        char(255),
	@fetch_cnt	int,
	@objid 		int

-- Set following defaults
set @flags 		= 0 
set @orderby 	= null
set @flags2 	= 0
set @AllColumns	= 1

IF EXISTS (SELECT name, type from sysobjects where type = 'U' and name = N'#genColDef') begin 
	drop table #genColDef
	end

-- Create work table
create table #genColDef
(
	tableName        	nvarchar(128)   COLLATE database_default NOT NULL,
	col_id           	int             NOT NULL,
	colName     		nvarchar(128)   COLLATE database_default NOT NULL,
	datatype     		nvarchar(30)   	COLLATE database_default NOT NULL,
	col_precScale		nvarchar(50)	NULL,
	col_null         	bit           	NOT NULL,  /* status & 8 */
	DefaultValue      	nvarchar(257)  	COLLATE database_default NULL
   ,col_identity    	bit               /* status & 128 */

   ,col_desc			nvarchar(250)	NULL
   ,entityType			nvarchar(25)    NULL	
   ,CreatedDate			datetime        NOT NULL CONSTRAINT DF_genColDef_CreatedDate DEFAULT (getdate())

)

if (@TypeFilter is null) 
	set @entityFilter = null
else begin
	set @entityFilter = '%' + @TypeFilter + '%' 
end

-- ********************************************************************
-- * - declare cursor for loop thru table
-- ********************************************************************
declare curLocal CURSOR FOR
	-- Get tables
	SELECT 
	--	*,
	-- 	Table_Schema, 
	 	Table_Name 
		,Case When Table_Type = 'BASE TABLE' Then 'table'
			  When Table_Type = 'VIEW' Then 'view'
		 else Table_Type
		 end As EntityType
	FROM 
		Information_Schema.Tables 
	WHERE 
-- 		lower(Table_Type) = 'base table' 
--  	AND 
		Table_Name <> 'dtproperties'
-- 	AND Table_Name Like 'HD07%'
	And (Table_Name Like @TableFilter or @TableFilter is null)
	And (Table_Type Like @entityFilter or @entityFilter is null)
	Order by Table_Name

open  curLocal
fetch next from curLocal into @tablename, @entityType
WHILE @@FETCH_STATUS = 0 Begin
	--set @tablename = '[' + @tablename + ']'
	select @objid = object_id('[' + @tablename + ']')
	--select object_id('[program.unit]')

	if @Debug > 5 begin
		print 'next: ' + @tablename
		print '@objid: ' + convert(varchar,@objid)
		end

	Insert #genColDef 
	select
		@tablename, 
		c.colid, 
		c.name, 
		st.name As DataType,
--          	case 
-- 				when bt.name in (N'nchar', N'nvarchar') 
-- 				then c.length/2 
-- 				else c.length end As Length,

		PrecScale = 
			case when (st.name in (N'decimal',N'numeric') )
				 	then cast(c.xprec as varchar(3)) + ', ' + cast(c.xscale  as varchar(3))
				 when c.length = -1
				 	then 'MAX'
				 when st.name in (N'nchar', N'nvarchar') 
				 	then cast(c.length/2  as varchar(4))
				 else cast(c.length  as varchar(4)) end,

		-- Nullable
		convert(bit, ColumnProperty(@objid, c.name, N'AllowsNull')) As AllowNulls
		,IsNull(cn.text,'') As DefaultValue
		-- Identity
			,case 
				when (@flags & 0x40000000 = 0) 
				then convert(bit, ColumnProperty(@objid, c.name, N'IsIdentity')) 
				else 0 
			end As IdentityType
-- 			,ColumnProperty(@objid, c.name, N'Precision') As TypePrecision
-- 			,ColumnProperty(@objid, c.name, N'Scale')	 As Scale

		--,convert(nvarchar(250),sp.value)
		,'TBD'
		,@entityType
		,getdate()
-- 
-- 	select * 
	from 
		dbo.syscolumns c
--where c.name = 'description'
		-- NonDRI Default and Rule filters
		left outer join dbo.sysobjects d on d.id = c.cdefault
		left outer join dbo.sysobjects r on r.id = c.domain
		-- Fully derived data type name
		join dbo.systypes st on st.xusertype = c.xusertype
		-- Physical base data type name
		join dbo.systypes bt on bt.xusertype = c.xtype
		-- DRIDefault text, if it's only one row.
		left outer join dbo.syscomments t on t.id = c.cdefault and t.colid = 1
				and not exists (select * from dbo.syscomments where id = c.cdefault and colid = 2)
		left outer join dbo.syscomments 	cn on t.id is not null and cn.id = t.id
		-- Any description for the column
		-- mp - sysproperties doesn't exist under ss2005
--		left outer join dbo.sysproperties 	sp 
--			on  c.id = sp.id and c.colid = sp.smallid and sp.name = 'MS_Description'
	where 
		c.id = @objid
	order by 
		c.colid

	/* Get next row                                    */
	fetch next from curLocal into @tablename, @entityType

   	set @fetch_cnt = @@rowcount

end /* while */

close curLocal
deallocate curLocal
-- Now display columns

IF EXISTS (SELECT name, type from sysobjects where type = 'U' and name = N'_Dictionary') begin 
--	select 'exists'
	delete from _Dictionary
	Insert _Dictionary select * from #genColDef 
	end
else begin
--	select 'not found'
	select * Into _Dictionary from #genColDef order by tableName, col_id

	end
IF EXISTS (SELECT name, type from sysobjects where type = 'U' and name = N'_DictTable') begin 
	-- only add new tables
	Insert _DictTable 
		select distinct tablename, tablename + '- TBD' As Description, 1 As IsActive, 1 As ReportGroup, 1 As ReportOrder, 1 As Synchronize, getdate() As Created
		from _Dictionary 
		Where (tablename not like '_d%') and tablename not in (select tablename from _DictTable)
	end
else begin
--	select 'not found'
	select distinct tablename, tablename + '- TBD' As Description, 1 As IsActive, 1 As ReportGroup, 1 As ReportOrder, 1 As Synchronize, getdate() As Created  into _DictTable from _Dictionary 

	end

select * From _Dictionary order by tableName, col_id
-- drop temp table
drop table #genColDef



GO
/****** Object:  StoredProcedure [dbo].[CodeTable_Select]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
GetCodeValues "Gender", "IntegerValue" ,"en"

[CodeTable_Select] "GridPageSize", "IntegerValue" ,"en"

[P_VOS_GET_CODES] "Ethnicity", "IntegerValue" ,"es"
*/

-- =============================================
-- CodeTable_Select
-- Description:	copied [P_VOS_GET_CODES] to replicate processing
-- =============================================
CREATE PROCEDURE [dbo].[CodeTable_Select] 
			@Code varchar(50),
			@Sort varchar(20),
			@Language	varchar(10)
AS
BEGIN
	SET NOCOUNT ON;
	
SELECT *	
FROM [codeTable]
Where CodeName = @Code 
AND LanguageCode = @Language
And IsActive = 1
Order by 
	CASE @Sort 
		WHEN 'CodeName' THEN CodeName 
		WHEN 'StringValue' THEN StringValue 
		WHEN 'Description' Then Description
	END, 
	CASE @Sort 
		WHEN 'NumericValue' THEN NumericValue
	END,
	CASE @Sort 
		WHEN 'Id' THEN Id
		WHEN 'IntegerValue' THEN IntegerValue 
		WHEN 'SortOrder' THEN SortOrder
	END,
	CASE @Sort
		WHEN 'Created' THEN Created
		WHEN 'Modified' THEN Modified
	END

END


GO
/****** Object:  StoredProcedure [dbo].[GroupMember_SelectCustomers_Paging]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
sp_who

select * 
from GroupMember
Update GroupMember
Set IsActive = 0
Where groupId = 1 and ContactId <> 14167


-- ===========================================================

DECLARE @RC int, @Filter varchar(500)
DECLARE @StartPageIndex int, @PageSize int, @totalRows int

set @Filter = ' where groupId = 62 '
set @Filter = ' where GroupCode = ''OrgApprovers'' '

set @StartPageIndex = 1
set @PageSize = 15

EXECUTE @RC = [GroupMember_SelectCustomers_Paging] 
   @Filter  ,@StartPageIndex  ,@PageSize  ,@totalRows OUTPUT

select 'total rows = ' + convert(varchar,@totalRows)


*/
/* =================================================
= Get group members - Customer types. 
= Uses custom pagin to only return enough rows for one page
=	Options:
= - all records for a group (by id or code)
= - all records for a contact (that is all groups of which the contact is a member)
= @StartPageIndex - starting page number. If interface is at 20 when next page is requested, this would be set to 21?
= @PageSize - number of records on a page
= @totalRows OUTPUT - total available rows. Used by interface to build a custom pager
= ------------------------------------------------------
= Modifications
= 13-05-27 mparsons - copies from VOS
-- ================================================= */
CREATE PROCEDURE [dbo].[GroupMember_SelectCustomers_Paging]

    @Filter				varchar(500)
		,@StartPageIndex int
		,@PageSize		int
		,@TotalRows			int OUTPUT
AS 

	SET NOCOUNT ON;
-- paging
DECLARE 
	@first_id			int
	,@startRow		int
	,@debugLevel	int
	,@SQL					varchar(1000)
			,@SortOrder       varchar(100)
	,@OrderBy			varchar(100)

-- =================================
Set @debugLevel = 4
if len(isnull(@SortOrder,'')) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by grp.Title, mbr.SortName '

CREATE TABLE #tempSearchTable(
	RowNumber int PRIMARY KEY IDENTITY(1,1) NOT NULL,
	GroupId int NOT NULL,
	GroupMemberId int NOT NULL,
	GroupName varchar(100),
	SortName varchar(100)
)

-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))

	set @SQL = 'SELECT Distinct gm.GroupId, gm.ID, grp.Title	,mbr.SortName
	FROM [AppGroup.Member] gm
			Inner Join [AppGroup] grp on gm.GroupId = grp.Id
			Inner Join [LR.PatronOrgSummary] mbr on gm.UserId = mbr.UserId ' 
		+ @Filter

--, mbr.SortName
--			Inner  Join UserSummary mbr on gm.ContactId = mbr.Id
	if charindex( 'order by', lower(@Filter) ) = 0 
		set @SQL = 	@SQL + @OrderBy
		
  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL
  
	INSERT INTO #tempSearchTable (GroupId, GroupMemberId, GroupName, SortName)
	exec (@sql)


print 'added to temp table: ' + convert(varchar,@@rowcount)
if @debugLevel > 7 begin
	select * from #tempSearchTable
	end


-- Get the total rows 
SELECT @totalRows = COUNT(RowNumber) FROM #tempSearchTable
print 'rows = ' + str(@totalRows)

if @StartPageIndex < 1		set @StartPageIndex = 1

-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex = 0		SET @StartPageIndex = 1
PRINT '@StartPageIndex = ' + convert(varchar,@StartPageIndex)

SET ROWCOUNT @StartPageIndex
SELECT @first_id = RowNumber FROM #tempSearchTable   ORDER BY RowNumber
PRINT '@first_id = ' + convert(varchar,@first_id)
if @first_id = 1 set @first_id = 0

--set max to return
SET ROWCOUNT @PageSize

SELECT     Distinct
RowNumber,
    grp.Title As GroupName, 
    grp.GroupCode,
    gm.GroupId, 
    gm.UserId, gm.OrgId,
    gm.ID,     gm.ID As GroupMemberId, 
    gm.Status, 
    gm.Category, 
    gm.Comment
-- Customer
      ,mbr.Lastname
      ,mbr.Fullname
      ,mbr.SortName
			,mbr.Email

From #tempSearchTable work
Inner Join [AppGroup.Member] gm on work.GroupMemberId = gm.Id
			Inner Join [AppGroup] grp on gm.GroupId = grp.Id
			Inner Join [LR.PatronOrgSummary] mbr on gm.UserId = mbr.UserId
			
WHERE RowNumber > @first_id 
order by RowNumber		

SET ROWCOUNT 0
--
---- Get the total rows 
--SELECT @totalRows = COUNT(RowNumber) FROM #tempSearchTable
--
--print 'rows = ' + str(@totalRows)


GO
/****** Object:  StoredProcedure [dbo].[Library.GetMyLibrary]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

[Library.GetMyLibrary] 2

*/
--get personal library
CREATE PROCEDURE [dbo].[Library.GetMyLibrary]
    @CreatedById int
As
SELECT     
    base.Id, 
    base.Title, 
    base.Description, 
    LibraryTypeId, 
    lt.Title as LibraryType,
    IsDiscoverable, 
        0 as OrgId,
    'Personal' As Organization,
    IsPublic, IsActive,
		base.PublicAccessLevel,
	base.OrgAccessLevel,
    base.ImageUrl, 
    base.Created, base.CreatedById, 
    base.LastUpdated, base.LastUpdatedById
    
FROM [Library] base
inner join [Library.Type] lt on base.LibraryTypeId = lt.Id
WHERE base.CreatedById = @CreatedById
And lt.Id = 1

GO
/****** Object:  StoredProcedure [dbo].[Organization.MemberSearch]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
sp_who
SELECT [OrgMbrId]
      ,[OrgId]
      ,[Organization]
      ,[UserId]
      ,[FirstName]
      ,[LastName]
      ,[Email]
      ,[OrgMemberTypeId]
      ,[OrgMemberType]
      ,[MemberAdded]
      ,[BaseOrgId]
      ,[BaseOrganization]
  FROM [dbo].[Organization_MemberSummary]
GO


-- ===========================================================

DECLARE @RC int, @Filter varchar(500),@SortOrder       varchar(200)
DECLARE @StartPageIndex int, @PageSize int, @totalRows int
set @SortOrder = ''
set @Filter = ' where OrgId = 1 '

set @Filter = ' where OrgId = 1 and OrgMemberTypeId > 1'

--set @Filter = ' '
set @StartPageIndex = 1
set @PageSize = 15

EXECUTE @RC = [Organization.MemberSearch] 
   @Filter  ,@SortOrder, @StartPageIndex  ,@PageSize  ,@totalRows OUTPUT

select 'total rows = ' + convert(varchar,@totalRows)


*/
/* =================================================
= Search org members - 
=	Options:
= - all records for a group (by id or code)
= - all records for a contact (that is all groups of which the contact is a member)
= @StartPageIndex - starting page number. If interface is at 20 when next page is requested, this would be set to 21?
= @PageSize - number of records on a page
= @totalRows OUTPUT - total available rows. Used by interface to build a custom pager
= ------------------------------------------------------
= Modifications
= 14-02-06 mparsons - created
-- ================================================= */
CREATE PROCEDURE [dbo].[Organization.MemberSearch]

    @Filter				varchar(500)
    ,@SortOrder       varchar(200)
	,@StartPageIndex int
	,@PageSize		int
	,@TotalRows			int OUTPUT
AS 

	SET NOCOUNT ON;
-- paging
DECLARE 
	@first_id			int
	,@startRow		int
	,@debugLevel	int
	,@SQL					varchar(1000)
	,@OrderBy			varchar(100)

-- =================================
Set @debugLevel = 4
if len(isnull(@SortOrder,'')) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by Organization, LastName '

CREATE TABLE #tempSearchTable(
	RowNumber int PRIMARY KEY IDENTITY(1,1) NOT NULL,
	OrgId int NOT NULL,
	MbrId int NOT NULL
)

-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

	print '@Filter len: '  +  convert(varchar,len(@Filter))

	set @SQL = 'SELECT OrgId, OrgMbrId FROM [Organization_MemberSummary]  ' 
		+ @Filter

	if charindex( 'order by', lower(@Filter) ) = 0 
		set @SQL = 	@SQL + @OrderBy
		
  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL
  
	INSERT INTO #tempSearchTable (OrgId, MbrId)
	exec (@sql)
	
 --print 'rows: ' + convert(varchar, @@ROWCOUNT)
  SELECT @TotalRows = @@ROWCOUNT
-- =================================

print 'added to temp table: ' + convert(varchar,@@rowcount)
if @debugLevel > 7 begin
	select * from #tempSearchTable
	end

if @StartPageIndex < 1		set @StartPageIndex = 1

-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex = 0		SET @StartPageIndex = 1
PRINT '@StartPageIndex = ' + convert(varchar,@StartPageIndex)

SET ROWCOUNT @StartPageIndex
SELECT @first_id = RowNumber FROM #tempSearchTable   ORDER BY RowNumber
PRINT '@first_id = ' + convert(varchar,@first_id)
if @first_id = 1 set @first_id = 0

--set max to return
SET ROWCOUNT @PageSize

SELECT     Distinct
RowNumber	
,gm.[OrgMbrId] As Id
    ,gm.[OrgMbrId]
      ,gm.[OrgId]
      ,[Organization]
      ,[UserId]
      ,[FirstName]
      ,[LastName]
      ,[Email]
      ,[OrgMemberTypeId]
      ,[OrgMemberType]
      ,[MemberAdded]
	  ,[MemberAdded] As Created
      ,[BaseOrgId]
      ,[BaseOrganization]

From #tempSearchTable work
Inner Join Organization_MemberSummary gm on work.MbrId = gm.OrgMbrId
			
WHERE RowNumber > @first_id 
order by RowNumber		

SET ROWCOUNT 0

GO
/****** Object:  StoredProcedure [dbo].[Organization_QuickAssign]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

USE [Gateway]
GO
select * from organization
siuccwd-6
USE [Gateway]
GO

SELECT [Id]
      ,[OrgId]
      ,[UserId]
      ,[OrgMemberTypeId]
      ,[Created]
      ,[CreatedById]
   
  FROM [dbo].[Organization.Member]
GO
USE [Gateway]
GO

SELECT [Id]
      ,[OrgContactId]
      ,[RoleId]
      ,[Created]

  FROM [dbo].[Organization.MemberRole]
GO


-- ======================================================

DECLARE @RC int,@userId int,@OrgId int,@UserEmail varchar(100),@CreatedById int
,@OrgMbrTypeId int ,@IsAdminMbr bit

-- Set parameter values 
set @OrgId= 42
set @userId= 0
set @UserEmail  = 'dowen@usd116.org'
set @CreatedById= 2
set @OrgMbrTypeId  = 1
set @IsAdminMbr = 1

--SELECT [Id] As UserId,[FullName],[Email],[JobTitle],[RoleProfile],[OrganizationId],Organization,[PublishingRole],[Created]      ,[LastUpdated] 	  FROM [Isle_IOER].[dbo].[Patron_Summary] where UserName = @userEmail

EXECUTE @RC = [dbo].[Organization_QuickAssign] 
   @OrgId
  ,@userId
  ,@UserEmail
  ,OrgMbrTypeId
  ,@CreatedById
  ,@IsAdminMbr

  EXECUTE @RC = [dbo].[Organization_QuickAssign]    @OrgId  ,@userId  ,'',2  ,2,0

EXECUTE @RC = [dbo].[Organization_QuickAssign]    42  ,229  ,'',1  ,2,1

[dbo].[Organization_QuickAssign]    4 ,2  ,'',1  ,2,1

*/
CREATE PROCEDURE [dbo].[Organization_QuickAssign]
            @OrgId int, 
			@userId int,
            @UserEmail varchar(100),
			@OrgMbrTypeId int ,
            @CreatedById int,
			@IsAdminMbr bit
As


If ltrim(rtrim(@UserEmail)) = ''   SET @UserEmail = NULL 
If @CreatedById = 0   SET @CreatedById = NULL 
If @userId = 0   SET @userId = NULL 
If @OrgMbrTypeId = 0   SET @OrgMbrTypeId = 2 

if @OrgId < 1 or (@UserEmail is null AND @userId IS NULL) begin
	RAISERROR(' Error - Org Id and user email or userid are required', 18, 1)   
	return -1
	end
declare @CurrOrgId int
--============================================================
-- get user and update profile
--============================================================

if @userId is NULL begin
	print 'select user = ' +  @userEmail
	SELECT @userId = isnull([Id],0)   FROM [Isle_IOER].[dbo].[Patron] where Email = @userEmail
	print 'user id = ' +  convert(varchar, @userId)
	end


if @userId is not null AND @userId > 0 begin
	print 'check for proflie'
	if NOT exists( SELECT [UserId] FROM Isle_IOER.[dbo].[Patron.Profile] where [UserId]= @userId)  begin
		--do insert
		print 'Adding user profile for org '
		INSERT INTO Isle_IOER.[dbo].[Patron.Profile] ([UserId] ,[OrganizationId],[CreatedById] ,[LastUpdatedId])
		 VALUES (@userId ,@orgId ,@userId,@userId)
      end    
    else begin
		-- check for existing
		SELECT @CurrOrgId = isnull(OrganizationId,0) FROM Isle_IOER.[dbo].[Patron.Profile] where [UserId]= @userId
		if @CurrOrgId = 0 begin
			
			--do update
			print 'no existing org, update user profile for org '
			UPDATE [Isle_IOER].[dbo].[Patron.Profile]
			   SET [OrganizationId] = @orgId 
			 WHERE UserId = @userId
			end

		 else begin 
			print 'Existing org found: ' + convert(varchar,@CurrOrgId)
			end
		end
	--confirm
	--SELECT [Id] As UserId,[FullName],[Email],[JobTitle],[RoleProfile],[OrganizationId],Organization,[PublishingRole],[Created]      ,[LastUpdated]
	--  FROM [Isle_IOER].[dbo].[Patron_Summary]
	--  WHERE Id = @userId
	print 'Creating orgMbr '
	-- create org mbr and role of admin ==> assumes for first user for org
	declare @OrgMbrId int
	INSERT INTO [dbo].[Organization.Member]
           ([OrgId],[UserId],[OrgMemberTypeId],[CreatedById],[LastUpdatedById])
     VALUES
           (@orgId, @userId, @OrgMbrTypeId, @userId, @userId)
	select  @OrgMbrId = SCOPE_IDENTITY() 
	print '@OrgMbrId ' + convert(varchar, isnull(@OrgMbrId,0))
	print '@IsAdminMbr ' + convert(varchar, isnull(@IsAdminMbr,0))

	if @IsAdminMbr = 1 AND isnull(@OrgMbrId,0) > 0 begin
		print 'adding admin role '
		--admin
		INSERT INTO [dbo].[Organization.MemberRole]
           (OrgContactId,[RoleId],[CreatedById])
		VALUES
           (@OrgMbrId, 1, @userId)

		   print 'adding library admin role '
		--library admin
		INSERT INTO [dbo].[Organization.MemberRole]
           (OrgContactId,[RoleId],[CreatedById])
		VALUES
           (@OrgMbrId, 3, @userId)

		end
	end
else begin 
	print 'error, user was not found, not assigned to org'
	end


GO
/****** Object:  StoredProcedure [dbo].[Organization_QuickCreateAndAssign]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

SELECT [UserId]
      ,[UserName]

      ,[FullName]
            ,[OrganizationId]
      ,[Organization]
            ,[FirstName]
      ,[LastName]
      ,[Email]
      ,[JobTitle]
      ,[RoleProfile]

  FROM [IsleContent].[dbo].[LR.PatronOrgSummary]
  where 
  --lastname like 'm%'
  email like '%rico%'
  order by LastName

GO

SELECT [id]
      ,[Name]
      ,[OrgTypeId]
      ,[parentId]
      ,[IsActive]
  
  FROM [dbo].[Organization]
GO



Id	Title
1	K12 School
2	K12 School Division
3	State Agency
4	Commercial

SELECT *  FROM [Isle_IOER].[dbo].[Patron] where UserName = 'Lward@illinoiscsi.org'
 or isactive = 0
-- ======================================================

DECLARE @RC int,@Name varchar(100),@OrgTypeId int,@parentId int,@UserEmail varchar(100),@AssignUserEmailToOrg bit,@AssignOrgPublishingRights bit
,@CreatedById int

-- Set parameter values 
set @Name= 'Rico Enterprises'
set @OrgTypeId= 3
set @parentId = 0
set @UserEmail  = 'ams@ricoenterprises.com'
set @AssignUserEmailToOrg = 1
-- if org is to have full rights, set to 1, else 0
set @AssignOrgPublishingRights = 1
set @CreatedById= 2

EXECUTE @RC = [dbo].[Organization_QuickCreateAndAssign] 
   @Name
  ,@OrgTypeId
  ,@parentId
  ,@UserEmail
  ,@AssignUserEmailToOrg
  ,@AssignOrgPublishingRights
  ,@CreatedById
GO



*/
CREATE PROCEDURE [dbo].[Organization_QuickCreateAndAssign]
            @Name varchar(100), 
            @OrgTypeId int, 
            @parentId int, 
            @UserEmail varchar(100),
			@AssignUserEmailToOrg bit,
			@AssignOrgPublishingRights bit,
            @CreatedById int
As
If @Name = ''   SET @Name = NULL 
If @OrgTypeId < 1   SET @OrgTypeId = 4 
If @parentId < 1   SET @parentId = NULL 
--email would be required - to lookup profile, but make optional for org
If @UserEmail = ''   SET @UserEmail = NULL 
If @CreatedById = 0   SET @CreatedById = NULL 


if @Name is null or @UserEmail is null begin
	RAISERROR(' Error - Org name and user email are required', 18, 1)   
	return -1
	end

declare @orgId int, @userId int, @OrgEmail varchar(100), @RC int
--could use first contact's email?
if @AssignUserEmailToOrg= 1 
	set @OrgEmail= @userEmail 
else
	set @OrgEmail = NULL

--============================================================
-- create org
--============================================================

INSERT INTO [Organization] (
    Name,     OrgTypeId,     parentId,     IsActive,     Email,     CreatedById,     LastUpdatedById)
Values (    @Name,     @OrgTypeId,     @parentId,     1,     @OrgEmail,     @CreatedById,     @CreatedById)
 
select  @orgId = SCOPE_IDENTITY() 

if @OrgId is null OR @OrgId = 0 begin
	RAISERROR(' Error - Org was not created, need to investigate', 18, 1)   
	return -1
	end

print 'new org id = ' + convert(varchar, @OrgId)
--============================================================
-- add to publishing group
--============================================================
if @AssignOrgPublishingRights = 1 begin
	print 'adding org to publishing group'
	INSERT INTO [dbo].[AppGroup.OrgMember]
			   ([GroupId], [OrgId],[IsActive])
		 VALUES (5,@OrgId ,1)

	end

--============================================================
-- get user and update profile
--============================================================

SELECT @userId = [Id]   FROM [Isle_IOER].[dbo].[Patron] where Email = @userEmail
print 'user id = ' +  convert(varchar, @userId)

if @userId is not null AND @userId > 0 begin
	print 'check for proflie'
	if NOT exists( SELECT [UserId] FROM Isle_IOER.[dbo].[Patron.Profile] where [UserId]= @userId)  begin
		--do insert
		INSERT INTO Isle_IOER.[dbo].[Patron.Profile] ([UserId] ,[OrganizationId],[CreatedById] ,[LastUpdatedId])
		 VALUES (@userId ,@orgId ,@userId,@userId)
      end    
    else begin
		--do update
		print 'Assigning user to org '
		UPDATE [Isle_IOER].[dbo].[Patron.Profile]
		   SET [OrganizationId] = @orgId 
		 WHERE UserId = @userId
		 end
	--confirm
	SELECT [Id] As UserId,[FullName],[Email],[JobTitle],[RoleProfile],[OrganizationId],Organization,[PublishingRole],[Created]      ,[LastUpdated]
	  FROM [Isle_IOER].[dbo].[Patron_Summary]
	  WHERE Id = @userId

	
	--============================================================
	-- create org mbr and role of admin 
	-- ==> assumes for first user for org
	--============================================================

	declare @OrgMbrId int
	INSERT INTO [dbo].[Organization.Member]
           ([OrgId],[UserId],[OrgMemberTypeId],[CreatedById],[LastUpdatedById])
     VALUES
           (@orgId, @userId, 1, @userId, @userId)
	select  @OrgMbrId = SCOPE_IDENTITY() 

	if isnull(@OrgMbrId,0) > 0 begin
		--admin
		INSERT INTO [dbo].[Organization.MemberRole]
           (OrgContactId,[RoleId],[CreatedById])
		VALUES
           (@OrgMbrId, 1, @userId)
		--library admin
		INSERT INTO [dbo].[Organization.MemberRole]
           (OrgContactId,[RoleId],[CreatedById])
		VALUES
           (@OrgMbrId, 3, @userId)

		end
	end
else begin 
	print 'error, user was not found, not assigned to org'
	end


GO
/****** Object:  StoredProcedure [dbo].[OrganizationDelete]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[OrganizationDelete]
        @id int
As
DELETE FROM [Organization]
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[OrganizationGet]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[OrganizationGet] 4

*/
CREATE PROCEDURE [dbo].[OrganizationGet]
    @id int
As
SELECT     
    base.id, 
    base.Name, 
    base.OrgTypeId, ot.Title as OrgType,
    base.parentId, 
    case when base.parentId is not null and base.parentId > 0 then 
      parent.Name
      else '' end as ParentOrganization,
    base.IsActive, 
    base.MainPhone, base.MainExtension,base.Fax, base.TTY, 
    base.WebSite, 
    base.Email, 
    base.LogoUrl, 
    base.Address,  base.Address2,  base.City, base.State, base.Zipcode, 
    base.Created, base.CreatedById, 
    base.LastUpdated,  base.LastUpdatedById, 
    base.RowId
FROM [Organization] base
inner join [Codes.OrgType] ot on base.OrgTypeId = ot.Id
left join Organization parent on base.parentId = parent.Id

WHERE base.id = @id

GO
/****** Object:  StoredProcedure [dbo].[OrganizationGetByName]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[OrganizationGetByName] 'Pre-Alpha evaluators'

*/
/*
get org by name - initally for easy retrieve for tagging/demo sessions
*/
CREATE PROCEDURE [dbo].[OrganizationGetByName]
    @Name varchar(100)
As
declare @Id int

select @Id = isnull(Id,0) from [Organization] where [Organization].Name = @Name
 
SELECT     
    base.id, 
    base.Name, 
    base.OrgTypeId, ot.Title as OrgType,
    base.parentId, 
    case when base.parentId is not null and base.parentId > 0 then 
      parent.Name
      else '' end as ParentOrganization,
    base.IsActive, 
    base.MainPhone, base.MainExtension,base.Fax, base.TTY, 
    base.WebSite, 
    base.Email, 
    base.LogoUrl, 
    base.Address,  base.Address2,  base.City, base.State, base.Zipcode, 
    base.Created, base.CreatedById, 
    base.LastUpdated,  base.LastUpdatedById, 
    base.RowId
FROM [Organization] base
inner join [Codes.OrgType] ot on base.OrgTypeId = ot.Id
left join Organization parent on base.parentId = parent.Id

WHERE base.id = @id

GO
/****** Object:  StoredProcedure [dbo].[OrganizationInsert]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[OrganizationInsert]
            @Name varchar(100), 
            @OrgTypeId int, 
            @parentId int, 
            @IsActive bit, 
            @MainPhone varchar(20), 
            @MainExtension varchar(10), 
            @Fax varchar(10), 
            @TTY varchar(10), 
            @WebSite varchar(100), 
            @Email varchar(100), 
            @LogoUrl varchar(200), 
            @Address varchar(50), 
            @Address2 varchar(50), 
            @City varchar(50), 
            @State char(2), 
            @Zipcode varchar(10), 
            @CreatedById int
As
If @OrgTypeId = 0   SET @OrgTypeId = NULL 
If @parentId = 0   SET @parentId = NULL 

If @MainPhone = ''   SET @MainPhone = NULL 
If @MainExtension = ''   SET @MainExtension = NULL 
If @Fax = ''   SET @Fax = NULL 
If @TTY = ''   SET @TTY = NULL 
If @WebSite = ''   SET @WebSite = NULL 
If @Email = ''   SET @Email = NULL 
If @LogoUrl = ''   SET @LogoUrl = NULL 
If @Address = ''   SET @Address = NULL 
If @Address2 = ''   SET @Address2 = NULL 
If @City = ''   SET @City = NULL 
If @State = ''   SET @State = NULL 
If @Zipcode = ''   SET @Zipcode = NULL 

If @CreatedById = 0   SET @CreatedById = NULL 


INSERT INTO [Organization] (

    Name, 
    OrgTypeId, 
    parentId, 
    IsActive, 
    MainPhone, 
    MainExtension, 
    Fax, 
    TTY, 
    WebSite, 
    Email, 
    LogoUrl, 
    Address, 
    Address2, 
    City, 
    State, 
    Zipcode, 
    CreatedById, 
    LastUpdatedById
)
Values (

    @Name, 
    @OrgTypeId, 
    @parentId, 
    @IsActive, 
    @MainPhone, 
    @MainExtension, 
    @Fax, 
    @TTY, 
    @WebSite, 
    @Email, 
    @LogoUrl, 
    @Address, 
    @Address2, 
    @City, 
    @State, 
    @Zipcode, 
    @CreatedById, 
    @CreatedById
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[OrganizationRequestDelete]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[OrganizationRequestDelete]
        @Id int
As
DELETE FROM [OrganizationRequest]
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[OrganizationRequestGet]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[OrganizationRequestGet]
    @Id int
As
SELECT     Id, 
    UserId, 
    OrgId, 
    OrganzationName, 
    Action, 
    IsActive, 
    Created, 
    LastUpdated, 
    LastUpdatedById
FROM [OrganizationRequest]
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[OrganizationRequestInsert]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[OrganizationRequestInsert]
            @UserId int, 
            @OrgId int, 
            @OrganzationName varchar(100), 
            @Action varchar(100)
As
If @UserId = 0   SET @UserId = NULL 
If @OrgId = 0   SET @OrgId = NULL 
If @OrganzationName = ''   SET @OrganzationName = NULL 
If @Action = ''   SET @Action = 'New organization' 

INSERT INTO [OrganizationRequest] (
    UserId, 
    OrgId, 
    OrganzationName, 
    Action
)
Values (
    @UserId, 
    @OrgId, 
    @OrganzationName, 
    @Action
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[OrganizationRequestSearch]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
SELECT             
    base.id, 
    base.UserId, 
    base.OrgId, 
    base.OrganzationName, 
    base.[Action],
    base.IsActive, 
   
    org.Address,  org.Address2,  org.City, org.State, org.Zipcode, 
    base.Created, base.LastUpdated,  base.LastUpdatedById
 
From [OrganizationRequest] base 
  left join [Organization] org on base.OrgId = org.Id

GO



--=====================================================

DECLARE @RC int,@SortOrder varchar(100),@Filter varchar(5000)
DECLARE @StartPageIndex int, @PageSize int, @TotalRows int
--
set @SortOrder = ''

-- blind search 
set @Filter = ''
set @Filter = ' OrgTypeId= 2'

set @Filter = ' base.IsActive = 1'

set @StartPageIndex = 1
set @PageSize = 55
--set statistics time on       
EXECUTE @RC = [OrganizationRequestSearch]
     @Filter,@SortOrder  ,@StartPageIndex  ,@PageSize, @TotalRows OUTPUT

select 'total rows = ' + convert(varchar,@TotalRows)

--set statistics time off       


*/


/* =============================================
      Description:      Org search
  Uses custom paging to only return enough rows for one page
     Options:

  @StartPageIndex - starting page number. If interface is at 20 when next
page is requested, this would be set to 21?
  @PageSize - number of records on a page
  @TotalRows OUTPUT - total available rows. Used by interface to build a
custom pager
  ------------------------------------------------------
Modifications
13-09-19 mparsons - new

*/

CREATE PROCEDURE [dbo].[OrganizationRequestSearch]
		@Filter           varchar(5000)
		,@SortOrder       varchar(100)
		,@StartPageIndex  int
		,@PageSize        int
		,@TotalRows       int OUTPUT

As

SET NOCOUNT ON;
-- paging
DECLARE
      @first_id               int
      ,@startRow        int
      ,@debugLevel      int
      ,@SQL             varchar(5000)
      ,@OrderBy         varchar(100)


-- =================================

Set @debugLevel = 4

if len(@SortOrder) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by base.Id '

--===================================================
-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1

 
-- =================================
CREATE TABLE #tempWorkTable(
      RowNumber         int PRIMARY KEY IDENTITY(1,1) NOT NULL,
      Id int,
      Title             varchar(200)
)

-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))
  set @SQL = 'SELECT distinct base.Id, base.OrganzationName 
        from [OrganizationRequest] base 
        left join [Organization] org on base.OrgId = org.Id  '
        + @Filter
        
  if charindex( 'order by', lower(@Filter) ) = 0
    set @SQL = @SQL + ' ' + @OrderBy

  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL

  INSERT INTO #tempWorkTable (Id, Title)
  exec (@SQL)
  --print 'rows: ' + convert(varchar, @@ROWCOUNT)
  SELECT @TotalRows = @@ROWCOUNT
-- =================================

print 'added to temp table: ' + convert(varchar,@TotalRows)
if @debugLevel > 7 begin
  select * from #tempWorkTable
  end

-- Calculate the range
--===================================================
PRINT '@StartPageIndex = ' + convert(varchar,@StartPageIndex)

SET ROWCOUNT @StartPageIndex
--SELECT @first_id = RowNumber FROM #tempWorkTable   ORDER BY RowNumber
SELECT @first_id = @StartPageIndex
PRINT '@first_id = ' + convert(varchar,@first_id)

if @first_id = 1 set @first_id = 0
--set max to return
SET ROWCOUNT @PageSize

-- ================================= 
SELECT        
    RowNumber, 
    base.id, 
    base.UserId, 
    base.OrgId, 
    base.OrganzationName, 
    base.[Action],
    base.IsActive, 
   
    org.Address,  org.Address2,  org.City, org.State, org.Zipcode, 
    base.Created, base.LastUpdated,  base.LastUpdatedById
 
From #tempWorkTable work
  Inner join [OrganizationRequest] base on work.Id = base.Id
  left join [Organization] org on base.OrgId = org.Id
   
WHERE RowNumber > @first_id
order by RowNumber 



GO
/****** Object:  StoredProcedure [dbo].[OrganizationRequestUpdate]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Update Procedure for [OrganizationRequest] ---
CREATE PROCEDURE [dbo].[OrganizationRequestUpdate]
        @Id int, 
        @Action varchar(100),
		@IsActive bit,
        @LastUpdatedById int
As
If @Action = ''   SET @Action = NULL 

UPDATE [OrganizationRequest] 
SET 
    Action = @Action, 
	IsActive= @IsActive,
    LastUpdated = GETDATE(), 
    LastUpdatedById = @LastUpdatedById
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[OrganizationSearch]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
SELECT     
    base.id, 
    base.Name, 
    OrgTypeId, orgt.Title as OrgType,
    parentId, 
    IsActive, 
    MainPhone, MainExtension,Fax, TTY, 
    WebSite, 
    Email, 
    LogoUrl, 
    Address,  Address2,  City, State, Zipcode, 
    base.Created, base.CreatedById, 
    base.LastUpdated,  base.LastUpdatedById, 
    base.RowId
FROM [Organization] base
inner join [Codes.OrgType] orgt on base.OrgTypeId = orgt.Id

GO



--=====================================================

DECLARE @RC int,@SortOrder varchar(100),@Filter varchar(5000)
DECLARE @StartPageIndex int, @PageSize int, @TotalRows int
--
set @SortOrder = ''

-- blind search 
set @Filter = ' OrgTypeId= 2'

--set @Filter = ''

set @StartPageIndex = 1
set @PageSize = 55
--set statistics time on       
EXECUTE @RC = [OrganizationSearch]
     @Filter,@SortOrder  ,@StartPageIndex  ,@PageSize, @TotalRows OUTPUT

select 'total rows = ' + convert(varchar,@TotalRows)

--set statistics time off       


*/


/* =============================================
      Description:      Org search
  Uses custom paging to only return enough rows for one page
     Options:

  @StartPageIndex - starting page number. If interface is at 20 when next
page is requested, this would be set to 21?
  @PageSize - number of records on a page
  @TotalRows OUTPUT - total available rows. Used by interface to build a
custom pager
  ------------------------------------------------------
Modifications
13-04-02 mparsons - new

*/

CREATE PROCEDURE [dbo].[OrganizationSearch]
		@Filter           varchar(5000)
		,@SortOrder       varchar(100)
		,@StartPageIndex  int
		,@PageSize        int
		,@TotalRows       int OUTPUT

As

SET NOCOUNT ON;
-- paging
DECLARE
      @first_id               int
      ,@startRow        int
      ,@debugLevel      int
      ,@SQL             varchar(5000)
      ,@OrderBy         varchar(100)


-- =================================

Set @debugLevel = 4

if len(@SortOrder) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by base.Name '

--===================================================
-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1

 
-- =================================
CREATE TABLE #tempWorkTable(
      RowNumber         int PRIMARY KEY IDENTITY(1,1) NOT NULL,
      Id int,
      Title             varchar(200)
)

-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))
  set @SQL = 'SELECT distinct base.Id, base.Name 
        from [Organization] base   '
        + @Filter
        
  if charindex( 'order by', lower(@Filter) ) = 0
    set @SQL = @SQL + ' ' + @OrderBy

  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL

  INSERT INTO #tempWorkTable (Id, Title)
  exec (@SQL)
  --print 'rows: ' + convert(varchar, @@ROWCOUNT)
  SELECT @TotalRows = @@ROWCOUNT
-- =================================

print 'added to temp table: ' + convert(varchar,@TotalRows)
if @debugLevel > 7 begin
  select * from #tempWorkTable
  end

-- Calculate the range
--===================================================
PRINT '@StartPageIndex = ' + convert(varchar,@StartPageIndex)

SET ROWCOUNT @StartPageIndex
--SELECT @first_id = RowNumber FROM #tempWorkTable   ORDER BY RowNumber
SELECT @first_id = @StartPageIndex
PRINT '@first_id = ' + convert(varchar,@first_id)

if @first_id = 1 set @first_id = 0
--set max to return
SET ROWCOUNT @PageSize

-- ================================= 
SELECT        
    RowNumber, 
    base.id, 
    base.Name, 
    base.OrgTypeId, orgt.Title as OrgType,
    base.parentId, 
    case when base.parentId is not null then parent.Name
      else '' end as ParentOrg,
    base.IsActive, 
    base.MainPhone, base.MainExtension,base.Fax, base.TTY, 
    base.WebSite, 
    base.Email, 
    base.LogoUrl, 
    base.Address,  base.Address2,  base.City, base.State, base.Zipcode, base.Zipcode4, 
    base.Created, base.CreatedById, 
    base.LastUpdated,  base.LastUpdatedById, 
    base.RowId
 
From #tempWorkTable work
  Inner join [Organization] base on work.Id = base.Id
  inner join [Codes.OrgType] orgt on base.OrgTypeId = orgt.Id
  left join [Organization] parent on base.ParentId = parent.Id
   
WHERE RowNumber > @first_id
order by RowNumber 


GO
/****** Object:  StoredProcedure [dbo].[OrganizationUpdate]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Update Procedure for [Organization] ---
CREATE PROCEDURE [dbo].[OrganizationUpdate]
        @id int,
        @Name varchar(100),  
        @parentId int, 
        @IsActive bit, 
        @MainPhone varchar(20), 
        @MainExtension varchar(10), 
        @Fax varchar(10), 
        @TTY varchar(10), 
        @WebSite varchar(100), 
        @Email varchar(100), 
        @LogoUrl varchar(200), 
        @Address varchar(50), 
        @Address2 varchar(50), 
        @City varchar(50), 
        @State char(2), 
        @Zipcode varchar(10),  
        @LastUpdatedById int

As
If @Name = ''   SET @Name = NULL 
If @parentId = 0   SET @parentId = NULL 
If @IsActive = 0   SET @IsActive = NULL 
If @MainPhone = ''   SET @MainPhone = NULL 
If @MainExtension = ''   SET @MainExtension = NULL 
If @Fax = ''   SET @Fax = NULL 
If @TTY = ''   SET @TTY = NULL 
If @WebSite = ''   SET @WebSite = NULL 
If @Email = ''   SET @Email = NULL 
If @LogoUrl = ''   SET @LogoUrl = NULL 
If @Address = ''   SET @Address = NULL 
If @Address2 = ''   SET @Address2 = NULL 
If @City = ''   SET @City = NULL 
If @State = ''   SET @State = NULL 
If @Zipcode = ''   SET @Zipcode = NULL 

UPDATE [Organization] 
SET 
    Name = @Name, 
    parentId = @parentId, 
    IsActive = @IsActive, 
    MainPhone = @MainPhone, 
    MainExtension = @MainExtension, 
    Fax = @Fax, 
    TTY = @TTY, 
    WebSite = @WebSite, 
    Email = @Email, 
    LogoUrl = @LogoUrl, 
    Address = @Address, 
    Address2 = @Address2, 
    City = @City, 
    State = @State, 
    Zipcode = @Zipcode,  
    LastUpdated = getdate(), 
    LastUpdatedById = @LastUpdatedById

WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[PatronGet]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
PatronGet 0, 'IllinoisPathways','', ''

PatronGet 0, '','info@illinoisworknet.com', ''

PatronGet 'IllinoisPathways','77BAE02B-9E76-4877-A90C-BFFFE9B3464E'
PatronGet 1, '', '', ''

*/
CREATE PROCEDURE [dbo].[PatronGet]
		@Id int,
		@UserName varchar(50),
		@Email varchar(100),
		@RowId varchar(40)


As
if @Id = 0            set @Id = null
if len(@RowId) = 0    set @RowId = null
if len(@Email) = 0    set @Email = null
if len(@UserName) = 0 set @UserName = null

if @id is null And @RowId is null And @Email is null And @UserName is null begin
  print 'no id provided'
  RAISERROR(' A valid id or rowId must be supplied', 18, 1)    
  RETURN -1 
  end

SELECT 
    Id, 
    UserName,  [Password],
    FirstName, 
    LastName, 
    Email, 
    SecretQuestionId, 
    SecretAnswer, 
    Created, 
    LastUpdated, 
    LastUpdatedById,
    RowId
FROM [Patron]

WHERE	
    (Id = @Id OR @Id is null)
And (UserName = @UserName OR @UserName is null)
And (Email = @Email OR @Email is null)
And (RowId = @RowId OR @RowId is null)

GO
/****** Object:  StoredProcedure [dbo].[SqlQueryDelete]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SqlQueryDelete]
        @id int
As
DELETE FROM SqlQuery
WHERE id = @id


GO
/****** Object:  StoredProcedure [dbo].[SqlQueryGet]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ==================================================
-- Retrieve single row from SqlQuery using id or code
-- Modifications
-- 09-04-17 mparsons - created
-- ==================================================
CREATE PROCEDURE [dbo].[SqlQueryGet]
        @id int,
        @QueryCode varchar(50)
As
  If @id = 0          SET @id = NULL 
  If @QueryCode = ''  SET @QueryCode = NULL 

SELECT     
		id, 
    Title, 
    [Description], 
    QueryCode, 
    Category, 
    [SQL], 
    OwnerId, 
    IsPublic, 
    Created, 
    CreatedBy, 
    LastUpdated, 
    LastUpdatedBy
FROM SqlQuery
WHERE 
		(id			    = @id					or @id is null)
And	(QueryCode  = @QueryCode	or @QueryCode is null)


GO
/****** Object:  StoredProcedure [dbo].[SqlQueryInsert]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SqlQueryInsert]
            @Title varchar(125), 
            @Description varchar(500), 
            @QueryCode varchar(50), 
            @Category varchar(25), 
            @SQL varchar(MAX), 
            @OwnerId int, 
            @IsPublic bit, 
            @CreatedBy varchar(25)
As
  If @Title = '' 
  SET @Title = NULL 
If @Description = '' 
  SET @Description = NULL 
If @QueryCode = '' 
  SET @QueryCode = NULL 
If @Category = '' 
  SET @Category = NULL 
If @OwnerId = 0 
  SET @OwnerId = NULL 
If @IsPublic = 0 
  SET @IsPublic = NULL 
If @CreatedBy = '' 
  SET @CreatedBy = 'dbo' 



INSERT INTO SqlQuery(

    Title, 
    Description, 
    QueryCode, 
    Category, 
    SQL, 
    OwnerId, 
    IsPublic, 
    Created, 
    CreatedBy, 
    LastUpdated, 
    LastUpdatedBy
)
Values (

    @Title, 
    @Description, 
    @QueryCode, 
    @Category, 
    @SQL, 
    @OwnerId, 
    @IsPublic, 
    getdate(), 
    @CreatedBy, 
    getdate(), 
    @CreatedBy
)
 
select SCOPE_IDENTITY() As Id


GO
/****** Object:  StoredProcedure [dbo].[SqlQuerySelect]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
SqlQuerySelect '','',4,''

SqlQuerySelect 'ev','',4,0
*/
-- ==================================================
-- Retrieve rows from SqlQuery using one or more filters
-- Modifications
-- 08-10-22 mparsons - created
-- 09-04-17 mparsons - removed id and changed QueryCode to use Like
-- 09-10-14 mparsons - added order by
-- 12-11-07 mparsons - added keyword
-- ==================================================
CREATE PROCEDURE [dbo].[SqlQuerySelect] 
        @QueryCode varchar(50), 
        @Category varchar(25), 
        @IsPublic smallint, 
        @OwnerId	int 
        ,@Keyword varchar(75)
--        ,@OwnerUserName varchar(75)
As
  If @QueryCode = ''  SET @QueryCode = NULL 
	if @QueryCode is not null begin
		if CHARINDEX('%', @QueryCode) = 0
			set @QueryCode = @QueryCode + '%'
		end

  If @Category = ''   SET @Category = NULL 
  -- @IsPublic -1 means all
  If @IsPublic < 0 or @IsPublic > 1   SET @IsPublic = NULL 
  If @OwnerId = 0			SET @OwnerId = NULL 
  If rtrim(ltrim(@Keyword)) = ''    SET @Keyword = NULL 
	if @Keyword is not null begin
		if CHARINDEX('%', @Keyword) = 0
			set @Keyword = '%' + @Keyword + '%'
		end

SELECT 
    q.id, 
    q.Title, 
    q.Description, 
    q.QueryCode, 
    q.Category, 
    q.SQL, 
    q.OwnerId, 
    q.IsPublic, 
    q.Created, 
    q.CreatedBy, 
    q.LastUpdated, 
    q.LastUpdatedBy
--		,vu.UserName
--		,vu.FullName
FROM 
  SqlQuery q
--    Left Join UserSummary vu on q.OwnerId = vu.Id
WHERE 
  	(q.QueryCode  like @QueryCode	or @QueryCode is null)
And	(q.Category	  = @Category	  or @Category	is null)
And	(q.IsPublic	  = @IsPublic	  or @IsPublic	is null)
And	(q.OwnerId	  = @OwnerId		or @OwnerId		is null)
And	(@Keyword is null OR 
    (q.QueryCode	like @Keyword	 OR q.Title	like @Keyword	OR q.SQL	like @Keyword	  )
    )
Order by q.Title


GO
/****** Object:  StoredProcedure [dbo].[SqlQueryUpdate]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--- Update Procedure for SqlQuery---
CREATE PROCEDURE [dbo].[SqlQueryUpdate]
        @id int,
        @Title varchar(125), 
        @Description varchar(500), 
        @QueryCode varchar(50), 
        @Category varchar(25), 
        @SQL varchar(MAX), 
        @OwnerId int, 
        @IsPublic bit, 
        @LastUpdatedBy varchar(25)

As
  If @Title = '' 
  SET @Title = NULL 
If @Description = '' 
  SET @Description = NULL 
If @QueryCode = '' 
  SET @QueryCode = NULL 
If @Category = '' 
  SET @Category = NULL 
If @OwnerId = 0 
  SET @OwnerId = NULL 
If @IsPublic = 0 
  SET @IsPublic = NULL 
If @LastUpdatedBy = '' 
  SET @LastUpdatedBy = 'dbo' 

UPDATE SqlQuery
SET 
    Title = @Title, 
    [Description] = @Description, 
    QueryCode = @QueryCode, 
    Category = @Category, 
    [SQL] = @SQL, 
    OwnerId = @OwnerId, 
    IsPublic = @IsPublic, 
    LastUpdatedBy = @LastUpdatedBy,
		LastUpdated = getdate()
WHERE id = @id


GO
/****** Object:  Table [dbo].[AppGroup]    Script Date: 3/9/2014 10:00:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AppGroup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [int] NULL,
	[GroupCode] [varchar](50) NOT NULL,
	[Title] [varchar](100) NOT NULL,
	[Description] [varchar](500) NULL,
	[GroupTypeId] [int] NULL,
	[IsActive] [bit] NULL,
	[ContactId] [int] NULL,
	[OrgId] [int] NULL,
	[ParentGroupId] [int] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_AppGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_AppGroup_RowGuid] UNIQUE NONCLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_AppGroupCode] UNIQUE NONCLUSTERED 
(
	[GroupCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AppGroup.Member]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AppGroup.Member](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[UserRowId] [uniqueidentifier] NULL,
	[OrgId] [int] NULL,
	[Status] [varchar](50) NULL,
	[Category] [varchar](25) NULL,
	[IsActive] [bit] NULL,
	[Comment] [varchar](500) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_AppGroup.TeamMember] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AppGroup.OrgMember]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppGroup.OrgMember](
	[GroupId] [int] NOT NULL,
	[OrgId] [int] NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_GroupContact] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AppGroup.Privilege]    Script Date: 3/9/2014 10:00:59 PM ******/
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
	[Created] [datetime] NULL,
 CONSTRAINT [PK_AppGroupPrivilege] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AppGroup.TeamMember]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppGroup.TeamMember](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[UserRowId] [uniqueidentifier] NULL,
	[CreatePrivilege] [int] NULL,
	[ReadPrivilege] [int] NULL,
	[WritePrivilege] [int] NULL,
	[DeletePrivilege] [int] NULL,
	[AppendPrivilege] [int] NULL,
	[AppendToPrivilege] [int] NULL,
	[AssignPrivilege] [int] NULL,
	[ApprovePrivilege] [int] NULL,
	[SharePrivilege] [int] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_AppGroup.Manager] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Application]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Application](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](100) NULL,
	[Description] [varchar](250) NULL,
	[RowId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Application] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Application.Member]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Application.Member](
	[AppId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[Password] [nvarchar](128) NOT NULL,
	[RowId] [uniqueidentifier] NULL,
	[Token] [varchar](100) NULL,
	[PasswordQuestion] [nvarchar](256) NULL,
	[PasswordAnswer] [nvarchar](128) NULL,
	[IsApproved] [bit] NOT NULL,
	[ApprovalDate] [datetime] NOT NULL,
	[IsLockedOut] [bit] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastLoginDate] [datetime] NOT NULL,
	[Comment] [varchar](max) NULL,
 CONSTRAINT [PK_Application.Member] PRIMARY KEY CLUSTERED 
(
	[AppId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ApplicationObject]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ApplicationObject](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ObjectName] [nvarchar](100) NULL,
	[DisplayName] [nvarchar](75) NULL,
	[Description] [nvarchar](250) NULL,
	[Active] [bit] NULL,
	[ObjectType] [varchar](50) NULL,
	[RelatedUrl] [varchar](150) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_ApplicationObject] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ApplicationRolePrivilege]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApplicationRolePrivilege](
	[RoleId] [int] NOT NULL,
	[ObjectId] [int] NOT NULL,
	[Sequence] [int] NOT NULL,
	[Create] [int] NULL,
	[Read] [int] NULL,
	[Write] [int] NULL,
	[Delete] [int] NULL,
	[Append] [int] NULL,
	[AppendTo] [int] NULL,
	[Assign] [int] NULL,
	[Approve] [int] NULL,
	[Share] [int] NULL,
	[Created] [datetime] NULL,
	[RoleCode] [smallint] NULL,
 CONSTRAINT [PK_RolePrivilege] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC,
	[ObjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AppUser]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AppUser](
	[Id] [int] NOT NULL,
	[UserName] [varchar](50) NOT NULL,
	[Password] [varchar](50) NULL,
	[FirstName] [varchar](50) NULL,
	[LastName] [varchar](50) NULL,
	[Email] [varchar](100) NULL,
	[SecretQuestionId] [int] NULL,
	[SecretAnswer] [varchar](50) NULL,
	[Created] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_AppUser] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AppUser.ExternalAccount]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AppUser.ExternalAccount](
	[UserId] [int] NULL,
	[ExternalSiteId] [int] NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LoginId] [varchar](50) NULL,
	[Password] [varchar](50) NULL,
	[Token] [varchar](100) NULL,
 CONSTRAINT [PK_AppUser.ExternalSite] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AppUser.Profile]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AppUser.Profile](
	[UserId] [int] NOT NULL,
	[MainPhone] [varchar](15) NULL,
	[JobTitle] [varchar](50) NULL,
	[PublishingRoleId] [int] NULL,
	[RoleProfile] [varchar](500) NULL,
	[OrganizationId] [int] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedId] [int] NULL,
	[Notes] [varchar](1000) NULL,
 CONSTRAINT [PK_AppUser.Profile] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AppVisitLog]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AppVisitLog](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[SessionId] [varchar](30) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[Application] [varchar](50) NOT NULL,
	[URL] [varchar](300) NOT NULL,
	[Parameters] [varchar](300) NULL,
	[IsPostback] [bit] NULL,
	[Userid] [varchar](50) NULL,
	[Comment] [varchar](250) NULL,
	[RemoteIP] [varchar](25) NULL,
	[ServerName] [varchar](25) NULL,
	[CurrentZip] [varchar](12) NULL,
 CONSTRAINT [PK_AppVisitLog] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.AuthorizationLevel]    Script Date: 3/9/2014 10:00:59 PM ******/
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
/****** Object:  Table [dbo].[Codes.ExternalSite]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ExternalSite](
	[Id] [int] IDENTITY(1,1) NOT NULL,
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
/****** Object:  Table [dbo].[Codes.GroupType]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.GroupType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Codes.GroupType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.OrgMemberRole]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.OrgMemberRole](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Codes.OrgMemberRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.OrgMemberType]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.OrgMemberType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Codes.OrgMemberType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.OrgType]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.OrgType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Codes.OrgType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.PrivilegeDepth]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.PrivilegeDepth](
	[Id] [int] NOT NULL,
	[Title] [varchar](75) NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Codes.PrivilegeDepth] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.PrivilegeType]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.PrivilegeType](
	[Id] [int] NOT NULL,
	[Title] [varchar](75) NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Codes.PrivilegeType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.PublishingRole]    Script Date: 3/9/2014 10:00:59 PM ******/
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
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Codes.PublishingRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.State]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.State](
	[Id] [int] IDENTITY(1,1) NOT NULL,
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
/****** Object:  Table [dbo].[CodeTable]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CodeTable](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CodeName] [varchar](50) NOT NULL,
	[LanguageCode] [varchar](10) NOT NULL,
	[StringValue] [varchar](100) NOT NULL,
	[NumericValue] [decimal](12, 4) NULL,
	[IntegerValue] [int] NULL,
	[Description] [varchar](500) NULL,
	[SortOrder] [tinyint] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[Modified] [datetime] NULL,
 CONSTRAINT [PK_CodeTable] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[EmailNotice]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EmailNotice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](75) NOT NULL,
	[Category] [varchar](50) NOT NULL,
	[NoticeCode] [varchar](50) NOT NULL,
	[Description] [varchar](500) NOT NULL,
	[Filter] [varchar](max) NULL,
	[isActive] [bit] NOT NULL,
	[LanguageCode] [varchar](10) NULL,
	[FromEmail] [nvarchar](100) NOT NULL,
	[CcEmail] [nvarchar](500) NULL,
	[BccEmail] [nvarchar](100) NULL,
	[Subject] [nvarchar](100) NOT NULL,
	[HtmlBody] [nvarchar](max) NOT NULL,
	[TextBody] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedById] [int] NULL,
	[CreatedBy] [varchar](50) NULL,
	[LastUpdatedBy] [varchar](50) NULL,
 CONSTRAINT [PK_EmailNotice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Organization]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Organization](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[OrgTypeId] [int] NULL,
	[parentId] [int] NULL,
	[IsActive] [bit] NULL,
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
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Organization] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Organization.Member]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Organization.Member](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrgId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[OrgMemberTypeId] [int] NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Organization.Member] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Organization.MemberRole]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Organization.MemberRole](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrgContactId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Organization.MemberRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[OrganizationRequest]    Script Date: 3/9/2014 10:00:59 PM ******/
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
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_OrganizationRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[OrganizationTemp]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[OrganizationTemp](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[OrgTypeId] [int] NULL,
	[parentId] [int] NULL,
	[IsActive] [bit] NULL,
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
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_OrganizationTemp] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SqlQuery]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SqlQuery](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](125) NOT NULL,
	[Description] [varchar](500) NULL,
	[QueryCode] [varchar](50) NULL,
	[Category] [varchar](25) NULL,
	[SQL] [varchar](max) NOT NULL,
	[OwnerId] [int] NULL,
	[IsPublic] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](50) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](50) NULL,
 CONSTRAINT [PK_SqlQuery] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [dbo].[LR.PatronOrgSummary]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
select * from Isle_IOER.[dbo].[Patron_Summary]

SELECT [UserId]
      ,[UserName]

      ,[FullName]
            ,[OrganizationId]
      ,[Organization]
            ,[FirstName]
      ,[LastName]
      ,[Email]
      ,[JobTitle]
      ,[RoleProfile]
	  ,Created, LastUpdated

  FROM [dbo].[LR.PatronOrgSummary]
  order by LastName
*/
/*
[LR.PatronOrgSummary] - select User summary . 
    
*/
CREATE VIEW [dbo].[LR.PatronOrgSummary] AS
SELECT base.[Id] As UserId
      ,base.[UserName]
      ,base.[FirstName],base.[LastName], base.[FullName], base.SortName
      ,base.[Email]
      ,PublishingRole
	  ,base.ImageUrl 
	  ,base.UserRowId
      ,base.[JobTitle]
      ,base.[RoleProfile]
      ,base.[OrganizationId]
      ,case when org.Id is not null then org.Name
        else 'none' end As Organization
        	  ,base.Created, base.LastUpdated
  FROM Isle_IOER.[dbo].[Patron_Summary] base
  Left join [Gateway].[dbo].[Organization] org on base.OrganizationId = org.Id
  



GO
/****** Object:  View [dbo].[Organization_MemberSummary]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[Organization_MemberSummary]
AS
SELECT        
	ombr.Id As OrgMbrId,
	ombr.OrgId, 
	org.Name AS Organization, 
	ombr.UserId, 
	pos.FirstName,   
	pos.LastName, 
	pos.Email, 
	ombr.OrgMemberTypeId, 
	omt.Title AS OrgMemberType, 
	ombr.Created AS MemberAdded, 
	pos.OrganizationId AS BaseOrgId, 
	pos.Organization AS BaseOrganization

FROM            
	dbo.Organization org
	INNER JOIN dbo.[Organization.Member] ombr ON org.id = ombr.OrgId 
	INNER JOIN dbo.[Codes.OrgMemberType] omt ON ombr.OrgMemberTypeId = omt.Id 
	INNER JOIN dbo.[LR.PatronOrgSummary] pos ON ombr.UserId = pos.UserId


GO
/****** Object:  View [dbo].[OrganizationMember.RoleCSV]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
USE [Gateway]
GO

SELECT [OrgMbrId]
      ,[Roles]
  FROM [dbo].[OrganizationMember.RoleCSV]
GO


*/

CREATE VIEW [dbo].[OrganizationMember.RoleCSV]
AS
SELECT     TOP (100) PERCENT 
base.[Id] As OrgMbrId,
    CASE
          WHEN Roles IS NULL THEN ''
          WHEN len(Roles) = 0 THEN ''
          ELSE left(Roles,len(Roles)-1)
    END AS Roles

FROM [dbo].[Organization.Member] base

CROSS APPLY (
    SELECT codes.Title  + ', '
   -- ,rsub.ResourceId
    FROM dbo.[Organization.MemberRole] omr 
	Inner Join [Codes.OrgMemberRole] codes on omr.RoleId = codes.Id  
    WHERE base.Id = omr.OrgContactId
    FOR XML Path('') ) D (Roles)
where Roles is not null


GO
/****** Object:  View [dbo].[Organization.AdminMembers]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[Organization.AdminMembers]
AS
SELECT Distinct       
	ombr.Id As OrgMbrId,
	ombr.OrgId, 
	org.Name AS Organization, 
	ombr.UserId, 
	pos.FirstName,   
	pos.LastName, 
	pos.Email, 
	ombr.OrgMemberTypeId, 
	omt.Title AS OrgMemberType, 
	omroles.[Roles],
	ombr.Created AS MemberAdded, 
	pos.OrganizationId AS BaseOrgId, 
	pos.Organization AS BaseOrganization

FROM            
	dbo.Organization org
	INNER JOIN dbo.[Organization.Member] ombr ON org.id = ombr.OrgId 
	INNER JOIN dbo.[Codes.OrgMemberType] omt ON ombr.OrgMemberTypeId = omt.Id 
	INNER JOIN dbo.[LR.PatronOrgSummary] pos ON ombr.UserId = pos.UserId
	Inner Join dbo.[Organization.MemberRole] omr on ombr.Id = omr.OrgContactId
	inner join [OrganizationMember.RoleCSV] omroles on ombr.Id = omroles.OrgMbrId
where omr.RoleId in (1,3)

GO
/****** Object:  View [dbo].[ApplicationPrivileges]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- select * from ApplicationPrivileges Order by 1, 5,6
CREATE VIEW [dbo].[ApplicationPrivileges]
AS
SELECT Top 100 Percent
base.RoleId,
 --dbo.WorkNetRole.Name, dbo.WorkNetRole.RoleCode As RoleLevel,
base.ObjectId, dbo.ApplicationObject.ObjectName, dbo.ApplicationObject.DisplayName, 
dbo.ApplicationObject.ObjectType,
--dbo.WorkNetRole.RoleCode As Sequence, 
base.[Create] AS CreatePrivilege,   auth0.Title AS CreateTitle, 
base.[Read]   AS ReadPrivilege,     auth1.Title AS ReadTitle, 
base.Write    AS WritePrivilege,   auth2.Title AS UpdateTitle, 
base.Write    AS UpdatePrivilege,
base.[Delete] AS DeletePrivilege,   auth3.Title AS DeleteTitle, 
base.Append   AS AppendPrivilege,   auth4.Title AS AppendTitle, 
base.Approve  AS ApprovePrivilege,  auth5.Title AS ApproveTitle, 
base.Assign   AS AssignPrivilege,   auth6.Title AS AssignTitle, 
base.Share    AS SharePrivilege,    auth7.Title AS ShareTitle, 
base.AppendTo AS AppendToPrivilege, auth8.Title AS AppendToTitle, 
base.Created,
--dbo.ApplicationObject.SubObjectId,
--dbo.ApplicationObject.SubObjectName
ApplicationObject.description 
FROM dbo.ApplicationRolePrivilege base
  INNER JOIN dbo.ApplicationObject ON base.ObjectId = dbo.ApplicationObject.id 
  --INNER JOIN dbo.WorkNetRole ON base.RoleId = dbo.WorkNetRole.id 
  INNER JOIN [Codes.AuthorizationLevel] auth0 ON base.[Create] = auth0.Id 
  INNER JOIN dbo.[Codes.AuthorizationLevel] AS auth1 ON base.[Read] = auth1.Id 
  INNER JOIN dbo.[Codes.AuthorizationLevel] AS auth2 ON base.Write = auth2.Id 
  INNER JOIN dbo.[Codes.AuthorizationLevel] AS auth3 ON base.[Delete] = auth3.Id 
  INNER JOIN dbo.[Codes.AuthorizationLevel] AS auth4 ON base.Append = auth4.Id 
  INNER JOIN dbo.[Codes.AuthorizationLevel] AS auth5 ON base.Approve = auth5.Id 
  INNER JOIN dbo.[Codes.AuthorizationLevel] AS auth6 ON base.Assign = auth6.Id 
  INNER JOIN dbo.[Codes.AuthorizationLevel] AS auth7 ON base.Share = auth7.Id 
  INNER JOIN dbo.[Codes.AuthorizationLevel] AS auth8 ON base.AppendTo = auth8.Id
order by
base.RoleId, 
--dbo.WorkNetRole.Name, 
base.ObjectId, dbo.ApplicationObject.ObjectName



GO
/****** Object:  View [dbo].[AppObject_Group_OrgPrivilegesSummary]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[AppObject_Group_OrgPrivilegesSummary]
AS
SELECT     
  dbo.ApplicationObject.ObjectName, 
  dbo.ApplicationObject.DisplayName, 
  dbo.AppGroup.GroupCode, 
  dbo.AppGroup.Title, 
  dbo.[AppGroup.Privilege].GroupId, 
  ombr.OrgId, org.Name As OrgName,
  dbo.[AppGroup.Privilege].ObjectId, 
  
  dbo.[AppGroup.Privilege].CreatePrivilege, dbo.[AppGroup.Privilege].ReadPrivilege, dbo.[AppGroup.Privilege].WritePrivilege, dbo.[AppGroup.Privilege].DeletePrivilege, dbo.[AppGroup.Privilege].AppendPrivilege, dbo.[AppGroup.Privilege].AppendToPrivilege, dbo.[AppGroup.Privilege].AssignPrivilege, dbo.[AppGroup.Privilege].ApprovePrivilege, dbo.[AppGroup.Privilege].SharePrivilege, 
  dbo.ApplicationObject.Description
  
FROM  dbo.AppGroup 
INNER JOIN dbo.[AppGroup.OrgMember] ombr ON dbo.AppGroup.id = ombr.GroupId 
INNER JOIN dbo.[AppGroup.Privilege] ON dbo.AppGroup.id = dbo.[AppGroup.Privilege].GroupId 
INNER JOIN dbo.ApplicationObject ON dbo.[AppGroup.Privilege].ObjectId = dbo.ApplicationObject.id
inner Join dbo.Organization org on ombr.OrgId = org.id


GO
/****** Object:  View [dbo].[AppObject_Group_UserPrivileges]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[AppObject_Group_UserPrivileges]
AS
SELECT     
  dbo.ApplicationObject.ObjectName, 
  dbo.ApplicationObject.DisplayName, 
  dbo.AppGroup.GroupCode, 
  dbo.AppGroup.ContactId, 
  dbo.AppGroup.Title, 
  dbo.[AppGroup.Privilege].GroupId, 
  dbo.[AppGroup.Member].UserId, 
  dbo.[AppGroup.Privilege].ObjectId, 
  dbo.[AppGroup.Privilege].CreatePrivilege, dbo.[AppGroup.Privilege].ReadPrivilege, dbo.[AppGroup.Privilege].WritePrivilege, dbo.[AppGroup.Privilege].DeletePrivilege, dbo.[AppGroup.Privilege].AppendPrivilege, dbo.[AppGroup.Privilege].AppendToPrivilege, dbo.[AppGroup.Privilege].AssignPrivilege, dbo.[AppGroup.Privilege].ApprovePrivilege, dbo.[AppGroup.Privilege].SharePrivilege, 
  dbo.ApplicationObject.Description
FROM  dbo.AppGroup 
INNER JOIN dbo.[AppGroup.Member] ON dbo.AppGroup.id = dbo.[AppGroup.Member].GroupId 
INNER JOIN dbo.[AppGroup.Privilege] ON dbo.AppGroup.id = dbo.[AppGroup.Privilege].GroupId 
INNER JOIN dbo.ApplicationObject ON dbo.[AppGroup.Privilege].ObjectId = dbo.ApplicationObject.id


GO
/****** Object:  View [dbo].[Organization_Summary]    Script Date: 3/9/2014 10:00:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[Organization_Summary]
AS
SELECT     
    base.id, 
    base.Name, 
    base.OrgTypeId, ot.Title as OrgType,
    base.parentId, 
    case when base.parentId is not null and base.parentId > 0 then 
      parent.Name
      else '' end as ParentOrganization,
    base.IsActive, 
    base.MainPhone, base.MainExtension,base.Fax, base.TTY, 
    base.WebSite, 
    base.Email, 
    base.LogoUrl, 
    base.Address,  base.Address2,  base.City, base.State, base.Zipcode, 
    base.Created, base.CreatedById, 
    base.LastUpdated,  base.LastUpdatedById, 
    base.RowId
FROM [Organization] base
inner join [Codes.OrgType] ot on base.OrgTypeId = ot.Id
left join Organization parent on base.parentId = parent.Id

GO
/****** Object:  Index [IX_AppGroup.OrgMember]    Script Date: 3/9/2014 10:00:59 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppGroup.OrgMember] ON [dbo].[AppGroup.OrgMember]
(
	[GroupId] ASC,
	[OrgId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_AppGroup.OrgMember_1]    Script Date: 3/9/2014 10:00:59 PM ******/
CREATE NONCLUSTERED INDEX [IX_AppGroup.OrgMember_1] ON [dbo].[AppGroup.OrgMember]
(
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_AppGroup.OrgMember_OrgId]    Script Date: 3/9/2014 10:00:59 PM ******/
CREATE NONCLUSTERED INDEX [IX_AppGroup.OrgMember_OrgId] ON [dbo].[AppGroup.OrgMember]
(
	[OrgId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Organization.Member_orgIdUserId]    Script Date: 3/9/2014 10:00:59 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Organization.Member_orgIdUserId] ON [dbo].[Organization.Member]
(
	[OrgId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Organization.MemberRole_MbrIdRoleId]    Script Date: 3/9/2014 10:00:59 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Organization.MemberRole_MbrIdRoleId] ON [dbo].[Organization.MemberRole]
(
	[OrgContactId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_SqlQuery_QueryCode]    Script Date: 3/9/2014 10:00:59 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_SqlQuery_QueryCode] ON [dbo].[SqlQuery]
(
	[QueryCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AppGroup] ADD  CONSTRAINT [DF_AppGroup_GroupCode]  DEFAULT (newid()) FOR [GroupCode]
GO
ALTER TABLE [dbo].[AppGroup] ADD  CONSTRAINT [DF_AppGroup_GroupTypeId]  DEFAULT ((1)) FOR [GroupTypeId]
GO
ALTER TABLE [dbo].[AppGroup] ADD  CONSTRAINT [DF_AppGroup_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[AppGroup] ADD  CONSTRAINT [DF_AppGroup_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[AppGroup] ADD  CONSTRAINT [DF_AppGroup_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[AppGroup] ADD  CONSTRAINT [DF_AppGroup_RowGuid]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[AppGroup.Member] ADD  CONSTRAINT [DF_AppGroup.Member_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[AppGroup.Member] ADD  CONSTRAINT [DF_GroupTeamMember_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[AppGroup.Member] ADD  CONSTRAINT [DF_GroupTeamMember_Created1]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[AppGroup.OrgMember] ADD  CONSTRAINT [DF_GroupContact_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[AppGroup.OrgMember] ADD  CONSTRAINT [DF_GroupContact_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[AppGroup.OrgMember] ADD  CONSTRAINT [DF_GroupContact_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[AppGroup.OrgMember] ADD  CONSTRAINT [DF_AppGroup.OrgMember_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[AppGroup.Privilege] ADD  CONSTRAINT [DF_ApplicationGroupPrivilege_Create]  DEFAULT ((0)) FOR [CreatePrivilege]
GO
ALTER TABLE [dbo].[AppGroup.Privilege] ADD  CONSTRAINT [DF_ApplicationGroupPrivilege_Read]  DEFAULT ((0)) FOR [ReadPrivilege]
GO
ALTER TABLE [dbo].[AppGroup.Privilege] ADD  CONSTRAINT [DF_ApplicationGroupPrivilege_Write]  DEFAULT ((0)) FOR [WritePrivilege]
GO
ALTER TABLE [dbo].[AppGroup.Privilege] ADD  CONSTRAINT [DF_ApplicationGroupPrivilege_Delete]  DEFAULT ((0)) FOR [DeletePrivilege]
GO
ALTER TABLE [dbo].[AppGroup.Privilege] ADD  CONSTRAINT [DF_ApplicationGroupPrivilege_Append]  DEFAULT ((0)) FOR [AppendPrivilege]
GO
ALTER TABLE [dbo].[AppGroup.Privilege] ADD  CONSTRAINT [DF_ApplicationGroupPrivilege_AppendTo]  DEFAULT ((0)) FOR [AppendToPrivilege]
GO
ALTER TABLE [dbo].[AppGroup.Privilege] ADD  CONSTRAINT [DF_ApplicationGroupPrivilege_Assign]  DEFAULT ((0)) FOR [AssignPrivilege]
GO
ALTER TABLE [dbo].[AppGroup.Privilege] ADD  CONSTRAINT [DF_ApplicationGroupPrivilege_Approve]  DEFAULT ((0)) FOR [ApprovePrivilege]
GO
ALTER TABLE [dbo].[AppGroup.Privilege] ADD  CONSTRAINT [DF_ApplicationGroupPrivilege_Share]  DEFAULT ((0)) FOR [SharePrivilege]
GO
ALTER TABLE [dbo].[AppGroup.Privilege] ADD  CONSTRAINT [DF_ApplicationGroupPrivilege_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[AppGroup.TeamMember] ADD  CONSTRAINT [DF_GroupTeamManager_Create]  DEFAULT ((0)) FOR [CreatePrivilege]
GO
ALTER TABLE [dbo].[AppGroup.TeamMember] ADD  CONSTRAINT [DF_GroupTeamManager_Read]  DEFAULT ((0)) FOR [ReadPrivilege]
GO
ALTER TABLE [dbo].[AppGroup.TeamMember] ADD  CONSTRAINT [DF_GroupTeamManager_Write]  DEFAULT ((0)) FOR [WritePrivilege]
GO
ALTER TABLE [dbo].[AppGroup.TeamMember] ADD  CONSTRAINT [DF_GroupTeamManager_Delete]  DEFAULT ((0)) FOR [DeletePrivilege]
GO
ALTER TABLE [dbo].[AppGroup.TeamMember] ADD  CONSTRAINT [DF_GroupTeamManager_Append]  DEFAULT ((0)) FOR [AppendPrivilege]
GO
ALTER TABLE [dbo].[AppGroup.TeamMember] ADD  CONSTRAINT [DF_GroupTeamManager_AppendTo]  DEFAULT ((1)) FOR [AppendToPrivilege]
GO
ALTER TABLE [dbo].[AppGroup.TeamMember] ADD  CONSTRAINT [DF_GroupTeamManager_Assign]  DEFAULT ((0)) FOR [AssignPrivilege]
GO
ALTER TABLE [dbo].[AppGroup.TeamMember] ADD  CONSTRAINT [DF_GroupTeamManager_Approve]  DEFAULT ((0)) FOR [ApprovePrivilege]
GO
ALTER TABLE [dbo].[AppGroup.TeamMember] ADD  CONSTRAINT [DF_GroupTeamManager_Share]  DEFAULT ((0)) FOR [SharePrivilege]
GO
ALTER TABLE [dbo].[AppGroup.TeamMember] ADD  CONSTRAINT [DF_GroupTeamManager_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[AppGroup.TeamMember] ADD  CONSTRAINT [DF_GroupTeamManager_Created1]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Application] ADD  CONSTRAINT [DF_Application_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Application.Member] ADD  CONSTRAINT [DF_Application.Member_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[ApplicationObject] ADD  CONSTRAINT [DF_ApplicationObject_Active]  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[ApplicationObject] ADD  CONSTRAINT [DF_ApplicationObject_ObjectType]  DEFAULT ('Control') FOR [ObjectType]
GO
ALTER TABLE [dbo].[ApplicationObject] ADD  CONSTRAINT [DF_ApplicationObject_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[ApplicationObject] ADD  CONSTRAINT [DF_ApplicationObject_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege] ADD  CONSTRAINT [DF_RolePrivilege_Sequence]  DEFAULT ((1)) FOR [Sequence]
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege] ADD  CONSTRAINT [DF_RolePrivilege_CreatePrivilege]  DEFAULT ((0)) FOR [Create]
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege] ADD  CONSTRAINT [DF_RolePrivilege_ReadPrivilege]  DEFAULT ((0)) FOR [Read]
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege] ADD  CONSTRAINT [DF_RolePrivilege_WritePrivilege]  DEFAULT ((0)) FOR [Write]
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege] ADD  CONSTRAINT [DF_RolePrivilege_DeletePrivilege]  DEFAULT ((0)) FOR [Delete]
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege] ADD  CONSTRAINT [DF_RolePrivilege_AppendPrivilege]  DEFAULT ((0)) FOR [Append]
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege] ADD  CONSTRAINT [DF_RolePrivilege_AppendToPrivilege]  DEFAULT ((0)) FOR [AppendTo]
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege] ADD  CONSTRAINT [DF_RolePrivilege_AssignPrivilege]  DEFAULT ((0)) FOR [Assign]
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege] ADD  CONSTRAINT [DF_RolePrivilege_ApprovePrivilege]  DEFAULT ((0)) FOR [Approve]
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege] ADD  CONSTRAINT [DF_RolePrivilege_SharePrivilege]  DEFAULT ((0)) FOR [Share]
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege] ADD  CONSTRAINT [DF_ApplicationRolePrivilege_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [DF_AppUser_Password]  DEFAULT (newid()) FOR [Password]
GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [DF_AppUser_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [DF_AppUser_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[AppUser] ADD  CONSTRAINT [DF_AppUser_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[AppUser.Profile] ADD  CONSTRAINT [DF_AppUser.Profile_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[AppUser.Profile] ADD  CONSTRAINT [DF_AppUser.Profile_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Codes.AuthorizationLevel] ADD  CONSTRAINT [DF_Codes.AuthorizationLevel_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Codes.GroupType] ADD  CONSTRAINT [DF_Codes.GroupType_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Codes.OrgMemberRole] ADD  CONSTRAINT [DF_Codes.OrgMemberRole_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Codes.OrgMemberType] ADD  CONSTRAINT [DF_Codes.OrgMemberType_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Codes.OrgType] ADD  CONSTRAINT [DF_Codes.OrgType_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Codes.PrivilegeDepth] ADD  CONSTRAINT [DF_Codes.PrivilegeDepth_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Codes.PrivilegeType] ADD  CONSTRAINT [DF_Codes.PrivilegeType_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Codes.PublishingRole] ADD  CONSTRAINT [DF_Codes.PublishingRole_IsOrgRequired]  DEFAULT ((1)) FOR [IsOrgRequired]
GO
ALTER TABLE [dbo].[Codes.PublishingRole] ADD  CONSTRAINT [DF_Codes.PublishingRole_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.PublishingRole] ADD  CONSTRAINT [DF_Codes.PublishingRole_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[CodeTable] ADD  CONSTRAINT [DF_CodeTable_LanguageCode_1]  DEFAULT ('en') FOR [LanguageCode]
GO
ALTER TABLE [dbo].[CodeTable] ADD  CONSTRAINT [DF_CodeTable_SortOrder]  DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[CodeTable] ADD  CONSTRAINT [DF_CodeTable_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[CodeTable] ADD  CONSTRAINT [DF_CodeTable_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[CodeTable] ADD  CONSTRAINT [DF_CodeTable_Modified]  DEFAULT (getdate()) FOR [Modified]
GO
ALTER TABLE [dbo].[EmailNotice] ADD  CONSTRAINT [DF_EmailNotice_isActive]  DEFAULT ((1)) FOR [isActive]
GO
ALTER TABLE [dbo].[EmailNotice] ADD  CONSTRAINT [DF_EmailNotice_langCode]  DEFAULT ('en') FOR [LanguageCode]
GO
ALTER TABLE [dbo].[EmailNotice] ADD  CONSTRAINT [DF_EmailNotice_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[EmailNotice] ADD  CONSTRAINT [DF_EmailNotice_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Organization] ADD  CONSTRAINT [DF_Organization_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Organization] ADD  CONSTRAINT [DF_Organization_State]  DEFAULT ('IL') FOR [State]
GO
ALTER TABLE [dbo].[Organization] ADD  CONSTRAINT [DF_Organization_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Organization] ADD  CONSTRAINT [DF_Organization_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Organization] ADD  CONSTRAINT [DF_Organization_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Organization.Member] ADD  CONSTRAINT [DF_Organization.Member_OrgMemberTypeId]  DEFAULT ((2)) FOR [OrgMemberTypeId]
GO
ALTER TABLE [dbo].[Organization.Member] ADD  CONSTRAINT [DF_Organization.Member_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Organization.Member] ADD  CONSTRAINT [DF_Organization.Member_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Organization.MemberRole] ADD  CONSTRAINT [DF_Organization.MemberRole_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[OrganizationRequest] ADD  CONSTRAINT [DF_OrganizationRequest_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[OrganizationRequest] ADD  CONSTRAINT [DF_OrganizationRequest_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[OrganizationRequest] ADD  CONSTRAINT [DF_OrganizationRequest_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[OrganizationTemp] ADD  CONSTRAINT [DF_OrganizationTemp_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[OrganizationTemp] ADD  CONSTRAINT [DF_OrganizationTemp_State]  DEFAULT ('IL') FOR [State]
GO
ALTER TABLE [dbo].[OrganizationTemp] ADD  CONSTRAINT [DF_OrganizationTemp_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[OrganizationTemp] ADD  CONSTRAINT [DF_OrganizationTemp_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[OrganizationTemp] ADD  CONSTRAINT [DF_OrganizationTemp_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[SqlQuery] ADD  CONSTRAINT [DF_Table_1_Type]  DEFAULT ('General') FOR [Category]
GO
ALTER TABLE [dbo].[SqlQuery] ADD  CONSTRAINT [DF_SqlQuery_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[SqlQuery] ADD  CONSTRAINT [DF_SqlQuery_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[AppGroup]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup_Application] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Application] ([Id])
GO
ALTER TABLE [dbo].[AppGroup] CHECK CONSTRAINT [FK_AppGroup_Application]
GO
ALTER TABLE [dbo].[AppGroup]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup_Organization] FOREIGN KEY([OrgId])
REFERENCES [dbo].[Organization] ([id])
GO
ALTER TABLE [dbo].[AppGroup] CHECK CONSTRAINT [FK_AppGroup_Organization]
GO
ALTER TABLE [dbo].[AppGroup.Member]  WITH CHECK ADD  CONSTRAINT [FK_GroupMember_Group] FOREIGN KEY([GroupId])
REFERENCES [dbo].[AppGroup] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AppGroup.Member] CHECK CONSTRAINT [FK_GroupMember_Group]
GO
ALTER TABLE [dbo].[AppGroup.OrgMember]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup.OrgMember_AppGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[AppGroup] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AppGroup.OrgMember] CHECK CONSTRAINT [FK_AppGroup.OrgMember_AppGroup]
GO
ALTER TABLE [dbo].[AppGroup.OrgMember]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup.OrgMember_Organization] FOREIGN KEY([OrgId])
REFERENCES [dbo].[Organization] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AppGroup.OrgMember] CHECK CONSTRAINT [FK_AppGroup.OrgMember_Organization]
GO
ALTER TABLE [dbo].[AppGroup.Privilege]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup.Privilege_AppGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[AppGroup] ([Id])
GO
ALTER TABLE [dbo].[AppGroup.Privilege] CHECK CONSTRAINT [FK_AppGroup.Privilege_AppGroup]
GO
ALTER TABLE [dbo].[AppGroup.Privilege]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup.Privilege_ApplicationObject] FOREIGN KEY([ObjectId])
REFERENCES [dbo].[ApplicationObject] ([id])
GO
ALTER TABLE [dbo].[AppGroup.Privilege] CHECK CONSTRAINT [FK_AppGroup.Privilege_ApplicationObject]
GO
ALTER TABLE [dbo].[AppGroup.Privilege]  WITH CHECK ADD  CONSTRAINT [FK_AppGroup.Privilege_ApplicationObject1] FOREIGN KEY([ObjectId])
REFERENCES [dbo].[ApplicationObject] ([id])
GO
ALTER TABLE [dbo].[AppGroup.Privilege] CHECK CONSTRAINT [FK_AppGroup.Privilege_ApplicationObject1]
GO
ALTER TABLE [dbo].[AppGroup.TeamMember]  WITH CHECK ADD  CONSTRAINT [FK_TeamMember_Group] FOREIGN KEY([GroupId])
REFERENCES [dbo].[AppGroup] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AppGroup.TeamMember] CHECK CONSTRAINT [FK_TeamMember_Group]
GO
ALTER TABLE [dbo].[Application.Member]  WITH CHECK ADD  CONSTRAINT [FK_Application.Member_Application] FOREIGN KEY([AppId])
REFERENCES [dbo].[Application] ([Id])
GO
ALTER TABLE [dbo].[Application.Member] CHECK CONSTRAINT [FK_Application.Member_Application]
GO
ALTER TABLE [dbo].[Application.Member]  WITH CHECK ADD  CONSTRAINT [FK_Application.Member_AppUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[AppUser] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Application.Member] CHECK CONSTRAINT [FK_Application.Member_AppUser]
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationRolePrivilege_ApplicationObject] FOREIGN KEY([ObjectId])
REFERENCES [dbo].[ApplicationObject] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ApplicationRolePrivilege] CHECK CONSTRAINT [FK_ApplicationRolePrivilege_ApplicationObject]
GO
ALTER TABLE [dbo].[AppUser.ExternalAccount]  WITH CHECK ADD  CONSTRAINT [FK_AppUser.ExternalSite_AppUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[AppUser.ExternalAccount] CHECK CONSTRAINT [FK_AppUser.ExternalSite_AppUser]
GO
ALTER TABLE [dbo].[AppUser.ExternalAccount]  WITH CHECK ADD  CONSTRAINT [FK_AppUser.ExternalSite_Codes.ExternalSite] FOREIGN KEY([ExternalSiteId])
REFERENCES [dbo].[Codes.ExternalSite] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AppUser.ExternalAccount] CHECK CONSTRAINT [FK_AppUser.ExternalSite_Codes.ExternalSite]
GO
ALTER TABLE [dbo].[AppUser.Profile]  WITH CHECK ADD  CONSTRAINT [FK_AppUser.Profile_AppUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[AppUser.Profile] CHECK CONSTRAINT [FK_AppUser.Profile_AppUser]
GO
ALTER TABLE [dbo].[Organization.Member]  WITH CHECK ADD  CONSTRAINT [FK_Organization.Member_Codes.OrgMemberType] FOREIGN KEY([OrgMemberTypeId])
REFERENCES [dbo].[Codes.OrgMemberType] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Organization.Member] CHECK CONSTRAINT [FK_Organization.Member_Codes.OrgMemberType]
GO
ALTER TABLE [dbo].[Organization.Member]  WITH CHECK ADD  CONSTRAINT [FK_Organization.Member_Organization] FOREIGN KEY([OrgId])
REFERENCES [dbo].[Organization] ([id])
GO
ALTER TABLE [dbo].[Organization.Member] CHECK CONSTRAINT [FK_Organization.Member_Organization]
GO
ALTER TABLE [dbo].[Organization.MemberRole]  WITH CHECK ADD  CONSTRAINT [FK_Organization.MemberRole_Codes.OrgMemberRole] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Codes.OrgMemberRole] ([Id])
GO
ALTER TABLE [dbo].[Organization.MemberRole] CHECK CONSTRAINT [FK_Organization.MemberRole_Codes.OrgMemberRole]
GO
ALTER TABLE [dbo].[Organization.MemberRole]  WITH CHECK ADD  CONSTRAINT [FK_Organization.MemberRole_Organization.Contact] FOREIGN KEY([OrgContactId])
REFERENCES [dbo].[Organization.Member] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Organization.MemberRole] CHECK CONSTRAINT [FK_Organization.MemberRole_Organization.Contact]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnHidden', @value=0 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnOrder', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnWidth', @value=615 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ApplicationObject', @level2type=N'COLUMN',@level2name=N'id'
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
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnHidden', @value=N'False' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice', @level2type=N'COLUMN',@level2name=N'Id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnOrder', @value=0 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice', @level2type=N'COLUMN',@level2name=N'Id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnWidth', @value=-1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice', @level2type=N'COLUMN',@level2name=N'Id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track user who created the org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice', @level2type=N'COLUMN',@level2name=N'CreatedBy'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track last user to update an org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice', @level2type=N'COLUMN',@level2name=N'LastUpdatedBy'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DefaultView', @value=0x02 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Filter', @value=NULL , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_OrderBy', @value=NULL , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_OrderByOn', @value=N'False' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Orientation', @value=NULL , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_RowHeight', @value=1455 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_TableMaxRecords', @value=10000 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK to Parent organization ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Organization', @level2type=N'COLUMN',@level2name=N'parentId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'True if organization is active' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Organization', @level2type=N'COLUMN',@level2name=N'IsActive'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track user who created the org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Organization', @level2type=N'COLUMN',@level2name=N'CreatedById'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track last user to update an org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Organization', @level2type=N'COLUMN',@level2name=N'LastUpdatedById'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'primary, alternate, state' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Organization.Member', @level2type=N'COLUMN',@level2name=N'OrgMemberTypeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'primary, alternate, state' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Organization.MemberRole', @level2type=N'COLUMN',@level2name=N'RoleId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK to Parent organization ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrganizationTemp', @level2type=N'COLUMN',@level2name=N'parentId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'True if organization is active' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrganizationTemp', @level2type=N'COLUMN',@level2name=N'IsActive'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track user who created the org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrganizationTemp', @level2type=N'COLUMN',@level2name=N'CreatedById'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track last user to update an org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrganizationTemp', @level2type=N'COLUMN',@level2name=N'LastUpdatedById'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'short description of sql' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SqlQuery', @level2type=N'COLUMN',@level2name=N'Description'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'for use if called from code' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SqlQuery', @level2type=N'COLUMN',@level2name=N'QueryCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track user who created the org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SqlQuery', @level2type=N'COLUMN',@level2name=N'CreatedBy'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Track last user to update an org' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SqlQuery', @level2type=N'COLUMN',@level2name=N'LastUpdatedBy'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[41] 4[20] 2[19] 3) )"
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
         Begin Table = "AppGroup"
            Begin Extent = 
               Top = 0
               Left = 393
               Bottom = 119
               Right = 565
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "AppGroup.Member"
            Begin Extent = 
               Top = 33
               Left = 655
               Bottom = 152
               Right = 833
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "AppGroup.Privilege"
            Begin Extent = 
               Top = 26
               Left = 71
               Bottom = 296
               Right = 282
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ApplicationObject"
            Begin Extent = 
               Top = 132
               Left = 385
               Bottom = 287
               Right = 554
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
         Width = 2100
         Width = 2520
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
  ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'AppObject_Group_UserPrivileges'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'       Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'AppObject_Group_UserPrivileges'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'AppObject_Group_UserPrivileges'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[45] 4[26] 2[10] 3) )"
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
         Begin Table = "Organization"
            Begin Extent = 
               Top = 3
               Left = 23
               Bottom = 152
               Right = 221
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Organization.Member"
            Begin Extent = 
               Top = 17
               Left = 290
               Bottom = 229
               Right = 575
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "LR.PatronOrgSummary"
            Begin Extent = 
               Top = 97
               Left = 672
               Bottom = 313
               Right = 993
            End
            DisplayFlags = 280
            TopColumn = 3
         End
         Begin Table = "Codes.OrgMemberType"
            Begin Extent = 
               Top = 183
               Left = 32
               Bottom = 323
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
      Begin ColumnWidths = 12
         Width = 284
         Width = 1380
         Width = 1500
         Width = 1215
         Width = 1455
         Width = 1500
         Width = 2490
         Width = 2055
         Width = 1545
         Width = 1260
         Width = 1500
         Width = 2205
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 2265
         Alias = 3780
         Table = 3285
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         G' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Organization_MemberSummary'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'roupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Organization_MemberSummary'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Organization_MemberSummary'
GO
