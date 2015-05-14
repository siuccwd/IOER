USE [Isle_IOER]
GO
/****** Object:  StoredProcedure [dbo].[aspAllDatabaseTableCounts]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- raw select without any joins - for updates
/*
exec aspAllDatabaseTableCounts null, null, 10
exec aspAllDatabaseTableCounts 'Vos_DB', 'base table', 15

exec aspAllDatabaseTableCounts 'LearningRegistryCache_Dev_20120928', 'base table', 15

*/
-- =========================================================
-- = Display row count for all tables in passed database 
-- = or all databases if @DbName is null
-- =
-- = NOTE:
-- = place in master database to make available from all databases 
-- =========================================================
CREATE PROCEDURE [dbo].[aspAllDatabaseTableCounts]
	@DbName		varchar(50) = null,
	@TypeFilter	varchar(25) = null,
	@Debug 		int = 0
   	with recompile
As
--@TypeFilter can be:
--	'Base Table'
--	View
--  NULL
declare
    @msg            char(255),
	@tableFetch_cnt	int,
	@tableRowCount		int,
	@tableNbr		int,
	@Table			varchar(75),
	@fetch_cnt		int,
	@DatabaseCnt	int,
	@DatabaseNbr	int,
	@CountSql		varchar(800),
	@SelectSql		varchar(800),
	@InsertSql		varchar(800),
	@TablesSql		varchar(800),
	@db				varchar(75),
	@AltOwner		varchar(25),
	@ServerName		varchar(50)
	,@EntityFilter nvarchar(25) 

set @DatabaseNbr = 0
set @tableNbr = 0

set @ServerName = @@Servername

set nocount on
set @AltOwner = 'dbo.'

if (@TypeFilter is null) 
	set @entityFilter = null
else begin
	set @entityFilter = lower(@TypeFilter) 
end

create table #tmpDbTables (
  Table_Name     nvarchar(128) NOT NULL
)
create table #ServerDatabaseTables (
  ServerName 	nvarchar(128) NOT NULL,
  DatabaseName 	nvarchar(128) NOT NULL,
  tableName     nvarchar(128) NOT NULL,
  tableRowCount    int         
)

IF Not EXISTS (SELECT name, type from sysobjects where type = 'U' and name = N'ServerDatabaseTables') begin 
	print 'create ServerDatabaseTables'
	create table ServerDatabaseTables (
	  ServerName 	nvarchar(128) NOT NULL,
	  DatabaseName 	nvarchar(128) NOT NULL,
	  tableName     nvarchar(128) NOT NULL,
	  tableRowCount    int         
	)
	end
declare curDatabase CURSOR FOR
	-- Get Databases

	SELECT 
		Name As DatabaseName
	FROM 
		master.dbo.sysdatabases
	WHERE 	name NOT IN 
		(
		 'master', 'model', 'tempdb', 'msdb', 
		 'distribution', 'pubs', 'northwind'
		)
		AND DATABASEPROPERTY(name, 'IsOffline') = 0 
		AND DATABASEPROPERTY(name, 'IsSuspect') = 0 
		And Name = @DbName or @DbName is null
	ORDER BY 
		DB_NAME(dbid)

open  curDatabase
fetch next from curDatabase 
		into @db

WHILE @@FETCH_STATUS = 0 Begin
	set @DatabaseNbr = @DatabaseNbr + 1
  	set @msg = str(@DatabaseNbr) + '. Database = ' + @db 	
  	if @Debug > 0 print @msg

	-- Process each table in  the database =====================================
 	truncate table #tmpDbTables	
--					+ 'WHERE lower(Table_Type) = ''base table'' AND Table_Name <> ''dtproperties'' '
-- 	set @TablesSql 	= 'Insert #tmpDbTables SELECT Table_Name FROM ' + @db + '.Information_Schema.Tables ' 
-- 					+ 'WHERE Table_Name <> ''dtproperties'' '
-- 					+ 'And (lower(Table_Type) Like ' + @entityFilter + ' or ' + @entityFilter + 'is null)'
	set @TablesSql 	= 'SELECT Table_Name FROM ' + @db + '.Information_Schema.Tables ' 
					+ 'WHERE Table_Name <> ''dtproperties'' '

	if (@EntityFilter is not null) Begin
		set @TablesSql = @TablesSql 
					+ 'And (lower(Table_Type) = ''' + @entityFilter + ''')'
	End

  	if @Debug > 10 Begin
		print @TablesSql
		exec(@TablesSql)			
	End

	-- Add the insert statement
	set @TablesSql = 'Insert #tmpDbTables ' + @TablesSql 

	exec(@TablesSql)	

	declare curLocal CURSOR FOR
		-- Get tables
		SELECT Table_Name 
		FROM 
			#tmpDbTables

	open  curLocal
	fetch next from curLocal 
			into @Table

	WHILE @@FETCH_STATUS = 0 Begin
		set @tableNbr = @tableNbr + 1
	  	set @msg = '    ' + str(@tableNbr) + '. Table = ' + @Table 	
	  	if @Debug > 0 print @msg	

		if 1 = 1 begin
			set @InsertSql = 'Insert #ServerDatabaseTables Select ''' + @ServerName + ''', ''' + @db + ''' As DatabaseName,  ''' + @Table + ''' As TableName, count(*) As [' + @Table + '_Count] from ' + @db + '.' + @AltOwner + '[' + @Table + ']'
			if (@Debug > 9) 
				print @InsertSql				
			exec(@InsertSql)
			end
		else begin
			set @CountSql = 'Select ''' + @db + ''' As DatabaseName,  ''' + @Table + ''' As TableName, count(*) As [' + @Table + '_Count] from ' + @AltOwner + @Table 
		--	set @CountSql = 'Select ' + @Table + ' As TableName, @tableRowCount = count(*)  from ' + @Table 
			
		-- 	select @tableRowCount = exec(@CountSql)
			exec(@CountSql)
		end
	
		/* Get next row                                    */
		fetch next from curLocal 
			into @Table
	
	   	set @fetch_cnt = @@rowcount
	
	end /* while */
	
	close curLocal
	deallocate curLocal

	-- Now display counts
--	select * from #ServerDatabaseTables order by tableName
	
	-- drop temp table
-- 	truncate table #ServerDatabaseTables


	-- =========================================================================
	/* Get next database                                    */
	fetch next from curDatabase 
		into @db

   	set @fetch_cnt = @@rowcount

end /* while */

close curDatabase
deallocate curDatabase

set nocount off
-- Now display counts
	if @DbName is null begin
		Delete ServerDatabaseTables Where ServerName = @ServerName
		Insert ServerDatabaseTables select * from #ServerDatabaseTables order by ServerName, DatabaseName, tableName
		End
	Else Begin
		Delete ServerDatabaseTables Where ServerName = @ServerName And  DatabaseName = @DbName
		Insert ServerDatabaseTables select * from #ServerDatabaseTables order by ServerName, DatabaseName, tableName
		End

	select * from #ServerDatabaseTables order by ServerName, DatabaseName, tableName

-- drop temp table
drop table #ServerDatabaseTables
drop table #tmpDbTables
-- drop temp table
-- drop table #tmpDbCnt


GO
/****** Object:  StoredProcedure [dbo].[aspCreateProcedure_Delete]    Script Date: 2/22/2015 10:28:07 AM ******/
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
CREATE  Procedure [dbo].[aspCreateProcedure_Delete]
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
/****** Object:  StoredProcedure [dbo].[aspCreateProcedure_Get]    Script Date: 2/22/2015 10:28:07 AM ******/
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
CREATE  Procedure [dbo].[aspCreateProcedure_Get] 
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
/****** Object:  StoredProcedure [dbo].[aspCreateProcedure_GetSingle]    Script Date: 2/22/2015 10:28:07 AM ******/
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
CREATE   Procedure [dbo].[aspCreateProcedure_GetSingle] 
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
/****** Object:  StoredProcedure [dbo].[aspCreateProcedure_Insert]    Script Date: 2/22/2015 10:28:07 AM ******/
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
CREATE   Procedure [dbo].[aspCreateProcedure_Insert]
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
/****** Object:  StoredProcedure [dbo].[aspCreateProcedure_Update]    Script Date: 2/22/2015 10:28:07 AM ******/
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
CREATE  Procedure [dbo].[aspCreateProcedure_Update] 
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
/****** Object:  StoredProcedure [dbo].[aspCreateProcedures]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

exec aspCreateProcedures 1,1,1,1,1,'|Resource.GroupType|',1,'public'

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

CREATE   PROCEDURE [dbo].[aspCreateProcedures] 
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
/****** Object:  StoredProcedure [dbo].[aspGenerateColumnDef]    Script Date: 2/22/2015 10:28:07 AM ******/
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
CREATE     PROCEDURE [dbo].[aspGenerateColumnDef]
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
/****** Object:  StoredProcedure [dbo].[aspGenerateDBDictionary]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:JOHIR
-- Create date: 01/12/2012
-- Description:	GENERATE DATA DICTIONARY FROM SQL SERVER
-- =============================================
CREATE proc [dbo].[aspGenerateDBDictionary] 
AS
BEGIN

select a.name [Table],b.name [Attribute],c.name [DataType],b.isnullable [Allow Nulls?],CASE WHEN 
d.name is null THEN 0 ELSE 1 END [PKey?],
CASE WHEN e.parent_object_id is null THEN 0 ELSE 1 END [FKey?],CASE WHEN e.parent_object_id 
is null THEN '-' ELSE g.name  END [Ref Table],
CASE WHEN h.value is null THEN '-' ELSE h.value END [Description]
from sysobjects as a
join syscolumns as b on a.id = b.id
join systypes as c on b.xtype = c.xtype 
left join (SELECT  so.id,sc.colid,sc.name 
      FROM    syscolumns sc
      JOIN sysobjects so ON so.id = sc.id
      JOIN sysindexkeys si ON so.id = si.id 
                    and sc.colid = si.colid
      WHERE si.indid = 1) d on a.id = d.id and b.colid = d.colid
left join sys.foreign_key_columns as e on a.id = e.parent_object_id and b.colid = e.parent_column_id    
left join sys.objects as g on e.referenced_object_id = g.object_id  
left join sys.extended_properties as h on a.id = h.major_id and b.colid = h.minor_id
where a.type = 'U' order by a.name

END
GO
/****** Object:  StoredProcedure [dbo].[AuditReport.Detail_Insert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 7/2/2012
-- Description:	Create a detail record for the audit report
-- =============================================
CREATE PROCEDURE [dbo].[AuditReport.Detail_Insert]
	@ReportId int, 
	@FileName varchar(100), 
	@DocID varchar(100),
	@URI varchar(500), 
	@MessageType char(1), 
	@MessageRouting varchar(2),
	@Message varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO [AuditReport.Detail] (ReportId, [Filename], DocID, URI, MessageType,
		MessageRouting, Message)
	VALUES (@ReportId, @FileName, @DocID, @URI, @MessageType, @MessageRouting, @Message)
END


GO
/****** Object:  StoredProcedure [dbo].[AuditReport_Insert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 7/2/2012
-- Description:	Create an Audit Report record
-- =============================================
CREATE PROCEDURE [dbo].[AuditReport_Insert]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO AuditReport (Created)
	VALUES (getdate())

	SELECT @@IDENTITY AS ReportId
END

GO
/****** Object:  StoredProcedure [dbo].[Blacklist.HostsGetByHost]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:	    Jerome Grimmer
-- Create date: 3/11/2014
-- Description:	Get Blacklisted host by host name
-- =============================================
CREATE PROCEDURE [dbo].[Blacklist.HostsGetByHost]
	@Hostname varchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, Hostname, RecordSource, Created, CreatedById, LastUpdated, LastUpdatedId
	FROM [Blacklist.Hosts]
	WHERE Hostname = @Hostname
END

GO
/****** Object:  StoredProcedure [dbo].[CareerClusterSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
CareerClusterSelect 0, ''

CareerClusterSelect 11, ''

*/
-- =============================================
-- Author:		Michael Parsons
-- get list of IL Pathways career clusters
-- Modifications
-- 
-- =============================================
CREATE PROCEDURE [dbo].[CareerClusterSelect]
	@ClusterId		      int,
  @IlPathwayChannel  varchar(50)
AS
BEGIN
	SET NOCOUNT ON;
  If @ClusterId = 0				      SET @ClusterId = null
  If @IlPathwayChannel = ''			SET @IlPathwayChannel = null
  SELECT 
      [Id]
      ,IlPathwayName As Title
      ,IlPathwayName + ' (' + convert(varchar, isnull(WareHouseTotal,0)) + ')' As FormattedTitle
      ,[prefix], [Description]
      ,IlPathwayChannel

    FROM [CareerCluster]
  Where 
  (IsActive = 1 and IsIlPathway = 1)
  And (Id = @ClusterId		or @ClusterId is null)
  And (IlPathwayChannel = @IlPathwayChannel		or @IlPathwayChannel is null)
  Order by IlPathwayName
END


GO
/****** Object:  StoredProcedure [dbo].[Codes.AccessibilityApiSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	Select rows from Codes.AccessibilityApi
-- =============================================
CREATE PROCEDURE [dbo].[Codes.AccessibilityApiSelect] 
	@Filter varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Sql varchar(1000)
	SET @Sql = 'SELECT Id, Title, Description, IsActive, WarehouseTotal, schemaValue
FROM [Codes.AccessibilityApi]
'+@Filter
	PRINT @Sql
	EXEC(@Sql)
END

GO
/****** Object:  StoredProcedure [dbo].[Codes.AccessibilityControlGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	Get row from Codes.AccessibilityControl
-- =============================================
CREATE PROCEDURE [dbo].[Codes.AccessibilityControlGet]
	@Id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, Title, Description, IsActive, WarehouseTotal, schemaValue
	FROM [Codes.AccessibilityControl]
	WHERE Id = @Id
END

GO
/****** Object:  StoredProcedure [dbo].[Codes.AccessibilityControlSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	Select rows from Codes.AccessibilityControl
-- =============================================
CREATE PROCEDURE [dbo].[Codes.AccessibilityControlSelect] 
	@Filter varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Sql varchar(1000)
	SET @Sql = 'SELECT Id, Title, Description, IsActive, WarehouseTotal, schemaValue
FROM [Codes.AccessibilityControl]
'+@Filter
	PRINT @Sql
	EXEC(@Sql)
END

GO
/****** Object:  StoredProcedure [dbo].[Codes.AccessibilityFeatureGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	Get row from Codes.AccessibilityFeature
-- =============================================
CREATE PROCEDURE [dbo].[Codes.AccessibilityFeatureGet]
	@Id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, Title, Description, IsActive, WarehouseTotal, schemaValue
	FROM [Codes.AccessibilityFeature]
	WHERE Id = @Id
END

GO
/****** Object:  StoredProcedure [dbo].[Codes.AccessibilityFeatureSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	Select rows from Codes.AccessibilityFeature
-- =============================================
CREATE PROCEDURE [dbo].[Codes.AccessibilityFeatureSelect] 
	@Filter varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Sql varchar(1000)
	SET @Sql = 'SELECT Id, Title, Description, IsActive, WarehouseTotal, schemaValue
FROM [Codes.AccessibilityFeature]
'+@Filter
	PRINT @Sql
	EXEC(@Sql)
END

GO
/****** Object:  StoredProcedure [dbo].[Codes.AccessibilityHazardGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	Get row from Codes.AccessibilityHazard
-- =============================================
CREATE PROCEDURE [dbo].[Codes.AccessibilityHazardGet]
	@Id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, Title, Description, IsActive, WarehouseTotal, AntonymId, schemaValue
	FROM [Codes.AccessibilityHazard]
	WHERE Id = @Id
END

GO
/****** Object:  StoredProcedure [dbo].[Codes.AccessibilityHazardSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	Select rows from Codes.AccessibilityHazard
-- =============================================
CREATE PROCEDURE [dbo].[Codes.AccessibilityHazardSelect] 
	@Filter varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Sql varchar(1000)
	SET @Sql = 'SELECT Id, Title, Description, IsActive, WarehouseTotal, AntonymId, schemaValue
FROM [Codes.AccessibilityHazard]
'+@Filter
	PRINT @Sql
	EXEC(@Sql)
END

GO
/****** Object:  StoredProcedure [dbo].[Codes.GradeLevelGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Codes.GradeLevelGet] 1, '', '', ''

[Codes.GradeLevelGet] 0, '7', '', ''

[Codes.GradeLevelGet] 0, '', 'Grade 7', ''

[Codes.GradeLevelGet] 0, '', '', 'http://purl.org/ASN/scheme/ASNEducationLevel/K'

MODIFICATIONS:
  2013-05-23 jgrimmer - Added IsEducationBand, FromAge, ToAge
*/
CREATE PROCEDURE [dbo].[Codes.GradeLevelGet]
    @Id     int,
    @Title  varchar(50), 
    @Description varchar(100),
    @AlignmentUrl varchar(150)
As
if @Id= 0 set @Id = NULL
if @Title = '' set @Title = NULL
if @Description = '' set @Description = NULL
if @AlignmentUrl = '' set @AlignmentUrl = NULL

SELECT     Id, 
    Title, 
    AgeLevel, 
    Description, 
    IsPathwaysLevel, 
    AlignmentUrl, 
    SortOrder, 
    WarehouseTotal, 
    GradeRange, 
    GradeGroup, 
    IsActive, 
    PathwaysEducationLevelId,
    IsEducationBand,
    FromAge,
    ToAge
FROM [Codes.GradeLevel]
WHERE 
    (Id = @Id OR @Id is null)
AND (Title = @Title OR @Title is null)
AND (Description = @Description OR @Description is null)
AND (AlignmentUrl = @AlignmentUrl OR @AlignmentUrl is null)

GO
/****** Object:  StoredProcedure [dbo].[Codes.GradeLevelSelectByAgeRange]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 5/22/2013
-- Description:	Gets Grade Levels by Age Range
--
-- 2013-11-14 jgrimmer - fix bug where grade levels were being returned as grade bands
-- =============================================
CREATE PROCEDURE [dbo].[Codes.GradeLevelSelectByAgeRange]
	@MinAge int,
	@MaxAge int,
	@IsEducationBand bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @IsEducationBand = 'True' BEGIN
		SELECT Id, Title, AgeLevel, [Description], IsPathwaysLevel, AlignmentUrl, SortOrder,
			WarehouseTotal, GradeRange, GradeGroup, IsActive, IsEducationBand, FromAge, ToAge
		FROM [Codes.GradeLevel]
		WHERE (NOT(@MinAge > ToAge OR @MaxAge < FromAge)) AND IsEducationBand = 'True'
	END ELSE BEGIN
		SELECT Id, Title, AgeLevel, Description, IsPathwaysLevel, AlignmentUrl, SortOrder,
			WarehouseTotal, GradeRange, GradeGroup, IsActive, IsEducationBand, FromAge, ToAge
		FROM [Codes.GradeLevel]
		WHERE (AgeLevel BETWEEN @MinAge AND @MaxAge AND IsEducationBand = 'False')
	END
	/*
	WHERE ((AgeLevel BETWEEN @MinAge AND @MaxAge) AND @IsEducationBand = 'False' AND IsEducationBand = 'False') OR
		(NOT (@MinAge > ToAge AND @MaxAge < FromAge)) AND
		IsEducationBand '= @IsEducationBand)*/
END

GO
/****** Object:  StoredProcedure [dbo].[Codes.LanguageSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Create date: 7/02/2012
-- Description:	Select [Codes.Language]
-- =============================================
CREATE PROCEDURE [dbo].[Codes.LanguageSelect]

AS
BEGIN
SELECT [Id]
      ,[Title]
      ,[Title] + ' (' + isnull(convert(varchar,WareHouseTotal),0) + ')' As FormattedTitle
      ,isnull(WareHouseTotal,0) As WareHouseTotal
      ,WareHouseTotal As ItemCount
      ,[IsActive]
      ,[IsPathwaysLanguage]
  FROM [dbo].[Codes.Language]
  where [IsPathwaysLanguage]= 1
  order by Id
END


GO
/****** Object:  StoredProcedure [dbo].[Codes.RatingTypeGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/22/2012
-- Description:	Get RatingType
-- =============================================
CREATE PROCEDURE [dbo].[Codes.RatingTypeGet]
	@Id int,
	@Type varchar(50),
	@Identifier varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @Id = 0 SET @Id = NULL
	
	SELECT Id, [Type], Identifier, [Description], Created
	FROM [Codes.RatingType]
	WHERE (Id = @Id OR @Id IS NULL) AND
		([Type] = @Type OR @Type IS NULL) AND
		(Identifier = @Identifier OR @Identifier IS NULL)
END

GO
/****** Object:  StoredProcedure [dbo].[Codes.RatingTypeInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/22/2012
-- Description:	Create RatingType record
-- =============================================
CREATE PROCEDURE [dbo].[Codes.RatingTypeInsert]
	@Type varchar(50),
	@Identifier varchar(500),
	@Description varchar(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [Codes.RatingType] ([Type], Identifier, [Description], Created)
	VALUES (@Type, @Identifier, @Description, GETDATE())
	
	SELECT @@IDENTITY AS Id
END

GO
/****** Object:  StoredProcedure [dbo].[Codes.ResPropertyType_Lookup]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 09/04/2012
-- Description:	Lookup a Resource Property Type by name
-- =============================================
CREATE PROCEDURE [dbo].[Codes.ResPropertyType_Lookup]
	@Title varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT Id, Title
    FROM [Codes.ResPropertyType]
    WHERE Title = @Title
END

GO
/****** Object:  StoredProcedure [dbo].[Codes_EducationLevel_Select]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Create date: 7/02/2012
-- Description:	Select [Codes.EducationLevel]
-- =============================================
CREATE PROCEDURE [dbo].[Codes_EducationLevel_Select]

AS
BEGIN
SELECT [Id]
      ,[Title]
      ,[Title] + ' (' + isnull(convert(varchar,WareHouseTotal),0) + ')' As FormattedTitle
      ,isnull(WareHouseTotal,0) As WareHouseTotal
      ,[Description]
      ,[IsPathwaysLevel]
  FROM [dbo].[Codes.PathwaysEducationLevel]
  where [IsPathwaysLevel]= 1
  order by SortOrder
END


GO
/****** Object:  StoredProcedure [dbo].[Codes_ResourceType_Select]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Create date: 6/16/2012
-- Description:	Select one or more name/value pairs for a resource
-- =============================================
Create PROCEDURE [dbo].[Codes_ResourceType_Select]

AS
BEGIN
SELECT [Id]
      ,[Title]

  FROM [dbo].[Codes.ResourceType]
  order by       
  [SortOrder], [Title]


END

GO
/****** Object:  StoredProcedure [dbo].[CodeTable_Select]    Script Date: 2/22/2015 10:28:07 AM ******/
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
/****** Object:  StoredProcedure [dbo].[CodeTables_UpdatePublisherTotals]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
Exec CodeTables_UpdatePublisherTotals 10
*/
-- =========================================================================
-- 13/03/04 mparsons - was taking too long, so split up
-- =========================================================================
CREATE  Procedure [dbo].[CodeTables_UpdatePublisherTotals]
    @debugLevel int = 0

AS
--declare @debugLevel int
-- ==========================================================
print 'updating Publishers ...'
-- updates -----------------------------------
UPDATE [dbo].[PublisherSummary]
   SET [ResourceTotal] = totals.ResourcesCount
   ,IsActive= 1
from [dbo].[PublisherSummary] ps
inner join (
 SELECT base.[Publisher], count(*) As ResourcesCount
  FROM [dbo].[Resource.Version_Summary] base
  inner join [dbo].[PublisherSummary] existing on base.[Publisher] = existing.[Publisher]
  group by base.[Publisher]
 -- Order by 2 desc
  ) totals on ps.Publisher = totals.Publisher
print 'Updated = ' + convert(varchar, isnull(@@rowcount,0))


-- inserts -----------------------------------
INSERT INTO [dbo].[PublisherSummary]
           ([Publisher],[IsActive],[ResourceTotal])
SELECT base.[Publisher], 1,count(*) As Resources
  FROM [dbo].[Resource.Version_Summary] base
  left join [dbo].[PublisherSummary] existing on base.[Publisher] = existing.[Publisher]
  where 
		base.[Publisher] is not null 
  AND existing.[Publisher] is null
  group by base.[Publisher]
  order by 3 desc
  
print 'Insert = ' + convert(varchar, isnull(@@rowcount,0))  
-- ============================================================================
--handle delete of publishers, or mark of inactive 
--this can only be done if all related resources are inactive 
/*

UPDATE [dbo].[PublisherSummary]
   SET IsActive = 0
--    select count(*)
from [dbo].[PublisherSummary] ps
inner join (
 SELECT distinct rv.[Publisher], lr.IsActive
 --, lr.ResourceUrl
  FROM [dbo].[Resource.Version] rv  
  Inner JOIN dbo.[Resource] lr ON lr.Id = rv.ResourceIntId
  inner join [dbo].[PublisherSummary] existing on rv.[Publisher] = existing.[Publisher]
  Where existing.IsActive = 1 AND lr.IsActive = 0

  ) totals on ps.Publisher = totals.Publisher
  
  
Inner join [dbo].[Resource.Version] rv  ON ps.Publisher = rv.Publisher
Inner JOIN dbo.[Resource] lr ON lr.Id = rv.ResourceIntId
Where ps.IsActive = 1 AND lr.IsActive = 0

-- ============== OR maybe this:
SELECT ps.[Publisher]
--, isnull(ActiveResources.ActiveTotals, 0) As ActiveTotals
FROM [PublisherSummary]  ps
  left Join 
  (
 SELECT rv.[Publisher], isnull(count(*),0) As ActiveTotals
 --, lr.ResourceUrl
  FROM [dbo].[Resource.Version] rv  
  Inner JOIN dbo.[Resource] lr ON lr.Id = rv.ResourceIntId
  Where 
  lr.Isactive = 1

  --lr.ResourceUrl like '%actionbioscience.org%'
  --AND lr.IsActive = 1
  group by rv.[Publisher]
  --order by 2 
  ) As ActiveResources on ps.[Publisher] = ActiveResources.[Publisher]
  where ActiveResources.ActiveTotals = 0


print 'Set inactive = ' + convert(varchar, isnull(@@rowcount,0)) 
*/

-- ==========================================================



  

GO
/****** Object:  StoredProcedure [dbo].[CodeTables_UpdateWarehouseTotals]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
UPDATE [dbo].[Resource.Subject]
   SET [CodeId] = code.Id
--		select [ResourceIntId] ,[Subject], code.Id
from [Resource.Subject] base
inner join [Codes.subject] code on base.Subject = code.Title
 

*/


/*
Exec CodeTables_UpdateWarehouseTotals 5
*/
-- =========================================================================
-- 13/03/04 mparsons - was taking too long, so split up
-- 14/05/28 mparsons - added Codes.TagValue. Leaving others for now
-- =========================================================================
CREATE  Procedure [dbo].[CodeTables_UpdateWarehouseTotals]
    @debugLevel int = 0

AS
--declare @debugLevel int
-- ==========================================================
print 'updating all Codes.TagValue ...'
UPDATE [dbo].[Codes.TagValue]
   SET [WarehouseTotal] = base.WarehouseTotal
from [Codes.TagValue] codes      
Inner join ( 
SELECT [TagValueId],count(*) AS wareHouseTotal
  FROM [dbo].[Resource.Tag] base
  inner join [Resource] rvs on base.ResourceIntId = rvs.Id
  where rvs.IsActive = 1
  group by [TagValueId]
    ) base on codes.Id = base.[TagValueId]
-- ==========================================================
print 'updating StandardBody.Node ...'
UPDATE [dbo].[StandardBody.Node]
   SET [WarehouseTotal] = base.WarehouseTotal
-- select codes.Title, base.WarehouseTotal      
FROM [dbo].[StandardBody.Node] codes
Inner join ( 
SELECT StandardId ,count(*) AS wareHouseTotal
  FROM [dbo].[Resource.Standard] base
  inner join [Resource.Version_Summary] rvs on base.ResourceIntId = rvs.ResourceIntId
    group by StandardId
    ) base on codes.Id = base.StandardId
print 'Updated = ' + convert(varchar, isnull(@@rowcount,0)) 

-- *** TODO - remove the follwing after convertion ***
-- ==========================================================
print 'updating Codes.ResourceType ...'
UPDATE [dbo].[Codes.ResourceType]
   SET [WarehouseTotal] = base.WarehouseTotal
-- select codes.Title, base.WarehouseTotal      
FROM [dbo].[Codes.ResourceType] codes
Inner join ( 
SELECT [ResourceTypeId] ,count(*) AS wareHouseTotal
  FROM [dbo].[Resource.ResourceType] base
  inner join [Resource.Version_Summary] rvs on base.ResourceIntId = rvs.ResourceIntId
    group by [ResourceTypeId]
    ) base on codes.Id = base.[ResourceTypeId]
print 'Updated = ' + convert(varchar, isnull(@@rowcount,0)) 

-- ==========================================================
print 'updating [Codes.ResourceTypeCategory] ...'
UPDATE [dbo].[Codes.ResourceTypeCategory]
   SET [WarehouseTotal] = base.WarehouseTotal
-- select codes.[Category], base.WarehouseTotal      
FROM [dbo].[Codes.ResourceTypeCategory] codes
Inner join ( 
SELECT [CategoryId] ,sum(wareHouseTotal) As wareHouseTotal
  FROM [dbo].[Codes.ResourceType]
    group by [CategoryId]
    ) base on codes.Id = base.[CategoryId]
print 'Updated = ' + convert(varchar, isnull(@@rowcount,0)) 

-- ==========================================================
print 'updating Codes.GradeLevel ...'
UPDATE [dbo].[Codes.GradeLevel]
SET [WarehouseTotal] = base.WarehouseTotal
-- select codes.Title, base.WarehouseTotal  
FROM [dbo].[Codes.GradeLevel] codes
Inner join ( 
SELECT [GradeLevelId],count(*) AS wareHouseTotal
  FROM [dbo].[Resource.GradeLevel] base
  inner join [Resource.Version_Summary] rvs on base.ResourceIntId = rvs.ResourceIntId
  group by [GradeLevelId]
    ) base on codes.Id = base.[GradeLevelId]

print 'Updated = ' + convert(varchar, isnull(@@rowcount,0))
if @debugLevel > 8  begin  
  select * from [dbo].[Codes.GradeLevel]
  end
-- ==========================================================
--print 'updating Codes.PathwaysEducationLevel ...'
--UPDATE [dbo].[Codes.PathwaysEducationLevel]
--SET [WarehouseTotal] = base.WarehouseTotal
---- select codes.Title, base.WarehouseTotal  
--FROM [dbo].[Codes.PathwaysEducationLevel] codes
--Inner join ( 
--SELECT [PathwaysEducationLevelId],count(*) AS wareHouseTotal
--  FROM [dbo].[Resource.EducationLevel] base
--  inner join [Resource.Version_Summary] rvs on base.ResourceIntId = rvs.ResourceIntId
--  group by [PathwaysEducationLevelId]
--    ) base on codes.Id = base.PathwaysEducationLevelId

--print 'Updated = ' + convert(varchar, isnull(@@rowcount,0))
--if @debugLevel > 8  begin  
--  select * from [dbo].[Codes.PathwaysEducationLevel]
--  end  
-- ==========================================================
print 'updating Codes.IntendedAudience ...'
UPDATE [dbo].[Codes.AudienceType]
SET [WarehouseTotal] = base.WarehouseTotal
-- select codes.Title, base.WarehouseTotal  
FROM [dbo].[Codes.AudienceType] codes
Inner join ( 
SELECT [AudienceId],count(*) AS wareHouseTotal
  FROM [dbo].[Resource.IntendedAudience] base
  inner join [Resource.Version_Summary] rvs on base.ResourceIntId = rvs.ResourceIntId
  group by [AudienceId]
    ) base on codes.Id = base.[AudienceId]

print 'Updated = ' + convert(varchar, isnull(@@rowcount,0))
-- ==========================================================
print 'updating Codes.Language  ...'
UPDATE [dbo].[Codes.Language]
SET [WarehouseTotal] = base.WarehouseTotal
-- select codes.Title, base.WarehouseTotal  
FROM [dbo].[Codes.Language] codes
Inner join ( 
SELECT [LanguageId],count(*) AS wareHouseTotal
  FROM [dbo].[Resource.Language] base
  inner join [Resource.Version_Summary] rvs on base.ResourceIntId = rvs.ResourceIntId
  group by [LanguageId]
    ) base on codes.Id = base.[LanguageId]

print 'Updated = ' + convert(varchar, isnull(@@rowcount,0))
    
-- ==========================================================
print 'updating Codes.ResourceFormat ...'
UPDATE [dbo].[Codes.ResourceFormat]
SET [WarehouseTotal] = base.WarehouseTotal
-- select codes.Title, base.WarehouseTotal  
FROM [dbo].[Codes.ResourceFormat] codes
Inner join ( 
SELECT [CodeId],count(*) AS wareHouseTotal
  FROM [dbo].[Resource.Format] base
  inner join [Resource.Version_Summary] rvs on base.ResourceIntId = rvs.ResourceIntId
  group by [CodeId]
    ) base on codes.Id = base.[CodeId]

print 'Updated = ' + convert(varchar, isnull(@@rowcount,0))
/*    
SELECT [Value],count(*) AS wareHouseTotal
  FROM [dbo].[Resource.Property]
    where [PropertyTypeId]= 3
      group by [Value]

SELECT [Id]
      ,[Title]
      ,[WarehouseTotal]
  FROM [.[dbo].[Codes.ResourceFormat]
GO


*/
-- ==========================================================
print 'updating CareerClusters ...'
UPDATE [dbo].CareerCluster
SET [WarehouseTotal] = base.WarehouseTotal
-- select codes.IlPathwayName, base.WarehouseTotal  
FROM [dbo].CareerCluster codes
Inner join ( 
SELECT ClusterId,count(*) AS WarehouseTotal
  FROM [dbo].[Resource.Cluster] base
  inner join [Resource.Version_Summary] rvs on base.ResourceIntId = rvs.ResourceIntId
  group by ClusterId
    ) base on codes.Id = base.ClusterId

print 'Updated = ' + convert(varchar, isnull(@@rowcount,0))
-- ==========================================================
print 'updating Access rights ...'
-- updates
UPDATE [dbo].[Codes.AccessRights]
   SET [WarehouseTotal] = base.WarehouseTotal
-- select codes.Title, base.WarehouseTotal      
FROM [dbo].[Codes.AccessRights] codes
inner join (
 SELECT AccessRightsId, count(*) As WarehouseTotal
  FROM [dbo].[Resource.Version_Summary] base
  group by base.AccessRightsId
  --Order by 2 desc
  ) base on codes.Id = base.AccessRightsId
print 'Updated = ' + convert(varchar, isnull(@@rowcount,0))

-- ======
--OLD via text
--print 'updating Access rights ...'
---- updates
--UPDATE [dbo].[Codes.AccessRights]
--   SET [WarehouseTotal] = base.WarehouseTotal
---- select codes.Title, base.WarehouseTotal      
--FROM [dbo].[Codes.AccessRights] codes
--inner join (
-- SELECT isnull(base.AccessRights,'Unknown') As AccessRights, count(*) As WarehouseTotal
--  FROM [dbo].[Resource.Version_Summary] base
--  group by base.AccessRights
--  --Order by 2 desc
--  ) base on codes.Title = base.AccessRights
--print 'Updated = ' + convert(varchar, isnull(@@rowcount,0))
-- ==========================================================
print 'updating Codes.EducationUse ...'
UPDATE [dbo].[Codes.EducationalUse]
   SET [WarehouseTotal] = base.WarehouseTotal
-- select codes.Title, base.WarehouseTotal      
FROM [dbo].[Codes.EducationalUse] codes
Inner join ( 
SELECT EducationUseId ,count(*) AS wareHouseTotal
  FROM [dbo].[Resource.EducationUse] base
  inner join [Resource.Version_Summary] rvs on base.ResourceIntId = rvs.ResourceIntId
    group by EducationUseId
    ) base on codes.Id = base.EducationUseId
print 'Updated = ' + convert(varchar, isnull(@@rowcount,0)) 
-- ==========================================================
print 'updating Subjects ...'
UPDATE [dbo].[Codes.Subject]
SET [WarehouseTotal] = base.WarehouseTotal
-- select codes.Title, base.WarehouseTotal  
FROM [dbo].[Codes.Subject] codes
Inner join ( 
SELECT [CodeId],count(*) AS wareHouseTotal
  FROM [dbo].[Resource.Subject] base
  inner join [Resource.Version_Summary] rvs on base.ResourceIntId = rvs.ResourceIntId
  group by [CodeId]
    ) base on codes.Id = base.[CodeId]

print 'Updated = ' + convert(varchar, isnull(@@rowcount,0))

-- ==========================================================


GO
/****** Object:  StoredProcedure [dbo].[ConditionsOfUse_Select]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ConditionsOfUse_Select]
As
SELECT	[Id],
		[Summary] AS [Title],
		[Title] AS [Description],
		[Url],
		[IconUrl]

FROM [ConditionOfUse]

WHERE     (IsActive = 1)
ORDER BY SortOrderAuthoring


GO
/****** Object:  StoredProcedure [dbo].[ConvertGradesToK12Ages]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 4/1/2013
-- Description:	Convert Grade Levels to Age Ranges for K-12
-- =============================================
CREATE PROCEDURE [dbo].[ConvertGradesToK12Ages]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [Resource.AgeRange] (ResourceIntId, FromAge, ToAge)
	SELECT ResourceIntId, min(MinAge) AS MinAge, max(MaxAge) AS MaxAge
	FROM [Resource.EducationLevel]
	INNER JOIN [GradeToAgeRange] ON [Resource.EducationLevel].OriginalLevel = [GradeToAgeRange].GradeLevel
	GROUP BY ResourceIntId
	ORDER BY ResourceIntId
END

GO
/****** Object:  StoredProcedure [dbo].[CreatorSearch]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

SELECT [Creator], count(*) As ResourceTotal
      
  FROM [dbo].[Resource.Version] base
  inner join [Resource] res on base.ResourceIntId = res.Id
Where base.isActive = 1
group by Creator

  
-- ==========================================================================

DECLARE @RC int,@SortOrder varchar(100),@Filter varchar(5000)
DECLARE @StartPageIndex int, @PageSize int, @totalRows int
--
set @SortOrder = '[Creator] desc'

-- blind search 
set @Filter = ''
set @Filter = ' rv.creator like ''illinois%'' ' 
set @Filter = ' rv.creator like ''c%'' ' 

set @Filter = ''
set @StartPageIndex = 1
set @PageSize = 100

EXECUTE @RC = CreatorSearch
     @Filter,@SortOrder  ,@StartPageIndex  ,@PageSize  ,@totalRows OUTPUT

select 'total rows = ' + convert(varchar,@totalRows)



*/


CREATE PROCEDURE [dbo].[CreatorSearch]
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
      ,@lastRow int
      ,@minRows int
      ,@debugLevel      int
      ,@SQL             varchar(5000)
      ,@Sql2            varchar(200)
      ,@DefaultFilter   varchar(1000)
      ,@OrderBy         varchar(100)
-- =================================

Set @debugLevel = 4

Set @DefaultFilter= ' (IsActive = 1) '
if len(@SortOrder) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by Creator '
--===================================================
-- Calculate the range
--===================================================      
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1
SET @lastRow =  @StartPageIndex + @PageSize

-- =================================

  if len(isnull(@Filter,'')) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
        
     set @Filter =  @Filter + ' AND ' + @DefaultFilter
     end
  else begin
    set @Filter =     ' where rv.Creator is not null AND ' + @DefaultFilter
    end

  print '@Filter len: '  +  convert(varchar,len(@Filter))

  set @SQL = 'SELECT Distinct  pub.[Creator], ResourceTotal FROM
   (SELECT  ROW_NUMBER() OVER('+ @OrderBy + ') as RowNumber,
      [Creator], count(*) As ResourceTotal
  FROM [dbo].[Resource.Version] rv ' 
        + @Filter + ' group by [Creator]  
   ) as DerivedTableName
       Inner join [dbo].[Resource.Version] pub on DerivedTableName.[Creator] = pub.[Creator]
WHERE RowNumber BETWEEN ' + convert(varchar,@StartPageIndex) + ' AND ' + convert(varchar,@lastRow) + ' ' 
+ @OrderBy

  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL
  exec (@SQL)
  
  --===============================
  DECLARE @TempItems TABLE
(
   RowsFound int
)


set @Sql2= 'SELECT distinct  count(Creator) FROM [dbo].[Resource.Version] rv '
        + @Filter + ' '

 print @Sql2
 INSERT INTO @TempItems (RowsFound)
  exec (@Sql2)
  
  select @TotalRows= RowsFound from @TempItems

GO
/****** Object:  StoredProcedure [dbo].[EmailNoticeDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

--- Delete Procedure for EmailNotice---
CREATE PROCEDURE [dbo].[EmailNoticeDelete]
        @id int
As
DELETE FROM EmailNotice
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[EmailNoticeGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Get Single Procedure for EmailNotice---
--if exists (select * from dbo.sysobjects where id = object_id(N'[EmailNoticeGet]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
--Drop Procedure EmailNoticeGet
--Go
/*
exec EmailNoticeGet null, 'CourseLogInInstructions'
exec EmailNoticeGet 5, ''
*/
CREATE PROCEDURE [dbo].[EmailNoticeGet]
    @id						int,
		@NoticeCode		varchar(50) = null,
		@LangCode			varchar(10) = null
As
  If @id = 0						SET @id = NULL 
  If @NoticeCode = ''		SET @NoticeCode = NULL 
  If @LangCode = ''			SET @LangCode = NULL 

SELECT     id, 
    Title,
    NoticeCode, 
		Category,
    Description, 
    Filter, 
    IsActive, 
    FromEmail, 
    CcEmail, 
    BccEmail, 
		LanguageCode,
    Subject, 
    HtmlBody, 
    TextBody, 
    Created, 
    CreatedBy, 
    LastUpdated, 
    LastUpdatedBy
FROM EmailNotice
WHERE 
			(id						= @id or @id is null)
And		(NoticeCode		= @NoticeCode or @NoticeCode is null)
And		(LanguageCode = @LangCode or @LangCode is null)


GO
/****** Object:  StoredProcedure [dbo].[EmailNoticeInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =============================================================================
-- Description:	Insert Procedure for EmailNotice
-- 09-08-06 mparsons - increased @Filter to varchar(MAX)
-- 11-03-10 mparsons - increased @CcEmail to 200
-- 11-04-11 mparsons - increased @CcEmail to 500, just in case
-- =============================================================================
*/
CREATE PROCEDURE [dbo].[EmailNoticeInsert]
            @NoticeCode varchar(50), 
            @Title varchar(75), 
            @Description varchar(500), 
            @Filter varchar(MAX), 
            @isActive bit, 
            @LanguageCode varchar(10), 
            @FromEmail nvarchar(100), 
            @CcEmail nvarchar(500), 
            @BccEmail nvarchar(100), 
            @Subject nvarchar(100), 
            @HtmlBody nvarchar(MAX), 
            @TextBody nvarchar(MAX), 
            @CreatedBy varchar(25)
						,@Category		varchar(50)
As

If @Description = ''    SET @Description = NULL 
If @Filter = ''         SET @Filter = NULL 
If @Category = ''       SET @Category = 'General' 
If @LanguageCode = ''   SET @LanguageCode = 'en' 
If @FromEmail = ''      SET @FromEmail = NULL 
If @CcEmail = ''        SET @CcEmail = NULL 
If @BccEmail = ''       SET @BccEmail = NULL 
If @Subject = ''        SET @Subject = 'Missing'
If @TextBody = ''       SET @TextBody = NULL 
If @CreatedBy = ''			SET @CreatedBy = 'dbo' 

INSERT INTO EmailNotice(
    Title,
    NoticeCode, 
    Description, 
    Filter, 
    isActive, 
    LanguageCode, 
    FromEmail, 
    CcEmail, 
    BccEmail, 
    Subject, 
    HtmlBody, 
    TextBody, 
    Created, 
    CreatedBy, 
    LastUpdated, 
    LastUpdatedBy
		,Category
)
Values (
    @Title,
    @NoticeCode, 
    @Description, 
    @Filter, 
    @isActive, 
    @LanguageCode, 
    @FromEmail, 
    @CcEmail, 
    @BccEmail, 
    @Subject, 
    @HtmlBody, 
    @TextBody, 
    getdate(), 
    @CreatedBy, 
    getdate(), 
    @CreatedBy
		,@Category
)
 
select SCOPE_IDENTITY() as id

GO
/****** Object:  StoredProcedure [dbo].[EmailNoticeSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
 EmailNoticeSelect 'courses', 2,''
 EmailNoticeSelect '',2,'', 'iwts%'
*/
CREATE PROCEDURE [dbo].[EmailNoticeSelect]
		@Category			varchar(50),
    @IsActive     int = null,
		@LangCode			varchar(10) = 'en',
		@Keyword			varchar(200)

As

  If @Category = ''			SET @Category = NULL 
  If @IsActive < 0 OR @IsActive > 1 SET @IsActive = NULL 
  If @LangCode = ''		SET @LangCode = NULL 
  If @Keyword = ''		SET @Keyword = NULL 


SELECT     id, 
		[Title] + ' - ' + isnull(LanguageCode,'en') As Title_lang,
		[Title] + '  (' + NoticeCode + ')' As Title,
    NoticeCode, 
		Category,
    Description, 
    Filter, 
    isActive, 
    FromEmail, 
    CcEmail, 
    BccEmail, 
		LanguageCode,
    Subject, 
    HtmlBody, 
    TextBody, 
    Created, 
    CreatedBy, 
    LastUpdated, 
    LastUpdatedBy
FROM 
		EmailNotice
WHERE 
		(Category			= @Category or @Category is null)
And	(LanguageCode = @LangCode or @LangCode is null)
And	(IsActive	          = @IsActive		or @IsActive is null)
And	(@Keyword	is NULL 
      OR [Title] like @Keyword
      OR NoticeCode like @Keyword
      OR HtmlBody like @Keyword
    )
Order By 
		Title, LanguageCode

GO
/****** Object:  StoredProcedure [dbo].[EmailNoticeUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =============================================================================
-- Description:	Update Procedure for EmailNotice
-- 09-08-06 mparsons - increased @Filter to varchar(MAX)
-- 11-03-10 mparsons - increased @CcEmail to 200
-- 11-04-11 mparsons - increased @CcEmail to 500, just in case
-- =============================================================================
*/
CREATE PROCEDURE [dbo].[EmailNoticeUpdate]
        @id int,
        @NoticeCode varchar(50), 
        @Title varchar(75), 
        @Description varchar(500), 
        @Filter varchar(MAX), 
        @isActive bit, 
        @LanguageCode varchar(10), 
        @FromEmail nvarchar(100), 
        @CcEmail nvarchar(500), 
        @BccEmail nvarchar(100), 
        @Subject nvarchar(100), 
        @HtmlBody nvarchar(MAX), 
        @TextBody nvarchar(MAX), 
        @LastUpdatedBy varchar(25)
				,@Category		varchar(50)

As

If @Description = ''    SET @Description = NULL 
If @Category = ''       SET @Category = 'General' 
If @Filter = ''         SET @Filter = NULL 
If @LanguageCode = ''   SET @LanguageCode = 'en' 
If @FromEmail = ''      SET @FromEmail = NULL 
If @CcEmail = ''        SET @CcEmail = NULL 
If @BccEmail = ''       SET @BccEmail = NULL 
If @Subject = ''        SET @Subject = 'Missing' 
If @TextBody = ''       SET @TextBody = NULL 

If @LastUpdatedBy = '' 
  SET @LastUpdatedBy = 'dbo' 

UPDATE EmailNotice
SET 
    NoticeCode = @NoticeCode, 
    Title = @Title, 
    Description = @Description, 
		Category = @Category,
    Filter = @Filter, 
    isActive = @isActive, 
    LanguageCode = @LanguageCode, 
    FromEmail = @FromEmail, 
    CcEmail = @CcEmail, 
    BccEmail = @BccEmail, 
    Subject = @Subject, 
    HtmlBody = @HtmlBody, 
    TextBody = @TextBody, 
    LastUpdated = getdate(), 
    LastUpdatedBy = @LastUpdatedBy
WHERE id = @id

GO
/****** Object:  StoredProcedure [dbo].[Library.ResourceGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[Library.ResourceGet] 31
*/
CREATE PROCEDURE [dbo].[Library.ResourceGet]
    @Id int
As
SELECT     
	libres.Id, 
	libres.Id As LibraryResourceId,
    LibrarySectionId, 
    libres.ResourceIntId, 
	lr.ResourceVersionIntId,
	 CASE
      WHEN lr.Title IS NULL THEN 'No Title'
      WHEN len(lr.Title) = 0 THEN 'No Title'
      ELSE lr.Title
    END AS Title,
	lr.ResourceUrl,
	lr.SortTitle,
    Comment, 
    Created, 
    CreatedById
FROM [Library.Resource] libres
Inner join  [dbo].[lr.ResourceVersion_Summary] lr on libres.ResourceIntId = lr.ResourceIntId

WHERE libres.Id = @Id

grant execute on [Library.ResourceGet] to Public

GO
/****** Object:  StoredProcedure [dbo].[Library.ResourceInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
select top 200 * from [resource.version_summary]



[Library.ResourceInsert] 1, 0, 153249, 'TBD', 2

*/
/*
need to handle insert by code (for views, published, etc
If @LibrarySectionId is 0, and have a code, would need to look up by code and createdById
  - would have to handle creating of the section for the first time.
  - might be cleaner to handle server side
  
- we may want to handle my published differently anyway - or allow to delete?
  - if persisting under Resource.PublishedBy, no need to persist specifically, just allow display
- we could allow null section id - useful for quick adds
*/
CREATE PROCEDURE [dbo].[Library.ResourceInsert]
            @LibraryId int, 
            @LibrarySectionId int, 
            @ResourceIntId int, 
            @Comment varchar(500), 
            @CreatedById int
As
If @ResourceIntId = 0   SET @ResourceIntId = NULL 
If @LibrarySectionId = 0   SET @LibrarySectionId = NULL 
If @LibraryId = 0   SET @LibraryId = NULL 
If @Comment = ''   SET @Comment = NULL 
If @CreatedById = 0   SET @CreatedById = NULL 

If (@LibrarySectionId is null AND @LibraryId is null) OR @ResourceIntId is null begin
  print '[Library.ResourceInsert] Error: Incomplete parameters were provided'
	RAISERROR('[Library.ResourceInsert] Error: incomplete parameters were provided. Require Source @ResourceIntId, and @sectionId or @LibraryId', 18, 1)    
	RETURN -1 
  end
declare @sectionId int
,@availableSectionsCount int
,@newLibResId	int
,@setSectionAsDefault bit

set @setSectionAsDefault= 0

  
If @LibrarySectionId is null begin
    print '@LibrarySectionId not include, get default'
   select @LibrarySectionId = isnull(Id,0) from [Library.Section] where LibraryId = @LibraryId and IsDefaultSection = 1
   if @LibrarySectionId is null or @LibrarySectionId = 0 begin
      print 'default section not found, get min updateable'
      select @LibrarySectionId = isnull(min(Id),0) from [Library.Section] where LibraryId = @LibraryId and AreContentsReadOnly = 0
      if @LibrarySectionId is null or @LibrarySectionId = 0 begin
        print 'no available updateable sections, creating'
        --create
        INSERT INTO [dbo].[Library.Section]
           ([LibraryId]
           ,[SectionTypeId]
           ,[Description]
           ,[ParentId]
           ,[IsDefaultSection]
           ,[AreContentsReadOnly]
           ,[Created]
           ,[CreatedById]
           ,[LastUpdated]
           ,[LastUpdatedById])
         VALUES
               (@LibraryId, 
               3, 
               'Default section', 
               null, 
               1, 
               0, 
               getdate(), @CreatedById, 
               getdate(), @CreatedById 
                )
        set @LibrarySectionId = SCOPE_IDENTITY()
        end  --insert 
     else begin
        -- set as default
        UPDATE [dbo].[Library.Section]    
          SET [IsDefaultSection] = 1
        WHERE id = @LibrarySectionId
        end --min updateable found
      end --= default not found
      
   end --get default
   
--check for dups
if exists(SELECT [ResourceIntId] FROM [dbo].[Library.Resource] 
    where [ResourceIntId]= @ResourceIntId and LibrarySectionId = @LibrarySectionId) begin
    print '[Library.ResourceInsert] Error: resource already exists in collection'
    RAISERROR('[Library.ResourceInsert] Error: resource already exists in collection', 18, 1)    
    RETURN -1 
    end   
    
INSERT INTO [Library.Resource] (

    LibrarySectionId, 
    ResourceIntId, 
    Comment, 
    Created, 
    CreatedById
)
Values (

    @LibrarySectionId, 
    @ResourceIntId, 
    @Comment, 
    getdate(), 
    @CreatedById
)
 
select @newLibResId = SCOPE_IDENTITY() 
	
	UPDATE [Isle_IOER].[dbo].[Resource]
	SET FavoriteCount = FavoriteCount + 1
	WHERE Id = @ResourceIntId

select @newLibResId  as Id

GO
/****** Object:  StoredProcedure [dbo].[Library.SubscriptionGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*


*/
CREATE PROCEDURE [dbo].[Library.SubscriptionGet]
        @Id int,
        @LibraryId int, 
        @UserId int
As
If @Id = 0          SET @Id = NULL 
If @LibraryId = 0   SET @LibraryId = NULL 
If @UserId = 0      SET @UserId = NULL 

If @Id = 0 OR ( @LibraryId = 0 And @UserId = 0 ) begin
  RAISERROR('[Library.SubscriptionGet] - invalid request. Require @Id, OR @LibraryId AND @UserId', 18, 1)  
  return -1
  end
  
SELECT     
    base.Id, 
    LibraryId, lib.Title as Library, 
    UserId, 
    SubscriptionTypeId, cst.Title as SubscriptionType,
    PrivilegeId As MemberTypeId, 
    base.Created
FROM [Library.Subscription] base
Inner Join dbo.Library lib on base.LibraryId = lib.Id
Inner Join dbo.[Codes.SubscriptionType] cst on base.SubscriptionTypeId = cst.Id
WHERE 
    (base.Id = @Id or @Id is null)
And (LibraryId = @LibraryId or @LibraryId is null)    
And (UserId = @UserId or @UserId is null)


GO
/****** Object:  StoredProcedure [dbo].[Map.CleanseUrlSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/7/2014
-- Description:	Get URL cleansing rules for a given host
-- =============================================
CREATE PROCEDURE [dbo].[Map.CleanseUrlSelect]
	@Host varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, Host, urlParameterToDrop, Created, CreatedById, LastUpdated, LastUpdatedBy
	FROM [Map.CleanseUrl]
	WHERE Host = @Host
END

GO
/****** Object:  StoredProcedure [dbo].[Map.PathwayRules_Load]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 09/09/2012
-- Description:	Load Pathway map rules
-- =============================================
CREATE PROCEDURE [dbo].[Map.PathwayRules_Load]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT rules.Id, 'Cluster' AS PropertyType, 0 AS PropertyTypeId, OriginalValue, IsRegex, IsCaseSensitive,
		'False' AS ImportWithoutTranslation, 'False' AS DoNotImport, MappedValue, Sequence, IsActive, 
		rules.Created, rules.CreatedBy, rules.LastUpdated, LastUpdatedBy, Pathway.Id AS MappedId
	FROM [dbo].[Map.PathwayRules] rules
	LEFT JOIN Pathway ON rules.MappedValue = Pathway.Pathway
END

GO
/****** Object:  StoredProcedure [dbo].[Map.Rules_Load]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/31/2012
-- Description:	Load all map rules from Map.Rules table
-- =============================================
CREATE PROCEDURE [dbo].[Map.Rules_Load]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT rules.Id, ruletype.Title AS PropertyType, PropertyTypeId, OriginalValue, IsRegex, IsCaseSensitive, ImportWithoutTranslation,
		DoNotImport, MappedValue, Sequence, IsActive, Created, CreatedBy, LastUpdated, LastUpdatedBy,
		CASE
			WHEN rules.PropertyTypeId = 1 AND rules.MappedValue = audience.Title THEN audience.Id
			WHEN rules.PropertyTypeId = 2 AND rules.MappedValue = education.Title THEN education.Id
			WHEN rules.PropertyTypeId = 3 AND rules.MappedValue = format.Title THEN format.Id
			WHEN rules.PropertyTypeId = 7 AND rules.MappedValue = [type].Title THEN [type].Id
			ELSE NULL
		END AS MappedId
	FROM dbo.[Map.Rules] rules
	INNER JOIN [Codes.ResPropertyType] ruletype ON rules.PropertyTypeId = ruletype.Id
	LEFT JOIN [Codes.AudienceType] audience ON rules.MappedValue = audience.Title 
		AND rules.PropertyTypeId = 1
	LEFT JOIN [Codes.PathwaysEducationLevel] education ON rules.MappedValue = education.Title 
		AND rules.PropertyTypeId = 2
	LEFT JOIN [Codes.ResourceFormat] format ON rules.MappedValue = format.Title
		AND rules.PropertyTypeId = 3
	LEFT JOIN [Codes.ResourceType] [type] ON rules.MappedValue = [type].Title
		AND rules.PropertyTypeId = 7
END

GO
/****** Object:  StoredProcedure [dbo].[MapClusters.Search]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

-- ===========================================================

DECLARE @RC int,@Filter varchar(500), @StartPageIndex int, @PageSize int, @totalRows int,@SortOrder varchar(100)
set @SortOrder = '' 
set @Filter = ' CommunityId = 2 AND createdById = 2 '

set @Filter = ' MappedClusterId= 1 '
set @StartPageIndex = 1
set @PageSize = 25

exec [MapClusters.Search] @Filter, @SortOrder, @StartPageIndex  ,@PageSize  ,@totalRows OUTPUT

select 'total rows = ' + convert(varchar,@totalRows)


*/

/* =================================================
= MapClusters.Search
=		@StartPageIndex - starting page number. If interface is at 20 when next page is requested, this would be set to 21?
=		@PageSize - number of records on a page
=		@totalRows OUTPUT - total available rows. Used by interface to build a custom pager
= ------------------------------------------------------
= Modifications
= 14-03-17 mparsons - Created 
-- ================================================= */
Create PROCEDURE [dbo].[MapClusters.Search]
		@Filter				varchar(500)
		,@SortOrder			varchar(500)
		,@StartPageIndex	int
		,@PageSize		    int
		,@TotalRows			int OUTPUT
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
  set @OrderBy = ' Order by [FilterValue] '
--===================================================
-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1

 
-- =================================
CREATE TABLE #tempWorkTable(
	RowNumber int PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Id int NOT NULL,
	MappedClusterId int

)
-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter 
     end
	 else begin 
	 set @Filter =     '  '
	 end
 
set @SQL = 'SELECT [Id]  ,[MappedClusterId]   FROM [dbo].[Map.CareerCluster] mcc  '  
	  + @Filter


if charindex( 'order by', lower(@Filter) ) = 0 
	set @SQL = 	@SQL + @OrderBy
if @debugLevel > 3 begin
  print '@SQL len: '  +  convert(varchar,len(@SQL))
	print @SQL
	end
	
INSERT INTO #tempWorkTable (Id, MappedClusterId)
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
		base.[Id]
      ,[FilterValue]
      ,[IsRegex]
      ,base.[MappedClusterId]
	  ,cc.Title as ClusterTitle
      ,base.[IsActive]
      ,[Created]
      ,[LastUpdated]
      ,[LastUpdatedBy]

From #tempWorkTable temp
inner join [dbo].[Map.CareerCluster] base on temp.Id = base.Id
inner join [dbo].[codes.CareerCluster] cc on base.MappedClusterId = cc.Id
WHERE RowNumber > @first_id 
order by RowNumber		

SET ROWCOUNT 0

GO
/****** Object:  StoredProcedure [dbo].[MapK12Subject.Search]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

-- ===========================================================

DECLARE @RC int,@Filter varchar(500), @StartPageIndex int, @PageSize int, @totalRows int,@SortOrder varchar(100)
set @SortOrder = '' 
set @Filter = ''

set @Filter = ' MappedSubjectId= 4 '

set @StartPageIndex = 1
set @PageSize = 25

exec [MapK12Subject.Search] @Filter, @SortOrder, @StartPageIndex  ,@PageSize  ,@totalRows OUTPUT

select 'total rows = ' + convert(varchar,@totalRows)


*/

/* =================================================
= MapK12Subject.Search
=		@StartPageIndex - starting page number. If interface is at 20 when next page is requested, this would be set to 21?
=		@PageSize - number of records on a page
=		@totalRows OUTPUT - total available rows. Used by interface to build a custom pager
= ------------------------------------------------------
= Modifications
= 14-03-17 mparsons - Created 
-- ================================================= */
Create PROCEDURE [dbo].[MapK12Subject.Search]
		@Filter				varchar(500)
		,@SortOrder			varchar(500)
		,@StartPageIndex	int
		,@PageSize		    int
		,@TotalRows			int OUTPUT
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
  set @OrderBy = ' Order by [FilterValue] '
--===================================================
-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1

 
-- =================================
CREATE TABLE #tempWorkTable(
	RowNumber int PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Id int NOT NULL,
	MappedSubjectId int

)
-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter 
     end
	 else begin 
	 set @Filter =     '  '
	 end
 
set @SQL = 'SELECT [Id]  ,[MappedSubjectId]   FROM [dbo].[Map.K12Subject] mcc  '  
	  + @Filter


if charindex( 'order by', lower(@Filter) ) = 0 
	set @SQL = 	@SQL + @OrderBy
if @debugLevel > 3 begin
  print '@SQL len: '  +  convert(varchar,len(@SQL))
	print @SQL
	end
	
INSERT INTO #tempWorkTable (Id, MappedSubjectId)
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
		base.[Id]
      ,[FilterValue]
      ,[IsRegex]
      ,base.[MappedSubjectId], cc.Title As SubjectTitle
      ,base.[IsActive]
      ,base.[Created]
      ,base.[LastUpdated]
      ,base.[LastUpdatedBy]

From #tempWorkTable temp
inner join [dbo].[Map.K12Subject]  base on temp.Id = base.Id
inner join [dbo].[Codes.Subject] cc on base.MappedSubjectId = cc.Id

WHERE RowNumber > @first_id 
order by RowNumber		

SET ROWCOUNT 0

GO
/****** Object:  StoredProcedure [dbo].[ParadataPublish.Comment]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 05/17/2013
-- Description:	Publish Comments
-- =============================================
CREATE PROCEDURE [dbo].[ParadataPublish.Comment]
	@StartDate datetime,
	@EndDate datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT '' AS ActorDescription, 'User' AS ActorUserType, '2000-01-01/2000-01-01' AS DateRange, 'ISLE Detail Page' AS ContextId, 0 AS SampleSize, 0.0 AS Value, 
		ResourceUrl, '' AS RelatedObjectType, '' AS RelatedObjectId, '' AS RelatedObjectContent, rc.Created AS [Date], Comment
	FROM [Resource.Comment] rc
	INNER JOIN [Resource] res ON rc.ResourceIntId = res.Id
	WHERE rc.Created >= @StartDate AND rc.Created <= @EndDate AND rc.ResourceIntId NOT IN (SELECT Id FROM PrivateResource) AND
		(rc.CreatedById IS NULL OR rc.CreatedById = 0)
	
	
END

GO
/****** Object:  StoredProcedure [dbo].[ParadataPublish.Evaluation]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2013-05-17
-- Description:	Summarize Evaluations for Publishing
-- =============================================
CREATE PROCEDURE [dbo].[ParadataPublish.Evaluation]
	@StartDate datetime,
	@EndDate datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @DateRange varchar(21)
	SET @DateRange = CONVERT(varchar(10),@StartDate,120)+'/'+CONVERT(varchar(10),@EndDate,120)

	SELECT '' AS ActorDescription, 'User' AS ActorUserType, @DateRange AS DateRange, 'ISLE Detail Page' AS ContextId, COUNT(*) AS SampleSize,
		AVG(convert(decimal,Value)) AS Value, ResourceUrl, '' AS RelatedObjectType,
		CASE
			WHEN StandardId IS NOT NULL THEN sbn.NotationCode
			WHEN RubricId IS NOT NULL THEN rn.Notation
			ELSE NULL
		END AS RelatedObjectId,
		CASE
			WHEN StandardId IS NOT NULL THEN sbn.[Description]
			WHEN RubricId IS NOT NULL THEN rn.ShortDescription
			ELSE NULL
		END AS RelatedObjectContent, '2000-01-01' AS [Date], '' AS Comment
	FROM [Resource.Evaluation] rev
	INNER JOIN [Resource] res ON rev.ResourceIntId = res.Id
	LEFT JOIN [StandardBody.Node] sbn ON rev.StandardId = sbn.Id
	LEFT JOIN [Rubric.Node] rn ON rev.RubricId = rn.Id
	WHERE rev.Created >= @StartDate AND rev.Created <= @EndDate AND ResourceIntId NOT IN (SELECT Id FROM PrivateResource)
	GROUP BY ResourceUrl, StandardId, NotationCode, sbn.[Description], RubricId, Notation, rn.ShortDescription
END

GO
/****** Object:  StoredProcedure [dbo].[ParadataPublish.Favorite]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2013-05-17
-- Description:	Summarize Favorites for Publishing
-- =============================================
CREATE PROCEDURE [dbo].[ParadataPublish.Favorite]
	@StartDate datetime,
	@EndDate datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @DateRange varchar(21)
	SET @DateRange = CONVERT(varchar(10),@StartDate,120)+'/'+CONVERT(varchar(10),@EndDate,120)

	SELECT '' AS ActorDescription, 'User' AS ActorUserType, @DateRange AS DateRange, 'ISLE Detail Page' AS ContextId, 0 AS SampleSize, COUNT(*) AS Value,
		ResourceUrl, '' AS RelatedObjectType, '' AS RelatedObjectId, '' AS RelatedObjectContent, '2000-01-01' AS [Date], '' AS Comment
	FROM [IsleContent].[dbo].[Library.Resource] lr
	INNER JOIN [Resource] res on lr.ResourceIntId = res.Id
	WHERE lr.Created >= @StartDate AND lr.Created <= @EndDate AND ResourceIntId NOT IN (SELECT Id FROM PrivateResource)
	GROUP BY ResourceUrl
END

GO
/****** Object:  StoredProcedure [dbo].[ParadataPublish.Like]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2013-05-17
-- Description:	Summarize Likes/Dislikes for Publishing
-- =============================================
CREATE PROCEDURE [dbo].[ParadataPublish.Like] 
	@StartDate DateTime,
	@EndDate DateTime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @DateRange varchar(21)
	SET @DateRange = CONVERT(varchar(10),@StartDate,120)+'/'+CONVERT(varchar(10),@EndDate,120)
	
	SELECT '' AS ActorDescription, 'User' AS UserType, @DateRange AS DateRange, 'ISLE Detail Page' AS ContextId, COUNT(*) AS SampleSize,
		CASE
			WHEN IsLike = 'True' THEN 5
			ELSE 1
		END AS Value,
		ResourceUrl, '' AS RelatedObjectType, '' AS RelatedObjectId, '' AS RelatedObjectContent, '2000-01-01' AS [Date], '' AS Comment
	FROM [Resource.Like]
	INNER JOIN [Resource] ON [Resource.Like].ResourceIntId = [Resource].Id
	WHERE [Resource.Like].Created >= @StartDate AND [Resource.Like].Created <= @EndDate AND ResourceIntId NOT IN (SELECT Id FROM PrivateResource)
	GROUP BY ResourceUrl, IsLike
END

GO
/****** Object:  StoredProcedure [dbo].[ParadataPublish.View]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/6/2013
-- Description:	Summarize Views for publishing
-- =============================================
CREATE PROCEDURE [dbo].[ParadataPublish.View]
	-- Add the parameters for the stored procedure here
	@StartDate datetime,
	@EndDate datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @DateRange varchar(21)
	SET @DateRange = CONVERT(varchar(10),@StartDate,120)+'/'+CONVERT(varchar(10),@EndDate,120)
	
	SELECT '' AS ActorDescription, 'User' AS UserType, @DateRange AS DateRange, 'ISLE Detail Page' AS ContextId,
		COUNT(*) AS Value, ResourceUrl, '' AS RelatedObjectType, '' AS RelatedObjectId, '' AS RelatedObjectContent,
		'' AS Comment
	FROM [Resource.View] rv
	INNER JOIN [Resource] r ON rv.ResourceIntId = r.Id
	WHERE rv.Created >= @StartDate AND rv.Created <= @EndDate AND ResourceIntId NOT IN (SELECT Id FROM PrivateResource)
	GROUP BY ResourceUrl
END

GO
/****** Object:  StoredProcedure [dbo].[Patron.ExternalAccountGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Patron.ExternalAccountGet]
    @Id int,
    @PatronId int, 
    @ExternalSiteId int,
    @Token  varchar(50) 
As
if @Id = 0 set @Id = null
if @PatronId = 0 set @PatronId = null
if @ExternalSiteId = 0 set @ExternalSiteId = null
if len(@Token) = 0 set @Token = null

if @id is null And @PatronId is null And @ExternalSiteId is null begin
  print 'no id provided'
  RAISERROR(' A valid id or rowId must be supplied', 18, 1)    
  RETURN -1 
  end
  
SELECT     
    PatronId, 
    Id, 
    ExternalSiteId, 
    LoginId, 
    Token
FROM [Patron.ExternalAccount]
WHERE 
    (Id = @Id OR @Id is null)
And (PatronId = @PatronId OR @PatronId is null)    
And (ExternalSiteId = @ExternalSiteId OR @ExternalSiteId is null)
And (Token = @Token OR @Token is null)

GO
/****** Object:  StoredProcedure [dbo].[Patron.ExternalAccountInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Patron.ExternalAccountInsert]
            @PatronId int,  
            @ExternalSiteId int, 
            @LoginId varchar(100), 
            @Token varchar(50)
As
If @PatronId = 0   SET @PatronId = NULL 
If @ExternalSiteId = 0   SET @ExternalSiteId = NULL 
If @LoginId = ''   SET @LoginId = NULL 
--If @Password = ''   SET @Password = NULL 
If @Token = ''   SET @Token = NULL 

INSERT INTO [Patron.ExternalAccount] (

    PatronId, 
    ExternalSiteId, 
    LoginId, 
    Token
)
Values (

    @PatronId, 
    @ExternalSiteId, 
    @LoginId, 
    @Token
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[Patron.GetByExtAccount]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
PatronGet 'IllinoisPathways','77BAE02B-9E76-4877-A90C-BFFFE9B3464E'
*/
CREATE PROCEDURE [dbo].[Patron.GetByExtAccount]
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
    base.IsActive, 
    base.Created, 
    base.LastUpdated,     base.LastUpdatedById
    ,ExternalSiteId, LoginId, Token
FROM [Patron] base
inner join [Patron.ExternalAccount] ext on base.Id = ext.PatronId

WHERE	
    (LoginId = @LoginId OR @LoginId is null)
And (Token = @Token OR @Token is null)    
And (ExternalSiteId = @ExternalSiteId OR @ExternalSiteId is null)

GO
/****** Object:  StoredProcedure [dbo].[Patron.ProfileAssignOrgId]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Update Procedure for [Patron.Profile] ---
Create PROCEDURE [dbo].[Patron.ProfileAssignOrgId]
        @UserId int,
        @OrganizationId int 
As

If @OrganizationId = 0   SET @OrganizationId = NULL 

if @UserId is not null AND @UserId > 0 begin
	print 'check for proflie'
	if NOT exists( SELECT [UserId] FROM [dbo].[Patron.Profile] where [UserId]= @UserId)  begin
		--do insert
		INSERT INTO [dbo].[Patron.Profile] ([UserId] ,[OrganizationId],[CreatedById] ,[LastUpdatedId])
		 VALUES (@UserId ,@OrganizationId ,@UserId,@UserId)
      end    
    else begin
		--do update
		print 'Assigning user to org '
		UPDATE [dbo].[Patron.Profile]
		   SET [OrganizationId] = @OrganizationId,
		   LastUpdated = getdate()
		 WHERE UserId = @UserId
		 end
	--confirm
	SELECT [Id] As UserId,[FullName],[Email],[JobTitle],[RoleProfile],[OrganizationId],Organization,[PublishingRole],[Created]      ,[LastUpdated]
	  FROM [Isle_IOER].[dbo].[Patron_Summary]
	  WHERE Id = @UserId
	end
else begin 
	print 'error, user was not found, not assigned to org'
	end

GO
/****** Object:  StoredProcedure [dbo].[Patron.ProfileGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Patron.ProfileGet] 2
*/
CREATE PROCEDURE [dbo].[Patron.ProfileGet]
    @UserId int
As
SELECT     UserId, 
    JobTitle, 
    case when PublishingRoleId is null then 8
    else PublishingRoleId end as PublishingRoleId,
    
    case when PublishingRoleId is null then 'Public'
    else pr.Title end as PublishingRole,
	base.ImageUrl,  
    RoleProfile, 
    OrganizationId, 
    isnull(org.Name, '') as Organization,
    base.Created, 
    base.CreatedById, 
    base.LastUpdated, 
    base.LastUpdatedId
FROM [Patron.Profile] base
left join [Codes.AudienceType] pr on base.PublishingRoleId = pr.id
left join [Gateway.OrgSummary] org on OrganizationId = org.Id
WHERE base.UserId = @UserId

GO
/****** Object:  StoredProcedure [dbo].[Patron.ProfileInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Patron.ProfileInsert]
            @UserId int, 
            @JobTitle varchar(100), 
            @PublishingRoleId int, 
            @RoleProfile varchar(500), 
            @OrganizationId int
			,@ImageUrl varchar(200)
As
--declare @ImageUrl varchar(200)
--set @ImageUrl= ''

If @JobTitle = ''   SET @JobTitle = NULL 
If @PublishingRoleId = 0   SET @PublishingRoleId = NULL 
If @RoleProfile = ''   SET @RoleProfile = NULL 
If @ImageUrl = ''   SET @ImageUrl = NULL 
If @OrganizationId = 0   SET @OrganizationId = NULL 

INSERT INTO [Patron.Profile] (
    UserId,
    JobTitle, 
    PublishingRoleId, 
    RoleProfile, 
    OrganizationId, 
    CreatedById, 
    LastUpdatedId,
	ImageUrl
)
Values (
    @UserId,
    @JobTitle, 
    @PublishingRoleId, 
    @RoleProfile, 
    @OrganizationId, 
    @UserId, 
    @UserId,
	@ImageUrl
)
 
select @@RowCount as Id

GO
/****** Object:  StoredProcedure [dbo].[Patron.ProfileUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
USE [Isle_IOER]
GO

SELECT [Id]
      ,[UserName]
      ,[FirstName]
      ,[LastName]
      ,[FullName]
      ,[SortName]
      ,[Email]
      ,[JobTitle]
      ,[RoleProfile]
      ,[OrganizationId]
      ,[Organization]
      ,[PublishingRoleId]
      ,[PublishingRole]
      ,[Created]
      ,[LastUpdated]
  FROM [dbo].[Patron_Summary]
where OrganizationId is null


DECLARE @RC int, @UserId int, @JobTitle varchar(100),@PublishingRoleId int, @RoleProfile varchar(500), @OrganizationId int

-- TODO: Set parameter values here.
Set @UserId = 244
Set @JobTitle = ' test set of insert'
Set @PublishingRoleId = 1
Set @RoleProfile = ''
Set @OrganizationId = 3

-- TODO: Set parameter values here.

EXECUTE @RC = [dbo].[Patron.ProfileUpdate] 
   @UserId
  ,@JobTitle
  ,@PublishingRoleId
  ,@RoleProfile
  ,@OrganizationId
GO


EXECUTE @RC = [dbo].[Patron.ProfileUpdate] 
   @UserId
  ,@JobTitle
  ,@PublishingRoleId
  ,@RoleProfile
  ,@OrganizationId
GO




*/
--- Update Procedure for [Patron.Profile] ---
--Modifications
--14-02-07 mparsons - add existance check, as profile may not exist yet
CREATE PROCEDURE [dbo].[Patron.ProfileUpdate]
        @UserId int,
        @JobTitle varchar(100), 
        @PublishingRoleId int, 
        @RoleProfile varchar(500), 
        @OrganizationId int 
		,@ImageUrl varchar(200)
As
--declare @ImageUrl varchar(200)
--set @ImageUrl= ''

If @JobTitle = ''   SET @JobTitle = NULL 
If @PublishingRoleId = 0   SET @PublishingRoleId = NULL 
If @RoleProfile = ''   SET @RoleProfile = NULL 
If @ImageUrl = ''   SET @ImageUrl = NULL 
If @OrganizationId = 0   SET @OrganizationId = NULL 
--If @LastUpdatedId = 0   SET @LastUpdatedId = NULL 

print 'check for proflie'
	if NOT exists( SELECT [UserId] FROM [dbo].[Patron.Profile] where [UserId]= @UserId)  begin
		--do insert
		 INSERT INTO [Patron.Profile] (
			UserId,
			JobTitle, 
			PublishingRoleId, 
			RoleProfile, 
			OrganizationId, 
			CreatedById, 
			LastUpdatedId,
			ImageUrl
		)
		Values (
			@UserId,
			@JobTitle, 
			@PublishingRoleId, 
			@RoleProfile, 
			@OrganizationId, 
			@UserId, 
			@UserId,
			@ImageUrl
		)

      end    
    else begin
		--do update
		UPDATE [Patron.Profile] 
		SET 
			JobTitle = @JobTitle, 
			PublishingRoleId = @PublishingRoleId, 
			RoleProfile = @RoleProfile, 
			ImageUrl= @ImageUrl,
			OrganizationId = @OrganizationId, 
			LastUpdated = getdate()
			--,LastUpdatedId = @LastUpdatedId
		WHERE UserId = @UserId
		end

GO
/****** Object:  StoredProcedure [dbo].[Patron.RecoverPassword]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Patron.RecoverPassword] 'IllinoisPathways'


[Patron.RecoverPassword] 'mparsons'

*/
CREATE PROCEDURE [dbo].[Patron.RecoverPassword]
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
    UserName,  [Password],
    FirstName, 
    LastName, 
    Email, 
    IsActive, 
    Created, 
    LastUpdated, 
    LastUpdatedById,
    RowId
FROM [Patron]

WHERE	
    (UserName = @Lookup OR Email = @Lookup)
--AND IsActive = 1
-- not sure if IsActive should be checked here, or let interface
-- for example user may be attempting password recovery on an unconfirmed account
-- Actually in the latter case, it would be OK. The recover password code, would have to ensure IsActive is updated!!


GO
/****** Object:  StoredProcedure [dbo].[PatronAuthorize]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
PatronAuthorize 'IllinoisPathways','A2-F6-F3-62-D7-DB-BD-5E-68-30-CB-6B-B7-7E-EE-A2'

PatronAuthorize 'dane','24-E5-6B-78-02-F9-CE-CD-FB-40-F7-EB-08-56-06-B7'

PatronAuthorize 'kbrynteson@niu.edu','A2-F6-F3-62-D7-DB-BD-5E-68-30-CB-6B-B7-7E-EE-A2'

PatronAuthorize 'dane2','D8-79-ED-82-6A-1A-3F-65-12-FC-5E-CD-62-4E-FD-44'
*/
/* 
==================================================================
Modifications
13-04-29 mparsons - only allow active accounts
==================================================================
*/
CREATE PROCEDURE [dbo].[PatronAuthorize]
		@UserName varchar(75),
		@Password varchar(50)

As

--if NOT exists( SELECT * FROM [Patron] WHERE UserName = @UserName AND Password = @Password ) begin
--                print 'internal test message'
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
    IsActive, 
    Created, 
    LastUpdated, 
    LastUpdatedById,
    RowId
FROM [Patron]

WHERE	
    (UserName = @UserName OR Email = @UserName )
AND (Password = @Password 

)
And (IsActive = 1)

GO
/****** Object:  StoredProcedure [dbo].[PatronGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
PatronGet 0, 'IllinoisPathways','', ''

PatronGet 0, '','info@illinoisworknet.com', ''

PatronGet '','','','A7D110F7-AC7F-44B8-B2DE-CB1F1BFFC15C'
PatronGet 2, '', '', ''

*/
/* 
==================================================================
--- Get Procedure for [Patron] ---
Modifications
13-04-29 mparsons - added IsActive 
==================================================================
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
  RAISERROR(' A valid id or rowId, or email address, or usernme must be supplied', 18, 1)      
  RETURN -1 
  end

SELECT 
    Id, 
    UserName,  [Password],
    FirstName, 
    LastName, 
    Email, 
    IsActive, 
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
/****** Object:  StoredProcedure [dbo].[PatronInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/* 
==================================================================
--- Insert Procedure for [Patron] ---
Modifications
13-04-29 mparsons - added IsActive
==================================================================
*/
CREATE PROCEDURE [dbo].[PatronInsert]
            @UserName   varchar(50), 
            @Password   varchar(50), 
            @FirstName  varchar(50), 
            @LastName   varchar(50), 
            @Email      varchar(100),  
            @IsActive   bit
As
If @UserName = ''   SET @UserName = NULL 
If @Password = ''   SET @Password = NULL 
If @FirstName = ''   SET @FirstName = NULL 
If @LastName = ''   SET @LastName = NULL 
If @Email = ''   SET @Email = NULL 

if @Email is null OR @Password is null  begin
  print 'no id provided'
  RAISERROR('Error - at minimum, a valid email address and password are required', 18, 1)    
  RETURN -1 
  end
  
INSERT INTO [Patron] (
    UserName, 
    Password, 
    FirstName, 
    LastName, 
    Email,  
    IsActive
)
Values (

    @UserName, 
    @Password, 
    @FirstName, 
    @LastName, 
    @Email,  
    @IsActive
)
 
select SCOPE_IDENTITY() as Id


GO
/****** Object:  StoredProcedure [dbo].[PatronSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PatronSelect]
As
SELECT 
    Id, 
    UserName,  
    FirstName, 
    LastName, 
    Email, 
    IsActive, 
    Created, 
    LastUpdated, 
    LastUpdatedById
FROM [Patron]
ORDER BY LastName, FirstName

GO
/****** Object:  StoredProcedure [dbo].[PatronUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* 
==================================================================
--- Update Procedure for [Patron] ---
Modifications
13-04-29 mparsons - added IsActive 
==================================================================
*/
CREATE PROCEDURE [dbo].[PatronUpdate]
    @Id int,
    @Password varchar(50), 
    @FirstName varchar(50), 
    @LastName varchar(50), 
    @Email varchar(100), 
    @IsActive int 

As

If @Password = ''   SET @Password = NULL 
If @FirstName = ''  SET @FirstName = NULL 
If @LastName = ''   SET @LastName = NULL 
If @Email = ''      SET @Email = NULL 

If @Password is NULL begin
  select @Password = [Password] from [Patron] WHERE Id = @Id
end

UPDATE [Patron] 
SET 
    Password = @Password, 
    FirstName = @FirstName, 
    LastName = @LastName, 
    Email = @Email, 
    IsActive = @IsActive,
	LastUpdated = getdate() 
    
WHERE Id = @Id


GO
/****** Object:  StoredProcedure [dbo].[Publish.PendingGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Publish.PendingGet]
    @ResourceId int
    ,@ResourceVersionId int
    ,@Id int
As
if @ResourceId = 0     set @ResourceId = NULL
if @ResourceVersionId = 0     set @ResourceVersionId = NULL
if @Id = 0     set @Id = NULL


SELECT     Id, 
    ResourceIntId, 
    ResourceVersionIntId, 
    DocId, 
    Reason, 
    IsPublished, 
    Created, 
    CreatedById, 
    PublishedDate, 
    LrEnvelope
FROM [Publish.Pending]
WHERE 
    (ResourceIntId = @ResourceId or @ResourceId is null)
AND (ResourceVersionIntId = @ResourceVersionId or @ResourceVersionId is null)    
AND (Id = @Id or @Id is null)

GO
/****** Object:  StoredProcedure [dbo].[Publish.PendingInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Publish.PendingInsert]
        @ResourceId int, 
        @ResourceVersionId int, 
        @Reason varchar(100), 
        @CreatedById int, 
        @LrEnvelope varchar(MAX)
As
--??
If @ResourceId = 0   SET @ResourceId = NULL 
If @ResourceVersionId = 0   SET @ResourceVersionId = NULL 

If @Reason = ''   SET @Reason = 'Approval Pending' 

INSERT INTO [Publish.Pending] (

    ResourceIntId, 
    ResourceVersionIntId, 
    Reason, 
    IsPublished, 
    Created, 
    CreatedById, 
    LrEnvelope
)
Values (

    @ResourceId, 
    @ResourceVersionId, 
    @Reason, 
    0, 
    getdate(), 
    @CreatedById, 
    @LrEnvelope
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[Publish.PendingUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
--- Update Procedure for [Publish.Pending] ---
- if only reason for update is to publish, then just call DoPublish or something??
=> would be called after the publish, and provide the DocId
*/
CREATE PROCEDURE [dbo].[Publish.PendingUpdate]
        @Id int,
        @DocId varchar(500), 
        @IsPublished bit, 
        @PublishedDate datetime

As

If @DocId = ''   SET @DocId = NULL 
If @PublishedDate < '2000-01-01'   SET @PublishedDate = NULL 

UPDATE [Publish.Pending] 
SET 
    DocId = @DocId, 
    --Reason = @Reason, 
    IsPublished = @IsPublished, 
    PublishedDate = @PublishedDate
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[PublisherSearch]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
SELECT 
         ROW_NUMBER() OVER( Order by [ResourceTotal] desc, Publisher) as RowNumber,
      [Publisher] ,[ResourceTotal]
  FROM [dbo].[PublisherSummary]   where  Publisher like '%f%'  AND  (IsActive = 1)  
  

SELECT [Publisher], [Publisher]  + ' ( ' + convert(varchar, [ResourceTotal]) + ' ) ' As Combined FROM [dbo].[PublisherSummary] where IsActive = 1 order by 1
  

DECLARE @RC int,@SortOrder varchar(100),@Filter varchar(5000)
DECLARE @StartPageIndex int, @PageSize int, @totalRows int
--
set @SortOrder = '[ResourceTotal] desc, Publisher'

-- blind search 
set @Filter = ''
set @Filter = ' Publisher like ''%f%'' ' 

set @StartPageIndex = 40
set @PageSize = 100

EXECUTE @RC = PublisherSearch
     @Filter,@SortOrder  ,@StartPageIndex  ,@PageSize  ,@totalRows OUTPUT

select 'total rows = ' + convert(varchar,@totalRows)



*/


CREATE PROCEDURE [dbo].[PublisherSearch]
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
      ,@lastRow int
      ,@minRows int
      ,@debugLevel      int
      ,@SQL             varchar(5000)
      ,@Sql2            varchar(200)
      ,@DefaultFilter   varchar(1000)
      ,@OrderBy         varchar(100)
-- =================================

Set @debugLevel = 4

Set @DefaultFilter= ' (IsActive = 1 AND Publisher is NOT null) '
if len(@SortOrder) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by Publisher '
--===================================================
-- Calculate the range
--===================================================      
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1
SET @lastRow =  @StartPageIndex + @PageSize

-- =================================

  if len(isnull(@Filter,'')) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where  ' + @Filter
        
     set @Filter =  @Filter + ' AND ' + @DefaultFilter
     end
  else begin
    set @Filter =     ' where ' + @DefaultFilter
    end

  print '@Filter len: '  +  convert(varchar,len(@Filter))

  set @SQL = 'SELECT Distinct    
   pub.[Publisher] ,pub.[ResourceTotal] FROM
   (SELECT 
         ROW_NUMBER() OVER(' + @OrderBy + ') as RowNumber,
      [Publisher] ,[ResourceTotal]
  FROM [dbo].[PublisherSummary]  ' 
        + @Filter + ' 
   ) as DerivedTableName
       Inner join [dbo].[PublisherSummary] pub on DerivedTableName.[Publisher] = pub.[Publisher]
WHERE RowNumber BETWEEN ' + convert(varchar,@StartPageIndex) + ' AND ' + convert(varchar,@lastRow) + ' ' 
+ @OrderBy

  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL
  exec (@SQL)
  
  --===============================
  DECLARE @TempItems TABLE
(
   RowsFound int
)


set @Sql2= 'SELECT count(*) FROM [dbo].[PublisherSummary]  '
        + @Filter + ' '

 print @Sql2
 INSERT INTO @TempItems (RowsFound)
  exec (@Sql2)
  
  select @TotalRows= RowsFound from @TempItems

GO
/****** Object:  StoredProcedure [dbo].[RatingTypeGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/22/2012
-- Description:	Get RatingType
-- =============================================
CREATE PROCEDURE [dbo].[RatingTypeGet]
	@Id int,
	@Type varchar(50),
	@Identifier varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @Id = 0 SET @Id = NULL
	
	SELECT Id, [Type], Identifier, [Description], Created
	FROM RatingType
	WHERE (Id = @Id OR @Id IS NULL) AND
		([Type] = @Type OR @Type IS NULL) AND
		(Identifier = @Identifier OR @Identifier IS NULL)
END

GO
/****** Object:  StoredProcedure [dbo].[RatingTypeInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/22/2012
-- Description:	Create RatingType record
-- =============================================
CREATE PROCEDURE [dbo].[RatingTypeInsert]
	@Type varchar(50),
	@Identifier varchar(500),
	@Description varchar(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO RatingType ([Type], Identifier, [Description], Created)
	VALUES (@Type, @Identifier, @Description, GETDATE())
	
	SELECT @@IDENTITY AS Id
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityApiImport]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	[Resource.AccessibilityApiImport]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AccessibilityApiImport]
	@ResourceIntId int,
	@CodeId int,
	@OriginalValue varchar(100),
	@TotalRows int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @NewId int,
		@mapperId int,
		@exists int,
		@IsDuplicate bit,
		@RecordCount int,
		@SuppressOutput bit,
		@KeywordId int

		IF @OriginalValue = '' SET @OriginalValue = NULL
		IF @CodeId = -1 SET @SuppressOutput = 'True' ELSE SET @SuppressOutput = 'False'
		IF @TotalRows = -1 SET @SuppressOutput = 'True'
		IF @CodeId < 1 SET @CodeId = NULL

		IF @OriginalValue IS NULL AND @CodeId IS NULL BEGIN
			PRINT 'no values provided'
			RETURN -1
		END

		SET @IsDuplicate = 'False'
		SET @TotalRows = 0

		-- ====================================================================================
		-- Do keyword check.  If keyword found, delete keyword.
		SELECT @KeywordId = Id
		FROM [Resource.Keyword]
		WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
		IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
			DELETE FROM [Resource.Keyword]
			WHERE Id = @KeywordId
		END
		-- ====================================================================================

		IF @CodeId IS NULL BEGIN
			IF @SuppressOutput = 0
				PRINT 'INSERT via string'
			SELECT @CodeId = isnull(base.id,0)
			FROM [Codes.AccessibilityApi] base
			INNER JOIN [Map.AccessibilityApi] mapper on base.Id = mapper.CodeId
			WHERE mapper.OriginalValue = @OriginalValue

			IF @CodeId IS NULL OR @CodeId = 0 BEGIN
				-- no mapping, write to exception table and return.
				IF NOT exists(SELECT ResourceIntId FROM [Audit.AccessibilityApi_Orphan]
					WHERE ResourceIntId = @ResourceIntId AND OriginalValue = @OriginalValue) BEGIN
					if @SuppressOutput = 0
						print '@@ no mapping, writing to audit table: ' + @OriginalValue
					INSERT INTO [Audit.AccessibilityApi_Orphan] (ResourceIntId, OriginalValue)
					VALUES (@ResourceIntId, @OriginalValue)
				END ELSE BEGIN
					IF @SuppressOutput = 0
						print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
				END
				IF @SuppressOutput = 0
					SELECT 0 AS Id, 0 AS IsDuplicate
				return -1
			END
		END

		IF @CodeId > 0 BEGIN
			IF @SuppressOutput = 0
				PRINT '[CodeId] = ' + convert(varchar,@CodeId)
			SELECT @exists = base.Id
			FROM [Resource.AccessibilityApi] base
			WHERE base.ResourceIntId = @ResourceIntId AND base.AccessibilityApiId = @CodeId
			IF @exists IS NOT NULL AND @exists <> 0 BEGIN
				SET @newId = @exists
				SET @IsDuplicate = 1
				IF @SuppressOutput = 0
					PRINT 'found duplicate'
			END ELSE BEGIN
				INSERT INTO [Resource.AccessibilityApi] (ResourceIntId, AccessibilityApiId, OriginalValue)
				VALUES (@ResourceIntId, @CodeId, @OriginalValue)
			END
		END
		SET @TotalRows = @@ROWCOUNT
		SET @NewId = @@IDENTITY
		IF @SuppressOutput = 0
			SELECT @NewId AS Id, @IsDuplicate AS IsDuplicate
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityApiInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	[Resource.AccessibilityApiInsert]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AccessibilityApiInsert]
	@ResourceIntId int,
	@AccessibilityApiId int,
	@OriginalValue varchar(100),
	@CreatedById int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @NewId int

	INSERT INTO [Resource.AccessibilityApi] (ResourceIntId, AccessibilityApiId, OriginalValue, CreatedById)
	VALUES (@ResourceIntId, @AccessibilityApiId, @OriginalValue, @CreatedById)
	SET @NewId = @@IDENTITY

	SELECT @NewId AS Id
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityApiSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	[Resource.AccessibilityApiSelect]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AccessibilityApiSelect] 
	@ResourceIntId int,
	@AccessibilityApiId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @ResourceIntId = 0 SET @ResourceIntId = NULL
	IF @AccessibilityApiId = 0 SET @AccessibilityApiId = NULL

	SELECT Id, ResourceIntId, AccessibilityApiId, OriginalValue, CreatedById
	FROM [Resource.AccessibilityApi]
	WHERE (ResourceIntId = @ResourceIntId OR @ResourceIntId IS NULL) AND
		(AccessibilityApiId = @AccessibilityApiId OR @AccessibilityApiId IS NULL)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityControl_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.AccessibilityControl_SelectedCodes] 448010
*/
Create PROCEDURE [dbo].[Resource.AccessibilityControl_SelectedCodes]
    @ResourceIntId int
As
SELECT distinct
	code.Id, code.Title, code.[Description]
   -- ,ResourceIntId
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.AccessibilityControl] code
Left Join [Resource.AccessibilityControl] rpw on code.Id = rpw.AccessibilityControlId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityControlImport]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	[Resource.AccessibilityControlImport]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AccessibilityControlImport]
	@ResourceIntId int,
	@CodeId int,
	@OriginalValue varchar(100),
	@TotalRows int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @NewId int,
		@mapperId int,
		@exists int,
		@IsDuplicate bit,
		@RecordCount int,
		@SuppressOutput bit,
		@KeywordId int

		IF @OriginalValue = '' SET @OriginalValue = NULL
		IF @CodeId = -1 SET @SuppressOutput = 'True' ELSE SET @SuppressOutput = 'False'
		IF @TotalRows = -1 SET @SuppressOutput = 'True'
		IF @CodeId < 1 SET @CodeId = NULL

		IF @OriginalValue IS NULL AND @CodeId IS NULL BEGIN
			PRINT 'no values provided'
			RETURN -1
		END

		SET @IsDuplicate = 'False'
		SET @TotalRows = 0

		-- ====================================================================================
		-- Do keyword check.  If keyword found, delete keyword.
		SELECT @KeywordId = Id
		FROM [Resource.Keyword]
		WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
		IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
			DELETE FROM [Resource.Keyword]
			WHERE Id = @KeywordId
		END
		-- ====================================================================================

		IF @CodeId IS NULL BEGIN
			IF @SuppressOutput = 0
				PRINT 'INSERT via string'
			SELECT @CodeId = isnull(base.id,0)
			FROM [Codes.AccessibilityControl] base
			INNER JOIN [Map.AccessibilityControl] mapper on base.Id = mapper.CodeId
			WHERE mapper.OriginalValue = @OriginalValue

			IF @CodeId IS NULL OR @CodeId = 0 BEGIN
				-- no mapping, write to exception table and return.
				IF NOT exists(SELECT ResourceIntId FROM [Audit.AccessibilityControl_Orphan]
					WHERE ResourceIntId = @ResourceIntId AND OriginalValue = @OriginalValue) BEGIN
					if @SuppressOutput = 0
						print '@@ no mapping, writing to audit table: ' + @OriginalValue
					INSERT INTO [Audit.AccessibilityControl_Orphan] (ResourceIntId, OriginalValue)
					VALUES (@ResourceIntId, @OriginalValue)
				END ELSE BEGIN
					IF @SuppressOutput = 0
						print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
				END
				IF @SuppressOutput = 0
					SELECT 0 AS Id, 0 AS IsDuplicate
				return -1
			END
		END

		IF @CodeId > 0 BEGIN
			IF @SuppressOutput = 0
				PRINT '[CodeId] = ' + convert(varchar,@CodeId)
			SELECT @exists = base.Id
			FROM [Resource.AccessibilityControl] base
			WHERE base.ResourceIntId = @ResourceIntId AND base.AccessibilityControlId = @CodeId
			IF @exists IS NOT NULL AND @exists <> 0 BEGIN
				SET @newId = @exists
				SET @IsDuplicate = 1
				IF @SuppressOutput = 0
					PRINT 'found duplicate'
			END ELSE BEGIN
				INSERT INTO [Resource.AccessibilityControl] (ResourceIntId, AccessibilityControlId, OriginalValue)
				VALUES (@ResourceIntId, @CodeId, @OriginalValue)
			END
		END
		SET @TotalRows = @@ROWCOUNT
		SET @NewId = @@IDENTITY
		IF @SuppressOutput = 0
			SELECT @NewId AS Id, @IsDuplicate AS IsDuplicate
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityControlInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	[Resource.AccessibilityControlInsert]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AccessibilityControlInsert]
	@ResourceIntId int,
	@AccessibilityControlId int,
	@OriginalValue varchar(100),
	@CreatedById int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @NewId int

	INSERT INTO [Resource.AccessibilityControl] (ResourceIntId, AccessibilityControlId, OriginalValue, CreatedById)
	VALUES (@ResourceIntId, @AccessibilityControlId, @OriginalValue, @CreatedById)
	SET @NewId = @@IDENTITY

	SELECT @NewId AS Id
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityControlSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	[Resource.AccessibilityControlSelect]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AccessibilityControlSelect] 
	@ResourceIntId int,
	@AccessibilityControlId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @ResourceIntId = 0 SET @ResourceIntId = NULL
	IF @AccessibilityControlId = 0 SET @AccessibilityControlId = NULL

	SELECT Id, ResourceIntId, AccessibilityControlId, OriginalValue, CreatedById
	FROM [Resource.AccessibilityControl]
	WHERE (ResourceIntId = @ResourceIntId OR @ResourceIntId IS NULL) AND
		(AccessibilityControlId = @AccessibilityControlId OR @AccessibilityControlId IS NULL)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityFeature_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.AccessibilityFeature_SelectedCodes] 448010
*/
Create PROCEDURE [dbo].[Resource.AccessibilityFeature_SelectedCodes]
    @ResourceIntId int
As
SELECT distinct
	code.Id, code.Title, code.[Description]
   -- ,ResourceIntId
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.AccessibilityFeature] code
Left Join [Resource.AccessibilityFeature] rpw on code.Id = rpw.AccessibilityFeatureId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityFeatureImport]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	[Resource.AccessibilityFeatureImport]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AccessibilityFeatureImport]
	@ResourceIntId int,
	@CodeId int,
	@OriginalValue varchar(100),
	@TotalRows int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @NewId int,
		@mapperId int,
		@exists int,
		@IsDuplicate bit,
		@RecordCount int,
		@SuppressOutput bit,
		@KeywordId int

		IF @OriginalValue = '' SET @OriginalValue = NULL
		IF @CodeId = -1 SET @SuppressOutput = 'True' ELSE SET @SuppressOutput = 'False'
		IF @TotalRows = -1 SET @SuppressOutput = 'True'
		IF @CodeId < 1 SET @CodeId = NULL

		IF @OriginalValue IS NULL AND @CodeId IS NULL BEGIN
			PRINT 'no values provided'
			RETURN -1
		END

		SET @IsDuplicate = 'False'
		SET @TotalRows = 0

		-- ====================================================================================
		-- Do keyword check.  If keyword found, delete keyword.
		SELECT @KeywordId = Id
		FROM [Resource.Keyword]
		WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
		IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
			DELETE FROM [Resource.Keyword]
			WHERE Id = @KeywordId
		END
		-- ====================================================================================

		IF @CodeId IS NULL BEGIN
			IF @SuppressOutput = 0
				PRINT 'INSERT via string'
			SELECT @CodeId = isnull(base.id,0)
			FROM [Codes.AccessibilityFeature] base
			INNER JOIN [Map.AccessibilityFeature] mapper on base.Id = mapper.CodeId
			WHERE mapper.OriginalValue = @OriginalValue

			IF @CodeId IS NULL OR @CodeId = 0 BEGIN
				-- no mapping, write to exception table and return.
				IF NOT exists(SELECT ResourceIntId FROM [Audit.AccessibilityFeature_Orphan]
					WHERE ResourceIntId = @ResourceIntId AND OriginalValue = @OriginalValue) BEGIN
					if @SuppressOutput = 0
						print '@@ no mapping, writing to audit table: ' + @OriginalValue
					INSERT INTO [Audit.AccessibilityFeature_Orphan] (ResourceIntId, OriginalValue)
					VALUES (@ResourceIntId, @OriginalValue)
				END ELSE BEGIN
					IF @SuppressOutput = 0
						print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
				END
				IF @SuppressOutput = 0
					SELECT 0 AS Id, 0 AS IsDuplicate
				return -1
			END
		END

		IF @CodeId > 0 BEGIN
			IF @SuppressOutput = 0
				PRINT '[CodeId] = ' + convert(varchar,@CodeId)
			SELECT @exists = base.Id
			FROM [Resource.AccessibilityFeature] base
			WHERE base.ResourceIntId = @ResourceIntId AND base.AccessibilityFeatureId = @CodeId
			IF @SuppressOutput = 0
				PRINT '[exists test complete]'
			IF @exists IS NOT NULL AND @exists <> 0 BEGIN
				SET @newId = @exists
				SET @IsDuplicate = 1
				IF @SuppressOutput = 0
					PRINT 'found duplicate'
			END ELSE BEGIN
				INSERT INTO [Resource.AccessibilityFeature] (ResourceIntId, AccessibilityFeatureId, OriginalValue)
				VALUES (@ResourceIntId, @CodeId, @OriginalValue)
				IF @SuppressOutput = 0
					PRINT '[inserted]'
			END
		END
		SET @TotalRows = @@ROWCOUNT
		SET @NewId = @@IDENTITY
		IF @SuppressOutput = 0
			SELECT @NewId AS Id, @IsDuplicate AS IsDuplicate
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityFeatureInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	[Resource.AccessibilityFeatureInsert]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AccessibilityFeatureInsert]
	@ResourceIntId int,
	@AccessibilityFeatureId int,
	@OriginalValue varchar(100),
	@CreatedById int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @NewId int

	INSERT INTO [Resource.AccessibilityFeature] (ResourceIntId, AccessibilityFeatureId, OriginalValue, CreatedById)
	VALUES (@ResourceIntId, @AccessibilityFeatureId, @OriginalValue, @CreatedById)
	SET @NewId = @@IDENTITY

	SELECT @NewId AS Id
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityFeatureSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	[Resource.AccessibilityFeatureSelect]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AccessibilityFeatureSelect] 
	@ResourceIntId int,
	@AccessibilityFeatureId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @ResourceIntId = 0 SET @ResourceIntId = NULL
	IF @AccessibilityFeatureId = 0 SET @AccessibilityFeatureId = NULL

	SELECT Id, ResourceIntId, AccessibilityFeatureId, OriginalValue, CreatedById
	FROM [Resource.AccessibilityFeature]
	WHERE (ResourceIntId = @ResourceIntId OR @ResourceIntId IS NULL) AND
		(AccessibilityFeatureId = @AccessibilityFeatureId OR @AccessibilityFeatureId IS NULL)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityHazard_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.AccessibilityHazard_SelectedCodes] 448010
*/
Create PROCEDURE [dbo].[Resource.AccessibilityHazard_SelectedCodes]
    @ResourceIntId int
As
SELECT distinct
	code.Id, code.Title, code.[Description]
   -- ,ResourceIntId
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.AccessibilityHazard] code
Left Join [Resource.AccessibilityHazard] rpw on code.Id = rpw.AccessibilityHazardId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityHazardImport]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	[Resource.AccessibilityHazardImport]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AccessibilityHazardImport]
	@ResourceIntId int,
	@CodeId int,
	@OriginalValue varchar(100),
	@TotalRows int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @NewId int,
		@mapperId int,
		@exists int,
		@IsDuplicate bit,
		@RecordCount int,
		@SuppressOutput bit,
		@KeywordId int,
		@AntonymId int,
		@AntonymIdExists int

		IF @OriginalValue = '' SET @OriginalValue = NULL
		IF @CodeId = -1 SET @SuppressOutput = 'True' ELSE SET @SuppressOutput = 'False'
		IF @TotalRows = -1 SET @SuppressOutput = 'True'
		IF @CodeId < 1 SET @CodeId = NULL

		IF @OriginalValue IS NULL AND @CodeId IS NULL BEGIN
			PRINT 'no values provided'
			RETURN -1
		END

		SET @IsDuplicate = 'False'
		SET @TotalRows = 0

		-- ====================================================================================
		-- Do keyword check.  If keyword found, delete keyword.
		SELECT @KeywordId = Id
		FROM [Resource.Keyword]
		WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
		IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
			DELETE FROM [Resource.Keyword]
			WHERE Id = @KeywordId
		END
		-- ====================================================================================

		IF @CodeId IS NULL BEGIN
			IF @SuppressOutput = 0
				PRINT 'INSERT via string'
			SELECT @CodeId = isnull(base.id,0), @AntonymId = isnull(base.AntonymId,0)
			FROM [Codes.AccessibilityHazard] base
			INNER JOIN [Map.AccessibilityHazard] mapper on base.Id = mapper.CodeId
			WHERE mapper.OriginalValue = @OriginalValue

			IF @CodeId IS NULL OR @CodeId = 0 BEGIN
				-- no mapping, write to exception table and return.
				IF NOT exists(SELECT ResourceIntId FROM [Audit.AccessibilityHazard_Orphan]
					WHERE ResourceIntId = @ResourceIntId AND OriginalValue = @OriginalValue) BEGIN
					if @SuppressOutput = 0
						print '@@ no mapping, writing to audit table: ' + @OriginalValue
					INSERT INTO [Audit.AccessibilityHazard_Orphan] (ResourceIntId, OriginalValue)
					VALUES (@ResourceIntId, @OriginalValue)
				END ELSE BEGIN
					IF @SuppressOutput = 0
						print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
				END
				IF @SuppressOutput = 0
					SELECT 0 AS Id, 0 AS IsDuplicate
				return -1
			END
		END

		IF @CodeId > 0 BEGIN
			IF @SuppressOutput = 0
				PRINT '[CodeId] = ' + convert(varchar,@CodeId)
			IF @AntonymId = 0 OR @AntonymId IS NULL BEGIN
				SELECT @AntonymId = AntonymId
				FROM [Codes.AccessibilityHazard]
				WHERE Id = @CodeId
			END

			SELECT @exists = base.Id
			FROM [Resource.AccessibilityHazard] base
			WHERE base.ResourceIntId = @ResourceIntId AND base.AccessibilityHazardId = @CodeId
			SELECT @AntonymIdExists = base.Id
			FROM [Resource.AccessibilityHazard] base
			WHERE base.ResourceIntId = @ResourceIntId AND base.AccessibilityHazardId = @AntonymId
			IF @AntonymIdExists IS NOT NULL AND @AntonymIdExists <> 0 BEGIN
				DELETE FROM [Resource.AccessibilityHazard] 
				WHERE Id = @AntonymIdExists
			END
			IF @exists IS NOT NULL AND @exists <> 0 BEGIN
				SET @newId = @exists
				SET @IsDuplicate = 1
				IF @SuppressOutput = 0
					PRINT 'found duplicate'
			END ELSE BEGIN
				INSERT INTO [Resource.AccessibilityHazard] (ResourceIntId, AccessibilityHazardId, OriginalValue)
				VALUES (@ResourceIntId, @CodeId, @OriginalValue)
			END
		END
		SET @TotalRows = @@ROWCOUNT
		SET @NewId = @@IDENTITY
		IF @SuppressOutput = 0
			SELECT @NewId AS Id, @IsDuplicate AS IsDuplicate
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityHazardInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	[Resource.AccessibilityHazardInsert]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AccessibilityHazardInsert]
	@ResourceIntId int,
	@AccessibilityHazardId int,
	@OriginalValue varchar(100),
	@CreatedById int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @NewId int,
		@AntonymId int,
		@exists int

	-- Look up antonym and check for its existence.  If it exists, delete it before inserting the new hazard row.
	SELECT @AntonymId = AntonymId
	FROM [Codes.AccessibilityHazard]
	WHERE Id = @AccessibilityHazardId

	SELECT @exists = Id
	FROM [Resource.AccessibilityHazard]
	WHERE ResourceIntId = @ResourceIntId AND AccessibilityHazardId = @AntonymId
	IF @exists IS NOT NULL AND @exists <> 0 BEGIN
		DELETE FROM [Resource.AccessibilityHazard]
		WHERE Id = @exists
	END

	-- Now do the insert
	INSERT INTO [Resource.AccessibilityHazard] (ResourceIntId, AccessibilityHazardId, OriginalValue, CreatedById)
	VALUES (@ResourceIntId, @AccessibilityHazardId, @OriginalValue, @CreatedById)
	SET @NewId = @@IDENTITY

	SELECT @NewId AS Id
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessibilityHazardSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/10/2014
-- Description:	[Resource.AccessibilityHazardSelect]
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AccessibilityHazardSelect] 
	@ResourceIntId int,
	@AccessibilityHazardId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @ResourceIntId = 0 SET @ResourceIntId = NULL
	IF @AccessibilityHazardId = 0 SET @AccessibilityHazardId = NULL

	SELECT rah.Id, ResourceIntId, AccessibilityHazardId, OriginalValue, CreatedById, AntonymId
	FROM [Resource.AccessibilityHazard] rah
	INNER JOIN [Codes.AccessibilityHazard] cah ON rah.AccessibilityHazardId = cah.Id
	WHERE (ResourceIntId = @ResourceIntId OR @ResourceIntId IS NULL) AND
		(AccessibilityHazardId = @AccessibilityHazardId OR @AccessibilityHazardId IS NULL)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AccessRights_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.AccessRights_SelectedCodes] 448010
*/
CREATE PROCEDURE [dbo].[Resource.AccessRights_SelectedCodes]
    @ResourceIntId int
As
SELECT distinct
	code.Id, code.Title, code.[Description]
   -- ,ResourceIntId
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.AccessRights] code
Left Join [Resource.Version] rpw on code.Id = rpw.AccessRightsId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1

GO
/****** Object:  StoredProcedure [dbo].[Resource.AgeRangeGetByIntId]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 11/14/2013
-- Description:	Get age range record based on ResourceIntId
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AgeRangeGetByIntId]
	@ResourceIntId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, ResourceIntId, FromAge, ToAge, OriginalLevel,
		Created, CreatedById
	FROM [Resource.AgeRange]
	WHERE ResourceIntId = @ResourceIntId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AgeRangeImport]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 5/8/2013
-- Description:	Imports Age Range
--
-- 2013-11-13 jgrimmer - If the top end of the age range indicates Adult Ed., and a lower age range exists
--						 and ends before High School (age 14), throw out the Adult Ed. range and keep the
--						 lower age range.
-- =============================================
CREATE PROCEDURE [dbo].[Resource.AgeRangeImport]
	@ResourceIntId int,
	@iFromAge int,
	@iToAge int,
	@iOriginalValue varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @id int, @FromAge int, @ToAge int, @OriginalValue varchar(50), @AgeRange int, @swapAge int
	IF @iFromAge > @iToAge BEGIN
		-- Swap From and To ages
		SET @swapAge = @iFromAge
		SET @iFromAge = @iToAge
		SET @iToAge = @swapAge
	END
	
	SELECT @id = Id, @FromAge = FromAge, @ToAge = ToAge, @OriginalValue = OriginalLevel
	FROM [Resource.AgeRange]
	WHERE ResourceIntId = @ResourceIntId
	
	IF @id IS NULL BEGIN
		-- Row does not exist
		SET @AgeRange = ABS(@iToAge - @iFromAge) + 1
		INSERT INTO [Resource.AgeRange] (ResourceIntId, FromAge, ToAge, AgeSpan, OriginalLevel, Created, CreatedById)
		VALUES (@ResourceIntId, @iFromAge, @iToAge, @AgeRange, @iOriginalValue, GETDATE(), NULL)
	END ELSE BEGIN
		-- If already flagged as Adult Ed and a lower end of age range < high school, toss adult ed.
		IF @ToAge >= 21 AND (@iFromAge < 14 OR @FromAge < 14) BEGIN
			IF @iFromAge < @FromAge SET @FromAge = @iFromAge
			IF @iToAge < @ToAge SET @ToAge = @iToAge
		END ELSE IF @ToAge >= 21 AND (@iFromAge >= 14) BEGIN
			IF @iFromAge < @FromAge SET @FromAge = @iFromAge
			IF @iToAge > @ToAge SET @ToAge = @iToAge
		END

		-- Update row if not adult ed, or adult ed but also includes high school
		IF @iToAge < 21 OR (@iToAge >= 21 AND @ToAge > 13) BEGIN
			-- Update existing row
			IF @iFromAge < @FromAge SET @FromAge = @iFromAge
			IF @iToAge > @ToAge SET @ToAge = @iToAge
			SET @AgeRange = ABS(@iToAge - @iFromAge) + 1
			UPDATE [Resource.AgeRange]
			SET FromAge = @FromAge,
				ToAge = @ToAge,
				AgeSpan = @AgeRange,
				Created = GETDATE(),
				CreatedById = NULL
			WHERE Id = @id
		END
	END
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.AssessmentType_Import]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
Exec CodeTables_UpdateWarehouseTotals 10

DECLARE @OriginalValue varchar(500)

set @OriginalValue = '15  - 17'
--set @OriginalValue = Replace(@OriginalValue, ' - ', '-')
set @OriginalValue = Replace(@OriginalValue, ' ', '')
select @OriginalValue

SELECT [Id]
      ,[Title]

  FROM [dbo].[Codes.GradeLevel]
GO

select * from [dbo].[Audit.GradeLevel_Orphan]


SELECT    Id, PropertyTypeId, OriginalValue,  MappedValue
FROM         [Map.Rules]
WHERE     (PropertyTypeId = 2)
ORDER BY OriginalValue

SELECT    map.Id, map.PropertyTypeId, map.OriginalValue,  map.MappedValue
FROM         [Map.Rules] map
inner join [Codes.GradeLevel] codes on map.MappedValue = codes.Title
WHERE     (map.PropertyTypeId = 2)
ORDER BY map.OriginalValue


--===============
SELECT [ResourceIntId] ,[OriginalValue]

  FROM [dbo].[Audit.GradeLevel_Orphan]
  where MappingType = 'Map.GradeLevel'

--======================================================
DECLARE @RC int, @ResourceIntId uniqueidentifier,@OriginalValue varchar(500)
EXECUTE [dbo].[Resource.GradeLevel_ImportResProperty] 
SELECT 
@ResourceIntId=[ResourceIntId] ,@OriginalValue=[OriginalValue]

  FROM [dbo].[Audit.GradeLevel_Orphan]
  where MappingType = 'Map.GradeLevel'
  
--======================================================

SELECT top 1000
[ResourceIntId]
      ,[Name]
      ,[Value]
      ,[PropertyTypeId]
      ,[Imported]
  FROM [dbo].[Resource.Property]
  where PropertyTypeId = 2 AND Value = 'High School'
  order by 1

B773208E-FF0B-49CA-A416-000030A2138D
5360B642-B3B7-4CD7-B6E1-0000C3F499B8
33E8A821-C0F2-42CE-B7E6-0002D78CA861
0A62FF63-FC37-4DDD-8688-0007A01A6C04

-- ====================================================================
DECLARE @RC int, @ResourceIntId uniqueidentifier,@OriginalValue varchar(500)
, @totalRows  int

set @ResourceIntId = '5360B642-B3B7-4CD7-B6E1-0000C3F499B8'
set @OriginalValue = 'Summer School'
set @OriginalValue = '&gt;1'
set @OriginalValue = 'Grade 1'
*/
/*
select newId(), @ResourceIntId, isnull(codes.id,0), @OriginalValue
  FROM [Map.GradeLevel] map 
  inner join [dbo].[Codes.GradeLevel] codes on map.CodeId = codes.Id
  left join [dbo].[Resource.GradeLevel] red on @ResourceIntId = red.ResourceIntId and red.[GradeLevelId] = codes.id
    
 where map.OriginalValue  = @OriginalValue
 */
/* 
EXECUTE @RC = [dbo].[Resource.GradeLevel_Import]
   @ResourceIntId
  ,@OriginalValue, '', @totalRows OUTPUT
  
  select  @totalRows
  


*/
/*
Notes:
- may need to use a passed schema to help with mapping

*/
CREATE PROCEDURE [dbo].[Resource.AssessmentType_Import]
            @ResourceIntId	int,
            @OriginalValue  varchar(500)
            ,@TotalRows     int OUTPUT

As
begin 
declare 
  @GradeLevelId int
  , @SuppressOutput bit
  , @RecordCount int
  
  set @SuppressOutput  = 0
  If @TotalRows = -1		SET @SuppressOutput = 1
  
  set @TotalRows = 0
  set @GradeLevelId = 0
  
If @OriginalValue = '' begin
  print 'no value provided'
  return -1
  end              
-- ======================================================
-- If it exists as a keyword, blow it away because we're putting it in as an Assessment Type
DECLARE @KeywordId int
SELECT @KeywordId = Id
FROM [Resource.Keyword]
WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
	DELETE FROM [Resource.Keyword]
	WHERE Id = @KeywordId
END
-- ======================================================
 -- now check if mapping exists
select @RecordCount = isnull(Count(*),0)
  FROM [Map.AssessmentType] map 
  inner join [dbo].[Codes.AssessmentType] codes on map.CodeId = codes.Id
  --inner join [dbo].[Codes.GradeLevel] codes on map.MappedValue = codes.[Title]
 where map.LRValue  = @OriginalValue
 
If @RecordCount is null OR @RecordCount = 0	begin 
  --no mapping, write to exceptions table and return
  if NOT exists(SELECT [ResourceIntId] FROM [dbo].[Audit.GradeLevel_Orphan] 
    where [ResourceIntId]= @ResourceIntId and [OriginalValue] = @OriginalValue) begin
    print 'no mapping, writing to exceptions table: ' + @OriginalValue
    INSERT INTO [dbo].[Audit.AssessmentType_Orphan]
             ([ResourceIntId]
             ,[OriginalValue])
       VALUES
             (@ResourceIntId
             ,@OriginalValue)
    end
  return -1
  end
-- ======================================================
--mapping exists, need to handle already in target table 
--print 'doing insert'
  INSERT INTO [Resource.AssessmentType]
  (
	  ResourceIntId, 
	  AssessmentTypeId
  )
  
select @ResourceIntId, isnull(codes.id,0)
  FROM [Map.AssessmentType] map 
  inner join [dbo].[Codes.AssessmentType] codes on map.CodeId = codes.Id
  left join [dbo].[Resource.AssessmentType] red on @ResourceIntId = red.ResourceIntId and red.[AssessmentTypeId] = codes.id
    
 where map.LRValue  = @OriginalValue
 and red.Id is null

set @TotalRows = @@rowcount

end





GO
/****** Object:  StoredProcedure [dbo].[Resource.AssessmentType_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[Resource.AssessmentType_SelectedCodes] 444662
*/
CREATE PROCEDURE [dbo].[Resource.AssessmentType_SelectedCodes]
    @ResourceIntId int

As
SELECT distinct
	code.Id, code.Title, code.[Description]
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.AssessmentType] code
Left Join dbo.[Resource.AssessmentType] rpw on code.Id = rpw.AssessmentTypeId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1
Order by 2


GO
/****** Object:  StoredProcedure [dbo].[Resource.AssessmentTypeDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [dbo].[Resource.AssessmentTypeDelete]
            @ResourceIntId int, 
            @AssessmentTypeId int, 
            @Id int
As
If @ResourceIntId = 0   SET @ResourceIntId = NULL 
If @AssessmentTypeId = 0   SET @AssessmentTypeId = NULL 
If @Id = 0   SET @Id = NULL 

DELETE FROM [Resource.AssessmentType]
WHERE (Id = @Id OR @Id is NULL)
AND (ResourceIntId = @ResourceIntId OR @ResourceIntId is NULL)
And (AssessmentTypeId = @AssessmentTypeId OR @AssessmentTypeId is NULL)



GO
/****** Object:  StoredProcedure [dbo].[Resource.AssessmentTypeInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [dbo].[Resource.AssessmentTypeInsert]
            @ResourceIntId int, 
            @AssessmentTypeId int,  
            @CreatedById int
            --,@ResourceId uniqueidentifier
As

If @CreatedById = 0   SET @CreatedById = NULL 
--If @ResourceId = ''   SET @ResourceId = NULL 
INSERT INTO [Resource.AssessmentType] (

    ResourceIntId, 
    AssessmentTypeId, 
    CreatedById
    --,ResourceId
)
Values (

    @ResourceIntId, 
    @AssessmentTypeId, 
    @CreatedById
    --,@ResourceId
)
 
select SCOPE_IDENTITY() as Id


GO
/****** Object:  StoredProcedure [dbo].[Resource.AssessmentTypeSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [dbo].[Resource.AssessmentTypeSelect]
  @ResourceIntId int
As
SELECT 
    base.Id, 
    ResourceIntId, 
    AssessmentTypeId, codes.Title As AssessmentType,  
    base.Created, 
    base.CreatedById 
    
FROM [Resource.AssessmentType] base
inner join [Codes.AssessmentType] codes on base.AssessmentTypeId = codes.Id
where ResourceIntId= @ResourceIntId
order by codes.Title



GO
/****** Object:  StoredProcedure [dbo].[Resource.Cluster_Import]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




/* =============================================
Description:      [Resource.Cluster_Import]
------------------------------------------------------
Modifications
=============================================

*/
CREATE PROCEDURE [dbo].[Resource.Cluster_Import]
          @ResourceIntId int,
          @ClusterId	    int,
          @OriginalValue  varchar(100)
          ,@TotalRows     int OUTPUT

As
declare @mapperId int
, @exists int
, @IsDuplicate bit
, @RecordCount int
, @SuppressOutput bit
, @KeywordId int

if @OriginalValue = '' set @OriginalValue = NULL
If @ClusterId = -1		SET @SuppressOutput = 1
else set @SuppressOutput  = 0
If @ClusterId < 1		SET @ClusterId = NULL 
  
If @OriginalValue is NULL and @ClusterId is null begin
  print 'no values provided'
  return -1
  end    


set @IsDuplicate= 0
set @TotalRows= 0
-- ==================================================================
-- Do keyword check.  If keyword found, delete keyword.
SELECT @KeywordId = Id
FROM [Resource.Keyword]
WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
	DELETE FROM [Resource.Keyword]
	WHERE Id = @KeywordId
END

-- ==================================================================

if @ClusterId is null begin
	print 'insert via Cluster'
	 -- so first check if mapping exists
	SELECT @ClusterId = isnull(base.id,0)
  FROM [dbo].[CareerCluster] base
  inner join [dbo].[Map.Cluster] mapper on base.Id = mapper.[CodeId]
  where mapper.LRValue = @OriginalValue
   
	If @ClusterId is null OR @ClusterId = 0	begin	
    --no mapping, write to exceptions table and return
    -- when called from interface, may need real message?
    if NOT exists(SELECT [ResourceIntId] FROM [dbo].[Audit.Cluster_Orphan] 
    where [ResourceIntId] = @ResourceIntId and [OriginalValue] = @OriginalValue) begin
      print '@@ no mapping, writing to audit table: ' + @OriginalValue
      INSERT INTO [dbo].[Audit.Cluster_Orphan]
           ([ResourceIntId]
           ,[OriginalValue])
      VALUES
           (@ResourceIntId
           ,@OriginalValue)
      
      end    
    else begin
      print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
      end

    --return -1
    if @SuppressOutput = 0
      select '' as Id, 0 As IsDuplicate
    end
	end

if @ClusterId > 0 begin
  print '[ClusterId] = ' + convert(varchar, @ClusterId)



-- exists check for dups, or allow fail on dup
  select @exists = base.[ResourceIntId]
	from [dbo].[Resource.Cluster] base
	where base.[ResourceIntId] = @ResourceIntId
	And base.[ClusterId] = @ClusterId
	
	if @exists is not NULL AND @exists != 0 begin
	  set @IsDuplicate= 1
	  print 'found duplicate'
	end
	else begin
    INSERT INTO [dbo].[Resource.Cluster]
	    ([ResourceIntId]
	    ,[ClusterId]
		,[CreatedById])

    select
	    @ResourceIntId, 
	    @ClusterId, 
		NULL
    end
  set @TotalRows = @@rowcount
  if @SuppressOutput = 0    
    select @@IDENTITY as Id, @IsDuplicate As IsDuplicate    
end

GO
/****** Object:  StoredProcedure [dbo].[Resource.Cluster_PopulateEnergy]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
select count(*) from [Resource]
go

SELECT top 1000
 base.[ResourceId]
      ,[ClusterId]
      ,[ResourceUrl]
      ,[Title]
      ,[Description]
      ,[Keywords]
      ,[Subjects]
  FROM [Resource.Version_Summary] base
  Left join [dbo].[Resource.Cluster] rclu 
          on base.ResourceId = rclu.ResourceId And rclu.[ClusterId] = 91
where rclu.[ClusterId] is not null
  
  
GO




EXECUTE [dbo].[Resource.Cluster_PopulateEnergy] 100, 91
go
EXECUTE [dbo].[Resource.Cluster_PopulateEnergy] 100, 1

*/

/*
Map clusters 

Notes
- should join to resource version to only target resources already in the pathway
- add a date or means to only target recent additions rather than the whole database
*/
CREATE PROCEDURE [dbo].[Resource.Cluster_PopulateEnergy]
            @MaxRecords int
            ,@clusterId int

As
begin 
          
Declare 
@MappedClusterId int
,@OriginalValue varchar(200)
,@Filter varchar(200)
,@cntr int
--,@clusterId int
--,@interval int
,@debugLevel int

--set @interval= 25
set @cntr= 0
set @clusterId = 91
set @debugLevel= 10

select 'started',  getdate()
	-- Loop thru and call proc
	DECLARE thisCursor CURSOR FOR
      SELECT  OriginalValue, MappedClusterId
      FROM         [Map.CareerCluster]
      where IsActive= 1 AND (MappedClusterId = @clusterId or @clusterId = 0)

	OPEN thisCursor
	FETCH NEXT FROM thisCursor INTO @OriginalValue, @MappedClusterId
	WHILE @@FETCH_STATUS = 0 BEGIN
	  set @cntr = @cntr+ 1
	  if @MaxRecords > 0 AND @cntr > @MaxRecords begin
		  print '### Early exit based on @MaxRecords = ' + convert(varchar, @MaxRecords)
		  select 'exiting',  getdate()
		  set @cntr = @cntr - 1
		  BREAK
		  End	  
		  
		set @Filter = '%' + ltrim(rtrim(@OriginalValue)) + '%'
		
    -- === via title =======================================================  		
    INSERT INTO [dbo].[Resource.Cluster]
               ([ResourceId]
               ,[ClusterId])
    SELECT distinct lrs.ResourceId,  @MappedClusterId
    FROM dbo.[Resource.Version_Summary] lrs 
    left join [dbo].[Resource.Cluster] rc on lrs.ResourceId = rc.ResourceId AND rc.ClusterId = @MappedClusterId
    where rc.[ClusterId] is null
    And (lrs.Title like @Filter 
         OR lrs.[Description] like @Filter
         OR lrs.Subjects like @Filter
         OR lrs.Keywords like @Filter
         )
    if @@rowcount > 0 print '&& match found on title'

    -- === via subjects =======================================================    
    --INSERT INTO [dbo].[Resource.Cluster]
    --           ([ResourceId]
    --           ,[ClusterId])
    --SELECT distinct lrs.ResourceId,  @MappedClusterId
    --FROM dbo.[Resource.Subject] lrs 
    --left join [dbo].[Resource.Cluster] rc on lrs.ResourceId = rc.ResourceId AND rc.ClusterId = @MappedClusterId
    --where rc.[ClusterId] is null
    --And lrs.SubjectsIdx like @Filter
    --if @@rowcount > 0 print '&& match found on subjects'
  	 
  	-- === via keywords =======================================================
   -- INSERT INTO [dbo].[Resource.Cluster]
   --            ([ResourceId]
   --            ,[ClusterId])
   -- SELECT distinct lrs.ResourceId,  @MappedClusterId
   -- FROM dbo.[Resource.Keyword] lrs 
   -- left join [dbo].[Resource.Cluster] rc on lrs.ResourceId = rc.ResourceId AND rc.ClusterId = @MappedClusterId
   -- where rc.[ClusterId] is null
   -- And lrs.KeywordsIdx like @Filter
  	--if @@rowcount > 0 print '&& match found on keywords'
  	 
		FETCH NEXT FROM thisCursor INTO @OriginalValue, @MappedClusterId
	END
	CLOSE thisCursor
	DEALLOCATE thisCursor
	select 'completed',  getdate()
  select 'processed records: ' + convert(varchar, @cntr)
  
  
end


GO
/****** Object:  StoredProcedure [dbo].[Resource.Cluster_PopulateFromMapping]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*

select count(*) from [Resource]
go

SELECT top 1000
 base.[ResourceId]
      ,[ClusterId]
      ,[ResourceUrl]
      ,[Title]
      ,[Description]
      ,[Keywords]
      ,[Subjects]
  FROM [Resource.Version_Summary] base
  Left join [dbo].[Resource.Cluster] rclu 
          on base.ResourceId = rclu.ResourceId And rclu.[ClusterId] = 91
where rclu.[ClusterId] is not null
  
  
GO

UPDATE [dbo].[Map.CareerCluster]
   SET [IsActive] = 1
 WHERE Id < 72
GO




EXECUTE [dbo].[Resource.Cluster_PopulateFromMapping] 0, 91


set statistics time on   
EXECUTE [dbo].[Resource.Cluster_PopulateFromMapping] 0, 11
set statistics time off    

*/

/*
Map clusters 
Loop thru defined mapping values, and apply to existing resources

Notes
- should join to resource version to only target resources already in the pathway
- add a date or means to only target recent additions rather than the whole database
*/
CREATE PROCEDURE [dbo].[Resource.Cluster_PopulateFromMapping]
            @MaxRecords int
            ,@clusterId int

As
begin 
          
Declare 
@MapId int
,@MappedClusterId int
,@FilterValue varchar(200)
,@Filter varchar(200)
,@cntr int
--,@clusterId int
,@interval int
,@debugLevel int
,@affectedCount int
,@totalCount int
set @interval= 25
set @cntr= 0
--set @clusterId = 91
set @debugLevel= 10
set @affectedCount= 0
set @totalCount= 0

SET NOCOUNT ON;
-- ===============================================
select 'started',  getdate()
	-- Loop thru and call proc
	DECLARE thisCursor CURSOR FOR
      SELECT  FilterValue, MappedClusterId, Id
      FROM   [Map.CareerCluster]
      where IsActive= 1 
      AND (MappedClusterId = @clusterId or @clusterId = 0)

	OPEN thisCursor
	FETCH NEXT FROM thisCursor INTO @FilterValue, @MappedClusterId, @MapId
	WHILE @@FETCH_STATUS = 0 BEGIN
	  set @cntr = @cntr+ 1
	  if @MaxRecords > 0 AND @cntr > @MaxRecords begin
		  print '### Early exit based on @MaxRecords = ' + convert(varchar, @MaxRecords)
		  select 'exiting',  getdate()
		  set @cntr = @cntr - 1
		  BREAK
		  End	  
		  
    select @cntr As Cntr,convert(varchar, @MappedClusterId) As ClusterId,@FilterValue	As Filter
    print convert(varchar, @cntr) + '. Cluster/Filter: ' + convert(varchar, @MappedClusterId) + ' - ' + @FilterValue		
		set @Filter = '%' + ltrim(rtrim(@FilterValue)) + '%'
		
    -- === via title =======================================================  		
    INSERT INTO [dbo].[Resource.Cluster]
               ([ResourceIntId]
               ,[ClusterId])
    SELECT distinct lrs.ResourceIntId,  @MappedClusterId
    FROM dbo.[Resource.Version_Summary] lrs 
    left join [dbo].[Resource.Cluster] rc on lrs.ResourceIntId = rc.ResourceIntId AND rc.ClusterId = @MappedClusterId
	--Left Join [dbo].[Resource.KeywordsCsvList] keys on lrs.ResourceIntId = keys.ResourceIntId
	--Left Join [dbo].[Resource.SubjectsCsvList] subs on lrs.ResourceIntId = subs.ResourceIntId
    where rc.[ClusterId] is null
    And (lrs.Title like @Filter 
         OR lrs.[Description] like @Filter
        -- OR keys.Keywords like @Filter
        -- OR subs.Subjects like @Filter
         )
    set @affectedCount = @@rowcount
    if @affectedCount is not null AND @affectedCount > 0 begin
      set @totalCount= @totalCount + @affectedCount
      print '-> match found for cluster using filter. Count: '  
              + convert(varchar, @affectedCount)  + '. #' 
              + convert(varchar, @MappedClusterId) + ' - ' + @Filter
      --if ((@dupsCntr / @interval) * @interval) = @dupsCntr 
      
      end

    -- todo - once run, could arbitrarily set to false?
    --      - probably only useful for initial runs, as in prod will run regularly against new records
    
    
    -- === via subjects =======================================================    
    INSERT INTO [dbo].[Resource.Cluster]
               ([ResourceIntId]
               ,[ClusterId])
    SELECT distinct lrs.ResourceIntId,  @MappedClusterId
    FROM dbo.[Resource.Subject] lrs 
    left join [dbo].[Resource.Cluster] rc on lrs.ResourceIntId = rc.ResourceIntId AND rc.ClusterId = @MappedClusterId
    where rc.[ClusterId] is null
    And lrs.Subject like @Filter

	set @affectedCount = @@rowcount
    if @affectedCount is not null AND @affectedCount > 0 begin
      set @totalCount= @totalCount + @affectedCount
      print '$$$$$$$ match found on subjects for cluster. Count: '  
              + convert(varchar, @affectedCount)  + '. #' 
	  end
    
  	 
  	-- === via keywords =======================================================
    INSERT INTO [dbo].[Resource.Cluster]
               ([ResourceIntId]
               ,[ClusterId])
    SELECT distinct lrs.ResourceIntId,  @MappedClusterId
    FROM dbo.[Resource.Keyword] lrs 
    left join [dbo].[Resource.Cluster] rc on lrs.ResourceIntId = rc.ResourceIntId AND rc.ClusterId = @MappedClusterId
    where rc.[ClusterId] is null
    And lrs.Keyword like @Filter

  	set @affectedCount = @@rowcount
    if @affectedCount is not null AND @affectedCount > 0 begin
      set @totalCount= @totalCount + @affectedCount
      print 'KKKKKKK match found on keywords for cluster. Count: '  
              + convert(varchar, @affectedCount)  + '. #' 
	  end
    
  	 
		FETCH NEXT FROM thisCursor INTO @FilterValue, @MappedClusterId, @MapId
	END
	CLOSE thisCursor
	DEALLOCATE thisCursor
	select 'completed',  getdate()
  select 'processed records: ' + convert(varchar, @cntr)
  select 'Clusters created: ' + convert(varchar, @totalCount)
  
  
end


GO
/****** Object:  StoredProcedure [dbo].[Resource.Cluster_PopulateResourceFromMapping]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*

select count(*) from [Resource]
go

SELECT top 1000
 base.[ResourceId]
      ,[ClusterId]
      ,[ResourceUrl]
      ,[Title]
      ,[Description]
      ,[Keywords]
      ,[Subjects]
  FROM [Resource.Version_Summary] base
  Left join [dbo].[Resource.Cluster] rclu 
          on base.ResourceId = rclu.ResourceId And rclu.[ClusterId] = 91
where rclu.[ClusterId] is not null
  
  
GO

UPDATE [dbo].[Map.CareerCluster]
   SET [IsActive] = 1
 WHERE Id < 72
GO




EXECUTE [dbo].[Resource.Cluster_PopulateFromMapping] 0, 91



EXECUTE [dbo].[Resource..Cluster_PopulateResourceFromMapping] 450295, 0


*/

/*
Map clusters 
Loop thru defined mapping values, and apply to existing resources

Notes
- should join to resource version to only target resources already in the pathway
- add a date or means to only target recent additions rather than the whole database
*/
CREATE PROCEDURE [dbo].[Resource.Cluster_PopulateResourceFromMapping]
            @ResourceIntId int
            ,@clusterId int

As
begin 
          
Declare 
@MapId int
,@MappedClusterId int
,@FilterValue varchar(200)
,@Filter varchar(200)
,@cntr int
--,@clusterId int
,@interval int
,@debugLevel int
,@affectedCount int
,@totalCount int
set @interval= 25
set @cntr= 0
--set @clusterId = 91
set @debugLevel= 10
set @affectedCount= 0
set @totalCount= 0

SET NOCOUNT ON;
-- ===============================================
select 'started',  getdate()
	-- Loop thru and call proc
	DECLARE thisCursor CURSOR FOR
      SELECT  FilterValue, MappedClusterId, Id
      FROM   [Map.CareerCluster]
      where IsActive= 1 
      AND (MappedClusterId = @clusterId or @clusterId = 0)

	OPEN thisCursor
	FETCH NEXT FROM thisCursor INTO @FilterValue, @MappedClusterId, @MapId
	WHILE @@FETCH_STATUS = 0 BEGIN
	  set @cntr = @cntr+ 1  
		  
    select @cntr As Cntr,convert(varchar, @MappedClusterId) As ClusterId,@FilterValue	As Filter
    print convert(varchar, @cntr) + '. Cluster/Filter: ' + convert(varchar, @MappedClusterId) + ' - ' + @FilterValue		
		set @Filter = '%' + ltrim(rtrim(@FilterValue)) + '%'
		
    -- === via title =======================================================  		
    INSERT INTO [dbo].[Resource.Cluster]
               ([ResourceIntId]
               ,[ClusterId])
    SELECT distinct lrs.ResourceIntId,  @MappedClusterId
    FROM dbo.[Resource.Version_Summary] lrs 
    left join [dbo].[Resource.Cluster] rc on lrs.ResourceIntId = rc.ResourceIntId AND rc.ClusterId = @MappedClusterId
    where lrs.ResourceIntId = @ResourceIntId 
	AND rc.[ClusterId] is null
    And (lrs.Title like @Filter 
         OR lrs.[Description] like @Filter
         )
    set @affectedCount = @@rowcount
    if @affectedCount is not null AND @affectedCount > 0 begin
      set @totalCount= @totalCount + @affectedCount
      print '-> match found for cluster using filter. Count: '  
              + convert(varchar, @affectedCount)  + '. #' 
              + convert(varchar, @MappedClusterId) + ' - ' + @Filter
      --if ((@dupsCntr / @interval) * @interval) = @dupsCntr 
      
      end

    -- === via subjects =======================================================    
    INSERT INTO [dbo].[Resource.Cluster]
               ([ResourceIntId]
               ,[ClusterId])
    SELECT distinct lrs.ResourceIntId,  @MappedClusterId
    FROM dbo.[Resource.Subject] lrs 
    left join [dbo].[Resource.Cluster] rc on lrs.ResourceIntId = rc.ResourceIntId AND rc.ClusterId = @MappedClusterId
        where lrs.ResourceIntId = @ResourceIntId 
	AND rc.[ClusterId] is null
    And lrs.Subject like @Filter

	set @affectedCount = @@rowcount
    if @affectedCount is not null AND @affectedCount > 0 begin
      set @totalCount= @totalCount + @affectedCount
      print '$$$$$$$ match found on subjects for cluster. Count: '  
              + convert(varchar, @affectedCount)  + '. #' 
	  end
    
  	 
  	-- === via keywords =======================================================
    INSERT INTO [dbo].[Resource.Cluster]
               ([ResourceIntId]
               ,[ClusterId])
    SELECT distinct lrs.ResourceIntId,  @MappedClusterId
    FROM dbo.[Resource.Keyword] lrs 
    left join [dbo].[Resource.Cluster] rc on lrs.ResourceIntId = rc.ResourceIntId AND rc.ClusterId = @MappedClusterId
    where lrs.ResourceIntId = @ResourceIntId 
	AND rc.[ClusterId] is null
    And lrs.Keyword like @Filter

  	set @affectedCount = @@rowcount
    if @affectedCount is not null AND @affectedCount > 0 begin
      set @totalCount= @totalCount + @affectedCount
      print 'KKKKKKK match found on keywords for cluster. Count: '  
              + convert(varchar, @affectedCount)  + '. #' 
	  end
    
  	 
		FETCH NEXT FROM thisCursor INTO @FilterValue, @MappedClusterId, @MapId
	END
	CLOSE thisCursor
	DEALLOCATE thisCursor
	select 'completed',  getdate()
  select 'processed records: ' + convert(varchar, @cntr)
  select 'Clusters created: ' + convert(varchar, @totalCount)
  
  
end


GO
/****** Object:  StoredProcedure [dbo].[Resource.ClusterDelete2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.ClusterDelete2]
        @ResourceIntId int,
        @ClusterId int
        
As
DELETE FROM [Resource.Cluster] 
WHERE ClusterId = @ClusterId AND 
ResourceIntId = @ResourceIntId

GO
/****** Object:  StoredProcedure [dbo].[Resource.ClusterGet2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.ClusterGet2] 8, 13

*/
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 09/13/2012
-- Description:	Get a Resource.Cluster row
-- =============================================
CREATE PROCEDURE [dbo].[Resource.ClusterGet2]
	@ResourceIntId int,
	@ClusterId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT ResourceId, ResourceIntId, ClusterId
	FROM [Resource.Cluster]
	WHERE ResourceIntId = @ResourceIntId AND ClusterId = @ClusterId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.ClusterInsert2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

-- ====================================================================
DECLARE @RC int, @ResourceId uniqueidentifier,@ClusterId int
DECLARE @PropertyName varchar(100), @Value varchar(500)

set @ResourceId = '7F1427AF-ABB8-4CCD-AC00-000BB5EE7C30'
set @ClusterId = 1


EXECUTE @RC = [.[dbo].[Resource.ClusterInsert2]     @ResourceId  ,@ClusterId, 2
GO


*/
Create PROCEDURE [dbo].[Resource.ClusterInsert2]
            @ResourceIntId int, 
            @ClusterId int
            ,@CreatedById int

As
--declare @CreatedById int
--set @CreatedById = 0

if @ResourceIntId = 0	Set @ResourceIntId = NULL
If @ClusterId = 0				SET @ClusterId = NULL 
If @CreatedById = 0			SET @CreatedById = NULL 

if @ClusterId is null OR @ResourceIntId is null begin
	print 'Resource.ClusterInsert2 Error: Incomplete parameters were provided'
	RAISERROR('Resource.ClusterInsert2 Error: incomplete parameters were provided. Require Source ResourceIntId, and @ClusterId', 18, 1)    
	RETURN -1 
	end
	
	
INSERT INTO [dbo].[Resource.Cluster]
           (ResourceIntId
           ,[ClusterId], CreatedById)
Values (
	@ResourceIntId, 
	@ClusterId, @CreatedById
)


GO
/****** Object:  StoredProcedure [dbo].[Resource.ClusterSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[Resource.ClusterSelect] 'E74B0BEF-4074-4937-81D6-0000C7FC4EA2'

[Resource.ClusterSelect] 'B0BDDDF1-75DA-4D6D-8555-0027D1ACCC4A'

*/
CREATE PROCEDURE [dbo].[Resource.ClusterSelect]
    @ResourceId varchar(50)
As
SELECT base.[ResourceIntId]
      ,base.[ClusterId], cc.IlPathwayName As Cluster
  FROM [dbo].[Resource.Cluster] base
  inner join CareerCluster cc on base.ClusterId = cc.Id
where base.ResourceId = @ResourceId  
Order by cc.IlPathwayName


GO
/****** Object:  StoredProcedure [dbo].[Resource.ClusterSelect_SelectedCodes2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[Resource.ClusterSelect_SelectedCodes2] 8
*/
CREATE PROCEDURE [dbo].[Resource.ClusterSelect_SelectedCodes2]
    @ResourceIntId int
As
SELECT 
	base.Id, base.IlPathwayName As Title, base.[Description]
--    ,ResourceId
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as HasCluster	
FROM [dbo].CareerCluster base
Left Join [Resource.Cluster] rpw on base.Id = rpw.ClusterId
		and rpw.ResourceIntId = @ResourceIntId
where IsIlPathway = 1

Order by base.IlPathwayName

GO
/****** Object:  StoredProcedure [dbo].[Resource.ClusterSelect2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[Resource.ClusterSelect2] 8



*/
CREATE PROCEDURE [dbo].[Resource.ClusterSelect2]
    @ResourceIntId int
As
SELECT ResourceIntId
      ,base.[ClusterId], cc.IlPathwayName As Cluster
  FROM [dbo].[Resource.Cluster] base
  inner join CareerCluster cc on base.ClusterId = cc.Id
where base.ResourceIntId = @ResourceIntId  
Order by cc.IlPathwayName


GO
/****** Object:  StoredProcedure [dbo].[Resource.Comment_Import]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/28/2013
-- Description:	Import Comment from Learning Registry
-- mods
-- 14-10-10 mparsons - removed resourceId from insert
-- =============================================
CREATE PROCEDURE [dbo].[Resource.Comment_Import] 
	@ResourceIntId int,
	@Comment varchar(max),
	@IsActive bit,
	@Created datetime,
	@CreatedBy varchar(100),
	@ResourceId uniqueidentifier,
	@Commenter varchar(500),
	@DocId varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    IF @Comment IS NULL OR @Comment = '' BEGIN
		PRINT 'Silly person.  You forgot the comment!'
		RETURN -1
	END
	
	IF @ResourceIntId = 0 SET @ResourceIntId = NULL
	
	DECLARE @RowCount int
	SELECT @RowCount = COUNT(*)
	FROM [Resource.Comment]
	WHERE ResourceId = @ResourceId AND DocId = @DocId

	IF @RowCount = 0 BEGIN
		INSERT INTO [Resource.Comment] (ResourceIntId, Comment, IsActive, Created, CreatedBy, Commenter, DocId)
		VALUES (@ResourceIntId,@Comment, @IsActive, @Created, @CreatedBy, @Commenter, @DocId)
	END
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.CommentDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.CommentDelete]
        @Id int
As
DELETE FROM [Resource.Comment]
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.CommentInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.CommentInsert]
            @ResourceIntId int, 
            @Comment varchar(MAX), 
            @IsActive bit,
            @CreatedById int,
            @CreatedBy varchar(100),
            @Commenter varchar(500)
As
--@ResourceId uniqueidentifier,
If @CreatedById = 0   SET @CreatedById = NULL 
If @CreatedBy = ''   SET @CreatedBy = NULL 
If @Commenter = ''   SET @Commenter = NULL 

INSERT INTO [Resource.Comment] (

    ResourceIntId, 
    Comment, 
    IsActive,
    CreatedById,
    CreatedBy,
    Commenter
)
Values (

    @ResourceIntId, 
    @Comment,  
    @IsActive,
    @CreatedById,
    @CreatedBy,
    @Commenter
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.CommentSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.CommentSelect]
  @ResourceIntId int
As
--@ResourceId varchar(40)
SELECT 
    Id, 
    --ResourceId, 
    ResourceIntId, 
    Comment, IsActive,
    Created, 
    CreatedById,
    CreatedBy,
    Commenter
FROM [Resource.Comment]
where ResourceIntId = @ResourceIntId 
Order by created desc


GO
/****** Object:  StoredProcedure [dbo].[Resource.ConditionsOfUse_Select]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.ConditionsOfUse_Select]
As
SELECT	[Id],
		[Summary] AS [Title],
		[Title] AS [Description],
		[Url],
		[IconUrl],
		[MiniIconUrl]

FROM [ConditionOfUse]

WHERE [IsActive] = 1


GO
/****** Object:  StoredProcedure [dbo].[Resource.DetailViewInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/5/2013
-- Description:	Insert a Resource.DetailView row
-- Modifications:
-- 14/02/03 mparsons - removed updating of ViewCount
-- =============================================
CREATE PROCEDURE [dbo].[Resource.DetailViewInsert]
	@ResourceIntId int,
	@CreatedById int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @ReturnValue int
	INSERT INTO [Resource.DetailView] (ResourceIntId, Created, CreatedById)
	VALUES (@ResourceIntId, GETDATE(), @CreatedById)
	
	SET @ReturnValue = @@IDENTITY
	
	--UPDATE [Resource]
	--SET ViewCount = ViewCount + 1
	--WHERE Id = @ResourceIntId
	
	SELECT @ReturnValue AS Id	
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.EducationUse_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.EducationUse_SelectedCodes] 10
*/
CREATE PROCEDURE [dbo].[Resource.EducationUse_SelectedCodes]
    @ResourceIntId int
    --    @ResourceId uniqueidentifier
As
SELECT distinct
	code.Id, code.Title, code.[Description]
	--code.EdUseCategoryId as Category, 
   -- ,ResourceIntId
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.EducationalUse] code
Left Join [Resource.EducationUse] rpw on code.Id = rpw.EducationUseId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1
Order by code.Title ASC


GO
/****** Object:  StoredProcedure [dbo].[Resource.EducationUseDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.EducationUseDelete]
        @ResourceIntId int,
        @EducationUseId int
        --,@Id int
As
DELETE FROM [Resource.EducationUse]
WHERE ResourceId = @ResourceIntId  
AND EducationUseId = @EducationUseId

GO
/****** Object:  StoredProcedure [dbo].[Resource.EducationUseGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[Resource.EducationUseGet] 1, 0, 0 

*/
CREATE PROCEDURE [dbo].[Resource.EducationUseGet]
    @Id int,
    @ResourceIntId int, 
    @EducationUseId int 
As
If @Id = 0   SET @Id = NULL 
If @ResourceIntId = 0   SET @ResourceIntId = NULL 
If @EducationUseId = 0   SET @EducationUseId = NULL 

SELECT     
    Id, 
    ResourceIntId, 
    EducationUseId, 
    OriginalType, 
    Created, 
    ResourceId
FROM [Resource.EducationUse]
WHERE 
    (Id = @Id or @Id is null )
And (ResourceIntId = @ResourceIntId or @ResourceIntId is null )
And (EducationUseId = @EducationUseId or @EducationUseId is null )



GO
/****** Object:  StoredProcedure [dbo].[Resource.EducationUseImport]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*


-- ====================================================================
DECLARE @RC int, @ResourceIntId uniqueidentifier, @EducationUseId int, @OriginalValue varchar(100),@TotalRows     int

set @EducationUseId= 0
set @ResourceIntId = '7F1427AF-ABB8-4CCD-AC00-000BB5EE7C30'
set @OriginalValue = 'Syllabus'

set @ResourceIntId = 'c73542af-bdaf-4043-be18-417d5a68e6be'
set @OriginalValue = 'Image/jpg2'



EXECUTE @RC = [dbo].[Resource.EducationUseImport] 
   @ResourceIntId, @EducationUseId
  ,@OriginalValue, @totalRows OUTPUT

GO


*/
/* =============================================
Description:      [Resource.EducationUseImport]
------------------------------------------------------
Modifications
2013-08-29 jgrimmer - Code table is [Codes.EducationalUse] not [Codes.EducationUse].  Fixed.
=============================================

*/
CREATE PROCEDURE [dbo].[Resource.EducationUseImport]
          @ResourceIntId     int, 
          @EducationUseId	int,
          @OriginalValue  varchar(100)
          ,@TotalRows     int OUTPUT
          ,@ResourceRowId uniqueidentifier
As
declare @NewId uniqueidentifier
, @id int
, @mapperId int
, @exists int
, @IsDuplicate bit
, @RecordCount int
, @SuppressOutput bit
, @KeywordRowId uniqueidentifier

if @OriginalValue = '' set @OriginalValue = NULL
If @EducationUseId = -1		SET @SuppressOutput = 1
else set @SuppressOutput  = 0
If @TotalRows = -1		SET @SuppressOutput = 1
If @EducationUseId < 1		SET @EducationUseId = NULL 
  
If @OriginalValue is NULL and @EducationUseId is null begin
  print 'no values provided'
  return -1
  end    

set @IsDuplicate= 0
set @TotalRows= 0

-- ==================================================================
-- Do keyword check.  If keyword found, delete keyword.
--SELECT @KeywordRowId = RowId
--FROM [Resource.Keyword]
--WHERE ResourceId = @ResourceIntId AND Keyword = @OriginalValue
--IF @KeywordRowId IS NOT NULL AND @KeywordRowId <> '00000000-0000-0000-0000-000000000000' BEGIN
--	DELETE FROM [Resource.Keyword]
--	WHERE RowId = @KeywordRowId
--END

-- ==================================================================
if @EducationUseId is null begin
	print 'insert via EducationUse'
	 -- so first check if mapping exists
	SELECT @EducationUseId = isnull(base.id,0)
  FROM [dbo].[Codes.EducationalUse] base
  inner join [dbo].[Map.EducationUse] mapper on base.Id = mapper.[CodeId]
  where mapper.LRValue = @OriginalValue
   
	If @EducationUseId is null OR @EducationUseId = 0	begin	
    --no mapping, write to exceptions table and return
    -- when called from interface, may need real message?
    if NOT exists(SELECT ResourceIntId FROM [dbo].[Audit.EducationUse_Orphan] 
    where ResourceIntId= @ResourceIntId and [OriginalValue] = @OriginalValue) begin
      print '@@ no mapping, writing to audit table: ' + @OriginalValue
      INSERT INTO [dbo].[Audit.EducationUse_Orphan]
           (ResourceIntId
           ,[OriginalValue])
      VALUES
           (@ResourceIntId
           ,@OriginalValue)
      
      end    
    else begin
      print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
      end

    --return -1
    if @SuppressOutput = 0
      select '' as Id, 0 As IsDuplicate
    end
	end

if @EducationUseId > 0 begin
  if @SuppressOutput = 0
    print '[EducationUseId] = ' + convert(varchar, @EducationUseId)

  set @NewId= NEWID()
  if @SuppressOutput = 0
    print '@NewId: ' + convert(varchar(50), @NewId)

-- exists check for dups, or allow fail on dup
  select @exists = base.[Id]
	from [dbo].[Resource.EducationUse] base
	where base.[ResourceIntId] = @ResourceIntId
	And base.[EducationUseId] = @EducationUseId
	
	if @exists is not NULL AND @exists != '00000000-0000-0000-0000-000000000000' begin
	 -- set @NewId= @exists
	  set @IsDuplicate= 1
	  if @SuppressOutput = 0
	    print 'found duplicate'
	end
	else begin
    INSERT INTO [dbo].[Resource.EducationUse]
	    ([ResourceIntId]
	    ,EducationUseId
	    ,[OriginalType])

    select
	    @ResourceIntId, 
	    @EducationUseId, 
	    @OriginalValue
    end
  set @TotalRows = @@rowcount
  if @SuppressOutput = 0    
    select @NewId as Id, @IsDuplicate As IsDuplicate    
end




GO
/****** Object:  StoredProcedure [dbo].[Resource.EducationUseInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.EducationUseInsert]
            @ResourceIntId int, 
            @EducationUseId int, 
            @OriginalValue varchar(100), 
            @CreatedById int, 
            @ResourceRowId varchar(40)
As
If @ResourceIntId = 0     SET @ResourceIntId = NULL 
If @EducationUseId = 0    SET @EducationUseId = NULL 
If @CreatedById = 0       SET @CreatedById = NULL 
If @OriginalValue = ''    SET @OriginalValue = NULL 
If @ResourceRowId = ''    SET @ResourceRowId = NULL 

INSERT INTO [Resource.EducationUse] (

    ResourceIntId, 
    EducationUseId, 
    OriginalType, 
    Created, 
    CreatedById,
    ResourceId
)
Values (

    @ResourceIntId, 
    @EducationUseId, 
    @OriginalValue, 
    getdate(), 
    @CreatedById,
    @ResourceRowId
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.EducationUseSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.EducationUseSelect] 0, 2

*/
Create PROCEDURE [dbo].[Resource.EducationUseSelect]
	@ResourceIntId int,
	@EducationUseId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	IF @ResourceIntId = 0 SET @ResourceIntId = NULL
	IF @EducationUseId = 0 SET @EducationUseId = NULL

  SELECT 
      Id, 
      ResourceIntId, 
      EducationUseId, 
      OriginalType, 
      Created, 
      ResourceId
  FROM [Resource.EducationUse]
  where 
		(ResourceIntId = @ResourceIntId OR @ResourceIntId IS NULL) AND
		(EducationUseId = @EducationUseId OR @EducationUseId IS NULL)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.EvaluationGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.EvaluationGet]
    @Id int
As
SELECT     Id, 
    ResourceIntId, 
    Created, 
    CreatedById, 
    StandardId, 
    RubricId, 
    Value, 
    ScaleMin, 
    ScaleMax, 
    CriteriaInfo
FROM [Resource.Evaluation]
WHERE Id = @Id


GO
/****** Object:  StoredProcedure [dbo].[Resource.EvaluationInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.EvaluationInsert]
            @ResourceIntId int, 
            @CreatedById int, 
            @StandardId int, 
            @RubricId int, 
            @Value float, 
            @ScaleMin int, 
            @ScaleMax int, 
            @CriteriaInfo varchar(500)
As
If @ResourceIntId = 0   SET @ResourceIntId = NULL 
If @CreatedById = 0   SET @CreatedById = NULL 
If @StandardId = 0   SET @StandardId = NULL 
If @RubricId = 0   SET @RubricId = NULL 
If @Value = 0   SET @Value = NULL 
If @ScaleMin = 0   SET @ScaleMin = NULL 
If @ScaleMax = 0   SET @ScaleMax = NULL 
If @CriteriaInfo = ''   SET @CriteriaInfo = NULL 
INSERT INTO [Resource.Evaluation] (

    ResourceIntId, 
    Created, 
    CreatedById, 
    StandardId, 
    RubricId, 
    Value, 
    ScaleMin, 
    ScaleMax, 
    CriteriaInfo
)
Values (

    @ResourceIntId, 
    GETDATE(), 
    @CreatedById, 
    @StandardId, 
    @RubricId, 
    @Value, 
    @ScaleMin, 
    @ScaleMax, 
    @CriteriaInfo
)
 
select SCOPE_IDENTITY() as Id


GO
/****** Object:  StoredProcedure [dbo].[Resource.EvaluationSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.EvaluationSelect]
	@ResourceIntId int,
	@CreatedById int,
	@StandardId int,
	@RubricId int
As

	IF @ResourceIntId = 0 SET @ResourceIntId = NULL
	IF @CreatedById = 0 SET @CreatedById = NULL
	IF @StandardId = 0 SET @StandardId = NULL
	IF @RubricId = 0 SET @RubricId = NULL
	
SELECT 
    Id, 
    ResourceIntId, 
    Created, 
    CreatedById, 
    StandardId, 
    RubricId, 
    Value, 
    ScaleMin, 
    ScaleMax, 
    CriteriaInfo
FROM [Resource.Evaluation]
WHERE (ResourceIntId = @ResourceIntId OR @ResourceIntId IS NULL) AND
	(CreatedById = @CreatedById OR @CreatedById IS NULL) AND
	(StandardId = @StandardId OR @StandardId IS NULL) AND
	(RubricId = @RubricId OR @RubricId IS NULL)


GO
/****** Object:  StoredProcedure [dbo].[Resource.Format_Import]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*


-- ====================================================================
DECLARE @RC int, @ResourceIntId int, @ResourceFormatId int, @OriginalValue varchar(100),@TotalRows     int

set @ResourceFormatId= 0
set @OriginalValue = 'Syllabus'

set @ResourceIntId = 8
set @OriginalValue = 'Image/jpg2'

EXECUTE @RC = [dbo].[Resource.Format_Import] 
   @ResourceIntId, @ResourceFormatId
  ,@OriginalValue, @totalRows OUTPUT

GO


*/
/* =============================================
Description:      [Resource.Format_Import]
------------------------------------------------------
Modifications
2012-11-15 jgrimmer - Added keyword checking.  If exists as keyword, delete keyword.
2013-03-15 jgrimmer - Added @ResourceIntId
2013-04-19 jgrimmer - Modified keyword check to use ResourceIntId
2013-08-28 mparsons - removed ResourceId
=============================================

*/
CREATE PROCEDURE [dbo].[Resource.Format_Import]
          @ResourceIntId		int,
          @ResourceFormatId	    int,
          @OriginalValue		varchar(100)
          ,@TotalRows			int OUTPUT


As
declare @NewId uniqueidentifier
, @mapperId int
, @exists uniqueidentifier
, @IsDuplicate bit
, @RecordCount int
, @SuppressOutput bit
, @KeywordId int

if @OriginalValue = '' set @OriginalValue = NULL
If @ResourceFormatId = -1		SET @SuppressOutput = 1
else set @SuppressOutput  = 0
If @TotalRows = -1		SET @SuppressOutput = 1
If @ResourceFormatId < 1		SET @ResourceFormatId = NULL 
  
If @OriginalValue is NULL and @ResourceFormatId is null begin
  print 'no values provided'
  return -1
  end    

set @IsDuplicate= 0
set @TotalRows= 0

-- ==================================================================
-- Do keyword check.  If keyword found, delete keyword.
SELECT @KeywordId = Id
FROM [Resource.Keyword]
WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
	DELETE FROM [Resource.Keyword]
	WHERE Id = @KeywordId
END
	
-- ==================================================================
if @ResourceFormatId is null begin
	if @SuppressOutput = 0
        print 'insert via Format'
	 -- so first check if mapping exists
	SELECT @ResourceFormatId = isnull(base.id,0)
  FROM [dbo].[Codes.ResourceFormat] base
  inner join [dbo].[Map.ResourceFormat] mapper on base.Id = mapper.[CodeId]
  where mapper.LRValue = @OriginalValue
   
	If @ResourceFormatId is null OR @ResourceFormatId = 0	begin	
    --no mapping, write to exceptions table and return
    -- when called from interface, may need real message?
    if NOT exists(SELECT [ResourceIntId] FROM [dbo].[Audit.ResourceFormat_Orphan] 
    where [ResourceIntId]= @ResourceIntId and [OriginalValue] = @OriginalValue) begin
      if @SuppressOutput = 0
        print '@@ no mapping, writing to audit table: ' + @OriginalValue
      INSERT INTO [dbo].[Audit.ResourceFormat_Orphan]
           ([RowId]
           ,[ResourceIntId]
           ,[OriginalValue])
      VALUES
           (newId()
           ,@ResourceIntId
           ,@OriginalValue)
      
      end    
    else begin
      if @SuppressOutput = 0
        print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
      end

    --return -1
    if @SuppressOutput = 0
      select '' as Id, 0 As IsDuplicate
    end
	end

if @ResourceFormatId > 0 begin
  if @SuppressOutput = 0
        print '[FormatId] = ' + convert(varchar, @ResourceFormatId)

  set @NewId= NEWID()
  if @SuppressOutput = 0
        print '@NewId: ' + convert(varchar(50), @NewId)

-- exists check for dups, or allow fail on dup
  select @exists = base.[RowId]
	from [dbo].[Resource.Format] base
	where base.[ResourceIntId] = @ResourceIntId
	And base.[CodeId] = @ResourceFormatId
	
	if @exists is not NULL AND @exists != '00000000-0000-0000-0000-000000000000' begin
	  set @NewId= @exists
	  set @IsDuplicate= 1
	  if @SuppressOutput = 0
        print 'found duplicate'
	end
	else begin
    INSERT INTO [dbo].[Resource.Format]
	    ([RowId]
	    ,[CodeId]
	    ,OriginalValue
	    ,ResourceIntId)

    select
	    @NewId,
	    @ResourceFormatId, 
	    @OriginalValue,
	    @ResourceIntId
    end
  set @TotalRows = @@rowcount
  if @SuppressOutput = 0    
    select @NewId as Id, @IsDuplicate As IsDuplicate    
end




GO
/****** Object:  StoredProcedure [dbo].[Resource.Format_ImportFromMappingOrphan]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
Attempt to import Resource.Format from the Audit.ResourceFormat_Orphan table
this would be run on demand after attempted cleanups of the mapping
We also need a process to permanently ignore some mappings
*/


-- 2013-03-13 jgrimmer - Added ResourceIntId
CREATE PROCEDURE [dbo].[Resource.Format_ImportFromMappingOrphan]
      @MaxRecords int,
      @DebugLevel int

As
begin 
          
Declare 
@ResourceIntId int
,@RowId uniqueidentifier
,@Value varchar(200)
,@MappedCodeId int
,@cntr int
,@existsCntr int
,@totalRows int

set @cntr = 0
set @existsCntr = 0

select 'started',  getdate()
	-- Loop thru and call proc
	DECLARE thisCursor CURSOR FOR
	SELECT base.RowId, base.[ResourceIntId], base.[OriginalValue] As Value, isnull(map.CodeId, -1)
	  FROM [dbo].[Audit.ResourceFormat_Orphan] base
	  Inner join [Map.ResourceFormat] map on base.[OriginalValue] = map.LRValue
	  Inner join [dbo].[Codes.ResourceFormat] codes on map.CodeId = codes.Id
	  Inner join [Resource] ON base.ResourceIntId = [Resource].Id
	  where 
		  (base.IsActive is null OR base.IsActive = 1)
	--AND map.CodeId = 4      
	  And (base.[FoundMapping] is null OR base.[FoundMapping] = 0)

  --?? if not tried before, but don't want to do all. actually ok, as we are doing a join
  -- still we want to be able to ignore stuff that won't be mapped
  -- maybe if the create date and last run date are X days apart, set the ignore flag

 
	OPEN thisCursor
	FETCH NEXT FROM thisCursor INTO @RowId, @ResourceIntId, @Value, @MappedCodeId
	WHILE @@FETCH_STATUS = 0 BEGIN
	  set @cntr = @cntr+ 1
	  if @MaxRecords > 0 AND @cntr > @MaxRecords begin
		  print '### Early exit based on @MaxRecords = ' + convert(varchar, @MaxRecords)
		  select 'exiting',  getdate()
		  set @cntr = @cntr - 1
		  BREAK
		  End	  
	  if @MaxRecords > 0 AND @cntr < 25  
	    print ' =======> ' +@Value

    set @totalRows = -1 
	  -- not sure what to use for schema here
    EXECUTE [dbo].[Resource.Format_Import] 
        @ResourceIntId, @MappedCodeId,
        @Value, @totalRows OUTPUT
        
		--if map was successful, either delete or at least mark
		if @totalRows > 0 begin
		  if @DebugLevel > 5
		    print 'appeared to be mapped, now mark/delete'

		  UPDATE [dbo].[Audit.ResourceFormat_Orphan]
			SET [LastRerunDate] = getdate()
			,[FoundMapping] = 1
		  WHERE [RowId] = @RowId
		  end
		else begin
		-- check if a entry already exists for the value (ie current results in a duplicate)
		
		  select @existsCntr = isnull(count(*),0)
			FROM [Map.ResourceFormat] map 
			inner join [dbo].[Codes.ResourceFormat] codes on map.[CodeId] = codes.Id
			inner join [dbo].[Resource.Format] red 
				  on @ResourceIntId = red.ResourceIntId 
				  and red.[CodeId] = codes.id
			 where map.LRValue  = @Value
			 group by red.ResourceIntId
         
    	if @existsCntr > 0 begin
		    if @DebugLevel > 5
		    print 'the orphan reference already exists, mark it'
		    UPDATE [dbo].[Audit.ResourceFormat_Orphan]
			  SET [LastRerunDate] = getdate()
			  ,[FoundMapping] = 1
			WHERE [RowId] = @RowId
		    end
		  else begin	
	      if @DebugLevel > 5
		    print 'not mapped, update rundate????'
			UPDATE [dbo].[Audit.ResourceFormat_Orphan]
			  SET [LastRerunDate] = getdate()
			WHERE [RowId] = @RowId
	      end
  		end
		
		FETCH NEXT FROM thisCursor INTO @RowId, @ResourceIntId, @Value, @MappedCodeId
	END
	CLOSE thisCursor
	DEALLOCATE thisCursor
	select 'completed',  getdate()
  select 'processed records: ' + convert(varchar, @cntr)
  
end


GO
/****** Object:  StoredProcedure [dbo].[Resource.Format_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.ResourceFormat_SelectedCodes] ''9B184F11-B032-4CDB-993D-00109FDCDF68''
*/
CREATE PROCEDURE [dbo].[Resource.Format_SelectedCodes]
    @ResourceIntId int
As
SELECT distinct
	code.Id, code.Title, code.[Description]
   -- ,ResourceIntId
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.ResourceFormat] code
Left Join [Resource.Format] rpw on code.Id = rpw.CodeId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1
Order by 2


GO
/****** Object:  StoredProcedure [dbo].[Resource.FormatDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.FormatDelete]
        @ResourceId uniqueidentifier,
        @CodeId int
As
DELETE FROM [Resource.Format]
WHERE ResourceId = @ResourceId  
AND CodeId = @CodeId

GO
/****** Object:  StoredProcedure [dbo].[Resource.FormatGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.FormatGet]
	@RowId uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT RowId, ResourceId, OriginalValue, CodeId
	FROM [Resource.Format]
	WHERE RowId = @RowId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.FormatGetById]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[Resource.FormatGetById]
	@ResourceIntId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT ResourceIntId, OriginalValue, CodeId,RowId, ResourceId, Created, CreatedById
	FROM [Resource.Format]
	WHERE ResourceIntId = @ResourceIntId
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.FormatInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[Resource.FormatInsert]
	@ResourceIntId int,
	@ResourceFormatId int,
	@OriginalValue varchar(100)
	,@CreatedById int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    
declare @NewId uniqueidentifier
, @exists uniqueidentifier
, @IsDuplicate bit
, @RecordCount int
, @SuppressOutput bit   --????
, @ErrorMsg varchar(100)

If @CreatedById = 0		SET @CreatedById = NULL
if @OriginalValue = '' set @OriginalValue = NULL
If @ResourceFormatId = -1		SET @SuppressOutput = 1
else set @SuppressOutput  = 0
If @ResourceFormatId < 1		SET @ResourceFormatId = NULL 
	

if @ResourceIntId is null begin
	print '[Resource.FormatInsert] Error: Incomplete parameters were provided'
	RAISERROR('[Resource.FormatInsert] Error: incomplete parameters were provided. Require Source ResourceIntId', 18, 1)    
	RETURN -1 
	end

If @OriginalValue is NULL and @ResourceFormatId is null begin
	print 'no values were provided'
	return -1
	end  


  set @IsDuplicate= 0
	SET @NewId = NEWID()
	PRINT '@NewId: ' + convert(varchar(50),@NewId)
	set @ErrorMsg = ''
	
-- ==================================================================
if @ResourceFormatId is null begin
	print 'insert via Format'
	 -- so first check if mapping exists
	SELECT @ResourceFormatId = isnull(base.id,0)
	  FROM [dbo].[Codes.ResourceFormat] base
	  inner join [dbo].[Map.ResourceFormat] mapper on base.Id = mapper.[CodeId]
	  where mapper.LRValue = @OriginalValue
   
	If @ResourceFormatId is null OR @ResourceFormatId = 0	begin	
    --no mapping, write to exceptions table and return
    -- when called from interface, may need real message?
    if NOT exists(SELECT [ResourceIntId] FROM [dbo].[Audit.ResourceFormat_Orphan] 
    where [ResourceIntId]= @ResourceIntId and [OriginalValue] = @OriginalValue) begin
      set @ErrorMsg = 'No mapping was found for parameter, writing to audit table'
      print '@@ no mapping, writing to audit table: ' + @OriginalValue
      INSERT INTO [dbo].[Audit.ResourceFormat_Orphan]
           ([RowId]
           ,[ResourceIntId]
           ,[OriginalValue])
      VALUES
           (newId()
           ,@ResourceIntId
           ,@OriginalValue)
      
      end    
    else begin
      print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
      set @ErrorMsg = 'No mapping was found for parameter, the value already exists in audit table'
      end

    --return -1
    if @SuppressOutput = 0
      select '' as Id, 0 As IsDuplicate, @ErrorMsg
    end
  end
	
if @ResourceFormatId > 0 begin
  print '[ResourceFormatId] = ' + convert(varchar, @ResourceFormatId)

  -- exists check for dups, or allow fail on dup
  select @exists = base.[RowId]
	from [dbo].[Resource.Format] base
	where ( base.[ResourceIntId] = @ResourceIntId )
	And base.[CodeId] = @ResourceFormatId
	
	if @exists is not NULL AND @exists != '00000000-0000-0000-0000-000000000000' begin
	  set @NewId= @exists
	  set @IsDuplicate= 1
	  print 'found duplicate'
	  set @ErrorMsg = 'The resource format already exists for this resource'
	  end
	else begin
    INSERT INTO [dbo].[Resource.Format]
	    ([RowId]
		,[ResourceIntId]
	    ,[CodeId]
	    ,OriginalValue
	    ,CreatedById)

    select
	    @NewId,
		@ResourceIntId,
	    @ResourceFormatId, 
	    @OriginalValue,
	    @CreatedById
    end

  if @SuppressOutput = 0    
    select @NewId as Id, @IsDuplicate As sDuplicate, @ErrorMsg As [Message]    
  end
  
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.FormatSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.FormatSelect]
	@ResourceId uniqueidentifier, @CodeId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    IF @CodeId = 0 SET @CodeId = NULL
    IF @ResourceId = '00000000-0000-0000-0000-000000000000' SET @ResourceId = NULL
    
    SELECT RowId, ResourceId, OriginalValue, CodeId
    FROM [Resource.Format]
    WHERE (ResourceId = @ResourceId OR @ResourceId IS NULL) AND
		(CodeId = @CodeId OR @CodeId IS NULL)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.FormatSelect2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.FormatSelect2]
	@ResourceIntId int
AS
BEGIN
	SET NOCOUNT ON;
    
    SELECT RowId, ResourceIntId, OriginalValue, CodeId, Created, CreatedById
    ,ResourceId
    FROM [Resource.Format]
    WHERE (ResourceIntId = @ResourceIntId)
    Order by CodeId
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.FormatUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.FormatUpdate]
	@RowId uniqueidentifier,
	@CodeId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    UPDATE [Resource.Format]
    SET CodeId = @CodeId
    WHERE RowId = @RowId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.GradeLevel_Delete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.GradeLevel_Delete]
        @ResourceIntId int,
        @GradeLevelId int,
        @Id  int
As
If @Id = 0   SET @Id = NULL 
If @ResourceIntId = 0   SET @ResourceIntId = NULL 
If @GradeLevelId = 0   SET @GradeLevelId = NULL 

DELETE FROM [Resource.GradeLevel]
WHERE 
    (ResourceIntId = @ResourceIntId AND GradeLevelId = @GradeLevelId)
OR  (Id = @Id)    

GO
/****** Object:  StoredProcedure [dbo].[Resource.GradeLevel_Import]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
Notes:
- may need to use a passed schema to help with mapping

- 2012-11-15 jgrimmer - Added checking for keyword.  If exists as keyword, delete from keywords.
- 2013-05-08 jgrimmer - Altered to use ResourceIntId instead of ResourceId
*/
CREATE PROCEDURE [dbo].[Resource.GradeLevel_Import]
            @ResourceIntId	int,
            @OriginalValue  varchar(500)
            ,@TotalRows     int OUTPUT

As
begin 
declare 
  @GradeLevelId int
  , @SuppressOutput bit
  , @RecordCount int
  
  set @SuppressOutput  = 0
  If @TotalRows = -1		SET @SuppressOutput = 1
  
  set @TotalRows = 0
  set @GradeLevelId = 0
  
If @OriginalValue = '' begin
  print 'no value provided'
  return -1
  end              
set @OriginalValue = Replace(@OriginalValue, '- ', '-')  
set @OriginalValue = Replace(@OriginalValue, ' -', '-')  
set @OriginalValue = Replace(@OriginalValue, '1-U', '1+') 
set @OriginalValue = Replace(@OriginalValue, 'U-U', '0-18') 
set @OriginalValue = Replace(@OriginalValue, '&gt;', '>')  
set @OriginalValue = Replace(@OriginalValue, '&lt;', '<')  
set @OriginalValue = Replace(@OriginalValue, '> ', '>') 
set @OriginalValue = Replace(@OriginalValue, '>1', '1+') 
set @OriginalValue = Replace(@OriginalValue, '>2', '2+') 
set @OriginalValue = Replace(@OriginalValue, '>3', '3+') 
set @OriginalValue = Replace(@OriginalValue, '>4', '4+') 
set @OriginalValue = Replace(@OriginalValue, '>5', '5+') 
set @OriginalValue = Replace(@OriginalValue, '>6', '6+') 
set @OriginalValue = Replace(@OriginalValue, '>7', '7+')
set @OriginalValue = Replace(@OriginalValue, '>8', '8+')
set @OriginalValue = Replace(@OriginalValue, '>9', '9+')
set @OriginalValue = Replace(@OriginalValue, '>10', '10+') 
set @OriginalValue = Replace(@OriginalValue, '>11', '11+') 
set @OriginalValue = Replace(@OriginalValue, '>12', '12+') 
set @OriginalValue = Replace(@OriginalValue, '>13', '13+') 
set @OriginalValue = Replace(@OriginalValue, '>14', '14+') 
set @OriginalValue = Replace(@OriginalValue, '>15', '15+') 
set @OriginalValue = Replace(@OriginalValue, '>16', '16+') 
set @OriginalValue = Replace(@OriginalValue, '>17', '17+') 
set @OriginalValue = Replace(@OriginalValue, '>18', '18+') 
set @OriginalValue = Replace(@OriginalValue, '>19', '19+') 
set @OriginalValue = Replace(@OriginalValue, '>20', '20+') 

set @OriginalValue = Replace(@OriginalValue, '03-', '3-') 
set @OriginalValue = Replace(@OriginalValue, '04-', '4-') 
set @OriginalValue = Replace(@OriginalValue, '05-', '5-') 
set @OriginalValue = Replace(@OriginalValue, '06-', '6-') 
set @OriginalValue = Replace(@OriginalValue, '07-', '7-') 
set @OriginalValue = Replace(@OriginalValue, '08-', '8-') 
set @OriginalValue = Replace(@OriginalValue, '09-', '9-') 

set @OriginalValue = Replace(@OriginalValue, '-U', '+') 
set @OriginalValue = Replace(@OriginalValue, '-+', '+') 

set @OriginalValue = Replace(@OriginalValue, '13-19', '13+') 
set @OriginalValue = Replace(@OriginalValue, '13-20', '13+') 
set @OriginalValue = Replace(@OriginalValue, '13-90', '13+') 
set @OriginalValue = Replace(@OriginalValue, '13-99', '13+') 
set @OriginalValue = Replace(@OriginalValue, '13-9999', '13+') 
set @OriginalValue = Replace(@OriginalValue, '13-U', '13+') 

set @OriginalValue = Replace(@OriginalValue, '14-19', '14+') 
set @OriginalValue = Replace(@OriginalValue, '14-20', '14+') 
set @OriginalValue = Replace(@OriginalValue, '14-90', '14+') 
set @OriginalValue = Replace(@OriginalValue, '14-99', '14+') 
set @OriginalValue = Replace(@OriginalValue, '14-9999', '14+') 

set @OriginalValue = Replace(@OriginalValue, '15-19', '15+') 
set @OriginalValue = Replace(@OriginalValue, '15-20', '15+') 
set @OriginalValue = Replace(@OriginalValue, '15-90', '15+') 
set @OriginalValue = Replace(@OriginalValue, '15-99', '15+') 
set @OriginalValue = Replace(@OriginalValue, '15-9999', '15+') 

set @OriginalValue = Replace(@OriginalValue, '16-19', '16+') 
set @OriginalValue = Replace(@OriginalValue, '16-20', '16+') 
set @OriginalValue = Replace(@OriginalValue, '16-90', '16+') 
set @OriginalValue = Replace(@OriginalValue, '16-99', '16+') 
set @OriginalValue = Replace(@OriginalValue, '16-9999', '16+') 

set @OriginalValue = Replace(@OriginalValue, '17-19', '17+') 
set @OriginalValue = Replace(@OriginalValue, '17-20', '17+') 
set @OriginalValue = Replace(@OriginalValue, '17-90', '17+') 
set @OriginalValue = Replace(@OriginalValue, '17-99', '17+') 
set @OriginalValue = Replace(@OriginalValue, '17-9999', '17+') 

set @OriginalValue = Replace(@OriginalValue, '18-18', '18+') 
set @OriginalValue = Replace(@OriginalValue, '18-20', '18+') 
set @OriginalValue = Replace(@OriginalValue, '18-90', '18+') 
set @OriginalValue = Replace(@OriginalValue, '18-99', '18+') 
set @OriginalValue = Replace(@OriginalValue, '18-9999', '18+') 

set @OriginalValue = Replace(@OriginalValue, '19-19', '19+') 
set @OriginalValue = Replace(@OriginalValue, '19-20', '19+') 
set @OriginalValue = Replace(@OriginalValue, '19-90', '19+') 
set @OriginalValue = Replace(@OriginalValue, '19-99', '19+') 
set @OriginalValue = Replace(@OriginalValue, '19-9999', '19+') 

set @OriginalValue = Replace(@OriginalValue, '20-20', '20+') 
set @OriginalValue = Replace(@OriginalValue, '20-90', '20+') 
set @OriginalValue = Replace(@OriginalValue, '20-99', '20+') 
set @OriginalValue = Replace(@OriginalValue, '20-9999', '20+') 


set @OriginalValue = Replace(@OriginalValue, '21-90', '21+') 
set @OriginalValue = Replace(@OriginalValue, '21-99', '21+') 
set @OriginalValue = Replace(@OriginalValue, '21-9999', '21+') 

set @OriginalValue = Replace(@OriginalValue, '0-3', '1+') 
set @OriginalValue = Replace(@OriginalValue, '0-16', '1+') 

set @OriginalValue = Replace(@OriginalValue, 'Nov-', '11-') 
set @OriginalValue = Replace(@OriginalValue, 'Dec-', '12-') 
set @OriginalValue = Replace(@OriginalValue, 'Oct-', '10-') 
set @OriginalValue = Replace(@OriginalValue, 'Sep-', '9-') 

set @OriginalValue = Replace(@OriginalValue, 'U-', '1-') 
-- ======================================================
-- If it exists as a keyword, blow it away because we're putting it in as an Education Level
DECLARE @KeywordId int
SELECT @KeywordId = Id
FROM [Resource.Keyword]
WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
	DELETE FROM [Resource.Keyword]
	WHERE Id = @KeywordId
END
-- ======================================================
 -- now check if mapping exists
select @RecordCount = isnull(Count(*),0)
  FROM [Map.GradeLevel] map 
  inner join [dbo].[Codes.GradeLevel] codes on map.CodeId = codes.Id
  --inner join [dbo].[Codes.GradeLevel] codes on map.MappedValue = codes.[Title]
 where map.OriginalValue  = @OriginalValue
 
If @RecordCount is null OR @RecordCount = 0	begin 
  --no mapping, write to exceptions table and return
  if NOT exists(SELECT [ResourceIntId] FROM [dbo].[Audit.GradeLevel_Orphan] 
    where [ResourceIntId]= @ResourceIntId and [OriginalValue] = @OriginalValue) begin
    print 'no mapping, writing to exceptions table: ' + @OriginalValue
    INSERT INTO [dbo].[Audit.GradeLevel_Orphan]
             ([RowId]
             ,[ResourceIntId]
             ,[OriginalValue])
       VALUES
             (newId()
             ,@ResourceIntId
             ,@OriginalValue)
    end
  return -1
  end
-- ======================================================
--mapping exists, need to handle already in target table 
--print 'doing insert'
  INSERT INTO [Resource.GradeLevel]
  (
	  ResourceIntId, 
	  GradeLevelId,
	  OriginalLevel
  )
  
select @ResourceIntId, isnull(codes.id,0), @OriginalValue
  FROM [Map.GradeLevel] map 
  inner join [dbo].[Codes.GradeLevel] codes on map.CodeId = codes.Id
  left join [dbo].[Resource.GradeLevel] red on @ResourceIntId = red.ResourceIntId and red.[GradeLevelId] = codes.id
    
 where map.OriginalValue  = @OriginalValue
 and red.Id is null

set @TotalRows = @@rowcount

end





GO
/****** Object:  StoredProcedure [dbo].[Resource.GradeLevel_ImportFromMappingOrphan]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
Attempt to import Resource.Format from the Audit.GradeLevel_Orphan table
this would be run on demand after attempted cleanups of the mapping
We also need a process to permanently ignore some mappings
*/



CREATE PROCEDURE [dbo].[Resource.GradeLevel_ImportFromMappingOrphan]
      @MaxRecords int,
      @DebugLevel int

As
begin 
          
Declare @ResourceId uniqueidentifier
,@ResourceIntId int
,@RowId uniqueidentifier
,@Value varchar(200)
,@MappedCodeId int
,@cntr int
,@existsCntr int
,@totalRows int

set @cntr = 0
set @existsCntr = 0

select 'started',  getdate()
	-- Loop thru and call proc
	DECLARE thisCursor CURSOR FOR
	SELECT base.RowId, base.[ResourceIntId], base.[OriginalValue] As Value, isnull(map.CodeId, -1)
  FROM [dbo].[Audit.GradeLevel_Orphan] base
  Inner join [Map.GradeLevel] map on base.[OriginalValue] = map.OriginalValue
  Inner join [dbo].[Codes.GradeLevel] codes on map.CodeId = codes.Id
  Inner join [Resource] ON base.ResourceIntId = [Resource].Id
  where 
      (base.IsActive is null OR base.IsActive = 1)
  And (base.[FoundMapping] is null OR base.[FoundMapping] = 0)
  --?? if not tried before, but don't want to do all. actually ok, as we are doing a join
  -- still we want to be able to ignore stuff that won't be mapped
  -- maybe if the create date and last run date are X days apart, set the ignore flag

 
	OPEN thisCursor
	FETCH NEXT FROM thisCursor INTO @RowId, @ResourceIntId, @Value, @MappedCodeId
	WHILE @@FETCH_STATUS = 0 BEGIN
	  set @cntr = @cntr+ 1
	  if @MaxRecords > 0 AND @cntr > @MaxRecords begin
		  print '### Early exit based on @MaxRecords = ' + convert(varchar, @MaxRecords)
		  select 'exiting',  getdate()
		  set @cntr = @cntr - 1
		  BREAK
		  End	  
	  if @MaxRecords > 0 AND @cntr < 25  
	    print ' =======> ' +@Value

    set @totalRows = -1 
	  -- not sure what to use for schema here
    EXECUTE [dbo].[Resource.GradeLevel_Import] 
        @ResourceIntId,
        @Value, @totalRows OUTPUT
        
		--if map was successful, either delete or at least mark
		if @totalRows > 0 begin
		  if @DebugLevel > 5
		    print 'appeared to be mapped, now mark/delete'
		  UPDATE [dbo].[Audit.GradeLevel_Orphan]
        SET [LastRerunDate] = getdate()
        ,[FoundMapping] = 1
      WHERE [RowId] = @RowId
		  end
		else begin
		-- check if a entry already exists for the value (ie current results in a duplicate)
		
		  select @existsCntr = isnull(count(*),0)
        FROM [Map.GradeLevel] map 
        inner join [dbo].[Codes.GradeLevel] codes on map.[CodeId] = codes.Id
        inner join [dbo].[Resource.GradeLevel] red 
              on @ResourceIntId = red.ResourceIntId 
              and red.[GradeLevelId] = codes.id
         where map.OriginalValue  = @Value
         group by red.ResourceIntId
         
    	if @existsCntr > 0 begin
		    if @DebugLevel > 5
		    print 'the orphan reference already exists, mark it'
		    UPDATE [dbo].[Audit.GradeLevel_Orphan]
          SET [LastRerunDate] = getdate()
          ,[FoundMapping] = 1
        WHERE [RowId] = @RowId
		    end
		  else begin	
	      if @DebugLevel > 5
		    print 'not mapped, update rundate????'
	      UPDATE [dbo].[Audit.GradeLevel_Orphan]
          SET [LastRerunDate] = getdate()
        WHERE [RowId] = @RowId
	      end
  		end
		
		FETCH NEXT FROM thisCursor INTO @RowId, @ResourceIntId, @Value, @MappedCodeId
	END
	CLOSE thisCursor
	DEALLOCATE thisCursor
	select 'completed',  getdate()
  select 'processed records: ' + convert(varchar, @cntr)
  
end


GO
/****** Object:  StoredProcedure [dbo].[Resource.GradeLevel_Insert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*

-- ====================================================================
DECLARE @RC int, @ResourceId uniqueidentifier,@GradeLevelId int
DECLARE @OriginalValue varchar(500)

set @ResourceId = '987b71d5-9ba8-4703-b885-a80564ed30f1'
set @GradeLevelId = 6
set @OriginalValue = ''

EXECUTE @RC = [dbo].[Resource.GradeLevel_Insert] 
   @ResourceId
  ,@GradeLevelId
  ,@OriginalValue



*/
/*
-- =============================================
-- Create date: 3/12/2013
-- Description:	Insert row into [Resource.GradeLevel] table
-- =============================================
modifications
12-11-05 mparsons - need to remove unknown grade level when inserting actual gl
*/
Create PROCEDURE [dbo].[Resource.GradeLevel_Insert]
            @ResourceIntId  int,
            @GradeLevelId   int,
            @CreatedById   int
           

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
  If @ResourceIntId = 0   SET @ResourceIntId = NULL 
  If @GradeLevelId = 0   SET @GradeLevelId = NULL 

  If @ResourceIntId is NULL OR @GradeLevelId is null begin
    print 'no values provided'
    return -1
    end    
  
  declare @RecordCount int
  
    --should ensure doesn't already exist
    select @RecordCount = isnull(Count(*),0)
    FROM [dbo].[Resource.GradeLevel] rel 
    where rel.ResourceIntId  = @ResourceIntId AND rel.GradeLevelId = @GradeLevelId 	
    If @RecordCount is null OR @RecordCount = 0	begin	
      INSERT INTO [Resource.GradeLevel]
      (
	      ResourceIntId, 
	      GradeLevelId,
	      CreatedById
      )
	    Values (
		    @ResourceIntId, 
		    @GradeLevelId, 
		    @CreatedById
	    )
	    SELECT SCOPE_IDENTITY() AS Id

	      
	    end
    else begin
    	print 'Duplicate GradeLevelId'
      SELECT 0 AS Id
      end
    
End


GO
/****** Object:  StoredProcedure [dbo].[Resource.GradeLevel_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.GradeLevel_SelectedCodes] 1518
*/
CREATE PROCEDURE [dbo].[Resource.GradeLevel_SelectedCodes]
    @ResourceIntId int

As
SELECT distinct
	code.Id, code.Title, code.[Description], code.[AliasValues]
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
	,code.[SortOrder]
FROM [dbo].[Codes.GradeLevel] code
Left Join dbo.[Resource.GradeLevel] rpw on code.Id = rpw.GradeLevelId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1
Order by code.[SortOrder]


GO
/****** Object:  StoredProcedure [dbo].[Resource.GradeLevelSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.GradeLevelSelect] 1518
*/
Create PROCEDURE [dbo].[Resource.GradeLevelSelect]
    @ResourceIntId int

As
SELECT [Id]
      ,[ResourceIntId]
      ,[GradeLevelId]
      ,[OriginalLevel]
      ,[Created]
      ,[CreatedById]
      ,[PathwaysEducationLevelId]
  FROM [dbo].[Resource.GradeLevel]
  WHERE (ResourceIntId = @ResourceIntId)

GO
/****** Object:  StoredProcedure [dbo].[Resource.GroupType_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.GroupType_SelectedCodes] 444662
*/
CREATE PROCEDURE [dbo].[Resource.GroupType_SelectedCodes]
    @ResourceIntId int
    --    @ResourceIntId uniqueidentifier
As
SELECT distinct
	code.Id, code.Title, code.[Description]
   -- ,ResourceId
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.GroupType] code
Left Join [Resource.GroupType] rpw on code.Id = rpw.GroupTypeId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1
Order by 2


GO
/****** Object:  StoredProcedure [dbo].[Resource.GroupTypeDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.GroupTypeDelete]
            @ResourceIntId int, 
            @GroupTypeId int, 
            @Id int
As
If @ResourceIntId = 0   SET @ResourceIntId = NULL 
If @GroupTypeId = 0   SET @GroupTypeId = NULL 
If @Id = 0   SET @Id = NULL 

DELETE FROM [Resource.GroupType]
WHERE (Id = @Id OR @Id is NULL)
AND (ResourceIntId = @ResourceIntId OR @ResourceIntId is NULL)
And (GroupTypeId = @GroupTypeId OR @GroupTypeId is NULL)


GO
/****** Object:  StoredProcedure [dbo].[Resource.GroupTypeImport]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



/*


-- ====================================================================
DECLARE @RC int, @ResourceIntId uniqueidentifier, @GroupTypeId int, @OriginalValue varchar(100),@TotalRows     int

set @GroupTypeId= 0
set @ResourceIntId = '7F1427AF-ABB8-4CCD-AC00-000BB5EE7C30'
set @OriginalValue = 'Syllabus'

set @ResourceIntId = 'c73542af-bdaf-4043-be18-417d5a68e6be'
set @OriginalValue = 'Image/jpg2'



EXECUTE @RC = [dbo].[Resource.EducationUseImport] 
   @ResourceIntId, @GroupTypeId
  ,@OriginalValue, @totalRows OUTPUT

GO


*/
/* =============================================
Description:      [Resource.GroupTypeImport]
------------------------------------------------------
Modifications
14-05-09 mparsons - removed ResourceRowId
=============================================

*/
CREATE PROCEDURE [dbo].[Resource.GroupTypeImport]
          @ResourceIntId     int, 
          @GroupTypeId	int,
          @OriginalValue  varchar(100)
          ,@TotalRows     int OUTPUT
         -- ,@ResourceRowId uniqueidentifier
As
declare @NewId uniqueidentifier
, @id int
, @mapperId int
, @exists int
, @IsDuplicate bit
, @RecordCount int
, @SuppressOutput bit
, @KeywordRowId uniqueidentifier

if @OriginalValue = '' set @OriginalValue = NULL
If @GroupTypeId = -1		SET @SuppressOutput = 1
else set @SuppressOutput  = 0
If @TotalRows = -1		SET @SuppressOutput = 1
If @GroupTypeId < 1		SET @GroupTypeId = NULL 
  
If @OriginalValue is NULL and @GroupTypeId is null begin
  print 'no values provided'
  return -1
  end    

set @IsDuplicate= 0
set @TotalRows= 0

-- ==================================================================
-- Do keyword check.  If keyword found, delete keyword.
--SELECT @KeywordRowId = RowId
--FROM [Resource.Keyword]
--WHERE ResourceId = @ResourceIntId AND Keyword = @OriginalValue
--IF @KeywordRowId IS NOT NULL AND @KeywordRowId <> '00000000-0000-0000-0000-000000000000' BEGIN
--	DELETE FROM [Resource.Keyword]
--	WHERE RowId = @KeywordRowId
--END

-- ==================================================================
if @GroupTypeId is null begin
	print 'insert via EducationUse'
	 -- so first check if mapping exists
	SELECT @GroupTypeId = isnull(base.id,0)
  FROM [dbo].[Codes.GroupType] base
  inner join [dbo].[Map.GroupType] mapper on base.Id = mapper.[CodeId]
  where mapper.LRValue = @OriginalValue
   
	If @GroupTypeId is null OR @GroupTypeId = 0	begin	
    --no mapping, write to exceptions table and return
    -- when called from interface, may need real message?
    if NOT exists(SELECT ResourceIntId FROM [dbo].[Audit.GroupType_Orphan] 
    where ResourceIntId= @ResourceIntId and [OriginalValue] = @OriginalValue) begin
      print '@@ no mapping, writing to audit table: ' + @OriginalValue
      INSERT INTO [dbo].[Audit.GroupType_Orphan]
           (ResourceIntId
           ,[OriginalValue])
      VALUES
           (@ResourceIntId
           ,@OriginalValue)
      
      end    
    else begin
      print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
      end

    --return -1
    if @SuppressOutput = 0
      select '' as Id, 0 As IsDuplicate
    end
	end

if @GroupTypeId > 0 begin
  if @SuppressOutput = 0
    print '[GroupTypeId] = ' + convert(varchar, @GroupTypeId)

  set @NewId= NEWID()
  if @SuppressOutput = 0
    print '@NewId: ' + convert(varchar(50), @NewId)

-- exists check for dups, or allow fail on dup
  select @exists = base.[Id]
	from [dbo].[Resource.GroupType] base
	where base.[ResourceIntId] = @ResourceIntId
	And base.[GroupTypeId] = @GroupTypeId
	
	if @exists is not NULL AND @exists != 0 begin
	 -- set @NewId= @exists
	  set @IsDuplicate= 1
	  if @SuppressOutput = 0
	    print 'found duplicate'
	end
	else begin
    INSERT INTO [dbo].[Resource.GroupType]
	    ([ResourceIntId]
	    ,GroupTypeId)

    select
	    @ResourceIntId, 
	    @GroupTypeId 
    end
  set @TotalRows = @@rowcount
  if @SuppressOutput = 0    
    select @NewId as Id, @IsDuplicate As IsDuplicate    
end






GO
/****** Object:  StoredProcedure [dbo].[Resource.GroupTypeInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.GroupTypeInsert]
            @ResourceIntId int, 
            @GroupTypeId int,  
            @CreatedById int
            --,@ResourceId uniqueidentifier
As

If @CreatedById = 0   SET @CreatedById = NULL 
--If @ResourceId = ''   SET @ResourceId = NULL 
INSERT INTO [Resource.GroupType] (

    ResourceIntId, 
    GroupTypeId, 
    CreatedById
    --,ResourceId
)
Values (

    @ResourceIntId, 
    @GroupTypeId, 
    @CreatedById
    --,@ResourceId
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.GroupTypeSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.GroupTypeSelect]
  @ResourceIntId int
As
SELECT 
    base.Id, 
    ResourceIntId, 
    GroupTypeId, codes.Title As GroupType,  
    base.Created, 
    base.CreatedById, 
    base.ResourceId
FROM [Resource.GroupType] base
inner join [Codes.GroupType] codes on base.GroupTypeId = codes.Id
where ResourceIntId= @ResourceIntId
order by codes.Title


GO
/****** Object:  StoredProcedure [dbo].[Resource.IntendedAudience_Import]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =============================================
Description:      [Resource.IntendedAudience_Import]
------------------------------------------------------
Modifications
2013-03-13 jgrimmer - Added @ResourceIntId 
2013-04-19 jgrimmer - Modified keyword check to use ResourceIntId
2014-05-09 mparsons - overdue chg to use res int id, not res id
=============================================

*/
CREATE PROCEDURE [dbo].[Resource.IntendedAudience_Import]
          @ResourceId     uniqueidentifier, 
          @IntendedAudienceId	    int,
          @OriginalValue  varchar(100)
          ,@TotalRows     int OUTPUT
          ,@ResourceIntId int

As
declare @NewId uniqueidentifier
, @mapperId int
, @exists uniqueidentifier
, @IsDuplicate bit
, @RecordCount int
, @SuppressOutput bit
, @KeywordId int

if @OriginalValue = '' set @OriginalValue = NULL
If @IntendedAudienceId = -1		SET @SuppressOutput = 1
else set @SuppressOutput  = 0
If @IntendedAudienceId < 1		SET @IntendedAudienceId = NULL 
  
If @OriginalValue is NULL and @IntendedAudienceId is null begin
  print 'no values provided'
  return -1
  end    

set @IsDuplicate= 0
set @TotalRows= 0

-- ==================================================================
-- Do keyword check.  If keyword found, delete keyword.
SELECT @KeywordId = Id
FROM [Resource.Keyword]
WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
	DELETE FROM [Resource.Keyword]
	WHERE Id = @KeywordId
END
	
-- ==================================================================
if @IntendedAudienceId is null begin
	print 'insert via Format'
	 -- so first check if mapping exists
	SELECT @IntendedAudienceId = isnull(base.id,0)
  FROM [dbo].[Codes.AudienceType] base
  inner join [dbo].[Map.AudienceType] mapper on base.Title = mapper.MappedValue
  where mapper.OriginalValue = @OriginalValue
   
	If @IntendedAudienceId is null OR @IntendedAudienceId = 0	begin	
    --no mapping, write to exceptions table and return
    -- when called from interface, may need real message?
    if NOT exists(SELECT [ResourceIntId] FROM [dbo].[Audit.AudienceType_Orphan]
    where [ResourceIntId]= @ResourceIntId and [OriginalValue] = @OriginalValue) begin
      print '@@ no mapping, writing to audit table: ' + @OriginalValue
      INSERT INTO [dbo].[Audit.AudienceType_Orphan]
           ([RowId]
           ,[ResourceIntId]
           ,[OriginalValue])
      VALUES
           (newId()
           ,@ResourceIntId
           ,@OriginalValue)
      
      end    
    else begin
      print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
      end

    --return -1
    if @SuppressOutput = 0
      select '' as Id, 0 As IsDuplicate
    end
	end

if @IntendedAudienceId > 0 begin
  print '[FormatId] = ' + convert(varchar, @IntendedAudienceId)

  set @NewId= NEWID()
  print '@NewId: ' + convert(varchar(50), @NewId)

-- exists check for dups, or allow fail on dup
  select @exists = base.[RowId]
	from [dbo].[Resource.IntendedAudience] base
	where base.[ResourceIntId] = @ResourceIntId
	And base.[AudienceId] = @IntendedAudienceId
	
	if @exists is not NULL AND @exists != '00000000-0000-0000-0000-000000000000' begin
	  set @NewId= @exists
	  set @IsDuplicate= 1
	  print 'found duplicate'
	end
	else begin
    INSERT INTO [dbo].[Resource.IntendedAudience]
	    ([RowId]
	    ,[AudienceId]
	    ,OriginalAudience
	    ,ResourceIntId)

    select
	    @NewId,
	    @IntendedAudienceId, 
	    @OriginalValue,
	    @ResourceIntId
    end
  set @TotalRows = @@rowcount
  if @SuppressOutput = 0    
    select @NewId as Id, @IsDuplicate As IsDuplicate    
end




GO
/****** Object:  StoredProcedure [dbo].[Resource.IntendedAudience_ImportFromMappingOrphan]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[Resource.IntendedAudience_ImportFromMappingOrphan]
            @MaxRecords int,
            @DebugLevel int

As
begin 
          
Declare @ResourceId uniqueidentifier
,@ResourceIntId int
,@RowId uniqueidentifier
,@Value varchar(200)
,@MappedCodeId int
,@cntr int
,@existsCntr int
,@totalRows int

set @cntr = 0
set @existsCntr = 0

select 'started',  getdate()
	-- Loop thru and call proc
	DECLARE thisCursor2 CURSOR FOR
	SELECT base.RowId, base.[ResourceId], base.[OriginalValue] As Value, isnull(map.CodeId, -1), [Resource].Id AS ResourceIntId
  FROM [dbo].[Audit.AudienceType_Orphan] base
  Inner join [Map.AudienceType] map on base.[OriginalValue] = map.OriginalValue
  Inner join [dbo].[Codes.AudienceType] codes on map.[CodeId] = codes.Id
  Inner join [Resource] ON base.ResourceIntId = [Resource].Id
  where 
      (base.IsActive is null OR base.IsActive = 1)
  And (base.[FoundMapping] is null OR base.[FoundMapping] = 0)
  --?? if not tried before, but don't want to all. actually ok, as we are doing a join
  -- still we want to be able to ignore stuff that won't be mapped
  -- maybe if the create date and last run date are X days apart, set the ignore flag

 
	OPEN thisCursor2
	FETCH NEXT FROM thisCursor2 INTO @RowId, @ResourceId, @Value, @MappedCodeId, @ResourceIntId
	WHILE @@FETCH_STATUS = 0 BEGIN
	  set @cntr = @cntr+ 1
	  if @MaxRecords > 0 AND @cntr > @MaxRecords begin
		  print '### Early exit based on @MaxRecords = ' + convert(varchar, @MaxRecords)
		  select 'exiting',  getdate()
		  set @cntr = @cntr - 1
		  BREAK
		  End	  
	  if @MaxRecords > 0 AND @cntr < 25  
	    print '==========> ' +@Value

    set @totalRows = -1 
	  -- not sure what to use for schema here
    EXECUTE [dbo].[Resource.IntendedAudience_Import] 
        @ResourceId, @MappedCodeId,
        @Value, @totalRows OUTPUT,
        @ResourceIntId
        
		--if map was successful, either delete or at least mark
		if @totalRows > 0 begin
		  if @DebugLevel > 5
		    print ' **appeared to be mapped, now mark/delete'
		  UPDATE [dbo].[Audit.AudienceType_Orphan]
        SET [LastRerunDate] = getdate()
        ,[FoundMapping] = 1
      WHERE [RowId] = @RowId
		  end
		else begin
		-- check if a entry already exists for the value (ie current results is a duplicate)
		
		  select @existsCntr = isnull(count(*),0)
        FROM [Map.AudienceType] map 
        inner join [dbo].[Codes.AudienceType] codes on map.[CodeId] = codes.Id
        inner join [dbo].[Resource.IntendedAudience] red 
              on @ResourceIntId = red.ResourceIntId 
              and red.[AudienceId] = codes.id
         where map.OriginalValue  = @Value
         group by red.ResourceIntId
         
    	if @existsCntr > 0 begin
		    if @DebugLevel > 5
		      print ' &&the orphan reference already exists, mark it'
		    UPDATE [dbo].[Audit.AudienceType_Orphan]
          SET [LastRerunDate] = getdate()
          ,[FoundMapping] = 1
        WHERE [RowId] = @RowId
		    end
		  else begin	
	      if @DebugLevel > 5
		      print ' ##not mapped, update rundate????'
	      UPDATE [dbo].[Audit.AudienceType_Orphan]
          SET [LastRerunDate] = getdate()
        WHERE [RowId] = @RowId
	      end
  		end
		
		FETCH NEXT FROM thisCursor2 INTO @RowId, @ResourceId, @Value, @MappedCodeId, @ResourceIntId
	END
	CLOSE thisCursor2
	DEALLOCATE thisCursor2
	select 'completed',  getdate()
  select 'processed records: ' + convert(varchar, @cntr)
  
end


GO
/****** Object:  StoredProcedure [dbo].[Resource.IntendedAudience_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.IntendedAudience_SelectedCodes] ''9B184F11-B032-4CDB-993D-00109FDCDF68''
*/
CREATE PROCEDURE [dbo].[Resource.IntendedAudience_SelectedCodes]
    @ResourceIntId int
As
SELECT distinct
	code.Id, code.Title, code.[Description]
   -- ,ResourceIntId
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.AudienceType] code
Left Join [Resource.IntendedAudience] rpw on code.Id = rpw.AudienceId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1
Order by 2


GO
/****** Object:  StoredProcedure [dbo].[Resource.IntendedAudienceDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.IntendedAudienceDelete]
        @ResourceId uniqueidentifier,
        @AudienceId int
As
DELETE FROM [Resource.IntendedAudience]
WHERE ResourceId = @ResourceId  
AND AudienceId = @AudienceId

GO
/****** Object:  StoredProcedure [dbo].[Resource.IntendedAudienceGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.IntendedAudienceGet]
	@RowId uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT RowID, ResourceId, AudienceId, OriginalAudience
    FROM [Resource.IntendedAudience]
    WHERE RowID = @RowId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.IntendedAudienceGetByResourceIdAudienceId]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.IntendedAudienceGetByResourceIdAudienceId]
	@ResourceId uniqueidentifier,
	@AudienceId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @ResourceId = '00000000-0000-0000-0000-000000000000' SET @ResourceId = NULL
	IF @AudienceId = 0 SET @AudienceId = NULL

	SELECT RowID, ResourceId, AudienceId, OriginalAudience
    FROM [Resource.IntendedAudience]
    WHERE (ResourceId = @ResourceId OR @ResourceId IS NULL) AND
		(AudienceId = @AudienceId OR @AudienceId IS NULL)

END

GO
/****** Object:  StoredProcedure [dbo].[Resource.IntendedAudienceInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--- Insert Procedure for Resource.IntendedAudience---


CREATE PROCEDURE [dbo].[Resource.IntendedAudienceInsert]
            --@ResourceId uniqueidentifier, 
			@ResourceIntId int,
            @AudienceId int, 
            @OriginalAudience varchar(50),
            @CreatedById int

As
--declare @CreatedById int
--set @CreatedById = 0

If @OriginalAudience = ''	SET @OriginalAudience = NULL 
If @CreatedById = 0	SET @CreatedById = NULL 

declare @NewId uniqueidentifier
set @NewId= NEWID()

-- ========================================
if @AudienceId is null begin
	print 'insert via @OriginalAudience'
	select @AudienceId = isnull(id,0) from  [Codes.AudienceType]  rpt 
	where rpt.Title = @OriginalAudience
	
	If @AudienceId is null OR @AudienceId = 0	begin	
		SET @AudienceId = 1 
		print 'the property name was not found: ' + @OriginalAudience
		end
	end
else begin
	print 'insert via @AudienceId'
	end
	
--========================================	
INSERT INTO [Resource.IntendedAudience](
	RowId,
    --ResourceId, 
	ResourceIntId,
    AudienceId, 
    OriginalAudience, CreatedById
)
Values (
	@NewId,
    --@ResourceId, 
	@ResourceIntId,
    @AudienceId, 
    @OriginalAudience, @CreatedById
)
 
select @NewId as Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.IntendedAudienceInsert2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.IntendedAudienceInsert2]
            @ResourceId uniqueidentifier, 
            @AudienceId int, 
            @OriginalAudience varchar(50),
            @CreatedById int
As

If @OriginalAudience = ''	SET @OriginalAudience = NULL 
If @CreatedById = 0	SET @CreatedById = NULL 

declare @NewId uniqueidentifier
set @NewId= NEWID()

-- ========================================
if @AudienceId is null begin
	print 'insert via @OriginalAudience'
	select @AudienceId = isnull(id,0) from  [Codes.AudienceType]  rpt 
	where rpt.Title = @OriginalAudience
	
	If @AudienceId is null OR @AudienceId = 0	begin	
		SET @AudienceId = 1 
		print 'the property name was not found: ' + @OriginalAudience
		end
	end
else begin
	print 'insert via @AudienceId'
	end
	
--========================================	
INSERT INTO [Resource.IntendedAudienceInsert2](
	RowId,
    ResourceId, 
    AudienceId, 
    OriginalAudience, CreatedById
)
Values (
	@NewId,
    @ResourceId, 
    @AudienceId, 
    @OriginalAudience, @CreatedById
)
 
select @NewId as Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.IntendedAudienceSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.IntendedAudienceSelect]
	@ResourceId varchar(50)
	,	@ResourceIntId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    IF @ResourceId = '' SET @ResourceId = NULL
    else IF @ResourceId = '00000000-0000-0000-0000-000000000000' SET @ResourceId = NULL
    IF @ResourceIntId = 0 SET @ResourceIntId = NULL
    
    SELECT RowId, ResourceIntId, AudienceId, OriginalAudience
    ,ResourceId
    FROM [Resource.IntendedAudience]
    WHERE (ResourceId = @ResourceId)
    AND	(ResourceIntId = @ResourceIntId OR @ResourceIntId IS NULL)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.IntendedAudienceUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.IntendedAudienceUpdate]
	@RowId uniqueidentifier,
	@AudienceId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    UPDATE [Resource.IntendedAudience]
    SET AudienceId = @AudienceId
    WHERE RowID = @RowId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.ItemType_Import]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





/* =============================================
Description:      [Resource.ItemType_Import]
------------------------------------------------------
Modifications
=============================================

*/
CREATE PROCEDURE [dbo].[Resource.ItemType_Import]
          @ResourceIntId int,
          @ItemTypeId	    int,
          @OriginalValue  varchar(100)
          ,@TotalRows     int OUTPUT

As
declare @mapperId int
, @exists int
, @IsDuplicate bit
, @RecordCount int
, @SuppressOutput bit
, @KeywordId int

if @OriginalValue = '' set @OriginalValue = NULL
If @ItemTypeId = -1		SET @SuppressOutput = 1
else set @SuppressOutput  = 0
If @ItemTypeId < 1		SET @ItemTypeId = NULL 
  
If @OriginalValue is NULL and @ItemTypeId is null begin
  print 'no values provided'
  return -1
  end    


set @IsDuplicate= 0
set @TotalRows= 0
-- ==================================================================
-- Do keyword check.  If keyword found, delete keyword.
SELECT @KeywordId = Id
FROM [Resource.Keyword]
WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
	DELETE FROM [Resource.Keyword]
	WHERE Id = @KeywordId
END

-- ==================================================================

if @ItemTypeId is null begin
	print 'insert via ItemType'
	 -- so first check if mapping exists
	SELECT @ItemTypeId = isnull(base.id,0)
  FROM [dbo].[Codes.ItemType] base
  inner join [dbo].[Map.ItemType] mapper on base.Id = mapper.[CodeId]
  where mapper.LRValue = @OriginalValue
   
	If @ItemTypeId is null OR @ItemTypeId = 0	begin	
    --no mapping, write to exceptions table and return
    -- when called from interface, may need real message?
    if NOT exists(SELECT [ResourceIntId] FROM [dbo].[Audit.ItemType_Orphan] 
    where [ResourceIntId] = @ResourceIntId and [OriginalValue] = @OriginalValue) begin
      print '@@ no mapping, writing to audit table: ' + @OriginalValue
      INSERT INTO [dbo].[Audit.ItemType_Orphan]
           ([ResourceIntId]
           ,[OriginalValue])
      VALUES
           (@ResourceIntId
           ,@OriginalValue)
      
      end    
    else begin
      print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
      end

    --return -1
    if @SuppressOutput = 0
      select '' as Id, 0 As IsDuplicate
    end
	end

if @ItemTypeId > 0 begin
  print '[ItemTypeId] = ' + convert(varchar, @ItemTypeId)



-- exists check for dups, or allow fail on dup
  select @exists = base.[Id]
	from [dbo].[Resource.ItemType] base
	where base.[ResourceIntId] = @ResourceIntId
	And base.[ItemTypeId] = @ItemTypeId
	
	if @exists is not NULL AND @exists != 0 begin
	  set @IsDuplicate= 1
	  print 'found duplicate'
	end
	else begin
    INSERT INTO [dbo].[Resource.ItemType]
	    ([ResourceIntId]
	    ,[ItemTypeId]
		,[CreatedById])

    select
	    @ResourceIntId, 
	    @ItemTypeId, 
		NULL
    end
  set @TotalRows = @@rowcount
  if @SuppressOutput = 0    
    select @@IDENTITY as Id, @IsDuplicate As IsDuplicate    
end




GO
/****** Object:  StoredProcedure [dbo].[Resource.ItemType_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.ItemType_SelectedCodes] 444662
*/
CREATE PROCEDURE [dbo].[Resource.ItemType_SelectedCodes]
    @ResourceIntId int

As
SELECT distinct
	code.Id, code.Title, code.[Description]
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.ItemType] code
Left Join dbo.[Resource.ItemType] rpw on code.Id = rpw.ItemTypeId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1
Order by 2

GO
/****** Object:  StoredProcedure [dbo].[Resource.ItemTypeDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[Resource.ItemTypeDelete]
            @ResourceIntId int, 
            @ItemTypeId int, 
            @Id int
As
If @ResourceIntId = 0   SET @ResourceIntId = NULL 
If @ItemTypeId = 0   SET @ItemTypeId = NULL 
If @Id = 0   SET @Id = NULL 

DELETE FROM [Resource.ItemType]
WHERE (Id = @Id OR @Id is NULL)
AND (ResourceIntId = @ResourceIntId OR @ResourceIntId is NULL)
And (ItemTypeId = @ItemTypeId OR @ItemTypeId is NULL)


GO
/****** Object:  StoredProcedure [dbo].[Resource.ItemTypeInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.ItemTypeInsert]
            @ResourceIntId int, 
            @ItemTypeId int,  
            @CreatedById int
            --,@ResourceId uniqueidentifier
As

If @CreatedById = 0   SET @CreatedById = NULL 
--If @ResourceId = ''   SET @ResourceId = NULL 
INSERT INTO [Resource.ItemType] (

    ResourceIntId, 
    ItemTypeId, 
    CreatedById,
    LastUpdatedById
)
Values (

    @ResourceIntId, 
    @ItemTypeId, 
    @CreatedById,
    @CreatedById
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.ItemTypeSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.ItemTypeSelect]
  @ResourceIntId int
As
SELECT 
    base.Id, 
    ResourceIntId, 
    ItemTypeId, codes.Title As ItemType,  
    base.Created, base.CreatedById 
	, base.LastUpdated,     base.LastUpdatedById 
    
FROM [Resource.ItemType] base
inner join [Codes.ItemType] codes on base.ItemTypeId = codes.Id
where ResourceIntId= @ResourceIntId
order by codes.Title


GO
/****** Object:  StoredProcedure [dbo].[Resource.ItemTypeUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Update Procedure for [Resource.ItemType] ---
CREATE PROCEDURE [dbo].[Resource.ItemTypeUpdate]
        @Id int,
        @ItemTypeId int, 
        @LastUpdatedById int

As

If @ItemTypeId = 0   SET @ItemTypeId = NULL 
If @LastUpdatedById = 0   SET @LastUpdatedById = NULL 

UPDATE [Resource.ItemType] 
SET 
    ItemTypeId = @ItemTypeId, 
    LastUpdated = GETDATE(), 
    LastUpdatedById = @LastUpdatedById
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.KeywordGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- 13-03-14 jgrimmer - Added ResourceIntId
-- 13-04-12 mparsons - changed to use integer PK, and updated columns
CREATE PROCEDURE [dbo].[Resource.KeywordGet]
	@Id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT Id, 
    --ResourceId, 
    Keyword, ResourceIntId, Created, CreatedById 
    FROM [Resource.Keyword]
    WHERE Id = @Id
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.KeywordInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* Modifications
-- 12-11-05 jgrimmer - Added checking to skip keyword if it exists in another table
-- 13-03-13 jgrimmer - Added (optional for compatibility with existing code) @ResourceIntId parameter
-- 13-08-28 mparsons - Removed ResourceId, and Changed existance checks to use ResourceIntId
*/
CREATE PROCEDURE [dbo].[Resource.KeywordInsert]
	@ResourceIntId	int,
	@OriginalValue varchar(100),
	@CreatedById int

AS
BEGIN
	SET NOCOUNT ON;
	
	If  @ResourceIntId = NULL OR @ResourceIntId = 0 begin
		RAISERROR('Error - require resourceIntId', 18, 1) 
		return -1
		end
	
	DECLARE @RowCount int
	SELECT @RowCount = COUNT(*)
	FROM (
		SELECT OriginalLevel AS OriginalValue FROM [Resource.EducationLevel] WHERE ResourceIntId = @ResourceIntId AND OriginalLevel = @OriginalValue
		UNION ALL
		SELECT OriginalValue FROM [Audit.EducationLevel_Orphan] WHERE ResourceIntId = @ResourceIntId AND OriginalValue = @OriginalValue
		UNION ALL
		SELECT OriginalValue FROM [Resource.Format] WHERE ResourceIntId = @ResourceIntId AND OriginalValue = @OriginalValue
		UNION ALL
		SELECT OriginalValue FROM [Audit.ResourceFormat_Orphan] WHERE ResourceIntId = @ResourceIntId AND OriginalValue = @OriginalValue
		UNION ALL
		SELECT OriginalAudience AS OriginalValue FROM [Resource.IntendedAudience] WHERE ResourceIntId = @ResourceIntId AND OriginalAudience = @OriginalValue
		UNION ALL
		SELECT OriginalValue FROM [Audit.AudienceType_Orphan] WHERE ResourceIntId = @ResourceIntId AND OriginalValue = @OriginalValue
		UNION ALL
		SELECT Keyword AS OriginalValue FROM [Resource.Keyword] WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
		UNION ALL
		SELECT OriginalLanguage AS OriginalValue FROM [Resource.Language] WHERE ResourceIntId = @ResourceIntId AND OriginalLanguage = @OriginalValue
		UNION ALL
		SELECT OriginalValue FROM [Audit.Language_Orphan] WHERE ResourceIntId = @ResourceIntId AND OriginalValue = @OriginalValue
		UNION ALL
		SELECT Subject AS OriginalValue FROM [Resource.Subject] WHERE ResourceIntId = @ResourceIntId AND Subject = @OriginalValue
		UNION ALL
		SELECT OriginalType AS OriginalValue FROM [Resource.ResourceType] WHERE ResourceIntId = @ResourceIntId AND OriginalType = @OriginalValue
		UNION ALL
		SELECT OriginalValue FROM [Audit.ResourceType_Orphan] WHERE ResourceIntId = @ResourceIntId AND OriginalValue = @OriginalValue
	) tbl

	DECLARE @RowId uniqueidentifier
	IF @RowCount IS NULL OR @RowCount = 0 BEGIN
		-- Keyword is not in database.  Add it.		
	    
		SET @RowId = NEWID()
	    IF @OriginalValue = '' SET @OriginalValue = NULL
		
	    INSERT INTO [Resource.Keyword] (ResourceIntId, Keyword, CreatedById, Created)
		VALUES (@ResourceIntId, @OriginalValue, @CreatedById, GETDATE())
		SELECT @RowId AS RowId
    END else Begin
		SELECT '' AS RowId
		end
    
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.KeywordInsertUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.KeywordInsertUpdate]
	@ResourceId uniqueidentifier,
	@OriginalValue varchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Look to see if Resource.Keyword row exists
    DECLARE @RowId uniqueidentifier
    SELECT @RowId = ResourceId
    FROM [Resource.Keyword]
    WHERE ResourceId = @ResourceId

	IF @RowId IS NULL BEGIN
		-- Does not exist, insert new row
	    INSERT INTO [Resource.Keyword] (ResourceId, OriginalValue)
		VALUES (@ResourceId, @OriginalValue)
    END ELSE BEGIN
		-- Exists, update existing row
		UPDATE [Resource.Keyword]
		SET OriginalValue = @OriginalValue
		WHERE ResourceId = @ResourceId
    END
    
    SELECT @ResourceId AS RowId
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.KeywordSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource.KeywordSelect]
	@ResourceIntId int,
	@OriginalValue varchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @ResourceIntId = 0   SET @ResourceIntId = NULL
	IF @OriginalValue = ''  SET @OriginalValue = NULL
	
	SELECT Id, ResourceIntId, Keyword, CreatedById, Created
	FROM [Resource.Keyword]
	WHERE (ResourceIntId = @ResourceIntId OR @ResourceIntId IS NULL) AND
		(Keyword = @OriginalValue OR @OriginalValue IS NULL)
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.Language_Import]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*


-- ====================================================================
DECLARE @RC int, @ResourceId uniqueidentifier, @LanguageId int, @OriginalValue varchar(100),@TotalRows     int

set @LanguageId= 0
set @ResourceIntId = 8
set @OriginalValue = 'en'


EXECUTE @RC = [dbo].[Resource.Language_Import] 
   '', @LanguageId  ,@OriginalValue, @totalRows OUTPUT, @ResourceIntId

GO


*/
/* =============================================
Description:      [Resource.Language_Import]
------------------------------------------------------
Modifications
  2012-11-15 jgrimmer - Added keyword checking.  If exists as keyword, delete keyword.
  2013-03-13 jgrimmer - Added @ResourceIntId
  2013-04-19 jgrimmer - Modified keyword check to use ResourceIntId
  2013-08-22 mparsons - dropped resourceId
=============================================

*/
CREATE PROCEDURE [dbo].[Resource.Language_Import]
          @ResourceId     varchar(50), 
          @LanguageId	    int,
          @OriginalValue  varchar(100)
          ,@TotalRows     int OUTPUT
          ,@ResourceIntId int

As
declare @NewId uniqueidentifier
, @mapperId int
, @exists uniqueidentifier
, @IsDuplicate bit
, @RecordCount int
, @SuppressOutput bit
, @KeywordId int

if @OriginalValue = '' set @OriginalValue = NULL
If @LanguageId = -1		SET @SuppressOutput = 1
else set @SuppressOutput  = 0
If @LanguageId < 1		SET @LanguageId = NULL 
  
If @OriginalValue is NULL and @LanguageId is null begin
  print 'no values provided'
  return -1
  end    


set @IsDuplicate= 0
set @TotalRows= 0
-- ==================================================================
-- Do keyword check.  If keyword found, delete keyword.
SELECT @KeywordId = Id
FROM [Resource.Keyword]
WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
	DELETE FROM [Resource.Keyword]
	WHERE Id = @KeywordId
END

-- ==================================================================

if @LanguageId is null begin
	print 'insert via Language'
	 -- so first check if mapping exists
	SELECT @LanguageId = isnull(base.id,0)
	  FROM [dbo].[Codes.Language] base
	  inner join [dbo].[Map.Language] mapper on base.Id = mapper.[LanguageId]
	  where mapper.LRValue = @OriginalValue
   
	If @LanguageId is null OR @LanguageId = 0	begin	
    --no mapping, write to exceptions table and return
    -- when called from interface, may need real message?
    if NOT exists(SELECT OriginalValue FROM [dbo].[Audit.Language_Orphan] 
		where [ResourceIntId]= @ResourceIntId and [OriginalValue] = @OriginalValue) begin
      print '@@ no mapping, writing to audit table: ' + @OriginalValue
      INSERT INTO [dbo].[Audit.Language_Orphan]
           ([RowId]
           ,[ResourceIntId]
           ,[OriginalValue])
      VALUES
           (newId()
           ,@ResourceIntId
           ,@OriginalValue)
      
      end    
    else begin
      print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
      end

	--return -1
	if @SuppressOutput = 0
		select '' as Id, 0 As IsDuplicate
		end
	end

if @LanguageId > 0 begin
  print '[LanguageId] = ' + convert(varchar, @LanguageId)

  set @NewId= NEWID()
  print '@NewId: ' + convert(varchar(50), @NewId)

-- exists check for dups, or allow fail on dup
  select @exists = base.[RowId]
	from [dbo].[Resource.Language] base
	where base.[ResourceIntId] = @ResourceIntId
	And base.[LanguageId] = @LanguageId
	
	if @exists is not NULL AND @exists != '00000000-0000-0000-0000-000000000000' begin
	  set @NewId= @exists
	  set @IsDuplicate= 1
	  print 'found duplicate'
	end
	else begin
    INSERT INTO [dbo].[Resource.Language]
	    ([RowId]
	    ,[LanguageId]
	    ,OriginalLanguage
		,ResourceIntId)

    select
	    @NewId,
	    @LanguageId, 
	    @OriginalValue,
	    @ResourceIntId
    end
  set @TotalRows = @@rowcount
  if @SuppressOutput = 0    
    select @NewId as Id, @IsDuplicate As IsDuplicate    
end



GO
/****** Object:  StoredProcedure [dbo].[Resource.Language_ImportFromMappingOrphan]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
Attempt to import Resource.Language from the Audit.Language_Orphan table
this would be run on demand after attempted cleanups of the mapping
We also need a process to permanently ignore some mappings
*/
/* =============================================
Description:     [Resource.Language_ImportFromMappingOrphan]
------------------------------------------------------
Modifications
  2013-03-13 jgrimmer - Added ResourceIntId
  2013-08-22 mparsons - dropped resourceId
=============================================

*/


CREATE PROCEDURE [dbo].[Resource.Language_ImportFromMappingOrphan]
            @MaxRecords int,
            @DebugLevel int

As
begin 
          
Declare 
@ResourceIntId int
,@RowId uniqueidentifier
,@Value varchar(200)
,@MappedCodeId int
,@cntr int
,@existsCntr int
,@totalRows int

set @cntr = 0
set @existsCntr = 0

select 'started',  getdate()
	-- Loop thru and call proc
	DECLARE thisCursor3 CURSOR FOR
	SELECT base.RowId, base.[ResourceIntId], base.[OriginalValue] As Value, isnull(map.LanguageId, 0), [Resource].Id AS ResourceIntId
  --  select count(*) 
  FROM [dbo].[Audit.Language_Orphan] base
  Inner join [Map.Language] map on base.[OriginalValue] = map.LRValue
  Inner join [dbo].[Codes.Language] codes on map.[LanguageId] = codes.Id
  Inner join [Resource] ON base.ResourceIntId = [Resource].Id
  where 
      (base.IsActive is null OR base.IsActive = 1)
  And (base.[FoundMapping] is null OR base.[FoundMapping] = 0)
  --?? if not tried before, but don't want to all. actually ok, as we are doing a join
  -- still we want to be able to ignore stuff that won't be mapped
  -- maybe if the create date and last run date are X days apart, set the ignore flag

 
	OPEN thisCursor3
	FETCH NEXT FROM thisCursor3 INTO @RowId, @ResourceIntId, @Value, @MappedCodeId, @ResourceIntId
	WHILE @@FETCH_STATUS = 0 BEGIN
	  set @cntr = @cntr+ 1
	  if @MaxRecords > 0 AND @cntr > @MaxRecords begin
		  print '### Early exit based on @MaxRecords = ' + convert(varchar, @MaxRecords)
		  select 'exiting',  getdate()
		  set @cntr = @cntr - 1
		  BREAK
		  End	  
	  if @MaxRecords > 0 AND @cntr < 25  
	    print ' ==> ' +@Value

	  -- not sure what to use for schema here
    EXECUTE [dbo].[Resource.Language_Import] 
        '', @MappedCodeId,
        @Value, @totalRows OUTPUT,
        @ResourceIntId
        
		--if map was successful, either delete or at least mark
		if @totalRows > 0 begin
		  if @DebugLevel > 5
		    print ' **appeared to be mapped, now mark/delete'
		  UPDATE [dbo].[Audit.Language_Orphan]
        SET [LastRerunDate] = getdate()
        ,[FoundMapping] = 1
      WHERE [RowId] = @RowId
		  end
		else begin
		-- check if a entry already exists for the value (ie current results in a duplicate)
		
		  select @existsCntr = isnull(count(*),0)
        FROM [Map.Language] map 
        inner join [dbo].[Codes.Language] codes on map.[LanguageId] = codes.Id
        inner join [dbo].[Resource.Language] red 
              on @ResourceIntId = red.ResourceIntId 
              and red.[LanguageId] = codes.id
         where map.LRValue  = @Value
         group by red.ResourceIntId
         
    	if @existsCntr > 0 begin
		    if @DebugLevel > 5
		      print ' &&the orphan reference already exists, mark it'
		    UPDATE [dbo].[Audit.Language_Orphan]
          SET [LastRerunDate] = getdate()
          ,[FoundMapping] = 1
        WHERE [RowId] = @RowId
		    end
		  else begin	
	      if @DebugLevel > 5
		      print ' ##not mapped, update rundate????'
	      UPDATE [dbo].[Audit.Language_Orphan]
          SET [LastRerunDate] = getdate()
        WHERE [RowId] = @RowId
	      end
  		end
		
		FETCH NEXT FROM thisCursor3 INTO @RowId, @ResourceIntId, @Value, @MappedCodeId, @ResourceIntId
	END
	CLOSE thisCursor3
	DEALLOCATE thisCursor3
	select 'completed',  getdate()
  select 'processed records: ' + convert(varchar, @cntr)
  
end


GO
/****** Object:  StoredProcedure [dbo].[Resource.Language_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.Language_SelectedCodes] 444662
*/
CREATE PROCEDURE [dbo].[Resource.Language_SelectedCodes]
    @ResourceIntId int
    --    @ResourceId uniqueidentifier
As
SELECT distinct
	code.Id, code.Title
   -- ,ResourceId
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.Language] code
Left Join [Resource.Language] rpw on code.Id = rpw.LanguageId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsPathwaysLanguage = 1
Order by 2


GO
/****** Object:  StoredProcedure [dbo].[Resource.LanguageInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =============================================
Description:      [Resource.LanguageInsert]
------------------------------------------------------
Modifications
  2013-08-22 mparsons - dropped resourceId (left parm for now)
  2013-08-27 mparsons - removed resourceId parm
=============================================
*/
CREATE PROCEDURE [dbo].[Resource.LanguageInsert]
	@LanguageId			int,
	@ResourceIntId		int,
	@CreatedById		int,
	@OriginalLanguage	varchar(100)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	if @OriginalLanguage = '' set @OriginalLanguage= NULL
	if @CreatedById = 0 set @CreatedById= NULL


    DECLARE @RowId uniqueidentifier
    SET @RowId = NEWID()
    
    INSERT INTO [Resource.Language] (RowId, LanguageId, ResourceIntId, OriginalLanguage, CreatedById)
    VALUES (@RowId, @LanguageId, @ResourceIntId, @OriginalLanguage, @CreatedById)
    
    SELECT @RowId AS RowId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LanguageSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =============================================
Description:      [Resource.LanguageSelect]
------------------------------------------------------
Modifications
  2013-08-22 mparsons - dropped resourceId
=============================================
*/
CREATE PROCEDURE [dbo].[Resource.LanguageSelect]
  @ResourceIntId int,
	@OriginalLanguage varchar(100)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  IF @OriginalLanguage = ''       SET @OriginalLanguage = NULL
  IF @ResourceIntId = 0   SET @ResourceIntId = NULL
  If  @ResourceIntId = NULL begin
    RAISERROR('Error - require resourceIntId', 18, 1) 
    return -1
    end
    
  SELECT RowId, ResourceIntId, OriginalLanguage, LanguageId
  
  FROM [Resource.Language]
  WHERE 
      (ResourceIntId = @ResourceIntId OR @ResourceIntId IS NULL)
  AND (OriginalLanguage = @OriginalLanguage OR @OriginalLanguage IS NULL)
   
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.LanguageSelect2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =============================================
Description:      [Resource.LanguageSelect]
------------------------------------------------------
Modifications
  2013-08-22 mparsons - dropped resourceId
=============================================
*/
CREATE PROCEDURE [dbo].[Resource.LanguageSelect2]
  @ResourceIntId int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  IF @ResourceIntId = 0   SET @ResourceIntId = NULL
  If  @ResourceIntId = NULL begin
    RAISERROR('Error - require resourceIntId', 18, 1) 
    return -1
    end
    
  SELECT RowId, ResourceIntId, OriginalLanguage, LanguageId
 
  FROM [Resource.Language]
  WHERE 
      (ResourceIntId = @ResourceIntId)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LikeGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/7/2013
-- Description:	Get resource Like row
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LikeGet]
	@Id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, ResourceId, ResourceIntId, IsLike, Created, CreatedById
	FROM [Resource.Like]
	WHERE Id = @Id
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LikeGetDisplay]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/7/2013
-- Description:	Get resource Like Summary
-- mods
-- 14-10-10 mparsons - added HasRating to simplify checking if user has rated the resource
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LikeGetDisplay]
	@Id int, 
	@UserId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT rls.ResourceIntId, rls.LikeCount
		, rls.DislikeCount, IsNull(YouLikeThis,'False') AS YouLikeThis
		, isnull(YouDislikeThis,'False') AS YouDislikeThis
		, case when isnull(ylt.ResourceIntId, 0) > 0 then 1 else 0 end As HasRating
	FROM [Resource.LikesSummary] rls
	LEFT JOIN (SELECT ResourceIntId,
		CASE WHEN IsLike = 'True' THEN 'True' ELSE 'False' END AS YouLikeThis,
		CASE WHEN IsLike = 'False' THEN 'True' ELSE 'False' END AS YouDislikeThis
		FROM [Resource.Like] WHERE CreatedById = @UserId) ylt ON rls.ResourceIntId = ylt.ResourceIntId
	WHERE rls.ResourceIntId = @Id
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LikeInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/7/2013
-- Description:	Insert resource Like row
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LikeInsert]
	@ResourceId uniqueidentifier, @ResourceIntId int, @IsLike bit, @CreatedById int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [Resource.Like] (ResourceId, ResourceIntId, IsLike, Created, CreatedById)
	VALUES (@ResourceId, @ResourceIntId, @IsLike, GETDATE(), @CreatedById)
	
	SELECT @@IDENTITY AS Id
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LikeSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/7/2013
-- Description:	Get Likes/Dislikes for a resource by Id, Date Range, IsLike
-- NOTE: If IsLike is NULL, the proc will pull for both
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LikeSelect]
	@ResourceIntId int, @StartDate datetime, @EndDate datetime, @IsLike varchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @IsLike = 'both' SET @IsLike = NULL
	IF @IsLike = 'like' SET @IsLike = 'True'
	IF @IsLike = 'dislike' SET @IsLike = 'False'
	SELECT Id, ResourceId, ResourceIntId, IsLike, Created, CreatedById
	FROM [Resource.Like]
	WHERE (ResourceIntId = @ResourceIntId OR @ResourceIntId IS NULL) AND
		(Created >= @StartDate AND Created <= @EndDate) AND
		(IsLike = @IsLike OR @IsLike IS NULL)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LikeSummaryGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/7/2013
-- Description:	Get resource LikeSummary row by ResourceIntId
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LikeSummaryGet]
	@ResourceIntId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, ResourceId, ResourceIntId, LikeCount, DislikeCount, LastUpdated
	FROM [Resource.LikeSummary]
	WHERE ResourceIntId = @ResourceIntId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LikeSummaryInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/7/2013
-- Description:	Insert resource LikeSummary row
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LikeSummaryInsert]
	@ResourceId uniqueidentifier, @ResourceIntId int, @LikeCount int, @DislikeCount int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [Resource.LikeSummary] (ResourceId, ResourceIntId, LikeCount, DislikeCount, LastUpdated)
	VALUES (@ResourceId, @ResourceIntId, @LikeCount, @DislikeCount, GETDATE())
	
	SELECT @@IDENTITY AS Id
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LikeSummaryUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 3/7/2013
-- Description:	Update resource LikeSummary row
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LikeSummaryUpdate]
	@Id int, @LikeCount int, @DislikeCount int	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE [Resource.LikeSummary]
	SET LikeCount = @LikeCount,
		DislikeCount = @DislikeCount,
		LastUpdated = GETDATE()
	WHERE Id = @Id
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LinkCheckAdd]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 5/14/2013
-- Description:	Add new resources to LinkCheck table
-- =============================================
CREATE PROCEDURE [dbo].[Resource.LinkCheckAdd]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO [Resource.LinkCheck] (ResourceIntId)
	SELECT ResourceIntId
	FROM [Resource] res
	LEFT JOIN [Resource.LinkCheck] rlc ON res.Id = rlc.ResourceIntId
	WHERE rlc.ResourceIntId IS NULL
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.LinkCheckGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.LinkCheckGet]
    @Id int
As
SELECT     rlc.Id, 
    rlc.ResourceIntId, 
    rlc.LastCheckDate, 
    rlc.HostTimeoutCount, 
    rlc.ServerErrorCount, 
    rlc.IsBadLink,
    res.ResourceUrl
FROM [Resource.LinkCheck] rlc
INNER JOIN [Resource] res ON rlc.ResourceIntId = res.Id
WHERE rlc.Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.LinkCheckSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.LinkCheckSelect]
	@days int,
	@HostTimeoutCount int,
	@ServerErrorCount int,
	@HostName varchar(100)
As
DECLARE @EndDate datetime
IF @days > 0 SET @EndDate = DATEADD(d,0-@days,getdate()) ELSE SET @EndDate = GETDATE()
IF @HostTimeoutCount = 0 SET @HostTimeoutCount = NULL
IF @ServerErrorCount = 0 SET @ServerErrorCount = NULL
IF @HostName = '' SET @HostName = NULL ELSE SET @HostName = '%'+@HostName+'%'

SELECT 
    rlc.Id, 
    rlc.ResourceIntId, 
    rlc.LastCheckDate, 
    rlc.HostTimeoutCount, 
    rlc.ServerErrorCount, 
    rlc.IsBadLink,
    res.ResourceUrl
FROM [Resource.LinkCheck] rlc
INNER JOIN [Resource] res on rlc.ResourceIntId = res.Id
WHERE (rlc.LastCheckDate < @EndDate) AND 
	(rlc.HostTimeoutCount >= @HostTimeoutCount OR @HostTimeoutCount IS NULL) AND
	(rlc.ServerErrorCount >= @ServerErrorCount OR @ServerErrorCount IS NULL) AND
	(res.ResourceUrl LIKE @HostName OR @HostName IS NULL)

GO
/****** Object:  StoredProcedure [dbo].[Resource.LinkCheckUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Update Procedure for [Resource.LinkCheck] ---
CREATE PROCEDURE [dbo].[Resource.LinkCheckUpdate]
        @LastCheckDate datetime, 
        @HostTimeoutCount int, 
        @ServerErrorCount int, 
        @IsBadLink bit, 
@Id int
As
UPDATE [Resource.LinkCheck] 
SET 
    LastCheckDate = @LastCheckDate, 
    HostTimeoutCount = @HostTimeoutCount, 
    ServerErrorCount = @ServerErrorCount, 
    IsBadLink = @IsBadLink
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.PathwaySelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[Resource.PathwaySelect] ''
*/
CREATE PROCEDURE [dbo].[Resource.PathwaySelect]
    @ResourceId uniqueidentifier
As
SELECT 
	base.Id, base.IlPathwayName As Title
--    ,ResourceId
	,CASE
		WHEN rpw.ResourceId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as HasCluster
FROM [dbo].CareerCluster base
Left Join [Resource.Pathway] rpw on base.Id = rpw.PathwayId
		and rpw.ResourceId = @ResourceId
where IsIlPathway = 1

Order by base.IlPathwayName

GO
/****** Object:  StoredProcedure [dbo].[Resource.PropertyInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
declare @PropertyName varchar(100),@NewId uniqueidentifier
set @NewId= NEWID()
set @PropertyName = 'subject'

Select
	@NewId,
    '@ResourceId', 
    @PropertyName, 
    '@Value', 
    rpt.Id	--PropertyTypeId
from [Resource.Property] base    
left Join [Codes.ResPropertyType] rpt on @PropertyName = rpt.Title

8B5D62B9-CCBF-4E01-8177-DB746540F397

-- ====================================================================
DECLARE @RC int, @ResourceId uniqueidentifier,@PropertyTypeId int
DECLARE @PropertyName varchar(100), @Value varchar(500)

set @ResourceId = '7F1427AF-ABB8-4CCD-AC00-000BB5EE7C30'
set @PropertyTypeId = 0
set @PropertyName = 'subjecterr'
set @Value = 'test2'

EXECUTE @RC = [LearningRegistryCache2].[dbo].[Resource.PropertyInsert] 
   @ResourceId
  ,@PropertyTypeId
  ,@PropertyName
  ,@Value
GO

-- 2013-03-13 jgrimmer - Added ResourceIntId
*/
CREATE PROCEDURE [dbo].[Resource.PropertyInsert]
            @ResourceId uniqueidentifier, 
            @PropertyTypeId int,
            @PropertyName varchar(100), 
            @Value varchar(500),
            @ResourceIntId int

As
if @PropertyName is null OR LEN(@PropertyName) = 0 set @PropertyName = 'other'
If @PropertyTypeId = 0		SET @PropertyTypeId = NULL 
  
declare @NewId uniqueidentifier
set @NewId= NEWID()
print '@NewId: ' + convert(varchar(50), @NewId)

if @PropertyTypeId is null begin
	print 'insert via PropertyName'
	select @PropertyTypeId = isnull(id,0) from  [Codes.ResPropertyType]  rpt 
	where rpt.Title = @PropertyName
	
	If @PropertyTypeId is null OR @PropertyTypeId = 0	begin	
		SET @PropertyTypeId = 8 
		print 'the property name was not found: ' + @PropertyName
		end
	end
else begin
	print 'insert via @PropertyTypeId'
	end
	
	
	INSERT INTO [Resource.Property]
	(
		RowId,
		ResourceId, 
		PropertyTypeId,
		Value,
		ResourceIntId
	)
	Values (
		@NewId,
		@ResourceId, 
		@PropertyTypeId, 
		@Value,
		@ResourceIntId
	)

--)
 
select @NewId as Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.PropertySelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Create date: 6/16/2012
-- Description:	Select one or more name/value pairs for a resource
--
-- 2013-03-13 jgrimmer - Added ResourceIntId
-- =============================================
CREATE PROCEDURE [dbo].[Resource.PropertySelect]
	@filter varchar(4000)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @sql varchar(4500)
	SET @sql = 'SELECT rp.RowId, ResourceId, PropertyTypeId, rpt.Title As [Name], [Value], [ResourceIntId] FROM [Resource.Property] rp Inner join [Codes.ResPropertyType] rpt on rp.PropertyTypeId = rpt.Id ' + @filter

	PRINT @sql
	EXEC(@sql)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.PublishedByInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

-- ====================================================================
DECLARE @RC int, @ResourceId uniqueidentifier,@PublishedById int
DECLARE @PropertyName varchar(100), @Value varchar(500)

set @ResourceId = '7F1427AF-ABB8-4CCD-AC00-000BB5EE7C30'
set @PublishedById = 1


EXECUTE @RC = [dbo].[Resource.PublishedByInsert] 
   @ResourceId
  ,@PublishedById



*/
CREATE PROCEDURE [dbo].[Resource.PublishedByInsert]
            @ResourceIntId int, 
            @PublishedById int

As
--if len(rtrim(@ResourceId)) = 0	Set @ResourceId = NULL
If @ResourceIntId = 0				SET @ResourceIntId = NULL 
If @PublishedById = 0				SET @PublishedById = NULL 

if @PublishedById is null OR @ResourceIntId is null begin
	print 'Resource.PublishedByInsert Error: Incomplete parameters were provided'
	RAISERROR('Resource.PublishedByInsert Error: incomplete parameters were provided. Require Source @ResourceIntId, and @PublishedById', 18, 1)    
	RETURN -1 
	end
	
	
INSERT INTO [dbo].[Resource.PublishedBy]
           (ResourceIntId
           ,[PublishedById])
Values (
	@ResourceIntId, 
	@PublishedById
)



GO
/****** Object:  StoredProcedure [dbo].[Resource.RatingSummaryDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/22/2012
-- Description:	Delete Paradata Rating Summary records
-- =============================================
CREATE PROCEDURE [dbo].[Resource.RatingSummaryDelete]
	@ResourceId uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Initialization
	
	DELETE FROM [Resource.RatingSummary]
	WHERE ResourceId = @ResourceId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.RatingSummaryGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/22/2012
-- Description:	Retrieve Rating Summary
--
-- 2012-12-18 jgrimmer - Added Description
-- 2013-03-14 jgrimmer - Added ResourceIntId
-- =============================================
CREATE PROCEDURE [dbo].[Resource.RatingSummaryGet]
	@ResourceId uniqueidentifier,
	@RatingTypeId int,
	@Type varchar(50),
	@Identifier varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @ResourceId = '00000000-0000-0000-0000-000000000000' SET @ResourceId = NULL
	IF @Type = '' SET @Type = NULL
	
	SELECT ResourceId, RatingTypeId, RatingCount, RatingTotal, RatingAverage,
		LastUpdated, [Type], [Description], [Identifier], ResourceIntId
	FROM [Resource.RatingSummary] prs
	INNER JOIN [Codes.RatingType] rt ON prs.RatingTypeId = rt.Id
	WHERE (ResourceId = @ResourceId OR @ResourceId IS NULL) AND
		([Type] = @Type OR @Type IS NULL) AND
		(Identifier = @Identifier OR @Identifier IS NULL)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.RatingSummaryInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 8/22/2012
-- Description:	Create Paradata Rating Summary row
--
-- 2013-03-14 jgrimmer - Added @ResourceIntId
-- =============================================
CREATE PROCEDURE [dbo].[Resource.RatingSummaryInsert]
	@ResourceId uniqueidentifier,
	@RatingTypeId int,
	@RatingCount int,
	@RatingTotal int,
	@RatingAverage decimal(5,2),
	@ResourceIntId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [Resource.RatingSummary] 
		(ResourceId, RatingTypeId, RatingCount, RatingTotal, RatingAverage, 
		 LastUpdated, ResourceIntId)
	VALUES (@ResourceId, @RatingTypeId, @RatingCount, @RatingTotal, @RatingAverage,
		GETDATE(), @ResourceIntId)
		
	SELECT @ResourceId AS RowId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.RatingSummaryUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/22/2012
-- Description:	Update Paradata Rating Summary Record
--
-- 2013-03-14 jgrimmer - Added @ResourceIntId
-- =============================================
CREATE PROCEDURE [dbo].[Resource.RatingSummaryUpdate]
	@ResourceId uniqueidentifier,
	@RatingTypeId int,
	@RatingCount int,
	@RatingTotal int,
	@RatingAverage decimal(5,2),
	@ResourceIntId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    UPDATE [Resource.RatingSummary]
    SET RatingCount = @RatingCount,
		RatingTotal = @RatingTotal,
		RatingAverage = @RatingAverage,
		LastUpdated = GETDATE()
	WHERE (ResourceId = @ResourceId OR ResourceIntId = @ResourceIntId) AND RatingTypeId = @RatingTypeId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.RecommendationDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.RecommendationDelete]
        @Id int
As
DELETE FROM [Resource.Recommendation]
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.RecommendationGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.RecommendationGet]
    @Id int
As
SELECT     Id, 
    ResourceIntId, 
    TypeId, 
    IsActive, 
    Comment, 
    Created, 
    CreatedById
FROM [Resource.Recommendation]
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.RecommendationInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.RecommendationInsert]
            @ResourceIntId int, 
            @TypeId int, 
            @Comment varchar(MAX), 
            @CreatedById int
As

If @TypeId = 0   SET @TypeId = NULL 
If @Comment = ''   SET @Comment = NULL 
If @CreatedById = 0   SET @CreatedById = NULL 

INSERT INTO [Resource.Recommendation] (

    ResourceIntId, 
    TypeId, 
    Comment, 
    Created, 
    CreatedById
)
Values (

    @ResourceIntId, 
    @TypeId, 
    @Comment, 
    getdate(), 
    @CreatedById
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.RecommendationSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.RecommendationSelect]
			@ResourceIntId int
As
SELECT 
    Id, 
    ResourceIntId, 
    TypeId, 
    IsActive, 
    Comment, 
    Created, 
    CreatedById
FROM [Resource.Recommendation]
WHERE ResourceIntId = @ResourceIntId
Order by Created


GO
/****** Object:  StoredProcedure [dbo].[Resource.RecommendationUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Update Procedure for [Resource.Recommendation] ---
CREATE PROCEDURE [dbo].[Resource.RecommendationUpdate]
        @Id int, 
        @IsActive bit, 
        @Comment varchar(MAX)

As

If @Comment = ''   SET @Comment = NULL 


UPDATE [Resource.Recommendation] 
SET 
    IsActive = @IsActive, 
    Comment = @Comment
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.RelatedUrlDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.RelatedUrlDelete]
        @Id int
As
DELETE FROM [Resource.RelatedUrl]
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.RelatedUrlGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.RelatedUrlGet]
    @Id int
As
SELECT     Id, 
    ResourceIntId, 
    RelatedUrl, 
    IsActive, 
    Created, 
    CreatedById, 
    ResourceId
FROM [Resource.RelatedUrl]
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.RelatedUrlInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.RelatedUrlInsert]
            @ResourceIntId int, 
            @RelatedUrl varchar(300), 
            @CreatedById int
            --,@ResourceId varchar(40)
As
If @ResourceIntId = 0   SET @ResourceIntId = NULL 
If @CreatedById = 0   SET @CreatedById = NULL 
--If @ResourceId = ''   SET @ResourceId = NULL 

INSERT INTO [Resource.RelatedUrl] (

    ResourceIntId, 
    RelatedUrl, 
    CreatedById 
    --,ResourceId
)
Values (

    @ResourceIntId, 
    @RelatedUrl, 
    @CreatedById
    --,@ResourceId
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.RelatedUrlSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.RelatedUrlSelect]
            @ResourceIntId int
            --,@ResourceId varchar(40)
As
If @ResourceIntId = 0   SET @ResourceIntId = NULL 

SELECT 
    Id, 
    ResourceIntId, 
    RelatedUrl, 
    IsActive, 
    Created, 
    CreatedById, 
    ResourceId
FROM [Resource.RelatedUrl]
where ResourceIntId= @ResourceIntId
Order by RelatedUrl


GO
/****** Object:  StoredProcedure [dbo].[Resource.ResourceType_FixUnknownTypes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
Exec CodeTables_UpdateWarehouseTotals 10

select [MappingType], OriginalValue from [dbo].[Audit.ResourceType_Orphan]



EXECUTE [dbo].[Resource.ResourceType_FixUnknownTypes] 25


*/

Create PROCEDURE [dbo].[Resource.ResourceType_FixUnknownTypes]
            @MaxRecords int

As
begin 
          
Declare 
@ResourceId uniqueidentifier
,@RowId uniqueidentifier
,@OriginalValue varchar(200)
,@cntr int
,@totalRows int
,@ResourceTypeId	int


set @cntr= 0
set @ResourceTypeId = 0

select 'started',  getdate()
	-- Loop thru and call proc
	DECLARE thisCursor CURSOR FOR
    select base.RowId, base.ResourceId, base.[OriginalType]
      FROM [dbo].[Resource.ResourceType] base
      left join [Map.ResourceType] map on base.[OriginalType] = map.LRValue
      left join [dbo].[Codes.ResourceType] codes on map.[CodeId] = codes.Id
      --left join [dbo].[Resource.ResourceType] rtype on base.ResourceId = rtype.ResourceId and rtype.[ResourceTypeId] = codes.id
    where [ResourceTypeId] = 18
    -- and rtype.ResourceId is  null
 
	OPEN thisCursor
	FETCH NEXT FROM thisCursor INTO @RowId, @ResourceId, @OriginalValue
	WHILE @@FETCH_STATUS = 0 BEGIN
	  set @cntr = @cntr+ 1
	  if @MaxRecords > 0 AND @cntr > @MaxRecords begin
		  print '### Early exit based on @MaxRecords = ' + convert(varchar, @MaxRecords)
		  select 'exiting',  getdate()
		  set @cntr = @cntr - 1
		  BREAK
		  End	  
		  
 --   if @cntr < 50 print '-- ' + convert(varchar(50),@ResourceId) + ' / ' + @OriginalValue 
    
	  SELECT @ResourceTypeId = isnull(base.id,0)
    FROM [dbo].[Codes.ResourceType] base
    inner join [dbo].[Map.ResourceType] mapper on base.Id = mapper.[CodeId]
    where mapper.LRValue = @OriginalValue		  
		  
	  If @ResourceTypeId is null OR @ResourceTypeId = 0	begin	
	    --no mapping, check if exists in audit
	    if NOT exists(SELECT [ResourceId] FROM [dbo].[Audit.ResourceType_Orphan] 
      where [ResourceId]= @ResourceId and [OriginalValue] = @OriginalValue) begin
        print '@@ no mapping, writing to audit table: ' + @OriginalValue
        INSERT INTO [dbo].[Audit.ResourceType_Orphan]
             ([RowId]
             ,[ResourceId]
             ,[OriginalValue])
        VALUES
             (newId()
             ,@ResourceId
             ,@OriginalValue)
        
        end    
      else begin
        print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
        end
      end
	   else begin
	    --do update
	    print '** updating value: ' + @OriginalValue + ' to type id: ' + convert(varchar,@ResourceTypeId)
      UPDATE [dbo].[Resource.ResourceType]
         SET [ResourceTypeId] = @ResourceTypeId
       WHERE RowId= @RowId
       	  
	    end

		
		FETCH NEXT FROM thisCursor INTO @RowId, @ResourceId, @OriginalValue
	END
	CLOSE thisCursor
	DEALLOCATE thisCursor
	select 'completed',  getdate()
  select 'processed records: ' + convert(varchar, @cntr)
  
end


GO
/****** Object:  StoredProcedure [dbo].[Resource.ResourceType_ImportFromMappingOrphan]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
Attempt to import Resource.ResourceType from the Audit.ResourceType_Orphan table
this would be run on demand after attempted cleanups of the mapping
We also need a process to permanently ignore some mappings
*/
--
-- Modifications
-- 2013-03-14 jgrimmer - Added ResourceIntId
CREATE PROCEDURE [dbo].[Resource.ResourceType_ImportFromMappingOrphan]
            @MaxRecords int,
            @DebugLevel int

As
begin 
          
Declare @ResourceId varchar(40)
,@ResourceIntId int
,@RowId uniqueidentifier
,@Value varchar(200)
,@MappedCodeId int
,@cntr int
,@existsCntr int
,@totalRows int

set @cntr = 0
set @existsCntr = 0

select 'started',  getdate()
	-- Loop thru and call proc
	DECLARE thisCursor2 CURSOR FOR
	SELECT base.RowId, base.[ResourceId], base.[OriginalValue] As Value, isnull(map.CodeId, -1), [Resource].Id AS ResourceIntId
  FROM [dbo].[Audit.ResourceType_Orphan] base
  Inner join [Map.ResourceType] map on base.[OriginalValue] = map.LRValue
  Inner join [dbo].[Codes.ResourceType] codes on map.[CodeId] = codes.Id
  Inner join [Resource] ON base.[ResourceIntId] = [Resource].Id
  where 
      (base.IsActive is null OR base.IsActive = 1)
  And (base.[FoundMapping] is null OR base.[FoundMapping] = 0)
  --?? if not tried before, but don't want to all. actually ok, as we are doing a join
  -- still we want to be able to ignore stuff that won't be mapped
  -- maybe if the create date and last run date are X days apart, set the ignore flag

 
	OPEN thisCursor2
	FETCH NEXT FROM thisCursor2 INTO @RowId, @ResourceId, @Value, @MappedCodeId, @ResourceIntId
	WHILE @@FETCH_STATUS = 0 BEGIN
	  set @cntr = @cntr+ 1
	  if @MaxRecords > 0 AND @cntr > @MaxRecords begin
		  print '### Early exit based on @MaxRecords = ' + convert(varchar, @MaxRecords)
		  select 'exiting',  getdate()
		  set @cntr = @cntr - 1
		  BREAK
		  End	  
	  if @MaxRecords > 0 AND @cntr < 25  
	    print '==========> ' +@Value

    set @totalRows = -1 
	  -- not sure what to use for schema here
    EXECUTE [dbo].[Resource.ResourceTypeImport] 
        @ResourceId, @MappedCodeId,
        @Value, @totalRows OUTPUT,
        @ResourceIntId
        
		--if map was successful, either delete or at least mark
		if @totalRows > 0 begin
		  if @DebugLevel > 5
		    print ' **appeared to be mapped, now mark/delete'
		  UPDATE [dbo].[Audit.ResourceType_Orphan]
        SET [LastRerunDate] = getdate()
        ,[FoundMapping] = 1
      WHERE [RowId] = @RowId
		  end
		else begin
		-- check if a entry already exists for the value (ie current results is a duplicate)
		
		  select @existsCntr = isnull(count(*),0)
        FROM [Map.ResourceType] map 
        inner join [dbo].[Codes.ResourceType] codes on map.[CodeId] = codes.Id
        inner join [dbo].[Resource.ResourceType] red 
              on @ResourceIntId = red.ResourceIntId 
              and red.[ResourceTypeId] = codes.id
         where map.LRValue  = @Value
         group by red.ResourceIntId
         
    	if @existsCntr > 0 begin
		    if @DebugLevel > 5
		      print ' &&the orphan reference already exists, mark it'
		    UPDATE [dbo].[Audit.ResourceType_Orphan]
          SET [LastRerunDate] = getdate()
          ,[FoundMapping] = 1
        WHERE [RowId] = @RowId
		    end
		  else begin	
	      if @DebugLevel > 5
		      print ' ##not mapped, update rundate????'
	      UPDATE [dbo].[Audit.ResourceType_Orphan]
          SET [LastRerunDate] = getdate()
        WHERE [RowId] = @RowId
	      end
  		end
		
		FETCH NEXT FROM thisCursor2 INTO @RowId, @ResourceId, @Value, @MappedCodeId, @ResourceIntId
	END
	CLOSE thisCursor2
	DEALLOCATE thisCursor2
	select 'completed',  getdate()
  select 'processed records: ' + convert(varchar, @cntr)
  
end


GO
/****** Object:  StoredProcedure [dbo].[Resource.ResourceType_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.ResourceType_SelectedCodes] ''9B184F11-B032-4CDB-993D-00109FDCDF68''
*/
CREATE PROCEDURE [dbo].[Resource.ResourceType_SelectedCodes]
    @ResourceIntId int
As
SELECT distinct
	code.Id, code.Title, code.[Description]
   -- ,ResourceIntId
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.ResourceType] code
Left Join [Resource.ResourceType] rpw on code.Id = rpw.ResourceTypeId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1
Order by 2


GO
/****** Object:  StoredProcedure [dbo].[Resource.ResourceTypeDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
Mods
13-08-22 mparsons - change to use @ResourceIntId
*/
Create PROCEDURE [dbo].[Resource.ResourceTypeDelete]
        @ResourceIntId int,
        @ResourceTypeId int
As
DELETE FROM [Resource.ResourceType]
WHERE ResourceIntId = @ResourceIntId  
AND ResourceTypeId = @ResourceTypeId

GO
/****** Object:  StoredProcedure [dbo].[Resource.ResourceTypeGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[Resource.ResourceTypeGet] 'BE598CA0-ED66-4DD9-9402-2A187FDC8DE7'



*/

/*
Mods
13-06-03 mparsons - changed as did not match the DAL get

13-08-22 mparsons - drop proc, as not used
*/
Create PROCEDURE [dbo].[Resource.ResourceTypeGet]
	@RowId uniqueidentifier
	--,@ResourceId uniqueidentifier,
	--@CodeId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--IF @RowId = '00000000-0000-0000-0000-000000000000' SET @RowId = NULL
	--IF @ResourceId = '00000000-0000-0000-0000-000000000000' SET @ResourceId = NULL
	--IF @CodeId = '' SET @CodeId = NULL

	SELECT RowId, ResourceIntId, OriginalType AS OriginalValue, ResourceTypeId AS CodeId,
		crt.Title AS MappedValue
	FROM [Resource.ResourceType] rrt
	INNER JOIN [Codes.ResourceType] crt ON rrt.ResourceTypeId = crt.Id
	WHERE   (RowId = @RowId)


END
GO
/****** Object:  StoredProcedure [dbo].[Resource.ResourceTypeImport]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*


-- ====================================================================
DECLARE @RC int, @ResourceId uniqueidentifier, @ResourceIntId int, @ResourceTypeId int, @OriginalValue varchar(100),@TotalRows     int

set @ResourceTypeId= 0
set @ResourceId = ''
set @ResourceIntId= 11
set @OriginalValue = 'Syllabus'

set @ResourceId = 'c73542af-bdaf-4043-be18-417d5a68e6be'
set @OriginalValue = 'Image/jpg2'



EXECUTE @RC = [dbo].[Resource.ResourceTypeImport] 
   @ResourceId, @ResourceTypeId
  ,@OriginalValue, @totalRows OUTPUT, @ResourceIntId

GO


*/
/* =============================================
Description:      [Resource.ResourceTypeImport]
------------------------------------------------------
Modifications
12-09-19 mparsons - added duplicates check (multiple inputs can map to same output)
12-11-15 jgrimmer - added keyword check.  If exists as keyword, delete the keyword.
13-03-14 jgrimmer - added ResourceIntId
13-04-19 jgrimmer - Modified keyword check to use ResourceIntId
14-05-09 mparsons - changed any references to resourceId to ResourceIntId (should have been done last year!)
=============================================

*/
CREATE PROCEDURE [dbo].[Resource.ResourceTypeImport]
          @ResourceId		varchar(40), 
          @ResourceTypeId	int,
          @OriginalValue	varchar(100)
          ,@TotalRows		int OUTPUT
          ,@ResourceIntId	int

As
declare @NewId uniqueidentifier
, @mapperId int
, @exists uniqueidentifier
, @IsDuplicate bit
, @RecordCount int
, @SuppressOutput bit
, @KeywordId int

if @OriginalValue = '' set @OriginalValue = NULL
If @ResourceTypeId = -1		SET @SuppressOutput = 1
else set @SuppressOutput  = 0
If @TotalRows = -1		SET @SuppressOutput = 1
If @ResourceTypeId < 1		SET @ResourceTypeId = NULL 
  
If @OriginalValue is NULL and @ResourceTypeId is null begin
  print 'no values provided'
  return -1
  end    

set @IsDuplicate= 0
set @TotalRows= 0

-- ==================================================================
-- Do keyword check.  If keyword found, delete keyword.
SELECT @KeywordId = Id
FROM [Resource.Keyword]
WHERE ResourceIntId = @ResourceIntId AND Keyword = @OriginalValue
IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
	DELETE FROM [Resource.Keyword]
	WHERE Id = @KeywordId
END

-- ==================================================================
if @ResourceTypeId is null begin
	print 'insert via ResourceType'
	 -- so first check if mapping exists
	SELECT @ResourceTypeId = isnull(base.id,0)
  FROM [dbo].[Codes.ResourceType] base
  inner join [dbo].[Map.ResourceType] mapper on base.Id = mapper.[CodeId]
  where mapper.LRValue = @OriginalValue
   
	If @ResourceTypeId is null OR @ResourceTypeId = 0	begin	
    --no mapping, write to exceptions table and return
    -- when called from interface, may need real message?
    if NOT exists(SELECT [ResourceIntId] FROM [dbo].[Audit.ResourceType_Orphan] 
    where [ResourceIntId]= @ResourceIntId and [OriginalValue] = @OriginalValue) begin
      print '@@ no mapping, writing to audit table: ' + @OriginalValue
      INSERT INTO [dbo].[Audit.ResourceType_Orphan]
           ([RowId]
           ,[ResourceIntId]
           ,[OriginalValue])
      VALUES
           (newId()
           ,@ResourceIntId
           ,@OriginalValue)
      
      end    
    else begin
      print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
      end

    --return -1
    if @SuppressOutput = 0
      select '' as Id, 0 As IsDuplicate
    end
	end

if @ResourceTypeId > 0 begin
  if @SuppressOutput = 0
    print '[ResourceTypeId] = ' + convert(varchar, @ResourceTypeId)

  set @NewId= NEWID()
  if @SuppressOutput = 0
    print '@NewId: ' + convert(varchar(50), @NewId)

-- exists check for dups, or allow fail on dup
  select @exists = base.[RowId]
	from [dbo].[Resource.ResourceType] base
	where base.[ResourceIntId] = @ResourceIntId
	And base.[ResourceTypeId] = @ResourceTypeId
	
	if @exists is not NULL AND @exists != '00000000-0000-0000-0000-000000000000' begin
	  set @NewId= @exists
	  set @IsDuplicate= 1
	  if @SuppressOutput = 0
	    print 'found duplicate'
	end
	else begin
    INSERT INTO [dbo].[Resource.ResourceType]
	    ([RowId]
	    ,[ResourceTypeId]
	    ,[OriginalType]
	    ,[ResourceIntId])

    select
	    @NewId,
	    @ResourceTypeId, 
	    @OriginalValue,
	    @ResourceIntId
    end
  set @TotalRows = @@rowcount
  if @SuppressOutput = 0    
    select @NewId as Id, @IsDuplicate As IsDuplicate    
end




GO
/****** Object:  StoredProcedure [dbo].[Resource.ResourceTypeInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*


-- ====================================================================
DECLARE @RC int, @ResourceIntId int, @ResourceTypeId int, @OriginalValue varchar(100),@TotalRows     int

set @ResourceTypeId= 0
set @ResourceIntId = 10
set @OriginalValue = 'Syllabus'

set @ResourceIntId = 1
set @OriginalValue = 'Image/jpg2'



EXECUTE @RC = [dbo].[Resource.ResourceTypeInsert] 
   @ResourceIntId, @ResourceTypeId
  ,@OriginalValue
  ,22
  

GO


*/
/* =============================================
Description:      [Resource.ResourceTypeInsert]
------------------------------------------------------
Modifications
12-09-19 mparsons - added duplicates check (multiple inputs can map to same output)
13-08-22 mparsons - dropped resourceId
=============================================

*/
CREATE PROCEDURE [dbo].[Resource.ResourceTypeInsert]
		  @ResourceIntId int,
          @ResourceTypeId	int,
          @OriginalValue varchar(100)
          ,@CreatedById int
As

declare @NewId uniqueidentifier
, @mapperId int
, @exists uniqueidentifier
, @IsDuplicate bit
, @RecordCount int
, @SuppressOutput bit
, @ErrorMsg varchar(100)
--, @CreatedById int
--set @CreatedById = 0

if @OriginalValue = '' set @OriginalValue = NULL
If @ResourceTypeId < 1		SET @ResourceTypeId = NULL 
If @CreatedById = 0		SET @CreatedById = NULL
else set @SuppressOutput  = 0
If @ResourceTypeId < 1		SET @ResourceTypeId = NULL 
  
If @OriginalValue is NULL and @ResourceTypeId is null begin
  print 'no values provided'
  return -1
  end    


set @IsDuplicate= 0
--set @TotalRows= 0
	set @ErrorMsg = ''
-- ==================================================================
if @ResourceTypeId is null begin
	print 'insert via ResourceType'
	 -- so first check if mapping exists
	SELECT @ResourceTypeId = isnull(base.id,0)
	  FROM [dbo].[Codes.ResourceType] base
	  inner join [dbo].[Map.ResourceType] mapper on base.Id = mapper.[CodeId]
	  where mapper.LRValue = @OriginalValue
   
	If @ResourceTypeId is null OR @ResourceTypeId = 0	begin	
		--no mapping, write to exceptions table and return
		-- when called from interface, may need real message?
		if NOT exists(SELECT [OriginalValue] FROM [dbo].[Audit.ResourceType_Orphan] 
		where [ResourceIntId]= @ResourceIntId and [OriginalValue] = @OriginalValue) begin
		set @ErrorMsg = 'No mapping was found for parameter, writing to audit table'
		  print '@@ no mapping, writing to audit table: ' + @OriginalValue
		  INSERT INTO [dbo].[Audit.ResourceType_Orphan]
			   ([RowId]
			   ,[ResourceIntId]
			   ,[OriginalValue])
		  VALUES
			   (newId()
			   ,@ResourceIntId
			   ,@OriginalValue)
      
		  end    
		else begin
		  print '@@ no mapping, ALREADY IN audit table: ' + @OriginalValue
		  set @ErrorMsg = 'No mapping was found for parameter, the value already exists in audit table'
		  end

    --return -1
    if @SuppressOutput = 0
      select '' as Id, 0 As IsDuplicate, @ErrorMsg
    end
	end

if @ResourceTypeId > 0 begin
  print '[ResourceTypeId] = ' + convert(varchar, @ResourceTypeId)

  set @NewId= NEWID()
  print '@NewId: ' + convert(varchar(50), @NewId)

-- exists check for dups, or allow fail on dup
  select @exists = base.[RowId]
	from [dbo].[Resource.ResourceType] base
	where ( base.[ResourceIntId] = @ResourceIntId )
	And base.[ResourceTypeId] = @ResourceTypeId
	
	if @exists is not NULL AND @exists != '00000000-0000-0000-0000-000000000000' begin
		  set @NewId= @exists
		  set @IsDuplicate= 1
		  print 'found duplicate'
		  end
	else begin
		INSERT INTO [dbo].[Resource.ResourceType]
			([RowId]
			,[ResourceIntId]
			,[ResourceTypeId]
			,[OriginalType]
			,CreatedById)

		select
			@NewId,
			@ResourceIntId,
			@ResourceTypeId, 
			@OriginalValue,
			@CreatedById
		end
  --set @TotalRows = @@rowcount
  if @SuppressOutput = 0    
    select @NewId as Id, @IsDuplicate As sDuplicate, @ErrorMsg As [Message]    
end



GO
/****** Object:  StoredProcedure [dbo].[Resource.ResourceTypeSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[Resource.ResourceTypeSelect] 1


[Resource.ResourceTypeSelect] AND [Resource.ResourceTypeSelect] are identical now, do we need both???
*/

CREATE PROCEDURE [dbo].[Resource.ResourceTypeSelect]
	@ResourceIntId int
	
AS
BEGIN
	SET NOCOUNT ON;
	
	--IF @ResourceIntId = 0 SET @ResourceIntId = NULL

    SELECT RowId, ResourceIntId, ResourceTypeId, OriginalType, Created, CreatedById
	,crt.Title AS MappedValue
    FROM [Resource.ResourceType] rrt
	INNER JOIN [Codes.ResourceType] crt ON rrt.ResourceTypeId = crt.Id

    WHERE 
	(ResourceIntId = @ResourceIntId)
    --(ResourceIntId = @ResourceIntId OR @ResourceIntId IS NULL)

END


GO
/****** Object:  StoredProcedure [dbo].[Resource.ResourceTypeSelect2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [dbo].[Resource.ResourceTypeSelect2]
	@ResourceIntId int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	

    SELECT RowId, ResourceIntId, ResourceTypeId, OriginalType, Created, CreatedById

    FROM [Resource.ResourceType]
    WHERE 
    (ResourceIntId = @ResourceIntId)

    Order by ResourceIntId
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.Standard_Import]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2/18/2013
-- Description:	[Resource.Standard_Import]
--
-- 2013-03-14 jgrimmer - Added ResourceIntId
-- 2013-04-17 jgrimmer - Added AlignmentTypeValue
-- 2014-08-14 jgrimmer - Removed ResourceId, as we're using ResourceIntId instead
-- 2014-11-06 jgrimmer - Changed @OriginalValue to @StandardUrl, and added @NotationCode parameter
-- =============================================
CREATE PROCEDURE [dbo].[Resource.Standard_Import]
	@StandardId int,
	@StandardUrl varchar(100),
	@NotationCode varchar(100),
	@TotalRows int output,
	@ResourceIntId int,
	@AlignmentTypeValue varchar(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @NewId uniqueidentifier,
		@mapperId int,
		@exists int,
		@alignmentTypeId int,
		@OriginalValue varchar(100)
		
	SET @NewId = NEWID()
	SET @TotalRows = 0
	IF @StandardUrl = '' SET @StandardUrl = NULL
	IF @NotationCode = '' SET @NotationCode = NULL
	IF @StandardId < 1 SET @StandardId = NULL
	
	IF @StandardUrl IS NULL AND @StandardId IS NULL AND @NotationCode IS NULL BEGIN
		PRINT 'no values provided'
		return -1
	END
	
--	=====================================================================
--	First see if mapping exists for standard
	PRINT 'checking for mapping'
	if @StandardId IS NULL BEGIN
		PRINT 'checking for mapping'
		SELECT @StandardId = isnull(Id,0)
		FROM [StandardBody.Node] mapper
		WHERE mapper.NotationCode = @StandardUrl OR mapper.StandardGuid = @StandardUrl
			OR mapper.StandardUrl = @StandardUrl OR mapper.AltUrl = @StandardUrl
		PRINT '@StandardId = '+convert(varchar,@StandardId)	
		IF @StandardId = 0 OR @StandardId IS NULL BEGIN
		-- check notationCode
			SELECT @StandardId = isnull(Id,0)
			FROM [StandardBody.Node] mapper
			WHERE mapper.NotationCode = @NotationCode OR mapper.StandardGuid = @NotationCode
				OR mapper.StandardUrl = @NotationCode OR mapper.AltUrl = @NotationCode
			PRINT '@StandardId = '+convert(varchar,@StandardId)
			IF @StandardId = 0 OR @StandardId IS NULL BEGIN --quepasa
			-- no mapping, write to archive table and return
			-- when called from interface, may need real message?
				IF NOT exists(SELECT [ResourceIntId] FROM [Audit.ResourceStandard_Orphan]
					WHERE ResourceIntId = @ResourceIntId AND StandardUrl = @StandardUrl) BEGIN
					PRINT '@@ no mapping, writing to audit table: ' + @StandardUrl
					INSERT INTO [Audit.ResourceStandard_Orphan] (ResourceIntId, StandardUrl, NotationCode, StandardAlignment, Created)
					VALUES (@ResourceIntId, @StandardUrl, @NotationCode, @AlignmentTypeValue, GETDATE())
					SET @TotalRows = @@rowcount
					RETURN 0
				END ELSE BEGIN
					PRINT '@@ no mapping, ALREADY IN audit table: ' + @StandardUrl
					SET @TotalRows = @@rowcount
					RETURN 0
				END
			END ELSE SET @OriginalValue = @NotationCode
		END ELSE SET @OriginalValue = @StandardUrl
	END

-- Second, see if mapping exists for Alignment Type (default value = 'Teaches')
	PRINT 'checking for alignment type'
	IF @AlignmentTypeValue IS NULL OR @AlignmentTypeValue = '' SET @AlignmentTypeValue = 'Teaches'
	SELECT @alignmentTypeId = ISNULL(CodeId,0)
	FROM [Map.AlignmentType] mapper
	WHERE mapper.LRValue = @AlignmentTypeValue
	PRINT '@alignmentTypeId = '+convert(varchar,@alignmentTypeId)
	IF @alignmentTypeId = 0 OR @alignmentTypeId IS NULL BEGIN
		-- Default to 'Teaches'
		SELECT @alignmentTypeId = CodeId
		FROM [Map.AlignmentType] mapper
		WHERE mapper.LRValue = 'Teaches'
	END
	
--	Check for duplicate in Resource.Standard table
	SELECT @exists = Id
	FROM [Resource.Standard]
	WHERE ResourceIntId = @ResourceIntId AND StandardId = @StandardId
	
	IF @exists IS NOT NULL AND @exists <> 0 BEGIN
		print 'found duplicate'
		SET @TotalRows = 0
	END ELSE BEGIN
		print 'adding to Standards table'
		INSERT INTO [Resource.Standard] (ResourceIntId, StandardId, StandardUrl, AlignmentTypeCodeId, Created, CreatedById)
		VALUES (@ResourceIntId, @StandardId, @OriginalValue, @alignmentTypeId, getdate(), NULL)
		SET @TotalRows = @@rowcount
	END
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.StandardDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Resource.StandardDelete]
        @Id int
As
DELETE FROM [Resource.Standard]
WHERE Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.StandardGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[Resource.StandardGet] 17
*/
CREATE PROCEDURE [dbo].[Resource.StandardGet]
    @Id int
As
SELECT 
    base.Id, 
    ResourceIntId, 
    StandardId, 
    ss.NotationCode,
    ss.StandardUrl,
    ss.Description As [Standard],
    
    base.AlignmentTypeCodeId,
    cat.Title As AlignmentTypeValue,
    
    base.AlignmentDegreeId, 
    cad.Title As AlignmentDegree,
    cad.Title As DegreeOfAlignment,
    AlignedById
        
FROM [Resource.Standard] base
inner join [StandardBody.Node] ss on base.StandardId = ss.Id
left join [Codes.AlignmentType] cat on base.AlignmentTypeCodeId = cat.Id
left join [Codes.AlignmentDegree] cad on base.AlignmentDegreeId = cad.Id
WHERE base.Id = @Id

GO
/****** Object:  StoredProcedure [dbo].[Resource.StandardInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[Resource.StandardInsert]
            @ResourceIntId int, 
            @StandardId int, 
            @StandardUrl varchar(200), 
            @AlignedById int,
            @AlignmentTypeCodeId int,
            @AlignmentDegreeId int
As
-- @ResourceId uniqueidentifier, 
If @StandardId = 0   SET @StandardId = NULL 
If @StandardUrl = ''   SET @StandardUrl = NULL 
If @AlignedById = 0   SET @AlignedById = NULL 
If @AlignmentTypeCodeId = 0 SET @AlignmentTypeCodeId = NULL
If @AlignmentDegreeId = 0 SET @AlignmentDegreeId = 2


INSERT INTO [Resource.Standard] (
    ResourceIntId, 
    StandardId, 
    StandardUrl, 
    AlignedById,
    AlignmentTypeCodeId,
    AlignmentDegreeId
)
Values (
    @ResourceIntId,
    @StandardId, 
    @StandardUrl, 
    @AlignedById,
    @AlignmentTypeCodeId,
    @AlignmentDegreeId
)
select SCOPE_IDENTITY() as Id 
--select @newId as Id


GO
/****** Object:  StoredProcedure [dbo].[Resource.StandardSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.StandardSelect] 444671
*/
CREATE PROCEDURE [dbo].[Resource.StandardSelect]
    @ResourceIntId int
As

SELECT 
    base.Id, 
    ResourceIntId, 
    StandardId, 
    ss.NotationCode,
    ss.StandardUrl,
    ss.Description As [Standard],
    
    base.AlignmentTypeCodeId,
    cat.Title As AlignmentTypeValue,
    
    base.AlignmentDegreeId, 
    cad.Title As AlignmentDegree,
    cad.Title As DegreeOfAlignment,
    AlignedById
        
FROM [Resource.Standard] base
inner join [StandardBody.Node] ss on base.StandardId = ss.Id
left join [Codes.AlignmentType] cat on base.AlignmentTypeCodeId = cat.Id
left join [Codes.AlignmentDegree] cad on base.AlignmentDegreeId = cad.Id
WHERE ResourceIntId = @ResourceIntId
Order by AlignmentTypeValue



GO
/****** Object:  StoredProcedure [dbo].[Resource.Subject_PopulateFromMapping]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*

select count(*) from [Resource]
go

SELECT top 1000
 base.[ResourceIntId]
      ,[ClusterId]
      ,[ResourceUrl]
      ,[Title]
      ,[Description]
      ,keys.[Keywords]
      ,subs.[Subjects]
  FROM [Resource.Version_Summary] base
  Left join [dbo].[Resource.Cluster] rclu 
          on base.ResourceIntId = rclu.ResourceIntId And rclu.[ClusterId] = 91
	Left Join [dbo].[Resource.KeywordsCsvList] keys on base.ResourceIntId = keys.ResourceIntId
	Left Join [dbo].[Resource.SubjectsCsvList] subs on base.ResourceIntId = subs.ResourceIntId
where rclu.[ClusterId] is not null
  
  
GO

UPDATE [dbo].[Map.Subject]
   SET [IsActive] = 1
 WHERE Id < 72
GO




EXECUTE [dbo].[Resource.Subject_PopulateFromMapping] 0, 91


EXECUTE [dbo].[Resource.Subject_PopulateFromMapping] 0, 8
 

*/

/*
Map Subjects 
Loop thru defined mapping values, and apply to existing resources

Notes
- should join to resource version to only target resources already in the pathway
- add a date or means to only target recent additions rather than the whole database
	===> would not pick up new filters if we did the latter
*/
CREATE PROCEDURE [dbo].[Resource.Subject_PopulateFromMapping]
            @MaxRecords int
            ,@SubjectId int

As
begin 
          
Declare 
@MapId int
,@MappedSubjectId int
,@Subject varchar(100)
,@FilterValue varchar(200)
,@Filter varchar(200)
,@cntr int
,@interval int
,@debugLevel int
,@affectedCount int
,@totalCount int
set @interval= 25
set @cntr= 0
set @debugLevel= 10
set @affectedCount= 0
set @totalCount= 0

SET NOCOUNT ON;
-- ===============================================
select 'started',  getdate()
	-- Loop thru and call proc
	DECLARE thisCursor CURSOR FOR
      SELECT  FilterValue, MappedSubjectId, base.Id, codes.Title
      FROM   [Map.K12Subject] base
	  Inner join [Codes.Subject] codes on base.MappedSubjectId = codes.Id
      where base.IsActive= 1 
      AND (MappedSubjectId = @SubjectId or @SubjectId = 0)

	OPEN thisCursor
	FETCH NEXT FROM thisCursor INTO @FilterValue, @MappedSubjectId, @MapId, @Subject
	WHILE @@FETCH_STATUS = 0 BEGIN
	  set @cntr = @cntr+ 1
	  if @MaxRecords > 0 AND @cntr > @MaxRecords begin
		  print '### Early exit based on @MaxRecords = ' + convert(varchar, @MaxRecords)
		  select 'exiting',  getdate()
		  set @cntr = @cntr - 1
		  BREAK
		  End	  
		  
    select @cntr As Cntr,convert(varchar, @MappedSubjectId) As SubjectId,@FilterValue	As Filter
    print convert(varchar, @cntr) + '. Subject/Filter: ' + convert(varchar, @MappedSubjectId) + ' - ' + @FilterValue		
		set @Filter = '%' + ltrim(rtrim(@FilterValue)) + '%'
		
    -- === via title =======================================================  		
    INSERT INTO [dbo].[Resource.Subject]
               ([ResourceIntId]
               ,[CodeId], Subject)
    SELECT distinct lrs.ResourceIntId,  @MappedSubjectId, @Subject
    FROM dbo.[Resource.Version_Summary] lrs 
    left join [dbo].[Resource.Subject] rc on lrs.ResourceIntId = rc.ResourceIntId AND rc.CodeId = @MappedSubjectId
	--Left Join [dbo].[Resource.KeywordsCsvList] keys on lrs.ResourceIntId = keys.ResourceIntId
	--Left Join [dbo].[Resource.SubjectsCsvList] subs on lrs.ResourceIntId = subs.ResourceIntId
    where rc.[CodeId] is null
    And (lrs.Title like @Filter 
         OR lrs.[Description] like @Filter
        -- OR keys.Keywords like @Filter
        -- OR subs.Subjects like @Filter
         )
    set @affectedCount = @@rowcount
    if @affectedCount is not null AND @affectedCount > 0 begin
      set @totalCount= @totalCount + @affectedCount
      print '-> match found for Subject using filter. Count: '  
              + convert(varchar, @affectedCount)  + '. #' 
              + convert(varchar, @MappedSubjectId) + ' - ' + @Filter
      --if ((@dupsCntr / @interval) * @interval) = @dupsCntr 
      
      end

    -- todo - once run, could arbitrarily set to false?
    --      - probably only useful for initial runs, as in prod will run regularly against new records
    
  	-- === via keywords =======================================================
    INSERT INTO [dbo].[Resource.Subject]
               ([ResourceIntId]
               ,[CodeId], Subject)
    SELECT distinct lrs.ResourceIntId,  @MappedSubjectId, @Subject
    FROM dbo.[Resource.Keyword] lrs 
    left join [dbo].[Resource.Subject] rc on lrs.ResourceIntId = rc.ResourceIntId AND rc.CodeId = @MappedSubjectId
    where rc.[CodeId] is null
    And lrs.Keyword like @Filter

  	set @affectedCount = @@rowcount
    if @affectedCount is not null AND @affectedCount > 0 begin
      set @totalCount= @totalCount + @affectedCount
      print 'KKKKKKK match found on keywords for Subject. Count: '  
              + convert(varchar, @affectedCount)  + '. #' 
	  end
    
    
    -- === via subjects =======================================================   
	-- different - probably an update 
	UPDATE [dbo].[Resource.Subject]
		SET [CodeId] = @MappedSubjectId
	From [dbo].[Resource.Subject] base
	where base.[CodeId] is null
    And base.Subject like @Filter

	set @affectedCount = @@rowcount
    if @affectedCount is not null AND @affectedCount > 0 begin
      set @totalCount= @totalCount + @affectedCount
      print '$$$$$$$ match found on subjects for Subject. Count: '  
              + convert(varchar, @affectedCount)  + '. #' 
	  end
    
  	 -- =====================================
  	 
	FETCH NEXT FROM thisCursor INTO @FilterValue, @MappedSubjectId, @MapId, @Subject
	END
	CLOSE thisCursor
	DEALLOCATE thisCursor
	select 'completed',  getdate()
  select 'processed records: ' + convert(varchar, @cntr)
  select 'Subjects created: ' + convert(varchar, @totalCount)
  
  
end


GO
/****** Object:  StoredProcedure [dbo].[Resource.Subject_SelectedCodes]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.Subject_SelectedCodes] 448010
*/
Create PROCEDURE [dbo].[Resource.Subject_SelectedCodes]
    @ResourceIntId int
As
SELECT distinct
	code.Id, code.Title, code.[Description]
   -- ,ResourceIntId
	,CASE
		WHEN rpw.ResourceIntId IS NOT NULL THEN 'true'
		ELSE 'false'
	END as IsSelected
FROM [dbo].[Codes.Subject] code
Left Join [Resource.Subject] rpw on code.Id = rpw.CodeId
		and rpw.ResourceIntId = @ResourceIntId
		where code.IsActive = 1

GO
/****** Object:  StoredProcedure [dbo].[Resource.SubjectGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*************************************************************
 * Modifications
 * 12-11-20 jgrimmer - Modified to use new field name in Resource.Subject table
 * 13-04-12 mparsons - changed to use integer PK, and updated columns
 *************************************************************/
CREATE PROCEDURE [dbo].[Resource.SubjectGet]
	--@ResourceId uniqueidentifier
	@ResourceIntId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

SELECT [Id]
      ,[ResourceIntId]
      ,[Subject]
      ,[Created]
      ,[CreatedById]
	  ,CodeId
  FROM [dbo].[Resource.Subject]
 WHERE ResourceIntId = @ResourceIntId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.SubjectImport]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

exec [dbo].[Resource.SubjectInsert2] 449805  ,'Arts'  ,2


*/

/*************************************************************
Modifications
2012-11-15 jgrimmer - Added duplicate checking and keyword check
2013-03-14 jgrimmer - Added ResourceIntId
2014-04-17 mparsons - added handling for k12 subjects
*/
CREATE PROCEDURE [dbo].[Resource.SubjectImport]
	@ResourceIntId int,
	@Subject varchar(300)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @KeywordId int, @CodeId int,
			@exists int
	

	
	--=========================================================
	-- Do keyword check.  If keyword found, delete keyword.
	SELECT @KeywordId = Id
	FROM [Resource.Keyword]
	WHERE ResourceIntId = @ResourceIntId AND Keyword = @Subject
	IF @KeywordId IS NOT NULL AND @KeywordId <> 0 BEGIN
		DELETE FROM [Resource.Keyword]
		WHERE Id = @KeywordId
	END
	--=========================================================
	-- Do K12 subject check.  If found, use codeId.
	SELECT @CodeId = isnull(Id, 0)
	FROM [Codes.Subject]
	WHERE Title = @Subject 
		
	if isnull(@CodeId,0) = 0 set @CodeId = NULL

    --=========================================================
    -- Do Duplicate check.  If already in table, skip the add
    SELECT @exists = Id
    FROM [Resource.Subject]
    WHERE ResourceIntId = @ResourceIntId AND [Subject] = @Subject
   	if @exists is not NULL AND @exists != 0 begin
	  print 'found duplicate'
	end
	else begin
	    INSERT INTO [Resource.Subject] (ResourceIntId, [Subject], Created, CreatedById, CodeId)
		VALUES (@ResourceIntId, @Subject, GETDATE(), NULL, @CodeId)
	end    
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.SubjectInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*


[dbo].[Resource.SubjectInsert] NULL,'fishsticks', 288525    

exec [dbo].[Resource.SubjectInsert] NULL,'REST', 122848    


*/
/*************************************************************
Modifications
2012-11-15 jgrimmer - Added duplicate checking and keyword check
2013-03-14 jgrimmer - Added ResourceIntId
2013-04-11 mparsons - change to use integer keys. Will be replaced by [Resource.SubjectInsert2]
*/
CREATE PROCEDURE [dbo].[Resource.SubjectInsert]
	@ResourceId uniqueidentifier,
	@Subject varchar(300),
	@ResourceIntId int = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @KeywordId int,
			@exists int
	
	IF @ResourceIntId = 0 SELECT @ResourceIntId = Id FROM [Resource] WHERE RowId = @ResourceId
	
	--=========================================================
	-- Do keyword check.  If keyword found, delete keyword.
	SELECT @KeywordId = isnull(Id, 0)
	FROM [Resource.Keyword]
	WHERE ResourceIntId = @ResourceIntId 
	AND Keyword = @Subject
	
	IF @KeywordId IS NOT NULL AND @KeywordId > 0 BEGIN
		DELETE FROM [Resource.Keyword]
		WHERE Id = @KeywordId
	END

  --=========================================================
  -- Do Duplicate check.  If already in table, skip the add
  SELECT @exists = Id
  FROM [Resource.Subject]
  WHERE ResourceIntId = @ResourceIntId AND [Subject] = @Subject
 	if @exists is not NULL AND @exists > 0 begin
    print 'found duplicate'
  end
  else begin
      INSERT INTO [Resource.Subject] ( ResourceIntId, [Subject])
	  VALUES ( @ResourceIntId, @Subject )
  end   
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.SubjectInsert2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
select top 100 * from [Resource.PublishedBy] 
where PublishedById = 2
--

SELECT rs.[Id],rs.[ResourceIntId]
      ,PublishedById
      ,rs.[Subject],rs.[Created]

  FROM [dbo].[Resource.Subject] rs
  inner join [Resource.PublishedBy] rp on rs.ResourceIntId = rp.ResourceIntId
where 
PublishedById= 2  
  order by 2, 3
  --
  
SELECT rs.[Id],rs.[ResourceIntId]
      ,PublishedById
      ,rs.[Keyword],rs.[Created]

  FROM [dbo].[Resource.Keyword] rs
  inner join [Resource.PublishedBy] rp on rs.ResourceIntId = rp.ResourceIntId
where 
PublishedById= 2

--rs.[ResourceIntId] = 288525  
  order by 2, 3
 
 
 
exec [dbo].[Resource.SubjectInsert2] 449805  ,'Arts'  ,2




*/
/*************************************************************
Modifications
2012-11-15 jgrimmer - Added duplicate checking and keyword check
2013-03-14 jgrimmer - Added ResourceIntId
2014-04-17 mparsons - added handling for k12 subjects
*/
CREATE PROCEDURE [dbo].[Resource.SubjectInsert2]
	@ResourceIntId int,
	@Subject varchar(100),
	@CreatedById int
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @KeywordId int, @CodeId int,
			@exists int
	
	--IF @ResourceIntId = 0 SELECT @ResourceIntId = Id FROM [Resource] WHERE RowId = @ResourceId
	
	--=========================================================
	-- Do keyword check.  If keyword found, delete keyword.
	SELECT @KeywordId = isnull(Id, 0)
	FROM [Resource.Keyword]
	WHERE ResourceIntId = @ResourceIntId 
	AND Keyword = @Subject
	
	IF @KeywordId IS NOT NULL AND @KeywordId > 0 BEGIN
		DELETE FROM [Resource.Keyword]
		WHERE Id = @KeywordId
	END

	--=========================================================
	-- Do K12 subject check.  If found, use codeId.
	SELECT @CodeId = isnull(Id, 0)
	FROM [Codes.Subject]
	WHERE Title = @Subject 
		
	if isnull(@CodeId,0) = 0 set @CodeId = NULL

  --=========================================================
  -- Do Duplicate check.  If already in table, skip the add
  SELECT @exists = Id
  FROM [Resource.Subject]
  WHERE ResourceIntId = @ResourceIntId AND [Subject] = @Subject
 	if @exists is not NULL AND @exists > 0 begin
    print 'found duplicate'
  end
  else begin
      INSERT INTO [Resource.Subject] ( ResourceIntId, [Subject], CreatedById, CodeId)
	  VALUES ( @ResourceIntId, @Subject, @CreatedById, @CodeId)
  end    
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.SubjectSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.SubjectSelect] 444671
*/
Create PROCEDURE [dbo].[Resource.SubjectSelect]
    @ResourceIntId int
As

SELECT [Id]
      ,[ResourceIntId]
      ,[Subject]
      ,[Created]
      ,[CreatedById]
  FROM [dbo].[Resource.Subject]
 WHERE ResourceIntId = @ResourceIntId
 Order by [Subject]



GO
/****** Object:  StoredProcedure [dbo].[Resource.Version_Display]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
select top 10 * from [Resource.Version]

exec [Resource.Version_Display] '063438CA-9FF4-42B9-A511-000028286F04'

[Resource.Version_Display] 2

*/


/* ============================================================
-- Description:	Get a resource version for display
-- 12-09-02 mparsons - added 
-- 12-10-10 mparsons - changed to use Resource.Subject, added Keywords
-- 13-01-31 mparsons - added join for new AccessRightsId
-- 13-02-13 mparsons - added return of res.Id
-- 13-02-19 mparsons - added InteractivityTypeId
-- 13-03-12 mparsons - removed keywords and subject for performanance
-- 13-06-12 mparsons - probably obsolete, but updated for int id just in case
-- 13-10-21 mparsons - used in webservice prototype, so updated to replace ed levels with grade levels
-- ============================================================
*/
CREATE PROCEDURE [dbo].[Resource.Version_Display]
	@Id int
AS

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT distinct
		base.RowId, 
		base.Id,
		res.Id As ResourceIntId,
		res.ResourceUrl, 
		res.ViewCount,
		res.FavoriteCount,
		base.DocId,
		base.Title, base.SortTitle,
		base.[Description], 
		isnull(base.Publisher, 'Unknown') As Publisher,
		isnull(base.Creator, 'Unknown') As Creator,
		isnull(base.Rights, 'Unknown') As Rights,
		--isnull(base.AccessRights, 'Unknown') As AccessRights,
		AccessRightsId,
		isnull(codes.title, 'Unknown') As AccessRights,
	    
		isnull(InteractivityTypeId, 0) As InteractivityTypeId,
		isnull(iatCodes.title, 'Unknown') As InteractivityType,
          
		isnull(base.TypicalLearningTime, '') As TypicalLearningTime,
		base.Modified, base.[Schema],
		base.Submitter, 
		isnull(base.Imported, base.Modified) As Imported,
		base.Created,
		base.IsSkeletonFromParadata
		,base.IsActive
		,res.IsActive As ResourceIsActive
		
		--,replace(isnull(rsub.Subjects, ''), '","', ', ') As Subjects
		,'' As Subjects
		,isnull(edList.GradeLevels,'') As GradeLevels
		--	,replace(isnull(rkwd.Keywords, ''), '","', ', ') As Keywords
		,'' As Keywords
		--,'TBD' As Keywords
		
		,isnull(langList.LanguageList,'') As LanguageList
		,isnull(typesList.ResourceTypesList,'') As ResourceTypesList
		,isnull(audienceList.AudienceList,'') As AudienceList
			
	FROM [Resource.Version] base		
	inner join [Resource] res on base.ResourceIntId = res.Id
	Left Join dbo.[Codes.AccessRights] codes on base.AccessRightsId = codes.Id
  Left Join dbo.[Codes.InteractivityType] iatCodes on base.InteractivityTypeId = codes.Id
	--Left Join [dbo].[Resource.SubjectCsv] rsub on res.RowId = rsub.ResourceId
	--Inner Join [dbo].[Resource.SubjectsCsvList] rsub on res.RowId = rsub.ResourceId
	--Left Join [dbo].[Resource.KeywordCsv] rkwd on res.RowId = rkwd.ResourceId
	--Left Join [dbo].[Resource.KeywordsCsvList] rkwd on res.RowId = rkwd.ResourceId
  Left Join [dbo].[Resource.GradeLevelList] edList on res.Id = edList.ResourceIntId
    Left Join [dbo].[Resource.LanguagesList] langList on res.Id = langList.ResourceIntId
    Left Join [dbo].[Resource.IntendedAudienceList] audienceList on res.Id = audienceList.ResourceIntId
    Left Join [dbo].[Resource.ResourceTypesList] typesList on res.Id = typesList.ResourceIntId
        
	WHERE (base.Id = @Id)
	
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.Version_Import]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 03/1/2013 - based on [Resource.VersionInsert]
-- Description:	Insert a ResourceVersion record
-- 2012-11-08 jgrimmer - Added Schema field
-- 2012-12-13 jgrimmer - Added logic to update SortTitle
-- 2013-01-14 mparsons - Setting AccessRights to Unknown if NULL 
-- 2013-02-01 jgrimmer - fixed bug where AccessRightsId is not inserted into the table
-- 2013-02-19 mparsons - added @InteractivityTypeId - need to coordinate with live import
-- 2013-03-01 jgrimmer - added @InteractivityType and code to map to @InteractivityTypeId.  Also modified so it could do both add and update
-- 2013-05-03 jgrimmer - Added @ResourceIntId (how did I miss this one?!)
-- 2013-06-20 jgrimmer - Removed ResourceId
-- 2014-02-24 jgrimmer - changed Title and Description to nvarchar to handle foreign languages such as Arabic.
-- 2014-04-17 mparsons - removed @ResourceId 
-- =============================================
CREATE PROCEDURE [dbo].[Resource.Version_Import]
	@RowId	uniqueidentifier,
	@DocId varchar(500),
	@Title nvarchar(300),
	@Description nvarchar(max),
	@Publisher varchar(100),
	@Creator varchar(100),
	@Rights varchar(500),
	@AccessRights varchar(100),
	@Modified datetime,
	@Submitter varchar(100),
	@Created datetime,
	@TypicalLearningTime varchar(50),
	@IsSkeletonFromParadata bit,
	@Schema varchar(50)
	,@AccessRightsId int
	,@InteractivityTypeId int
	,@InteractivityType varchar(100)
	,@ResourceIntId int
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @RowId = '00000000-0000-0000-0000-000000000000' SET @RowId = NULL
	IF @Publisher = '' SET @Publisher = NULL
	IF @Creator = '' SET @Creator = NULL
	IF @Rights = '' SET @Rights = NULL
	IF @Submitter = '' SET @Submitter = NULL
	IF @TypicalLearningTime = '' SET @TypicalLearningTime = NULL
	IF @Schema = '' SET @Schema = NULL
	--should we have a default?
	IF @InteractivityTypeId = 0 SET @InteractivityTypeId = NULL
	
	IF @InteractivityTypeId IS NULL
		SELECT @InteractivityTypeId = CodeId
		FROM [Map.InteractivityType]
		WHERE IsActive = 'True' AND OriginalValue = @InteractivityType
		
	if @AccessRightsId > 0 begin
	  IF @AccessRights is NULL OR rtrim(@AccessRights) = '' begin
	    SELECT @AccessRights = isnull(codes.Title,'Unknown')
      FROM dbo.[Codes.AccessRights] codes where codes.Id = @AccessRightsId
      end
	  end
	else begin 
    IF @AccessRights is NULL OR rtrim(@AccessRights) = '' SET @AccessRights = 'Unknown'
	  SELECT @AccessRightsId = isnull(codes.Id,8)
    FROM dbo.[Codes.AccessRights] codes where codes.Title = @AccessRights
      
		end
	IF @ResourceIntId = 0 SET @ResourceIntId = NULL
		
	DECLARE @NewId uniqueidentifier
	SET @NewId = NEWID()
	
	IF @RowId IS NULL BEGIN
		INSERT INTO [Resource.Version] (RowId, DocId, Title, [Description],
			Publisher, Creator, Rights, AccessRights, Modified, Submitter, Imported, Created,
			TypicalLearningTime, IsSkeletonFromParadata, [Schema], SortTitle
			, AccessRightsId
			, InteractivityTypeId
			, InteractivityType
			, ResourceIntId)
		
		VALUES (@NewId, @DocId, @Title, @Description, 
			@Publisher, @Creator, @Rights, @AccessRights, @Modified, @Submitter, GETDATE(),	@Created, 
			@TypicalLearningTime, @IsSkeletonFromParadata, @Schema, dbo.BuildSortTitle(@Title)
			, @AccessRightsId
			, @InteractivityTypeId
			, @InteractivityType
			, @ResourceIntId)
	END ELSE BEGIN
		UPDATE [Resource.Version]
		SET DocId = @DocId,
			Title = @Title,
			[Description] = @Description,
			Publisher = @Publisher,
			Creator = @Creator,
			Rights = @Rights,
			AccessRights = @AccessRights,
			Modified = GETDATE(),
			Created = @Created,
			TypicalLearningTime = @TypicalLearningTime,
			IsSkeletonFromParadata = @IsSkeletonFromParadata,
			[Schema] = @Schema,
			SortTitle = dbo.BuildSortTitle(@Title),
			AccessRightsId = @AccessRightsId,
			InteractivityType = @InteractivityType,
			InteractivityTypeId = @InteractivityTypeId
		WHERE RowId = @RowId
	END	
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.VersionDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 09/11/2012
-- Description:	Delete a version of a resource
-- =============================================
CREATE PROCEDURE [dbo].[Resource.VersionDelete]
	@RowId uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DELETE FROM [Resource.Version]
	WHERE RowId = @RowId
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.VersionDescriptionFix]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/6/2013
-- Description:	Removes special characters from descriptions in the SQL database
-- =============================================
CREATE PROCEDURE [dbo].[Resource.VersionDescriptionFix]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE [Resource.Version]
	SET [Description] = dbo.StripNonalphanumericCharactersDescription([Description])
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.VersionGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
select top 10 * from [Resource.Version]

exec [Resource.VersionGet] '063438CA-9FF4-42B9-A511-000028286F04'

[Resource.Version_Get]  '261F947B-25D4-4976-9A0C-00007994619C'
go
[Resource.VersionGet]  '3674fb51-ae70-454f-94bb-1929e4629230'



*/

-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/30/2012
-- Description:	Get Resource info from ResourceVersion (and other tables/views)
-- 13-01-31 mparsons - added join for new AccessRightsId
-- 13-02-13 mparsons - added return of res.Id
-- 13-02-19 mparsons - added InteractivityTypeId
-- 13-03-28 mparsons - added Requirements, fixed InteractivityTypeId
-- =============================================
CREATE PROCEDURE [dbo].[Resource.VersionGet]
	@RowId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
	  base.RowId,
	  base.Id,
    NULL As ResourceId, 
    res.Id As ResourceIntId,
		res.ResourceUrl,
		res.ViewCount,
		res.FavoriteCount,
		base.DocId,
		base.Title, base.SortTitle,
		base.[Description],
		ISNULL(base.Publisher,'Unknown') AS Publisher,
		ISNULL(base.Creator,'Unknown') AS Creator,
		ISNULL(base.Rights,'Unknown') AS Rights,
		AccessRightsId,
    isnull(codes.title, 'Unknown') As AccessRights,
    
    isnull(InteractivityTypeId, 0) As InteractivityTypeId,
    isnull(iatCodes.title, 'Unknown') As InteractivityType,
    
		ISNULL(base.TypicalLearningTime, 'Unknown') AS TypicalLearningTime,
		base.Modified,base.[Schema],

		base.Submitter,
		ISNULL(base.Imported, base.Modified) AS Imported,
		base.Created,
		base.IsSkeletonFromParadata
	  ,Requirements
				
		--==== get related tablelstuff, may not be needed anymore??
		,replace(isnull(rsub.Subjects, ''), '","', ', ') As Subjects
	,'' As Subjects2
		,isnull(edList.EducationLevels,'') As EducationLevels
		,replace(isnull(rkwd.Keywords, ''), '","', ', ') As Keywords
	--,'' As Keywords
		--,'TBD' As Keywords
		
		,isnull(langList.LanguageList,'') As LanguageList
		,isnull(typesList.ResourceTypesList,'') As ResourceTypesList
		,isnull(audienceList.AudienceList,'') As AudienceList

	FROM [Resource.Version] base
	inner join [Resource] res on base.ResourceIntId = res.Id
	Left Join dbo.[Codes.AccessRights] codes on base.AccessRightsId = codes.Id
  Left Join dbo.[Codes.InteractivityType] iatCodes on base.InteractivityTypeId = iatCodes.Id
	--Left Join [dbo].[Resource.SubjectCsv] rsub2 on res.RowId = rsub.ResourceId
	Left Join [dbo].[Resource.SubjectsCsvList] rsub on res.Id = rsub.ResourceIntId
	--Left Join [dbo].[Resource.KeywordCsv] rkwd2 on res.RowId = rkwd.ResourceId
	Left Join [dbo].[Resource.KeywordsCsvList] rkwd on res.Id = rkwd.ResourceIntId
  Left Join [dbo].[Resource.EducationLevelsList] edList on res.Id = edList.ResourceIntId
    Left Join [dbo].[Resource.LanguagesList] langList on res.Id = langList.ResourceIntId
    Left Join [dbo].[Resource.IntendedAudienceList] audienceList on res.Id = audienceList.ResourceIntId
    Left Join [dbo].[Resource.ResourceTypesList] typesList on res.Id = typesList.ResourceIntId
	WHERE base.RowId = @RowId
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.VersionGetById]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
select top 10 * from [Resource.Version]


exec [Resource.VersionGetById] 213385

[Resource.VersionGetById] 445731

--has requirements
[Resource.VersionGetById] 1149

*/


-- =============================================
-- Description:	Get Resource info from ResourceVersion 

-- 13-03-23 mparsons - added 
-- 13-03-28 mparsons - added Requirements, fixed InteractivityTypeId
-- =============================================
CREATE PROCEDURE [dbo].[Resource.VersionGetById]
	@Id int
AS
BEGIN

	SET NOCOUNT ON;

	SELECT 
	  base.RowId,
	  base.Id,
    NULL As ResourceId, 
    res.Id As ResourceIntId,
		res.ResourceUrl,
		res.ViewCount,
		res.FavoriteCount,
		base.DocId,
		base.IsActive,
		res.IsActive As ResourceIsActive,
		base.Title, base.SortTitle,
		base.[Description],
		ISNULL(base.Publisher,'Unknown') AS Publisher,
		ISNULL(base.Creator,'Unknown') AS Creator,
		ISNULL(base.Rights,'Unknown') AS Rights,
		AccessRightsId,
    isnull(codes.title, 'Unknown') As AccessRights,
    
    isnull(InteractivityTypeId, 0) As InteractivityTypeId,
    isnull(iatCodes.title, 'Unknown') As InteractivityType,
    
		ISNULL(base.TypicalLearningTime, 'Unknown') AS TypicalLearningTime,
		base.Modified,base.[Schema],

		base.Submitter,
		ISNULL(base.Imported, base.Modified) AS Imported,
		base.Created,
		base.IsSkeletonFromParadata
		,Requirements
		
	FROM [Resource.Version] base
	inner join [Resource] res on base.ResourceIntId = res.Id
	Left Join dbo.[Codes.AccessRights] codes on base.AccessRightsId = codes.Id
  Left Join dbo.[Codes.InteractivityType] iatCodes on base.InteractivityTypeId = iatCodes.Id

	WHERE base.Id = @Id
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.VersionInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/30/2012
-- Description:	Insert a ResourceVersion record
-- 2012-11-08 jgrimmer - Added Schema field
-- 2012-12-13 jgrimmer - Added logic to update SortTitle
-- 2013-01-14 mparsons - Setting AccessRights to Unknown if NULL 
-- 2013-02-01 jgrimmer - fixed bug where AccessRightsId is not inserted into the table
-- 2013-02-19 mparsons - added @InteractivityTypeId - need to coordinate with live import
-- 2013-03-04 mparsons - added ResourceIntId - used by publish to insert, not import yet
-- 2013-03-15 nargo	   - added Requirements
-- 2013-05-30 nargo    - temp fix for ResourceId
-- 2013-06-27 nargo	   - switched to return integer ID
-- 2014-04-17 mparsons - removed @ResourceId 
-- =============================================
CREATE PROCEDURE [dbo].[Resource.VersionInsert]
	@DocId varchar(500),
	@Title varchar(200),
	@Description varchar(max),
	@Publisher varchar(100),
	@Creator varchar(100),
	@Rights varchar(500),
	@AccessRights varchar(100),
	@Modified datetime,
	@Submitter varchar(100),
	@Created datetime,
	@TypicalLearningTime varchar(50),
	@IsSkeletonFromParadata bit,
	@Schema varchar(50)
	,@AccessRightsId int
	,@InteractivityTypeId int
	,@ResourceIntId  int
	,@Requirements varchar(200)
	
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @Publisher = '' SET @Publisher = NULL
	IF @Creator = '' SET @Creator = NULL
	IF @Rights = '' SET @Rights = NULL
	IF @Submitter = '' SET @Submitter = NULL
	IF @TypicalLearningTime = '' SET @TypicalLearningTime = NULL
	IF @Schema = '' SET @Schema = NULL
	--should we have a default?
	IF @InteractivityTypeId = 0 SET @InteractivityTypeId = NULL
	IF @ResourceIntId = 0 SET @ResourceIntId = NULL
	IF @Requirements = '' SET @Requirements = NULL
		
	if @AccessRightsId > 0 begin
	  IF @AccessRights is NULL OR rtrim(@AccessRights) = '' begin
	    SELECT @AccessRights = isnull(codes.Title,'Unknown')
      FROM dbo.[Codes.AccessRights] codes where codes.Id = @AccessRightsId
      end
	  end
	else begin 
    IF @AccessRights is NULL OR rtrim(@AccessRights) = '' SET @AccessRights = 'Unknown'
	  SELECT @AccessRightsId = isnull(codes.Id,8)
    FROM dbo.[Codes.AccessRights] codes where codes.Title = @AccessRights
      
		end
		
	DECLARE @NewId uniqueidentifier
	SET @NewId = NEWID()
	
	INSERT INTO [Resource.Version] (RowId, 
	DocId, Title, [Description],
		Publisher, Creator, Rights, AccessRights, Modified, Submitter, Imported, Created,
		TypicalLearningTime, IsSkeletonFromParadata, [Schema], SortTitle
		, AccessRightsId
		, InteractivityTypeId
		, ResourceIntId
		, Requirements
		)
		
	VALUES (@NewId, 
	@DocId, @Title, @Description, 
		@Publisher, @Creator, @Rights, @AccessRights, @Modified, @Submitter, GETDATE(),	@Created, 
		@TypicalLearningTime, @IsSkeletonFromParadata, @Schema, dbo.BuildSortTitle(@Title)
		, @AccessRightsId
		, @InteractivityTypeId
		, @ResourceIntId
		, @Requirements
		)
END
--SELECT @NewId AS RowId
SELECT SCOPE_IDENTITY() as Id, @NewId As RowId

GO
/****** Object:  StoredProcedure [dbo].[Resource.VersionSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource.VersionSelect] ' where vers.RowId =  ''3674fb51-ae70-454f-94bb-1929e4629230'' ' 
*/
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/30/2012
-- Description:	Select ResourceVersion rows
--
-- 2012-11-20 jgrimmer - Added SortTitle and Schema
-- 2013-06-20 jgrimmer - Removed reference to Resource.EducationLevelsList
-- 2013-08-29 jgrimmer - Added IsActive
-- 2014-02-25 jgrimmer - Added [Resource.Version].Id
-- =============================================
CREATE PROCEDURE [dbo].[Resource.VersionSelect]
	@Filter varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @sql varchar(max)
	SET @sql = 'SELECT vers.RowId, vers.Id, 
	'''' AS ResourceId, base.Id As ResourceIntId,
	base.ResourceUrl,
	base.ViewCount,
	base.FavoriteCount,
	vers.IsActive,
	vers.InteractivityTypeId,
	vers.InteractivityType,
	vers.Requirements,
	vers.DocId,
	vers.Title,
	vers.[Description],
	isnull(vers.Publisher,''Unknown'') AS Publisher,
	isnull(vers.Creator,''Unknown'') AS Creator,
	isnull(vers.Rights,''Unknown'') AS Rights,
  AccessRightsId,
  isnull(codes.title, ''Unknown'') As AccessRights,
	vers.Modified,
	vers.Submitter,
	isnull(vers.Imported, vers.Modified) AS Imported,
	vers.Created,
	isnull(vers.TypicalLearningTime,''Unknown'') AS TypicalLearningTime,
	vers.IsSkeletonFromParadata,
	vers.SortTitle,
	vers.[Schema],
	isnull(rsub.subjects,'''') AS Subjects,
	'''' AS EducationLevels,
	''TBD'' AS Keywords,
	isnull(langList.LanguageList,'''') AS LanguageList,
	isnull(typesList.ResourceTypesList,'''') AS ResourceTypesList
FROM [Resource] base
INNER JOIN [Resource.Version] vers on base.Id = vers.ResourceIntId
Left Join dbo.[Codes.AccessRights] codes on vers.AccessRightsId = codes.Id
LEFT JOIN [dbo].[Resource.SubjectsCsvList] rsub on base.Id = rsub.ResourceIntId
LEFT JOIN [dbo].[Resource.LanguagesList] langList on base.Id = langList.ResourceIntId
LEFT JOIN [dbo].[Resource.ResourceTypesList] typesList ON base.Id = typesList.ResourceIntId
 ' + @Filter
 
	EXEC(@sql)
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.VersionUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/30/2012
-- Description:	Update a Resource Version
-- 2012-11-08 jgrimmer - Added Schema field
-- 2012-12-13 jgrimmer - Added logic to calculate SortTitle from Title and update it
-- 2013-01-31 mparsons - added handling of AccessRights or AccessRightsId
-- 2013-02-19 mparsons - added @InteractivityTypeId - need to coordinate with live import
-- 2013-02-27 mparsons - removed columns that cannot be updated, like resourceId, docId, created, etc
-- 2013-08-22 mparsons - changed to use int Id. Removed @AccessRights - should only be using the code??
-- 2014-01-02 mparsons - added Requirements
-- =============================================
CREATE PROCEDURE [dbo].[Resource.VersionUpdate]
	@Id int,
	@Title varchar(200),
	@Description varchar(max),
	@Rights varchar(500),
	@AccessRightsId int,
	@TypicalLearningTime varchar(50),
	@Schema varchar(50)
	  ,@InteractivityTypeId int
	  ,@Requirements varchar(200)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @Description = '' SET @Description = NULL

	IF @Rights = '' SET @Rights = NULL
	IF @TypicalLearningTime = '' SET @TypicalLearningTime = NULL
	IF @Schema = '' SET @Schema = NULL
		--should we have a default?
	IF @InteractivityTypeId = 0 SET @InteractivityTypeId = NULL
	IF @AccessRightsId = 0 SET @AccessRightsId = NULL

	--if @AccessRightsId > 0 begin
	--  IF @AccessRights is NULL OR rtrim(@AccessRights) = '' begin
	--	SELECT @AccessRights = isnull(codes.Title,'Unknown')
	--	  FROM dbo.[Codes.AccessRights] codes where codes.Id = @AccessRightsId
	--	end
	--  end
	--else begin 
	--	SET @AccessRightsId = NULL
	--	IF @AccessRights is NULL OR rtrim(@AccessRights) = '' SET @AccessRights = 'Unknown'
	--	  SELECT @AccessRightsId = isnull(codes.Id,8)
	--	FROM dbo.[Codes.AccessRights] codes where codes.Title = @AccessRights
      
	--end
		
	UPDATE [Resource.Version]
	SET 
		Title = @Title,
		[Description] = @Description,
		Rights = @Rights,
		AccessRightsId = @AccessRightsId,
		Modified = getdate(),
		TypicalLearningTime = @TypicalLearningTime,
		InteractivityTypeId = @InteractivityTypeId,
		SortTitle = dbo.BuildSortTitle(@Title),
		Requirements = @Requirements
		
	WHERE Id = @Id
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.VersionUpdateByRowId]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/30/2012
-- Description:	Update a Resource Version
-- 2012-11-08 jgrimmer - Added Schema field
-- 2012-12-13 jgrimmer - Added logic to calculate SortTitle from Title and update it
-- 2013-01-31 mparsons - added handling of AccessRights or AccessRightsId
-- 2013-02-19 mparsons - added @InteractivityTypeId - need to coordinate with live import
-- 2012-02-27 mparsons - removed columns that cannot be updated, like resourceId, docId, created, etc
-- 2013-08-22 mparsons - renamed, preparing for obsoletion
-- =============================================
Create PROCEDURE [dbo].[Resource.VersionUpdateByRowId]
	@RowId uniqueidentifier,
	@Title varchar(200),
	@Description varchar(max),
	@Rights varchar(500),
	@AccessRights varchar(100),
	@AccessRightsId int,
	@TypicalLearningTime varchar(50),
	@Schema varchar(50)
  ,@InteractivityTypeId int
  ,@Modified datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @Description = '' SET @Description = NULL

	IF @Rights = '' SET @Rights = NULL
	IF @TypicalLearningTime = '' SET @TypicalLearningTime = NULL
	IF @Schema = '' SET @Schema = NULL
		--should we have a default?
	IF @InteractivityTypeId = 0 SET @InteractivityTypeId = NULL
	
	if @AccessRightsId > 0 begin
	  IF @AccessRights is NULL OR rtrim(@AccessRights) = '' begin
	    SELECT @AccessRights = isnull(codes.Title,'Unknown')
      FROM dbo.[Codes.AccessRights] codes where codes.Id = @AccessRightsId
      end
	  end
	else begin 
    IF @AccessRights is NULL OR rtrim(@AccessRights) = '' SET @AccessRights = 'Unknown'
	  SELECT @AccessRightsId = isnull(codes.Id,8)
    FROM dbo.[Codes.AccessRights] codes where codes.Title = @AccessRights
      
		end
		
	UPDATE [Resource.Version]
	SET 
		Title = @Title,
		[Description] = @Description,
		--Publisher = @Publisher,
		--Creator = @Creator,
		Rights = @Rights,
		--AccessRights = @AccessRights,
		AccessRightsId = @AccessRightsId,
		Modified = @Modified,
		--Submitter = @Submitter,
		--Imported = @Imported,
		--Created = @Created,
		TypicalLearningTime = @TypicalLearningTime,
		--IsSkeletonFromParadata = @IsSkeletonFromParadata,
		--[Schema] = @Schema,
		InteractivityTypeId = @InteractivityTypeId,
		SortTitle = dbo.BuildSortTitle(@Title)
		
	WHERE RowId = @RowId
END


GO
/****** Object:  StoredProcedure [dbo].[Resource.VersionUpdateDocId]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Create date: 05/24/2013
-- Description:	Update the docId for Resource Version
-- =============================================
Create PROCEDURE [dbo].[Resource.VersionUpdateDocId]
	@Id int,
	@DocId varchar(50)
AS
BEGIN
	SET NOCOUNT ON;

		
	UPDATE [Resource.Version]
	SET 
		DocId = @DocId
		
	WHERE Id = @Id
END

GO
/****** Object:  StoredProcedure [dbo].[Resource.ViewInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/5/2013
-- Description:	Insert a Resource.View row
-- =============================================
CREATE PROCEDURE [dbo].[Resource.ViewInsert]
	@ResourceIntId int,
	@CreatedById int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @ReturnValue int
	INSERT INTO [Resource.View] (ResourceIntId, Created, CreatedById)
	VALUES (@ResourceIntId, GETDATE(), @CreatedById)
	
	SET @ReturnValue = @@IDENTITY
	
	UPDATE [Resource]
	SET ViewCount = ViewCount + 1
	WHERE Id = @ResourceIntId
	
	SELECT @ReturnValue AS Id	
END

GO
/****** Object:  StoredProcedure [dbo].[Resource_BuildElasticSearch]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[Resource_BuildElasticSearch] '449897'

*/
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/4/2013
-- Description:	Build records for a single resource for ElasticSearch
-- TODO: Add code for IsDeleted and Evaluations
--
-- 2013-06-20 jgrimmer - Fixed bug with null grade level aliases
-- 2013-07-26 jgrimmer - Added code to process a list of ResourceIntIds
-- 2013-08-20 jgrimmer - Added LikeCount and DislikeCount
-- 2013-11-27 jgrimmer - Added check for IsDeleted in the Link Checker database
-- 2014-02-03 jgrimmer - Modify favorites to rely on [Resource].FavoriteCount only.  It is detail/library page's responsibility to update that count.
--						 Also add Submitter.
-- 2014-02-25 jgrimmer - Modified to include only resource versions which are active.
-- 2014-05-19 jgrimmer - Exclude resources from Smarter Balance, since Illinois is a PARCC member and not a Smarter Balance member state.

-- 2014-05-22 mparsons - added back missing accessibility steps
-- 2014-05-28 mparsons - added sortTitle
-- =============================================
CREATE PROCEDURE [dbo].[Resource_BuildElasticSearch]
	--@ResourceIntId int,
	@ResourceIntId varchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	--Create and populate table of ResourceIntIds to process
	CREATE TABLE #resourceIntIds (intID int)
	/*IF @ResourceIntId <> 0 BEGIN
		INSERT INTO #resourceIntIds (intID)
		VALUES(@ResourceIntId)
	END ELSE BEGIN*/
		DECLARE @pos int,@value varchar(12),@remainder varchar(max)
		SET @remainder = @ResourceIntId
		SET @pos = PATINDEX('%,%',@remainder)
		WHILE(@pos > 0) BEGIN
			SET @value = left(@remainder,@pos - 1)
			INSERT INTO #resourceIntIds (intID)
			VALUES(@value)
			SET @remainder = right(@remainder,len(@remainder) - @pos)
			SET @pos = PATINDEX('%,%',@remainder)
		END
		INSERT INTO #resourceIntIds (intID)
		VALUES(@remainder)
	--END
	
	--Create workTable
	CREATE TABLE #workTable (
		versionID int,
		intID int,
		title nvarchar(300),
		[description] nvarchar(max),
		publisher varchar(200),
		created datetime,
		accessRights varchar(100),
		accessRightsID int,
		keywords nvarchar(max),
		subjects nvarchar(max),
		languageIDs nvarchar(max),
		languages nvarchar(max),
		clusterIDs nvarchar(max),
		clusters nvarchar(max),
		audienceIDs nvarchar(max),
		audiences nvarchar(max),
		educationLevelIDs nvarchar(max),
		educationLevels nvarchar(max),
		educationLevelAliases nvarchar(max),
		resourceTypeIDs nvarchar(max),
		resourceTypes nvarchar(max),
		mediaTypeIDs nvarchar(max),
		mediaTypes nvarchar(max),
		groupTypeIDs nvarchar(max),
		groupTypes nvarchar(max),
		itemTypeIDs nvarchar(max),
		itemTypes nvarchar(max),
		standardIDs nvarchar(max),
		standardNotations nvarchar(max),
		LastUpdated datetime,
		Url varchar(500),
		libraryIDs varchar(max),
		collectionIDs varchar(max),
		IsDeleted bit,
		educationalUseIDs nvarchar(max),
		educationalUses nvarchar(max),
		usageRights varchar(300),
		usageRightsID int,
		usageRightsURL varchar(700),
		usageRightsIconURL varchar(300),
		usageRightsMiniIconUrl varchar(300),
		likesSummary int,
		evaluationCount int,
		evaluationScore int,
		commentsCount int,
		viewsCount int,
		detailViews int,
		favorites int,
		likeCount int,
		dislikeCount int,
		submitter varchar(100),
		accessibilityApiIDs nvarchar(max),
		accessibilityApis nvarchar(max),
		accessibilityControlIDs nvarchar(max),
		accessibilityControls nvarchar(max),
		accessibilityFeatureIDs nvarchar(max),
		accessibilityFeatures nvarchar(max),
		accessibilityHazardIDs nvarchar(max),
		accessibilityHazards nvarchar(max),
		sortTitle nvarchar(300))

	-- Get Resource/ResourceVersion information
	INSERT INTO #workTable (versionId, intId, title, [description], publisher,
		created, accessRights, accessRightsId, LastUpdated, url, usageRightsUrl, usageRights, usageRightsID, usageRightsIconURL, usageRightsMiniIconURL, viewsCount, 
		favorites, submitter, sortTitle)
	SELECT rv.Id, rv.ResourceIntId, rv.Title, rv.[Description], rv.Publisher, rv.Created, rv.AccessRights, rv.AccessRightsId, GETDATE(), r.ResourceUrl,
	rv.Rights, 
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.Summary
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'Read the Fine Print'
		ELSE NULL
	END AS usageRights,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.Id
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 4 -- Read the Fine Print
		ELSE NULL
	END AS usageRightsID,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.IconUrl
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'http://mirrors.creativecommons.org/presskit/cc.primary.srr.gif'
		ELSE NULL
	END usageRightsIconURL,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.MiniIconUrl
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN '/images/icons/rightsreserved.png'
		ELSE NULL
	END AS usageRightsMiniIconUrl,
	ViewCount, r.FavoriteCount, rv.Submitter,
	[dbo].[BuildSortTitle] (rv.[Title])
	FROM #resourceIntIds ridl
	INNER JOIN [Resource.Version] rv ON ridl.intID = rv.ResourceIntId
	INNER JOIN [Resource] r ON rv.ResourceIntId = r.Id
	LEFT JOIN [ConditionOfUse] cou ON rv.Rights = cou.Url
	WHERE ResourceIntId NOT IN (SELECT ResourceIntId FROM [Resource.Link] WHERE IsDeleted = 'True') AND 
		(cou.Id IS NULL OR cou.Id <> 3) AND r.IsActive = 'True' and rv.IsActive = 'True' AND Submitter NOT LIKE '%@smarterbalanced.org%'
	
	-- Keywords
	PRINT 'Keywords'
	UPDATE #workTable
	SET keywords = rs.Keywords,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Keywords, LEN(Keywords) - 1) AS Keywords
		FROM (SELECT ResourceIntId, (
			SELECT itbl.[Keyword]+','
			FROM [Resource.Keyword] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Keywords
		FROM [Resource.Keyword] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Subjects
	PRINT 'Subjects'
	UPDATE #workTable
	SET [subjects] = rs.[Subjects],
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Subjects, LEN(Subjects) - 1) AS Subjects
		FROM (SELECT ResourceIntId, (
			SELECT itbl.[Subject]+','
			FROM [Resource.Subject] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Subjects
		FROM [Resource.Subject] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID	
	--LanguageIDs
	PRINT 'Language IDs'
	UPDATE #workTable
	SET [languageIDs] = rs.[LanguageIds],
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(LanguageIDs, LEN(LanguageIDs) - 1) AS LanguageIDs
			FROM (SELECT ResourceIntId, (
				SELECT convert(nvarchar,itbl.[LanguageId])+','
				FROM [Resource.Language] itbl
				WHERE itbl.ResourceIntId = tbl.ResourceIntId
				for xml path('')) LanguageIDs
			FROM [Resource.Language] tbl
			GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
			
	-- Languages
	PRINT 'Languages'
	UPDATE #workTable
	SET [languages] = rs.[Languages],
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Languages, LEN(Languages) - 1) AS Languages
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.Language].Title+','
			FROM [Resource.Language] itbl
			INNER JOIN [Codes.Language] ON itbl.LanguageId = [Codes.Language].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Languages
		FROM [Resource.Language] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Career Cluster IDs
	PRINT 'Cluster IDs'
	UPDATE #workTable
	SET clusterIDs = rs.ClusterIds,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(ClusterIDs, LEN(ClusterIDs) - 1) AS ClusterIDs
			FROM (SELECT ResourceIntId, (
				SELECT convert(nvarchar,itbl.[ClusterId])+','
				FROM [Resource.Cluster] itbl
				WHERE itbl.ResourceIntId = tbl.ResourceIntId
				for xml path('')) ClusterIDs
			FROM [Resource.Cluster] tbl
			GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
	
	-- Clusters
	PRINT 'Clusters'
	UPDATE #workTable
	SET clusters = rs.Clusters,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Clusters, LEN(Clusters) - 1) AS Clusters
		FROM (SELECT ResourceIntId, (
			SELECT [CareerCluster].IlPathwayName+','
			FROM [Resource.Cluster] itbl
			INNER JOIN [CareerCluster] ON itbl.ClusterId = [CareerCluster].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Clusters
		FROM [Resource.Cluster] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Intended Audience Ids
	PRINT 'Intended Audience IDs'
	UPDATE #workTable
	SET audienceIDs = rs.AudienceIds,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(AudienceIDs, LEN(AudienceIDs) - 1) AS AudienceIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[AudienceId])+','
			FROM [Resource.IntendedAudience] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) AudienceIDs
		FROM [Resource.IntendedAudience] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Intended Audiences	
	PRINT 'Intended Audiences'
	UPDATE #workTable
	SET audiences = rs.Audiences,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Audiences, LEN(Audiences) - 1) AS Audiences
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.AudienceType].Title+','
			FROM [Resource.IntendedAudience] itbl
			INNER JOIN [Codes.AudienceType] ON itbl.AudienceId = [Codes.AudienceType].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Audiences
		FROM [Resource.IntendedAudience] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- EducationLevelIDs
	PRINT 'Grade Level IDs'
	UPDATE #workTable
	SET educationLevelIDs = rs.EducationLevelIds,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(EducationLevelIDs, LEN(EducationLevelIDs) - 1) AS EducationLevelIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[GradeLevelId])+','
			FROM [Resource.GradeLevel] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) EducationLevelIDs
		FROM [Resource.GradeLevel] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Education Levels
	PRINT 'Grade Levels'
	UPDATE #workTable
	SET educationLevels = rs.EducationLevels,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(EducationLevels, LEN(EducationLevels) - 1) AS EducationLevels
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.GradeLevel].Title+','
			FROM [Resource.GradeLevel] itbl
			INNER JOIN [Codes.GradeLevel] ON itbl.GradeLevelId = [Codes.GradeLevel].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) EducationLevels
		FROM [Resource.GradeLevel] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Education Level Aliases
	PRINT 'Grade Level Aliases'
	UPDATE #workTable
	SET educationLevelAliases = rs.EducationLevelAliases,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, replace(left(EducationLevelAliases, LEN(EducationLevelAliases) - 1), ',,', ',') AS EducationLevelAliases
		FROM (SELECT ResourceIntId, (
			SELECT isnull([Codes.GradeLevel].AliasValues,'')+','
			FROM [Resource.GradeLevel] itbl
			INNER JOIN [Codes.GradeLevel] ON itbl.GradeLevelId = [Codes.GradeLevel].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) EducationLevelAliases
		FROM [Resource.GradeLevel] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Resource Type IDs
	PRINT 'Resource Type IDs'
	UPDATE #workTable
	SET resourceTypeIDs = rs.ResourceTypeIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(ResourceTypeIDs, LEN(ResourceTypeIDs) - 1) AS ResourceTypeIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[ResourceTypeId])+','
			FROM [Resource.ResourceType] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) ResourceTypeIDs
		FROM [Resource.ResourceType] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID


	-- Resource Types
	PRINT 'Resource Types'
	UPDATE #workTable
	SET resourceTypes = rs.ResourceTypes,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(ResourceTypes, LEN(ResourceTypes) - 1) AS ResourceTypes
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.ResourceType].Title+','
			FROM [Resource.ResourceType] itbl
			INNER JOIN [Codes.ResourceType] ON itbl.ResourceTypeId = [Codes.ResourceType].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) ResourceTypes
		FROM [Resource.ResourceType] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID


	-- Resource Format IDs
	PRINT 'Resource Format IDs'
	UPDATE #workTable
	SET mediaTypeIDs = rs.MediaTypeIds,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(MediaTypeIDs, LEN(MediaTypeIDs) - 1) AS MediaTypeIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[CodeId])+','
			FROM [Resource.Format] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) MediaTypeIDs
		FROM [Resource.Format] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Resource Formats
	PRINT 'Resource Formats'
	UPDATE #workTable
	SET mediaTypes = rs.MediaTypes,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(MediaTypes, LEN(MediaTypes) - 1) AS MediaTypes
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.ResourceFormat].Title+','
			FROM [Resource.Format] itbl
			INNER JOIN [Codes.ResourceFormat] ON itbl.CodeId = [Codes.ResourceFormat].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) MediaTypes
		FROM [Resource.Format] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Group Type IDs
	PRINT 'Group Type IDs'
	UPDATE #workTable
	SET groupTypeIDs = rs.GroupTypeIds,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(GroupTypeIDs, LEN(GroupTypeIDs) - 1) AS GroupTypeIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[GroupTypeId])+','
			FROM [Resource.GroupType] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) GroupTypeIDs
		FROM [Resource.Format] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Group Types
	PRINT 'Group Types'
	UPDATE #workTable
	SET groupTypes = rs.GroupTypes,
		LastUpdated=GETDATE()
	FROM (SELECT ResourceIntId, left(GroupTypes, LEN(GroupTypes) - 1) AS GroupTypes
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.GroupType].Title+','
			FROM [Resource.GroupType] itbl
			INNER JOIN [Codes.GroupType] ON itbl.GroupTypeId = [Codes.GroupType].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) GroupTypes
		FROM [Resource.Format] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
				
	-- Item Type IDs
	PRINT 'Item Type IDs'	
	UPDATE #workTable
	SET itemTypeIDs = rs.ItemTypeIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(ItemTypeIDs, LEN(ItemTypeIDs) - 1) AS ItemTypeIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[ItemTypeId])+','
			FROM [Resource.ItemType] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) ItemTypeIDs
		FROM [Resource.ItemType] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Item Types
	PRINT 'Item Types'
	UPDATE #workTable
	SET itemTypes = rs.ItemTypes,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(ItemTypes, LEN(ItemTypes) - 1) AS ItemTypes
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.ItemType].Title+','
			FROM [Resource.ItemType] itbl
			INNER JOIN [Codes.ItemType] ON itbl.ItemTypeId = [Codes.ItemType].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) ItemTypes
		FROM [Resource.Format] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Standard IDs
	PRINT 'Standard IDs'
	UPDATE #workTable
	SET standardIDs = rs.StandardIDs
	FROM (SELECT ResourceIntId, left(StandardIDs, LEN(StandardIDs) - 1) AS StandardIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[StandardId])+','
			FROM [Resource.Standard] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) StandardIDs
		FROM [Resource.Standard] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Standard Notations
	PRINT 'Standards'
	UPDATE #workTable
	SET standardNotations = rs.Standards,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Standards, LEN(Standards) - 1) AS Standards
		FROM (SELECT ResourceIntId, (
			SELECT [StandardBody.Node].NotationCode+','
			FROM [Resource.Standard] itbl
			INNER JOIN [StandardBody.Node] ON itbl.StandardId = [StandardBody.Node].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Standards
		FROM [Resource.Standard] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Educational Use IDs
	PRINT 'Educational Use IDs'
	UPDATE #workTable
	SET educationalUseIDs = rs.EducationUseIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(EducationUseIDs, LEN(EducationUseIDs) - 1) AS EducationUseIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[EducationUseId])+','
			FROM [Resource.EducationUse] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) EducationUseIDs
		FROM [Resource.EducationUse] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Educational Uses
	PRINT 'Educational Uses'
	UPDATE #workTable
	SET educationalUses = rs.EducationUses,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(EducationUses, LEN(EducationUses) - 1) AS EducationUses
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.EducationalUse].Title+','
			FROM [Resource.EducationUse] itbl
			INNER JOIN [Codes.EducationalUse] ON itbl.EducationUseId = [Codes.EducationalUse].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) EducationUses
		FROM [Resource.EducationUse] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Likes minus dislikes
	PRINT 'Likes - Dislikes'
	UPDATE #workTable
	SET likesSummary = rls.LikeCount - rls.DislikeCount,
		LastUpdated = GETDATE()
	FROM [Resource.LikesSummary] rls
	WHERE rls.ResourceIntId = #workTable.intID
	
	-- LikeCount, DislikeCount
	PRINT 'LikeCount, DislikeCount'
	UPDATE #workTable
	SET likeCount = rls.LikeCount,
		dislikeCount = rls.DislikeCount
	FROM [Resource.LikesSummary] rls
	WHERE rls.ResourceIntId = #workTable.intID

	-- LibraryIDs
	PRINT 'Library IDs'
	UPDATE #workTable
	SET libraryIDs = rs.LibraryIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(LibraryIDs, LEN(LibraryIDs) - 1) AS LibraryIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,[IsleContent].[dbo].[Library.Section].LibraryId)+','
			FROM [IsleContent].[dbo].[Library.Resource] itbl
			INNER JOIN [IsleContent].[dbo].[Library.Section] 
				ON itbl.LibrarySectionId = [IsleContent].[dbo].[Library.Section].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) LibraryIDs
		FROM [IsleContent].[dbo].[Library.Resource] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Collection IDs
	PRINT 'Collection IDs'
	UPDATE #workTable
	SET collectionIDs = rs.CollectionIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(CollectionIDs, LEN(CollectionIDs) - 1) AS CollectionIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[LibrarySectionId])+','
			FROM [IsleContent].[dbo].[Library.Resource] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) CollectionIDs
		FROM [IsleContent].[dbo].[Library.Resource] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
	
	-- Comment Count
	PRINT 'Comment Count'
	UPDATE #workTable
	SET commentsCount = rs.CommentsCount
	FROM (SELECT ResourceIntId, COUNT(*) AS CommentsCount
		FROM [Resource.Comment]
		GROUP BY ResourceIntId) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Detail Views
	PRINT 'Detail Views'
	UPDATE #workTable
	SET detailViews = rs.DetailViews,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, COUNT(*) AS DetailViews
		FROM [Resource.DetailView]
		GROUP BY ResourceIntId) rs
	WHERE #workTable.intID = rs.ResourceIntId AND
		(#workTable.detailViews <> rs.DetailViews OR
		(#workTable.detailViews IS NULL AND rs.DetailViews IS NOT NULL))

	-- Favorites - commented out.  Relying only on [Resource].FavoriteCount
/*	PRINT 'Favorites'
	UPDATE #workTable
	SET favorites = rs.Favorites,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, r.FavoriteCount + COUNT(*) AS Favorites
		FROM [IsleContent].[dbo].[Library.Resource] lr
		INNER JOIN [Resource] r ON lr.ResourceIntId = r.Id
		GROUP BY ResourceIntId, FavoriteCount) rs
	WHERE #workTable.intID = rs.ResourceIntId AND
		(#workTable.detailViews <> rs.Favorites OR
		(#workTable.detailViews IS NULL AND rs.Favorites IS NOT NULL)) */


	-- accessibilityApi IDs
	PRINT 'AccessibilityApi IDs'
	UPDATE #workTable
	SET accessibilityApiIDs = rs.accessibilityApiIDs
	FROM (SELECT ResourceIntId, left(accessibilityApiIDs, LEN(accessibilityApiIDs) - 1) AS accessibilityApiIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[AccessibilityApiId])+','
			FROM [Resource.AccessibilityApi] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) accessibilityApiIDs
		FROM [Resource.AccessibilityApi] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- accessibilityApis
	PRINT 'AccessibilityApis'
	UPDATE #workTable
	SET accessibilityApis = rs.accessibilityApi,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(accessibilityApi, LEN(accessibilityApi) - 1) AS accessibilityApi
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.AccessibilityApi].Title+','
			FROM [Resource.AccessibilityApi] itbl
			INNER JOIN [Codes.AccessibilityApi] ON itbl.AccessibilityApiId = [Codes.AccessibilityApi].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) accessibilityApi
		FROM [Resource.AccessibilityApi] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- accessibilityControl IDs
	PRINT 'AccessibilityControl IDs'
	UPDATE #workTable
	SET accessibilityControlIDs = rs.accessibilityControlIDs
	FROM (SELECT ResourceIntId, left(accessibilityControlIDs, LEN(accessibilityControlIDs) - 1) AS accessibilityControlIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[AccessibilityControlId])+','
			FROM [Resource.AccessibilityControl] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) accessibilityControlIDs
		FROM [Resource.AccessibilityControl] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- accessibilityControls
	PRINT 'AccessibilityControls'
	UPDATE #workTable
	SET accessibilityControls = rs.accessibilityControl,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(accessibilityControl, LEN(accessibilityControl) - 1) AS accessibilityControl
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.AccessibilityControl].Title+','
			FROM [Resource.AccessibilityControl] itbl
			INNER JOIN [Codes.AccessibilityControl] ON itbl.AccessibilityControlId = [Codes.AccessibilityControl].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) accessibilityControl
		FROM [Resource.AccessibilityControl] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- accessibilityFeature IDs
	PRINT 'AccessibilityFeature IDs'
	UPDATE #workTable
	SET accessibilityFeatureIDs = rs.accessibilityFeatureIDs
	FROM (SELECT ResourceIntId, left(accessibilityFeatureIDs, LEN(accessibilityFeatureIDs) - 1) AS accessibilityFeatureIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[AccessibilityFeatureId])+','
			FROM [Resource.AccessibilityFeature] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) accessibilityFeatureIDs
		FROM [Resource.AccessibilityFeature] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- accessibilityFeatures
	PRINT 'AccessibilityFeatures'
	UPDATE #workTable
	SET accessibilityFeatures = rs.accessibilityFeature,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(accessibilityFeature, LEN(accessibilityFeature) - 1) AS accessibilityFeature
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.AccessibilityFeature].Title+','
			FROM [Resource.AccessibilityFeature] itbl
			INNER JOIN [Codes.AccessibilityFeature] ON itbl.AccessibilityFeatureId = [Codes.AccessibilityFeature].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) accessibilityFeature
		FROM [Resource.AccessibilityFeature] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- accessibilityHazard IDs
	PRINT 'AccessibilityHazard IDs'
	UPDATE #workTable
	SET accessibilityHazardIDs = rs.accessibilityHazardIDs
	FROM (SELECT ResourceIntId, left(accessibilityHazardIDs, LEN(accessibilityHazardIDs) - 1) AS accessibilityHazardIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[AccessibilityHazardId])+','
			FROM [Resource.AccessibilityHazard] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) accessibilityHazardIDs
		FROM [Resource.AccessibilityHazard] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- accessibilityHazards
	PRINT 'AccessibilityHazards'
	UPDATE #workTable
	SET accessibilityHazards = rs.accessibilityHazard,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(accessibilityHazard, LEN(accessibilityHazard) - 1) AS accessibilityHazard
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.AccessibilityHazard].Title+','
			FROM [Resource.AccessibilityHazard] itbl
			INNER JOIN [Codes.AccessibilityHazard] ON itbl.AccessibilityHazardId = [Codes.AccessibilityHazard].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) accessibilityHazard
		FROM [Resource.AccessibilityHazard] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Output the data
	SELECT
		versionID,
		intID,
		title,
		[description],
		publisher,
		created,
		accessRights,
		accessRightsID,
		keywords,
		subjects,
		languageIDs,
		languages,
		clusterIDs,
		clusters,
		audienceIDs,
		audiences,
		educationLevelIDs,
		educationLevels,
		educationLevelAliases,
		resourceTypeIDs,
		resourceTypes,
		mediaTypeIDs,
		mediaTypes,
		groupTypeIDs,
		groupTypes,
		itemTypeIDs,
		itemTypes,
		standardIDs,
		standardNotations,
		LastUpdated,
		Url,
		libraryIDs,
		collectionIDs,
		IsDeleted,
		educationalUseIDs,
		educationalUses,
		usageRights,
		usageRightsID,
		usageRightsURL,
		usageRightsIconURL,
		usageRightsMiniIconUrl,
		likesSummary,
		evaluationCount,
		evaluationScore,
		commentsCount,
		viewsCount,
		detailViews,
		favorites,
		likeCount,
		dislikeCount,
		submitter,
		accessibilityApiIDs,
		accessibilityApis,
		accessibilityControlIDs,
		accessibilityControls,
		accessibilityFeatureIDs,
		accessibilityFeatures,
		accessibilityHazardIDs,
		accessibilityHazards,
		sortTitle
	FROM #workTable
	
	DROP TABLE #workTable

END

GO
/****** Object:  StoredProcedure [dbo].[Resource_BuildElasticSearch2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/4/2013
-- Description:	Build records for a single resource for ElasticSearch
-- TODO: Add code for IsDeleted and Evaluations
--
-- 2013-06-20 jgrimmer - Fixed bug with null grade level aliases
-- 2013-07-26 jgrimmer - Added code to process a list of ResourceIntIds
-- 2013-08-20 jgrimmer - Added LikeCount and DislikeCount
-- =============================================
CREATE PROCEDURE [dbo].[Resource_BuildElasticSearch2]
	--@ResourceIntId int,
	@ResourceIntId varchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	--Create and populate table of ResourceIntIds to process
	CREATE TABLE #resourceIntIds (intID int)
	/*IF @ResourceIntId <> 0 BEGIN
		INSERT INTO #resourceIntIds (intID)
		VALUES(@ResourceIntId)
	END ELSE BEGIN*/
		DECLARE @pos int,@value varchar(12),@remainder varchar(max)
		SET @remainder = @ResourceIntId
		SET @pos = PATINDEX('%,%',@remainder)
		WHILE(@pos > 0) BEGIN
			SET @value = left(@remainder,@pos - 1)
			INSERT INTO #resourceIntIds (intID)
			VALUES(@value)
			SET @remainder = right(@remainder,len(@remainder) - @pos)
			SET @pos = PATINDEX('%,%',@remainder)
		END
		INSERT INTO #resourceIntIds (intID)
		VALUES(@remainder)
	--END
	
	--Create workTable
	CREATE TABLE #workTable (
		versionID int,
		intID int,
		title nvarchar(300),
		[description] nvarchar(max),
		publisher varchar(200),
		created datetime,
		accessRights varchar(100),
		accessRightsID int,
		keywords nvarchar(max),
		subjects nvarchar(max),
		languageIDs nvarchar(max),
		languages nvarchar(max),
		clusterIDs nvarchar(max),
		clusters nvarchar(max),
		audienceIDs nvarchar(max),
		audiences nvarchar(max),
		educationLevelIDs nvarchar(max),
		educationLevels nvarchar(max),
		educationLevelAliases nvarchar(max),
		resourceTypeIDs nvarchar(max),
		resourceTypes nvarchar(max),
		mediaTypeIDs nvarchar(max),
		mediaTypes nvarchar(max),
		groupTypeIDs nvarchar(max),
		groupTypes nvarchar(max),
		itemTypeIDs nvarchar(max),
		itemTypes nvarchar(max),
		standardIDs nvarchar(max),
		standardNotations nvarchar(max),
		LastUpdated datetime,
		Url varchar(500),
		libraryIDs varchar(max),
		collectionIDs varchar(max),
		IsDeleted bit,
		educationalUseIDs nvarchar(max),
		educationalUses nvarchar(max),
		usageRights varchar(300),
		usageRightsID int,
		usageRightsURL varchar(700),
		usageRightsIconURL varchar(300),
		usageRightsMiniIconUrl varchar(300),
		likesSummary int,
		evaluationCount int,
		evaluationScore int,
		commentsCount int,
		viewsCount int,
		detailViews int,
		favorites int,
		likeCount int,
		dislikeCount int)

	-- Get Resource/ResourceVersion information
	INSERT INTO #workTable (versionId, intId, title, [description], publisher,
		created, accessRights, accessRightsId, LastUpdated, url, usageRightsUrl, usageRights, usageRightsID, usageRightsIconURL, usageRightsMiniIconURL, viewsCount)
	SELECT rv.Id, rv.ResourceIntId, rv.Title, rv.[Description], rv.Publisher, rv.Created, rv.AccessRights, rv.AccessRightsId, GETDATE(), r.ResourceUrl,
	rv.Rights, 
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.Summary
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'Read the Fine Print'
		ELSE NULL
	END AS usageRights,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.Id
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 4 -- Read the Fine Print
		ELSE NULL
	END AS usageRightsID,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.IconUrl
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'http://mirrors.creativecommons.org/presskit/cc.primary.srr.gif'
		ELSE NULL
	END usageRightsIconURL,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.MiniIconUrl
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN '/images/icons/rightsreserved.png'
		ELSE NULL
	END AS usageRightsMiniIconUrl,
	ViewCount
	FROM #resourceIntIds ridl
	INNER JOIN [Resource.Version] rv ON ridl.intID = rv.ResourceIntId
	INNER JOIN [Resource] r ON rv.ResourceIntId = r.Id
	LEFT JOIN [ConditionOfUse] cou ON rv.Rights = cou.Url
	WHERE /* rv.ResourceIntId = @ResourceIntId AND */ (cou.Id IS NULL OR cou.Id <> 3) AND r.IsActive = 'True'
	
	-- Keywords
	PRINT 'Keywords'
	UPDATE #workTable
	SET keywords = rs.Keywords,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Keywords, LEN(Keywords) - 1) AS Keywords
		FROM (SELECT ResourceIntId, (
			SELECT itbl.[Keyword]+','
			FROM [Resource.Keyword] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Keywords
		FROM [Resource.Keyword] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Subjects
	PRINT 'Subjects'
	UPDATE #workTable
	SET [subjects] = rs.[Subjects],
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Subjects, LEN(Subjects) - 1) AS Subjects
		FROM (SELECT ResourceIntId, (
			SELECT itbl.[Subject]+','
			FROM [Resource.Subject] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Subjects
		FROM [Resource.Subject] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID	
	--LanguageIDs
	PRINT 'Language IDs'
	UPDATE #workTable
	SET [languageIDs] = rs.[LanguageIds],
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(LanguageIDs, LEN(LanguageIDs) - 1) AS LanguageIDs
			FROM (SELECT ResourceIntId, (
				SELECT convert(nvarchar,itbl.[LanguageId])+','
				FROM [Resource.Language] itbl
				WHERE itbl.ResourceIntId = tbl.ResourceIntId
				for xml path('')) LanguageIDs
			FROM [Resource.Language] tbl
			GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
			
	-- Languages
	PRINT 'Languages'
	UPDATE #workTable
	SET [languages] = rs.[Languages],
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Languages, LEN(Languages) - 1) AS Languages
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.Language].Title+','
			FROM [Resource.Language] itbl
			INNER JOIN [Codes.Language] ON itbl.LanguageId = [Codes.Language].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Languages
		FROM [Resource.Language] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Career Cluster IDs
	PRINT 'Cluster IDs'
	UPDATE #workTable
	SET clusterIDs = rs.ClusterIds,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(ClusterIDs, LEN(ClusterIDs) - 1) AS ClusterIDs
			FROM (SELECT ResourceIntId, (
				SELECT convert(nvarchar,itbl.[ClusterId])+','
				FROM [Resource.Cluster] itbl
				WHERE itbl.ResourceIntId = tbl.ResourceIntId
				for xml path('')) ClusterIDs
			FROM [Resource.Cluster] tbl
			GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
	
	-- Clusters
	PRINT 'Clusters'
	UPDATE #workTable
	SET clusters = rs.Clusters,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Clusters, LEN(Clusters) - 1) AS Clusters
		FROM (SELECT ResourceIntId, (
			SELECT [CareerCluster].IlPathwayName+','
			FROM [Resource.Cluster] itbl
			INNER JOIN [CareerCluster] ON itbl.ClusterId = [CareerCluster].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Clusters
		FROM [Resource.Cluster] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Intended Audience Ids
	PRINT 'Intended Audience IDs'
	UPDATE #workTable
	SET audienceIDs = rs.AudienceIds,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(AudienceIDs, LEN(AudienceIDs) - 1) AS AudienceIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[AudienceId])+','
			FROM [Resource.IntendedAudience] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) AudienceIDs
		FROM [Resource.IntendedAudience] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Intended Audiences	
	PRINT 'Intended Audiences'
	UPDATE #workTable
	SET audiences = rs.Audiences,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Audiences, LEN(Audiences) - 1) AS Audiences
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.AudienceType].Title+','
			FROM [Resource.IntendedAudience] itbl
			INNER JOIN [Codes.AudienceType] ON itbl.AudienceId = [Codes.AudienceType].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Audiences
		FROM [Resource.IntendedAudience] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- EducationLevelIDs
	PRINT 'Grade Level IDs'
	UPDATE #workTable
	SET educationLevelIDs = rs.EducationLevelIds,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(EducationLevelIDs, LEN(EducationLevelIDs) - 1) AS EducationLevelIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[GradeLevelId])+','
			FROM [Resource.GradeLevel] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) EducationLevelIDs
		FROM [Resource.GradeLevel] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Education Levels
	PRINT 'Grade Levels'
	UPDATE #workTable
	SET educationLevels = rs.EducationLevels,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(EducationLevels, LEN(EducationLevels) - 1) AS EducationLevels
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.GradeLevel].Title+','
			FROM [Resource.GradeLevel] itbl
			INNER JOIN [Codes.GradeLevel] ON itbl.GradeLevelId = [Codes.GradeLevel].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) EducationLevels
		FROM [Resource.GradeLevel] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Education Level Aliases
	PRINT 'Grade Level Aliases'
	UPDATE #workTable
	SET educationLevelAliases = rs.EducationLevelAliases,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, replace(left(EducationLevelAliases, LEN(EducationLevelAliases) - 1), ',,', ',') AS EducationLevelAliases
		FROM (SELECT ResourceIntId, (
			SELECT isnull([Codes.GradeLevel].AliasValues,'')+','
			FROM [Resource.GradeLevel] itbl
			INNER JOIN [Codes.GradeLevel] ON itbl.GradeLevelId = [Codes.GradeLevel].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) EducationLevelAliases
		FROM [Resource.GradeLevel] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Resource Type IDs
	PRINT 'Resource Type IDs'
	UPDATE #workTable
	SET resourceTypeIDs = rs.ResourceTypeIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(ResourceTypeIDs, LEN(ResourceTypeIDs) - 1) AS ResourceTypeIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[ResourceTypeId])+','
			FROM [Resource.ResourceType] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) ResourceTypeIDs
		FROM [Resource.ResourceType] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID


	-- Resource Types
	PRINT 'Resource Types'
	UPDATE #workTable
	SET resourceTypes = rs.ResourceTypes,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(ResourceTypes, LEN(ResourceTypes) - 1) AS ResourceTypes
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.ResourceType].Title+','
			FROM [Resource.ResourceType] itbl
			INNER JOIN [Codes.ResourceType] ON itbl.ResourceTypeId = [Codes.ResourceType].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) ResourceTypes
		FROM [Resource.ResourceType] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID


	-- Resource Format IDs
	PRINT 'Resource Format IDs'
	UPDATE #workTable
	SET mediaTypeIDs = rs.MediaTypeIds,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(MediaTypeIDs, LEN(MediaTypeIDs) - 1) AS MediaTypeIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[CodeId])+','
			FROM [Resource.Format] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) MediaTypeIDs
		FROM [Resource.Format] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Resource Formats
	PRINT 'Resource Formats'
	UPDATE #workTable
	SET mediaTypes = rs.MediaTypes,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(MediaTypes, LEN(MediaTypes) - 1) AS MediaTypes
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.ResourceFormat].Title+','
			FROM [Resource.Format] itbl
			INNER JOIN [Codes.ResourceFormat] ON itbl.CodeId = [Codes.ResourceFormat].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) MediaTypes
		FROM [Resource.Format] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Group Type IDs
	PRINT 'Group Type IDs'
	UPDATE #workTable
	SET groupTypeIDs = rs.GroupTypeIds,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(GroupTypeIDs, LEN(GroupTypeIDs) - 1) AS GroupTypeIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[GroupTypeId])+','
			FROM [Resource.GroupType] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) GroupTypeIDs
		FROM [Resource.Format] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Group Types
	PRINT 'Group Types'
	UPDATE #workTable
	SET groupTypes = rs.GroupTypes,
		LastUpdated=GETDATE()
	FROM (SELECT ResourceIntId, left(GroupTypes, LEN(GroupTypes) - 1) AS GroupTypes
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.GroupType].Title+','
			FROM [Resource.GroupType] itbl
			INNER JOIN [Codes.GroupType] ON itbl.GroupTypeId = [Codes.GroupType].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) GroupTypes
		FROM [Resource.Format] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
				
	-- Item Type IDs
	PRINT 'Item Type IDs'	
	UPDATE #workTable
	SET itemTypeIDs = rs.ItemTypeIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(ItemTypeIDs, LEN(ItemTypeIDs) - 1) AS ItemTypeIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[ItemTypeId])+','
			FROM [Resource.ItemType] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) ItemTypeIDs
		FROM [Resource.ItemType] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Item Types
	PRINT 'Item Types'
	UPDATE #workTable
	SET itemTypes = rs.ItemTypes,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(ItemTypes, LEN(ItemTypes) - 1) AS ItemTypes
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.ItemType].Title+','
			FROM [Resource.ItemType] itbl
			INNER JOIN [Codes.ItemType] ON itbl.ItemTypeId = [Codes.ItemType].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) ItemTypes
		FROM [Resource.Format] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Standard IDs
	PRINT 'Standard IDs'
	UPDATE #workTable
	SET standardIDs = rs.StandardIDs
	FROM (SELECT ResourceIntId, left(StandardIDs, LEN(StandardIDs) - 1) AS StandardIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[StandardId])+','
			FROM [Resource.Standard] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) StandardIDs
		FROM [Resource.Standard] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Standard Notations
	PRINT 'Standards'
	UPDATE #workTable
	SET standardNotations = rs.Standards,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Standards, LEN(Standards) - 1) AS Standards
		FROM (SELECT ResourceIntId, (
			SELECT [StandardBody.Node].NotationCode+','
			FROM [Resource.Standard] itbl
			INNER JOIN [StandardBody.Node] ON itbl.StandardId = [StandardBody.Node].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Standards
		FROM [Resource.Standard] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Educational Use IDs
	PRINT 'Educational Use IDs'
	UPDATE #workTable
	SET educationalUseIDs = rs.EducationUseIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(EducationUseIDs, LEN(EducationUseIDs) - 1) AS EducationUseIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[EducationUseId])+','
			FROM [Resource.EducationUse] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) EducationUseIDs
		FROM [Resource.EducationUse] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Educational Uses
	PRINT 'Educational Uses'
	UPDATE #workTable
	SET educationalUses = rs.EducationUses,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(EducationUses, LEN(EducationUses) - 1) AS EducationUses
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.EducationalUse].Title+','
			FROM [Resource.EducationUse] itbl
			INNER JOIN [Codes.EducationalUse] ON itbl.EducationUseId = [Codes.EducationalUse].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) EducationUses
		FROM [Resource.EducationUse] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Likes minus dislikes
	PRINT 'Likes - Dislikes'
	UPDATE #workTable
	SET likesSummary = rls.LikeCount - rls.DislikeCount,
		LastUpdated = GETDATE()
	FROM [Resource.LikesSummary] rls
	WHERE rls.ResourceIntId = #workTable.intID
	
	-- LikeCount, DislikeCount
	PRINT 'LikeCount, DislikeCount'
	UPDATE #workTable
	SET likeCount = rls.LikeCount,
		dislikeCount = rls.DislikeCount
	FROM [Resource.LikesSummary] rls
	WHERE rls.ResourceIntId = #workTable.intID

	-- LibraryIDs
	PRINT 'Library IDs'
	UPDATE #workTable
	SET libraryIDs = rs.LibraryIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(LibraryIDs, LEN(LibraryIDs) - 1) AS LibraryIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,[IsleContent].[dbo].[Library.Section].LibraryId)+','
			FROM [IsleContent].[dbo].[Library.Resource] itbl
			INNER JOIN [IsleContent].[dbo].[Library.Section] 
				ON itbl.LibrarySectionId = [IsleContent].[dbo].[Library.Section].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) LibraryIDs
		FROM [IsleContent].[dbo].[Library.Resource] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Collection IDs
	PRINT 'Collection IDs'
	UPDATE #workTable
	SET collectionIDs = rs.CollectionIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(CollectionIDs, LEN(CollectionIDs) - 1) AS CollectionIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[LibrarySectionId])+','
			FROM [IsleContent].[dbo].[Library.Resource] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) CollectionIDs
		FROM [IsleContent].[dbo].[Library.Resource] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
	
	-- Comment Count
	PRINT 'Comment Count'
	UPDATE #workTable
	SET commentsCount = rs.CommentsCount
	FROM (SELECT ResourceIntId, COUNT(*) AS CommentsCount
		FROM [Resource.Comment]
		GROUP BY ResourceIntId) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Detail Views
	PRINT 'Detail Views'
	UPDATE #workTable
	SET detailViews = rs.DetailViews,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, COUNT(*) AS DetailViews
		FROM [Resource.DetailView]
		GROUP BY ResourceIntId) rs
	WHERE #workTable.intID = rs.ResourceIntId AND
		(#workTable.detailViews <> rs.DetailViews OR
		(#workTable.detailViews IS NULL AND rs.DetailViews IS NOT NULL))

	-- Favorites
	PRINT 'Favorites'
	UPDATE #workTable
	SET favorites = rs.Favorites,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, r.FavoriteCount + COUNT(*) AS Favorites
		FROM [IsleContent].[dbo].[Library.Resource] lr
		INNER JOIN [Resource] r ON lr.ResourceIntId = r.Id
		GROUP BY ResourceIntId, FavoriteCount) rs
	WHERE #workTable.intID = rs.ResourceIntId AND
		(#workTable.detailViews <> rs.Favorites OR
		(#workTable.detailViews IS NULL AND rs.Favorites IS NOT NULL))

		
	-- Output the data
	SELECT
		versionID,
		intID,
		title,
		[description],
		publisher,
		created,
		accessRights,
		accessRightsID,
		keywords,
		subjects,
		languageIDs,
		languages,
		clusterIDs,
		clusters,
		audienceIDs,
		audiences,
		educationLevelIDs,
		educationLevels,
		educationLevelAliases,
		resourceTypeIDs,
		resourceTypes,
		mediaTypeIDs,
		mediaTypes,
		groupTypeIDs,
		groupTypes,
		itemTypeIDs,
		itemTypes,
		standardIDs,
		standardNotations,
		LastUpdated,
		Url,
		libraryIDs,
		collectionIDs,
		IsDeleted,
		educationalUseIDs,
		educationalUses,
		usageRights,
		usageRightsID,
		usageRightsURL,
		usageRightsIconURL,
		usageRightsMiniIconUrl,
		likesSummary,
		evaluationCount,
		evaluationScore,
		commentsCount,
		viewsCount,
		detailViews,
		favorites,
		likeCount,
		dislikeCount
	FROM #workTable
	
	DROP TABLE #workTable

END

GO
/****** Object:  StoredProcedure [dbo].[Resource_BuildElasticSearchV2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[Resource_BuildElasticSearchV2] '449897'

*/
-- ======================================================================
-- Author:		Jerome Grimmer
-- Create date: 6/4/2013
-- Description:	Build records for a single resource for ElasticSearch
-- TODO: Add code for IsDeleted and Evaluations
--
-- 2013-06-20 jgrimmer - Fixed bug with null grade level aliases
-- 2013-07-26 jgrimmer - Added code to process a list of ResourceIntIds
-- 2013-08-20 jgrimmer - Added LikeCount and DislikeCount
-- 2013-11-27 jgrimmer - Added check for IsDeleted in the Link Checker database
-- 2014-02-03 jgrimmer - Modify favorites to rely on [Resource].FavoriteCount only.  It is detail/library page's responsibility to update that count.
--						 Also add Submitter.
-- 2014-02-25 jgrimmer - Modified to include only resource versions which are active.
-- 2014-05-19 jgrimmer - Exclude resources from Smarter Balance, since Illinois is a PARCC member and not a Smarter Balance member state.
-- ======================================================================
-- 2014-05-21 mparsons - create new version using Resource.Tag
-- 2014-05-28 mparsons - added sortTitle
-- ======================================================================

CREATE PROCEDURE [dbo].[Resource_BuildElasticSearchV2]
	--@ResourceIntId int,
	@ResourceIntId varchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	--Create and populate table of ResourceIntIds to process
	CREATE TABLE #resourceIntIds (intID int)
	/*IF @ResourceIntId <> 0 BEGIN
		INSERT INTO #resourceIntIds (intID)
		VALUES(@ResourceIntId)
	END ELSE BEGIN*/
		DECLARE @pos int,@value varchar(12),@remainder varchar(max), @categoryId int

		SET @remainder = @ResourceIntId
		SET @pos = PATINDEX('%,%',@remainder)
		WHILE(@pos > 0) BEGIN
			SET @value = left(@remainder,@pos - 1)
			INSERT INTO #resourceIntIds (intID)
			VALUES(@value)
			SET @remainder = right(@remainder,len(@remainder) - @pos)
			SET @pos = PATINDEX('%,%',@remainder)
		END
		INSERT INTO #resourceIntIds (intID)
		VALUES(@remainder)
	--END
	
	--Create workTable
	CREATE TABLE #workTable (
		versionID int,
		intID int,
		title nvarchar(300),
		[description] nvarchar(max),
		publisher varchar(200),
		created datetime,
		accessRights varchar(100),
		accessRightsID int,
		keywords nvarchar(max),
		subjects nvarchar(max),
		languageIDs nvarchar(max),
		languages nvarchar(max),
		clusterIDs nvarchar(max),
		clusters nvarchar(max),
		audienceIDs nvarchar(max),
		audiences nvarchar(max),
		educationLevelIDs nvarchar(max),
		educationLevels nvarchar(max),
		educationLevelAliases nvarchar(max),
		resourceTypeIDs nvarchar(max),
		resourceTypes nvarchar(max),
		mediaTypeIDs nvarchar(max),
		mediaTypes nvarchar(max),
		groupTypeIDs nvarchar(max),
		groupTypes nvarchar(max),
		itemTypeIDs nvarchar(max),
		itemTypes nvarchar(max),
		standardIDs nvarchar(max),
		standardNotations nvarchar(max),
		LastUpdated datetime,
		Url varchar(500),
		libraryIDs varchar(max),
		collectionIDs varchar(max),
		IsDeleted bit,
		educationalUseIDs nvarchar(max),
		educationalUses nvarchar(max),
		usageRights varchar(300),
		usageRightsID int,
		usageRightsURL varchar(700),
		usageRightsIconURL varchar(300),
		usageRightsMiniIconUrl varchar(300),
		likesSummary int,
		evaluationCount int,
		evaluationScore int,
		commentsCount int,
		viewsCount int,
		detailViews int,
		favorites int,
		likeCount int,
		dislikeCount int,
		submitter varchar(100),
		accessibilityApiIDs nvarchar(max),
		accessibilityApis nvarchar(max),
		accessibilityControlIDs nvarchar(max),
		accessibilityControls nvarchar(max),
		accessibilityFeatureIDs nvarchar(max),
		accessibilityFeatures nvarchar(max),
		accessibilityHazardIDs nvarchar(max),
		accessibilityHazards nvarchar(max),
		[careerPlannings] [nvarchar](max) NULL,
		[careerPlanningIDs] [nvarchar](max) NULL,
		[disabilityTopics] [nvarchar](max) NULL,
		[disabilityTopicIDs] [nvarchar](max) NULL,
		[jobPreparations] [nvarchar](max) NULL,
		[jobPreparationIDs] [nvarchar](max) NULL,
		[employerPrograms] [nvarchar](max) NULL,
		[employerProgramIDs] [nvarchar](max) NULL,
		[k12Subjects] [nvarchar](max) NULL,
		[k12SubjectIDs] [nvarchar](max) NULL,
		[veteransServices] [nvarchar](max) NULL,
		[veteransServiceIDs] [nvarchar](max) NULL,
		[workSupportServices] [nvarchar](max) NULL,
		[workSupportServiceIDs] [nvarchar](max) NULL,
		[wfePartners] [nvarchar](max) NULL,
		[wfePartnerIDs] [nvarchar](max) NULL,
		[workplaceSkills] [nvarchar](max) NULL,
		[workPlaceSkillsIDs] [nvarchar](max) NULL,
		regions nvarchar(MAX) NULL,
		regionIDs nvarchar(MAX) NULL,
		targetSites nvarchar(MAX) NULL,
		targetSiteIDs nvarchar(MAX) NULL,
		sortTitle nvarchar(300)
		)

	-- Get Resource/ResourceVersion information
	INSERT INTO #workTable (versionId, intId, title, [description], publisher,
		created, accessRights, accessRightsId, LastUpdated, url, usageRightsUrl, usageRights, usageRightsID, usageRightsIconURL, usageRightsMiniIconURL, viewsCount, 
		favorites, submitter, sortTitle)
	SELECT rv.Id, rv.ResourceIntId, rv.Title, rv.[Description], rv.Publisher, rv.Created, rv.AccessRights, rv.AccessRightsId, GETDATE(), r.ResourceUrl,
	rv.Rights, 
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.Summary
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'Read the Fine Print'
		ELSE NULL
	END AS usageRights,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.Id
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 4 -- Read the Fine Print
		ELSE NULL
	END AS usageRightsID,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.IconUrl
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'http://mirrors.creativecommons.org/presskit/cc.primary.srr.gif'
		ELSE NULL
	END usageRightsIconURL,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.MiniIconUrl
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN '/images/icons/rightsreserved.png'
		ELSE NULL
	END AS usageRightsMiniIconUrl,
	ViewCount, r.FavoriteCount, rv.Submitter,
	[dbo].[BuildSortTitle] (rv.[Title])
	FROM #resourceIntIds ridl
	INNER JOIN [Resource.Version] rv ON ridl.intID = rv.ResourceIntId
	INNER JOIN [Resource] r ON rv.ResourceIntId = r.Id
	LEFT JOIN [ConditionOfUse] cou ON rv.Rights = cou.Url
	WHERE ResourceIntId NOT IN (SELECT ResourceIntId FROM [Resource.Link] WHERE IsDeleted = 'True') AND 
		(cou.Id IS NULL OR cou.Id <> 3) AND r.IsActive = 'True' and rv.IsActive = 'True' AND Submitter NOT LIKE '%@smarterbalanced.org%'
	
	-- Keywords
	PRINT 'Keywords'
	UPDATE #workTable
	SET keywords = rs.Keywords,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Keywords, LEN(Keywords) - 1) AS Keywords
		FROM (SELECT ResourceIntId, (
			SELECT itbl.[Keyword]+','
			FROM [Resource.Keyword] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Keywords
		FROM [Resource.Keyword] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Subjects
	PRINT 'Subjects'
	UPDATE #workTable
	SET [subjects] = rs.[Subjects],
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Subjects, LEN(Subjects) - 1) AS Subjects
		FROM (SELECT ResourceIntId, (
			SELECT itbl.[Subject]+','
			FROM [Resource.Subject] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Subjects
		FROM [Resource.Subject] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID	
	--===================================================================
	-- k12Subject Ids
	set @categoryId= 20
	UPDATE #workTable
	SET k12SubjectIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- k12Subjects
	
	UPDATE #workTable
	SET k12Subjects = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 


	--===================================================================
	--LanguageIDs
	set @categoryId= 17
	PRINT 'Language IDs'
	UPDATE #workTable
	SET [languageIDs] = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
			
	-- Languages
	PRINT 'Languages'
	UPDATE #workTable
	SET [languages] = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- Career Cluster IDs
	set @categoryId= 8
	PRINT 'Cluster IDs'
	UPDATE #workTable
	SET clusterIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
	
	-- Clusters

	PRINT 'Clusters'
	UPDATE #workTable
	SET clusters = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- Intended Audience Ids
	set @categoryId= 7
	PRINT 'Intended Audience IDs'
	UPDATE #workTable
	SET audienceIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- Intended Audiences	
	PRINT 'Intended Audiences'
	UPDATE #workTable
	SET audiences = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- GradeLevel IDs
	set @categoryId= 13
	PRINT 'Grade Level IDs'
	UPDATE #workTable
	SET educationLevelIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- Grade Levels
	PRINT 'Grade Levels'
	UPDATE #workTable
	SET educationLevels = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- Grade Level Aliases
	PRINT 'Grade Level Aliases'
	UPDATE #workTable
	SET educationLevelAliases = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, 
			CASE 
			WHEN LEN(RelatedList) > 1 THEN replace(left(RelatedList, LEN(RelatedList) - 1),',,',',')
			ELSE NULL
		END AS RelatedTags

			--,left(RelatedList, LEN(RelatedList) - 1) AS relatedTags2
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.AliasValues + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- =================================================================
	-- Resource Type IDs
	set @categoryId= 19
	PRINT 'Resource Type IDs'
	UPDATE #workTable
	SET resourceTypeIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 


	-- Resource Types
	PRINT 'Resource Types'
	UPDATE #workTable
	SET resourceTypes = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 


	-- Resource Format IDs
	set @categoryId= 18
	PRINT 'Resource Format IDs'
	UPDATE #workTable
	SET mediaTypeIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- Resource Formats
	PRINT 'Resource Formats'
	UPDATE #workTable
	SET mediaTypes = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- Group Type IDs
	set @categoryId= 14
	PRINT 'Group Type IDs'
	UPDATE #workTable
	SET groupTypeIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- Group Types
	PRINT 'Group Types'
	UPDATE #workTable
	SET groupTypes = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
				
	-- Item Type IDs
	set @categoryId= 15
	PRINT 'Item Type IDs'	
	UPDATE #workTable
	SET itemTypeIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- Item Types
	PRINT 'Item Types'
	UPDATE #workTable
	SET itemTypes = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- Standard IDs
	PRINT 'Standard IDs'
	UPDATE #workTable
	SET standardIDs = rs.StandardIDs
	FROM (SELECT ResourceIntId, left(StandardIDs, LEN(StandardIDs) - 1) AS StandardIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[StandardId])+','
			FROM [Resource.Standard] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) StandardIDs
		FROM [Resource.Standard] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
		
	-- Standard Notations
	PRINT 'Standards'
	UPDATE #workTable
	SET standardNotations = rs.Standards,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(Standards, LEN(Standards) - 1) AS Standards
		FROM (SELECT ResourceIntId, (
			SELECT [StandardBody.Node].NotationCode+','
			FROM [Resource.Standard] itbl
			INNER JOIN [StandardBody.Node] ON itbl.StandardId = [StandardBody.Node].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) Standards
		FROM [Resource.Standard] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Educational Use IDs
	set @categoryId= 11
	PRINT 'Educational Use IDs'
	UPDATE #workTable
	SET educationalUseIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- Educational Uses
	PRINT 'Educational Uses'
	UPDATE #workTable
	SET educationalUses = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- Likes minus dislikes
	PRINT 'Likes - Dislikes'
	UPDATE #workTable
	SET likesSummary = rls.LikeCount - rls.DislikeCount,
		LastUpdated = GETDATE()
	FROM [Resource.LikesSummary] rls
	WHERE rls.ResourceIntId = #workTable.intID
	
	-- LikeCount, DislikeCount
	PRINT 'LikeCount, DislikeCount'
	UPDATE #workTable
	SET likeCount = rls.LikeCount,
		dislikeCount = rls.DislikeCount
	FROM [Resource.LikesSummary] rls
	WHERE rls.ResourceIntId = #workTable.intID

	-- LibraryIDs
	PRINT 'Library IDs'
	UPDATE #workTable
	SET libraryIDs = rs.LibraryIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(LibraryIDs, LEN(LibraryIDs) - 1) AS LibraryIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,[IsleContent].[dbo].[Library.Section].LibraryId)+','
			FROM [IsleContent].[dbo].[Library.Resource] itbl
			INNER JOIN [IsleContent].[dbo].[Library.Section] 
				ON itbl.LibrarySectionId = [IsleContent].[dbo].[Library.Section].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) LibraryIDs
		FROM [IsleContent].[dbo].[Library.Resource] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Collection IDs
	PRINT 'Collection IDs'
	UPDATE #workTable
	SET collectionIDs = rs.CollectionIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(CollectionIDs, LEN(CollectionIDs) - 1) AS CollectionIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[LibrarySectionId])+','
			FROM [IsleContent].[dbo].[Library.Resource] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId
			for xml path('')) CollectionIDs
		FROM [IsleContent].[dbo].[Library.Resource] tbl
		GROUP BY ResourceIntId) otab) rs
	WHERE rs.ResourceIntId = #workTable.intID
	
	-- Comment Count
	PRINT 'Comment Count'
	UPDATE #workTable
	SET commentsCount = rs.CommentsCount
	FROM (SELECT ResourceIntId, COUNT(*) AS CommentsCount
		FROM [Resource.Comment]
		GROUP BY ResourceIntId) rs
	WHERE rs.ResourceIntId = #workTable.intID

	-- Detail Views
	PRINT 'Detail Views'
	UPDATE #workTable
	SET detailViews = rs.DetailViews,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, COUNT(*) AS DetailViews
		FROM [Resource.DetailView]
		GROUP BY ResourceIntId) rs
	WHERE #workTable.intID = rs.ResourceIntId AND
		(#workTable.detailViews <> rs.DetailViews OR
		(#workTable.detailViews IS NULL AND rs.DetailViews IS NOT NULL))

	-- ======================================================================
	-- accessibilityApi IDs
	set @categoryId= 2
	PRINT 'AccessibilityApi IDs'
	UPDATE #workTable
	SET accessibilityApiIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- accessibilityApis
	PRINT 'AccessibilityApis'
	UPDATE #workTable
	SET accessibilityApis = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- accessibilityControl IDs
	set @categoryId= 3
	PRINT 'AccessibilityControl IDs'
	UPDATE #workTable
	SET accessibilityControlIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- accessibilityControls
	PRINT 'AccessibilityControls'
	UPDATE #workTable
	SET accessibilityControls = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- accessibilityFeature IDs
	set @categoryId= 4
	PRINT 'AccessibilityFeature IDs'
	UPDATE #workTable
	SET accessibilityFeatureIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- accessibilityFeatures
	PRINT 'AccessibilityFeatures'
	UPDATE #workTable
	SET accessibilityFeatures = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- accessibilityHazard IDs
	set @categoryId= 5
	PRINT 'AccessibilityHazard IDs'
	UPDATE #workTable
	SET accessibilityHazardIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- accessibilityHazards
	PRINT 'AccessibilityHazards'
	UPDATE #workTable
	SET accessibilityHazards = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- =============================================================================
	-- 9 - career planning IDs
	set @categoryId= 9
	PRINT 'careerPlanningIDs'
	UPDATE #workTable
	SET careerPlanningIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- accessibilityHazards
	PRINT 'careerPlannings'
	UPDATE #workTable
	SET careerPlannings = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- =============================================================================
	-- disabilityTopicIDs
	set @categoryId= 10
	PRINT 'disabilityTopicIDs'
	UPDATE #workTable
	SET disabilityTopicIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	-- disabilityTopics
	PRINT 'disabilityTopics'
	UPDATE #workTable
	SET disabilityTopics = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
	
	-- =============================================================================
	-- employerProgramIDs
	set @categoryId= 12
	PRINT 'employerProgramIDs'
	UPDATE #workTable
	SET employerProgramIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	PRINT 'employerPrograms'
	UPDATE #workTable
	SET employerPrograms = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 


	-- =============================================================================
	-- jobPreparationIDs
	set @categoryId= 16
	PRINT 'jobPreparationIDs'
	UPDATE #workTable
	SET jobPreparationIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	PRINT 'jobPreparations'
	UPDATE #workTable
	SET jobPreparations = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	-- =============================================================================
	-- veteransServiceIDs
	set @categoryId= 21
	PRINT 'veteransServiceIDs'
	UPDATE #workTable
	SET veteransServiceIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	PRINT 'veteransServices'
	UPDATE #workTable
	SET veteransServices = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 


	-- =============================================================================
	-- wfePartnerIDs
	set @categoryId= 22
	PRINT 'wfePartnerIDs'
	UPDATE #workTable
	SET wfePartnerIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	PRINT 'wfePartners'
	UPDATE #workTable
	SET wfePartners = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	
	-- =============================================================================
	-- workSupportServiceIDs
	set @categoryId= 23
	PRINT 'workSupportServiceIDs'
	UPDATE #workTable
	SET workSupportServiceIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	PRINT 'wfePartners'
	UPDATE #workTable
	SET workSupportServices = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	
	-- =============================================================================
	-- workPlaceSkillsIDs
	set @categoryId= 24
	PRINT 'workPlaceSkillsIDs'
	UPDATE #workTable
	SET workPlaceSkillsIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	PRINT 'workPlaceSkills'
	UPDATE #workTable
	SET workPlaceSkills = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 

	
	-- =============================================================================
	-- regionIDs
	set @categoryId= 25
	PRINT 'regionIDs'
	UPDATE #workTable
	SET regionIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	PRINT 'regions'
	UPDATE #workTable
	SET regions = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 


	-- =============================================================================
	-- targetSiteIDs
	set @categoryId= 27
	PRINT 'targetSiteIDs'
	UPDATE #workTable
	SET targetSiteIDs = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT convert(varchar,itbl.TagValueId)+','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
			FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 
		
	PRINT 'targetSites'
	UPDATE #workTable
	SET targetSites = rs.relatedTags,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
		FROM (SELECT ResourceIntId, (
			SELECT  itbl.TagTitle + ','
			FROM [Resource_TagSummary] itbl
			WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
			for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
		WHERE tbl.CategoryId = @categoryId
		GROUP BY ResourceIntId) otab) rs
	WHERE #workTable.intID = rs.ResourceIntId 


-- =============================================================================

-- Favorites - commented out.  Relying only on [Resource].FavoriteCount
/*	PRINT 'Favorites'
	UPDATE #workTable
	SET favorites = rs.Favorites,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, r.FavoriteCount + COUNT(*) AS Favorites
		FROM [IsleContent].[dbo].[Library.Resource] lr
		INNER JOIN [Resource] r ON lr.ResourceIntId = r.Id
		GROUP BY ResourceIntId, FavoriteCount) rs
	WHERE #workTable.intID = rs.ResourceIntId AND
		(#workTable.detailViews <> rs.Favorites OR
		(#workTable.detailViews IS NULL AND rs.Favorites IS NOT NULL)) */

	-- ==========================================================================================		
	-- Output the data
	SELECT
		versionID,
		intID,
		title,
		[description],
		publisher,
		created,
		accessRights,
		accessRightsID,
		keywords,
		subjects,
		languageIDs,
		languages,
		clusterIDs,
		clusters,
		audienceIDs,
		audiences,
		educationLevelIDs,
		educationLevels,
		educationLevelAliases,
		resourceTypeIDs,
		resourceTypes,
		mediaTypeIDs,
		mediaTypes,
		groupTypeIDs,
		groupTypes,
		itemTypeIDs,
		itemTypes,
		standardIDs,
		standardNotations,
		LastUpdated,
		Url,
		libraryIDs,
		collectionIDs,
		IsDeleted,
		educationalUseIDs,
		educationalUses,
		usageRights,
		usageRightsID,
		usageRightsURL,
		usageRightsIconURL,
		usageRightsMiniIconUrl,
		likesSummary,
		evaluationCount,
		evaluationScore,
		commentsCount,
		viewsCount,
		detailViews,
		favorites,
		likeCount,
		dislikeCount,
		submitter,
		accessibilityApiIDs,
		accessibilityApis,
		accessibilityControlIDs,
		accessibilityControls,
		accessibilityFeatureIDs,
		accessibilityFeatures,
		accessibilityHazardIDs,
		accessibilityHazards,
		[careerPlannings] ,
		[careerPlanningIDs], 
		[disabilityTopics],
		[disabilityTopicIDs],
		[jobPreparations],
		[jobPreparationIDs],
		[employerPrograms],
		[employerProgramIDs],
		[k12Subjects],
		[k12SubjectIDs],
		[veteransServices],
		[veteransServiceIDs],
		[workSupportServices],
		[workSupportServiceIDs],
		[wfePartners],
		[wfePartnerIDs] ,
		[workplaceSkills],
		[workPlaceSkillsIDs],
		regions,
		regionIDs,
		targetSites,
		targetSiteIDs,
		sortTitle
	FROM #workTable
	
	DROP TABLE #workTable

END

GO
/****** Object:  StoredProcedure [dbo].[Resource_IndexUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 4/25/2013
-- Description:	Update Resource_Index table for ElasticSearch
--
-- 2013-07-08 jgrimmer - Remove resources where [Resource.Version].IsActive <> 'True'
-- 2013-08-20 jgrimmer - Added likeCount and dislikeCount
-- 2013-11-20 jgrimmer - Added check for IsDeleted in the LinkChecker database
-- 2014-02-03 jgrimmer - Modify favorites to rely on [Resource].FavoriteCount only.  It is detail/library page's responsibility to update that count.
--						 Also add Submitter.
-- 2014-05-19 jgrimmer - Exclude resources from Smarter Balance, since Illinois is a PARCC member and not a Smarter Balance member state, and remove 
--						 check for PrivateResource (docID is null)
-- 2014-05-28 mparsons - added sortTitle
-- =============================================
CREATE PROCEDURE [dbo].[Resource_IndexUpdate]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @InitStartTime datetime, @StartTime datetime, @EndTime datetime, @ElapsedTime float, @FinishTime datetime

SET @StartTime = GETDATE()
SET @InitStartTime = @StartTime

-- Remove inactive resources
DELETE
FROM [Resource_Index]
WHERE intID IN (SELECT Id FROM [Resource] WHERE IsActive = 'False') OR
	versionID IN (SELECT Id FROM [Resource.Version] WHERE IsActive = 'False') OR
	intId IN (Select ResourceIntId FROM [Resource.Link] WHERE IsDeleted = 'True')
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'Delete inactive resources = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()


-- Update existing resources Resource.Version data
UPDATE Resource_Index
SET title = rv.Title, 
	[description] = rv.[Description],
	publisher = rv.Publisher,
	created = rv.Created,
	accessRights = rv.AccessRights,
	accessRightsID = rv.AccessRightsId,
	LastUpdated = GETDATE(),
	url = r.ResourceUrl,
	usageRightsURL = rv.Rights,
	usageRights = 
		CASE 
			WHEN cou.Id IS NOT NULL THEN cou.Summary
			WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'Read the Fine Print'
			ELSE NULL
		END,
	usageRightsID = 
		CASE 
			WHEN cou.Id IS NOT NULL THEN cou.Id
			WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 4  -- Read the Fine Print
			ELSE NULL
		END,
	usageRightsIconURL = 
		CASE 
			WHEN cou.Id IS NOT NULL THEN cou.IconUrl
			WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'http://mirrors.creativecommons.org/presskit/cc.primary.srr.gif'
			ELSE NULL
		END,
	usageRightsMiniIconURL = 
		CASE 
			WHEN cou.Id IS NOT NULL THEN cou.MiniIconUrl
			WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN '/images/icons/rightsreserved.png'
			ELSE NULL
		END,
	viewsCount = ViewCount,
	timeRequired = TypicalLearningTime,
	creator = rv.Creator,
	requirements = rv.Requirements,
	isBasedOnUrl = RelatedUrl,
	favorites = r.FavoriteCount,
		submitter = rv.Submitter,
	 [SortTitle] = [dbo].[BuildSortTitle] (rv.[Title])
FROM [Resource.Version] rv
INNER JOIN [Resource] r ON rv.ResourceIntId = r.Id
LEFT JOIN [ConditionOfUse] cou ON rv.Rights = cou.Url
LEFT JOIN [Resource.RelatedUrl] rru ON r.Id = rru.ResourceIntId
WHERE Resource_Index.versionId = rv.Id AND
	(Resource_Index.title <> rv.Title OR Resource_Index.[description] <> rv.[Description] OR
	 Resource_Index.publisher <> rv.Publisher OR  Resource_Index.accessRights <> rv.AccessRights OR
	 Resource_Index.accessRightsId <> rv.AccessRightsId OR Resource_Index.url <> r.ResourceUrl OR
	 Resource_Index.usageRightsURL <> rv.Rights OR viewsCount <> ViewCount OR
	 Resource_Index.timeRequired <> rv.TypicalLearningTime OR Resource_Index.creator <> rv.Creator OR
	 Resource_Index.requirements <> rv.Requirements OR Resource_Index.isBasedOnUrl <> rru.RelatedUrl OR
 	 Resource_Index.favorites <> r.FavoriteCount OR
	 Resource_Index.submitter <> rv.Submitter) AND
	 Resource_Index.intID NOT IN (SELECT ResourceIntId FROM [Resource.Link] WHERE IsDeleted = 'True') AND
	 rv.Submitter NOT LIKE '%smarterbalanced.org%'

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Resource.Version] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Add new resources, skipping inactive and private resources
INSERT INTO Resource_Index (versionId, intId, title, [description], publisher,
	created, accessRights, accessRightsId, LastUpdated, url, usageRightsUrl, usageRights, usageRightsID, usageRightsIconURL, usageRightsMiniIconURL,
	viewsCount, timeRequired, creator, requirements, isBasedOnUrl, favorites, submitter)
SELECT DISTINCT rv.Id, rv.ResourceIntId, rv.Title, rv.[Description], rv.Publisher, rv.Created, rv.AccessRights, rv.AccessRightsId, GETDATE(), r.ResourceUrl,
	rv.Rights, 
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.Summary
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'Read the Fine Print'
		ELSE NULL
	END AS usageRights,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.Id
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 4 -- Read the Fine Print
		ELSE NULL
	END AS usageRightsId,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.IconUrl
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'http://mirrors.creativecommons.org/presskit/cc.primary.srr.gif'
		ELSE NULL
	END AS usageRightsIconURL,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.MiniIconUrl
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN '/images/icons/rightsreserved.png'
		ELSE NULL
	END AS usageRightsMiniIconURL,
	ViewCount, TypicalLearningTime, Creator, Requirements, RelatedUrl, r.FavoriteCount, rv.Submitter
FROM [Resource.Version] rv
INNER JOIN [Resource] r ON rv.ResourceIntId = r.Id
LEFT JOIN [Resource.PublishedBy] rpb ON rv.ResourceIntId = rpb.ResourceIntId
LEFT JOIN [ConditionOfUse] cou ON rv.Rights = cou.Url
LEFT JOIN [Resource.RelatedUrl] rru ON r.Id = rru.ResourceIntId
WHERE rv.Id NOT IN (SELECT versionId FROM Resource_Index) AND (cou.Id IS NULL OR cou.Id <> 3) AND r.IsActive = 'True' AND 
	rv.IsActive = 'True' AND (rpb.PublishedById IS NULL OR rpb.PublishedById <> 22) AND
	r.Id NOT IN (SELECT ResourceIntId FROM [Resource.Link] WHERE IsDeleted = 'True') AND
	rv.Submitter NOT LIKE '%smarterbalanced.org%'

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'INSERT from [Resource.Version] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Keywords
UPDATE Resource_Index
SET keywords = rk.Keywords,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Keywords, LEN(Keywords) - 1) AS Keywords
	FROM (SELECT ResourceIntId, (
		SELECT isnull(itbl.[Keyword],'')+','
		FROM [Resource.Keyword] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Keywords
	FROM [Resource.Keyword] tbl
	GROUP BY ResourceIntId) otab) rk
WHERE Resource_Index.intId = rk.ResourceIntId AND
	(Resource_Index.keywords <> rk.Keywords OR 
	(Resource_Index.keywords IS NULL AND rk.Keywords IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Resource.Keyword] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Subjects
UPDATE Resource_Index
SET [subjects] = rs.[Subjects],
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Subjects, LEN(Subjects) - 1) AS Subjects
	FROM (SELECT ResourceIntId, (
		SELECT itbl.[Subject]+','
		FROM [Resource.Subject] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Subjects
	FROM [Resource.Subject] tbl
	GROUP BY ResourceIntId) otab) rs 
WHERE Resource_Index.intId = rs.ResourceIntId AND
	(Resource_Index.subjects <> rs.Subjects OR
	(Resource_Index.subjects IS NULL AND rs.Subjects IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Resource.Subject] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Language IDs
UPDATE Resource_Index
SET [languageIDs] = rl1.[LanguageIds],
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Languages, LEN(Languages) - 1) AS LanguageIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[LanguageId])+','
		FROM [Resource.Language] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Languages
	FROM [Resource.Language] tbl
	GROUP BY ResourceIntId) otab) rl1
WHERE Resource_Index.intId = rl1.ResourceIntId AND
	(Resource_Index.languageIDs <> rl1.LanguageIds OR
	(Resource_Index.languageIDs IS NULL AND rl1.LanguageIds IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.Language] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Languages
UPDATE Resource_Index
SET [languages] = rl2.[Languages],
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Languages, LEN(Languages) - 1) AS Languages
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.Language].Title+','
		FROM [Resource.Language] itbl
		INNER JOIN [Codes.Language] ON itbl.LanguageId = [Codes.Language].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Languages
	FROM [Resource.Language] tbl
	GROUP BY ResourceIntId) otab) rl2
WHERE Resource_Index.intId = rl2.ResourceIntId AND
	(Resource_Index.languages <> rl2.Languages OR
	(Resource_Index.languages IS NULL AND rl2.Languages IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.Language] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Career Cluster IDs
UPDATE Resource_Index
SET clusterIDs = cc1.ClusterIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Clusters, LEN(Clusters) - 1) AS ClusterIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[ClusterId])+','
		FROM [Resource.Cluster] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Clusters
	FROM [Resource.Cluster] tbl
	GROUP BY ResourceIntId) otab) cc1
WHERE Resource_Index.intId = cc1.ResourceIntId AND
	(Resource_Index.clusterIDs <> cc1.ClusterIds OR 
	(Resource_Index.clusterIDs IS NULL AND cc1.ClusterIds IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.Cluster] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Clusters
UPDATE Resource_Index
SET clusters = cc2.Clusters,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Clusters, LEN(Clusters) - 1) AS Clusters
	FROM (SELECT ResourceIntId, (
		SELECT '"'+[CareerCluster].IlPathwayName+'",'
		FROM [Resource.Cluster] itbl
		INNER JOIN [CareerCluster] ON itbl.ClusterId = [CareerCluster].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Clusters
	FROM [Resource.Cluster] tbl
	GROUP BY ResourceIntId) otab) cc2
WHERE Resource_Index.intId = cc2.ResourceIntId AND
	(Resource_Index.clusters <> cc2.Clusters OR
	(Resource_Index.clusters IS NULL AND cc2.Clusters IS NOT NULL))


SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.Cluster] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Intended Audience Ids
UPDATE Resource_Index
SET audienceIDs = ia1.AudienceIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Audiences, LEN(Audiences) - 1) AS AudienceIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[AudienceId])+','
		FROM [Resource.IntendedAudience] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Audiences
	FROM [Resource.IntendedAudience] tbl
	GROUP BY ResourceIntId) otab) ia1
WHERE Resource_Index.intId = ia1.ResourceIntId AND
	(Resource_Index.audienceIDs <> ia1.AudienceIds OR
	(Resource_Index.audienceIDs IS NULL AND ia1.AudienceIds IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.IntendedAudience] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Intended Audiences
UPDATE Resource_Index
SET audiences = ia2.Audiences,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Audiences, LEN(Audiences) - 1) AS Audiences
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.AudienceType].Title+','
		FROM [Resource.IntendedAudience] itbl
		INNER JOIN [Codes.AudienceType] ON itbl.AudienceId = [Codes.AudienceType].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Audiences
	FROM [Resource.IntendedAudience] tbl
	GROUP BY ResourceIntId) otab) ia2
WHERE Resource_Index.intId = ia2.ResourceIntId AND
	(Resource_Index.audiences <> ia2.Audiences OR
	(Resource_Index.audiences IS NULL AND ia2.Audiences IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.IntendedAudience] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- gradeLevelIds
UPDATE Resource_Index
SET gradeLevelIDs = el1.gradeLevelIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(gradeLevels, LEN(gradeLevels) - 1) AS gradeLevelIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[GradeLevelId])+','
		FROM [Resource.GradeLevel] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) gradeLevels
	FROM [Resource.GradeLevel] tbl
	GROUP BY ResourceIntId) otab) el1
WHERE Resource_Index.intId = el1.ResourceIntId AND
	(Resource_Index.gradeLevelIds <> el1.gradeLevelIds OR
	(Resource_Index.gradeLevelIds IS NULL AND el1.gradeLevelIds IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.GradeLevel] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- gradeLevels
UPDATE Resource_Index
SET gradeLevels = el2.gradeLevels,
	LastUpdated = GETDATE()	
FROM (SELECT ResourceIntId, left(gradeLevels, LEN(gradeLevels) - 1) AS gradeLevels
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.GradeLevel].Title+','
		FROM [Resource.GradeLevel] itbl
		INNER JOIN [Codes.GradeLevel] ON itbl.GradeLevelId = [Codes.GradeLevel].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) gradeLevels
	FROM [Resource.GradeLevel] tbl
	GROUP BY ResourceIntId) otab) el2
WHERE Resource_Index.intId = el2.ResourceIntId AND
	(Resource_Index.gradeLevels <> el2.gradeLevels OR
	(Resource_Index.gradeLevels IS NULL AND el2.gradeLevels IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.GradeLevel] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- gradeLevelAliases
UPDATE Resource_Index
SET gradeLevelAliases = el2.gradeLevels,
	LastUpdated = GETDATE()	
FROM (SELECT ResourceIntId, 
		CASE 
			WHEN LEN(gradeLevels) > 1 THEN replace(left(gradeLevels, LEN(gradeLevels) - 1),',,',',')
			ELSE NULL
		END AS gradeLevels
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.GradeLevel].AliasValues+','
		FROM [Resource.GradeLevel] itbl
		INNER JOIN [Codes.GradeLevel] ON itbl.GradeLevelId = [Codes.GradeLevel].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) gradeLevels
	FROM [Resource.GradeLevel] tbl
	GROUP BY ResourceIntId) otab) el2
WHERE Resource_Index.intId = el2.ResourceIntId AND
	(Resource_Index.gradeLevelAliases <> el2.gradeLevels OR
	(Resource_Index.gradeLevelAliases IS NULL AND el2.gradeLevels IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #3 from [Resource.GradeLevel] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Resource Type Ids
UPDATE Resource_Index
SET resourceTypeIDs = rt1.ResourceTypeIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(ResourceTypes, LEN(ResourceTypes) - 1) AS ResourceTypeIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[ResourceTypeId])+','
		FROM [Resource.ResourceType] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) ResourceTypes
	FROM [Resource.ResourceType] tbl
	GROUP BY ResourceIntId) otab) rt1
WHERE Resource_Index.intId = rt1.ResourceIntId AND
	(Resource_Index.resourceTypeIds <> rt1.ResourceTypeIds OR
	(Resource_Index.resourceTypeIds IS NULL AND rt1.ResourceTypeIds IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.ResourceType] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Resource Types
UPDATE Resource_Index
SET resourceTypes = rt2.ResourceTypes,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(ResourceTypes, LEN(ResourceTypes) - 1) AS ResourceTypes
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.ResourceType].Title+','
		FROM [Resource.ResourceType] itbl
		INNER JOIN [Codes.ResourceType] ON itbl.ResourceTypeId = [Codes.ResourceType].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) ResourceTypes
	FROM [Resource.ResourceType] tbl
	GROUP BY ResourceIntId) otab) rt2
WHERE Resource_Index.intId = rt2.ResourceIntId AND
	(Resource_Index.resourceTypes <> rt2.ResourceTypes OR
	(Resource_Index.resourceTypes IS NULL AND rt2.ResourceTypes IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.ResourceType] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Resource Format Ids
UPDATE Resource_Index
SET mediaTypeIDs = rf1.ResourceFormatIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(ResourceFormats, LEN(ResourceFormats) - 1) AS ResourceFormatIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[CodeId])+','
		FROM [Resource.Format] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) ResourceFormats
	FROM [Resource.Format] tbl
	GROUP BY ResourceIntId) otab) rf1
WHERE Resource_Index.intId = rf1.ResourceIntId AND
	(Resource_Index.mediaTypeIDs <> rf1.ResourceFormatIds OR
	(Resource_Index.mediaTypeIDs IS NULL AND rf1.ResourceFormatIds IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.Format] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Resource Formats
UPDATE Resource_Index
SET mediaTypes = rf2.ResourceFormats,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(ResourceFormats, LEN(ResourceFormats) - 1) AS ResourceFormats
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.ResourceFormat].Title+','
		FROM [Resource.Format] itbl
		INNER JOIN [Codes.ResourceFormat] ON itbl.CodeId = [Codes.ResourceFormat].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) ResourceFormats
	FROM [Resource.Format] tbl
	GROUP BY ResourceIntId) otab) rf2
WHERE Resource_Index.intId = rf2.ResourceIntId AND
	(Resource_Index.mediaTypes <> rf2.ResourceFormats OR
	(Resource_Index.mediaTypes IS NULL AND rf2.ResourceFormats IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.Format] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Group Type Ids
UPDATE Resource_Index
SET groupTypeIDs = gt1.GroupTypeIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(GroupTypes, LEN(GroupTypes) - 1) AS GroupTypeIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[GroupTypeId])+','
		FROM [Resource.GroupType] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) GroupTypes
	FROM [Resource.GroupType] tbl
	GROUP BY ResourceIntId) otab) gt1
WHERE Resource_Index.intId = gt1.ResourceIntId AND
	(Resource_Index.groupTypeIds <> gt1.GroupTypeIds OR
	(Resource_Index.groupTypeIds IS NULL AND gt1.GroupTypeIds IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.GroupTypes] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Group Types
UPDATE Resource_Index
SET groupTypes = gt2.GroupTypes,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(GroupTypes, LEN(GroupTypes) - 1) AS GroupTypes
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.GroupType].Title+','
		FROM [Resource.GroupType] itbl
		INNER JOIN [Codes.GroupType] ON itbl.GroupTypeId = [Codes.GroupType].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) GroupTypes
	FROM [Resource.GroupType] tbl
	GROUP BY ResourceIntId) otab) gt2
WHERE Resource_Index.intId = gt2.ResourceIntId AND
	(Resource_Index.groupTypes <> gt2.GroupTypes OR
	(Resource_Index.groupTypes IS NULL AND gt2.GroupTypes IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.GroupTypes] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Item Type Ids
UPDATE Resource_Index
SET itemTypeIDs = it1.ItemTypeIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(ItemTypes, LEN(ItemTypes) - 1) AS ItemTypeIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[ItemTypeId])+','
		FROM [Resource.ItemType] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) ItemTypes
	FROM [Resource.ItemType] tbl
	GROUP BY ResourceIntId) otab) it1
WHERE Resource_Index.intId = it1.ResourceIntId AND
	(Resource_Index.itemTypeIds <> it1.ItemTypeIds OR
	(Resource_Index.itemTypeIds IS NULL AND it1.ItemTypeIds IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.ItemTypes] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
	
-- Item Types
UPDATE Resource_Index
SET itemTypes = it2.ItemTypes,
	LastUpdated = getdate()
FROM (SELECT ResourceIntId, left(ItemTypes, LEN(ItemTypes) - 1) AS ItemTypes
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.ItemType].Title+','
		FROM [Resource.ItemType] itbl
		INNER JOIN [Codes.ItemType] ON itbl.ItemTypeId = [Codes.ItemType].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) ItemTypes
	FROM [Resource.Format] tbl
	GROUP BY ResourceIntId) otab) it2 
WHERE Resource_Index.intId = it2.ResourceIntId AND
	(Resource_Index.itemTypes <> it2.ItemTypes OR
	(Resource_Index.itemTypes IS NULL AND it2.ItemTypes IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.ItemTypes] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Standard Ids
UPDATE Resource_Index
SET standardIDs = rst1.StandardIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Standards, LEN(Standards) - 1) AS StandardIds
	FROM (SELECT ResourceIntId, (
		SELECT isnull(convert(varchar,itbl.[StandardId]),'')+','
		FROM [Resource.Standard] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Standards
	FROM [Resource.Standard] tbl
	GROUP BY ResourceIntId) otab) rst1
WHERE Resource_Index.intId = rst1.ResourceIntId AND
	(Resource_Index.StandardIDs <> rst1.StandardIds OR
	(Resource_Index.StandardIDs IS NULL AND rst1.StandardIds IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.Standards] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Standards
UPDATE Resource_Index
SET standardNotations = rst2.Standards,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Standards, LEN(Standards) - 1) AS Standards
	FROM (SELECT ResourceIntId, (
		SELECT [StandardBody.Node].NotationCode+','
		FROM [Resource.Standard] itbl
		INNER JOIN [StandardBody.Node] ON itbl.StandardId = [StandardBody.Node].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Standards
	FROM [Resource.Standard] tbl
	GROUP BY ResourceIntId) otab) rst2 
WHERE Resource_Index.intId = rst2.ResourceIntId AND
	(Resource_Index.standardNotations <> rst2.Standards OR
	(Resource_Index.standardNotations IS NULL AND rst2.Standards IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
SET @FinishTime = @EndTime
PRINT 'UPDATE #2 from [Resource.Standards] = ' + convert(varchar,@ElapsedTime)

-- Alignment Type IDs
UPDATE Resource_Index
SET alignmentTypeIDs = rs.AlignmentTypeIDs,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(AlignmentTypeIDs, LEN(AlignmentTypeIDs) - 1) AS AlignmentTypeIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,isnull(itbl.[AlignmentTypeCodeId],2))+','
			FROM [Resource.Standard] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
			for xml path('')) AlignmentTypeIDs
		FROM [Resource.Standard] tbl
		GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND 
	(Resource_Index.alignmentTypeIDs <> rs.AlignmentTypeIDs OR
	(Resource_Index.alignmentTypeIDs IS NULL AND rs.AlignmentTypeIDs IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
SET @FinishTime = @EndTime
PRINT 'UPDATE Alignment Type IDs = ' + convert(varchar,@ElapsedTime)
	
-- Alignment Types
UPDATE Resource_Index
SET alignmentTypes = rs.AlignmentTypes,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(AlignmentTypes, LEN(AlignmentTypes) - 1) AS AlignmentTypes
		FROM (SELECT ResourceIntId, (
			SELECT isnull([Codes.AlignmentType].Title,'Teaches')+','
			FROM [Resource.Standard] itbl
			LEFT JOIN [Codes.AlignmentType] ON itbl.AlignmentTypeCodeId = [Codes.AlignmentType].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
			for xml path('')) AlignmentTypes
		FROM [Resource.Standard] tbl
		GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.alignmentTypes <> rs.AlignmentTypes OR
	(Resource_Index.alignmentTypes IS NULL AND rs.AlignmentTypes IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
SET @FinishTime = @EndTime
PRINT 'UPDATE Alignment Types = ' + convert(varchar,@ElapsedTime)

-- EducationalUseIds
UPDATE Resource_Index
SET educationalUseIDs = eu1.EducationalUseIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(EducationalUse, LEN(EducationalUse) - 1) AS EducationalUseIds
	FROM (SELECT ResourceIntId, (
		SELECT isnull(convert(varchar,itbl.[EducationUseId]),'')+','
		FROM [Resource.EducationUse] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) EducationalUse
	FROM [Resource.EducationUse] tbl
	GROUP BY ResourceIntId) otab) eu1
WHERE Resource_Index.intId = eu1.ResourceIntId AND
	(Resource_Index.educationalUseIDs <> eu1.EducationalUseIds OR
	(Resource_Index.educationalUseIDs IS NULL AND eu1.EducationalUseIds IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.EducationUse] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- EducationalUse
UPDATE Resource_Index
SET educationalUses = eu2.EducationalUses,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(EducationalUse, LEN(EducationalUse) - 1) AS EducationalUses
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.EducationalUse].Title+','
		FROM [Resource.EducationUse] itbl
		INNER JOIN [Codes.EducationalUse] ON itbl.EducationUseId = [Codes.EducationalUse].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) EducationalUse
	FROM [Resource.EducationUse] tbl
	GROUP BY ResourceIntId) otab) eu2 
WHERE Resource_Index.intId = eu2.ResourceIntId AND
	(Resource_Index.educationalUses <> eu2.EducationalUses OR
	(Resource_Index.educationalUses IS NULL AND eu2.EducationalUses IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.EducationUse] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Likes minus Dislikes
UPDATE Resource_Index
SET likesSummary = rls.LikeCount - rls.DislikeCount,
	LastUpdated = GETDATE()
FROM [Resource.LikesSummary] rls
WHERE Resource_Index.intId = rls.ResourceIntId AND
	((Resource_Index.likesSummary <> (rls.LikeCount - rls.DislikeCount) OR
	(Resource_Index.likesSummary IS NULL AND (rls.LikeCount - rls.DislikeCount) IS NOT NULL)))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE likesSummary from [Resource.LikesSummary] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Likes and Dislikes
UPDATE Resource_Index
SET likeCount = rls.LikeCount,
	dislikeCount = rls.DislikeCount,
	LastUpdated = GETDATE()
FROM [Resource.LikesSummary] rls
WHERE Resource_Index.intID = rls.ResourceIntId AND
 ((Resource_Index.likeCount <> rls.LikeCount OR (Resource_Index.likeCount IS NULL AND rls.LikeCount IS NOT NULL)) OR
 (Resource_Index.dislikeCount <> rls.DislikeCount OR (Resource_Index.dislikeCount IS NULL AND rls.DislikeCount IS NOT NULL)))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE likeCount, dislikeCount from [Resource.LikesSummary] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
	
-- Library IDs
UPDATE Resource_Index
SET libraryIDs = rs.LibraryIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(LibraryIDs, LEN(LibraryIDs) - 1) AS LibraryIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,[IsleContent].[dbo].[Library.Section].LibraryId)+','
			FROM [IsleContent].[dbo].[Library.Resource] itbl
			INNER JOIN [IsleContent].[dbo].[Library.Section] 
				ON itbl.LibrarySectionId = [IsleContent].[dbo].[Library.Section].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (itbl.CreatedById IS NULL OR itbl.CreatedById <> 22)
			for xml path('')) LibraryIDs
		FROM [IsleContent].[dbo].[Library.Resource] tbl
		GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.libraryIDs <> rs.LibraryIDs OR
	(Resource_Index.libraryIDs IS NULL AND rs.LibraryIDs IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Library.Section] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Collection IDs
UPDATE Resource_Index
SET collectionIDs = rs.CollectionIDs,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(CollectionIDs, LEN(CollectionIDs) - 1) AS CollectionIDs
	FROM (SELECT ResourceIntId, (
		SELECT convert(nvarchar,itbl.[LibrarySectionId])+','
		FROM [IsleContent].[dbo].[Library.Resource] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) CollectionIDs
	FROM [IsleContent].[dbo].[Library.Resource] tbl
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.collectionIDs <> rs.CollectionIDs OR
	(Resource_Index.collectionIDs IS NULL AND rs.CollectionIDs IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Library.Resource] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Comments Count
UPDATE Resource_Index
SET commentsCount = rs.CommentsCount,
	LastUpdated = getdate()
FROM (SELECT ResourceIntId, COUNT(*) AS CommentsCount
	FROM [Resource.Comment]
	WHERE (CreatedById IS NULL OR CreatedById <> 22)
	GROUP BY ResourceIntId) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.commentsCount <> rs.CommentsCount OR
	(Resource_Index.commentsCount IS NULL AND rs.CommentsCount IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Resource.Comments] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- AssessmentTypeIDs
UPDATE Resource_Index
SET assessmentTypeIDs = rs.AssessmentTypeIDs,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(AssessmentTypeIDs, LEN(AssessmentTypeIDs) - 1) AS AssessmentTypeIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,itbl.[AssessmentTypeId])+','
			FROM [Resource.AssessmentType] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
			for xml path('')) AssessmentTypeIDs
		FROM [Resource.AssessmentType] tbl
		GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.assessmentTypeIDs <> rs.AssessmentTypeIDs OR
	(Resource_Index.assessmentTypeIDs IS NULL AND rs.AssessmentTypeIDs IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.AssessmentType] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- AssessmentTypes
UPDATE Resource_Index
SET assessmentTypes = rs.AssessmentTypes,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(AssessmentTypes, LEN(AssessmentTypes) - 1) AS AssessmentTypes
		FROM (SELECT ResourceIntId, (
			SELECT [Codes.AssessmentType].Title+','
			FROM [Resource.AssessmentType] itbl
			INNER JOIN [Codes.AssessmentType] ON itbl.AssessmentTypeId = [Codes.AssessmentType].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
			for xml path('')) AssessmentTypes
		FROM [Resource.AssessmentType] tbl
		GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.assessmentTypes <> rs.AssessmentTypes OR
	(Resource_Index.assessmentTypes IS NULL AND rs.AssessmentTypes IS NOT NULL))


SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.AssessmentType] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- DetailViews
UPDATE Resource_Index
SET detailViews = rs.DetailViews,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, COUNT(*) AS DetailViews
	FROM [Resource.DetailView]
	WHERE (CreatedById IS NULL OR CreatedById <> 22)
	GROUP BY ResourceIntId) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.detailViews <> rs.DetailViews OR
	(Resource_Index.detailViews IS NULL AND rs.DetailViews IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Resource.DetailView] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Favorites - commented out.  Relying only on [Resource].FavoriteCount
/* UPDATE Resource_Index
SET favorites = rs.Favorites,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, r.FavoriteCount + COUNT(*) AS Favorites
	FROM [IsleContent].[dbo].[Library.Resource] lr
	INNER JOIN [Resource] r ON lr.ResourceIntId = r.Id
	WHERE (CreatedById IS NULL OR CreatedById <> 22)
	GROUP BY ResourceIntId, FavoriteCount) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.detailViews <> rs.Favorites OR
	(Resource_Index.detailViews IS NULL AND rs.Favorites IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Library.Resource] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE() */

-- accessibilityApi IDs
UPDATE Resource_Index
SET accessibilityApiIDs = rs.relatedTags
FROM (SELECT ResourceIntId, left(accessibilityApiIDs, LEN(accessibilityApiIDs) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(nvarchar,itbl.[AccessibilityApiId])+','
		FROM [Resource.AccessibilityApi] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) accessibilityApiIDs
	FROM [Resource.AccessibilityApi] tbl
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.accessibilityApiIDs <> rs.relatedTags OR
	(Resource_Index.accessibilityApiIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.AccessibilityApi] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- accessibilityApis
UPDATE Resource_Index
SET accessibilityApis = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(accessibilityApi, LEN(accessibilityApi) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.AccessibilityApi].Title+','
		FROM [Resource.AccessibilityApi] itbl
		INNER JOIN [Codes.AccessibilityApi] ON itbl.AccessibilityApiId = [Codes.AccessibilityApi].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) accessibilityApi
	FROM [Resource.AccessibilityApi] tbl
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.accessibilityApis <> rs.relatedTags OR
	(Resource_Index.accessibilityApis IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.AccessibilityApi] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- accessibilityControl IDs
UPDATE Resource_Index
SET accessibilityControlIDs = rs.relatedTags
FROM (SELECT ResourceIntId, left(accessibilityControlIDs, LEN(accessibilityControlIDs) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(nvarchar,itbl.[AccessibilityControlId])+','
		FROM [Resource.AccessibilityControl] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) accessibilityControlIDs
	FROM [Resource.AccessibilityControl] tbl
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.accessibilityControlIDs <> rs.relatedTags OR
	(Resource_Index.accessibilityControlIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.AccessibilityControl] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- accessibilityControls
UPDATE Resource_Index
SET accessibilityControls = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(accessibilityControl, LEN(accessibilityControl) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.AccessibilityControl].Title+','
		FROM [Resource.AccessibilityControl] itbl
		INNER JOIN [Codes.AccessibilityControl] ON itbl.AccessibilityControlId = [Codes.AccessibilityControl].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) accessibilityControl
	FROM [Resource.AccessibilityControl] tbl
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.accessibilityControls <> rs.relatedTags OR
	(Resource_Index.accessibilityControls IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.AccessibilityControl] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- accessibilityFeature IDs
UPDATE Resource_Index
SET accessibilityFeatureIDs = rs.relatedTags
FROM (SELECT ResourceIntId, left(accessibilityFeatureIDs, LEN(accessibilityFeatureIDs) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(nvarchar,itbl.[AccessibilityFeatureId])+','
		FROM [Resource.AccessibilityFeature] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) accessibilityFeatureIDs
	FROM [Resource.AccessibilityFeature] tbl
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.accessibilityFeatureIDs <> rs.relatedTags OR
	(Resource_Index.accessibilityFeatureIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.AccessibilityFeature] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- accessibilityFeatures
UPDATE Resource_Index
SET accessibilityFeatures = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(accessibilityFeature, LEN(accessibilityFeature) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.AccessibilityFeature].Title+','
		FROM [Resource.AccessibilityFeature] itbl
		INNER JOIN [Codes.AccessibilityFeature] ON itbl.AccessibilityFeatureId = [Codes.AccessibilityFeature].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) accessibilityFeature
	FROM [Resource.AccessibilityFeature] tbl
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.accessibilityFeatures <> rs.relatedTags OR
	(Resource_Index.accessibilityFeatures IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.AccessibilityFeature] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- accessibilityHazard IDs
UPDATE Resource_Index
SET accessibilityHazardIDs = rs.relatedTags
FROM (SELECT ResourceIntId, left(accessibilityHazardIDs, LEN(accessibilityHazardIDs) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(nvarchar,itbl.[AccessibilityHazardId])+','
		FROM [Resource.AccessibilityHazard] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) accessibilityHazardIDs
	FROM [Resource.AccessibilityHazard] tbl
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.accessibilityHazardIDs <> rs.relatedTags OR
	(Resource_Index.accessibilityHazardIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.AccessibilityHazard] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- accessibilityHazards
UPDATE Resource_Index
SET accessibilityHazards = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(accessibilityHazard, LEN(accessibilityHazard) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.AccessibilityHazard].Title+','
		FROM [Resource.AccessibilityHazard] itbl
		INNER JOIN [Codes.AccessibilityHazard] ON itbl.AccessibilityHazardId = [Codes.AccessibilityHazard].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) accessibilityHazard
	FROM [Resource.AccessibilityHazard] tbl
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_Index.intID = rs.ResourceIntId AND
	(Resource_Index.accessibilityHazards <> rs.relatedTags OR
	(Resource_Index.accessibilityHazards IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.AccessibilityHazard] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- =======================================================================
SET @StartTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@InitStartTime,@FinishTime)
PRINT 'Total Time: ' + convert(varchar,@ElapsedTime)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource_IndexUpdateV2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
[dbo].[Resource_IndexUpdateV2]

*/
-- ======================================================================
-- Author:		Jerome Grimmer
-- Create date: 4/25/2013
-- Description:	Update Resource_IndexV2 table for ElasticSearch
--
-- 2013-07-08 jgrimmer - Remove resources where [Resource.Version].IsActive <> 'True'
-- 2013-08-20 jgrimmer - Added likeCount and dislikeCount
-- 2013-11-20 jgrimmer - Added check for IsDeleted in the LinkChecker database
-- 2014-02-03 jgrimmer - Modify favorites to rely on [Resource].FavoriteCount only.  It is detail/library page's responsibility to update that count.
--						 Also add Submitter.
-- 2014-05-19 jgrimmer - Exclude resources from Smarter Balance, since Illinois is a PARCC member and not a Smarter Balance member state, and remove 
--						 check for PrivateResource (docID is null)
-- ======================================================================
-- 2014-05-21 mparsons - create new version using Resource.Tag
-- 2014-05-28 mparsons - added sortTitle
-- 2014-08-14 jgrimmer - Allow submitter to be NULL.
-- 2014-08-08 mparsons - changed targetSiteIDs to be based on the relative CodeId, not the absolute id (not Resource.Site for now)
-- 2014-10-16 mparsons - added qualify - renaming??
-- 2014-10-29 mparsons - added wioaWorks (replaces wioaWorks - 23)
-- 2014-11-25 mparsons - added layoff assistance (30)
-- ======================================================================
CREATE PROCEDURE [dbo].[Resource_IndexUpdateV2]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @InitStartTime datetime, @StartTime datetime, @EndTime datetime, @ElapsedTime float, @FinishTime datetime, @categoryId int, @processName varchar(50)

SET @StartTime = GETDATE()
SET @InitStartTime = @StartTime

-- Remove inactive resources
DELETE
FROM [Resource_IndexV2]
WHERE intID IN (SELECT Id FROM [Resource] WHERE IsActive = 0) 
OR	versionID IN (SELECT Id FROM [Resource.Version] WHERE IsActive = 0) 
OR	intId IN (Select ResourceIntId FROM [Resource.Link] WHERE IsDeleted = 1)
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'Delete inactive resources = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()


-- Update existing resources Resource.Version data
UPDATE Resource_IndexV2
SET title = rv.Title, 
	[description] = rv.[Description],
	publisher = rv.Publisher,
	created = rv.Created,
	accessRights = rv.AccessRights,
	accessRightsID = rv.AccessRightsId,
	LastUpdated = GETDATE(),
	url = r.ResourceUrl,
	usageRightsURL = rv.Rights,
	usageRights = 
		CASE 
			WHEN cou.Id IS NOT NULL THEN cou.Summary
			WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'Read the Fine Print'
			ELSE NULL
		END,
	usageRightsID = 
		CASE 
			WHEN cou.Id IS NOT NULL THEN cou.Id
			WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 4  -- Read the Fine Print
			ELSE NULL
		END,
	usageRightsIconURL = 
		CASE 
			WHEN cou.Id IS NOT NULL THEN cou.IconUrl
			WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'http://mirrors.creativecommons.org/presskit/cc.primary.srr.gif'
			ELSE NULL
		END,
	usageRightsMiniIconURL = 
		CASE 
			WHEN cou.Id IS NOT NULL THEN cou.MiniIconUrl
			WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN '/images/icons/rightsreserved.png'
			ELSE NULL
		END,
	viewsCount = ViewCount,
	timeRequired = TypicalLearningTime,
	creator = rv.Creator,
	requirements = rv.Requirements,
	isBasedOnUrl = RelatedUrl,
	favorites = r.FavoriteCount,
	submitter = rv.Submitter,
	 [SortTitle] = [dbo].[BuildSortTitle] (rv.[Title])
FROM [Resource.Version] rv
INNER JOIN [Resource] r ON rv.ResourceIntId = r.Id
LEFT JOIN [ConditionOfUse] cou ON rv.Rights = cou.Url
LEFT JOIN [Resource.RelatedUrl] rru ON r.Id = rru.ResourceIntId
WHERE Resource_IndexV2.versionId = rv.Id AND
	(Resource_IndexV2.title <> rv.Title OR Resource_IndexV2.[description] <> rv.[Description] OR
	 Resource_IndexV2.publisher <> rv.Publisher OR  Resource_IndexV2.accessRights <> rv.AccessRights OR
	 Resource_IndexV2.accessRightsId <> rv.AccessRightsId OR Resource_IndexV2.url <> r.ResourceUrl OR
	 Resource_IndexV2.usageRightsURL <> rv.Rights OR viewsCount <> ViewCount OR
	 Resource_IndexV2.timeRequired <> rv.TypicalLearningTime OR Resource_IndexV2.creator <> rv.Creator OR
	 Resource_IndexV2.requirements <> rv.Requirements OR Resource_IndexV2.isBasedOnUrl <> rru.RelatedUrl OR
 	 Resource_IndexV2.favorites <> r.FavoriteCount OR
	 Resource_IndexV2.submitter <> rv.Submitter) AND
	 Resource_IndexV2.intID NOT IN (SELECT ResourceIntId FROM [Resource.Link] WHERE IsDeleted = 'True') AND
	 (rv.Submitter IS NULL OR rv.Submitter NOT LIKE '%smarterbalanced.org%' )

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Resource.Version] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Add new resources, skipping inactive and private resources
INSERT INTO Resource_IndexV2 (versionId, intId, title, [description], publisher,
	created, accessRights, accessRightsId, LastUpdated, url, usageRightsUrl, usageRights, usageRightsID, usageRightsIconURL, usageRightsMiniIconURL,
	viewsCount, timeRequired, creator, requirements, isBasedOnUrl, favorites, submitter)
SELECT DISTINCT rv.Id, rv.ResourceIntId, rv.Title, rv.[Description], rv.Publisher, rv.Created, rv.AccessRights, rv.AccessRightsId, GETDATE(), r.ResourceUrl,
	rv.Rights, 
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.Summary
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'Read the Fine Print'
		ELSE NULL
	END AS usageRights,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.Id
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 4 -- Read the Fine Print
		ELSE NULL
	END AS usageRightsId,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.IconUrl
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN 'http://mirrors.creativecommons.org/presskit/cc.primary.srr.gif'
		ELSE NULL
	END AS usageRightsIconURL,
	CASE 
		WHEN cou.Id IS NOT NULL THEN cou.MiniIconUrl
		WHEN rv.Rights IS NOT NULL AND cou.Id IS NULL THEN '/images/icons/rightsreserved.png'
		ELSE NULL
	END AS usageRightsMiniIconURL,
	ViewCount, TypicalLearningTime, Creator, Requirements, RelatedUrl, r.FavoriteCount, rv.Submitter
FROM [Resource.Version] rv
INNER JOIN [Resource] r ON rv.ResourceIntId = r.Id
LEFT JOIN [Resource.PublishedBy] rpb ON rv.ResourceIntId = rpb.ResourceIntId
LEFT JOIN [ConditionOfUse] cou ON rv.Rights = cou.Url
LEFT JOIN [Resource.RelatedUrl] rru ON r.Id = rru.ResourceIntId
WHERE rv.Id NOT IN 	(SELECT versionId FROM Resource_IndexV2) 
AND (cou.Id IS NULL OR cou.Id <> 3) 
AND r.IsActive = 1 
AND rv.IsActive = 1 
AND (rpb.PublishedById IS NULL OR rpb.PublishedById <> 22) 
AND	r.Id NOT IN (SELECT ResourceIntId FROM [Resource.Link] WHERE IsDeleted = 'True') 
AND	(rv.Submitter IS NULL OR rv.Submitter NOT LIKE '%smarterbalanced.org%' )


SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'INSERT from [Resource.Version] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Keywords
UPDATE Resource_IndexV2
SET keywords = rk.Keywords,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Keywords, LEN(Keywords) - 1) AS Keywords
	FROM (SELECT ResourceIntId, (
		SELECT isnull(itbl.[Keyword],'')+','
		FROM [Resource.Keyword] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Keywords
	FROM [Resource.Keyword] tbl
	GROUP BY ResourceIntId) otab) rk
WHERE Resource_IndexV2.intId = rk.ResourceIntId AND
	(Resource_IndexV2.keywords <> rk.Keywords OR 
	(Resource_IndexV2.keywords IS NULL AND rk.Keywords IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Resource.Keyword] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Subjects ???? how to merge k12 subject, and existing subjects
UPDATE Resource_IndexV2
SET [subjects] = rs.[Subjects],
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Subjects, LEN(Subjects) - 1) AS Subjects
	FROM (SELECT ResourceIntId, (
		SELECT itbl.[Subject]+','
		FROM [Resource.Subject] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Subjects
	FROM [Resource.Subject] tbl
	GROUP BY ResourceIntId) otab) rs 
WHERE Resource_IndexV2.intId = rs.ResourceIntId AND
	(Resource_IndexV2.subjects <> rs.Subjects OR
	(Resource_IndexV2.subjects IS NULL AND rs.Subjects IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Resource.Subject] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- k12Subject Ids
UPDATE Resource_IndexV2
SET k12SubjectIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 20 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 20
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.k12SubjectIDs <> rs.relatedTags OR
	(Resource_IndexV2.k12SubjectIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [k12Subject Ids] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- k12Subjects
UPDATE Resource_IndexV2
SET k12Subjects = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 20 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 20
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.k12Subjects <> rs.relatedTags OR
	(Resource_IndexV2.k12Subjects IS NULL AND rs.relatedTags IS NOT NULL))


SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [k12Subjects] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Language IDs
UPDATE Resource_IndexV2
SET [languageIDs] = rl1.[LanguageIds],
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Languages, LEN(Languages) - 1) AS LanguageIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 17 AND itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Languages
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 17
	GROUP BY ResourceIntId) otab) rl1
WHERE Resource_IndexV2.intId = rl1.ResourceIntId AND
	(Resource_IndexV2.languageIDs <> rl1.LanguageIds OR
	(Resource_IndexV2.languageIDs IS NULL AND rl1.LanguageIds IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.Language] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Languages
UPDATE Resource_IndexV2
SET [languages] = rl2.[Languages],
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Languages, LEN(Languages) - 1) AS Languages
	FROM (SELECT ResourceIntId, (
		SELECT itbl.TagTitle +','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 17 AND itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Languages
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 17
	GROUP BY ResourceIntId) otab) rl2
WHERE Resource_IndexV2.intId = rl2.ResourceIntId AND
	(Resource_IndexV2.languages <> rl2.Languages OR
	(Resource_IndexV2.languages IS NULL AND rl2.Languages IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.Language] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Career Cluster IDs
UPDATE Resource_IndexV2
SET clusterIDs = cc1.ClusterIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Clusters, LEN(Clusters) - 1) AS ClusterIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 8 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Clusters
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 8
	GROUP BY ResourceIntId) otab) cc1
WHERE Resource_IndexV2.intId = cc1.ResourceIntId AND
	(Resource_IndexV2.clusterIDs <> cc1.ClusterIds OR 
	(Resource_IndexV2.clusterIDs IS NULL AND cc1.ClusterIds IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.Cluster] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Clusters
UPDATE Resource_IndexV2
SET clusters = cc2.Clusters,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Clusters, LEN(Clusters) - 1) AS Clusters
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 8 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Clusters
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 8
	GROUP BY ResourceIntId) otab) cc2
WHERE Resource_IndexV2.intId = cc2.ResourceIntId AND
	(Resource_IndexV2.clusters <> cc2.Clusters OR
	(Resource_IndexV2.clusters IS NULL AND cc2.Clusters IS NOT NULL))


SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.Cluster] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Intended Audience Ids
UPDATE Resource_IndexV2
SET audienceIDs = ia1.AudienceIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Audiences, LEN(Audiences) - 1) AS AudienceIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 7 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Audiences
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 7
	GROUP BY ResourceIntId) otab) ia1
WHERE Resource_IndexV2.intId = ia1.ResourceIntId AND
	(Resource_IndexV2.audienceIDs <> ia1.AudienceIds OR
	(Resource_IndexV2.audienceIDs IS NULL AND ia1.AudienceIds IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.IntendedAudience] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Intended Audiences
UPDATE Resource_IndexV2
SET audiences = ia2.Audiences,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Audiences, LEN(Audiences) - 1) AS Audiences
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 7 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Audiences
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 7
	GROUP BY ResourceIntId) otab) ia2
WHERE Resource_IndexV2.intId = ia2.ResourceIntId AND
	(Resource_IndexV2.audiences <> ia2.Audiences OR
	(Resource_IndexV2.audiences IS NULL AND ia2.Audiences IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.IntendedAudience] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- gradeLevelIds
UPDATE Resource_IndexV2
SET gradeLevelIDs = el1.gradeLevelIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(gradeLevels, LEN(gradeLevels) - 1) AS gradeLevelIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 13 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) gradeLevels
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 13
	GROUP BY ResourceIntId) otab) el1
WHERE Resource_IndexV2.intId = el1.ResourceIntId AND
	(Resource_IndexV2.gradeLevelIds <> el1.gradeLevelIds OR
	(Resource_IndexV2.gradeLevelIds IS NULL AND el1.gradeLevelIds IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.GradeLevel] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- gradeLevels
UPDATE Resource_IndexV2
SET gradeLevels = el2.RelatedTags,
	LastUpdated = GETDATE()	
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS RelatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 13 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 13
	GROUP BY ResourceIntId) otab) el2
WHERE Resource_IndexV2.intId = el2.ResourceIntId AND
	(Resource_IndexV2.gradeLevels <> el2.RelatedTags OR
	(Resource_IndexV2.gradeLevels IS NULL AND el2.RelatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.GradeLevel] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- gradeLevelAliases		
UPDATE Resource_IndexV2
SET gradeLevelAliases = el2.RelatedTags,
	LastUpdated = GETDATE()	
FROM (SELECT ResourceIntId, 
		CASE 
			WHEN LEN(RelatedList) > 1 THEN replace(left(RelatedList, LEN(RelatedList) - 1),',,',',')
			ELSE NULL
		END AS RelatedTags
	FROM (SELECT ResourceIntId, (
		SELECT itbl.AliasValues + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 13 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 13
	GROUP BY ResourceIntId) otab) el2
WHERE Resource_IndexV2.intId = el2.ResourceIntId AND
	(Resource_IndexV2.gradeLevelAliases <> el2.RelatedTags OR
	(Resource_IndexV2.gradeLevelAliases IS NULL AND el2.RelatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #3 from [Resource.GradeLevel] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Resource Type Ids
UPDATE Resource_IndexV2
SET resourceTypeIDs = rt1.ResourceTypeIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(ResourceTypes, LEN(ResourceTypes) - 1) AS ResourceTypeIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 19 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) ResourceTypes
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 19
	GROUP BY ResourceIntId) otab) rt1
WHERE Resource_IndexV2.intId = rt1.ResourceIntId AND
	(Resource_IndexV2.resourceTypeIds <> rt1.ResourceTypeIds OR
	(Resource_IndexV2.resourceTypeIds IS NULL AND rt1.ResourceTypeIds IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.ResourceType] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Resource Types
UPDATE Resource_IndexV2
SET resourceTypes = rt2.ResourceTypes,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(ResourceTypes, LEN(ResourceTypes) - 1) AS ResourceTypes
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 19 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) ResourceTypes
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 19
	GROUP BY ResourceIntId) otab) rt2
WHERE Resource_IndexV2.intId = rt2.ResourceIntId AND
	(Resource_IndexV2.resourceTypes <> rt2.ResourceTypes OR
	(Resource_IndexV2.resourceTypes IS NULL AND rt2.ResourceTypes IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.ResourceType] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Resource Format Ids
UPDATE Resource_IndexV2
SET mediaTypeIDs = rf1.ResourceFormatIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(ResourceFormats, LEN(ResourceFormats) - 1) AS ResourceFormatIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 18 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) ResourceFormats
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 18
	GROUP BY ResourceIntId) otab) rf1
WHERE Resource_IndexV2.intId = rf1.ResourceIntId AND
	(Resource_IndexV2.mediaTypeIDs <> rf1.ResourceFormatIds OR
	(Resource_IndexV2.mediaTypeIDs IS NULL AND rf1.ResourceFormatIds IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.Format] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Resource Formats
UPDATE Resource_IndexV2
SET mediaTypes = rf2.ResourceFormats,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(ResourceFormats, LEN(ResourceFormats) - 1) AS ResourceFormats
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 18 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) ResourceFormats
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 18
	GROUP BY ResourceIntId) otab) rf2
WHERE Resource_IndexV2.intId = rf2.ResourceIntId AND
	(Resource_IndexV2.mediaTypes <> rf2.ResourceFormats OR
	(Resource_IndexV2.mediaTypes IS NULL AND rf2.ResourceFormats IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.Format] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Group Type Ids
UPDATE Resource_IndexV2
SET groupTypeIDs = gt1.GroupTypeIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(GroupTypes, LEN(GroupTypes) - 1) AS GroupTypeIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 14 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) GroupTypes
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 14
	GROUP BY ResourceIntId) otab) gt1
WHERE Resource_IndexV2.intId = gt1.ResourceIntId AND
	(Resource_IndexV2.groupTypeIds <> gt1.GroupTypeIds OR
	(Resource_IndexV2.groupTypeIds IS NULL AND gt1.GroupTypeIds IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.GroupTypes] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Group Types
UPDATE Resource_IndexV2
SET groupTypes = gt2.GroupTypes,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(GroupTypes, LEN(GroupTypes) - 1) AS GroupTypes
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 14 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) GroupTypes
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 14
	GROUP BY ResourceIntId) otab) gt2
WHERE Resource_IndexV2.intId = gt2.ResourceIntId AND
	(Resource_IndexV2.groupTypes <> gt2.GroupTypes OR
	(Resource_IndexV2.groupTypes IS NULL AND gt2.GroupTypes IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.GroupTypes] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Item Type Ids
UPDATE Resource_IndexV2
SET itemTypeIDs = it1.ItemTypeIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(ItemTypes, LEN(ItemTypes) - 1) AS ItemTypeIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 15 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) ItemTypes
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 15
	GROUP BY ResourceIntId) otab) it1
WHERE Resource_IndexV2.intId = it1.ResourceIntId AND
	(Resource_IndexV2.itemTypeIds <> it1.ItemTypeIds OR
	(Resource_IndexV2.itemTypeIds IS NULL AND it1.ItemTypeIds IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.ItemTypes] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
	
-- Item Types
UPDATE Resource_IndexV2
SET itemTypes = it2.ItemTypes,
	LastUpdated = getdate()
FROM (SELECT ResourceIntId, left(ItemTypes, LEN(ItemTypes) - 1) AS ItemTypes
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 15 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) ItemTypes
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 15
	GROUP BY ResourceIntId) otab) it2 
WHERE Resource_IndexV2.intId = it2.ResourceIntId AND
	(Resource_IndexV2.itemTypes <> it2.ItemTypes OR
	(Resource_IndexV2.itemTypes IS NULL AND it2.ItemTypes IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.ItemTypes] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Standard Ids
UPDATE Resource_IndexV2
SET standardIDs = rst1.StandardIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Standards, LEN(Standards) - 1) AS StandardIds
	FROM (SELECT ResourceIntId, (
		SELECT isnull(convert(varchar,itbl.[StandardId]),'')+','
		FROM [Resource.Standard] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Standards
	FROM [Resource.Standard] tbl
	GROUP BY ResourceIntId) otab) rst1
WHERE Resource_IndexV2.intId = rst1.ResourceIntId AND
	(Resource_IndexV2.StandardIDs <> rst1.StandardIds OR
	(Resource_IndexV2.StandardIDs IS NULL AND rst1.StandardIds IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.Standards] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Standards
UPDATE Resource_IndexV2
SET standardNotations = rst2.Standards,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(Standards, LEN(Standards) - 1) AS Standards
	FROM (SELECT ResourceIntId, (
		SELECT [StandardBody.Node].NotationCode+','
		FROM [Resource.Standard] itbl
		INNER JOIN [StandardBody.Node] ON itbl.StandardId = [StandardBody.Node].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) Standards
	FROM [Resource.Standard] tbl
	GROUP BY ResourceIntId) otab) rst2 
WHERE Resource_IndexV2.intId = rst2.ResourceIntId AND
	(Resource_IndexV2.standardNotations <> rst2.Standards OR
	(Resource_IndexV2.standardNotations IS NULL AND rst2.Standards IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
SET @FinishTime = @EndTime
PRINT 'UPDATE #2 from [Resource.Standards] = ' + convert(varchar,@ElapsedTime)

-- Alignment Type IDs - ???????????????????
UPDATE Resource_IndexV2
SET alignmentTypeIDs = rs.AlignmentTypeIDs,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(AlignmentTypeIDs, LEN(AlignmentTypeIDs) - 1) AS AlignmentTypeIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,isnull(itbl.[AlignmentTypeCodeId],2))+','
			FROM [Resource.Standard] itbl
			WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
			for xml path('')) AlignmentTypeIDs
		FROM [Resource.Standard] tbl
		GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND 
	(Resource_IndexV2.alignmentTypeIDs <> rs.AlignmentTypeIDs OR
	(Resource_IndexV2.alignmentTypeIDs IS NULL AND rs.AlignmentTypeIDs IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
SET @FinishTime = @EndTime
PRINT 'UPDATE Alignment Type IDs = ' + convert(varchar,@ElapsedTime)
	
-- Alignment Types		?????????????????????
UPDATE Resource_IndexV2
SET alignmentTypes = rs.AlignmentTypes,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(AlignmentTypes, LEN(AlignmentTypes) - 1) AS AlignmentTypes
		FROM (SELECT ResourceIntId, (
			SELECT isnull([Codes.AlignmentType].Title,'Teaches')+','
			FROM [Resource.Standard] itbl
			LEFT JOIN [Codes.AlignmentType] ON itbl.AlignmentTypeCodeId = [Codes.AlignmentType].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
			for xml path('')) AlignmentTypes
		FROM [Resource.Standard] tbl
		GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.alignmentTypes <> rs.AlignmentTypes OR
	(Resource_IndexV2.alignmentTypes IS NULL AND rs.AlignmentTypes IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
SET @FinishTime = @EndTime
PRINT 'UPDATE Alignment Types = ' + convert(varchar,@ElapsedTime)

-- EducationalUseIds
UPDATE Resource_IndexV2
SET educationalUseIDs = eu1.EducationalUseIds,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(EducationalUse, LEN(EducationalUse) - 1) AS EducationalUseIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 11 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) EducationalUse
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 11
	GROUP BY ResourceIntId) otab) eu1
WHERE Resource_IndexV2.intId = eu1.ResourceIntId AND
	(Resource_IndexV2.educationalUseIDs <> eu1.EducationalUseIds OR
	(Resource_IndexV2.educationalUseIDs IS NULL AND eu1.EducationalUseIds IS NOT NULL))
	
SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.EducationUse] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- EducationalUse
UPDATE Resource_IndexV2
SET educationalUses = eu2.EducationalUses,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(EducationalUse, LEN(EducationalUse) - 1) AS EducationalUses
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 11 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) EducationalUse
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 11
	GROUP BY ResourceIntId) otab) eu2 
WHERE Resource_IndexV2.intId = eu2.ResourceIntId AND
	(Resource_IndexV2.educationalUses <> eu2.EducationalUses OR
	(Resource_IndexV2.educationalUses IS NULL AND eu2.EducationalUses IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.EducationUse] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Likes minus Dislikes
UPDATE Resource_IndexV2
SET likesSummary = rls.LikeCount - rls.DislikeCount,
	LastUpdated = GETDATE()
FROM [Resource.LikesSummary] rls
WHERE Resource_IndexV2.intId = rls.ResourceIntId AND
	((Resource_IndexV2.likesSummary <> (rls.LikeCount - rls.DislikeCount) OR
	(Resource_IndexV2.likesSummary IS NULL AND (rls.LikeCount - rls.DislikeCount) IS NOT NULL)))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE likesSummary from [Resource.LikesSummary] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Likes and Dislikes
UPDATE Resource_IndexV2
SET likeCount = rls.LikeCount,
	dislikeCount = rls.DislikeCount,
	LastUpdated = GETDATE()
FROM [Resource.LikesSummary] rls
WHERE Resource_IndexV2.intID = rls.ResourceIntId AND
 ((Resource_IndexV2.likeCount <> rls.LikeCount OR (Resource_IndexV2.likeCount IS NULL AND rls.LikeCount IS NOT NULL)) OR
 (Resource_IndexV2.dislikeCount <> rls.DislikeCount OR (Resource_IndexV2.dislikeCount IS NULL AND rls.DislikeCount IS NOT NULL)))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE likeCount, dislikeCount from [Resource.LikesSummary] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
	
-- Library IDs
UPDATE Resource_IndexV2
SET libraryIDs = rs.LibraryIDs,
		LastUpdated = GETDATE()
	FROM (SELECT ResourceIntId, left(LibraryIDs, LEN(LibraryIDs) - 1) AS LibraryIDs
		FROM (SELECT ResourceIntId, (
			SELECT convert(nvarchar,[IsleContent].[dbo].[Library.Section].LibraryId)+','
			FROM [IsleContent].[dbo].[Library.Resource] itbl
			INNER JOIN [IsleContent].[dbo].[Library.Section] 
				ON itbl.LibrarySectionId = [IsleContent].[dbo].[Library.Section].Id
			WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (itbl.CreatedById IS NULL OR itbl.CreatedById <> 22)
			for xml path('')) LibraryIDs
		FROM [IsleContent].[dbo].[Library.Resource] tbl
		GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.libraryIDs <> rs.LibraryIDs OR
	(Resource_IndexV2.libraryIDs IS NULL AND rs.LibraryIDs IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Library.Section] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Collection IDs
UPDATE Resource_IndexV2
SET collectionIDs = rs.CollectionIDs,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(CollectionIDs, LEN(CollectionIDs) - 1) AS CollectionIDs
	FROM (SELECT ResourceIntId, (
		SELECT convert(nvarchar,itbl.[LibrarySectionId])+','
		FROM [IsleContent].[dbo].[Library.Resource] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) CollectionIDs
	FROM [IsleContent].[dbo].[Library.Resource] tbl
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.collectionIDs <> rs.CollectionIDs OR
	(Resource_IndexV2.collectionIDs IS NULL AND rs.CollectionIDs IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Library.Resource] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Comments Count
UPDATE Resource_IndexV2
SET commentsCount = rs.CommentsCount,
	LastUpdated = getdate()
FROM (SELECT ResourceIntId, COUNT(*) AS CommentsCount
	FROM [Resource.Comment]
	WHERE (CreatedById IS NULL OR CreatedById <> 22)
	GROUP BY ResourceIntId) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.commentsCount <> rs.CommentsCount OR
	(Resource_IndexV2.commentsCount IS NULL AND rs.CommentsCount IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Resource.Comments] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- AssessmentTypeIDs
UPDATE Resource_IndexV2
SET assessmentTypeIDs = rs.relatedIds
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 25 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 25
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.assessmentTypeIDs <> rs.relatedIds OR
	(Resource_IndexV2.assessmentTypeIDs IS NULL AND rs.relatedIds IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.AssessmentType] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- AssessmentTypes
UPDATE Resource_IndexV2
SET assessmentTypes = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 4 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 4
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.assessmentTypes <> rs.relatedTags OR
	(Resource_IndexV2.assessmentTypes IS NULL AND rs.relatedTags IS NOT NULL))


SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.AssessmentType] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- DetailViews
UPDATE Resource_IndexV2
SET detailViews = rs.DetailViews,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, COUNT(*) AS DetailViews
	FROM [Resource.DetailView]
	WHERE (CreatedById IS NULL OR CreatedById <> 22)
	GROUP BY ResourceIntId) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.detailViews <> rs.DetailViews OR
	(Resource_IndexV2.detailViews IS NULL AND rs.DetailViews IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Resource.DetailView] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- Favorites - commented out.  Relying only on [Resource].FavoriteCount
/* UPDATE Resource_IndexV2
SET favorites = rs.Favorites,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, r.FavoriteCount + COUNT(*) AS Favorites
	FROM [IsleContent].[dbo].[Library.Resource] lr
	INNER JOIN [Resource] r ON lr.ResourceIntId = r.Id
	WHERE (CreatedById IS NULL OR CreatedById <> 22)
	GROUP BY ResourceIntId, FavoriteCount) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.detailViews <> rs.Favorites OR
	(Resource_IndexV2.detailViews IS NULL AND rs.Favorites IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE from [Library.Resource] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE() */


-- accessibilityApi IDs
/*--====== why is the format different for these???
Missing:
WHERE Resource_IndexV2.intId = cc1.ResourceIntId AND
	(Resource_IndexV2.clusterIDs <> cc1.ClusterIds OR 
	(Resource_IndexV2.clusterIDs IS NULL AND cc1.ClusterIds IS NOT NULL))
also:
LastUpdated = GETDATE()

*/
UPDATE Resource_IndexV2
SET accessibilityApiIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 2 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 2
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.accessibilityApiIDs <> rs.relatedTags OR
	(Resource_IndexV2.accessibilityApiIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.AccessibilityApi] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- accessibilityApis
UPDATE Resource_IndexV2
SET accessibilityApis = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 2 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 2
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.accessibilityApiIDs <> rs.relatedTags OR
	(Resource_IndexV2.accessibilityApiIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.AccessibilityApi] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- accessibilityControl IDs
UPDATE Resource_IndexV2
SET accessibilityControlIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 3 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 3
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.accessibilityApiIDs <> rs.relatedTags OR
	(Resource_IndexV2.accessibilityApiIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.AccessibilityControl] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- accessibilityControls
UPDATE Resource_IndexV2
SET accessibilityControls = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 3 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 3
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.accessibilityApiIDs <> rs.relatedTags OR
	(Resource_IndexV2.accessibilityApiIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.AccessibilityControl] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- accessibilityFeature IDs
UPDATE Resource_IndexV2
SET accessibilityFeatureIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 4 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 4
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.accessibilityApiIDs <> rs.relatedTags OR
	(Resource_IndexV2.accessibilityApiIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.AccessibilityFeature] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- accessibilityFeatures
UPDATE Resource_IndexV2
SET accessibilityFeatures = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 4 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 4
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.accessibilityApiIDs <> rs.relatedTags OR
	(Resource_IndexV2.accessibilityApiIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.AccessibilityFeature] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- accessibilityHazard IDs
UPDATE Resource_IndexV2
SET accessibilityHazardIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 5 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 5
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.accessibilityApiIDs <> rs.relatedTags OR
	(Resource_IndexV2.accessibilityApiIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #1 from [Resource.AccessibilityHazard] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- accessibilityHazards
UPDATE Resource_IndexV2
SET accessibilityHazards = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = 5 AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = 5
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.accessibilityApiIDs <> rs.relatedTags OR
	(Resource_IndexV2.accessibilityApiIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT 'UPDATE #2 from [Resource.AccessibilityHazard] = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- =======================================================================
-- 9 - career planning IDs == to Training & Credentials
set @categoryId= 9
set @processName= '9 - Training & Credentials'
UPDATE Resource_IndexV2
SET trainingIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.trainingIDs <> rs.relatedTags OR
	(Resource_IndexV2.trainingIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #1 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- titles
UPDATE Resource_IndexV2
SET training = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.training <> rs.relatedTags OR
	(Resource_IndexV2.training IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #2 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- =======================================================================
-- Disability Topic IDs
set @categoryId= 10
set @processName= '10 - Disability Topic'
UPDATE Resource_IndexV2
SET disabilityTopicIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.disabilityTopicIDs <> rs.relatedTags OR
	(Resource_IndexV2.disabilityTopicIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #1 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- titles
UPDATE Resource_IndexV2
SET disabilityTopics = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.disabilityTopics <> rs.relatedTags OR
	(Resource_IndexV2.disabilityTopics IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #2 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- =======================================================================
-- Disability - Region IDs
set @categoryId= 26
set @processName= '26 - - Region IDs'
UPDATE Resource_IndexV2
SET regionIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.regionIDs <> rs.relatedTags OR
	(Resource_IndexV2.regionIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #1 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- titles
UPDATE Resource_IndexV2
SET regions = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.regions <> rs.relatedTags OR
	(Resource_IndexV2.regions IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #2 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()


-- =======================================================================
-- Employer Program IDs  == to Network & Connect
set @categoryId= 12
set @processName= '12 - Network & Connect'
UPDATE Resource_IndexV2
SET networkingIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.networkingIDs <> rs.relatedTags OR
	(Resource_IndexV2.networkingIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #1 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- titles
UPDATE Resource_IndexV2
SET networking = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.networking <> rs.relatedTags OR
	(Resource_IndexV2.networking IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #2 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()


-- =======================================================================
-- Job Preparation IDs		== to jobs
set @categoryId= 16
set @processName= '16 - Job Preparation'
UPDATE Resource_IndexV2
SET jobIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.jobIDs <> rs.relatedTags OR
	(Resource_IndexV2.jobIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #1 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- titles
UPDATE Resource_IndexV2
SET jobs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.jobs <> rs.relatedTags OR
	(Resource_IndexV2.jobs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #2 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()


-- =======================================================================
-- Veterans Service IDs		to Resources
set @categoryId= 21
set @processName= '21 - Resources'
UPDATE Resource_IndexV2
SET resourceIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.resourceIDs <> rs.relatedTags OR
	(Resource_IndexV2.resourceIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #1 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- titles
UPDATE Resource_IndexV2
SET resources = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.resources <> rs.relatedTags OR
	(Resource_IndexV2.resources IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #2 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- =======================================================================
-- Workforce Education Partner IDs
set @categoryId= 22
set @processName= '22 - Workforce Education Partner'
UPDATE Resource_IndexV2
SET wfePartnerIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.wfePartnerIDs <> rs.relatedTags OR
	(Resource_IndexV2.wfePartnerIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #1 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- titles
UPDATE Resource_IndexV2
SET wfePartners = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.wfePartners <> rs.relatedTags OR
	(Resource_IndexV2.wfePartners IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #2 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()


-- =======================================================================
-- wioaWorks (formerly workSupportServiceIDs
set @categoryId= 23
set @processName= '23 - wioaWorks'
UPDATE Resource_IndexV2
SET wioaWorksIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.wioaWorksIDs <> rs.relatedTags OR
	(Resource_IndexV2.wioaWorksIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #1 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- titles
UPDATE Resource_IndexV2
SET wioaWorks = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.wioaWorks <> rs.relatedTags OR
	(Resource_IndexV2.wioaWorks IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #2 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- =======================================================================
-- Workplace Skill IDs		=== Explore Careers
set @categoryId= 24
set @processName= '24 - explore'
UPDATE Resource_IndexV2
SET exploreIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.exploreIDs <> rs.relatedTags OR
	(Resource_IndexV2.exploreIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #1 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- titles
UPDATE Resource_IndexV2
SET explore = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.explore <> rs.relatedTags OR
	(Resource_IndexV2.explore IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #2 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- =======================================================================
-- Qualify for Jobs IDs
set @categoryId= 29
set @processName= '29 - Qualify'
UPDATE Resource_IndexV2
SET qualifyIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.qualifyIDs <> rs.relatedTags OR
	(Resource_IndexV2.qualifyIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #1 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- titles
UPDATE Resource_IndexV2
SET qualify = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.qualify <> rs.relatedTags OR
	(Resource_IndexV2.qualify IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #2 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()


-- =======================================================================
-- Qualify for Jobs IDs
set @categoryId= 30
set @processName= '30 - Layoff Assistance'
UPDATE Resource_IndexV2
SET layoffAssistIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.layoffAssistIDs <> rs.relatedTags OR
	(Resource_IndexV2.layoffAssistIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #1 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- titles
UPDATE Resource_IndexV2
SET layoffAssist = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.layoffAssist <> rs.relatedTags OR
	(Resource_IndexV2.layoffAssist IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #2 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()

-- =======================================================================
-- TargetSite IDs
-- - use codeId rather than TagValueId. 
-- - OR convert to use resource.site??
set @categoryId= 27
set @processName= '27 - TargetSite'
--UPDATE Resource_IndexV2
--SET targetSiteIDs = rs.relatedTags,
--	LastUpdated = GETDATE()
--FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
--	FROM (SELECT ResourceIntId, (
--		SELECT convert(varchar,itbl.TagValueId)+','
--		FROM [Resource_TagSummary] itbl
--		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
--		for xml path('')) RelatedList
--		FROM [Resource_TagSummary] tbl
--	WHERE tbl.CategoryId = @categoryId
--	GROUP BY ResourceIntId) otab) rs
--WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
--	(Resource_IndexV2.targetSiteIDs <> rs.relatedTags OR
--	(Resource_IndexV2.targetSiteIDs IS NULL AND rs.relatedTags IS NOT NULL))

UPDATE Resource_IndexV2
SET targetSiteIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.CodeId)+','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.targetSiteIDs <> rs.relatedTags OR
	(Resource_IndexV2.targetSiteIDs IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #1 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
		
-- titles
UPDATE Resource_IndexV2
SET targetSites = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + ','
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		AND (CreatedById IS NULL OR CreatedById <> 22)
		for xml path('')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.targetSites <> rs.relatedTags OR
	(Resource_IndexV2.targetSites IS NULL AND rs.relatedTags IS NOT NULL))

SET @EndTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@StartTime,@EndTime)
PRINT @processName + ' UPDATE #2 = ' + convert(varchar,@ElapsedTime)
SET @StartTime = GETDATE()
-- =======================================================================
SET @StartTime = GETDATE()
SET @ElapsedTime = DATEDIFF(s,@InitStartTime,@FinishTime)
PRINT 'Total Time: ' + convert(varchar,@ElapsedTime)
END

GO
/****** Object:  StoredProcedure [dbo].[Resource_IndexV3TagsUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--DROP TABLE [Resource_IndexV3Tags]

/*
CREATE TABLE [dbo].[Resource_IndexV3Tags] (
	ResourceIntId int,
	Titles varchar(MAX),
	IDs varchar(MAX),
	CategoryId int,
	CategoryTitle varchar(50),
	AliasValues varchar(MAX)
)
*/


CREATE PROCEDURE [dbo].[Resource_IndexV3TagsUpdate]
	@resourceID int
AS

--DECLARE @resourceID int = 0;
/*DROP TABLE [dbo].[Resource_IndexV3Tags]

CREATE TABLE [dbo].[Resource_IndexV3Tags] ( ResourceIntId int, Titles varchar(MAX), IDs varchar(MAX), CategoryId int, CategoryTitle varchar(50), AliasValues varchar(MAX) )
	SELECT DISTINCT 
	ResourceIntId,
	(
		SELECT
		TagTitle + ','
		FROM [Resource_TagSummary]
		WHERE ResourceIntId = res.ResourceIntId
		AND CategoryId = res.CategoryId
		FOR XML PATH('') 
	) Titles,
	(
		SELECT
		convert(varchar, TagValueId) + ','
		FROM [Resource_TagSummary]
		WHERE ResourceIntId = res.ResourceIntId
		AND CategoryId = res.CategoryId
		FOR XML PATH('')
	) IDs,
	CategoryId,
	CategoryTitle,
	COALESCE(AliasValues, '')
	FROM [Resource_TagSummary] res
	WHERE @resourceID = 0 OR ( @resourceID != 0 AND @resourceID = ResourceIntId )
	*/
	
	--Use MERGE to do Upsert
	MERGE INTO [dbo].[Resource_IndexV3Tags] existing
	USING (
		SELECT DISTINCT 
		ResourceIntId,
		(
			SELECT
			TagTitle + ','
			FROM [Resource_TagSummary]
			WHERE ResourceIntId = res.ResourceIntId
			AND CategoryId = res.CategoryId
			FOR XML PATH('') 
		) Titles,
		(
			SELECT
			convert(varchar, TagValueId) + ','
			FROM [Resource_TagSummary]
			WHERE ResourceIntId = res.ResourceIntId
			AND CategoryId = res.CategoryId
			FOR XML PATH('')
		) IDs,
		CategoryId,
		CategoryTitle,
		COALESCE( --Need to concatenate AliasValues but return empty string if null
		(
			SELECT
			AliasValues + ','
			FROM [Resource_TagSummary]
			WHERE ResourceIntId = res.ResourceIntId
			AND CategoryId = res.CategoryId
			FOR XML PATH('')
		), '') AS AliasValues
		
		FROM [Resource_TagSummary] res
		WHERE @resourceID = 0 OR ( @resourceID != 0 AND @resourceID = ResourceIntId )
	) result
	ON result.ResourceIntId = existing.ResourceIntId AND result.CategoryId = existing.CategoryId
	WHEN MATCHED THEN --Update if found
	UPDATE
		SET existing.ResourceIntId = result.ResourceIntId,
			existing.Titles = result.Titles,
			existing.IDs = result.IDs,
			existing.CategoryId = result.CategoryId,
			existing.CategoryTitle = result.CategoryTitle,
			existing.AliasValues = result.AliasValues
	WHEN NOT MATCHED THEN --Insert if not found
		INSERT (ResourceIntId, Titles, IDs, CategoryId, CategoryTitle, AliasValues) 
		VALUES (result.ResourceIntId, result.Titles, result.IDs, result.CategoryId, result.CategoryTitle, result.AliasValues)
	;

	--Return result set
	IF( @resourceID > 0 )
	SELECT * FROM [Resource_IndexV3Tags] WHERE ResourceIntId = @resourceID
	ELSE 
	SELECT * FROM [Resource_IndexV3Tags]


GO
/****** Object:  StoredProcedure [dbo].[Resource_IndexV3TextsUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Resource_IndexV3TextsUpdate]
	@resourceID int
AS

MERGE INTO [dbo].[Resource_IndexV3Texts] existing
USING (
	SELECT DISTINCT
	--Easy Stuff
	ver.[ResourceIntId],
	ver.[Id] AS [ResourceVersionId],
	ver.[DocId],
	ver.Title,
	ver.SortTitle AS UrlTitle,
	ver.[Description],
	ver.Requirements,
	res.ResourceUrl AS Url,
	ver.Created AS ResourceCreated,
	ver.Creator,
	ver.Publisher,
	ver.Submitter,
	ver.Rights AS RightsUrl,
	--Special Fields
	(
		SELECT
		Keyword + ','
		FROM [Resource.Keyword]
		WHERE ResourceIntId = ver.ResourceIntId
		FOR XML PATH('')
	) AS Keywords,
	(
		SELECT 
		convert(varchar, sec.LibraryId) + ','
		FROM [IsleContent].[dbo].[Library.Resource] lib
		INNER JOIN [IsleContent].[dbo].[Library.Section] sec
		ON lib.LibrarySectionId = sec.Id
		WHERE lib.ResourceIntId = ver.ResourceIntId
		FOR XML PATH('')
	) AS LibraryIds,
	(
		SELECT 
		convert(varchar, lib.LibrarySectionId) + ','
		FROM [IsleContent].[dbo].[Library.Resource] lib
		WHERE lib.ResourceIntId = ver.ResourceIntId
		FOR XML PATH('')
	) AS CollectionIds,
	(
		SELECT
		convert(varchar, StandardId) + ','
		FROM [Resource.Standard]
		WHERE ResourceIntId = ver.ResourceIntId
		FOR XML PATH('')
	) AS StandardIds,
	(
		SELECT
		NotationCode + ','
		FROM [Resource.Standard] resStd
		LEFT JOIN [StandardBody.Node] node
		ON resStd.StandardId = node.Id
		WHERE ResourceIntId = ver.ResourceIntId
		FOR XML PATH('')
	) AS StandardNotations,
	--Paradata
	res.FavoriteCount AS P_Favorites,
	COALESCE ((
		SELECT
		COUNT(*) 
		FROM [Resource.DetailView]
		WHERE ResourceIntId = ver.ResourceIntId
	), 0) AS P_ResourceViews,
	COALESCE ((
		SELECT
		LikeCount
		FROM [Resource.LikesSummary]
		WHERE ResourceIntId = ver.ResourceIntId
	), 0) AS P_Likes,
	COALESCE ((
		SELECT
		DislikeCount
		FROM [Resource.LikesSummary]
		WHERE ResourceIntId = ver.ResourceIntId
	), 0) AS P_Dislikes,
	COALESCE ((
		SELECT
		CASE
			WHEN DislikeCount = 0.0 AND LikeCount > 0.0 THEN 1.0 
			WHEN DislikeCount = 0.0 AND LikeCount = 0.0 THEN 0.0
			WHEN DislikeCount > 0.0 THEN ( LikeCount / DislikeCount )
		ELSE 0.0
		END AS Rating
		FROM [Resource.LikesSummary]
		WHERE ResourceIntId = ver.ResourceIntId
	), 0.0) AS P_Rating,
	COALESCE ((
		SELECT
		COUNT(*)
		FROM [Resource.Comment]
		WHERE ResourceIntId = ver.ResourceIntId
	), 0) AS P_Comments,
	COALESCE ((
		SELECT
		COUNT(*)
		FROM [Resource.RatingSummary]
		WHERE ResourceIntId = ver.ResourceIntId
	), 0) AS P_Evaluations,
	COALESCE ((
		SELECT
		AVG( ( [RatingAverage] / 4 ) )
		FROM [Resource.RatingSummary]
		WHERE ResourceIntId = ver.ResourceIntId
		AND RatingTotal > 0 AND RatingAverage >= 0.0
	), 0.0) AS P_EvaluationsScore
	FROM [Resource.Version] ver
	LEFT JOIN [Resource] res ON res.Id = ver.ResourceIntId
	WHERE ver.IsActive = 1 AND res.IsActive = 1
	AND @resourceID = 0 OR ( @resourceID != 0 AND @resourceID = ver.ResourceIntId )
) result
ON result.ResourceIntId = existing.ResourceIntId
WHEN MATCHED THEN --Update if found
UPDATE
	SET	existing.ResourceIntId = result.ResourceIntId,
		existing.ResourceVersionId = result.ResourceVersionId,
		existing.DocId = result.DocId,
		existing.Title = result.Title,
		existing.UrlTitle = result.UrlTitle,
		existing.[Description] = result.[Description],
		existing.Requirements = result.Requirements,
		existing.Url = result.Url,
		existing.ResourceCreated = result.ResourceCreated,
		existing.Creator = result.Creator,
		existing.Publisher = result.Publisher,
		existing.Submitter = result.Submitter,
		existing.RightsUrl = result.RightsUrl,
		existing.Keywords = result.Keywords,
		existing.LibraryIds = result.LibraryIds,
		existing.CollectionIds = result.CollectionIds,
		existing.StandardIds = result.StandardIds,
		existing.P_ResourceViews = result.P_ResourceViews,
		existing.P_Likes = result.P_Likes,
		existing.P_Dislikes = result.P_Dislikes,
		existing.P_Rating = result.P_Rating,
		existing.P_Comments = result.P_Comments,
		existing.P_Evaluations = result.P_Evaluations,
		existing.P_EvaluationsScore = result.P_EvaluationsScore
WHEN NOT MATCHED THEN --Insert if not found
	INSERT ( ResourceIntId, ResourceVersionId, DocId, Title, UrlTitle, [Description], Requirements, Url, ResourceCreated, Creator, Publisher, Submitter, RightsUrl, Keywords, LibraryIds, CollectionIds, StandardIds, P_Favorites, P_ResourceViews, P_Likes, P_Dislikes, P_Rating, P_Comments, P_Evaluations, P_EvaluationsScore)
	VALUES ( result.ResourceIntId, result.ResourceVersionId, result.DocId, result.Title, result.UrlTitle, result.[Description], result.Requirements, result.Url, result.ResourceCreated, result.Creator, result.Publisher, result.Submitter, result.RightsUrl, result.Keywords, result.LibraryIds, result.CollectionIds, result.StandardIds, result.P_Favorites, result.P_ResourceViews, result.P_Likes, result.P_Dislikes, result.P_Rating, result.P_Comments, result.P_Evaluations, result.P_EvaluationsScore)
;

--Return results
IF( @resourceID > 0)
	SELECT * FROM [Resource_IndexV3Texts] WHERE ResourceIntId = @resourceID
ELSE
	SELECT * FROM [Resource_IndexV3Texts]
/*
SELECT DISTINCT
--Easy Stuff
ver.[ResourceIntId],
ver.[Id] AS [ResourceVersionId],
ver.[DocId],
ver.Title,
ver.SortTitle AS UrlTitle,
ver.[Description],
ver.Requirements,
res.ResourceUrl AS Url,
ver.Created AS ResourceCreated,
ver.Creator,
ver.Publisher,
ver.Submitter,
ver.Rights AS RightsUrl,
--Special Fields
(
	SELECT
	Keyword + ','
	FROM [Resource.Keyword]
	WHERE ResourceIntId = ver.ResourceIntId
	FOR XML PATH('')
) AS Keywords,
(
	SELECT 
	convert(varchar, sec.LibraryId) + ','
	FROM [IsleContent].[dbo].[Library.Resource] lib
	INNER JOIN [IsleContent].[dbo].[Library.Section] sec
	ON lib.LibrarySectionId = sec.Id
	WHERE lib.ResourceIntId = ver.ResourceIntId
	FOR XML PATH('')
) AS LibraryIds,
(
	SELECT 
	convert(varchar, lib.LibrarySectionId) + ','
	FROM [IsleContent].[dbo].[Library.Resource] lib
	WHERE lib.ResourceIntId = ver.ResourceIntId
	FOR XML PATH('')
) AS CollectionIds,
(
	SELECT
	convert(varchar, StandardId) + ','
	FROM [Resource.Standard]
	WHERE ResourceIntId = ver.resourceIntId
	FOR XML PATH('')
) AS StandardIds,
--Paradata
res.FavoriteCount AS P_Favorites,
COALESCE ((
	SELECT
	COUNT(*) 
	FROM [Resource.DetailView]
	WHERE ResourceIntId = ver.ResourceIntId
), 0) AS P_ResourceViews,
COALESCE ((
	SELECT
	LikeCount
	FROM [Resource.LikesSummary]
	WHERE ResourceIntId = ver.ResourceIntId
), 0) AS P_Likes,
COALESCE ((
	SELECT
	DislikeCount
	FROM [Resource.LikesSummary]
	WHERE ResourceIntId = ver.ResourceIntId
), 0) AS P_Dislikes,
COALESCE ((
	SELECT
	CASE
		WHEN DislikeCount = 0.0 AND LikeCount > 0.0 THEN 1.0 
		WHEN DislikeCount = 0.0 AND LikeCount = 0.0 THEN 0.0
		WHEN DislikeCount > 0.0 THEN ( LikeCount / DislikeCount )
	ELSE 0.0
	END AS Rating
	FROM [Resource.LikesSummary]
	WHERE ResourceIntId = ver.ResourceIntId
), 0.0) AS P_Rating,
COALESCE ((
	SELECT
	COUNT(*)
	FROM [Resource.Comment]
	WHERE ResourceIntId = ver.ResourceIntId
), 0) AS P_Comments,
COALESCE ((
	SELECT
	COUNT(*)
	FROM [Resource.RatingSummary]
	WHERE ResourceIntId = ver.ResourceIntId
), 0) AS P_Evaluations,
COALESCE ((
	SELECT
	AVG( ( [RatingAverage] / 4 ) )
	FROM [Resource.RatingSummary]
	WHERE ResourceIntId = ver.ResourceIntId
	AND RatingTotal > 0 AND RatingAverage >= 0.0
), 0.0) AS P_EvaluationsScore
FROM [Resource.Version] ver
LEFT JOIN [Resource] res ON res.Id = ver.ResourceIntId
WHERE ver.IsActive = 1
AND @resourceID = 0 OR ( @resourceID != 0 AND @resourceID = ver.ResourceIntId )
*/

--DROP TABLE [Resource_IndexV3Texts]

/*
CREATE TABLE [dbo].[Resource_IndexV3Texts] (
	ResourceIntId int,
	ResourceVersionId int,
	DocId varchar(MAX),
	Title varchar(MAX),
	UrlTitle varchar(MAX),
	Description varchar(MAX),
	Requirements varchar(MAX),
	Url varchar(MAX),
	ResourceCreated datetime,
	Creator varchar(MAX),
	Publisher varchar(MAX),
	Submitter varchar(MAX),
	RightsUrl varchar(MAX),
	Keywords varchar(MAX),
	LibraryIds varchar(MAX),
	CollectionIds varchar(MAX),
	StandardIds varchar(MAX),
	StandardNotations varchar(MAX),
	P_Favorites int,
	P_ResourceViews int,
	P_Likes int,
	P_Dislikes int,
	P_Rating float,
	P_Comments int,
	P_Evaluations int,
	P_EvaluationsScore float
)
*/

GO
/****** Object:  StoredProcedure [dbo].[Resource_Search]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
select top 500 * from Resource

SELECT distinct lr.ResourceId, lr.ResourceVersionId, lr.Title  
        FROM [dbo].[Resource.Version_Summary] lr
        Left JOIN dbo.[Resource.ResourceType] rrt ON lr.ResourceId = rrt.ResourceId 
        Left JOIN [Resource.EducationLevelsSummary] edList ON lr.ResourceId = edList.ResourceId   
        where  ( lr.[MegaSearchField] like '%image%' )   Order by lr.Title 

SELECT COUNT(*) FROM [dbo].[Resource.Version_Summary]

SELECT COUNT(RowId) FROM [dbo].[Resource]

select top 1000 * from [dbo].[Resource.Version_Summary]




set @Filter = ' where ( (lr.Title like ''%Energy%'' OR lr.[Description] like ''%Energy%'' OR lrp.Value like ''%Energy%'') OR (lr.Title like ''%Finance%'' OR lr.[Description] like ''%Finance%'' OR lrp.Value like ''%Finance%'') OR (lr.Title like ''%Health Science%'' OR lr.[Description] like ''%Health Science%'' OR lrp.Value like ''%Health Science%'') ) AND (lrp.Value in (''high school'',''Higher Education'')) '

2 sec
-- ===========================================================
set @Filter = ' where lr.Title like ''%manu%'' OR lr.Description like ''%manu%''  '

--set @Filter = ' where ((lr.Title like ''%Energy%'' OR [Description] like ''%Energy%'' OR lrp.Value like ''%Energy%'') OR (lr.Title like ''%Finance%'' OR [Description] like ''%Finance%'' OR lrp.Value like ''%Finance%''))  '

--set @Filter = ' where ((lr.Title like ''%Energy%'' OR [Description] like ''%Energy%'' OR lrp.Value like ''%Energy%'') OR (lr.Title like ''%Finance%'' OR [Description] like ''%Finance%'' OR lrp.Value like ''%Finance%''))  '

--set @Filter = @Filter + ' AND (edList.Level in (''secondary'')) '


set @Filter = ' where  ( (lr.Title like ''%Finance%'' OR lr.[Description] like ''%Finance%'' OR lrs.SubjectCsv like ''%Finance%'') OR (lr.Title like ''%Manufacturing%'' OR lr.[Description] like ''%Manufacturing%'' OR lrs.SubjectCsv like ''%Manufacturing%'')   ) '

 AND (edList.PathwaysEducationLevelId in (3,4,5,6))

AND (edList.PathwaysEducationLevelId in (''4'',''5'',''6'') ) 

lr.Title like ''%www.oercommons%'' OR lr.[Description] like ''%www.oercommons%'' OR lr.[Publisher] like ''%www.oercommons%'' OR lr.[ResourceUrl] like ''%www.oercommons%'' OR lrp.Value like ''%www.oercommons%'') 

set @Filter = ' where  ( lr.MegaSearchField like ''%194.95.207.89%'' 
OR lr.[MegaSearchField] like ''%image%'' ) ' 
set @Filter = ' where  ( (lr.Title like ''%Finance%''
OR lrs.SubjectsIdx like ''%Finance%'' OR lrkey.KeywordsIdx like ''%Finance%'') 
OR (lr.Title like ''%Manufacturing%'' 
OR lrs.SubjectsIdx like ''%Manufacturing%''  OR lrkey.KeywordsIdx like ''%Manufacturing%'')   ) '
-- or lrkey.OriginalValue like ''%Manufacturing%'')  

set @Filter = ' Where (edList.PathwaysEducationLevelId in (3,4,5,6)) ' 

set @Filter = @Filter + ' AND (lr.Creator = ''serc.carleton.edu'' ) ' 



--set @Filter = @Filter + ' OR FREETEXT(lrs.SubjectCsv, ''physic'') '
set @Filter = @Filter + ' OR lrs.SubjectsIdx like ''%physic%'' '

set @Filter = ' where  (lr.Title like ''a %'' OR lr.[Description] like ''a %'' OR lr.[ResourceUrl] like ''a %'' OR lrs.Subject like ''a %'' OR lr.Keywords like ''a %'') '
-- OR lr.[Description] like ''%Finance%'' OR lr.[Description] like ''%Manufacturing%'' 
 

--=====================================================

DECLARE @RC int,@SortOrder varchar(100),@Filter varchar(5000)
DECLARE @StartPageIndex int, @PageSize int, @TotalRows int, @OutputRelTables bit
--
set @SortOrder = 'ResourceVersionIntId DESC'

-- blind search 
set @Filter = ''
set @Filter = ' Where (edList.PathwaysEducationLevelId in (3,4,5,6)) ' 

set @Filter = ' where (lr.Title like ''%agriculture%''  
              OR lr.ResourceUrl like ''%uri_hu%''   
              OR lr.Description like ''%agriculture%'') '
                       
set @Filter = ' where ( lr.id in (select ResourceIntId from [Resource.Language] where LanguageId in (1)) ) AND ( (lr.AccessRightsId in (2,4)) ) AND ( lr.id in (select ResourceIntId from [Resource.Cluster] where ClusterId in (8,11)) )  '

set @filter = ' where   ( lr.id in (select ResourceIntId from [Resource.Language] where LanguageId in (1)) ) AND ( (lr.AccessRightsId in (2,4)) ) AND ( lr.id in (select ResourceIntId from [Resource.GradeLevel] where GradeLevelId in (4,5,6,7)) )  '
set @Filter = ' Where  (lr.[ResourceIntId] = 170347 ) ' 
					
					
set @Filter = ' (lr.[ResourceUrl] = ''http://news.nationalgeographic.com/news/2002/08/0801_020731_frontiers.html'') '

set @OutputRelTables = 0
set @StartPageIndex = 1
set @PageSize = 55
set statistics time on       
EXECUTE @RC = Resource_Search
     @Filter,@SortOrder  ,@StartPageIndex  ,@PageSize, @OutputRelTables  ,@TotalRows OUTPUT

select 'total rows = ' + convert(varchar,@TotalRows)

set statistics time off       


*/

/* =============================================
      Description:      Resource search
  Uses custom paging to only return enough rows for one page
     Options:

  @StartPageIndex - starting page number. If interface is at 20 when next
page is requested, this would be set to 21?
  @PageSize - number of records on a page
  @TotalRows OUTPUT - total available rows. Used by interface to build a
custom pager
  ------------------------------------------------------
Modifications
12-06-11 mparsons - new
12-09-05 mparsons - convert to use Resource.Version_Summary
12-10-02 mparsons - tuning
12-10-12 mparsons - add code to minimize impact of blind searches. Use TOP 1000 if no custom filter
12-10-23 jgrimmer - Modified to use new SortTitle field instead of Title field as the default sort field.
13-01-31 mparsons - subjects are no longer returned, so removed join and column
13-03-05 mparsons - EducationLevels,AudienceList are no longer returned, so removed join and column
=============================================

*/

CREATE PROCEDURE [dbo].[Resource_Search]
		@Filter           nvarchar(4000)
		,@SortOrder       varchar(100)
		,@StartPageIndex  int
		,@PageSize        int
		,@OutputRelTables bit
		,@TotalRows       int OUTPUT

As

SET NOCOUNT ON;
-- paging
DECLARE
      @first_id               int
      ,@startRow        int
      ,@minRows         int
      ,@debugLevel      int
      ,@SQL             nvarchar(4000)
      ,@TopClause       varchar(100)
      ,@DefaultFilter   nvarchar(1000)
      ,@OrderBy         varchar(100)
      ,@UsingCodeTableValues bit

-- =================================

Set @UsingCodeTableValues = 0
Set @debugLevel = 4
Set @TopClause= ''
if @StartPageIndex < 1        set @StartPageIndex = 1

--actually the following is implied in the view (and IsPathwayLevel)
Set @DefaultFilter= '  where ( lr.id in (select ResourceIntId from [Resource.Language] where LanguageId = 1) ) ' 
if len(@SortOrder) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by lr.SortTitle '

--===================================================
-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1
SET @minRows =  @StartPageIndex + @PageSize

if len(isnull(@Filter,'')) = 0 begin
   --Set @TopClause= ' TOP 25000 '
   --MP - actually, just use code table directly!!
   --Set @OutputRelTables = 0 
   Set @UsingCodeTableValues = 1
   end
else if @minRows < 1000   begin 
  set @TopClause= ' TOP 25000 '   
  end   
  
-- =================================
CREATE TABLE #tempWorkTable(
      RowNumber         int PRIMARY KEY IDENTITY(1,1) NOT NULL,
      ResourceIntId    int,
      ResourceVersionIntId int,
      Title             varchar(200)
)

-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))
-- Left JOIN dbo.[Resource.Property] lrp ON lr.ResourceId = lrp.ResourceId 
--Left JOIN [Resource.EducationLevelsSummary] edList ON lr.ResourceId = edList.ResourceId 
 
  --set @SQL = 'SELECT distinct ' + @TopClause + ' lr.ResourceId, lr.ResourceVersionIntId, lr.SortTitle  
  --      FROM [dbo].[Resource.Version_Summary] lr
  --      Left JOIN [Resource.Cluster] rcl ON lr.ResourceId = rcl.ResourceId 
  --      Left JOIN dbo.[Resource.Subject] lrs ON lr.ResourceId = lrs.ResourceId 
  --      Left JOIN dbo.[Resource.Language] lrl ON lr.ResourceId = lrl.ResourceId 
  --      Left JOIN dbo.[Resource.Keyword] lrkey ON lr.ResourceId = lrkey.ResourceId 
  --      Left JOIN dbo.[Resource.ResourceType] rrt ON lr.ResourceId = rrt.ResourceId 
  --      Left JOIN [Resource.EducationLevel] edList ON lr.ResourceId = edList.ResourceId  '
  --      + @Filter

  --skip keyword for now
  --        Left JOIN dbo.[Resource.Keyword] lrkey ON lr.ResourceId = lrkey.ResourceId
  set @SQL = 'SELECT distinct ' + @TopClause + ' lr.Id, lr.ResourceVersionIntId, lr.SortTitle  
        FROM [dbo].[Resource.Version_Summary] lr
          Left JOIN dbo.[Resource.Subject] lrs ON lr.Id = lrs.ResourceIntId  '
        + @Filter
        
  if charindex( 'order by', lower(@Filter) ) = 0
    set @SQL = @SQL + ' ' + @OrderBy

  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL

  INSERT INTO #tempWorkTable (ResourceIntId, ResourceVersionIntId, Title)
  exec (@SQL)
  SELECT @TotalRows = @@ROWCOUNT
-- =================================
print '==== added to temp table: ' + convert(varchar,@TotalRows)
if @debugLevel > 7 begin
  select * from #tempWorkTable
  end

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
--SELECT Distinct TOP (@PageSize)
SELECT 
--Distinct    
    RowNumber,
    '' As ResourceId, 
    lr.ResourceIntId,
    lr.ResourceVersionIntId As Id,
    lr.ResourceVersionIntId,
    lr.ResourceUrl,
	lr.ResourceVersionIntId As ResourceVersionId,
    DocId,
    CASE
      WHEN lr.Title IS NULL THEN 'No Title'
      WHEN len(lr.Title) = 0 THEN 'No Title'
      ELSE lr.Title
    END AS Title,
    lr.SortTitle,
    [Description],
    isnull(Publisher,'Unknown') As Publisher,
    rtrim(ltrim(isnull(Creator,''))) As Creator,
    lr.ViewCount,
    lr.FavoriteCount,

    '' As Subjects,
    --edList.EducationLevels,    
   '' As EducationLevels,
    Rights,
	InteractivityType, InteractivityTypeId,
	[Schema], Requirements,
	[Submitter],[TypicalLearningTime],
    AccessRights, AccessRightsId
	,lr.Modified
	,lr.Created
    ,lr.Imported
		--,isnull(audienceList.AudienceList,'') As AudienceList
		,'' As AudienceList
		,[LikeCount],[DislikeCount]
From #tempWorkTable
    Inner join [dbo].[Resource.Version_Summary] lr on #tempWorkTable.ResourceIntId = lr.ResourceIntId
    left Join [dbo].[Resource.LikesSummary] rlike on lr.ResourceIntId = rlike.ResourceIntId

WHERE RowNumber > @first_id
order by RowNumber 


GO
/****** Object:  StoredProcedure [dbo].[Resource_Search_FT]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
SELECT distinct lr.RowId, lr.Title  FROM dbo.Resource lr
        Left JOIN dbo.[Resource.Property] lrp ON lr.RowId = lrp.ResourceId 
        Left JOIN dbo.[Resource.ResourceType] rrt ON lr.RowId = rrt.ResourceId 
        Left JOIN [Resource.EducationLevelsSummary] edList ON lr.RowId = edList.ResourceId   
        where  (FREETEXT(lr.[Description], ' Manufacturing Research and Development ') or FREETEXT(lr.Title, ' Manufacturing Research and Development ') ) 
        UNION SELECT distinct lr.RowId, lr.Title  FROM dbo.Resource lr
        Left JOIN dbo.[Resource.Property] lrp ON lr.RowId = lrp.ResourceId 
        Left JOIN dbo.[Resource.ResourceType] rrt ON lr.RowId = rrt.ResourceId 
        Left JOIN [Resource.EducationLevelsSummary] edList ON lr.RowId = edList.ResourceId   
        where  (lrp.PropertyTypeId = 6 and FREETEXT(lrp.Value, ' Manufacturing Research and Development ') )   
        Order by lr.Title 

        
2 sec
-- ===========================================================

DECLARE @RC int,@SortOrder varchar(100),@Filter varchar(5000),@Keywords        varchar(1000)
DECLARE @StartPageIndex int, @PageSize int, @totalRows int, @OutputRelTables bit
--
set @Filter = ''
set @SortOrder = 'lr.ResourceVersionIntId'
set @Keywords =  ''

set @Filter = ' where (lr.AccessRightsId in (2,4))  '
--set @Filter = ''
set @Keywords =  ' algebra, math, integration '

--set @Filter =  ' ResourceUrl = ''http://phet.colorado.edu/en/simulation/battery-resistor-circuit'' '

set @OutputRelTables = 0 
set @StartPageIndex = 1
set @PageSize = 25
set statistics time on
EXECUTE @RC = Resource_Search_FT
     @Filter, @Keywords, @SortOrder, @StartPageIndex  ,@PageSize, @OutputRelTables  ,@totalRows OUTPUT

select 'total rows = ' + convert(varchar,@totalRows)
set statistics time off

*/

/* =============================================
  Description:      Resource search using full text catalog
  Uses custom paging to only return enough rows for one page
     Options:

  @StartPageIndex - starting page number. If interface is at 20 when next
page is requested, this would be set to 21?
  @PageSize - number of records on a page
  @totalRows OUTPUT - total available rows. Used by interface to build a
custom pager
  ------------------------------------------------------
Modifications
12-07-30 mparsons - new, now uses full text catalogs
12-09-05 mparsons - convert to use Resource.Version_Summary
12-10-23 jgrimmer - modified to use SortTitle instead of Title as the default sort field.
=============================================

*/

CREATE PROCEDURE [dbo].[Resource_Search_FT]
	@Filter           nvarchar(4000)
	,@Keywords        nvarchar(1000)
	,@SortOrder       varchar(100)
	,@StartPageIndex  int
	,@PageSize        int
	,@OutputRelTables bit
	,@TotalRows       int OUTPUT

As

SET NOCOUNT ON;
-- paging
DECLARE
      @first_id               int
      ,@startRow        int
      ,@minRows         int
      ,@debugLevel      int
      ,@FTFilter1        nvarchar(4000)
      ,@FTFilter2        nvarchar(4000)
      ,@UsingKeywordFilter bit
      ,@FTFilter3        nvarchar(4000)
      ,@SQL             varchar(8000)
      ,@TopClause       varchar(100)
      ,@OrderBy         varchar(100)
      ,@UsingCodeTableValues bit
      ,@And             varchar(15)
-- =================================

Set @UsingCodeTableValues = 0
Set @debugLevel = 4
Set @TopClause= ''
Set @FTFilter2= ''
set @UsingKeywordFilter= 1
Set @FTFilter3= ''
if @StartPageIndex < 1        set @StartPageIndex = 1

if len(@SortOrder) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by lr.SortTitle '

--===================================================
-- Calculate the range
--===================================================
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1
SET @minRows =  @StartPageIndex + @PageSize

if len(isnull(@Filter,'')) = 0 begin
   --Set @TopClause= ' TOP 25000 '
   --MP - actually, just use code table directly!!
   --Set @OutputRelTables = 0 
   Set @UsingCodeTableValues = 1
   end
else if @minRows < 1000   begin 
  set @TopClause= ' TOP 25000 '   
  end  
-- =================================

CREATE TABLE #tempWorkTable(
      RowNumber         int PRIMARY KEY IDENTITY(1,1) NOT NULL,
      ResourceIntId    int,
      ResourceVersionIntId int,
      Title             varchar(200)
)
-- =================================

  if len(@Filter) > 0 begin
     set @And= ' AND '
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end
  else if len(@Keywords) > 0 begin
    set @Filter = ' where '
    set @And= ''
    end
  print '@Filter len: '  +  convert(varchar,len(@Filter))
       
       --     or FREETEXT(lrkey.KeywordsIdx, ''' + @Keywords + ''') 
       --     or FREETEXT(lr.Publisher, ''' + @Keywords + ''')
            --or FREETEXT(lr.Description, ''' + @Keywords + ''')  
                -- or FREETEXT(lrkey.OriginalValue, ''' + @Keywords + ''') 
                    -- or FREETEXT(lrs.Subject, ''' + @Keywords + ''') 
     if len(@Keywords) > 0 begin
       set @FTFilter1 = @And + 
       ' (FREETEXT(lr.[Title], ''' + @Keywords + ''') 
       or FREETEXT(lr.ResourceUrl, ''' + @Keywords + ''')
       or FREETEXT(lr.Description, ''' + @Keywords + ''') 
       ) '
       
  --print '@FTFilter1: '  +  @FTFilter1
       set @FTFilter2 = @And + 
         ' lr.ResourceIntid in (select ResourceIntid from dbo.[Resource.Subject] 
    where FREETEXT(Subject, ''' + @Keywords + ''') )'
    
  --print '@FTFilter2: '  +  @FTFilter2


      set @SQL = 'SELECT distinct ' + @TopClause + ' lr.Id, lr.ResourceVersionIntId, lr.SortTitle  
        FROM [dbo].[Resource.Version_Summary] lr '
        + @Filter
        + @FTFilter1

      set @SQL = @SQL + 'UNION SELECT distinct ' + @TopClause + ' lr.Id, lr.ResourceVersionIntId, lr.SortTitle  
        FROM [dbo].[Resource.Version_Summary] lr '
        + @Filter
        + @FTFilter2  

      if @UsingKeywordFilter = 1 begin
        set @FTFilter3 = @And + 
         ' lr.ResourceIntid in (select ResourceIntid from dbo.[Resource.Keyword] 
        where FREETEXT(Keyword, ''' + @Keywords + ''') )'

       -- print '@FTFilter3: '  +  @FTFilter3
        set @SQL = @SQL + 'UNION SELECT distinct ' + @TopClause + ' lr.Id, lr.ResourceVersionIntId, lr.SortTitle  
            FROM [dbo].[Resource.Version_Summary] lr '
            + @Filter
            + @FTFilter3   
        end
       
     end
else begin
--  Left JOIN dbo.[Resource.Subject] lrs ON lr.Id = lrs.ResourceIntId
  set @SQL = 'SELECT distinct ' + @TopClause + ' lr.Id, lr.ResourceVersionIntId, lr.SortTitle  
        FROM [dbo].[Resource.Version_Summary] lr
          '
        + @Filter
	end     

--         

  if charindex( 'order by', lower(@Filter) ) = 0
    set @SQL = @SQL + ' ' + @OrderBy

  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL

  INSERT INTO #tempWorkTable (ResourceIntId, ResourceVersionIntId, Title)
  exec (@SQL)
  SELECT @totalRows = @@ROWCOUNT
-- =================================
print 'added to temp table: ' + convert(varchar,@totalRows)

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
SELECT Distinct    
   -- RowNumber,
    '' As ResourceId, 
    lr.ResourceIntId,
    lr.ResourceVersionId As RowId,
    lr.ResourceVersionId,
    lr.ResourceVersionIntId,
    lr.ResourceUrl,
    --DocId,
    CASE
      WHEN lr.Title IS NULL THEN 'No Title'
      WHEN len(lr.Title) = 0 THEN 'No Title'
      ELSE lr.Title
    END AS Title,
    lr.SortTitle,
    lr.[Description],
    isnull(Publisher,'Unknown') As Publisher,
    rtrim(ltrim(isnull(Creator,''))) As Creator,
    lr.ViewCount,
    lr.FavoriteCount,
    --Subjects,
    --replace(isnull(lr.Subjects, ''), '","', ', ') As Subjects,
    --replace(isnull(rsub.Subjects, ''), '","', ', ') As Subjects,
    --replace(isnull(rsub.SubjectCsv, ''), '","', ', ') As Subjects,
    '' As Subjects,
    --edList.EducationLevels,    
   '' As EducationLevels,
    Rights,
    AccessRights
   -- ,Modified
		--,isnull(audienceList.AudienceList,'') As AudienceList
		,'' As AudienceList
		,[LikeCount],[DislikeCount]
From #tempWorkTable
    Inner join [dbo].[Resource.Version_Summary] lr on #tempWorkTable.ResourceIntId = lr.ResourceIntId
    left Join [dbo].[Resource.LikesSummary] rlike on lr.ResourceIntId = rlike.ResourceIntId


WHERE RowNumber > @first_id
--order by RowNumber 


if @OutputRelTables = 1 begin
	--==============================================================================================
	if @UsingCodeTableValues = 0 begin
	  CREATE TABLE #edWorkTable(
		    PathwaysEducationLevelId int,
		    RecordCount	int
	  )
	  --			
	  INSERT INTO #edWorkTable (PathwaysEducationLevelId,RecordCount)
	  SELECT rel.PathwaysEducationLevelId, Count(*) As RecordCount 
	  FROM #tempWorkTable work
	  Inner JOIN [Resource.EducationLevelsSummary] rel ON work.ResourceIntId = rel.ResourceIntId 
	  group by rel.PathwaysEducationLevelId
	  --			
	  SELECT rel.PathwaysEducationLevelId As Id, codes.Title, 
	  codes.Title + '  (' + convert(varchar,RecordCount) + ')' As FormattedTitle,   
	  RecordCount 
	  FROM #edWorkTable rel
	  INNER JOIN dbo.[Codes.PathwaysEducationLevel] codes ON rel.PathwaysEducationLevelId = codes.Id
	  where codes.IsPathwaysLevel= 1
	  order by codes.SortOrder, codes.Title
	  end
	else begin
  	SELECT codes.[Id]
      ,codes.[Title]
	    ,codes.Title + '  (' + convert(varchar,isnull(WarehouseTotal,0)) + ')' As FormattedTitle  
	   ,WarehouseTotal  
    FROM [dbo].[Codes.PathwaysEducationLevel] codes
    where codes.IsPathwaysLevel= 1 AND isnull(WarehouseTotal,0) > 0
    order by SortOrder, codes.Title
	  end
	--			
	--=====================================
	if @UsingCodeTableValues = 0 begin
	  CREATE TABLE #rtWorkTable(
		    ResourceTypeId int,
		    RecordCount	int
	  )	
	  INSERT INTO #rtWorkTable (ResourceTypeId,RecordCount)
	  SELECT rel.ResourceTypeId, Count(*) As RecordCount 
	  FROM #tempWorkTable work
	  Inner JOIN [Resource.ResourceType] rel ON work.ResourceIntId = rel.ResourceIntId 
	  group by rel.ResourceTypeId
	  --			
	  SELECT rrt.ResourceTypeId As Id, 
	  crt.Title, 
	  crt.Title + '  (' + convert(varchar,RecordCount) + ')' As FormattedTitle,   
	  RecordCount 
	  FROM #rtWorkTable rrt
	  Inner Join dbo.[Codes.ResourceType] crt on rrt.ResourceTypeId = crt.Id
	  order by crt.SortOrder, crt.Title
	  end
	else begin
  	SELECT [Id]
      ,[Title]
	    ,Title + '  (' + convert(varchar,isnull(WarehouseTotal,0)) + ')' As FormattedTitle  
	   ,WarehouseTotal  
    FROM [dbo].[Codes.ResourceType]
    where isnull(WarehouseTotal,0) > 0
    order by SortOrder, Title
	  end
  	
	end


GO
/****** Object:  StoredProcedure [dbo].[Resource_Search2]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--WHERE ID BETWEEN @startRowIndex AND (@startRowIndex + @maximumRows) - 1
--ROW_NUMBER()
/*

  SELECT 
  --distinct count(*)
  distinct ROW_NUMBER() As Id, lr.ResourceId, lr.ResourceVersionId, lr.Title  
        FROM [dbo].[Resource.Version_Summary] lr
        Left JOIN [Resource.Cluster] rcl ON lr.ResourceId = rcl.ResourceId 
        Left JOIN dbo.[Resource.Subject] lrs ON lr.ResourceId = lrs.ResourceId 
        --Left JOIN dbo.[Resource.Keyword] lrkey ON lr.ResourceId = lrkey.ResourceId 
        Left JOIN dbo.[Resource.ResourceType] rrt ON lr.ResourceId = rrt.ResourceId 
        Left JOIN [Resource.EducationLevel] edList ON lr.ResourceId = edList.ResourceId  
WHERE ID BETWEEN 100 AND 200
-- =======================================================================================

DECLARE @RC int,@SortOrder varchar(100),@Filter varchar(5000)
DECLARE @StartPageIndex int, @PageSize int, @totalRows int, @OutputRelTables bit
--
set @SortOrder = ''
-- OR lr.[Description] like ''%Finance%'' OR lr.[Description] like ''%Manufacturing%'' 
 
-- blind search 
set @Filter = ''
set @Filter = ' where ( lr.id in (select ResourceIntId from [Resource.Language] where LanguageId in (1)) ) AND ( (lr.AccessRightsId in (2,4)) ) AND ( lr.id in (select ResourceIntId from [Resource.Cluster] where ClusterId in (8,11)) )  '

set @filter = ' where  ( lr.id in (select ResourceIntId from [Resource.Language] where LanguageId in (1)) ) '
    
set @OutputRelTables = 1
set @StartPageIndex = 1
set @PageSize = 55

set statistics time on     

EXECUTE @RC = Resource_Search2
     @Filter,@SortOrder  ,@StartPageIndex  ,@PageSize, @OutputRelTables  ,@totalRows OUTPUT

select 'total rows = ' + convert(varchar,@totalRows)

set statistics time off   
    
*/



CREATE PROCEDURE [dbo].[Resource_Search2]
		@Filter           varchar(5000)
		,@SortOrder       varchar(100)
		,@StartPageIndex  int
		,@PageSize        int
		,@OutputRelTables bit
		,@TotalRows       int OUTPUT

As

SET NOCOUNT ON;
-- paging
DECLARE
      @first_id               int
      ,@startRow        int
      ,@lastRow int
      ,@minRows int
      ,@debugLevel      int
      ,@SQL             varchar(5000)
      ,@TopClause       varchar(100)
      ,@DefaultFilter   varchar(1000)
      ,@OrderBy         varchar(100)
-- =================================

Set @debugLevel = 4
Set @TopClause= ''
--actually the following is implied in the view (and IsPathwayLevel)
Set @DefaultFilter= ' Where (edList.PathwaysEducationLevelId in (3,4,5,6)) ' 
if len(@SortOrder) > 0
      set @OrderBy = ' Order by ' + @SortOrder
else
      set @OrderBy = ' Order by lr.SortTitle '
--===================================================
-- Calculate the range
--===================================================      
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1
SET @lastRow =  @StartPageIndex + @PageSize

--
if len(isnull(@Filter,'')) = 0 begin
   Set @TopClause= ' TOP 8000 '
   Set @OutputRelTables = 0 
   end
else if @minRows < 1000   begin 
  set @TopClause= ' TOP 8000 '   
  end   
--
-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))
--                 Left JOIN dbo.[Resource.Property] lrp ON lr.ResourceId = lrp.ResourceId 
--Left JOIN [Resource.EducationLevelsSummary] edList ON lr.ResourceId = edList.ResourceId 
 
--Left JOIN [Resource.Cluster] rcl ON lr.ResourceId = rcl.ResourceId 
--Left JOIN dbo.[Resource.Subject] lrs ON lr.ResourceId = lrs.ResourceId 
--Left JOIN dbo.[Resource.ResourceType] rrt ON lr.ResourceId = rrt.ResourceId 
--Left JOIN [Resource.EducationLevel] edList ON lr.ResourceId = edList.ResourceId  
--    Left Join [dbo].[Resource.EducationLevelsList] edList on lr.ResourceId = edList.ResourceId
--Left JOIN dbo.[Resource.Subject] rsub ON lr.ResourceIntId = rsub.ResourceIntId 



  set @SQL = '  
SELECT Distinct    
   -- RowNumber,
    lr.ResourceId,
    lr.ResourceIntId,
    lr.ResourceVersionIntId,
    lr.ResourceUrl,
     CASE
      WHEN lr.Title IS NULL THEN ''No Title''
      WHEN len(lr.Title) = 0 THEN ''No Title''
      ELSE lr.Title
    END AS Title,
    lr.SortTitle,
    [Description],
    isnull(Publisher,''Unknown'') As Publisher,
    rtrim(ltrim(isnull(Creator,''''))) As Creator,
    lr.ViewCount,
    lr.FavoriteCount,
    '''' As Subjects,
    '''' As EducationLevels, 
    '''' As AudienceList, 
    Rights,
    AccessRights
FROM
   (SELECT distinct  TOP 25000 
         ROW_NUMBER() OVER(' + @OrderBy + ') as RowNumber,
        lr.ResourceIntId, lr.Title  
        FROM [dbo].[Resource.Version_Summary] lr
        --Left JOIN dbo.[Resource.Subject] lrs ON lr.ResourceIntId = lrs.ResourceIntId  
        ' 
        + @Filter + ' 
   ) as DerivedTableName
       Inner join [dbo].[Resource.Version_Summary] lr on DerivedTableName.ResourceIntId = lr.ResourceIntId

WHERE RowNumber BETWEEN ' + convert(varchar,@StartPageIndex) + ' AND ' + convert(varchar,@lastRow) + ' ' 


  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL
  exec (@SQL)
  --===============================
  
--DECLARE @TempItems TABLE
--(
--   ID int IDENTITY,
--   ResourceIntId int
--)

--INSERT INTO @TempItems (ResourceIntId)
--SELECT distinct lr.ResourceId 
--        FROM [dbo].[Resource.Version_Summary] lr
--        Left JOIN [Resource.Cluster] rcl ON lr.ResourceId = rcl.ResourceId 
--        Left JOIN dbo.[Resource.Subject] lrs ON lr.ResourceId = lrs.ResourceId 
--        Left JOIN dbo.[Resource.ResourceType] rrt ON lr.ResourceId = rrt.ResourceId 
--        Left JOIN [Resource.EducationLevel] edList ON lr.ResourceId = edList.ResourceId 
--        Where (rcl.ClusterId > 1 )
        
--select @TotalRows= count(*) from @TempItems 

--select @TotalRows



GO
/****** Object:  StoredProcedure [dbo].[Resource_SetActiveState]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Create date: 6/5/2012
-- Description:	Update Resource
-- 
-- =============================================
CREATE PROCEDURE [dbo].[Resource_SetActiveState]
	@ResourceId int, 
	@IsActive bit

AS

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	UPDATE [Resource]
	SET IsActive = @IsActive,
		LastUpdated = GETDATE()
	WHERE Id = @ResourceId
END

GO
/****** Object:  StoredProcedure [dbo].[ResourceDecreaseFavorite]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
SELECT TOP 1000 
Id,[ResourceUrl]
      ,[ViewCount]
      ,[FavoriteCount]
      ,[Created]
      --,[LastUpdated]
      --,[IsActive]
      --,[HasPathwayGradeLevel]
      
  FROM [Isle_IOER].[dbo].[Resource]
  where id = 10107

[ResourceDecreaseFavorite] 10107

*/
-- =============================================
-- Author:		MP
-- Create date: 4/21/2014
-- Description:	decrease favorite count by 1

-- =============================================
Create PROCEDURE [dbo].[ResourceDecreaseFavorite]
	@Id int
AS
declare @CurrentCnt int 
BEGIN TRANSACTION 
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select @CurrentCnt= isnull(FavoriteCount, 0) from Resource where Id = @Id
	print 'current fav count: ' + convert(varchar,@CurrentCnt)

	UPDATE [Resource]
	SET FavoriteCount = isnull(@CurrentCnt,0) - 1
	
	WHERE Id = @Id
COMMIT 

GO
/****** Object:  StoredProcedure [dbo].[ResourceDelete]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 08/24/2012
-- Description:	Delete a resource
-- Mods
-- 13-04-25 mparsons - updated to allow by id or rowId
-- =============================================
CREATE PROCEDURE [dbo].[ResourceDelete]
	@Id int,
	@RowId varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
  If @Id = 0          SET @Id = NULL
  If @RowId = ''      SET @RowId = NULL 
  if @Id is null AND @RowId is null begin
	  print 'ResourceGet - invalid request'
	  RAISERROR('ResourceGet - invalid request. Require Source @Id, OR @RowId', 18, 1)    
	  RETURN -1 
	  end
  
  DELETE FROM [Resource]
  WHERE (RowId = @RowId OR @RowId is null)
  AND (Id = @Id OR @Id is null)
END

GO
/****** Object:  StoredProcedure [dbo].[ResourceEducationLevel_SearchCounts]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
  
/*
SELECT [RowId]
      ,[ResourceId]
      ,[ResourceTypeId]
      ,[OriginalType]
  FROM [dbo].[Resource.ResourceType] base
  inner join [Codes.ResourceType] crt on base.ResourceTypeId = crt.Id
  
SELECT crt.Title,  SortOrder      ,Count(*) As RecordCount
  FROM [dbo].[Resource.ResourceType] base
  inner join [Codes.ResourceType] crt on base.ResourceTypeId = crt.Id
  group by crt.Title,  SortOrder
  
  
  Order by crt.SortOrder
  
GO


-- ===========================================================
DECLARE @RC int,@Filter varchar(5000)

set @Filter = ' where lr.Title like ''%manu%'' OR lr.Description like
''%manu%''  '

set @Filter = ' where ( (lr.Title like ''%Energy%'' OR lr.[Description] like ''%Energy%'' OR lrp.Value like ''%Energy%'') OR (lr.Title like ''%Finance%'' OR lr.[Description] like ''%Finance%'' OR lrp.Value like ''%Finance%'') OR (lr.Title like ''%Health Science%'' OR lr.[Description] like ''%Health Science%'' OR lrp.Value like ''%Health Science%'') ) AND (lrp.Value in (''high school'',''Higher Education'')) '

--set @Filter = @Filter + ' AND (edList.Level in (''secondary'')) '

EXECUTE @RC = ResourceEducationLevel_SearchCounts  @Filter

*/

/* =============================================
Description:      Resource Education search - returns group by count, typically using the same filter as for the resource search; shown on narrowing filters
  
------------------------------------------------------
Modifications
12-07-10 mparsons - new
=============================================

*/

CREATE PROCEDURE [dbo].[ResourceEducationLevel_SearchCounts]
		@Filter           varchar(5000)
As

SET NOCOUNT ON;
-- paging
DECLARE
      @debugLevel      int
      ,@SQL             varchar(5000)
      ,@GroupBy         varchar(200)

-- =================================

Set @debugLevel = 4

set @GroupBy = ' group by edList.PathwaysEducationLevelId  '

-- =================================

CREATE TABLE #tempWorkTable(
      PathwaysEducationLevelId int,
      RecordCount	int
)

-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))
  set @SQL = 'SELECT edList.PathwaysEducationLevelId, Count(*) As RecordCount 
		FROM dbo.Resource lr
        Left JOIN dbo.[Resource.Property] lrp ON lr.RowId = lrp.ResourceId 
        Left JOIN dbo.[Resource.ResourceType] rrt ON lr.RowId = rrt.ResourceId 
        Left JOIN [Resource.EducationLevelsSummary] edList ON lr.RowId = edList.ResourceId  '
        + @Filter

  set @SQL = @SQL + ' ' + @GroupBy

  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL

  INSERT INTO #tempWorkTable (PathwaysEducationLevelId,RecordCount)
  exec (@SQL)
  
  
SELECT rel.PathwaysEducationLevelId As Id, 
codes.Title, 
codes.Title + '  (' + convert(varchar,RecordCount) + ')' As FormattedTitle,   
RecordCount 
FROM #tempWorkTable rel
INNER JOIN dbo.[Codes.PathwaysEducationLevel] codes ON rel.PathwaysEducationLevelId = codes.Id
where codes.IsPathwaysLevel= 1
order by codes.SortOrder, codes.Title

-- =================================

GO
/****** Object:  StoredProcedure [dbo].[ResourceGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
declare @Id varchar(50)
set @Id = 'AD95FAD2-FC7B-48EE-8F51-E6904F7A9589'
set @Id = 'd9fe9bb9-f719-43f3-b3e6-1272b1684488'
--set @Id = 'e0219864-cb78-4146-b043-4e6476e3933e'
exec [ResourceGet]  0, @Id

[ResourceGet]  2,''

*/


/* =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/5/2012
-- Description:	Get a resource
	Note: should not do a get just by resourceUrl as can have duplicates by submitter
-- modifications
-- 12-06-15 mparsons added get by @ResourceUrl
-- 12-06-29 jgrimmer added get by @SubmitterRowId
-- 12-06-29 jgrimmer added SubmitterRowId and Created fields
-- 12-08-30 jgrimmer split fields between [Resource] and [Resource.Version] tables.
--					 modified so it gets a single row.
-- 13-01-02 jgrimmer added HasPathwayGradeLevel, Created, LastUpdated
-- 13-03-04 jgrimmer added Id
-- 13-04-25 mparsons - updated to allow by id or rowId
-- =============================================
*/
CREATE PROCEDURE [dbo].[ResourceGet]
		@Id int,
	  @RowId varchar(50)
AS

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
  If @Id = 0          SET @Id = NULL
  If @RowId = ''      SET @RowId = NULL 
  
  
  if @Id is null AND @RowId is null begin
	  print 'ResourceGet - invalid request'
	  RAISERROR('ResourceGet - invalid request. Require Source @Id, OR @RowId', 18, 1)    
	  RETURN -1 
	  end
  	
	SELECT distinct
		base.RowId, 
		base.ResourceUrl, 
		base.ViewCount,
		base.FavoriteCount,
		base.HasPathwayGradeLevel,
		base.Created, IsActive,
		base.LastUpdated,
		base.Id
	
	FROM [Resource] base
  WHERE 
      (base.RowId = @RowId OR @RowId is null)
  AND (Id = @Id OR @Id is null)
END

GO
/****** Object:  StoredProcedure [dbo].[ResourceInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/5/2012
-- Description:	Insert a resource row
-- 2012-07-02 jgrimmer - Added @SubmitterRowId, @Created
-- 2012-08-24 jgrimmer - Added @TypicalLearningTime
-- 2012-08-30 jgrimmer - Dropped fields found in [Resource.Version] table
-- 2012-09-21 jgrimmer - Added Created and LastUpdated dates ==> which never made it to TFS!!!
-- 2013-06-26 mparsons - changed, finally, to return the integer key
-- 2014-11-27 mparsons - changed URL to 600
-- =============================================
CREATE PROCEDURE [dbo].[ResourceInsert]
	@ResourceUrl varchar(600),
	@ViewCount int,
	@FavoriteCount int,
	@HasPathwayGradeLevel bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @NewId uniqueidentifier
	set @NewId= NEWID()
	IF @ViewCount IS NULL SET @ViewCount = 0
	IF @FavoriteCount IS NULL SET @FavoriteCount = 0
	IF @HasPathwayGradeLevel IS NULL SET @HasPathwayGradeLevel = 1

	INSERT INTO [Resource] (RowId, ResourceUrl, ViewCount, FavoriteCount, Created, LastUpdated, HasPathwayGradeLevel)
	VALUES (@NewId, @ResourceUrl, @ViewCount, @FavoriteCount, GETDATE(), GETDATE(), @HasPathwayGradeLevel)

  select SCOPE_IDENTITY() as Id, @NewId As RowId
	--select @NewId as Id	
END


GO
/****** Object:  StoredProcedure [dbo].[ResourcePagesReader]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--WHERE ID BETWEEN @startRowIndex AND (@startRowIndex + @maximumRows) - 1
--ROW_NUMBER()
/*


-- =======================================================================================

DECLARE @Filter varchar(5000), @@StartingId int, @DatasetSize int
--
-- blind search 
set @Filter = ''
    

set @@StartingId = 713
set @@StartingId = 790
set @DatasetSize = 55

set statistics time on     

EXECUTE dbo.ResourcePagesReader     @Filter, @@StartingId  ,@DatasetSize

set statistics time off   
    
*/



CREATE PROCEDURE [dbo].[ResourcePagesReader]
		@Filter           varchar(5000)
		,@StartingId  int
		,@DatasetSize        int

As

SET NOCOUNT ON;
-- paging
DECLARE
      @debugLevel      int
      ,@SQL             varchar(5000)

-- =================================

Set @debugLevel = 4


--
-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))
--                

SET ROWCOUNT @DatasetSize
SELECT [ResourceVersionId]
      ,[ResourceIntId]
      ,[DocId]
      ,[Title]
      ,[Description]
      ,[Publisher]
      ,[Created]
      ,[AccessRights]
      ,[Keywords]
      ,[Subjects]
      ,[LanguageIds]
      ,[Languages]
      ,[ClusterIds]
      ,[Clusters]
      ,[AudienceIds]
      ,[Audiences]
      ,[EducationLevelIds]
      ,[EducationLevels]
      ,[ResourceTypeIds]
      ,[ResourceTypes]
      ,[ResourceFormatIds]
      ,[ResourceFormats]
      ,[GroupTypeIds]
      ,[GroupTypes]
      ,[ItemTypeIds]
      ,[ItemTypes]
      ,[StandardIds]
      ,[Standards]
      ,[AssessmentTypeId]
      ,[EducationUseId]
      ,[OriginalType]
      ,[ResourceURL]
FROM [dbo].[Resource.SearchableIndexView2] lr
where ResourceIntId > @StartingId
order by ResourceIntId


  --===============================
  




GO
/****** Object:  StoredProcedure [dbo].[ResourcePagesSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--WHERE ID BETWEEN @startRowIndex AND (@startRowIndex + @maximumRows) - 1
--ROW_NUMBER()
/*


-- =======================================================================================

DECLARE @RC int,@SortOrder varchar(100),@Filter varchar(5000)
DECLARE @StartPageIndex int, @PageSize int
--
 
-- blind search 
set @Filter = ''
    

set @StartPageIndex = 1
set @PageSize = 55

set statistics time on     

EXECUTE dbo.ResourcePagesSelect     @Filter, @StartPageIndex  ,@PageSize

set statistics time off   
    
*/



CREATE PROCEDURE [dbo].[ResourcePagesSelect]
		@Filter           varchar(5000)
		,@StartPageIndex  int
		,@PageSize        int

As

SET NOCOUNT ON;
-- paging
DECLARE
      @first_id               int
      ,@startRow        int
      ,@lastRow int
      ,@minRows int
      ,@debugLevel      int
      ,@SQL             varchar(5000)
      ,@OrderBy         varchar(100)
-- =================================

Set @debugLevel = 4
set @OrderBy = ' Order by ResourceIntId '

--===================================================
-- Calculate the range
--===================================================      
SET @StartPageIndex =  (@StartPageIndex - 1)  * @PageSize
IF @StartPageIndex < 1        SET @StartPageIndex = 1
SET @lastRow =  @StartPageIndex + @PageSize

--
-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))
--                

  set @SQL = '  
SELECT [ResourceVersionId]
      ,DerivedTableName.[ResourceIntId]
      ,[DocId]
      ,DerivedTableName.[Title]
      ,[Description]
      ,[Publisher]
      ,[Created]
      ,[AccessRights]
      ,[Keywords]
      ,[Subjects]
      ,[LanguageIds]
      ,[Languages]
      ,[ClusterIds]
      ,[Clusters]
      ,[AudienceIds]
      ,[Audiences]
      ,[EducationLevelIds]
      ,[EducationLevels]
      ,[ResourceTypeIds]
      ,[ResourceTypes]
      ,[ResourceFormatIds]
      ,[ResourceFormats]
      ,[GroupTypeIds]
      ,[GroupTypes]
      ,[ItemTypeIds]
      ,[ItemTypes]
      ,[StandardIds]
      ,[Standards]
      ,[AssessmentTypeId]
      ,[EducationUseId]
      ,[OriginalType]
      ,[ResourceURL]
FROM
   (SELECT distinct  
         ROW_NUMBER() OVER(' + @OrderBy + ') as RowNumber,
        lr.ResourceIntId, lr.Title  
        FROM [dbo].[Resource.SearchableIndexView2] lr
        where ResourceIntId > 0
   ) as DerivedTableName
       Inner join [dbo].[Resource.SearchableIndexView2] lr on DerivedTableName.ResourceIntId = lr.ResourceIntId

WHERE RowNumber BETWEEN ' + convert(varchar,@StartPageIndex) + ' AND ' + convert(varchar,@lastRow) + ' ' 


  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL
  exec (@SQL)
  --===============================
  




GO
/****** Object:  StoredProcedure [dbo].[ResourceSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/5/2012
-- Description:	Select resources
-- 2012-07-02 jgrimmer Added SubmitterRowId and Created fields
-- 2012-08-30 jgrimmer Dropped fields moved to [Resource.Version] table.
-- 2013-01-02 jgrimmer - Added HasPathwayGradeLevel, Created, LastUpdated
-- 2013-03-04 jgrimmer Added Id
-- =============================================
CREATE PROCEDURE [dbo].[ResourceSelect]
	@filter varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @sql varchar(max)
	SET @sql = 'SELECT RowId, ResourceUrl, ViewCount, FavoriteCount, HasPathwayGradeLevel, Created, LastUpdated, Id
FROM [Resource]
' + @filter

	PRINT @sql
	EXEC(@sql)
END


GO
/****** Object:  StoredProcedure [dbo].[ResourceTag.Convert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
May 22 - ==> next resource format

select count(*) from [Resource.Tag]

SELECT 
      [CategoryId]     ,[CategoryTitle]
	  ,[TagTitle]	        ,[TagValueId]
			,count(*)

  FROM [Isle_IOER].[dbo].[Resource_TagSummary]
    group by 
	 [CategoryId]	 ,[CategoryTitle]
	  ,[TagTitle]	        ,[TagValueId]
*/

--==> may need some cleanup ex with audience type
/*
SELECT     TOP (200) RowID, ResourceId, AudienceId, OriginalAudience, CreatedById, Created, ResourceIntId
FROM         [Resource.IntendedAudience]
WHERE     (AudienceId IS NOT NULL) AND (ResourceIntId NOT IN
                          (SELECT     Id
                            FROM          Resource))
ORDER BY Created DESC

*/


-- =========================================================================
-- Create date: 4/25/2014
-- Description:	Populate the resource tag table from the resource child tables
--
-- Mods
-- 14-05-21 mparsons - update to check for existing - for reruns
--					 - to do - will be useful to include a date option, for ex to run after the import until latter is converted		
-- =========================================================================
CREATE PROCEDURE [dbo].[ResourceTag.Convert] 
AS
BEGIN
	SET NOCOUNT ON;
	
DECLARE @StartDate datetime
set @StartDate = '2014-05-01'

--  ========== Access Rights??? --> store with RV, only one value =================
-- if used, need to ensure only one record is created
--INSERT INTO [dbo].[Resource.Tag]
--           ([ResourceIntId]
--           ,[TagValueId]
--           ,[Created]
--           ,[CreatedById]
--           ,[OriginalValue])

--SELECT 
--top 1000
--		rv.[ResourceIntId]
--		,1		--Access Rights
--		,rv.AccessRightsId
--		,res.[Created]
--		,NULL		--[CreatedById]
--		,rv.AccessRights
      
--  FROM [dbo].[Resource.Version] rv
--  inner join [Resource] res on rv.resourceIntId = res.id
--where rv.IsActive = 1 AND AccessRightsId is not null 

--GO
--  ========== audienceType =================
INSERT INTO [dbo].[Resource.Tag]
           ([ResourceIntId]
           ,[TagValueId]
           ,[Created]
           ,[CreatedById]
           ,[OriginalValue])

SELECT 
--top 1000
		[ResourceIntId]
		--,7		--Audience Type		,[AudienceId]
		,codes.TagValueId
		,[Created]
		,[CreatedById]
		,[OriginalAudience]
      
  FROM [dbo].[Resource.IntendedAudience] base
  inner join [Codes.TagCategoryValue_summary] codes on base.AudienceId = codes.TagRelativeId
where codes.categoryId = 7
AND [AudienceId] is not null 
and base.Created > @StartDate
and codes.TagValueId not in 
	(select [TagValueId] from [Resource.Tag] where [ResourceIntId] = base.ResourceIntId)

--  ========== Assessment Type =================
INSERT INTO [dbo].[Resource.Tag]
           ([ResourceIntId]
           ,[TagValueId]
           ,[Created]
           ,[CreatedById]
           ,[OriginalValue])

SELECT 
--top 1000
		[ResourceIntId]
		--,25		--Assessment Type		,[AssessmentTypeId]
		,codes.TagValueId
		,[Created]
		,[CreatedById]
		,''
      
  FROM [dbo].[Resource.AssessmentType] base
  inner join [Codes.TagCategoryValue_summary] codes on base.AssessmentTypeId = codes.TagRelativeId
where codes.categoryId = 25
AND AssessmentTypeId is not null 
and base.Created > @StartDate
and codes.TagValueId not in 
	(select [TagValueId] from [Resource.Tag] where [ResourceIntId] = base.ResourceIntId)



--  ========== Career Cluster =================
INSERT INTO [dbo].[Resource.Tag]
           ([ResourceIntId]
           ,[TagValueId]
           ,[Created]
           ,[CreatedById]
           ,[OriginalValue])

SELECT 
--top 1000
		[ResourceIntId]
		--,8		--Career Cluster		,ClusterId
		,codes.TagValueId
		,[Created]
		,[CreatedById]
		,''
      
  FROM [dbo].[Resource.Cluster] base
  inner join [Codes.TagCategoryValue_summary] codes on base.ClusterId = codes.TagRelativeId
where codes.categoryId = 8
AND ClusterId is not null 
and base.Created > @StartDate
and codes.TagValueId not in 
	(select [TagValueId] from [Resource.Tag] where [ResourceIntId] = base.ResourceIntId)



--  ========== Educational Use =================
INSERT INTO [dbo].[Resource.Tag]
           ([ResourceIntId]
           ,[TagValueId]
           ,[Created]
           ,[CreatedById]
           ,[OriginalValue])

SELECT 
--top 1000
		[ResourceIntId]
		--,11		--Educational Use		,EducationUseId
		,codes.TagValueId
		,[Created]
		,[CreatedById]
		,OriginalType
      
  FROM [dbo].[Resource.EducationUse] base
  inner join [Codes.TagCategoryValue_summary] codes on base.EducationUseId = codes.TagRelativeId
where codes.categoryId = 11
AND EducationUseId is not null 
and base.Created > @StartDate
and codes.TagValueId not in 
	(select [TagValueId] from [Resource.Tag] where [ResourceIntId] = base.ResourceIntId)


--  ========== Grade Level =================
INSERT INTO [dbo].[Resource.Tag]
           ([ResourceIntId]
           ,[TagValueId]
           ,[Created]
           ,[CreatedById]
           ,[OriginalValue])

SELECT 
--top 1000
		[ResourceIntId]
		--,13		--Grade Level		,GradeLevelId
		,codes.TagValueId
		,[Created]
		,[CreatedById]
		,OriginalLevel
      
  FROM [dbo].[Resource.GradeLevel]  base
  inner join [Codes.TagCategoryValue_summary] codes on base.GradeLevelId = codes.TagRelativeId
where codes.categoryId = 13
AND GradeLevelId is not null 
and base.Created > @StartDate
and codes.TagValueId not in 
	(select [TagValueId] from [Resource.Tag] where [ResourceIntId] = base.ResourceIntId)


--  ========== Group Type =================
INSERT INTO [dbo].[Resource.Tag]
           ([ResourceIntId]
           ,[TagValueId]
           ,[Created]
           ,[CreatedById]
           ,[OriginalValue])

SELECT 
--top 1000
		[ResourceIntId]
		--,14	--Group Type		,GroupTypeId
		,codes.TagValueId
		,[Created]
		,[CreatedById]
		,''	--OriginalLevel
      
  FROM [dbo].[Resource.GroupType]  base
  inner join [Codes.TagCategoryValue_summary] codes on base.GroupTypeId = codes.TagRelativeId
where codes.categoryId = 14
AND  GroupTypeId is not null 
and base.Created > @StartDate
and codes.TagValueId not in 
	(select [TagValueId] from [Resource.Tag] where [ResourceIntId] = base.ResourceIntId)


--  ========== Item Type =================
INSERT INTO [dbo].[Resource.Tag]
           ([ResourceIntId]
           ,[TagValueId]
           ,[Created]
           ,[CreatedById]
           ,[OriginalValue])

SELECT 
--top 1000
		[ResourceIntId]
		--,15	--Item Type		,ItemTypeId
		,codes.TagValueId
		,[Created]
		,[CreatedById]
		,''	--OriginalLevel
      
  FROM [dbo].[Resource.ItemType]  base
  inner join [Codes.TagCategoryValue_summary] codes on base.ItemTypeId = codes.TagRelativeId
where codes.categoryId = 15
AND  ItemTypeId is not null 
and base.Created > @StartDate
and codes.TagValueId not in 
	(select [TagValueId] from [Resource.Tag] where [ResourceIntId] = base.ResourceIntId)


--  ========== Language =================
INSERT INTO [dbo].[Resource.Tag]
           ([ResourceIntId]
           ,[TagValueId]
           ,[Created]
           ,[CreatedById]
           ,[OriginalValue])

SELECT 
--top 15000
		[ResourceIntId]
		--,17			--Language		,LanguageId
		,codes.TagValueId
		,[Created]
		,[CreatedById]
		,OriginalLanguage
      
  FROM [dbo].[Resource.Language]  base
  inner join [Codes.TagCategoryValue_summary] codes on base.LanguageId = codes.TagRelativeId
where codes.categoryId = 17
AND  LanguageId is not null 
and base.Created > @StartDate
and codes.TagValueId not in 
	(select [TagValueId] from [Resource.Tag] where [ResourceIntId] = base.ResourceIntId)


--  ========== Resource Format =================
INSERT INTO [dbo].[Resource.Tag]
           ([ResourceIntId]
           ,[TagValueId]
           ,[Created]
           ,[CreatedById]
           ,[OriginalValue])

SELECT 
--top 1000
		[ResourceIntId]
		--,18			--Resource Format		,CodeId
		,codes.TagValueId
		,[Created]
		,[CreatedById]
		,OriginalValue
      
  FROM [dbo].[Resource.Format]  base
  inner join [Codes.TagCategoryValue_summary] codes on base.CodeId = codes.TagRelativeId
where codes.categoryId = 18
AND  base.CodeId is not null 
and base.Created > @StartDate
and codes.TagValueId not in 
	(select [TagValueId] from [Resource.Tag] where [ResourceIntId] = base.ResourceIntId)

--  ========== Subject ????????????????????????? =================
-- not sure we want to use as tab or .Text
-- just do coded for now
INSERT INTO [dbo].[Resource.Tag]
           ([ResourceIntId]
           ,[TagValueId]
           ,[Created]
           ,[CreatedById]
           ,[OriginalValue])

SELECT 
--top 1000
		[ResourceIntId]
		--,20			--Subject		,base.CodeId
		,codes.TagValueId
		,[Created]
		,[CreatedById]
		,[Subject]
      
  FROM [dbo].[Resource.Subject] base 
  inner join [Codes.TagCategoryValue_summary] codes on base.CodeId = codes.TagRelativeId
where codes.categoryId = 20
AND  base.CodeId is not null 
and base.Created > @StartDate
and codes.TagValueId not in 
	(select [TagValueId] from [Resource.Tag] where [ResourceIntId] = base.ResourceIntId)


--  ========== Resource Type =================
INSERT INTO [dbo].[Resource.Tag]
           ([ResourceIntId]
           ,[TagValueId]
           ,[Created]
           ,[CreatedById]
           ,[OriginalValue])

SELECT 
--top 1000
		[ResourceIntId]
		--,19			--Resource Type		,ResourceTypeId
		,codes.TagValueId
		,[Created]
		,[CreatedById]
		,OriginalType
      
  FROM [dbo].[Resource.ResourceType] base
  inner join [Codes.TagCategoryValue_summary] codes on base.ResourceTypeId = codes.TagRelativeId
where codes.categoryId = 19
AND ResourceTypeId is not null 
and base.Created > @StartDate
and codes.TagValueId not in 
	(select [TagValueId] from [Resource.Tag] where [ResourceIntId] = base.ResourceIntId)


--  ========== target Site??? =================
--skip for now. use current context - don't want to add 500K+ rows
--INSERT INTO [dbo].[Resource.Tag]
--           ([ResourceIntId]
--           ,[TagValueId]
--           ,[Created]
--           ,[CreatedById]
--           ,[OriginalValue])

--SELECT 
--top 1000
--		[Id]
--		,266
--		,base.[Created]
--		,isnull(rp.PublishedById,0)		
--		,''
      
--  FROM [dbo].[Resource] base
--  inner join [Resource.PublishedBy] rp on base.Id = rp.ResourceIntId
--where base.IsActive = 1
and base.Created > @StartDate
--and 266 not in 
--	(select [TagValueId] from [Resource.Tag] where [ResourceIntId] = base.Id)

END


GO
/****** Object:  StoredProcedure [dbo].[ResourceType_SearchCounts]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
  
/*
SELECT [RowId]
      ,[ResourceId]
      ,[ResourceTypeId]
      ,[OriginalType]
  FROM [dbo].[Resource.ResourceType] base
  inner join [Codes.ResourceType] crt on base.ResourceTypeId = crt.Id
  
SELECT crt.Title,  SortOrder      ,Count(*) As RecordCount
  FROM [dbo].[Resource.ResourceType] base
  inner join [Codes.ResourceType] crt on base.ResourceTypeId = crt.Id
  group by crt.Title,  SortOrder
  
  
  Order by crt.SortOrder
  
GO


-- ===========================================================
DECLARE @RC int,@Filter varchar(5000)

set @Filter = ' where lr.Title like ''%manu%'' OR lr.Description like
''%manu%''  '

set @Filter = ' where ( (lr.Title like ''%Energy%'' OR lr.[Description] like ''%Energy%'' OR lrp.Value like ''%Energy%'') OR (lr.Title like ''%Finance%'' OR lr.[Description] like ''%Finance%'' OR lrp.Value like ''%Finance%'') OR (lr.Title like ''%Health Science%'' OR lr.[Description] like ''%Health Science%'' OR lrp.Value like ''%Health Science%'') ) AND (lrp.Value in (''high school'',''Higher Education'')) '

--set @Filter = @Filter + ' AND (edList.Level in (''secondary'')) '
set @Filter = ''
EXECUTE @RC = ResourceType_SearchCounts  @Filter

*/

/* =============================================
      Description:      Resource Type search - returns group by count, typically using the same filter as for the research search
  
------------------------------------------------------
Modifications
12-06-29 mparsons - new
=============================================

*/

CREATE PROCEDURE [dbo].[ResourceType_SearchCounts]
		@Filter           varchar(5000)
As

SET NOCOUNT ON;
-- paging
DECLARE
      @debugLevel      int
      ,@SQL             varchar(5000)
      ,@GroupBy         varchar(200)

-- =================================

Set @debugLevel = 4

set @GroupBy = ' group by rrt.ResourceTypeId  '

-- =================================

CREATE TABLE #tempWorkTable(
      ResourceTypeId int,
      RecordCount	int
)

-- =================================

  if len(@Filter) > 0 begin
     if charindex( 'where', @Filter ) = 0 OR charindex( 'where',  @Filter ) > 10
        set @Filter =     ' where ' + @Filter
     end

  print '@Filter len: '  +  convert(varchar,len(@Filter))
--        Left JOIN dbo.[EducationLevel] edList ON lr.RowId = edList.ResourceId 
  set @SQL = 'SELECT rrt.ResourceTypeId, Count(*) As RecordCount 
		FROM dbo.Resource lr
        INNER JOIN dbo.[Resource.Property] lrp ON lr.RowId = lrp.ResourceId 
        Left JOIN dbo.[Resource.ResourceType] rrt ON lr.RowId = rrt.ResourceId 
        Left JOIN [Resource.EducationLevelsSummary] edList ON lr.RowId = edList.ResourceId  '
        + @Filter

  set @SQL = @SQL + ' ' + @GroupBy

  print '@SQL len: '  +  convert(varchar,len(@SQL))
  print @SQL

  INSERT INTO #tempWorkTable (ResourceTypeId,RecordCount)
  exec (@SQL)
  
  
SELECT rrt.ResourceTypeId As Id, 
crt.Title, 
crt.Title + '  (' + convert(varchar,RecordCount) + ')' As FormattedTitle,   
RecordCount 
FROM #tempWorkTable rrt
Inner Join dbo.[Codes.ResourceType] crt on rrt.ResourceTypeId = crt.Id
order by crt.SortOrder, crt.Title

-- =================================

GO
/****** Object:  StoredProcedure [dbo].[ResourceUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 6/5/2012
-- Description:	Update Resource
-- 2012-07-02 jgrimmer - Added SubmitterRowId, Imported, and Created
-- 2012-08-24 jgrimmer - Added TypicalLearningTime
-- 2012-09-21 jgrimmer - Removed fields moved to ResourceVersion and added Created and LastUpdated fields
-- 2013-06-04 jgrimmer - Added IsActive
-- 2014-11-27 mparsons - changed URL to 600
-- =============================================
CREATE PROCEDURE [dbo].[ResourceUpdate]
	@RowId uniqueidentifier, 
	@ResourceUrl varchar(600),
	@ViewCount int,
	@FavoriteCount int,
	@HasPathwayGradeLevel bit,
	@IsActive bit
AS
--	@ResourceUrl varchar(500),
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	UPDATE [Resource]
	SET ResourceUrl = @ResourceUrl,
		ViewCount = @ViewCount,
		FavoriteCount = @FavoriteCount,
		LastUpdated = GETDATE(),
		HasPathwayGradeLevel = @HasPathwayGradeLevel,
		IsActive = @IsActive
	WHERE RowId = @RowId
END

GO
/****** Object:  StoredProcedure [dbo].[ResourceUpdateById]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		MP
-- Create date: 2/3/2014
-- Description:	Update Resource By Id - should replace the existing one by rowId
-- 2014-11-27 mparsons - changed URL to 600
-- =============================================
CREATE PROCEDURE [dbo].[ResourceUpdateById]
	@Id int, 
	@ResourceUrl varchar(600),
	@ViewCount int,
	@FavoriteCount int,
	@IsActive bit
AS

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	UPDATE [Resource]
	SET ResourceUrl = @ResourceUrl,
		ViewCount = @ViewCount,
		FavoriteCount = @FavoriteCount,
		LastUpdated = GETDATE(), 
		IsActive = @IsActive
	WHERE Id = @Id
END

GO
/****** Object:  StoredProcedure [dbo].[ResourceUpdateFavorite]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

[ResourceUpdateFavorite] 10107

*/
-- =============================================
-- Author:		MP
-- Create date: 2/3/2014
-- Description:	update favorite count

-- =============================================
CREATE PROCEDURE [dbo].[ResourceUpdateFavorite]
	@Id int
AS
declare @CurrentCnt int 
BEGIN TRANSACTION 
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select @CurrentCnt= isnull(FavoriteCount, 0) from Resource where Id = @Id
	print 'current fav count: ' + convert(varchar,@CurrentCnt)

	UPDATE [Resource]
	SET FavoriteCount = isnull(@CurrentCnt,0) + 1
	
	WHERE Id = @Id
COMMIT 

GO
/****** Object:  StoredProcedure [dbo].[ResourceVersion_InactivateDups]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
--=======================================================================
--start: 1769679
select count(*) FROM [dbo].[Resource.Version]
--=======================================================================
SELECT     TOP (1000) Id, ResourceIntId, Title, IsActive, Description, Imported, Modified, Publisher, Creator, Submitter, AccessRightsId, AccessRights, Rights, DocId
FROM         [Resource.Version]
WHERE     (ResourceIntId IN
                          (SELECT     r.Id
                            FROM          Resource AS r INNER JOIN
                                                   [Resource.Version] AS rv ON r.Id = rv.ResourceIntId
                            WHERE      (r.IsActive = 1 AND rv.IsActive = 1)
                            GROUP BY r.Id
                            HAVING      (COUNT(*) > 1)
							)
)
ORDER BY ResourceIntId, Id DESC

  
--=======================================================================


EXECUTE [dbo].[ResourceVersion_InactivateDups] 1000, 1

EXECUTE [dbo].[ResourceVersion_InactivateDups] 50000, 1
go
EXECUTE [dbo].[ResourceVersion_InactivateDups] 600, 0, 1, 1

*/

/*
cleanup Versions

*/
CREATE PROCEDURE [dbo].[ResourceVersion_InactivateDups]
            @MaxRecords int
			,@DoingUpdate bit
As
begin 
          
Declare 
@ShowingAllRecords bit
,@cntr int

,@KeysCount int
,@StartingKeyId int
,@interval int
,@debugLevel int
,@affectedCount int
,@totalCount int
,@PrevResourceIntId int
,@BaseId int
,@BaseDesc varchar(max)
,@BaseTitle varchar(300)
,@Id int
,@ResourceIntId int
,@Title varchar(300)
,@Desc varchar(max)
,@IsActive bit
,@HoldDesc varchar(max)
,@HoldTitle varchar(300)

set @ShowingAllRecords= 0
set @interval= 25
set @cntr= 0
--set @clusterId = 91

set @BaseId = 0
set @PrevResourceIntId= 0
set @HoldTitle = ''
set @HoldDesc = ''

--set @DoingUpdate = 0
set @debugLevel= 10
set @affectedCount= 0
set @totalCount= 0

if @MaxRecords > 10000
	set @ShowingAllRecords= 0

-- ===============================================
select 'started',  getdate()
	-- Loop thru and call proc
	DECLARE thisCursor CURSOR FOR
      SELECT  Id, ResourceIntId, Title, [Description], IsActive
		FROM [Resource.Version]
		where 
		ResourceIntId IN
        (	SELECT     r.Id
            FROM Resource AS r 
			INNER JOIN [Resource.Version] AS rv ON r.Id = rv.ResourceIntId
            WHERE      (r.IsActive = 1 AND rv.IsActive = 1)
            GROUP BY r.Id
            HAVING      (COUNT(*) > 1)
		)
		ORDER BY ResourceIntId, Id DESC

	OPEN thisCursor
	FETCH NEXT FROM thisCursor INTO @Id, @ResourceIntId, @Title,@Desc,@IsActive
	WHILE @@FETCH_STATUS = 0 BEGIN
		set @cntr = @cntr+ 1
		if @MaxRecords > 0 AND @cntr > @MaxRecords begin
			print '### Early exit based on @MaxRecords = ' + convert(varchar, @MaxRecords)
			select 'exiting',  getdate()
			set @cntr = @cntr - 1
			BREAK
			End	  

		if  @PrevResourceIntId <> @ResourceIntId begin
			-- NOTE rvId sorts desc, so keep first one - no action
			print convert(varchar, @cntr)	
				+ '. New-ResId: ' + convert(varchar, @ResourceIntId) 	
				+ '. Last-Id: ' + convert(varchar, @Id) 	
				+ '. Title: ' + @Title	

			set @PrevResourceIntId = @ResourceIntId
			set @BaseId = @Id
			end
		else begin
			if @ShowingAllRecords = 1 begin
				print convert(varchar, @cntr)	
					+ '. Nxt-ResId: ' + convert(varchar, @ResourceIntId) 	
					+ '. Next-Id: ' + convert(varchar, @Id) 	
					+ '. Title: ' + @Title	
				end

			if @DoingUpdate= 1 begin
				UPDATE [dbo].[Resource.Version]
					SET [IsActive] = 0
				where Id = @Id
							
				set @totalCount= @totalCount+1
			end
		end

		FETCH NEXT FROM thisCursor INTO @Id, @ResourceIntId, @Title,@Desc,@IsActive
	END

	CLOSE thisCursor
	DEALLOCATE thisCursor
	select 'completed',  getdate()
	select 'processed records: ' + convert(varchar, @cntr)
	select 'Versions inactivated: ' + convert(varchar, @totalCount)

end

GO
/****** Object:  StoredProcedure [dbo].[ResourceVersion_SetActiveState]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Create date: 6/5/2012
-- Description:	Update Resource
-- 
-- =============================================
Create PROCEDURE [dbo].[ResourceVersion_SetActiveState]
	@Id int, 
	@IsActive bit

AS

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	UPDATE [Resource.Version]
	SET IsActive = @IsActive
	WHERE Id = @Id
END


GO
/****** Object:  StoredProcedure [dbo].[ResourceVersionClean]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
--=======================================================================
--start: 1769679
select count(*) FROM [dbo].[Resource.Version]
--=======================================================================
SELECT     TOP (1000) Id, ResourceIntId, Title, IsActive, Description, Imported, Modified, Publisher, Creator, Submitter, AccessRightsId, AccessRights, Rights, DocId
FROM         [Resource.Version]
WHERE     (ResourceIntId IN
                          (SELECT     r.Id
                            FROM          Resource AS r INNER JOIN
                                                   [Resource.Version] AS rv ON r.Id = rv.ResourceIntId
                            WHERE      (r.IsActive = 1 AND rv.IsActive = 1)
                            GROUP BY r.Id
                            HAVING      (COUNT(*) > 1)
							)
)
ORDER BY ResourceIntId, Id DESC

  where title = 'Chameleons: Masters of Disguise!'
--=======================================================================


EXECUTE [dbo].[ResourceVersionClean] 1, 3562, 1, 1

EXECUTE [dbo].[ResourceVersionClean] 50000, 0, 1, 1
go
EXECUTE [dbo].[ResourceVersionClean] 600, 0, 1, 1

*/

/*
cleanup Versions

*/
Create PROCEDURE [dbo].[ResourceVersionClean]
            @MaxRecords int
            ,@ResId int
			,@DoingBackup bit
			,@DoingUpdate bit
As
begin 
          
Declare 
--@DoingBackup bit
--,@DoingUpdate bit
@cntr int

,@KeysCount int
,@StartingKeyId int
,@interval int
,@debugLevel int
,@affectedCount int
,@totalCount int
,@PrevResourceIntId int
,@BaseId int
,@BaseDesc varchar(max)
,@BaseTitle varchar(300)
,@Id int
,@ResourceIntId int
,@Title varchar(300)
,@Desc varchar(max)
,@IsActive bit
,@HoldDesc varchar(max)
,@HoldTitle varchar(300)


set @interval= 25
set @cntr= 0
--set @clusterId = 91

if @ResId= 0	set @ResId= NULL
set @BaseId = 0
set @PrevResourceIntId= 0
set @HoldTitle = ''
set @HoldDesc = ''
--set @DoingBackup= 1
--set @DoingUpdate = 0
set @debugLevel= 10
set @affectedCount= 0
set @totalCount= 0

-- ===============================================
select 'started',  getdate()
	-- Loop thru and call proc
	DECLARE thisCursor CURSOR FOR
      SELECT  Id, ResourceIntId, Title, [Description], IsActive
			--, Imported, Modified, 
			--Publisher, Creator, Submitter, 
			--AccessRightsId, AccessRights, Rights, DocId

		FROM [Resource.Version]
		where 
			--(ResourceIntId = @ResId or @ResId is null )
		--OR 
		ResourceIntId IN
        (	SELECT     r.Id
            FROM Resource AS r 
			INNER JOIN [Resource.Version] AS rv ON r.Id = rv.ResourceIntId
            WHERE      (r.IsActive = 1 AND rv.IsActive = 1)
            GROUP BY r.Id
            HAVING      (COUNT(*) > 1)
		)
		ORDER BY ResourceIntId, Id DESC

	OPEN thisCursor
	FETCH NEXT FROM thisCursor INTO @Id, @ResourceIntId, @Title,@Desc,@IsActive
	WHILE @@FETCH_STATUS = 0 BEGIN
		set @cntr = @cntr+ 1
		if @MaxRecords > 0 AND @cntr > @MaxRecords begin
			print '### Early exit based on @MaxRecords = ' + convert(varchar, @MaxRecords)
			select 'exiting',  getdate()
			set @cntr = @cntr - 1
			BREAK
			End	  

		--if @PrevResourceIntId = 0 set @PrevResourceIntId = @ResourceIntId
		--if @BaseId = 0 set @BaseId = @Id

		if  @PrevResourceIntId <> @ResourceIntId begin
			-- update prev
			if @DoingUpdate= 1 AND @BaseId > 0 begin
				UPDATE [dbo].[Resource.Version]
					SET Title = @HoldTitle, [Description] = @HoldDesc
				where Id = @BaseId
				end

			print convert(varchar, @cntr)	
				+ '. New-ResId: ' + convert(varchar, @ResourceIntId) 	
				+ '. Id: ' + convert(varchar, @Id) 	
				+ '. Title: ' + @Title	
				+ '. IsActve: ' + convert(varchar, @IsActive)

			set @HoldTitle = isnull(@Title,'')
			set @HoldDesc = isnull(@Desc,'')
			set @PrevResourceIntId = @ResourceIntId
			set @BaseId = @Id
			end
		else begin
		
			print convert(varchar, @cntr)	
				+ '. ResId: ' + convert(varchar, @ResourceIntId) 	
				+ '. Next-Id: ' + convert(varchar, @Id) 	
				+ '. Title: ' + @Title	
				+ '. IsActve: ' + convert(varchar, @IsActive)

			if len(isnull(@Title,'')) > len(@HoldTitle) 
				set @HoldTitle = @Title

			if len(isnull(@Desc,'')) > len(@HoldDesc) 
				set @HoldDesc = @Desc

			if @DoingUpdate= 1 begin
				UPDATE [dbo].[Resource.Version]
					SET [IsActive] = 0
				where Id = @Id

				if @DoingBackup= 1 begin
					--DELETE FROM [dbo].[Resource.Version]
					--  where [ResourceIntId]= @ResourceIntId
					--	  and Id > @StartingKeyId

					INSERT INTO [dbo].[Resource.Version_Extras]
					   (Id, [ResourceIntId],[Title] )
					SELECT [Id]	,[ResourceIntId],[Title] 
						FROM [dbo].[Resource.Version]
						where Id = @Id
					end
				end
							
				set @totalCount= @totalCount+1
			end


		FETCH NEXT FROM thisCursor INTO @Id, @ResourceIntId, @Title,@Desc,@IsActive
	END

	if @DoingUpdate= 1 AND @BaseId > 0 begin
		UPDATE [dbo].[Resource.Version]
			SET Title = @HoldTitle, [Description] = @HoldDesc
		where Id = @BaseId
		end

	CLOSE thisCursor
	DEALLOCATE thisCursor
	select 'completed',  getdate()
  select 'processed records: ' + convert(varchar, @cntr)
  select 'Versions inactivated: ' + convert(varchar, @totalCount)
  
  
end


GO
/****** Object:  StoredProcedure [dbo].[Rubric.NodeTreeGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 2014-04-23
-- Description:	Retrieve a rubric node and all of its descendant nodes
-- =============================================
CREATE PROCEDURE [dbo].[Rubric.NodeTreeGet]
	@RootNodeId int,
	@RootNodeDotNotation varchar(50),
	@RootNodePurl varchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @RootNodeId = 0 SET @RootNodeId = NULL
	IF @RootNodeDotNotation = '' SET @RootNodeDotNotation = NULL
	IF @RootNodePurl = '' SET @RootNodePurl = NULL

	CREATE TABLE #workTable (Id int, ParentId int, Notation varchar(50), pUrl varchar(100), [Description] varchar(2000), Title varchar(500), Sequence int)
	DECLARE @lastPassRows int, @currentPassRows int, @parentId int
	SET @lastPassRows = 0

	INSERT INTO #workTable (Id, ParentId, Notation, pUrl, [Description], Title, Sequence)
	SELECT Id, ParentId, Notation, pUrl, [Description], ShortDescription AS Title, Sequence
	FROM [Rubric.Node]
	WHERE (Id = @RootNodeId OR @RootNodeId IS NULL) AND
		(Notation = @RootNodeDotNotation OR @RootNodeDotNotation IS NULL) AND
		(pUrl = @RootNodePurl OR @RootNodePurl IS NULL)
	SELECT @currentPassRows = COUNT(*)
	FROM #workTable
	PRINT 'lastPassRows='+convert(varchar,@lastPassRows)+' currentPassRows='+convert(varchar,@currentPassRows)

	WHILE @lastPassRows <> @currentPassRows BEGIN
		DECLARE parentCursor CURSOR FOR
		SELECT Id FROM #workTable
		WHERE Id NOT IN (SELECT parentId FROM #workTable WHERE parentId IS NOT NULL)

		OPEN parentCursor
		FETCH NEXT FROM parentCursor INTO @parentId
		WHILE @@FETCH_STATUS = 0 BEGIN
			PRINT 'parentId='+convert(varchar,@parentId)
			INSERT INTO #workTable (Id, ParentId, Notation, pUrl, [Description], Title, Sequence)
			SELECT Id, ParentId, Notation, pUrl, [Description], ShortDescription AS Title, Sequence
			FROM [Rubric.Node]
			WHERE ParentId = @parentId

			FETCH NEXT FROM parentCursor INTO @parentId
		END

		CLOSE parentCursor
		DEALLOCATE parentCursor
		SET @lastPassRows = @currentPassRows
		SELECT @currentPassRows = COUNT(*)
		FROM #workTable
	END

	SELECT *
	FROM #workTable
END

GO
/****** Object:  StoredProcedure [dbo].[SendLibraryFollowerEmail]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 7/16/2013
-- Description:	Sends email notifications of resources recently added to followed libraries
--
-- Debug levels:
-- 10 = Send email to admin address instead of actual recipient.
-- 11 = Same as 10 plus skip the update of the System.Process record
-- 12 = Send *NO* e-mail and skip update of the System.Process record
-- =============================================
CREATE PROCEDURE [dbo].[SendLibraryFollowerEmail]
	@code varchar(50),
	@subscriptionTypeId int,
	@debug int = 1,
	@adminAddress varchar(50) = 'jgrimmer@siuccwd.com'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Get last run date
	DECLARE @sysprocId int, @sysprocLastRun datetime
	SELECT @sysprocId = Id, @sysprocLastRun = LastRunDate
	FROM [System.Process]
	WHERE Code = @code
	DECLARE @LastRunDate datetime
	SET @LastRunDate = GETDATE()
	
	-- Get email notice
	DECLARE @Subject varchar(100), @HtmlBodyTemplate nvarchar(max)
	SELECT @Subject = [Subject], @HtmlBodyTemplate = HtmlBody
	FROM EmailNotice
	WHERE NoticeCode = 'LibraryNotification'
	
	DECLARE @HtmlBody nvarchar(max), @Data nvarchar(max), @LibraryTemplate nvarchar(100), @CollectionTemplate nvarchar(100), @ResourceTemplate nvarchar(500)
	SET @LibraryTemplate = '<h1>Library: @LibraryName</h1>'
	SET @CollectionTemplate = '<h2>Collection: @CollectionName</h2>'
	SET @ResourceTemplate = '<a href="http://ioer.ilsharedlearning.org/ResourceDetail.aspx?vid=@VersionId">@ResourceTitle</a><br />@Description<br /><br />'
	
	DECLARE @UserId int, @FullName varchar(101), @Email varchar(100), @LibraryId int, @LibraryTitle varchar(200),
		@CollectionId int, @CollectionTitle varchar(100),
		@ResourceVersionId int, @ResourceTitle varchar(300), @ResourceDescription varchar(max),
		@HoldUserId int, @HoldEmail varchar(100), @HoldLibraryId int, @HoldCollectionId int,
		@FirstTimeThru bit, @DoLibraryBreak bit, @DoCollectionBreak bit, @EmailsSent int, @BatchSize int
	SET @FirstTimeThru = 'True'
	SET @DoLibraryBreak = 'False'
	SET @DoCollectionBreak = 'False'
	SET @EmailsSent = 0
	SET @BatchSize = 2000
		
	-- Loop through subscribers
	PRINT 'Looping through subscribers'
	DECLARE dataCursor CURSOR FOR
		SELECT MIN(libsub.UserId) AS UserId, FullName, Email, als.Id AS LibraryId, als.Title AS LibraryTitle,
			libsec.Id AS CollectionId, libsec.Title AS CollectionTitle,
			ResourceVersionId, libres.Title AS ResourceTitle, libres.[Description]
		FROM [IsleContent].dbo.[Library.Subscription] libsub
		INNER JOIN [Patron_Summary] patsum ON libsub.UserId = patsum.Id
		INNER JOIN [ActiveLibrarySummary] als ON libsub.LibraryId = als.Id
		INNER JOIN [Library.Section] libsec ON als.Id = libsec.LibraryId
		INNER JOIN [Library.Resource] libres ON libsec.Id = libres.LibrarySectionId
		INNER JOIN [Resource.Version] vers ON libres.ResourceVersionId = vers.Id
		LEFT JOIN [Resource.Link] rlink ON libres.ResourceIntId = rlink.ResourceIntId
		WHERE als.LastUpdated >= @sysprocLastRun AND libres.Created >= @sysprocLastRun AND rlink.IsDeleted <> 'True' AND vers.IsActive = 'True'
		GROUP BY FullName, Email, als.Id, als.Title, libsec.Id, libsec.Title, ResourceVersionId, libres.Title, libres.Description
		ORDER BY UserId, als.Title, als.Id, libsec.Title, libsec.Id, libres.Title
	OPEN dataCursor
	FETCH NEXT FROM dataCursor INTO @UserId, @FullName, @Email, @LibraryId, @LibraryTitle,
		@CollectionId, @CollectionTitle, @ResourceVersionId, @ResourceTitle, @ResourceDescription
	
	WHILE @@FETCH_STATUS = 0 BEGIN
		PRINT '@FullName='+@FullName
		PRINT '@Email='+@Email
		PRINT '@UserId='+convert(varchar,@UserId)
		PRINT '@LibraryTitle='+@LibraryTitle
		PRINT '@CollectionTitle'+@CollectionTitle
		PRINT ''
		-- Do First Record special processing
		IF @FirstTimeThru = 'True' BEGIN
			SET @HtmlBody = @HtmlBodyTemplate
			SET	@HtmlBody = REPLACE(@HtmlBody,'@FullName',@FullName)
			SET @HoldEmail = @Email
			SET @HoldUserId = @UserId
			SET @Data = @LibraryTemplate+@CollectionTemplate
			SET @Data = REPLACE(@Data,'@LibraryName',@LibraryTitle)
			SET @Data = REPLACE(@Data,'@CollectionName',@CollectionTitle)
			SET @HoldUserId = @UserId
			SET @HoldLibraryId = @LibraryId
			SET @HoldCollectionId = @CollectionId
			SET @FirstTimeThru = 'False'
		END
		
		-- Do @UserId control break processing
		IF @UserId <> @HoldUserId BEGIN
			SET @DoLibraryBreak = 'True'
			SET @HtmlBody = REPLACE(@HtmlBody,'@ResourceList',@Data)
			IF @debug > 11 BEGIN
				PRINT 'Skipping Email for  <'+@HoldEmail+'>: '+@HtmlBody
			END ELSE IF @debug > 9 BEGIN
				PRINT 'Skipping Email for '+@HoldEmail+'> - Sending to '+@adminAddress+': '+@HtmlBody
				EXEC msdb.dbo.sp_send_dbmail
					@profile_name = 'DoNotReply-ILSharedLearning',
					@recipients=@adminAddress,
					@subject=@Subject,
					@body=@HtmlBody,
					@body_format='HTML'
			END ELSE BEGIN
				PRINT 'Sending Email to '+@HoldEmail+': '+@HtmlBody
				EXEC msdb.dbo.sp_send_dbmail
					@profile_name='DoNotReply-ILSharedLearning',
					@recipients=@HoldEmail,
					@subject=@Subject,
					@body=@HtmlBody,
					@body_format='HTML'
			END
			SET @HtmlBody = @HtmlBodyTemplate
			SET	@HtmlBody = REPLACE(@HtmlBody,'@FullName',@FullName)
			SET @HoldEmail = @Email
			SET @HoldUserId = @UserId
			SET @Data = ''
			
			SET @EmailsSent = @EmailsSent + 1
			IF @EmailsSent % @BatchSize = 0 BEGIN
				WAITFOR DELAY '00:01:00'
				EXEC msdb.dbo.sysmail_delete_mailitems_sp @sent_status='sent'
			END
		END
		
		-- Do @LibraryId control break processing
		IF @LibraryId <> @HoldLibraryId OR @DoLibraryBreak = 'True' BEGIN
			SET @DoLibraryBreak = 'False'
			SET @DoCollectionBreak = 'True'
			SET @Data = @Data+@LibraryTemplate
			SET @Data = REPLACE(@Data,'@LibraryName',@LibraryTitle)
			SET @HoldLibraryId = @LibraryId
		END
		
		-- Do @CollectionId control break processing
		IF @CollectionId <> @HoldCollectionId OR @DoCollectionBreak = 'True' BEGIN
			SET @DoCollectionBreak = 'False'
			SET @Data = @Data+@CollectionTemplate
			SET @Data = REPLACE(@Data,'@CollectionName',@CollectionTitle)
			SET @HoldCollectionId = @CollectionId
		END
		
		-- Do Resource processing
		SET @Data = @Data+@ResourceTemplate
		SET @Data = REPLACE(@Data,'@VersionId',@ResourceVersionId)
		SET @Data = REPLACE(@Data,'@ResourceTitle',@ResourceTitle)
		SET @Data = REPLACE(@Data,'@Description',@ResourceDescription)
		
		FETCH NEXT FROM dataCursor INTO @UserId, @FullName, @Email, @LibraryId, @LibraryTitle,
			@CollectionId, @CollectionTitle, @ResourceVersionId, @ResourceTitle, @ResourceDescription
	END
	CLOSE dataCursor
	DEALLOCATE dataCursor
	SET @HtmlBody = REPLACE(@HtmlBody,'@ResourceList',@Data)
	IF @debug > 11 BEGIN
		PRINT 'Skipping Email for  <'+@HoldEmail+'>: '+@HtmlBody
	END ELSE IF @debug > 9 BEGIN
		PRINT 'Skipping Email for '+@HoldEmail+'> - Sending to '+@adminAddress+': '+@HtmlBody
		EXEC msdb.dbo.sp_send_dbmail
			@profile_name = 'DoNotReply-ILSharedLearning',
			@recipients=@adminAddress,
			@subject=@Subject,
			@body=@HtmlBody,
			@body_format='HTML'
	END ELSE BEGIN
		PRINT 'Sending Email to '+@HoldEmail+': '+@HtmlBody
		EXEC msdb.dbo.sp_send_dbmail
			@profile_name='DoNotReply-ILSharedLearning',
			@recipients=@HoldEmail,
			@subject=@Subject,
			@body=@HtmlBody,
			@body_format='HTML'
	END

	-- Cleanup emails already sent
	WAITFOR DELAY '00:01:00'
	EXEC msdb.dbo.sysmail_delete_mailitems_sp @sent_status='sent'
	
	-- Update System.Process record if applicable
	IF @debug < 11 BEGIN
		UPDATE [System.Process]
		SET LastRunDate = @LastRunDate
		WHERE Code = @code
	END	
END

GO
/****** Object:  StoredProcedure [dbo].[StandardBody.NodeGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
 [StandardBody.NodeGet] 1, '', ''
[StandardBody.NodeGet] 0, '', ''

[StandardBody.NodeGet] 0, 'http://asn.jesandco.org/resources/S114343C','',''

[StandardBody.NodeGet] 0,'','','http://purl.org/ASN/resources/S114343D'


*/
CREATE PROCEDURE [dbo].[StandardBody.NodeGet]
    @Id int,
    @StandardUrl varchar(200),
    @NotationCode varchar(200),
    @AltUrl varchar(200)
As
if @Id = 0 set @Id = NULL
If @NotationCode = ''		SET @NotationCode = NULL 
if @StandardUrl = '' set @StandardUrl = NULL
If @AltUrl = ''		SET @AltUrl = NULL 

SELECT     
    Id, 
    ParentId, 
    LevelType, 
    NotationCode, 
    Description, 
    StandardUrl, 
    AltUrl, 
    StandardGuid, GradeLevels,
    WarehouseTotal
FROM [StandardBody.Node]
WHERE 
    (Id = @Id OR @Id is null)
and (StandardUrl = @StandardUrl OR @StandardUrl is null)
and (NotationCode = @NotationCode OR @NotationCode is null)
and (AltUrl = @AltUrl OR @AltUrl is null)


GO
/****** Object:  StoredProcedure [dbo].[StandardBody.NodeGradeLevelInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StandardBody.NodeGradeLevelInsert]
            @ParentId int, 
            @LevelType varchar(25), 
            @GradeLevelId int
As
If @ParentId = 0   SET @ParentId = NULL 
If @LevelType = ''   SET @LevelType = NULL 
If @GradeLevelId = 0   SET @GradeLevelId = NULL 
INSERT INTO [StandardBody.NodeGradeLevel] (

    ParentId, 
    GradeLevel, 
    GradeLevelId
)
Values (

    @ParentId, 
    @LevelType, 
    @GradeLevelId
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[StandardBody.NodeInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StandardBody.NodeInsert]
            @ParentId int, 
            @LevelType varchar(50), 
            @NotationCode varchar(50), 
            @Description varchar(MAX), 
            @StandardUrl varchar(200), 
            @AltUrl varchar(200), 
            @StandardGuid varchar(50)
As
If @ParentId = 0   SET @ParentId = NULL 
If @LevelType = ''   SET @LevelType = NULL 
If @NotationCode = ''   SET @NotationCode = NULL 
If @Description = ''   SET @Description = NULL 
If @StandardUrl = ''   SET @StandardUrl = NULL 
If @AltUrl = ''   SET @AltUrl = NULL 
If @StandardGuid = ''   SET @StandardGuid = NULL 

INSERT INTO [StandardBody.Node] (

    ParentId, 
    LevelType, 
    NotationCode, 
    Description, 
    StandardUrl, 
    AltUrl, 
    StandardGuid
)
Values (

    @ParentId, 
    @LevelType, 
    @NotationCode, 
    @Description, 
    @StandardUrl, 
    @AltUrl, 
    @StandardGuid
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[StandardBody.NodeSelect]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

[StandardBody.NodeSelect] 2501

*/
CREATE PROCEDURE [dbo].[StandardBody.NodeSelect]
	@ParentId int

As
SELECT 
    Id, 
    ParentId, 
    LevelType, 
    NotationCode, 
    Description, 
    StandardUrl, 
    AltUrl, 
    StandardGuid, 
	GradeLevels,
    WarehouseTotal
FROM [StandardBody.Node]
where ParentId= @ParentId
Order by ParentId, NotationCode

GO
/****** Object:  StoredProcedure [dbo].[StandardBody.SubjectGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
[StandardBody.SubjectGet] 1, 0, ''

*/
CREATE PROCEDURE [dbo].[StandardBody.SubjectGet]
    @Id int,
    @StandardBodyId int, 
    @Title varchar(200) 
As

If @StandardBodyId = 0   SET @StandardBodyId = NULL 
If @Title = ''   SET @Title = NULL 
If @Id = 0   SET @Id = NULL 


SELECT     Id, 
    StandardBodyId, 
    Title, 
    Description, 
    Url, 
    Language
FROM [StandardBody.Subject]
WHERE 
    (Id = @Id OR @Id is null)
AND (StandardBodyId = @StandardBodyId OR @StandardBodyId is null)    
AND (Title = @Title OR @Title is null)


GO
/****** Object:  StoredProcedure [dbo].[StandardBody.SubjectInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StandardBody.SubjectInsert]
            @StandardBodyId int, 
            @Title varchar(200), 
            @Description varchar(MAX), 
            @Url varchar(200), 
            @Language varchar(200)
As

If @Title = ''   SET @Title = NULL 
If @Description = ''   SET @Description = NULL 
If @Url = ''   SET @Url = NULL 
If @Language = ''   SET @Language = 'en' 

INSERT INTO [StandardBody.Subject] (

    StandardBodyId, 
    Title, 
    Description, 
    Url, 
    Language
)
Values (

    @StandardBodyId, 
    @Title, 
    @Description, 
    @Url, 
    @Language
)
 
select SCOPE_IDENTITY() as Id

GO
/****** Object:  StoredProcedure [dbo].[StandardBody.SubjectStandardConnectorInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 --[Standard.SubjectStandardConnectorInsert]
Create PROCEDURE [dbo].[StandardBody.SubjectStandardConnectorInsert]
            @StandardSubjectId int, 
            @DomainNodeId int
As

If @StandardSubjectId = 0   SET @StandardSubjectId = NULL 
If @DomainNodeId = 0        SET @DomainNodeId = NULL 


INSERT INTO [Standard.SubjectStandardConnector] (
  StandardSubjectId, 
  DomainNodeId 
)
Values (
  @StandardSubjectId, 
  @DomainNodeId 
)


GO
/****** Object:  StoredProcedure [dbo].[System.ProcessGet]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 1/22/2013
-- Description:	Retrieve a System.Process row by Id
-- Modifications
-- 13-11-15 mparsons - added string and int parms
-- =============================================
CREATE PROCEDURE [dbo].[System.ProcessGet]
	@Id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, Code, Title, [Description], StringParameter, IntParameter, 
		Created, CreatedBy, LastUpdated, LastUpdatedBy,
		LastRunDate
	FROM [System.Process]
	WHERE Id = @Id
END


GO
/****** Object:  StoredProcedure [dbo].[System.ProcessGetByCode]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 1/22/2013
-- Description:	Retrieve a System.Process row by Code
-- Modifications
-- 13-11-15 mparsons - added string and int parms
-- =============================================
CREATE PROCEDURE [dbo].[System.ProcessGetByCode]
	@Code varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Id, Code, Title, [Description], StringParameter, IntParameter, 
		Created, CreatedBy, LastUpdated, LastUpdatedBy,
		LastRunDate
	FROM [System.Process]
	WHERE Code = @Code
END


GO
/****** Object:  StoredProcedure [dbo].[System.ProcessInsert]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 1/22/2013
-- Description:	Create a System.Process row
-- =============================================
CREATE PROCEDURE [dbo].[System.ProcessInsert]
	@Code varchar(50),
	@Title varchar(100),
	@Description varchar(1000),
	@CreatedBy varchar(75),
	@LastRunDate datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	IF @Code = '' SET @Code = NULL
	IF @Title = '' SET @Title = NULL
	IF @Description = '' SET @Description = NULL
	IF @CreatedBy = '' SET @CreatedBy = NULL
	IF @LastRunDate = '' SET @LastRunDate = NULL

    INSERT INTO [System.Process] (Code, Title, [Description], Created, CreatedBy,
		LastUpdated, LastUpdatedBy, LastRunDate)
	VALUES (@Code, @Title, @Description, GETDATE(), @CreatedBy, GETDATE(), @CreatedBy, @LastRunDate)
	
	SELECT @@IDENTITY AS Id
END


GO
/****** Object:  StoredProcedure [dbo].[System.ProcessUpdate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 1/22/2013
-- Description:	Update a System.Process row
-- =============================================
CREATE PROCEDURE [dbo].[System.ProcessUpdate]
	@Id int,
	@Title varchar(100),
	@Description varchar(1000),
	@LastUpdatedBy varchar(75),
	@LastRunDate datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE [System.Process]
	SET Title = @Title,
		[Description] = @Description,
		LastUpdated = GETDATE(),
		LastUpdatedBy = @LastUpdatedBy,
		LastRunDate = @LastRunDate
	WHERE Id = @Id
END


GO
/****** Object:  StoredProcedure [dbo].[System.ProcessUpdateLastRun]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Description:	Update a System.Process row after an invocation
-- =============================================
Create PROCEDURE [dbo].[System.ProcessUpdateLastRun]
	@Id int,
	@StringParameter varchar(100),
	@IntParameter int,
	@LastUpdatedBy varchar(75)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE [System.Process]
	SET StringParameter = @StringParameter,
		IntParameter = @IntParameter,
		LastUpdated = GETDATE(),
		LastUpdatedBy = @LastUpdatedBy,
		LastRunDate = getdate()
	WHERE Id = @Id
END


GO
/****** Object:  StoredProcedure [dbo].[SystemProxies_DeleteInActivate]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
select *   from [System.GenerateLoginId]
WHERE (IsActive = 1) and ExpiryDate < getdate()

[dbo].[SystemProxies_DeleteInActivate]

*/

-- =============================================
-- Description:	Delete all inactive rows
-- Modifications
-- =============================================
CREATE PROCEDURE [dbo].[SystemProxies_DeleteInActivate]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
Delete [System.GenerateLoginId]
WHERE (IsActive = 0) 
END



GO
/****** Object:  StoredProcedure [dbo].[SystemProxies_InactivateExpired]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
select *   from [System.GenerateLoginId]
WHERE (IsActive = 1) and ExpiryDate < getdate()

[dbo].[SystemProxies_InactivateExpired]

*/

-- =============================================
-- Description:	Set all rows to inactive where expiry date is in the past 
-- Modifications
-- =============================================
CREATE PROCEDURE [dbo].[SystemProxies_InactivateExpired]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
Update         [System.GenerateLoginId]
set IsActive = 0
WHERE (IsActive = 1) and ExpiryDate < getdate()
END



GO
/****** Object:  StoredProcedure [dbo].[TestTempTableUse]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 
exec [TestTempTableUse] 'Patron'

CREATE TABLE #workTable (
		versionID int
		)
insert into #workTable (versionID) values( 1)
insert into #workTable (versionID) values( 2)
insert into #workTable (versionID) values( 3)
insert into #workTable (versionID) values( 4)


exec [TestTempTableUse] '#workTable'



*/


CREATE PROCEDURE [dbo].[TestTempTableUse]
		@tablename varchar(50)
		,@colname varchar(50)
		,@categoryId int
		--,@tablename varchar(50)


As
declare @sql varchar(2000)
--set @sql = 'select * from ' + @tablename

set @sql = 'UPDATE @tablename
SET @colnameIDs = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.TagValueId)+'',''
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('''')) RelatedList
		FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.@colnameIDs <> rs.relatedTags OR
	(Resource_IndexV2.@colnameIDs IS NULL AND rs.relatedTags IS NOT NULL))

-- titles
UPDATE Resource_IndexV2
SET @colnameList = rs.relatedTags,
	LastUpdated = GETDATE()
FROM (SELECT ResourceIntId, left(RelatedList, LEN(RelatedList) - 1) AS relatedTags
	FROM (SELECT ResourceIntId, (
		SELECT  itbl.TagTitle + '',''
		FROM [Resource_TagSummary] itbl
		WHERE itbl.CategoryId = @categoryId AND itbl.ResourceIntId = tbl.ResourceIntId 
		for xml path('''')) RelatedList
	FROM [Resource_TagSummary] tbl
	WHERE tbl.CategoryId = @categoryId
	GROUP BY ResourceIntId) otab) rs
WHERE Resource_IndexV2.intID = rs.ResourceIntId AND
	(Resource_IndexV2.@colnameList <> rs.relatedTags OR
	(Resource_IndexV2.@colnameList IS NULL AND rs.relatedTags IS NOT NULL))

	'
	set @Sql = replace(@sql, '@tablename', @tablename)
	set @Sql = replace(@sql, '@colname', @colname)
	set @Sql = replace(@sql, '@categoryId', @categoryId)


print (@sql)

--exec (@sql)


GO
/****** Object:  StoredProcedure [dbo].[zzDatabaseReset]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 9/21/2012
-- Description:	Clear Resource and Report database - start over
-- =============================================
CREATE PROCEDURE [dbo].[zzDatabaseReset]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    TRUNCATE TABLE [AuditReport.Detail]
    TRUNCATE TABLE [AuditReport]
    TRUNCATE TABLE [Resource.Cluster]
    TRUNCATE TABLE [Resource.EducationLevel]
    TRUNCATE TABLE [Resource.Format]
    TRUNCATE TABLE [Resource.IntendedAudience]
    TRUNCATE TABLE [Resource.Keyword]
    TRUNCATE TABLE [Resource.Language]
    TRUNCATE TABLE [Resource.Property]
    TRUNCATE TABLE [Resource.Rating]
    TRUNCATE TABLE [Resource.RatingSummary]
    TRUNCATE TABLE [Resource.ResourceType]
    TRUNCATE TABLE [Resource.Version]
    TRUNCATE TABLE [Resource.Subject]
    DELETE FROM Resource
END

GO
/****** Object:  UserDefinedFunction [dbo].[BuildSortTitle]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 11/28/2012
-- Description:	Builds SortTitle from Title
-- =============================================
CREATE FUNCTION [dbo].[BuildSortTitle]
(
	@Title varchar(300)
)
RETURNS varchar(300)
AS
BEGIN
	
	DECLARE @RetVal varchar(300)

	SET @RetVal = @Title
	IF @RetVal LIKE 'A %' AND (@RetVal LIKE 'A[^<=>]%' AND @RetVal LIKE 'A [^<=>]%')
		SET @RetVal = RIGHT(@RetVal, len(@RetVal) - 2)
	WHILE @@ROWCOUNT > 0
		IF @RetVal LIKE 'A %' AND (@RetVal LIKE 'A[^<=>]%' AND @RetVal LIKE 'A [^<=>]%')
			SET @RetVal = RIGHT(@RetVal, len(@RetVal) - 2)
	SET @RetVal = ltrim(rtrim(dbo.StripNonalphanumericCharacters(@RetVal)))
	IF @RetVal LIKE 'An %'
		SET @RetVal = RIGHT(@RetVal, len(@RetVal) - 3)
	WHILE @@ROWCOUNT > 0
		IF @RetVal LIKE 'An %'
		SET @RetVal = RIGHT(@RetVal, len(@RetVal) - 3)
	IF @RetVal LIKE 'The %'
		SET @RetVal = RIGHT(@RetVal, len(@RetVal) - 4)
	WHILE @@ROWCOUNT > 0	
		IF @RetVal LIKE 'The %'
		SET @RetVal = RIGHT(@RetVal, len(@RetVal) - 4)
	SET @RetVal = REPLACE(@RetVal,' a ', ' ')
	SET @RetVal = REPLACE(@RetVal,' an ', ' ')
	SET @RetVal = REPLACE(@RetVal,' the ', ' ')
	SET @RetVal = LTRIM(rtrim(@RetVal))
	
	RETURN @RetVal

END


GO
/****** Object:  UserDefinedFunction [dbo].[Resource.GetImageUrl]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*

select [dbo].[Resource.GetImageUrl]('http://owl.english.purdue.edu/workshops/pp/writewithppt.ppt',71278)

*/
-- =============================================
-- Create date: 03/31/2014
-- Description:	Determine image url for a resource, handling special files like pdfs, etc
-- Modifications
-- 14-06-18 mparsons - removed swf
-- =============================================
CREATE FUNCTION [dbo].[Resource.GetImageUrl]
(
	@ResourceUrl varchar(300), 
	@ResourceIntId int
)
RETURNS varchar(300)
AS
BEGIN
	
DECLARE @ImageUrl varchar(300)
	
Declare @ResImgeUrl varchar(200)
,@PdfImageUrl varchar(200),@PPTImageUrl varchar(200),@WordImageUrl varchar(200)
,@XlxImageUrl  varchar(200)
,@SwfImageUrl  varchar(200)

--set @ResImgeUrl = '//ioer.ilsharedlearning.org/OERThumbs/thumb/@rvid-thumb.png'
set @ResImgeUrl = '//ioer.ilsharedlearning.org/OERThumbs/large/@rid-large.png'
set @PdfImageUrl = '//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_pdf_200x150.png'
set @PPTImageUrl = '//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_pptx_200x150.png'
set @WordImageUrl = '//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_docx_200x150.png'
set @XlxImageUrl = '//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_xlsx_200x150.png'
set @SwfImageUrl = '//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_swf_200x200.png'
--		when Right(rtrim(lower(@ResourceUrl)), 4) = '.swf' then @SwfImageUrl
	Select @ImageUrl =
		case 
		when Right(rtrim(lower(@ResourceUrl)), 4) = '.pdf' then @PdfImageUrl
		when charindex('.ppt',Right(rtrim(lower(@ResourceUrl)), 5)) > 0 then @PPTImageUrl
		when charindex('.doc',Right(rtrim(lower(@ResourceUrl)), 5)) > 0 then @WordImageUrl
		when charindex('.xls',Right(rtrim(lower(@ResourceUrl)), 5)) > 0 then @XlxImageUrl
		else replace(@ResImgeUrl, '@rid', @ResourceIntId) end 

	SET @ImageUrl = LTRIM(rtrim(@ImageUrl))
	
	RETURN @ImageUrl

END

GO
/****** Object:  UserDefinedFunction [dbo].[StripNonalphanumericCharacters]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 10/18/2012
-- Description:	Strips non-alphanumeric, non-space characters. Works for English, Polish, and Spanish.
--
-- 2012-10-25 jgrimmer - Added capability to drop/replace HTML entities in table with other strings
-- 2012-10-29 jgrimmer - Added capability to drop the '#' character if it is the first character in the string.
--						 It is not dropped in other locations as it is an integral part of HTML entities.
-- =============================================
CREATE FUNCTION [dbo].[StripNonalphanumericCharacters] 
(
	-- Add the parameters for the function here
	@Input nvarchar(max)
)
RETURNS nvarchar(max)
AS
BEGIN
	DECLARE @Output nvarchar(max), @CharactersToKeep nvarchar(100), @CharactersToToss nvarchar(110)

	SET @CharactersToKeep = N'0-9A-Za-zÁÉÍÓÚÑáéíóúñ&#; '
	SET @CharactersToToss = N'%[^'+@CharactersToKeep+']%'

	SET @Output = @Input
	-- Declare the return variable here
	IF(PATINDEX(@CharactersToToss,@Output) > 0)
	WHILE PATINDEX(@CharactersToToss,@Output)>0 BEGIN
		SET @Output = STUFF(@Output,PATINDEX(@CharactersToToss,@Output),1,'')
	END
	
	IF LEFT(@Output,1) = '#' BEGIN
		SET @Output = RIGHT(@Output, LEN(@Output) - 1)
	END
	
	SET @Output = LTRIM(rtrim(@Output))
	
	IF LEN(@Output) > 0 AND PATINDEX('%&%',@Output) > 0 AND PATINDEX('%&%',@Output) < PATINDEX('%;%',@Output) BEGIN
	/* There are characters present and the '&' character comes before a ';' character; it's probable there's an HTML entity
	 * here.  We need to check to see if it is one of the ones we want to replace or delete.  If so, replace/delete it from
	 * the string. */
		DECLARE @EntityNbr varchar(10), @EntityName varchar(10), @IsDrop bit, @IsReplace bit, @ReplaceWith varchar(10)
		DECLARE specialCharCursor CURSOR FOR
			SELECT EntityNbr, EntityName, IsDrop, IsReplace, ReplaceWith
			FROM [Sort.SpecialCharacter]
		OPEN specialCharCursor
		FETCH NEXT FROM specialCharCursor INTO @EntityNbr, @EntityName, @IsDrop, @IsReplace, @ReplaceWith
		
		WHILE @@FETCH_STATUS = 0 BEGIN
			IF @IsDrop = 'True' BEGIN
				IF @EntityNbr IS NOT NULL
					SET @Output = REPLACE(@Output,@EntityNbr,'')
				IF @EntityName IS NOT NULL
					SET @Output = REPLACE(@Output,@EntityName,'')
			END ELSE IF @IsReplace = 'True' BEGIN
				IF @EntityNbr IS NOT NULL
					SET @Output = REPLACE(@Output,@EntityNbr,@ReplaceWith)
				IF @EntityName IS NOT NULL
					SET @Output = REPLACE(@Output,@EntityName,@ReplaceWith)
			END
		
			FETCH NEXT FROM specialCharCursor INTO @EntityNbr, @EntityName, @IsDrop, @IsReplace, @ReplaceWith
		END
		CLOSE specialCharCursor
		DEALLOCATE specialCharCursor
	END

	IF len(@Output) = 0
		SET @Output = @Input

	RETURN @Output
END


GO
/****** Object:  UserDefinedFunction [dbo].[StripNonalphanumericCharactersDescription]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jerome Grimmer
-- Create date: 10/18/2012
-- Description:	Strips non-alphanumeric, non-space characters. Works for English, Polish, and Spanish.
--
-- 2012-10-25 jgrimmer - Added capability to drop/replace HTML entities in table with other strings
-- 2012-10-29 jgrimmer - Added capability to drop the '#' character if it is the first character in the string.
--						 It is not dropped in other locations as it is an integral part of HTML entities.
-- =============================================
CREATE FUNCTION [dbo].[StripNonalphanumericCharactersDescription] 
(
	-- Add the parameters for the function here
	@Input nvarchar(max)
)
RETURNS nvarchar(max)
AS
BEGIN
	DECLARE @Output nvarchar(max), @CharactersToKeep nvarchar(100), @CharactersToToss nvarchar(110)

	SET @CharactersToKeep = N'0-9A-Za-zÁÉÍÓÚÑáéíóúñ&#; '
	SET @CharactersToToss = N'%[^'+@CharactersToKeep+']%'

	SET @Output = @Input
	-- Declare the return variable here
	IF(PATINDEX(@CharactersToToss,@Output) > 0)
	WHILE PATINDEX(@CharactersToToss,@Output)>0 BEGIN
		SET @Output = STUFF(@Output,PATINDEX(@CharactersToToss,@Output),1,'')
	END
	
	IF LEFT(@Output,1) = '#' BEGIN
		SET @Output = RIGHT(@Output, LEN(@Output) - 1)
	END
	
	SET @Output = LTRIM(rtrim(@Output))
	
	IF LEN(@Output) > 0 AND PATINDEX('%&%',@Output) > 0 AND PATINDEX('%&%',@Output) < PATINDEX('%;%',@Output) BEGIN
	/* There are characters present and the '&' character comes before a ';' character; it's probable there's an HTML entity
	 * here.  We need to check to see if it is one of the ones we want to replace or delete.  If so, replace/delete it from
	 * the string. */
		DECLARE @EntityNbr varchar(10), @EntityName varchar(10), @IsDrop bit, @IsReplace bit, @ReplaceWith varchar(10)
		DECLARE specialCharCursor CURSOR FOR
			SELECT EntityNbr, EntityName, IsDropDescription, IsReplaceDescription, ReplaceWith
			FROM [Sort.SpecialCharacter]
			WHERE IsDropDescription = 'True' OR IsReplaceDescription = 'True'
		OPEN specialCharCursor
		FETCH NEXT FROM specialCharCursor INTO @EntityNbr, @EntityName, @IsDrop, @IsReplace, @ReplaceWith
		
		WHILE @@FETCH_STATUS = 0 BEGIN
			IF @IsDrop = 'True' BEGIN
				IF @EntityNbr IS NOT NULL
					SET @Output = REPLACE(@Output,@EntityNbr,'')
				IF @EntityName IS NOT NULL
					SET @Output = REPLACE(@Output,@EntityName,'')
			END ELSE IF @IsReplace = 'True' BEGIN
				IF @EntityNbr IS NOT NULL
					SET @Output = REPLACE(@Output,@EntityNbr,@ReplaceWith)
				IF @EntityName IS NOT NULL
					SET @Output = REPLACE(@Output,@EntityName,@ReplaceWith)
			END
		
			FETCH NEXT FROM specialCharCursor INTO @EntityNbr, @EntityName, @IsDrop, @IsReplace, @ReplaceWith
		END
		CLOSE specialCharCursor
		DEALLOCATE specialCharCursor
	END

	IF len(@Output) = 0
		SET @Output = @Input

	RETURN @Output
END


GO
/****** Object:  Table [dbo].[_Dictionary]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[_Dictionary](
	[tableName] [nvarchar](128) NOT NULL,
	[col_id] [int] NOT NULL,
	[colName] [nvarchar](128) NOT NULL,
	[datatype] [nvarchar](30) NOT NULL,
	[col_precScale] [nvarchar](50) NULL,
	[col_null] [bit] NOT NULL,
	[DefaultValue] [nvarchar](257) NULL,
	[col_identity] [bit] NULL,
	[col_desc] [nvarchar](250) NULL,
	[entityType] [nvarchar](25) NULL,
	[CreatedDate] [datetime] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[_DictTable]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[_DictTable](
	[tablename] [nvarchar](128) NOT NULL,
	[Description] [nvarchar](133) NOT NULL,
	[IsActive] [int] NOT NULL,
	[ReportGroup] [int] NOT NULL,
	[ReportOrder] [int] NOT NULL,
	[Synchronize] [int] NOT NULL,
	[Created] [datetime] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ApplicationLog]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ApplicationLog](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[AppProcedure] [varchar](50) NULL,
	[Event] [varchar](50) NULL,
	[Type] [varchar](50) NULL,
	[Comment] [varchar](1000) NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PKY_ApplicationLog] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.AccessibilityApi_Orphan]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.AccessibilityApi_Orphan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
 CONSTRAINT [PK_Audit.AccessibilityApi_Orphan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.AccessibilityControl_Orphan]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.AccessibilityControl_Orphan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
 CONSTRAINT [PK_Audit.AccessibilityControl_Orphan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.AccessibilityFeature_Orphan]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.AccessibilityFeature_Orphan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
 CONSTRAINT [PK_Audit.AccessibilityFeature_Orphan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.AccessibilityHazard_Orphan]    Script Date: 2/22/2015 10:28:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.AccessibilityHazard_Orphan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
 CONSTRAINT [PK_Audit.AccessibilityHazard_Orphan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.AssessmentType_Orphan]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.AssessmentType_Orphan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
	[ResourceRowId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Audit.AssessmentType_Orphan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.AudienceType_Orphan]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.AudienceType_Orphan](
	[RowId] [uniqueidentifier] NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
	[ResourceId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Audit.AudienceType_Orphan] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.EducationLevel_Orphan]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.EducationLevel_Orphan](
	[RowId] [uniqueidentifier] NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
	[ResourceId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Audit.EducationLevel_Orphan] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.EducationUse_Orphan]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.EducationUse_Orphan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
	[ResourceRowId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Audit.EducationUse_Orphan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.GradeLevel_Orphan]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.GradeLevel_Orphan](
	[RowId] [uniqueidentifier] NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
 CONSTRAINT [PK_Audit.GradeLevel_Orphan] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.GroupType_Orphan]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.GroupType_Orphan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
	[ResourceRowId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Audit.GroupType_Orphan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.ItemType_Orphan]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.ItemType_Orphan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
	[ResourceRowId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Audit.ItemType_Orphan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.Language_Orphan]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.Language_Orphan](
	[RowId] [uniqueidentifier] NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
	[ResourceId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Audit.Language_Orphan] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.ResourceFormat_Orphan]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.ResourceFormat_Orphan](
	[RowId] [uniqueidentifier] NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
	[ResourceId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Audit.ResourceFormat_Orphan] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.ResourceStandard_Orphan]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.ResourceStandard_Orphan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[StandardUrl] [varchar](200) NULL,
	[StandardAlignment] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
	[NotationCode] [varchar](200) NULL,
 CONSTRAINT [PK_Audit.ResourceStandard_Orphan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit.ResourceType_Orphan]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit.ResourceType_Orphan](
	[RowId] [uniqueidentifier] NOT NULL,
	[ResourceIntId] [int] NULL,
	[OriginalValue] [varchar](200) NULL,
	[FoundMapping] [bit] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastRerunDate] [datetime] NULL,
	[ResourceId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Audit.ResourceType_Orphan] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AuditReport]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditReport](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Report] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AuditReport.Detail]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AuditReport.Detail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ReportId] [int] NOT NULL,
	[Filename] [varchar](100) NULL,
	[DocID] [varchar](100) NULL,
	[URI] [varchar](500) NULL,
	[MessageType] [char](1) NULL,
	[MessageRouting] [varchar](2) NULL,
	[Message] [varchar](500) NULL,
 CONSTRAINT [PK_AuditReport.Detail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Blacklist.Hosts]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Blacklist.Hosts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Hostname] [varchar](100) NULL,
	[RecordSource] [varchar](50) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedId] [int] NULL,
 CONSTRAINT [PK_Blacklist.Hosts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Blacklist.StagingHosts]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Blacklist.StagingHosts](
	[Line] [varchar](500) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[BouncedEmails]    Script Date: 2/22/2015 10:28:08 AM ******/
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
/****** Object:  Table [dbo].[CareerCluster]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CareerCluster](
	[Id] [int] NOT NULL,
	[CareerCluster] [nvarchar](255) NOT NULL,
	[CisFileNum] [char](6) NULL,
	[IsHighGrowth] [bit] NULL,
	[IsActive] [bit] NULL,
	[NaicSect] [char](2) NULL,
	[ShortName] [varchar](25) NULL,
	[HighGrowthName] [varchar](75) NULL,
	[IsIlPathway] [bit] NULL,
	[IlPathwayName] [varchar](75) NULL,
	[prefix] [varchar](15) NULL,
	[Description] [nvarchar](255) NULL,
	[IlPathwayChannel] [varchar](75) NULL,
	[IlPathwayReportOrder] [int] NULL,
	[WarehouseTotal] [int] NULL,
 CONSTRAINT [PK_CareerCluster] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.AccessibilityApi]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.AccessibilityApi](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](255) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[schemaValue] [varchar](50) NULL,
	[SortOrder] [int] NULL,
 CONSTRAINT [PK_Codes.AccessibilityApi] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.AccessibilityControl]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.AccessibilityControl](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](255) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[schemaValue] [varchar](50) NULL,
	[SortOrder] [int] NULL,
 CONSTRAINT [PK_Codes.AccessibilityControl] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.AccessibilityFeature]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.AccessibilityFeature](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](512) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[schemaValue] [varchar](50) NULL,
	[SortOrder] [int] NULL,
 CONSTRAINT [PK_Codes.AccessibilityFeature] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.AccessibilityHazard]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.AccessibilityHazard](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](255) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[AntonymId] [int] NULL,
	[schemaValue] [varchar](50) NULL,
	[SortOrder] [int] NULL,
 CONSTRAINT [PK_Codes.AccessibilityHazard] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.AccessRights]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.AccessRights](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[SortOrder] [int] NULL,
 CONSTRAINT [PK_Codes.AccessRights] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.AlignmentDegree]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.AlignmentDegree](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
 CONSTRAINT [PK_Codes.AlignmentDegree] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.AlignmentType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.AlignmentType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](50) NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
 CONSTRAINT [PK_Codes.AlignmentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.AssessmentType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.AssessmentType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[SortOrder] [int] NULL,
 CONSTRAINT [PK_Codes.AssessmentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.AudienceType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.AudienceType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[NsdtTitle] [varchar](50) NULL,
	[Description] [varchar](255) NULL,
	[IsPathways] [bit] NULL,
	[IsActive] [bit] NULL,
	[IsPublishingRole] [bit] NULL,
	[WarehouseTotal] [int] NULL,
 CONSTRAINT [PK_ResourceAudienceType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.CareerPlanning]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.CareerPlanning](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](512) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[schemaValue] [varchar](50) NULL,
 CONSTRAINT [PK_Codes.CareerPlanning] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.Disability]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.Disability](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](512) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[schemaValue] [varchar](50) NULL,
 CONSTRAINT [PK_Codes.Disability] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.EducationalUse]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.EducationalUse](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[SortOrder] [int] NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
 CONSTRAINT [PK_Codes.EducationalUse] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.EducationalUseCategory]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.EducationalUseCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Category] [varchar](50) NULL,
	[Description] [varchar](200) NULL,
	[SortOrder] [int] NULL,
	[WarehouseTotal] [int] NULL,
 CONSTRAINT [PK_Codes.EducationalUseCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.EmployerProgram]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.EmployerProgram](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](512) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[schemaValue] [varchar](50) NULL,
 CONSTRAINT [PK_Codes.EmployerProgram] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.GradeLevel]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.GradeLevel](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[AliasValues] [varchar](100) NULL,
	[NsdlTitle] [varchar](50) NULL,
	[AgeLevel] [int] NULL,
	[Description] [varchar](500) NULL,
	[IsPathwaysLevel] [bit] NULL,
	[IsK12Level] [bit] NULL,
	[IsActive] [bit] NULL,
	[AlignmentUrl] [varchar](150) NULL,
	[SortOrder] [int] NULL,
	[WarehouseTotal] [int] NULL,
	[GradeRange] [varchar](50) NULL,
	[GradeGroup] [varchar](50) NULL,
	[PathwaysEducationLevelId] [int] NULL,
	[IsEducationBand] [bit] NULL,
	[FromAge] [int] NULL,
	[ToAge] [int] NULL,
 CONSTRAINT [PK_Codes.GradeLevel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.GroupType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.GroupType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[Description] [varchar](255) NULL,
 CONSTRAINT [PK_Codes.GroupType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.InteractivityType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.InteractivityType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
 CONSTRAINT [PK_Codes.InteractivityType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ItemType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ItemType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[SortOrder] [int] NULL,
 CONSTRAINT [PK_Codes.ItemType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.JobPreparation]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.JobPreparation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](512) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[schemaValue] [varchar](50) NULL,
 CONSTRAINT [PK_Codes.JobSearch] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.Language]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.Language](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[IsPathwaysLanguage] [bit] NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
 CONSTRAINT [PK_Codes.Language] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.LibraryMemberType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.LibraryMemberType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Codes.LibraryMemberType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.PathwaysEducationLevel]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.PathwaysEducationLevel](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[IsPathwaysLevel] [bit] NULL,
	[AlignmentUrl] [varchar](150) NULL,
	[SortOrder] [int] NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[AgeLevel] [int] NULL,
 CONSTRAINT [PK_PathwaysEducationLevel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.RatingType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.RatingType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Type] [varchar](50) NULL,
	[Identifier] [varchar](500) NULL,
	[Description] [varchar](200) NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_RatingType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ResourceFormat]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ResourceFormat](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[IsIsleCode] [bit] NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[OrigTitle] [varchar](50) NULL,
 CONSTRAINT [PK_ResourceMaterialType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ResourceType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ResourceType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[IsIsleItem] [bit] NULL,
	[IsActive] [bit] NULL,
	[SortOrder] [int] NULL,
	[WarehouseTotal] [int] NULL,
	[CategoryId] [int] NULL,
	[OrigTitle] [varchar](50) NULL,
 CONSTRAINT [PK_CodesResourceType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ResourceTypeCategory]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ResourceTypeCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Category] [varchar](50) NULL,
	[Description] [varchar](200) NULL,
	[SortOrder] [int] NULL,
	[WarehouseTotal] [int] NULL,
 CONSTRAINT [PK_Codes.ResourceTypeCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.ResPropertyType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.ResPropertyType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
 CONSTRAINT [PK_ResourcePropertyType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.SecretQuestion]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.SecretQuestion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
 CONSTRAINT [PK_ResourceSecretQuestion] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.Site]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.Site](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NOT NULL,
	[Description] [varchar](500) NULL,
	[IsActive] [bit] NOT NULL,
	[SchemaTag] [varchar](100) NULL,
	[HasStandardsBrowser] [bit] NULL,
	[CssThemes] [varchar](200) NULL,
	[ApiRoot] [varchar](100) NULL,
 CONSTRAINT [PK_Codes.Site] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.SiteTagCategory]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.SiteTagCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Title] [varchar](100) NOT NULL,
 CONSTRAINT [PK_Codes.SiteTag] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.Subject]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.Subject](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
 CONSTRAINT [PK_Codes.Subject] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.TagCategory]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.TagCategory](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NOT NULL,
	[Description] [varchar](500) NULL,
	[SortOrder] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[SchemaTag] [varchar](100) NULL,
 CONSTRAINT [PK_Codes.TagCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.TagValue]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.TagValue](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CategoryId] [int] NOT NULL,
	[CodeId] [int] NOT NULL,
	[Title] [varchar](50) NOT NULL,
	[Description] [varchar](500) NULL,
	[SortOrder] [int] NULL,
	[IsActive] [bit] NULL,
	[SchemaTag] [varchar](100) NULL,
	[WarehouseTotal] [int] NULL,
	[AliasValues] [varchar](100) NULL,
 CONSTRAINT [PK_Codes.TagValue] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.TagValueKeyword]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.TagValueKeyword](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TagValueId] [int] NOT NULL,
	[Keyword] [varchar](50) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Codes.TagValueKeyword] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.TextType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.TextType](
	[Id] [int] NOT NULL,
	[Title] [varchar](50) NOT NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Codes.TextType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.VeteransService]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.VeteransService](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](512) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[schemaValue] [varchar](50) NULL,
 CONSTRAINT [PK_Codes.VeteransService] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.WorkforcePartnerService]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.WorkforcePartnerService](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](512) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[schemaValue] [varchar](50) NULL,
 CONSTRAINT [PK_Codes.WorkforcePartnerService] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.WorkplaceSkills]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.WorkplaceSkills](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](512) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[schemaValue] [varchar](50) NULL,
 CONSTRAINT [PK_Codes.WorkplaceSkills] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Codes.WorkSupportService]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Codes.WorkSupportService](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](50) NULL,
	[Description] [varchar](512) NULL,
	[IsActive] [bit] NULL,
	[WarehouseTotal] [int] NULL,
	[schemaValue] [varchar](50) NULL,
 CONSTRAINT [PK_Codes.WorkSupportService] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CodeTable]    Script Date: 2/22/2015 10:28:08 AM ******/
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
/****** Object:  Table [dbo].[ConditionOfUse]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ConditionOfUse](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Summary] [varchar](50) NULL,
	[Title] [varchar](100) NULL,
	[Description] [varchar](800) NULL,
	[IsActive] [bit] NULL,
	[SortOrderAuthoring] [int] NULL,
	[Url] [varchar](200) NULL,
	[IconUrl] [varchar](200) NULL,
	[ConditionOfUseCategory] [int] NULL,
	[WarehouseTotal] [int] NULL,
	[MiniIconUrl] [varchar](200) NULL,
 CONSTRAINT [PK_ConditionOfUse] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[EmailNotice]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EmailNotice](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](75) NULL,
	[Category] [varchar](50) NULL,
	[NoticeCode] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[Filter] [varchar](max) NULL,
	[isActive] [bit] NULL,
	[LanguageCode] [varchar](10) NULL,
	[FromEmail] [nvarchar](100) NULL,
	[CcEmail] [nvarchar](500) NULL,
	[BccEmail] [nvarchar](100) NULL,
	[Subject] [nvarchar](100) NULL,
	[HtmlBody] [nvarchar](max) NULL,
	[TextBody] [nvarchar](max) NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](25) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](25) NULL,
 CONSTRAINT [PK_EmailNotice] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Evaluation]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Evaluation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](75) NULL,
	[ShortName] [varchar](25) NULL,
	[Url] [varchar](200) NULL,
	[Description] [varchar](max) NULL,
	[WarehouseTotal] [int] NULL,
	[RequiresCertification] [bit] NULL,
	[PrivilegeCode] [varchar](50) NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Evaluation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Evaluation.Dimension]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Evaluation.Dimension](
	[Id] [int] NOT NULL,
	[EvaluationId] [int] NOT NULL,
	[DimensionId] [int] NOT NULL,
	[Title] [varchar](75) NOT NULL,
	[ShortName] [varchar](25) NULL,
	[Url] [varchar](200) NULL,
	[Description] [varchar](max) NULL,
	[IsActive] [bit] NOT NULL,
	[WarehouseTotal] [int] NULL,
 CONSTRAINT [PK_Evaluation.Dimension] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Evaluation.DimensionCriteria]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Evaluation.DimensionCriteria](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DimensionId] [int] NOT NULL,
	[Title] [varchar](75) NOT NULL,
	[Description] [varchar](max) NOT NULL,
	[Sequence] [int] NULL,
 CONSTRAINT [PK_Evaluation.DimensionCriteria] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[EvaluationTool]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EvaluationTool](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [varchar](100) NULL,
	[Description] [varchar](max) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_EvaluationTool] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[EvaluationTool.Section]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EvaluationTool.Section](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EvalToolId] [int] NULL,
	[Title] [varchar](500) NULL,
	[Description] [varchar](max) NULL,
	[Notation] [varchar](50) NULL,
	[SchemaUrl] [varchar](100) NULL,
	[AltUrl] [varchar](300) NULL,
	[Sequence] [int] NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_EvaluationTool.Section] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[GradeToAgeRange]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[GradeToAgeRange](
	[Id] [int] NOT NULL,
	[GradeLevel] [varchar](100) NULL,
	[MinAge] [int] NULL,
	[MaxAge] [int] NULL,
 CONSTRAINT [PK_GradeToAgeRange] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.AccessibilityApi]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.AccessibilityApi](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OriginalValue] [varchar](50) NULL,
	[CodeId] [int] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
 CONSTRAINT [PK_Map.AccessibilityApi] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.AccessibilityControl]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.AccessibilityControl](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OriginalValue] [varchar](50) NULL,
	[CodeId] [int] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
 CONSTRAINT [PK_Map.AccessibilityControl] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.AccessibilityFeature]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.AccessibilityFeature](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OriginalValue] [varchar](50) NULL,
	[CodeId] [int] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
 CONSTRAINT [PK_Map.AccessibilityFeature] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.AccessibilityHazard]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.AccessibilityHazard](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OriginalValue] [varchar](50) NULL,
	[CodeId] [int] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
 CONSTRAINT [PK_Map.AccessibilityHazard] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.AgeRangeGradeLevel]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.AgeRangeGradeLevel](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OriginalValue] [varchar](500) NOT NULL,
	[FromAge] [int] NULL,
	[ToAge] [int] NULL,
	[CodeId] [int] NULL,
	[IsActive] [bit] NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Map.AgeRangeGradeLevel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.AlignmentType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.AlignmentType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LRValue] [varchar](100) NULL,
	[CodeId] [int] NULL,
	[Description] [varchar](500) NULL,
 CONSTRAINT [PK_Map.AlignmentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.AssessmentType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.AssessmentType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LRValue] [varchar](100) NULL,
	[CodeId] [int] NULL,
	[Description] [varchar](500) NULL,
 CONSTRAINT [PK_Map.AssessmentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.AudienceType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.AudienceType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OriginalValue] [varchar](500) NULL,
	[IsRegex] [bit] NULL,
	[IsCaseSensitive] [bit] NULL,
	[ImportWithoutTranslation] [bit] NULL,
	[DoNotImport] [bit] NULL,
	[MappedValue] [varchar](100) NULL,
	[CodeId] [int] NULL,
	[Sequence] [int] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
 CONSTRAINT [PK_Map.AudienceType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.CareerCluster]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.CareerCluster](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FilterValue] [varchar](500) NULL,
	[IsRegex] [bit] NULL,
	[IsCaseSensitive] [bit] NULL,
	[MappedClusterId] [int] NOT NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](50) NULL,
 CONSTRAINT [PK_Map.CareerCluster] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.CleanseUrl]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.CleanseUrl](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Host] [varchar](100) NOT NULL,
	[urlParameterToDrop] [varchar](50) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [int] NULL,
 CONSTRAINT [PK_Map.CleanseUrl] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.Cluster]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.Cluster](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LRValue] [varchar](100) NULL,
	[CodeId] [int] NULL,
	[Description] [varchar](500) NULL,
 CONSTRAINT [PK_Map.Cluster] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.EducationLevel]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.EducationLevel](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OriginalValue] [varchar](500) NULL,
	[IsRegex] [bit] NULL,
	[IsCaseSensitive] [bit] NULL,
	[ImportWithoutTranslation] [bit] NULL,
	[DoNotImport] [bit] NULL,
	[CodeId] [int] NULL,
	[Sequence] [int] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
	[MappedValue] [varchar](100) NULL,
 CONSTRAINT [PK_Map.EducationLevel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.EducationUse]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.EducationUse](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LRValue] [varchar](100) NULL,
	[CodeId] [int] NULL,
	[Description] [varchar](500) NULL,
 CONSTRAINT [PK_Map.EducationUse] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.GradeLevel]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.GradeLevel](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OriginalValue] [varchar](500) NOT NULL,
	[CodeId] [int] NULL,
	[IsActive] [bit] NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Map.GradeLevel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.GroupType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.GroupType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LRValue] [varchar](100) NULL,
	[CodeId] [int] NULL,
	[Description] [varchar](500) NULL,
 CONSTRAINT [PK_Map.GroupType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.InteractivityType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.InteractivityType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OriginalValue] [varchar](500) NULL,
	[IsRegex] [bit] NULL,
	[IsCaseSensitive] [bit] NULL,
	[ImportWithoutTranslation] [bit] NULL,
	[DoNotImport] [bit] NULL,
	[MappedValue] [varchar](100) NULL,
	[CodeId] [int] NULL,
	[Sequence] [int] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
 CONSTRAINT [PK_Map.InteractivityType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.ItemType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.ItemType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LRValue] [varchar](100) NULL,
	[CodeId] [int] NULL,
	[Description] [varchar](500) NULL,
 CONSTRAINT [PK_Map.ItemType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.K12Subject]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.K12Subject](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FilterValue] [varchar](500) NULL,
	[IsRegex] [bit] NULL,
	[MappedSubjectId] [int] NOT NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](50) NULL,
 CONSTRAINT [PK_Map.K12Subject] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.Language]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.Language](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LRValue] [varchar](300) NULL,
	[MappedValue] [varchar](30) NULL,
	[LanguageId] [int] NULL,
 CONSTRAINT [PK_Map.Language] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.PathwayRules]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.PathwayRules](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PropertyTypeId] [int] NULL,
	[OriginalValue] [varchar](500) NULL,
	[IsRegex] [bit] NULL,
	[IsCaseSensitive] [bit] NULL,
	[MappedValue] [varchar](100) NULL,
	[Sequence] [int] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
 CONSTRAINT [PK_Map.PathwayRules] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.ResourceFormat]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.ResourceFormat](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LRValue] [varchar](100) NULL,
	[CodeId] [int] NULL,
	[OldCodeId] [int] NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Map.ResourceFormat] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.ResourceType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.ResourceType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LRValue] [varchar](100) NULL,
	[CodeId] [int] NULL,
	[Description] [varchar](500) NULL,
	[OldCodeId] [int] NULL,
 CONSTRAINT [PK_Mapping.ResourceType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Map.Rules]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Map.Rules](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PropertyTypeId] [int] NULL,
	[OriginalValue] [varchar](500) NULL,
	[IsRegex] [bit] NULL,
	[IsCaseSensitive] [bit] NULL,
	[ImportWithoutTranslation] [bit] NULL,
	[DoNotImport] [bit] NULL,
	[MappedValue] [varchar](100) NULL,
	[Sequence] [int] NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
 CONSTRAINT [PK_Map.Rules] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Patron]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Patron](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [varchar](50) NOT NULL,
	[Password] [varchar](50) NULL,
	[FirstName] [varchar](50) NULL,
	[LastName] [varchar](50) NULL,
	[Email] [varchar](100) NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[IsleIdentifier] [varchar](100) NULL,
 CONSTRAINT [PK_Patron] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Patron.ExternalAccount]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Patron.ExternalAccount](
	[PatronId] [int] NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ExternalSiteId] [int] NULL,
	[LoginId] [varchar](100) NULL,
	[Password] [varchar](50) NULL,
	[Token] [varchar](50) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Patron.ExternalAccount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Patron.Following]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Patron.Following](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FollowingUserId] [int] NOT NULL,
	[FollowedByUserId] [int] NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Patron.Following] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Patron.Note]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Patron.Note](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Title] [varchar](100) NOT NULL,
	[Category] [varchar](50) NULL,
	[Description] [varchar](max) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
	[RowId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Patron.Note_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Patron.Profile]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Patron.Profile](
	[UserId] [int] NOT NULL,
	[MainPhone] [varchar](15) NULL,
	[JobTitle] [varchar](100) NULL,
	[PublishingRoleId] [int] NULL,
	[RoleProfile] [varchar](500) NULL,
	[OrganizationId] [int] NULL,
	[ImageUrl] [varchar](200) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedId] [int] NULL,
	[Notes] [varchar](1000) NULL,
 CONSTRAINT [PK_Patron.Profile] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Patron.SearchFilter]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Patron.SearchFilter](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Title] [varchar](100) NOT NULL,
	[SQL] [varchar](1000) NULL,
	[Created] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_Patron.SearchFilter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Patron.Tag]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Patron.Tag](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatronId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[TagValueId] [int] NOT NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Patron.Tag] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Publish.Pending]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Publish.Pending](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[ResourceVersionIntId] [int] NULL,
	[DocId] [varchar](500) NULL,
	[Reason] [varchar](100) NOT NULL,
	[IsPublished] [bit] NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NOT NULL,
	[PublishedDate] [datetime] NULL,
	[LrEnvelope] [varchar](max) NULL,
 CONSTRAINT [PK_Publish.Pending] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PublisherSummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PublisherSummary](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Publisher] [varchar](200) NULL,
	[IsActive] [bit] NULL,
	[ResourceTotal] [int] NULL,
	[Note] [varchar](500) NULL,
 CONSTRAINT [PK_PublisherSummary] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PurgeFromElasticSearch]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PurgeFromElasticSearch](
	[ResourceIntId] [int] NULL,
	[ResourceVersionId] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource](
	[RowId] [uniqueidentifier] NOT NULL,
	[ResourceUrl] [varchar](600) NOT NULL,
	[ViewCount] [int] NULL,
	[FavoriteCount] [int] NULL,
	[Created] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
	[IsActive] [bit] NULL,
	[HasPathwayGradeLevel] [bit] NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_Resource] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.AccessibilityApi]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.AccessibilityApi](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[AccessibilityApiId] [int] NULL,
	[OriginalValue] [varchar](100) NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.AccessibilityApi] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.AccessibilityControl]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.AccessibilityControl](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[AccessibilityControlId] [int] NULL,
	[OriginalValue] [varchar](100) NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.AccessibilityControl] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.AccessibilityFeature]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.AccessibilityFeature](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[AccessibilityFeatureId] [int] NULL,
	[OriginalValue] [varchar](100) NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.AccessibilityFeature] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.AccessibilityHazard]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.AccessibilityHazard](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[AccessibilityHazardId] [int] NULL,
	[OriginalValue] [varchar](100) NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.AccessibilityHazard] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.AgeRange]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.AgeRange](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NOT NULL,
	[FromAge] [int] NOT NULL,
	[ToAge] [int] NOT NULL,
	[AgeSpan] [int] NULL,
	[OriginalLevel] [varchar](50) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.AgeRange] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.AssessmentType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.AssessmentType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NOT NULL,
	[AssessmentTypeId] [int] NOT NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.AssessmentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.Cluster]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.Cluster](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClusterId] [int] NOT NULL,
	[CreatedById] [int] NULL,
	[Created] [datetime] NULL,
	[ResourceIntId] [int] NOT NULL,
 CONSTRAINT [PK_Resource.Pathway] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.Comment]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Comment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[Comment] [varchar](max) NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[ResourceId] [uniqueidentifier] NULL,
	[RowId] [uniqueidentifier] NOT NULL,
	[CreatedBy] [varchar](100) NULL,
	[Commenter] [varchar](500) NULL,
	[DocId] [varchar](500) NULL,
 CONSTRAINT [PK_Resource.Comment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.DetailView]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.DetailView](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.DetailView] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.EducationLevel]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.EducationLevel](
	[RowId] [uniqueidentifier] NOT NULL,
	[ResourceId] [uniqueidentifier] NOT NULL,
	[PathwaysEducationLevelId] [int] NULL,
	[OriginalLevel] [varchar](100) NULL,
	[Created] [datetime] NULL,
	[EducationLevelId] [int] NULL,
	[ResourceIntId] [int] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_EducationLevel] PRIMARY KEY NONCLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.EducationUse]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.EducationUse](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[EducationUseId] [int] NULL,
	[OriginalType] [varchar](100) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[ResourceId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Resource.EducationUse] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.Evaluation]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Evaluation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NOT NULL,
	[EvaluationId] [int] NOT NULL,
	[Score] [int] NOT NULL,
	[UserHasCertification] [bit] NOT NULL,
	[CreatedById] [int] NULL,
	[Created] [datetime] NULL,
	[StandardId] [int] NULL,
	[RubricId] [int] NULL,
	[Value] [float] NULL,
	[ScaleMin] [int] NULL,
	[ScaleMax] [int] NULL,
	[CriteriaInfo] [varchar](500) NULL,
 CONSTRAINT [PK_Resource.Evaluation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.EvaluationSection]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.EvaluationSection](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceEvalId] [int] NOT NULL,
	[EvalDimensionId] [int] NULL,
	[Score] [int] NOT NULL,
	[CreatedById] [int] NULL,
	[Created] [datetime] NOT NULL,
 CONSTRAINT [PK_Resource.EvaluationSection] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.EvaluationSummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.EvaluationSummary](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[EvaluationSubjectId] [int] NULL,
	[EvaluationUrl] [varchar](200) NULL,
	[TotalCount] [int] NULL,
	[Dimension1Count] [int] NULL,
	[Dimension2Count] [int] NULL,
	[Dimension3Count] [int] NULL,
	[Dimension4Count] [int] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.EvaluationSummary] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.Format]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Format](
	[RowId] [uniqueidentifier] NOT NULL,
	[ResourceId] [uniqueidentifier] NULL,
	[OriginalValue] [varchar](200) NULL,
	[CodeId] [int] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[ResourceIntId] [int] NULL,
 CONSTRAINT [PK_Resource.Format] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.GradeLevel]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.GradeLevel](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[GradeLevelId] [int] NULL,
	[OriginalLevel] [varchar](100) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[PathwaysEducationLevelId] [int] NULL,
 CONSTRAINT [PK_Resource.GradeLevel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.GroupType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.GroupType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[GroupTypeId] [int] NOT NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[ResourceId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Resource.GroupType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.IntendedAudience]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.IntendedAudience](
	[RowID] [uniqueidentifier] NOT NULL,
	[ResourceId] [uniqueidentifier] NULL,
	[AudienceId] [int] NULL,
	[OriginalAudience] [varchar](50) NULL,
	[CreatedById] [int] NULL,
	[Created] [datetime] NULL,
	[ResourceIntId] [int] NULL,
 CONSTRAINT [PK_IntendedAudience] PRIMARY KEY CLUSTERED 
(
	[RowID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.ItemType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.ItemType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NOT NULL,
	[ItemTypeId] [int] NOT NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedById] [int] NULL,
 CONSTRAINT [PK_Resource.ItemType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.Keyword]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Keyword](
	[Id] [int] IDENTITY(1000,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[Keyword] [varchar](200) NULL,
	[CreatedById] [int] NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Resource_Keyword] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.Language]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Language](
	[RowId] [uniqueidentifier] NOT NULL,
	[LanguageId] [int] NULL,
	[OriginalLanguage] [varchar](100) NULL,
	[ResourceIntId] [int] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_ResourceLanguage] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.LearningMapReference]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.LearningMapReference](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NOT NULL,
	[Subject] [varchar](50) NOT NULL,
	[GradeLevels] [varchar](50) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.LearningMapReference] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.Like]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.Like](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[IsLike] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[ResourceId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Resource.Like] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.LikeSummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.LikeSummary](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[LikeCount] [int] NULL,
	[DislikeCount] [int] NULL,
	[LastUpdated] [datetime] NULL,
	[ResourceId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Resource.LikeSummary] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.LinkCheck]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.LinkCheck](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NOT NULL,
	[LastCheckDate] [datetime] NULL,
	[HostTimeoutCount] [int] NULL,
	[ServerErrorCount] [int] NULL,
	[IsBadLink] [bit] NULL,
 CONSTRAINT [PK_Resource.LinkCheck] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.PublishedBy]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.PublishedBy](
	[ResourceIntId] [int] NOT NULL,
	[PublishedById] [int] NOT NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Resource.PublishedBy] PRIMARY KEY CLUSTERED 
(
	[ResourceIntId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.RatingSummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.RatingSummary](
	[ResourceId] [uniqueidentifier] NOT NULL,
	[RatingTypeId] [int] NOT NULL,
	[RatingCount] [int] NULL,
	[RatingTotal] [int] NULL,
	[RatingAverage] [decimal](5, 2) NULL,
	[LastUpdated] [datetime] NULL,
	[ResourceIntId] [int] NOT NULL,
 CONSTRAINT [PK_Resource.RatingSummary] PRIMARY KEY CLUSTERED 
(
	[ResourceIntId] ASC,
	[RatingTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.Recommendation]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Recommendation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[TypeId] [int] NULL,
	[IsActive] [bit] NULL,
	[Comment] [varchar](max) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.Recommendation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.RelatedUrl]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.RelatedUrl](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[RelatedUrl] [varchar](300) NOT NULL,
	[IsActive] [bit] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[ResourceId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Resource.RelatedUrl] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.ResourceType]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.ResourceType](
	[RowId] [uniqueidentifier] NOT NULL,
	[ResourceId] [uniqueidentifier] NULL,
	[ResourceTypeId] [int] NULL,
	[OriginalType] [varchar](100) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[ResourceIntId] [int] NULL,
 CONSTRAINT [PK_Resource_ResourceType] PRIMARY KEY NONCLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.SearchText]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.SearchText](
	[ResourceIntId] [int] NOT NULL,
	[SearchText] [varchar](max) NOT NULL,
 CONSTRAINT [PK_Resource.SearchText] PRIMARY KEY CLUSTERED 
(
	[ResourceIntId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.Site]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.Site](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.Site] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.Standard]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Standard](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NOT NULL,
	[StandardId] [int] NOT NULL,
	[StandardUrl] [varchar](200) NULL,
	[AlignedById] [int] NULL,
	[AlignmentTypeCodeId] [int] NULL,
	[AlignmentDegreeId] [int] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.StandardAlignment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.StandardEvaluation]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.StandardEvaluation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceStandardId] [int] NOT NULL,
	[Score] [int] NULL,
	[CreatedById] [int] NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_Resource.StandardEvaluation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.Subject]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Subject](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[Subject] [varchar](100) NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[CodeId] [int] NULL,
 CONSTRAINT [PK_Resource_Subject] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.Tag]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Tag](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NOT NULL,
	[TagValueId] [int] NOT NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
	[OriginalValue] [varchar](100) NULL,
 CONSTRAINT [PK_Resource.Tag] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.Text]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Text](
	[Id] [int] NOT NULL,
	[ResourceIntId] [int] NOT NULL,
	[TypeId] [int] NOT NULL,
	[Text] [varchar](200) NOT NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.Text] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.Version]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource.Version](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[Title] [nvarchar](300) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[DocId] [varchar](500) NULL,
	[Publisher] [varchar](200) NULL,
	[Creator] [varchar](200) NULL,
	[Rights] [varchar](700) NULL,
	[AccessRights] [varchar](105) NULL,
	[Modified] [datetime] NULL,
	[Submitter] [varchar](100) NULL,
	[Imported] [datetime] NULL,
	[Created] [datetime] NULL,
	[TypicalLearningTime] [varchar](50) NULL,
	[IsSkeletonFromParadata] [bit] NULL,
	[IsActive] [bit] NULL,
	[Requirements] [varchar](200) NULL,
	[SortTitle] [nvarchar](300) NULL,
	[Schema] [varchar](50) NULL,
	[AccessRightsId] [int] NULL,
	[InteractivityTypeId] [int] NULL,
	[InteractivityType] [varchar](100) NULL,
	[RowId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Resource.Version2] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource.VersionWeight]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.VersionWeight](
	[ResourceVersionId] [int] NOT NULL,
	[Weight] [float] NULL,
 CONSTRAINT [PK_Resource.VersionWeight] PRIMARY KEY CLUSTERED 
(
	[ResourceVersionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource.View]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource.View](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceIntId] [int] NULL,
	[Created] [datetime] NULL,
	[CreatedById] [int] NULL,
 CONSTRAINT [PK_Resource.View] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Resource_Index]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource_Index](
	[versionID] [int] NOT NULL,
	[intID] [int] NULL,
	[title] [nvarchar](300) NOT NULL,
	[description] [nvarchar](max) NULL,
	[publisher] [varchar](200) NULL,
	[created] [datetime] NULL,
	[accessRights] [varchar](100) NULL,
	[accessRightsID] [int] NULL,
	[keywords] [nvarchar](max) NULL,
	[subjects] [nvarchar](max) NULL,
	[languageIDs] [nvarchar](max) NULL,
	[languages] [nvarchar](max) NULL,
	[clusterIDs] [nvarchar](max) NULL,
	[clusters] [nvarchar](max) NULL,
	[audienceIDs] [nvarchar](max) NULL,
	[audiences] [nvarchar](max) NULL,
	[gradeLevelIDs] [nvarchar](max) NULL,
	[gradeLevels] [nvarchar](max) NULL,
	[gradeLevelAliases] [nvarchar](max) NULL,
	[resourceTypeIDs] [nvarchar](max) NULL,
	[resourceTypes] [nvarchar](max) NULL,
	[mediaTypeIDs] [nvarchar](max) NULL,
	[mediaTypes] [nvarchar](max) NULL,
	[groupTypeIDs] [nvarchar](max) NULL,
	[groupTypes] [nvarchar](max) NULL,
	[itemTypeIDs] [nvarchar](max) NULL,
	[itemTypes] [nvarchar](max) NULL,
	[standardIDs] [nvarchar](max) NULL,
	[standardNotations] [nvarchar](max) NULL,
	[alignmentTypeIDs] [nvarchar](max) NULL,
	[alignmentTypes] [nvarchar](max) NULL,
	[LastUpdated] [datetime] NULL,
	[url] [varchar](500) NULL,
	[libraryIDs] [varchar](max) NULL,
	[collectionIDs] [varchar](max) NULL,
	[IsDeleted] [bit] NULL,
	[educationalUseIDs] [nvarchar](max) NULL,
	[educationalUses] [nvarchar](max) NULL,
	[usageRights] [varchar](300) NULL,
	[usageRightsID] [int] NULL,
	[usageRightsURL] [varchar](700) NULL,
	[usageRightsIconURL] [varchar](300) NULL,
	[usageRightsMiniIconURL] [varchar](300) NULL,
	[likesSummary] [int] NULL,
	[evaluationCount] [int] NULL,
	[evaluationScore] [int] NULL,
	[commentsCount] [int] NULL,
	[viewsCount] [int] NULL,
	[timeRequired] [varchar](50) NULL,
	[isBasedOnUrl] [varchar](300) NULL,
	[creator] [varchar](200) NULL,
	[requirements] [varchar](200) NULL,
	[assessmentTypeIDs] [nvarchar](max) NULL,
	[assessmentTypes] [nvarchar](max) NULL,
	[detailViews] [int] NULL,
	[favorites] [int] NULL,
	[likeCount] [int] NULL,
	[dislikeCount] [int] NULL,
	[submitter] [varchar](100) NULL,
	[accessibilityApiIDs] [nvarchar](max) NULL,
	[accessibilityApis] [nvarchar](max) NULL,
	[accessibilityControlIDs] [nvarchar](max) NULL,
	[accessibilityControls] [nvarchar](max) NULL,
	[accessibilityFeatureIDs] [nvarchar](max) NULL,
	[accessibilityFeatures] [nvarchar](max) NULL,
	[accessibilityHazardIDs] [nvarchar](max) NULL,
	[accessibilityHazards] [nvarchar](max) NULL,
	[lrDocId] [varchar](100) NULL,
	[usageRightsDescription] [varchar](100) NULL,
	[careerPlannings] [nvarchar](max) NULL,
	[careerPlanningIDs] [nvarchar](max) NULL,
	[disabilityTopics] [nvarchar](max) NULL,
	[disabilityTopicIDs] [nvarchar](max) NULL,
	[jobPreparations] [nvarchar](max) NULL,
	[jobPreparationIDs] [nvarchar](max) NULL,
	[employerPrograms] [nvarchar](max) NULL,
	[employerProgramIDs] [nvarchar](max) NULL,
	[k12Subjects] [nvarchar](max) NULL,
	[k12SubjectIDs] [nvarchar](max) NULL,
	[veteransServices] [nvarchar](max) NULL,
	[veteransServiceIDs] [nvarchar](max) NULL,
	[workSupportServices] [nvarchar](max) NULL,
	[workSupportServiceIDs] [nvarchar](max) NULL,
	[wfePartners] [nvarchar](max) NULL,
	[wfePartnerIDs] [nvarchar](max) NULL,
	[workplaceSkills] [nvarchar](max) NULL,
	[workPlaceSkillsIDs] [nvarchar](max) NULL,
	[regions] [nvarchar](max) NULL,
	[regionIDs] [nvarchar](max) NULL,
	[targetSites] [nvarchar](max) NULL,
	[targetSiteIDs] [nvarchar](max) NULL,
	[sortTitle] [nvarchar](300) NULL,
 CONSTRAINT [PK_Resource_Index] PRIMARY KEY CLUSTERED 
(
	[versionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource_IndexEvaluation]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource_IndexEvaluation](
	[intID] [int] NOT NULL,
	[standardID] [int] NULL,
	[standardNotation] [varchar](50) NULL,
	[rubricID] [int] NULL,
	[rubricNotation] [varchar](50) NULL,
	[evaluationCount] [int] NULL,
	[evaluationScore] [float] NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource_IndexV2]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource_IndexV2](
	[versionID] [int] NOT NULL,
	[intID] [int] NULL,
	[title] [nvarchar](300) NOT NULL,
	[description] [nvarchar](max) NULL,
	[publisher] [varchar](200) NULL,
	[created] [datetime] NULL,
	[accessRights] [varchar](100) NULL,
	[accessRightsID] [int] NULL,
	[keywords] [nvarchar](max) NULL,
	[subjects] [nvarchar](max) NULL,
	[languageIDs] [nvarchar](max) NULL,
	[languages] [nvarchar](max) NULL,
	[clusterIDs] [nvarchar](max) NULL,
	[clusters] [nvarchar](max) NULL,
	[audienceIDs] [nvarchar](max) NULL,
	[audiences] [nvarchar](max) NULL,
	[gradeLevelIDs] [nvarchar](max) NULL,
	[gradeLevels] [nvarchar](max) NULL,
	[gradeLevelAliases] [nvarchar](max) NULL,
	[resourceTypeIDs] [nvarchar](max) NULL,
	[resourceTypes] [nvarchar](max) NULL,
	[mediaTypeIDs] [nvarchar](max) NULL,
	[mediaTypes] [nvarchar](max) NULL,
	[groupTypeIDs] [nvarchar](max) NULL,
	[groupTypes] [nvarchar](max) NULL,
	[itemTypeIDs] [nvarchar](max) NULL,
	[itemTypes] [nvarchar](max) NULL,
	[standardIDs] [nvarchar](max) NULL,
	[standardNotations] [nvarchar](max) NULL,
	[alignmentTypeIDs] [nvarchar](max) NULL,
	[alignmentTypes] [nvarchar](max) NULL,
	[LastUpdated] [datetime] NULL,
	[url] [varchar](500) NULL,
	[libraryIDs] [varchar](max) NULL,
	[collectionIDs] [varchar](max) NULL,
	[IsDeleted] [bit] NULL,
	[educationalUseIDs] [nvarchar](max) NULL,
	[educationalUses] [nvarchar](max) NULL,
	[usageRights] [varchar](300) NULL,
	[usageRightsID] [int] NULL,
	[usageRightsURL] [varchar](700) NULL,
	[usageRightsIconURL] [varchar](300) NULL,
	[usageRightsMiniIconURL] [varchar](300) NULL,
	[likesSummary] [int] NULL,
	[evaluationCount] [int] NULL,
	[evaluationScore] [int] NULL,
	[commentsCount] [int] NULL,
	[viewsCount] [int] NULL,
	[timeRequired] [varchar](50) NULL,
	[isBasedOnUrl] [varchar](300) NULL,
	[creator] [varchar](200) NULL,
	[requirements] [varchar](200) NULL,
	[assessmentTypeIDs] [nvarchar](max) NULL,
	[assessmentTypes] [nvarchar](max) NULL,
	[detailViews] [int] NULL,
	[favorites] [int] NULL,
	[likeCount] [int] NULL,
	[dislikeCount] [int] NULL,
	[submitter] [varchar](100) NULL,
	[accessibilityApiIDs] [nvarchar](max) NULL,
	[accessibilityApis] [nvarchar](max) NULL,
	[accessibilityControlIDs] [nvarchar](max) NULL,
	[accessibilityControls] [nvarchar](max) NULL,
	[accessibilityFeatureIDs] [nvarchar](max) NULL,
	[accessibilityFeatures] [nvarchar](max) NULL,
	[accessibilityHazardIDs] [nvarchar](max) NULL,
	[accessibilityHazards] [nvarchar](max) NULL,
	[lrDocId] [varchar](100) NULL,
	[usageRightsDescription] [varchar](100) NULL,
	[training] [nvarchar](max) NULL,
	[trainingIDs] [nvarchar](max) NULL,
	[disabilityTopics] [nvarchar](max) NULL,
	[disabilityTopicIDs] [nvarchar](max) NULL,
	[jobs] [nvarchar](max) NULL,
	[jobIDs] [nvarchar](max) NULL,
	[networking] [nvarchar](max) NULL,
	[networkingIDs] [nvarchar](max) NULL,
	[k12Subjects] [nvarchar](max) NULL,
	[k12SubjectIDs] [nvarchar](max) NULL,
	[resources] [nvarchar](max) NULL,
	[resourceIDs] [nvarchar](max) NULL,
	[wioaWorks] [nvarchar](max) NULL,
	[wioaWorksIDs] [nvarchar](max) NULL,
	[wfePartners] [nvarchar](max) NULL,
	[wfePartnerIDs] [nvarchar](max) NULL,
	[explore] [nvarchar](max) NULL,
	[exploreIDs] [nvarchar](max) NULL,
	[regions] [nvarchar](max) NULL,
	[regionIDs] [nvarchar](max) NULL,
	[targetSites] [nvarchar](max) NULL,
	[targetSiteIDs] [nvarchar](max) NULL,
	[sortTitle] [nvarchar](300) NULL,
	[qualify] [nvarchar](max) NULL,
	[qualifyIDs] [nvarchar](max) NULL,
	[layoffAssist] [nvarchar](max) NULL,
	[layoffAssistIDs] [nvarchar](max) NULL,
 CONSTRAINT [PK_Resource_IndexV2] PRIMARY KEY CLUSTERED 
(
	[versionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource_IndexV2.Tags]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource_IndexV2.Tags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ResourceId] [int] NOT NULL,
	[TagCategory] [varchar](50) NOT NULL,
	[TagList] [varchar](max) NULL,
	[TagIDs] [varchar](max) NULL,
 CONSTRAINT [PK_Resource_IndexV2.Tags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource_IndexV3Tags]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource_IndexV3Tags](
	[ResourceIntId] [int] NULL,
	[Titles] [varchar](max) NULL,
	[IDs] [varchar](max) NULL,
	[CategoryId] [int] NULL,
	[CategoryTitle] [varchar](50) NULL,
	[AliasValues] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Resource_IndexV3Texts]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Resource_IndexV3Texts](
	[ResourceIntId] [int] NULL,
	[ResourceVersionId] [int] NULL,
	[DocId] [varchar](max) NULL,
	[Title] [varchar](max) NULL,
	[UrlTitle] [varchar](max) NULL,
	[Description] [varchar](max) NULL,
	[Requirements] [varchar](max) NULL,
	[Url] [varchar](max) NULL,
	[ResourceCreated] [datetime] NULL,
	[Creator] [varchar](max) NULL,
	[Publisher] [varchar](max) NULL,
	[Submitter] [varchar](max) NULL,
	[RightsUrl] [varchar](max) NULL,
	[Keywords] [varchar](max) NULL,
	[LibraryIds] [varchar](max) NULL,
	[CollectionIds] [varchar](max) NULL,
	[StandardIds] [varchar](max) NULL,
	[StandardNotations] [varchar](max) NULL,
	[P_Favorites] [int] NULL,
	[P_ResourceViews] [int] NULL,
	[P_Likes] [int] NULL,
	[P_Dislikes] [int] NULL,
	[P_Rating] [float] NULL,
	[P_Comments] [int] NULL,
	[P_Evaluations] [int] NULL,
	[P_EvaluationsScore] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Rubric.Heading]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Rubric.Heading](
	[Id] [int] NOT NULL,
	[Title] [varchar](100) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Rubric.Node]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Rubric.Node](
	[Id] [int] NOT NULL,
	[ParentId] [int] NULL,
	[Notation] [varchar](50) NULL,
	[pUrl] [varchar](100) NULL,
	[AltUrl] [varchar](300) NULL,
	[Description] [varchar](2000) NULL,
	[ShortDescription] [varchar](500) NULL,
	[HeadingId] [int] NULL,
	[Sequence] [int] NULL,
 CONSTRAINT [PK_Rubric.Node] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ServerDatabaseTables]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ServerDatabaseTables](
	[ServerName] [nvarchar](128) NOT NULL,
	[DatabaseName] [nvarchar](128) NOT NULL,
	[tableName] [nvarchar](128) NOT NULL,
	[tableRowCount] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Sort.SpecialCharacter]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sort.SpecialCharacter](
	[Id] [float] NULL,
	[Character] [nvarchar](10) NULL,
	[EntityNbr] [nvarchar](10) NULL,
	[EntityName] [nvarchar](10) NULL,
	[Description] [nvarchar](255) NULL,
	[IsDrop] [bit] NOT NULL,
	[IsReplace] [bit] NOT NULL,
	[ReplaceWith] [nvarchar](10) NULL,
	[IsReplaceDescription] [bit] NULL,
	[IsDropDescription] [bit] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Standard.SubjectStandardConnector]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Standard.SubjectStandardConnector](
	[StandardSubjectId] [int] NOT NULL,
	[DomainNodeId] [int] NOT NULL,
 CONSTRAINT [PK_Standard.SubjectStandardConnector] PRIMARY KEY CLUSTERED 
(
	[StandardSubjectId] ASC,
	[DomainNodeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[StandardBody]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StandardBody](
	[Id] [int] NOT NULL,
	[Title] [varchar](75) NULL,
	[ShortName] [varchar](25) NULL,
	[Url] [varchar](200) NULL,
	[Description] [varchar](max) NULL,
	[WarehouseTotal] [int] NULL,
 CONSTRAINT [PK_Codes.StandardType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[StandardBody.Node]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StandardBody.Node](
	[Id] [int] NOT NULL,
	[ParentId] [int] NULL,
	[LevelType] [varchar](50) NULL,
	[NotationCode] [varchar](50) NULL,
	[Description] [varchar](max) NULL,
	[StandardUrl] [varchar](200) NULL,
	[AltUrl] [varchar](200) NULL,
	[StandardGuid] [varchar](50) NULL,
	[WarehouseTotal] [int] NULL,
	[GradeLevels] [varchar](50) NULL,
 CONSTRAINT [PK_Standard.TopicLevel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[StandardBody.NodeGradeLevel]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StandardBody.NodeGradeLevel](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParentId] [int] NOT NULL,
	[GradeLevel] [varchar](25) NULL,
	[GradeLevelId] [int] NULL,
 CONSTRAINT [PK_StandardBody.NodeGradeLevel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[StandardBody.Subject]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StandardBody.Subject](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StandardBodyId] [int] NOT NULL,
	[Title] [varchar](200) NOT NULL,
	[ShortTitle] [varchar](50) NULL,
	[Description] [varchar](max) NULL,
	[Url] [varchar](200) NULL,
	[Language] [varchar](200) NULL,
	[Created] [datetime] NULL,
 CONSTRAINT [PK_StandardBody.Subject] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[System.GenerateLoginId]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[System.GenerateLoginId](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProxyId] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[ProxyType] [varchar](50) NULL,
	[IsActive] [bit] NOT NULL,
	[Created] [datetime] NOT NULL,
	[ExpiryDate] [datetime] NOT NULL,
	[AccessDate] [datetime] NULL,
 CONSTRAINT [PK_System.GenerateLoginId] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[System.Process]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[System.Process](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](50) NOT NULL,
	[Title] [varchar](100) NOT NULL,
	[Description] [varchar](1000) NOT NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [varchar](75) NULL,
	[LastUpdated] [datetime] NULL,
	[LastUpdatedBy] [varchar](75) NULL,
	[LastRunDate] [datetime] NULL,
	[StringParameter] [varchar](100) NULL,
	[IntParameter] [int] NULL,
 CONSTRAINT [PK_System.Process] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [dbo].[Resource.Version_Summary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
-- images
-- 46 sec, for 19000 using all 
--title, res, desc, pub: 43 s, 8540 rows
--title, desc: 47 s, 7109 rows
--title, res, desc: 29 s, 8535 rows
--res: 1 s, 1541 rows
-- added index on title and pub
--title, pub: 3 s, 721 rows
--title, res, pub: 16 s, 2166 rows
--title, res: 3 s, 2152 rows

SELECT [ResourceUrl]
      ,[ResourceIntId]
      ,[ResourceVersionId]

      ,[Title]
      ,[Description]
      ,[Publisher]
      --,[Creator]
      --,[Rights]
      --,[AccessRights]
      --,[TypicalLearningTime]
      ,[Modified]
      --,[Submitter]

  FROM [dbo].[Resource.Version_Summary]
  where 
 -- MegaSearchField like '%image%'
  --or 
  title like  '%www.freesound.org%'
  --OR ResourceUrl like  '%www.freesound.org%'
  --OR Description like  '%oercommons%'
  OR Publisher like  '%www.freesound.org%'
  --127
  OR Subjects like  '%image%'
  OR Keywords like  '%image%' 
  order by 1
GO

--683
SELECT 
[ResourceIntId]
      ,[Title]
      ,[Description]
  FROM [dbo].[Resource.Version]
  where ([ResourceIntId] = 170347 ) 
  title like  '%image%'
  OR Description like  '%image%'
  OR Publisher like  '%image%'
  --7136
  order by 2

  SELECT 
[ResourceIntId]
      ,[Title]
      ,[Description]
  FROM [dbo].[Resource.Version_Summary]
  where ([ResourceIntId] = 170347 ) 
*/
/*
Resource.Version_Summary - summary of key resource tables. 
        Should only be used by a search. Not for returning to code
        WARNING - changing view may require recreation of the Resource_Version_Summary_ClusteredIndex.sql (or at least doing a rebuild!!!!
-- 13-01-31 mparsons - added join for new AccessRightsId
-- 13-03-11 mparsons - changed to join on ResourceIntId
-- 14-01-21 mparsons - added all fields necessary for RV.Fill        
*/
CREATE VIEW [dbo].[Resource.Version_Summary] 
--WITH SCHEMABINDING 
AS

SELECT 
--top 1000
      lr.ResourceUrl
      , lr.Id 
      , lr.Id As ResourceIntId
      ,'' As [ResourceId]
     -- ,base.RowId as ResourceVersionId
      ,base.Id as ResourceVersionIntId
      ,base.[Title]
      ,base.[Description]
      ,base.[Publisher]
      ,base.[Creator]
      ,base.[Rights]
      ,lr.ViewCount
      ,lr.FavoriteCount
	  ,base.DocId
      --,base.[AccessRights]
      ,base.AccessRightsId ,codes.title As AccessRights
      ,base.InteractivityTypeId, base.InteractivityType
      ,base.[TypicalLearningTime]
      ,base.[Modified]
	  ,base.Created
	  ,base.[Imported]
      ,base.[Submitter]
	  ,base.[Schema], base.Requirements
      ,base.SortTitle
	  ,rpb.PublishedById
--    SELECT count(*)      
  FROM [dbo].[Resource.Version] base
  Inner JOIN dbo.[Resource] lr ON base.ResourceIntId = lr.Id
  Left Join dbo.[Codes.AccessRights] codes on base.AccessRightsId = codes.Id
  left Join dbo.[resource.PublishedBy] rpb on base.ResourceIntId = rpb.ResourceIntId
  where 
      base.[IsActive]= 1 
  AND lr.[IsActive]= 1 
  

GO
/****** Object:  View [dbo].[Patron_ResourceSummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*

SELECT [UserId],FirstName, LastName
      ,[FullName]      ,[SortName]
      ,[Email]
      ,[IsUserActive]
      ,[JobTitle]
      ,[ImageUrl]
      ,[OrganizationId]
      ,[Organization]
      ,[Created]
      ,[LastUpdated]
      ,[Published]
      ,[ResourceUrl]
      ,[ResourceTitle]
      ,[Description]
      ,[ResourceId]
  FROM [dbo].[Patron_ResourceSummary]
GO



*/
CREATE VIEW [dbo].[Patron_ResourceSummary]
AS
SELECT     
	base.Id As UserId, 
	base.FirstName, 
	base.LastName, 
	base.FirstName + ' ' + base.LastName AS FullName, 
	base.LastName + ', ' + base.FirstName AS SortName, 
	base.Email, 
	base.IsActive As IsUserActive,
	prof.JobTitle, 
	prof.ImageUrl, 
	prof.OrganizationId,
	case when isnull(prof.OrganizationId,0) > 0 then
		org.Name
	else '' end As Organization,

	base.Created,
	case when isnull(base.LastUpdated, base.Created) > isnull(prof.LastUpdated, prof.Created) then base.LastUpdated
	else isnull(prof.LastUpdated, prof.Created) end as LastUpdated
	,rpb.Created as Published
	,res.ResourceUrl, res.Title as ResourceTitle, res.Description
	,res.Id As ResourceId

FROM  dbo.Patron base 
Inner JOIN dbo.[Patron.Profile] prof ON base.Id = prof.UserId
Left Join Gateway.dbo.Organization org on prof.OrganizationId = org.id
Inner Join [Resource.PublishedBy] rpb on base.Id = rpb.PublishedById
Inner Join [Resource.Version_Summary] res on rpb.ResourceIntId = res.Id
--where base.IsActive = 1


GO
/****** Object:  View [dbo].[Resource.EvaluationsList]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE VIEW [dbo].[Resource.EvaluationsList] AS
  select distinct  [ResourceIntId],[CreatedById]
  --,count(*) as REcounts
  FROM [dbo].[Resource.Evaluation] 




GO
/****** Object:  View [dbo].[Resource.EvaluationsCount]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[Resource.EvaluationsCount] AS

 select re.[ResourceIntId], count(*) as EvaluationsCount 
 from [dbo].[Resource.EvaluationsList] re
 group by re.[ResourceIntId]



GO
/****** Object:  View [dbo].[Codes.TagCategoryValue_summary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*


SELECT [CategoryId]
      ,[Category]
      ,[TagValueId]
      ,[TagRelativeId]
      ,[Title]
	  ,Description
      ,[WarehouseTotal]
  FROM [dbo].[Codes.TagCategoryValue_summary]
GO




*/
CREATE VIEW [dbo].[Codes.TagCategoryValue_summary]
AS
SELECT        TOP (100) PERCENT 
dbo.[Codes.TagCategory].Id AS CategoryId, 
dbo.[Codes.TagCategory].Title AS Category, 
dbo.[Codes.TagValue].Id AS TagValueId, 
dbo.[Codes.TagValue].CodeId As TagRelativeId, 
--add alias
dbo.[Codes.TagValue].CodeId As CodeId, 
dbo.[Codes.TagValue].Title, 
dbo.[Codes.TagValue].Description, 
dbo.[Codes.TagValue].WarehouseTotal
FROM dbo.[Codes.TagValue] 
	INNER JOIN dbo.[Codes.TagCategory] ON dbo.[Codes.TagValue].CategoryId = dbo.[Codes.TagCategory].Id
WHERE        
	(dbo.[Codes.TagCategory].IsActive = 1) 
	AND (dbo.[Codes.TagValue].IsActive = 1)

ORDER BY dbo.[Codes.TagCategory].SortOrder


GO
/****** Object:  View [dbo].[Codes.ResourceFormatTag]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*


SELECT [Id]
      
      ,[CodeId]
      ,[Title]
	  ,Description
      ,[WarehouseTotal]
	  ,[Category]
  FROM [dbo].[Codes.ResourceFormatTag]
  order by Title
  


*/
CREATE VIEW [dbo].[Codes.ResourceFormatTag]
AS
SELECT      
	TagValueId As Id, 
	[TagRelativeId] As CodeId, 
	Title, 
	Description, 
	WarehouseTotal
	,Category

FROM dbo.[Codes.TagCategoryValue_Summary] 
	
WHERE CategoryId =18



GO
/****** Object:  View [dbo].[Codes.ResourceTypeTag]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*


SELECT [Id]
      
      ,[CodeId]
      ,[Title]
	  ,Description
      ,[WarehouseTotal]
	  ,[Category]
  FROM [dbo].[Codes.ResourceTypeTag]
  order by Title


*/
CREATE VIEW [dbo].[Codes.ResourceTypeTag]
AS
SELECT      
	TagValueId As Id, 
	[TagRelativeId] As CodeId, 
	Title, 
	Description, 
	WarehouseTotal
	,Category

FROM dbo.[Codes.TagCategoryValue_Summary] 
	
WHERE CategoryId =19



GO
/****** Object:  View [dbo].[StandardBodyNode_Summary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[StandardBodyNode_Summary]
AS
SELECT     
sbody.ShortName AS Body, 
subj.Id As SubjectId,
subj.Title AS FormalSubject, 
subj.ShortTitle AS Subject,

DomainNode.Id As DomainId,
DomainNode.LevelType AS DomainLevel, 
DomainNode.Description AS Domain,

ClusterNode.Id As ClusterId,  
ClusterNode.LevelType AS ClusterLevel, 
ClusterNode.Description AS Cluster, 
ClusterNode.NotationCode AS ClusterCode, 
 
StandardNode.Id AS StandardId, 
StandardNode.LevelType AS StandardLevel, 
StandardNode.NotationCode AS StandardCode, 
StandardNode.Description AS Standard, 
StandardNode.StandardUrl, 

ComponentNode.Id AS ComponentId, 
ComponentNode.LevelType AS ComponentLevel, 
ComponentNode.NotationCode AS ComponentCode, 
ComponentNode.Description AS Component, 
ComponentNode.StandardUrl AS ComponentUrl
FROM         dbo.StandardBody sbody
INNER JOIN dbo.[StandardBody.Subject] subj ON sbody.Id = subj.StandardBodyId 
INNER JOIN dbo.[Standard.SubjectStandardConnector] scon ON subj.Id = scon.StandardSubjectId 
INNER JOIN dbo.[StandardBody.Node] AS DomainNode ON scon.DomainNodeId = DomainNode.Id 
INNER JOIN dbo.[StandardBody.Node] AS ClusterNode ON DomainNode.Id = ClusterNode.ParentId 
INNER JOIN dbo.[StandardBody.Node] AS StandardNode ON ClusterNode.Id = StandardNode.ParentId 
LEFT OUTER JOIN dbo.[StandardBody.Node] AS ComponentNode ON StandardNode.Id = ComponentNode.ParentId


GO
/****** Object:  View [dbo].[Resource.StandardSummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*


*/
CREATE VIEW [dbo].[Resource.StandardSummary]
AS
SELECT       
	dbo.Resource.Id AS ResourceId, 
	dbo.Resource.ResourceUrl, 
	dbo.[Resource.Version].Id AS ResourceVersionId, 
	dbo.[Resource.Version].Title, 
	dbo.[Resource.Standard].StandardId, 
	dbo.[Resource.Standard].Id, 
	dbo.StandardBodyNode_Summary.Domain, 
	dbo.StandardBodyNode_Summary.Cluster, 
	dbo.StandardBodyNode_Summary.StandardLevel, 
	dbo.StandardBodyNode_Summary.Standard, 
	dbo.StandardBodyNode_Summary.ComponentLevel, 
	dbo.StandardBodyNode_Summary.Component

FROM            dbo.Resource 
INNER JOIN dbo.[Resource.Version] ON dbo.Resource.Id = dbo.[Resource.Version].ResourceIntId 
INNER JOIN dbo.[Resource.Standard] ON dbo.Resource.Id = dbo.[Resource.Standard].ResourceIntId 
INNER JOIN dbo.StandardBodyNode_Summary ON dbo.[Resource.Standard].Id = dbo.StandardBodyNode_Summary.StandardId


GO
/****** Object:  UserDefinedFunction [dbo].[SplitString]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*


SELECT * FROM [dbo].[SplitString] ('Youth, Disability, Advocacy' , ',' )
GO



*/
CREATE FUNCTION [dbo].[SplitString]
(
    @String NVARCHAR(4000),
    @Delimiter NCHAR(1)
)
RETURNS TABLE 
AS
RETURN 
(
    WITH Split(stpos,endpos) 
    AS(
        SELECT 0 AS stpos, CHARINDEX(@Delimiter,@String) AS endpos
        UNION ALL
        SELECT endpos+1, CHARINDEX(@Delimiter,@String,endpos+1)
            FROM Split
            WHERE endpos > 0
    )
    SELECT 'Id' = ROW_NUMBER() OVER (ORDER BY (SELECT 1)),
        'Data' = SUBSTRING(@String,stpos,COALESCE(NULLIF(endpos,0),LEN(@String)+1)-stpos)
    FROM Split
)

GO
/****** Object:  View [dbo].[ActiveLibrarySummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[ActiveLibrarySummary] AS
	SELECT lib.Id, lib.Title, lib.[Description], LibraryTypeId, IsDiscoverable, lib.IsPublic, COUNT(libres.Id) AS NbrResources, MAX(libres.Created) As LastUpdated
	FROM IsleContent.dbo.[Library] lib
	INNER JOIN IsleContent.dbo.[Library.Section] libsec on lib.Id = libsec.LibraryId
	INNER JOIN IsleContent.dbo.[Library.Resource] libres on libsec.Id = libres.LibrarySectionId
	WHERE lib.IsActive = 'True'
	GROUP BY lib.Id, lib.Title, lib.[Description], LibraryTypeId, IsDiscoverable, lib.IsPublic

GO
/****** Object:  View [dbo].[Codes.CareerCluster]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
SELECT [Id]
      ,[Title]
      ,[WarehouseTotal]
  FROM [dbo].[Codes.CareerCluster]
  where [WarehouseTotal] > 0 
  Order by Title
*/

CREATE VIEW [dbo].[Codes.CareerCluster] AS
SELECT [Id]
      ,[IlPathwayName] As Title
	  ,IsActive
      ,[WarehouseTotal]
  FROM [Isle_IOER].[dbo].[CareerCluster]
  Where [IsIlPathway]= 1 and IsActive= 1

GO
/****** Object:  View [dbo].[Codes.SiteTagCategory_Summary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*


SELECT Id
,[SiteId]

      ,[CategoryId]
	  ,SchemaTag
      ,[Title]
      ,[Description]
      ,[Site_SortOrder]
      ,[TagCategory_SortOrder]
  FROM [dbo].[Codes.SiteTagCategory_Summary]
where SiteId= 1





*/
CREATE VIEW [dbo].[Codes.SiteTagCategory_Summary]
AS
SELECT        TOP (100) PERCENT 
	dbo.[Codes.SiteTagCategory].Id, 
	dbo.[Codes.SiteTagCategory].SiteId, 
	dbo.[Codes.SiteTagCategory].CategoryId, 
	dbo.[Codes.TagCategory].Title, 
	dbo.[Codes.TagCategory].Description, 
	dbo.[Codes.TagCategory].SchemaTag, 
	dbo.[Codes.SiteTagCategory].SortOrder AS Site_SortOrder, 
	dbo.[Codes.TagCategory].SortOrder AS TagCategory_SortOrder
FROM dbo.[Codes.SiteTagCategory] 
INNER JOIN dbo.[Codes.TagCategory] ON dbo.[Codes.SiteTagCategory].CategoryId = dbo.[Codes.TagCategory].Id
WHERE        
	(dbo.[Codes.SiteTagCategory].IsActive = 1) 
AND (dbo.[Codes.TagCategory].IsActive = 1)
and categoryid <> 27
ORDER BY dbo.[Codes.SiteTagCategory].SiteId, 
		Site_SortOrder, 
		TagCategory_SortOrder, 
		dbo.[Codes.TagCategory].Title


GO
/****** Object:  View [dbo].[Gateway.OrgSummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



/*
SELECT [id]
      ,[Name]
      ,[OrgTypeId]
      ,[OrgType]
      ,[parentId]

      ,base.[IsActive]
      ,[MainPhone],[MainExtension],[Fax],[TTY]
      ,[WebSite],[Email]
      ,[LogoUrl]
      ,[Address],[Address2],[City],[State],[Zipcode]
      ,base.[Created]      ,base.[CreatedById]
      ,[LastUpdated]      ,[LastUpdatedById]
  FROM [IsleContent].[dbo].[Gateway.OrgSummary]
GO



*/

/*
[Gateway.OrgSummary] - select org summary . 
    
*/
Create VIEW [dbo].[Gateway.OrgSummary] AS

SELECT [id]
      ,[Name]
      ,[OrgTypeId]
      ,[OrgType]
      ,[parentId],[ParentOrganization]
      ,[IsActive]
      ,[MainPhone],[MainExtension],[Fax],[TTY]
      ,[WebSite],[Email],[LogoUrl]
      ,[Address],[Address2],[City],[State],[Zipcode]
      ,[Created],[CreatedById]
      ,[LastUpdated],[LastUpdatedById]

  FROM [Gateway].[dbo].[Organization_Summary]




GO
/****** Object:  View [dbo].[IntendedAudienceCounts]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
SELECT [IntendedAudience]
      ,[RecordCount]
  FROM [dbo].[IntendedAudienceCounts]
  order by IntendedAudience
GO



*/
CREATE VIEW [dbo].[IntendedAudienceCounts]
AS
SELECT Top 100 Percent
	case 
		when code.Title = 'Teacher/Education Specialist' then 'Educator'
		when code.Title = 'student' then 'Student'
		else 'Other' end As IntendedAudience

, COUNT(*) As RecordCount
  FROM [dbo].[Resource.IntendedAudience] base
  inner join dbo.[Codes.AudienceType] code on base.AudienceId = code.Id
group by case 
		when code.Title = 'Teacher/Education Specialist' then 'Educator'
		when code.Title = 'student' then 'Student'
		else 'Other' end 



GO
/****** Object:  View [dbo].[Library.Resource]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[Library.Resource] AS
	SELECT lr.Id, ResourceUrl, Title, [Description], LibrarySectionId, lr.ResourceIntId, rv.Id AS ResourceVersionId, Comment, lr.Created, CreatedById
	FROM IsleContent.dbo.[Library.Resource] lr
	INNER JOIN [Resource] r ON lr.ResourceIntId = r.Id
	INNER JOIN [Resource.Version] rv ON lr.ResourceIntId = rv.ResourceIntId


GO
/****** Object:  View [dbo].[Library.Section]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Library.Section] AS
	SELECT Id, LibraryId, SectionTypeId, Title, Description, ParentId, IsDefaultSection, IsPublic,
		AreContentsReadOnly, ImageUrl, Created, CreatedById, LastUpdated, LastUpdatedById
	FROM IsleContent.dbo.[Library.Section]
GO
/****** Object:  View [dbo].[Library.Subscription]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Library.Subscription] AS
	SELECT Id, LibraryId, UserId, SubscriptionTypeId, PrivilegeId, Created, LastUpdated
	FROM IsleContent.dbo.[Library.Subscription]
GO
/****** Object:  View [dbo].[Patron_Summary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
SELECT [Id]
      ,[UserName]
      ,[FirstName]
      ,[LastName]
      ,[FullName], SortName
	  ,ImageUrl, UserProfileUrl
      ,[Email]
	  ,HasProfile
      ,[JobTitle]
      ,[RoleProfile]
      ,[OrganizationId], created, LastUpdated

  FROM [Isle_IOER].[dbo].[Patron_Summary]
GO


*/
CREATE VIEW [dbo].[Patron_Summary]
AS
SELECT     
	base.Id, 
	base.UserName, 
	base.FirstName, base.LastName, 
	base.FirstName + ' ' + base.LastName AS FullName, 
	base.LastName + ', ' + base.FirstName AS SortName, 
	base.Email, 
	base.IsActive,
	case when isnull(prof.UserId,0) > 0 then 1
	else 0 end As HasProfile,
	prof.JobTitle, 
	prof.RoleProfile, 
	prof.ImageUrl, 
	'/Profile/' + convert(varchar, base.[Id]) + '/' + replace(base.FirstName + '_' + base.LastName, '''' , '') As UserProfileUrl,
	prof.OrganizationId,
	case when isnull(prof.OrganizationId,0) > 0 then
		org.Name
	else '' end As Organization,

	prof.PublishingRoleId, codes.Title As PublishingRole,
	base.Created,
	case when isnull(base.LastUpdated, base.Created) > isnull(prof.LastUpdated, prof.Created) then base.LastUpdated
	else isnull(prof.LastUpdated, prof.Created) end as LastUpdated,
	base.RowId as UserRowId
FROM  dbo.Patron base 
LEFT JOIN dbo.[Patron.Profile] prof ON base.Id = prof.UserId
Left Join [Codes.AudienceType] codes on prof.PublishingRoleId = codes.Id
Left Join Gateway.dbo.Organization org on prof.OrganizationId = org.id


GO
/****** Object:  View [dbo].[PrivateResource]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[PrivateResource]
AS
SELECT     ResourceIntId AS Id, DocId
FROM         dbo.[Resource.Version]
WHERE     (DocId IS NULL)

GO
/****** Object:  View [dbo].[Resource.EducationLevelsList_Delete]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
SELECT top 1000
 [ResourceId]
      ,[EducationLevels]
  FROM [dbo].[Resource.EducationLevelsList]
where len([EducationLevels] ) > 0

*/
CREATE VIEW [dbo].[Resource.EducationLevelsList_Delete]
AS
SELECT     TOP (100) PERCENT 
base.[Id] As ResourceIntId,
    CASE
          WHEN EducationLevels IS NULL THEN NULL
          WHEN len(EducationLevels) = 0 THEN NULL
          ELSE left(EducationLevels,len(EducationLevels)-1)
    END AS EducationLevels

FROM [dbo].[Resource] base
CROSS APPLY (
    --SELECT distinct EducationLevel  + ', '
    --FROM [Resource.EducationLevel] list
    --WHERE base.RowId = list.ResourceId 
    --and (EducationLevel is not null AND len(EducationLevel) > 0)
    -- soon
    SELECT distinct pel.Title  + ', '
    FROM [Resource.EducationLevel] list
    inner join [Codes.PathwaysEducationLevel] pel on list.PathwaysEducationLevelId = pel.Id
    WHERE base.Id = list.ResourceIntId 
    and (list.PathwaysEducationLevelId is not null AND list.PathwaysEducationLevelId > 0)
	and pel.IsPathwaysLevel = 1
   -- Order by 1
    FOR XML Path('') 
    ) D (EducationLevels)




GO
/****** Object:  View [dbo].[Resource.EducationLevelsSummary_Delete]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
SELECT top 1000
[ResourceIntId],
 [PathwaysEducationLevelId]
      ,[EducationLevel]
  FROM [dbo].[Resource.EducationLevelsSummary]


*/
CREATE VIEW [dbo].[Resource.EducationLevelsSummary_Delete]
AS

SELECT [RowId]
      ,ResourceIntId
      ,[PathwaysEducationLevelId]
      ,pel.Title  As [EducationLevel]
      ,[OriginalLevel]
  FROM [dbo].[Resource.EducationLevel] base
  inner join [Codes.PathwaysEducationLevel] pel on base.PathwaysEducationLevelId = pel.Id



GO
/****** Object:  View [dbo].[Resource.EvalDimensionsSummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*

USE [Isle_IOER]
GO

SELECT [ResourceIntId]
      ,[EvalDimensionId]
      ,[DimensionTitle]
      ,[HasCertificationTotal]
      ,[HasCertificationTotalScore]
      ,[HasNOTCertificationTotal]
      ,[HasNOTCertificationTotalScore]
      ,[TotalEvaluations]
  FROM [dbo].[Resource.EvalDimensionsSummary]

where resourceintId = 449873 and EvaluationId = 1

order by ResourceIntId



*/

CREATE VIEW [dbo].[Resource.EvalDimensionsSummary] AS

SELECT 
      re.ResourceIntId,
	  re.EvaluationId,
	  base.EvalDimensionId
      , edim.Title As DimensionTitle
	  --,re.UserHasCertification 
	  --,sum(base.Score) As TotalScore
	  --,sum(base.Score)/count(*) As AverageScorePercent
	  --,count(*) as TotalEvals
	  ,SUM(CASE WHEN re.UserHasCertification = 1 THEN 1 ELSE 0 END) AS [HasCertificationTotal]
	  ,SUM(CASE WHEN re.UserHasCertification = 1 THEN base.Score ELSE 0 END) AS [HasCertificationTotalScore]
	  ,SUM(CASE WHEN re.UserHasCertification = 0 THEN 1 ELSE 0 END) AS [HasNOTCertificationTotal]
	  ,SUM(CASE WHEN re.UserHasCertification = 0 THEN base.Score ELSE 0 END) AS [HasNOTCertificationTotalScore]
	  ,SUM(1) AS TotalEvaluations
	  
  FROM [dbo].[Resource.EvaluationSection] base
  inner join [Resource.Evaluation] re on base.ResourceEvalId = re.Id
  INNER JOIN dbo.[Evaluation.Dimension] edim  ON base.EvalDimensionId = edim.Id

 group by 
	re.ResourceIntId,
	re.EvaluationId,
	base.EvalDimensionId
	,edim.Title
	--,re.UserHasCertification
 --order by   
	--re.ResourceIntId
	--,base.EvalDimensionId
	




GO
/****** Object:  View [dbo].[Resource.EvaluationsSummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
USE [Isle_IOER]
GO

SELECT [ResourceIntId]
      ,[EvaluationId]
      ,[EvaluationTitle]
      ,[RequiresCertification]
      ,[PrivilegeCode]
      ,[HasCertificationTotal]
      ,[HasCertificationTotalScore]
      ,[HasNotCertificationTotal]
      ,[HasNOTCertificationTotalScore]
      ,[TotalEvaluations]
  FROM [dbo].[Resource.EvaluationsSummary]

where resourceIntId = 450310


*/

CREATE VIEW [dbo].[Resource.EvaluationsSummary] AS

SELECT        
	base.ResourceIntId, 
	base.EvaluationId, 
	dbo.Evaluation.Title AS EvaluationTitle, 
	isnull(dbo.Evaluation.RequiresCertification, 0) As RequiresCertification,
	isnull(dbo.Evaluation.PrivilegeCode,'') As PrivilegeCode,
	--base.UserHasCertification,
	 SUM(CASE WHEN base.UserHasCertification = 1 THEN 1 ELSE 0 END) AS [HasCertificationTotal],
	 SUM(CASE WHEN base.UserHasCertification = 1 THEN base.Score ELSE 0 END) AS [HasCertificationTotalScore],
	 SUM(CASE WHEN base.UserHasCertification = 0 THEN 1 ELSE 0 END) AS [HasNOTCertificationTotal],
	 SUM(CASE WHEN base.UserHasCertification = 0 THEN base.Score ELSE 0 END) AS [HasNOTCertificationTotalScore],
	 SUM(1) AS TotalEvaluations
	--sum(base.Score)/count(*) As AverageScorePercent
	--,count(*) As TotalEvals
FROM dbo.[Resource.Evaluation] base
INNER JOIN dbo.Evaluation  ON base.EvaluationId = dbo.Evaluation.Id

group by base.ResourceIntId, 
	base.EvaluationId, 
	dbo.Evaluation.Title,
	dbo.Evaluation.RequiresCertification,
	isnull(dbo.Evaluation.PrivilegeCode,'') 
	--,base.UserHasCertification


GO
/****** Object:  View [dbo].[Resource.GradeLevelList]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
SELECT top 1000
 [ResourceId]
      ,[GradeLevels]
  FROM [dbo].[Resource.GradeLevelList]
where len([GradeLevels] ) > 0

*/
CREATE VIEW [dbo].[Resource.GradeLevelList]
AS
SELECT     TOP (100) PERCENT 
	base.[Id] As ResourceIntId,
    CASE
          WHEN GradeLevels IS NULL THEN NULL
          WHEN len(GradeLevels) = 0 THEN NULL
          ELSE left(GradeLevels,len(GradeLevels)-1)
    END AS GradeLevels

FROM [dbo].[Resource] base
CROSS APPLY (
    --SELECT distinct GradeLevel  + ', '
    --FROM [Resource.GradeLevel] list
    --WHERE base.RowId = list.ResourceId 
    --and (GradeLevel is not null AND len(GradeLevel) > 0)
    -- soon
    SELECT distinct pel.Title  + ', '
    FROM [Resource.GradeLevel] list
    inner join [Codes.GradeLevel] pel on list.GradeLevelId = pel.Id
    WHERE base.Id = list.ResourceIntId 
    and (list.GradeLevelId is not null AND list.GradeLevelId > 0)
   -- Order by 1
    FOR XML Path('') 
    ) D (GradeLevels)


GO
/****** Object:  View [dbo].[Resource.IntendedAudienceList]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
SELECT top 100
 [ResourceId]
      ,[AudienceList]
  FROM [dbo].[Resource.IntendedAudienceList]
where AudienceList is not null AND  len([AudienceList] ) > 0

*/
CREATE VIEW [dbo].[Resource.IntendedAudienceList]
AS
SELECT  TOP (100) PERCENT 
  base.[Id] As ResourceIntId,
  CASE
        WHEN ItemsList IS NULL THEN NULL
        WHEN len(ItemsList) = 0 THEN NULL
        ELSE left(ItemsList,len(ItemsList)-1)
  END AS AudienceList

FROM [dbo].[Resource] base
CROSS APPLY (
    SELECT distinct Title  + ', '
    FROM [Resource.IntendedAudience] list
    inner join [Codes.AudienceType] pel on list.[AudienceId] = pel.Id
    WHERE base.Id = list.ResourceIntId 
    and (list.[AudienceId] is not null AND list.[AudienceId] > 0)

    FOR XML Path('') 
    ) D (ItemsList)




GO
/****** Object:  View [dbo].[Resource.KeywordsCsvList]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
SELECT TOP 1000 [ResourceIntId]
      ,[Keywords]
  FROM [Isle_IOER].[dbo].[Resource.KeywordsCsvList]

  
SELECT count(*)
  FROM [dbo].[Resource.KeywordsCsvList]

*/
CREATE VIEW [dbo].[Resource.KeywordsCsvList]
AS
SELECT     TOP (100) PERCENT 
base.[Id] As ResourceIntId,
    CASE
          WHEN Keywords IS NULL THEN ''
          WHEN len(Keywords) = 0 THEN ''
          ELSE left(Keywords,len(Keywords)-1)
    END AS Keywords

FROM [dbo].[Resource] base

CROSS APPLY (
    SELECT child.Keyword  + ', '
   -- ,child.ResourceId
    FROM dbo.[Resource.Keyword] child   
    WHERE base.Id = child.ResourceIntId
    FOR XML Path('') ) D (Keywords)
	where Keywords is not null


GO
/****** Object:  View [dbo].[Resource.LanguagesList]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
SELECT top 1000
 [ResourceIntId]
      ,[LanguageList]
  FROM [dbo].[Resource.LanguagesList]
where LanguageList is not null AND len([LanguageList] ) > 0

*/
CREATE VIEW [dbo].[Resource.LanguagesList]
AS
SELECT     TOP (100) PERCENT 
  base.[Id] As ResourceIntId,
  CASE
        WHEN ItemsList IS NULL THEN NULL
        WHEN len(ItemsList) = 0 THEN NULL
        ELSE left(ItemsList,len(ItemsList)-1)
  END AS LanguageList
/* using [Resource.Language]*/
FROM [dbo].[Resource] base
CROSS APPLY (
    SELECT distinct codes.Title  + ', '
    FROM [Resource.Language] list
    inner join [Codes.Language] codes on list.LanguageId = codes.Id
    WHERE base.Id = list.ResourceIntId 
    and (codes.Title is not null AND len(codes.Title) > 0)

    FOR XML Path('') 
    ) D (ItemsList)
    
/* using [Resource.Property]
FROM [dbo].[Resource] base 
CROSS APPLY
  (SELECT DISTINCT [Value] + ', '
    FROM          [Resource.Property] list
    WHERE      base.RowId = list.ResourceId
    AND list.[PropertyTypeId]= 4 
    AND ([Value] IS NOT NULL AND len([Value]) > 0 )
         FOR XML Path('')) D (ItemsList)

*/


GO
/****** Object:  View [dbo].[Resource.LikesSummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
SELECT top 100 [ResourceIntId]
      ,[LikeCount]
      ,[DislikeCount]
  FROM [Resource.LikesSummary]
  where LikeCount> 0 OR DislikeCount > 0
  
GO


*/

CREATE VIEW [dbo].[Resource.LikesSummary]
AS
	SELECT res.Id AS ResourceIntId, isnull(rls.LikeCount,0) + ISNULL(rLike.LikeCount,0) AS LikeCount, isnull(rls.DislikeCount,0) + ISNULL(rDislike.DislikeCount,0) AS DislikeCount
	FROM [Resource] res
	LEFT JOIN [Resource.LikeSummary] rls on res.Id = rls.ResourceIntId
	LEFT JOIN (
		SELECT ResourceIntId, COUNT(*) AS LikeCount
		FROM [Resource.Like]
		WHERE IsLike = 'True'
		GROUP BY ResourceIntId) rLike ON res.Id = rLike.ResourceIntId
	LEFT JOIN (
		SELECT ResourceIntId, COUNT(*) AS DislikeCount
		FROM [Resource.Like]
		WHERE IsLike = 'False'
		GROUP BY ResourceIntId) rDislike ON res.Id = rDislike.ResourceIntId



GO
/****** Object:  View [dbo].[Resource.Link]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Resource.Link]
AS
SELECT        ResourceIntId, LastCheckDate, HostName, IsDeleted, NbrDnsErrors, NbrTimeouts, NbrInternalServerErrors, NbrUnableToConnect
FROM            LinkChecker.dbo.[Resource.Link]

GO
/****** Object:  View [dbo].[Resource.ResourceTypesList]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
SELECT top 1000
 [ResourceId]
      ,[ResourceTypesList]
  FROM [dbo].[Resource.ResourceTypesList]
where 
ResourceId= '01ed5910-9965-4938-8c9a-c2eaebe62cda'
ItemsList is not null AND len([ItemsList] ) > 0


&lt;li&gt;Reference&lt;/li&gt
*/
CREATE VIEW [dbo].[Resource.ResourceTypesList]
AS
SELECT     TOP (100) PERCENT 
	base.[Id] As ResourceIntId,
    CASE
          WHEN ItemsList IS NULL THEN NULL
          WHEN len(ItemsList) = 0 THEN NULL
          --ELSE ItemsList
          ELSE left(ItemsList,len(ItemsList)-1)
    END AS ResourceTypesList

FROM [dbo].[Resource] base
CROSS APPLY (
    --SELECT distinct '<li>' + codes.Title  + '</li> '
    SELECT distinct codes.Title  + ', '
    FROM [Resource.ResourceType] list
    Inner Join dbo.[Codes.ResourceType] codes on list.ResourceTypeId = codes.Id 
    WHERE base.Id = list.ResourceIntId 

    FOR XML Path('') 
    ) D (ItemsList)




GO
/****** Object:  View [dbo].[Resource.SearchableIndexView]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[Resource.SearchableIndexView]
AS
SELECT rv.Id AS ResourceVersionId, rv.Title, convert(nvarchar,rv.[Description]) AS [Description], rv.Publisher, rv.Created, rv.AccessRights, rk.Keywords,
	rs.Subjects, rl1.LanguageIds, rl2.Languages, cc1.ClusterIds, cc2.Clusters, ia1.AudienceIds,
	ia2.Audiences, el1.EducationLevelIds, el2.EducationLevels, rt1.ResourceTypeIds, rt2.ResourceTypes,
	rf1.ResourceFormatIds, rf2.ResourceFormats, gt1.GroupTypeIds, gt2.GroupTypes, it1.ItemTypeIds,
	it2.ItemTypes, rst1.StandardIds, rst2.Standards
FROM [Resource.Version] rv

-- Keywords
LEFT JOIN (
	SELECT ResourceIntId, left(Keywords, LEN(Keywords) - 1) AS Keywords
	FROM (SELECT ResourceIntId, (
		SELECT itbl.[Keyword]+','
		FROM [Resource.Keyword] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Keywords
	FROM [Resource.Keyword] tbl
	GROUP BY ResourceIntId) otab) rk ON rv.ResourceIntId = rk.ResourceIntId

-- Subjects
LEFT JOIN (
	SELECT ResourceIntId, left(Subjects, LEN(Subjects) - 1) AS Subjects
	FROM (SELECT ResourceIntId, (
		SELECT itbl.[Subject]+','
		FROM [Resource.Subject] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Subjects
	FROM [Resource.Subject] tbl
	GROUP BY ResourceIntId) otab) rs ON rv.ResourceIntId = rs.ResourceIntId

-- Language Ids
LEFT JOIN (
	SELECT ResourceIntId, left(Languages, LEN(Languages) - 1) AS LanguageIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[LanguageId])+','
		FROM [Resource.Language] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Languages
	FROM [Resource.Language] tbl
	GROUP BY ResourceIntId) otab) rl1 ON rv.ResourceIntId = rl1.ResourceIntId

-- Languages
LEFT JOIN (
	SELECT ResourceIntId, left(Languages, LEN(Languages) - 1) AS Languages
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.Language].Title+','
		FROM [Resource.Language] itbl
		INNER JOIN [Codes.Language] ON itbl.LanguageId = [Codes.Language].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Languages
	FROM [Resource.Language] tbl
	GROUP BY ResourceIntId) otab) rl2 on rv.ResourceIntId = rl2.ResourceIntId

-- Career Cluster Ids
LEFT JOIN (
	SELECT ResourceIntId, left(Clusters, LEN(Clusters) - 1) AS ClusterIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[ClusterId])+','
		FROM [Resource.Cluster] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Clusters
	FROM [Resource.Cluster] tbl
	GROUP BY ResourceIntId) otab) cc1 ON rv.ResourceIntId = cc1.ResourceIntId
	
-- Career Clusters
LEFT JOIN (
SELECT ResourceIntId, left(Clusters, LEN(Clusters) - 1) AS Clusters
	FROM (SELECT ResourceIntId, (
		SELECT '"'+[CareerCluster].IlPathwayName+'",'
		FROM [Resource.Cluster] itbl
		INNER JOIN [CareerCluster] ON itbl.ClusterId = [CareerCluster].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Clusters
	FROM [Resource.Cluster] tbl
	GROUP BY ResourceIntId) otab) cc2 ON rv.ResourceIntId = cc2.ResourceIntId
	
-- Intended Audience Ids
LEFT JOIN(
	SELECT ResourceIntId, left(Audiences, LEN(Audiences) - 1) AS AudienceIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[AudienceId])+','
		FROM [Resource.IntendedAudience] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Audiences
	FROM [Resource.IntendedAudience] tbl
	GROUP BY ResourceIntId) otab) ia1 ON rv.ResourceIntId = ia1.ResourceIntId
	
-- Intended Audiences
LEFT JOIN (
	SELECT ResourceIntId, left(Audiences, LEN(Audiences) - 1) AS Audiences
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.AudienceType].Title+','
		FROM [Resource.IntendedAudience] itbl
		INNER JOIN [Codes.AudienceType] ON itbl.AudienceId = [Codes.AudienceType].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Audiences
	FROM [Resource.IntendedAudience] tbl
	GROUP BY ResourceIntId) otab) ia2 ON rv.ResourceIntId = ia2.ResourceIntId
	
-- Education Level Ids
LEFT JOIN (
	SELECT ResourceIntId, left(EducationLevels, LEN(EducationLevels) - 1) AS EducationLevelIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[PathwaysEducationLevelId])+','
		FROM [Resource.EducationLevel] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) EducationLevels
	FROM [Resource.EducationLevel] tbl
	GROUP BY ResourceIntId) otab) el1 ON rv.ResourceIntId = el1.ResourceIntId
	
-- Education Level
LEFT JOIN (
	SELECT ResourceIntId, left(EducationLevels, LEN(EducationLevels) - 1) AS EducationLevels
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.PathwaysEducationLevel].Title+','
		FROM [Resource.EducationLevel] itbl
		INNER JOIN [Codes.PathwaysEducationLevel] ON itbl.EducationLevelId = [Codes.PathwaysEducationLevel].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) EducationLevels
	FROM [Resource.EducationLevel] tbl
	GROUP BY ResourceIntId) otab) el2 ON rv.ResourceIntId = el2.ResourceIntId
	
-- Resource Type Ids
LEFT JOIN (
	SELECT ResourceIntId, left(ResourceTypes, LEN(ResourceTypes) - 1) AS ResourceTypeIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[ResourceTypeId])+','
		FROM [Resource.ResourceType] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) ResourceTypes
	FROM [Resource.ResourceType] tbl
	GROUP BY ResourceIntId) otab) rt1 ON rv.ResourceIntId = rt1.ResourceIntId
	
-- Resource Type
LEFT JOIN (
	SELECT ResourceIntId, left(ResourceTypes, LEN(ResourceTypes) - 1) AS ResourceTypes
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.ResourceType].Title+','
		FROM [Resource.ResourceType] itbl
		INNER JOIN [Codes.ResourceType] ON itbl.ResourceTypeId = [Codes.ResourceType].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) ResourceTypes
	FROM [Resource.ResourceType] tbl
	GROUP BY ResourceIntId) otab) rt2 ON rv.ResourceIntId = rt2.ResourceIntId
	
-- Resource Format Ids
LEFT JOIN (
	SELECT ResourceIntId, left(ResourceFormats, LEN(ResourceFormats) - 1) AS ResourceFormatIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[CodeId])+','
		FROM [Resource.Format] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) ResourceFormats
	FROM [Resource.Format] tbl
	GROUP BY ResourceIntId) otab) rf1 ON rv.ResourceIntId = rf1.ResourceIntId
	
-- Resource Formats
LEFT JOIN (
	SELECT ResourceIntId, left(ResourceFormats, LEN(ResourceFormats) - 1) AS ResourceFormats
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.ResourceFormat].Title+','
		FROM [Resource.Format] itbl
		INNER JOIN [Codes.ResourceFormat] ON itbl.CodeId = [Codes.ResourceFormat].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) ResourceFormats
	FROM [Resource.Format] tbl
	GROUP BY ResourceIntId) otab) rf2 ON rv.ResourceIntId = rf2.ResourceIntId

-- Group Type Ids
LEFT JOIN (
	SELECT ResourceIntId, left(GroupTypes, LEN(GroupTypes) - 1) AS GroupTypeIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[GroupTypeId])+','
		FROM [Resource.GroupType] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) GroupTypes
	FROM [Resource.GroupType] tbl
	GROUP BY ResourceIntId) otab) gt1 ON rv.ResourceIntId = gt1.ResourceIntId
	
-- Group Types
LEFT JOIN (
	SELECT ResourceIntId, left(GroupTypes, LEN(GroupTypes) - 1) AS GroupTypes
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.GroupType].Title+','
		FROM [Resource.GroupType] itbl
		INNER JOIN [Codes.GroupType] ON itbl.GroupTypeId = [Codes.GroupType].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) GroupTypes
	FROM [Resource.Format] tbl
	GROUP BY ResourceIntId) otab) gt2 ON rv.ResourceIntId = gt2.ResourceIntId
	
-- Item Type Ids
LEFT JOIN (
	SELECT ResourceIntId, left(ItemTypes, LEN(ItemTypes) - 1) AS ItemTypeIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[ItemTypeId])+','
		FROM [Resource.ItemType] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) ItemTypes
	FROM [Resource.ItemType] tbl
	GROUP BY ResourceIntId) otab) it1 ON rv.ResourceIntId = it1.ResourceIntId
	
-- Item Types
LEFT JOIN (
	SELECT ResourceIntId, left(ItemTypes, LEN(ItemTypes) - 1) AS ItemTypes
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.ItemType].Title+','
		FROM [Resource.ItemType] itbl
		INNER JOIN [Codes.ItemType] ON itbl.ItemTypeId = [Codes.ItemType].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) ItemTypes
	FROM [Resource.Format] tbl
	GROUP BY ResourceIntId) otab) it2 ON rv.ResourceIntId = it2.ResourceIntId
	
-- Standard Ids
LEFT JOIN (
	SELECT ResourceIntId, left(Standards, LEN(Standards) - 1) AS StandardIds
	FROM (SELECT ResourceIntId, (
		SELECT isnull(convert(varchar,itbl.[StandardId]),'')+','
		FROM [Resource.Standard] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Standards
	FROM [Resource.Standard] tbl
	GROUP BY ResourceIntId) otab) rst1 on rv.ResourceIntId = rst1.ResourceIntId
	
-- Standards
LEFT JOIN (
	SELECT ResourceIntId, left(Standards, LEN(Standards) - 1) AS Standards
	FROM (SELECT ResourceIntId, (
		SELECT [StandardBody.Node].NotationCode+','
		FROM [Resource.Standard] itbl
		INNER JOIN [StandardBody.Node] ON itbl.StandardId = [StandardBody.Node].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Standards
	FROM [Resource.Standard] tbl
	GROUP BY ResourceIntId) otab) rst2 ON rv.ResourceIntId = rst2.ResourceIntId

GO
/****** Object:  View [dbo].[Resource.SearchableIndexView2]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[Resource.SearchableIndexView2]
AS
SELECT rv.Id AS ResourceVersionId, rv.ResourceIntId, rv.DocId, rv.Title, rv.[Description], rv.Publisher, rv.Created, rv.AccessRights, rk.Keywords,
	rs.Subjects, rl1.LanguageIds, rl2.Languages, cc1.ClusterIds, cc2.Clusters, ia1.AudienceIds,
	ia2.Audiences, el1.EducationLevelIds, el2.EducationLevels, rt1.ResourceTypeIds, rt2.ResourceTypes,
	rf1.ResourceFormatIds, rf2.ResourceFormats, gt1.GroupTypeIds, gt2.GroupTypes, it1.ItemTypeIds,
	it2.ItemTypes, rst1.StandardIds, rst2.Standards, rast.AssessmentTypeId, edu1.EducationUseId, 
	edu2.OriginalType, [Resource].ResourceURL
FROM [Resource.Version] rv
INNER JOIN dbo.[Resource] ON dbo.[Resource].Id = rv.ResourceIntId
 
-- Keywords
LEFT JOIN (
	SELECT ResourceIntId, left(Keywords, LEN(Keywords) - 1) AS Keywords
	FROM (SELECT ResourceIntId, (
		SELECT itbl.[Keyword]+','
		FROM [Resource.Keyword] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Keywords
	FROM [Resource.Keyword] tbl
	GROUP BY ResourceIntId) otab) rk ON rv.ResourceIntId = rk.ResourceIntId

-- Subjects
LEFT JOIN (
	SELECT ResourceIntId, left(Subjects, LEN(Subjects) - 1) AS Subjects
	FROM (SELECT ResourceIntId, (
		SELECT itbl.[Subject]+','
		FROM [Resource.Subject] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Subjects
	FROM [Resource.Subject] tbl
	GROUP BY ResourceIntId) otab) rs ON rv.ResourceIntId = rs.ResourceIntId

-- Language Ids
LEFT JOIN (
	SELECT ResourceIntId, left(Languages, LEN(Languages) - 1) AS LanguageIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[LanguageId])+','
		FROM [Resource.Language] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Languages
	FROM [Resource.Language] tbl
	GROUP BY ResourceIntId) otab) rl1 ON rv.ResourceIntId = rl1.ResourceIntId

-- Languages
LEFT JOIN (
	SELECT ResourceIntId, left(Languages, LEN(Languages) - 1) AS Languages
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.Language].Title+','
		FROM [Resource.Language] itbl
		INNER JOIN [Codes.Language] ON itbl.LanguageId = [Codes.Language].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Languages
	FROM [Resource.Language] tbl
	GROUP BY ResourceIntId) otab) rl2 on rv.ResourceIntId = rl2.ResourceIntId

-- Career Cluster Ids
LEFT JOIN (
	SELECT ResourceIntId, left(Clusters, LEN(Clusters) - 1) AS ClusterIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[ClusterId])+','
		FROM [Resource.Cluster] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Clusters
	FROM [Resource.Cluster] tbl
	GROUP BY ResourceIntId) otab) cc1 ON rv.ResourceIntId = cc1.ResourceIntId
	
-- Career Clusters
LEFT JOIN (
SELECT ResourceIntId, left(Clusters, LEN(Clusters) - 1) AS Clusters
	FROM (SELECT ResourceIntId, (
		SELECT '"'+[CareerCluster].IlPathwayName+'",'
		FROM [Resource.Cluster] itbl
		INNER JOIN [CareerCluster] ON itbl.ClusterId = [CareerCluster].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Clusters
	FROM [Resource.Cluster] tbl
	GROUP BY ResourceIntId) otab) cc2 ON rv.ResourceIntId = cc2.ResourceIntId
	
-- Intended Audience Ids
LEFT JOIN(
	SELECT ResourceIntId, left(Audiences, LEN(Audiences) - 1) AS AudienceIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[AudienceId])+','
		FROM [Resource.IntendedAudience] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Audiences
	FROM [Resource.IntendedAudience] tbl
	GROUP BY ResourceIntId) otab) ia1 ON rv.ResourceIntId = ia1.ResourceIntId
	
-- Intended Audiences
LEFT JOIN (
	SELECT ResourceIntId, left(Audiences, LEN(Audiences) - 1) AS Audiences
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.AudienceType].Title+','
		FROM [Resource.IntendedAudience] itbl
		INNER JOIN [Codes.AudienceType] ON itbl.AudienceId = [Codes.AudienceType].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Audiences
	FROM [Resource.IntendedAudience] tbl
	GROUP BY ResourceIntId) otab) ia2 ON rv.ResourceIntId = ia2.ResourceIntId
	
-- Education Level Ids
LEFT JOIN (
	SELECT ResourceIntId, left(EducationLevels, LEN(EducationLevels) - 1) AS EducationLevelIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[PathwaysEducationLevelId])+','
		FROM [Resource.EducationLevel] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) EducationLevels
	FROM [Resource.EducationLevel] tbl
	GROUP BY ResourceIntId) otab) el1 ON rv.ResourceIntId = el1.ResourceIntId
	
-- Education Level
LEFT JOIN (
	SELECT ResourceIntId, left(EducationLevels, LEN(EducationLevels) - 1) AS EducationLevels
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.PathwaysEducationLevel].Title+','
		FROM [Resource.EducationLevel] itbl
		INNER JOIN [Codes.PathwaysEducationLevel] ON itbl.EducationLevelId = [Codes.PathwaysEducationLevel].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) EducationLevels
	FROM [Resource.EducationLevel] tbl
	GROUP BY ResourceIntId) otab) el2 ON rv.ResourceIntId = el2.ResourceIntId
	
-- Resource Type Ids
LEFT JOIN (
	SELECT ResourceIntId, left(ResourceTypes, LEN(ResourceTypes) - 1) AS ResourceTypeIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[ResourceTypeId])+','
		FROM [Resource.ResourceType] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) ResourceTypes
	FROM [Resource.ResourceType] tbl
	GROUP BY ResourceIntId) otab) rt1 ON rv.ResourceIntId = rt1.ResourceIntId
	
-- Resource Type
LEFT JOIN (
	SELECT ResourceIntId, left(ResourceTypes, LEN(ResourceTypes) - 1) AS ResourceTypes
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.ResourceType].Title+','
		FROM [Resource.ResourceType] itbl
		INNER JOIN [Codes.ResourceType] ON itbl.ResourceTypeId = [Codes.ResourceType].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) ResourceTypes
	FROM [Resource.ResourceType] tbl
	GROUP BY ResourceIntId) otab) rt2 ON rv.ResourceIntId = rt2.ResourceIntId
	
-- Resource Format Ids
LEFT JOIN (
	SELECT ResourceIntId, left(ResourceFormats, LEN(ResourceFormats) - 1) AS ResourceFormatIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[CodeId])+','
		FROM [Resource.Format] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) ResourceFormats
	FROM [Resource.Format] tbl
	GROUP BY ResourceIntId) otab) rf1 ON rv.ResourceIntId = rf1.ResourceIntId
	
-- Resource Formats
LEFT JOIN (
	SELECT ResourceIntId, left(ResourceFormats, LEN(ResourceFormats) - 1) AS ResourceFormats
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.ResourceFormat].Title+','
		FROM [Resource.Format] itbl
		INNER JOIN [Codes.ResourceFormat] ON itbl.CodeId = [Codes.ResourceFormat].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) ResourceFormats
	FROM [Resource.Format] tbl
	GROUP BY ResourceIntId) otab) rf2 ON rv.ResourceIntId = rf2.ResourceIntId

-- Group Type Ids
LEFT JOIN (
	SELECT ResourceIntId, left(GroupTypes, LEN(GroupTypes) - 1) AS GroupTypeIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[GroupTypeId])+','
		FROM [Resource.GroupType] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) GroupTypes
	FROM [Resource.GroupType] tbl
	GROUP BY ResourceIntId) otab) gt1 ON rv.ResourceIntId = gt1.ResourceIntId
	
-- Group Types
LEFT JOIN (
	SELECT ResourceIntId, left(GroupTypes, LEN(GroupTypes) - 1) AS GroupTypes
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.GroupType].Title+','
		FROM [Resource.GroupType] itbl
		INNER JOIN [Codes.GroupType] ON itbl.GroupTypeId = [Codes.GroupType].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) GroupTypes
	FROM [Resource.Format] tbl
	GROUP BY ResourceIntId) otab) gt2 ON rv.ResourceIntId = gt2.ResourceIntId
	
-- Item Type Ids
LEFT JOIN (
	SELECT ResourceIntId, left(ItemTypes, LEN(ItemTypes) - 1) AS ItemTypeIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[ItemTypeId])+','
		FROM [Resource.ItemType] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) ItemTypes
	FROM [Resource.ItemType] tbl
	GROUP BY ResourceIntId) otab) it1 ON rv.ResourceIntId = it1.ResourceIntId
	
-- Item Types
LEFT JOIN (
	SELECT ResourceIntId, left(ItemTypes, LEN(ItemTypes) - 1) AS ItemTypes
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.ItemType].Title+','
		FROM [Resource.ItemType] itbl
		INNER JOIN [Codes.ItemType] ON itbl.ItemTypeId = [Codes.ItemType].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) ItemTypes
	FROM [Resource.Format] tbl
	GROUP BY ResourceIntId) otab) it2 ON rv.ResourceIntId = it2.ResourceIntId
	
-- Standard Ids
LEFT JOIN (
	SELECT ResourceIntId, left(Standards, LEN(Standards) - 1) AS StandardIds
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[StandardId])+','
		FROM [Resource.Standard] itbl
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Standards
	FROM [Resource.Standard] tbl
	GROUP BY ResourceIntId) otab) rst1 on rv.ResourceIntId = rst1.ResourceIntId
	
-- Standards
LEFT JOIN (
	SELECT ResourceIntId, left(Standards, LEN(Standards) - 1) AS Standards
	FROM (SELECT ResourceIntId, (
		SELECT [StandardBody.Node].NotationCode+','
		FROM [Resource.Standard] itbl
		INNER JOIN [StandardBody.Node] ON itbl.StandardId = [StandardBody.Node].Id
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) Standards
	FROM [Resource.Standard] tbl
	GROUP BY ResourceIntId) otab) rst2 ON rv.ResourceIntId = rst2.ResourceIntId
	
-- Assessment Type XXXXXXXXXXX
LEFT JOIN (
	SELECT ResourceIntId, left(AssessmentType, LEN(AssessmentType) - 1) AS AssessmentTypeId
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[AssessmentTypeId])+','
		FROM [Resource.AssessmentType] itbl		
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) AssessmentType
	FROM [Resource.AssessmentType] tbl
	GROUP BY ResourceIntId) otab) rast ON rv.ResourceIntId = rast.ResourceIntId
		
-- Education Use Id XXXXXXXXXXX
LEFT JOIN (
	SELECT ResourceIntId, left(EducationUses, LEN(EducationUses) - 1) AS EducationUseId
	FROM (SELECT ResourceIntId, (
		SELECT convert(varchar,itbl.[EducationUseId])+','
		FROM [Resource.EducationUse] itbl		
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) EducationUses
	FROM [Resource.EducationUse] tbl
	GROUP BY ResourceIntId) otab) edu1 ON rv.ResourceIntId = edu1.ResourceIntId	

-- Education Use OriginalType XXXXXXXXXXX
LEFT JOIN (
	SELECT ResourceIntId, left(EducationUses, LEN(EducationUses) - 1) AS OriginalType
	FROM (SELECT ResourceIntId, (
		SELECT [Codes.EducationalUse].Title+','
		FROM [Resource.EducationUse] itbl
		INNER JOIN [Codes.EducationalUse] ON itbl.EducationUseId = [Codes.EducationalUse].Id		
		WHERE itbl.ResourceIntId = tbl.ResourceIntId
		for xml path('')) EducationUses
	FROM [Resource.EducationUse] tbl
	GROUP BY ResourceIntId) otab) edu2 ON rv.ResourceIntId = edu2.ResourceIntId


GO
/****** Object:  View [dbo].[Resource.StandardEvaluationList]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*
USE [Isle_IOER]
GO


where resourceIntId = 450310


*/

Create VIEW [dbo].[Resource.StandardEvaluationList] AS

SELECT    
	base.Id    
	, base.ResourceIntId
	, base.StandardId
	, reval.CreatedById
	, reval.Created
	, stan.NotationCode
	, stan.Description
    , reval.Score 

FROM dbo.[Resource.Standard] AS base 
INNER JOIN dbo.[Resource.StandardEvaluation] reval ON base.Id = reval.ResourceStandardId 
INNER JOIN dbo.[StandardBody.Node] stan ON base.StandardId  = stan.Id

	



GO
/****** Object:  View [dbo].[Resource.StandardEvaluationSummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
USE [Isle_IOER]
GO


where resourceIntId = 450310


*/

Create VIEW [dbo].[Resource.StandardEvaluationSummary] AS

SELECT        
	base.ResourceIntId
	, base.StandardId
	, stan.NotationCode
	, stan.Description
    --, reval.Score ,

	,sum(reval.Score)/count(*) As AverageScorePercent
	,count(*) As TotalEvals
FROM dbo.[Resource.Standard] AS base 
INNER JOIN dbo.[Resource.StandardEvaluation] reval ON base.Id = reval.ResourceStandardId 
INNER JOIN dbo.[StandardBody.Node] stan ON base.StandardId  = stan.Id

group by base.ResourceIntId
	, base.StandardId
	, stan.NotationCode
	, stan.Description
	


GO
/****** Object:  View [dbo].[Resource.SubjectsCsvList]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
SELECT top 200 ResourceId
      ,[Subjects]
  FROM [dbo].[Resource.SubjectsCsvList]
  where 
  ResourceId= '0987D8FA-5E7D-4C28-8423-000064517B67'
OR  
  ResourceId= 'D15CA074-EEB2-4BC7-AAA9-0007771B3A77'
  


SELECT top 200 ResourceId
      ,[Subjects]
  FROM [dbo].[Resource.SubjectsCsvList]
  where len(Subjects) > 0
  order by 1
  
SELECT count(*)
  FROM [dbo].[Resource.SubjectsCsvList]

*/
CREATE VIEW [dbo].[Resource.SubjectsCsvList]
AS
SELECT     TOP (100) PERCENT 
base.[Id] As ResourceIntId,
    CASE
          WHEN Subjects IS NULL THEN ''
          WHEN len(Subjects) = 0 THEN ''
          ELSE left(Subjects,len(Subjects)-1)
    END AS Subjects

FROM [dbo].[Resource] base

CROSS APPLY (
    SELECT rsub.Subject  + ', '
   -- ,rsub.ResourceId
    FROM dbo.[Resource.Subject] rsub   
    WHERE base.Id = rsub.ResourceIntId
    FOR XML Path('') ) D (Subjects)


GO
/****** Object:  View [dbo].[Resource.SubjectsList_RP]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[Resource.SubjectsList_RP]
AS
SELECT     TOP (100) PERCENT 
dbo.[Resource.Property].ResourceId, 
rpt.Title, 
dbo.[Resource.Property].Value AS [Subject]
FROM         dbo.[Resource.Property] 
INNER JOIN dbo.[Codes.ResPropertyType] rpt ON dbo.[Resource.Property].PropertyTypeId = rpt.Id
WHERE     (rpt.Title = 'subject')
ORDER BY dbo.[Resource.Property].ResourceId, Subject


GO
/****** Object:  View [dbo].[Resource.TagSubjectsCsvList]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*

SELECT top 1000 [ResourceIntId]
      ,[Subjects]
  FROM [dbo].[Resource.TagSubjectsCsvList]
  where subjects like '%technology%'



  
SELECT count(*)
  FROM [dbo].[Resource.TagSubjectsCsvList]

*/
CREATE VIEW [dbo].[Resource.TagSubjectsCsvList]
AS
SELECT   
base.[Id] As ResourceIntId,
    CASE
          WHEN Subjects IS NULL THEN ''
          WHEN len(Subjects) = 0 THEN ''
          ELSE left(Subjects,len(Subjects)-1)
    END AS Subjects

FROM [dbo].[Resource] base

CROSS APPLY (
    SELECT tv.Title + ', '
   -- ,rsub.ResourceId
    FROM dbo.[Resource.Tag] rsub  inner join [Codes.TagValue] tv on rsub.TagValueId = tv.Id 
    WHERE base.Id = rsub.ResourceIntId and tv.CategoryId = 29
    FOR XML Path('') 
	) D (Subjects)
	where Subjects is not null


GO
/****** Object:  View [dbo].[Resource.VersionRelevance]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[Resource.VersionRelevance]   AS
SELECT 	tbl.Id, TitlePoints, DescriptionPoints, AccessRightsPoints, InteractivityRights, EdUsePoints, LanguK12EdPoints, ClusterPoints, 
AudiencePoints, StandardPoints, TypePoints, FormatPoints, GroupTypePoints, K12EdPoints, AdultEdPoints, EdPoints,
TitlePoints + DescriptionPoints + AccessRightsPoints + InteractivityRights + EdUsePoints + LanguK12EdPoints + ClusterPoints + 
		AudiencePoints + StandardPoints + TypePoints +  FormatPoints +  GroupTypePoints + EdPoints AS TotalPoints
FROM (SELECT     Id, 
	CASE 
		WHEN rv.Title <> rv.[Description] THEN 1 
		ELSE 0 END AS TitlePoints,
	CASE
		WHEN rv.Title = rv.[Description] THEN 0
		WHEN len(rv.[Description]) > 400 THEN 5
		WHEN LEN(rv.[Description]) > 300 THEN 4
		WHEN LEN(rv.[Description]) > 200 THEN 3
		WHEN LEN(rv.[Description]) > 100 THEN 2
		WHEN LEN(rv.[Description]) > 0   THEN 1
		ELSE 0 END AS DescriptionPoints,
	CASE 
		WHEN rv.AccessRights = 'Unknown' THEN 0 
		WHEN rv.AccessRights IS NOT NULL THEN 1
		ELSE 0 END AS AccessRightsPoints,
	CASE 
		WHEN rv.InteractivityTypeId IS NOT NULL THEN 1
		ELSE 0 END AS InteractivityRights,
	CASE
		WHEN Lang.Fraction > 0 AND Lang.Fraction < 0.5 THEN 2
		WHEN Lang.Fraction >= 0.5 THEN 1
		ELSE 0 END AS LanguK12EdPoints,
	CASE
		WHEN EdUse.Fraction > 0 AND EdUse.Fraction < 0.5 THEN 2
		WHEN EdUse.Fraction >= 0.5 THEN 1
		ELSE 0 END AS EdUsePoints,
	CASE
		WHEN Cluster.Fraction > 0 AND Cluster.Fraction < 0.5 THEN 2
		WHEN Cluster.Fraction >= 0.5 THEN 1
		ELSE 0 END AS ClusterPoints,
	CASE 
		WHEN Audience.Fraction > 0 AND Audience.Fraction < 0.5 THEN 2
		WHEN Audience.Fraction >= 0.5 THEN 1
		ELSE 0 END AS AudiencePoints,
	CASE 
		WHEN NbrStandards > 0 AND NbrStandards <= 3 THEN (NbrStandards * 3)
		WHEN NbrStandards > 3 AND NbrStandards <= 5 THEN 9 + ((NbrStandards - 3) * 2)
		WHEN NbrStandards > 5 AND NbrStandards <= 7 THEN 13 + NbrStandards - 5
		WHEN NbrStandards > 7 THEN 15
		ELSE 0 END AS StandardPoints,
	CASE
		WHEN Type.Fraction > 0 AND Type.Fraction < 0.5 THEN 2
		WHEN Type.Fraction >= 0.5 THEN 1
		ELSE 0 END AS TypePoints,
	CASE
		WHEN [Format].Fraction > 0 AND [Format].Fraction < 0.5 THEN 2
		WHEN [Format].Fraction >= 0.5 THEN 1
		ELSE 0 END AS FormatPoints,
	CASE
		WHEN [GroupType].ResourceIntId IS NOT NULL THEN 1
		ELSE 0 END AS GroupTypePoints,
	CASE
		WHEN [Age].ResourceIntId IS NOT NULL THEN K12EdPoints
		ELSE 0 END AS K12EdPoints,
	CASE
		WHEN [AdultEd].ResourceIntId IS NOT NULL THEN AdultEdPoints
		ELSE 0 END AS AdultEdPoints,
	CASE
		WHEN [Age].ResourceIntId IS NOT NULL AND [AdultEd].ResourceIntId IS NULL THEN K12EdPoints
		WHEN [Age].ResourceIntId IS NULL AND [AdultEd].ResourceIntId IS NOT NULL THEN AdultEdPoints
		WHEN [Age].ResourceIntId IS NOT NULL AND [AdultEd].ResourceIntId IS NOT NULL THEN (K12EdPoints + AdultEdPoints) / 2
		ELSE 0 END AS EdPoints
FROM         [dbo].[Resource.Version] rv
LEFT JOIN (
	SELECT ResourceIntId, ResourceNbrEdUse, NbrEdUse, convert(float,ResourceNbrEdUse) / NbrEdUse AS Fraction
	FROM (SELECT ResourceIntId, COUNT([Resource.EducationUse].ResourceIntId) AS ResourceNbrEdUse
		FROM [dbo].[Resource.EducationUse] GROUP BY ResourceIntId) tbl
	LEFT JOIN (SELECT COUNT(*) AS NbrEdUse FROM [dbo].[Codes.EducationalUse]) tbl2 on 1 = 1) EdUse ON rv.ResourceIntId = EdUse.ResourceIntId
LEFT JOIN (
	SELECT ResourceIntId, ResourceNbrLanguages, NbrLanguages, convert(float,ResourceNbrLanguages) / NbrLanguages AS Fraction
	FROM (SELECT ResourceIntId, COUNT([Resource.Language].ResourceIntId) AS ResourceNbrLanguages
		FROM [dbo].[Resource.Language] GROUP BY ResourceIntId) tbl
	LEFT JOIN (SELECT COUNT(*) AS NbrLanguages FROM [dbo].[Codes.Language] WHERE IsPathwaysLanguage = 'True') tbl2 on 1 = 1) Lang ON rv.ResourceIntId = Lang.ResourceIntId
LEFT JOIN (
	SELECT ResourceIntId, ResourceNbrClusters, NbrClusters, convert(float,ResourceNbrClusters) / NbrClusters AS Fraction
		FROM (SELECT ResourceIntId, COUNT([Resource.Cluster].ResourceIntId) AS ResourceNbrClusters
		FROM [dbo].[Resource.Cluster] GROUP BY ResourceIntId) tbl
	LEFT JOIN (SELECT COUNT(*) AS NbrClusters FROM [dbo].[CareerCluster] WHERE IsIlPathway = 'True') tbl2 on 1 = 1) Cluster ON rv.ResourceIntId = Cluster.ResourceIntId
LEFT JOIN (
	SELECT ResourceIntId, ResourceNbrAudience, NbrAudience, convert(float,ResourceNbrAudience) / NbrAudience AS Fraction
	FROM (SELECT ResourceIntId, COUNT([Resource.IntendedAudience].ResourceIntId) AS ResourceNbrAudience
		FROM [dbo].[Resource.IntendedAudience] GROUP BY ResourceIntId) tbl
	LEFT JOIN (SELECT COUNT(*) AS NbrAudience FROM [dbo].[Codes.AudienceType]) tbl2 on 1 = 1) Audience ON rv.ResourceIntId = Audience.ResourceIntId
LEFT JOIN (SELECT ResourceIntId, COUNT(*) AS NbrStandards FROM [dbo].[Resource.Standard] GROUP BY ResourceIntId) [Standard] ON rv.ResourceIntId = [Standard].ResourceIntId
LEFT JOIN (
	SELECT ResourceIntId, ResourceNbrType, NbrType, convert(float,ResourceNbrType) / NbrType AS Fraction
	FROM (SELECT ResourceIntId, COUNT([Resource.ResourceType].ResourceIntId) AS ResourceNbrType
		FROM [dbo].[Resource.ResourceType] GROUP BY ResourceIntId) tbl
	LEFT JOIN (SELECT COUNT(*) AS NbrType FROM [dbo].[Codes.ResourceType] WHERE IsActive = 'True') tbl2 on 1 = 1) [Type] ON rv.ResourceIntId = [Type].ResourceIntId
LEFT JOIN (SELECT ResourceIntId, ResourceNbrFormats, NbrFormats, convert(float,ResourceNbrFormats) / NbrFormats AS Fraction
	FROM (SELECT ResourceIntId, COUNT([Resource.Format].ResourceIntId) AS ResourceNbrFormats
		FROM [dbo].[Resource.Format] GROUP BY ResourceIntId) tbl
	LEFT JOIN (SELECT COUNT(*) AS NbrFormats FROM [dbo].[Codes.ResourceFormat]) tbl2 on 1 = 1) [Format] ON rv.ResourceIntId = [Format].ResourceIntId
LEFT JOIN (
	SELECT ResourceIntId, 
		CASE
			WHEN AgeSpan > 6 THEN 0
			ELSE 6 - AgeSpan END AS K12EdPoints
	FROM [dbo].[Resource.AgeRange]) [Age] ON rv.ResourceIntId = [Age].ResourceIntId
LEFT JOIN (
	SELECT ResourceIntId, ResourceNbrGradeLevels, NbrGradeLevels, CONVERT(float, 4) / (NbrGradeLevels - 1) * (1 - ResourceNbrGradeLevels) + 5 AS [AdultEdPoints]
	FROM (
		SELECT ResourceIntId, COUNT(ResourceIntId) AS ResourceNbrGradeLevels
		FROM [dbo].[Resource.GradeLevel]
		INNER JOIN [dbo].[Codes.GradeLevel] ON [Resource.GradeLevel].GradeLevelId = [Codes.GradeLevel].Id
		WHERE AgeLevel >= 18 AND IsPathwaysLevel = 'True'
		GROUP BY ResourceIntId) tbl
	LEFT JOIN (
		SELECT COUNT(*) AS NbrGradeLevels
		FROM [dbo].[Codes.GradeLevel]
		WHERE AgeLevel >= 18 AND IsPathwaysLevel = 'True' AND WarehouseTotal > 0) tbl2 ON 1=1) [AdultEd] ON rv.ResourceIntId = [AdultEd].ResourceIntId
LEFT JOIN (
	SELECT ResourceIntId, ResourceNbrGroupType, NbrGroupType, convert(float,ResourceNbrGroupType) / NbrGroupType AS Fraction
	FROM (SELECT ResourceIntId, COUNT([Resource.GroupType].ResourceIntId) AS ResourceNbrGroupType
		FROM [dbo].[Resource.GroupType] GROUP BY ResourceIntId) tbl
	LEFT JOIN (SELECT COUNT(*) AS NbrGroupType FROM [dbo].[Codes.GroupType]) tbl2 on 1 = 1) [GroupType] ON rv.ResourceIntId = [GroupType].ResourceIntId) tbl


GO
/****** Object:  View [dbo].[Resource_TagSummary]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*

SELECT [ResourceTagId]
      ,[ResourceIntId]
      ,[TagValueId]
      ,[CodeId]
      ,[TagTitle]
      ,[CategoryId]
      ,[CategoryTitle]
      ,[created]
  FROM [dbo].[Resource_TagSummary]
  --where CategoryId= 2
  order by created desc
GO



*/
CREATE VIEW [dbo].[Resource_TagSummary]
AS
SELECT        
	dbo.[Resource.Tag].Id AS ResourceTagId, 
	dbo.[Resource.Tag].ResourceIntId, 
	dbo.[Resource.Tag].TagValueId, 
	dbo.[Codes.TagValue].CodeId, 
    dbo.[Codes.TagValue].Title AS TagTitle, 
	dbo.[Codes.TagValue].CategoryId, 
	dbo.[Codes.TagValue].AliasValues,
	dbo.[Codes.TagCategory].Title AS CategoryTitle,
	dbo.[Resource.Tag].created,
	dbo.[Resource.Tag].CreatedById
FROM dbo.[Resource.Tag] 
	INNER JOIN dbo.[Codes.TagValue] ON dbo.[Resource.Tag].TagValueId = dbo.[Codes.TagValue].Id 
	INNER JOIN dbo.[Codes.TagCategory] ON dbo.[Codes.TagValue].CategoryId = dbo.[Codes.TagCategory].Id



GO
/****** Object:  View [dbo].[StandardList]    Script Date: 2/22/2015 10:28:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[StandardList] AS
SELECT DISTINCT base.Id, base.parentId, notationCode, standardUrl AS url, altUrl, 
	CASE
		WHEN glt.GradeLevels IS NOT NULL THEN glt.gradeLevels
		--WHEN pglt.GradeLevels IS NOT NULL THEN pglt.gradeLevels
		--WHEN gpglt.GradeLevels IS NOT NULL THEN gpglt.gradeLevels
		--WHEN ggpglt.GradeLevels IS NOT NULL THEN ggpglt.gradeLevels
		ELSE NULL
	END AS gradeLevels, [description]
FROM [StandardBody.Node] base
LEFT JOIN (SELECT parentId, left(GradeLevel, LEN(GradeLevel) - 1) AS GradeLevels
	FROM (SELECT parentId, (
		SELECT convert(varchar,isnull(cgl.[NsdlTitle],''))+','
		FROM [StandardBody.NodeGradeLevel] itbl
		INNER JOIN [Codes.GradeLevel] cgl ON itbl.GradeLevelId = cgl.Id
		WHERE itbl.parentId = tbl.parentId
		for xml path('')) GradeLevel
	FROM [StandardBody.NodeGradeLevel] tbl
	GROUP BY parentId) otab) glt ON base.Id = glt.parentId
/*LEFT JOIN (SELECT parentId, left(GradeLevel, len(GradeLevel) - 1) AS GradeLevels
	FROM (SELECT DISTINCT node.parentId, (
		SELECT convert(varchar,isnull(cgl.[NsdlTitle],''))+','
		FROM [StandardBody.NodeGradeLevel] itbl
		INNER JOIN [Codes.GradeLevel] cgl ON itbl.GradeLevelId = cgl.Id
		WHERE itbl.parentId = tbl.parentId
		for xml path('')) GradeLevel
	FROM [StandardBody.NodeGradeLevel] tbl
	INNER JOIN [StandardBody.Node] node ON tbl.parentId = node.Id
	GROUP BY tbl.parentId, node.parentId) otab) pglt ON base.Id = pglt.parentId*/
GO
ALTER TABLE [dbo].[ApplicationLog] ADD  CONSTRAINT [DF_ApplicationLog_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Audit.AccessibilityApi_Orphan] ADD  CONSTRAINT [DF_Audit.AccessibilityApi_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.AccessibilityApi_Orphan] ADD  CONSTRAINT [DF_Audit.AccessibilityApi_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.AccessibilityControl_Orphan] ADD  CONSTRAINT [DF_Audit.AccessibilityControl_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.AccessibilityControl_Orphan] ADD  CONSTRAINT [DF_Audit.AccessibilityControl_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.AccessibilityFeature_Orphan] ADD  CONSTRAINT [DF_Audit.AccessibilityFeature_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.AccessibilityFeature_Orphan] ADD  CONSTRAINT [DF_Audit.AccessibilityFeature_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.AccessibilityHazard_Orphan] ADD  CONSTRAINT [DF_Audit.AccessibilityHazard_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.AccessibilityHazard_Orphan] ADD  CONSTRAINT [DF_Audit.AccessibilityHazard_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.AssessmentType_Orphan] ADD  CONSTRAINT [DF_Audit.AssessmentType_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.AssessmentType_Orphan] ADD  CONSTRAINT [DF_Audit.AssessmentType_Orphan_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Audit.AssessmentType_Orphan] ADD  CONSTRAINT [DF_Audit.AssessmentType_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.AudienceType_Orphan] ADD  CONSTRAINT [DF_Audit.AudienceType_Orphan_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Audit.AudienceType_Orphan] ADD  CONSTRAINT [DF_Audit.AudienceType_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.AudienceType_Orphan] ADD  CONSTRAINT [DF_Audit.AudienceType_Orphan_FoundMapping1]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Audit.AudienceType_Orphan] ADD  CONSTRAINT [DF_Audit.AudienceType_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.EducationLevel_Orphan] ADD  CONSTRAINT [DF_Audit.EducationLevel_Orphan_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Audit.EducationLevel_Orphan] ADD  CONSTRAINT [DF_Audit.EducationLevel_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.EducationLevel_Orphan] ADD  CONSTRAINT [DF_Audit.EducationLevel_Orphan_FoundMapping1]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Audit.EducationLevel_Orphan] ADD  CONSTRAINT [DF_Audit.EducationLevel_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.EducationUse_Orphan] ADD  CONSTRAINT [DF_Audit.EducationUse_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.EducationUse_Orphan] ADD  CONSTRAINT [DF_Audit.EducationUse_Orphan_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Audit.EducationUse_Orphan] ADD  CONSTRAINT [DF_Audit.EducationUse_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.GradeLevel_Orphan] ADD  CONSTRAINT [DF_Audit.GradeLevel_Orphan_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Audit.GradeLevel_Orphan] ADD  CONSTRAINT [DF_Audit.GradeLevel_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.GradeLevel_Orphan] ADD  CONSTRAINT [DF_Audit.GradeLevel_Orphan_FoundMapping1]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Audit.GradeLevel_Orphan] ADD  CONSTRAINT [DF_Audit.GradeLevel_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.GroupType_Orphan] ADD  CONSTRAINT [DF_Audit.GroupType_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.GroupType_Orphan] ADD  CONSTRAINT [DF_Audit.GroupType_Orphan_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Audit.GroupType_Orphan] ADD  CONSTRAINT [DF_Audit.GroupType_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.ItemType_Orphan] ADD  CONSTRAINT [DF_Audit.ItemType_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.ItemType_Orphan] ADD  CONSTRAINT [DF_Audit.ItemType_Orphan_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Audit.ItemType_Orphan] ADD  CONSTRAINT [DF_Audit.ItemType_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.Language_Orphan] ADD  CONSTRAINT [DF_Audit.Language_Orphan_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Audit.Language_Orphan] ADD  CONSTRAINT [DF_Audit.Language_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.Language_Orphan] ADD  CONSTRAINT [DF_Audit.Language_Orphan_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Audit.Language_Orphan] ADD  CONSTRAINT [DF_Audit.Language_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.ResourceFormat_Orphan] ADD  CONSTRAINT [DF_Audit.ResourceFormat_Orphan_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Audit.ResourceFormat_Orphan] ADD  CONSTRAINT [DF_Audit.ResourceFormat_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.ResourceFormat_Orphan] ADD  CONSTRAINT [DF_Audit.ResourceFormat_Orphan_FoundMapping1]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Audit.ResourceFormat_Orphan] ADD  CONSTRAINT [DF_Audit.ResourceFormat_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Audit.ResourceType_Orphan] ADD  CONSTRAINT [DF_Audit.ResourceType_Orphan_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Audit.ResourceType_Orphan] ADD  CONSTRAINT [DF_Audit.ResourceType_Orphan_FoundMapping]  DEFAULT ((0)) FOR [FoundMapping]
GO
ALTER TABLE [dbo].[Audit.ResourceType_Orphan] ADD  CONSTRAINT [DF_Audit.ResourceType_Orphan_FoundMapping1]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Audit.ResourceType_Orphan] ADD  CONSTRAINT [DF_Audit.ResourceType_Orphan_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Blacklist.Hosts] ADD  CONSTRAINT [DF_Blacklist.Hosts_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Blacklist.Hosts] ADD  CONSTRAINT [DF_Blacklist.Hosts_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[CareerCluster] ADD  CONSTRAINT [DF_CareerCluster_IsHighGrowth]  DEFAULT ((0)) FOR [IsHighGrowth]
GO
ALTER TABLE [dbo].[CareerCluster] ADD  CONSTRAINT [DF_CareerCluster_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[CareerCluster] ADD  CONSTRAINT [DF_CareerCluster_IsHighGrowth1]  DEFAULT ((0)) FOR [IsIlPathway]
GO
ALTER TABLE [dbo].[Codes.AccessibilityApi] ADD  CONSTRAINT [DF_Codes.AccessibilityApi_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.AccessibilityApi] ADD  CONSTRAINT [DF_Codes.AccessibilityApi_SortOrder]  DEFAULT ((20)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.AccessibilityControl] ADD  CONSTRAINT [DF_Codes.AccessibilityControl_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.AccessibilityControl] ADD  CONSTRAINT [DF_Codes.AccessibilityControl_SortOrder]  DEFAULT ((20)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.AccessibilityFeature] ADD  CONSTRAINT [DF_Codes.AccessibilityFeature_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.AccessibilityFeature] ADD  CONSTRAINT [DF_Codes.AccessibilityFeature_SortOrder]  DEFAULT ((20)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.AccessibilityHazard] ADD  CONSTRAINT [DF_Codes.AccessibilityHazard_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.AccessibilityHazard] ADD  CONSTRAINT [DF_Codes.AccessibilityHazard_SortOrder]  DEFAULT ((20)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.AlignmentDegree] ADD  CONSTRAINT [DF_Codes.AlignmentDegree_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.AlignmentDegree] ADD  CONSTRAINT [DF_Codes.AlignmentDegree_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.AlignmentType] ADD  CONSTRAINT [DF_Codes.AlignmentType_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.AlignmentType] ADD  CONSTRAINT [DF_Codes.AlignmentType_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.AssessmentType] ADD  CONSTRAINT [DF_Codes.AssessmentType_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.AssessmentType] ADD  CONSTRAINT [DF_Codes.AssessmentType_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.AssessmentType] ADD  CONSTRAINT [DF_Codes.AssessmentType_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.AudienceType] ADD  CONSTRAINT [DF_Codes.AudienceType_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.AudienceType] ADD  CONSTRAINT [DF_Codes.AudienceType_WarehouseTotal]  DEFAULT ((1)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.CareerPlanning] ADD  CONSTRAINT [DF_Codes.CareerPlanning_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.CareerPlanning] ADD  CONSTRAINT [DF_Codes.CareerPlanning_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.Disability] ADD  CONSTRAINT [DF_Codes.Disability_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.Disability] ADD  CONSTRAINT [DF_Codes.Disability_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.EducationalUse] ADD  CONSTRAINT [DF_Codes.EducationalUse_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.EducationalUse] ADD  CONSTRAINT [DF_Codes.EducationalUse_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.EducationalUseCategory] ADD  CONSTRAINT [DF_Codes.EducationalUseCategory_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.EmployerProgram] ADD  CONSTRAINT [DF_Codes.EmployerProgram_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.EmployerProgram] ADD  CONSTRAINT [DF_Codes.EmployerProgram_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.GradeLevel] ADD  CONSTRAINT [DF_Codes.GradeLevel_IsPathwaysLevel]  DEFAULT ((1)) FOR [IsPathwaysLevel]
GO
ALTER TABLE [dbo].[Codes.GradeLevel] ADD  CONSTRAINT [DF_Codes.GradeLevel_IsK12Level]  DEFAULT ((0)) FOR [IsK12Level]
GO
ALTER TABLE [dbo].[Codes.GradeLevel] ADD  CONSTRAINT [DF_Codes.GradeLevel_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.GradeLevel] ADD  CONSTRAINT [DF_Codes.GradeLevel_IsEducationBand]  DEFAULT ('False') FOR [IsEducationBand]
GO
ALTER TABLE [dbo].[Codes.GroupType] ADD  CONSTRAINT [DF_Codes.GroupType_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.InteractivityType] ADD  CONSTRAINT [DF_Codes.InteractivityType_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.ItemType] ADD  CONSTRAINT [DF_Codes.ItemType_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.ItemType] ADD  CONSTRAINT [DF_Codes.ItemType_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.ItemType] ADD  CONSTRAINT [DF_Codes.ItemType_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.JobPreparation] ADD  CONSTRAINT [DF_Codes.JobSearch_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.JobPreparation] ADD  CONSTRAINT [DF_Codes.JobSearch_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.Language] ADD  CONSTRAINT [DF_Codes.Language_IsPathwaysLanguage]  DEFAULT ((0)) FOR [IsPathwaysLanguage]
GO
ALTER TABLE [dbo].[Codes.Language] ADD  CONSTRAINT [DF_Codes.Language_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.LibraryMemberType] ADD  CONSTRAINT [DF_Codes.LibraryMemberType_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Codes.PathwaysEducationLevel] ADD  CONSTRAINT [DF_Codes.PathwaysEducationLevel_IsPathwaysLevel]  DEFAULT ((1)) FOR [IsPathwaysLevel]
GO
ALTER TABLE [dbo].[Codes.PathwaysEducationLevel] ADD  CONSTRAINT [DF_Codes.PathwaysEducationLevel_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.PathwaysEducationLevel] ADD  CONSTRAINT [DF_Codes.PathwaysEducationLevel_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.RatingType] ADD  CONSTRAINT [DF_Codes.RatingType_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.RatingType] ADD  CONSTRAINT [DF_RatingType_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Codes.ResourceFormat] ADD  CONSTRAINT [DF_Codes.ResourceFormat_IsIsleCode]  DEFAULT ((1)) FOR [IsIsleCode]
GO
ALTER TABLE [dbo].[Codes.ResourceFormat] ADD  CONSTRAINT [DF_Codes.ResourceFormat_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.ResourceType] ADD  CONSTRAINT [DF_Codes.ResourceType_IsIsleItem]  DEFAULT ((1)) FOR [IsIsleItem]
GO
ALTER TABLE [dbo].[Codes.ResourceType] ADD  CONSTRAINT [DF_Codes.ResourceType_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.ResourceType] ADD  CONSTRAINT [DF_Codes.ResourceType_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.ResourceTypeCategory] ADD  CONSTRAINT [DF_Codes.ResourceTypeCategory_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.Site] ADD  CONSTRAINT [DF_Codes.Site_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.SiteTagCategory] ADD  CONSTRAINT [DF_Codes.SiteTagCategory_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.SiteTagCategory] ADD  CONSTRAINT [DF_Codes.SiteTagCategory_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.Subject] ADD  CONSTRAINT [DF_Codes.Subject_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.TagCategory] ADD  CONSTRAINT [DF_Codes.TagCategory_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.TagCategory] ADD  CONSTRAINT [DF_Codes.TagCategory_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.TagValue] ADD  CONSTRAINT [DF_Codes.TagValue_SortOrder]  DEFAULT ((10)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Codes.TagValue] ADD  CONSTRAINT [DF_Codes.TagValue_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.TagValue] ADD  CONSTRAINT [DF_Codes.TagValue_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.TagValueKeyword] ADD  CONSTRAINT [DF_Codes.TagValueKeyword_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.TagValueKeyword] ADD  CONSTRAINT [DF_Codes.TagValueKeyword_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Codes.TextType] ADD  CONSTRAINT [DF_Codes.TextType_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.VeteransService] ADD  CONSTRAINT [DF_Codes.VeteransService_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.VeteransService] ADD  CONSTRAINT [DF_Codes.VeteransService_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.WorkforcePartnerService] ADD  CONSTRAINT [DF_Codes.WorkforcePartnerService_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.WorkforcePartnerService] ADD  CONSTRAINT [DF_Codes.WorkforcePartnerService_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.WorkplaceSkills] ADD  CONSTRAINT [DF_Codes.WorkplaceSkills_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.WorkplaceSkills] ADD  CONSTRAINT [DF_Codes.WorkplaceSkills_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
GO
ALTER TABLE [dbo].[Codes.WorkSupportService] ADD  CONSTRAINT [DF_Codes.WorkSupportService_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Codes.WorkSupportService] ADD  CONSTRAINT [DF_Codes.WorkSupportService_WarehouseTotal]  DEFAULT ((0)) FOR [WarehouseTotal]
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
ALTER TABLE [dbo].[ConditionOfUse] ADD  CONSTRAINT [DF_ConditionOfUse_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ConditionOfUse] ADD  CONSTRAINT [DF_ConditionOfUse_SortOrderAuthoring]  DEFAULT ((10)) FOR [SortOrderAuthoring]
GO
ALTER TABLE [dbo].[EmailNotice] ADD  CONSTRAINT [DF_EmailNotice_isActive]  DEFAULT ((1)) FOR [isActive]
GO
ALTER TABLE [dbo].[EmailNotice] ADD  CONSTRAINT [DF_EmailNotice_langCode]  DEFAULT ('en') FOR [LanguageCode]
GO
ALTER TABLE [dbo].[EmailNotice] ADD  CONSTRAINT [DF_EmailNotice_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[EmailNotice] ADD  CONSTRAINT [DF_EmailNotice_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Evaluation] ADD  CONSTRAINT [DF_Evaluation_RequiresCertification]  DEFAULT ((0)) FOR [RequiresCertification]
GO
ALTER TABLE [dbo].[Evaluation.Dimension] ADD  CONSTRAINT [DF_Evaluation.Dimension_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Evaluation.DimensionCriteria] ADD  CONSTRAINT [DF_Evaluation.DimensionCriteria_Sequence]  DEFAULT ((20)) FOR [Sequence]
GO
ALTER TABLE [dbo].[EvaluationTool] ADD  CONSTRAINT [DF_EvaluationTool_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[EvaluationTool.Section] ADD  CONSTRAINT [DF_EvaluationTool.Section_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Map.AccessibilityApi] ADD  CONSTRAINT [DF_Map.AccessibilityApi_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Map.AccessibilityApi] ADD  CONSTRAINT [DF_Map.AccessibilityApi_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Map.AccessibilityControl] ADD  CONSTRAINT [DF_Map.AccessibilityControl_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Map.AccessibilityControl] ADD  CONSTRAINT [DF_Map.AccessibilityControl_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Map.AccessibilityFeature] ADD  CONSTRAINT [DF_Map.AccessibilityFeature_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Map.AccessibilityFeature] ADD  CONSTRAINT [DF_Map.AccessibilityFeature_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Map.AccessibilityHazard] ADD  CONSTRAINT [DF_Map.AccessibilityHazard_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Map.AccessibilityHazard] ADD  CONSTRAINT [DF_Map.AccessibilityHazard_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Map.AgeRangeGradeLevel] ADD  CONSTRAINT [DF_Map.AgeRangeGradeLevel_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Map.AgeRangeGradeLevel] ADD  CONSTRAINT [DF_Map.AgeRangeGradeLevel_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Map.CareerCluster] ADD  CONSTRAINT [DF_Map.CareerCluster_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Map.CareerCluster] ADD  CONSTRAINT [DF_Map.CareerCluster_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Map.CareerCluster] ADD  CONSTRAINT [DF_Map.CareerCluster_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Map.CleanseUrl] ADD  CONSTRAINT [DF_Map.CleanseUrl_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Map.CleanseUrl] ADD  CONSTRAINT [DF_Map.CleanseUrl_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Map.GradeLevel] ADD  CONSTRAINT [DF_Map.GradeLevel_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Map.GradeLevel] ADD  CONSTRAINT [DF_Map.GradeLevel_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Map.K12Subject] ADD  CONSTRAINT [DF_Map.K12Subject_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Map.K12Subject] ADD  CONSTRAINT [DF_Map.K12Subject_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Map.K12Subject] ADD  CONSTRAINT [DF_Map.K12Subject_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Map.ResourceFormat] ADD  CONSTRAINT [DF_Map.ResourceFormat_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Map.Rules] ADD  CONSTRAINT [DF_Map.Rules_IsRegex]  DEFAULT ((0)) FOR [IsRegex]
GO
ALTER TABLE [dbo].[Map.Rules] ADD  CONSTRAINT [DF_Map.Rules_IsCaseSensitive]  DEFAULT ((0)) FOR [IsCaseSensitive]
GO
ALTER TABLE [dbo].[Map.Rules] ADD  CONSTRAINT [DF_Map.Rules_ImportWithoutTranslation]  DEFAULT ((0)) FOR [ImportWithoutTranslation]
GO
ALTER TABLE [dbo].[Map.Rules] ADD  CONSTRAINT [DF_Map.Rules_DoNotImport]  DEFAULT ((0)) FOR [DoNotImport]
GO
ALTER TABLE [dbo].[Map.Rules] ADD  CONSTRAINT [DF_Map.Rules_Sequence]  DEFAULT ((10)) FOR [Sequence]
GO
ALTER TABLE [dbo].[Map.Rules] ADD  CONSTRAINT [DF_Map.Rules_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Map.Rules] ADD  CONSTRAINT [DF_Map.Rules_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Map.Rules] ADD  CONSTRAINT [DF_Map.Rules_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Patron] ADD  CONSTRAINT [DF_Patron_Password]  DEFAULT (newid()) FOR [Password]
GO
ALTER TABLE [dbo].[Patron] ADD  CONSTRAINT [DF_Patron_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Patron] ADD  CONSTRAINT [DF_Patron_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Patron] ADD  CONSTRAINT [DF_Patron_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Patron] ADD  CONSTRAINT [DF_Patron_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Patron.ExternalAccount] ADD  CONSTRAINT [DF_Patron.ExternalAccount_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Patron.Following] ADD  CONSTRAINT [DF_Patron.Following_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Patron.Note] ADD  CONSTRAINT [DF_Patron.Note_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Patron.Note] ADD  CONSTRAINT [DF_Patron.Note_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Patron.Profile] ADD  CONSTRAINT [DF_Patron.Profile_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Patron.Profile] ADD  CONSTRAINT [DF_Patron.Profile_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Patron.SearchFilter] ADD  CONSTRAINT [DF_Patron.SearchFilter_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Patron.SearchFilter] ADD  CONSTRAINT [DF_Patron.SearchFilter_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Patron.Tag] ADD  CONSTRAINT [DF_Patron.Tag_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Publish.Pending] ADD  CONSTRAINT [DF_Publish.Pending_IsPublished]  DEFAULT ((0)) FOR [IsPublished]
GO
ALTER TABLE [dbo].[Publish.Pending] ADD  CONSTRAINT [DF_Publish.Pending_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[PublisherSummary] ADD  CONSTRAINT [DF_PublisherSummary_IsActive]  DEFAULT ((0)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Resource] ADD  CONSTRAINT [DF_Resource_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Resource] ADD  CONSTRAINT [DF_Resource_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Resource.AgeRange] ADD  CONSTRAINT [DF_Resource.AgeRange_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.AssessmentType] ADD  CONSTRAINT [DF_Resource.AssessmentType_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Cluster] ADD  CONSTRAINT [DF_Resource.Cluster_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Comment] ADD  CONSTRAINT [DF_Resource.Comment_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Resource.Comment] ADD  CONSTRAINT [DF_Resource.Comment_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Comment] ADD  CONSTRAINT [DF_Resource.Comment_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Resource.EducationLevel] ADD  CONSTRAINT [DF_Resource.EducationLevel_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Resource.EducationLevel] ADD  CONSTRAINT [DF_Resource.EducationLevel_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.EducationUse] ADD  CONSTRAINT [DF_Resource.EducationUser_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Evaluation] ADD  CONSTRAINT [DF_Resource.Evaluation_UserHasCertification]  DEFAULT ((0)) FOR [UserHasCertification]
GO
ALTER TABLE [dbo].[Resource.Evaluation] ADD  CONSTRAINT [DF_Resource.Evaluation_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.EvaluationSection] ADD  CONSTRAINT [DF_Resource.EvaluationSection_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.EvaluationSummary] ADD  CONSTRAINT [DF_Table_1_AlignmentDegreeId]  DEFAULT ((2)) FOR [Dimension2Count]
GO
ALTER TABLE [dbo].[Resource.EvaluationSummary] ADD  CONSTRAINT [DF_Resource.EvaluationSummary_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Format] ADD  CONSTRAINT [DF_Resource.Format_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.GradeLevel] ADD  CONSTRAINT [DF_Resource.GradeLevel_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.GroupType] ADD  CONSTRAINT [DF_Resource.GroupType_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.IntendedAudience] ADD  CONSTRAINT [DF_Resource.IntendedAudience_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.ItemType] ADD  CONSTRAINT [DF_Resource.ItemType_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.ItemType] ADD  CONSTRAINT [DF_Resource.ItemType_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[Resource.Keyword] ADD  CONSTRAINT [DF_Resource.Keyword_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Language] ADD  CONSTRAINT [DF_Resource.Language_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.LearningMapReference] ADD  CONSTRAINT [DF_Resource.LearningMapReference_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.LinkCheck] ADD  CONSTRAINT [DF_Resource.LinkCheck_LastCheckDate]  DEFAULT (((2000)-(1))-(1)) FOR [LastCheckDate]
GO
ALTER TABLE [dbo].[Resource.LinkCheck] ADD  CONSTRAINT [DF_Resource.LinkCheck_HostTimeoutCount]  DEFAULT ((0)) FOR [HostTimeoutCount]
GO
ALTER TABLE [dbo].[Resource.LinkCheck] ADD  CONSTRAINT [DF_Resource.LinkCheck_ServerErrorCount]  DEFAULT ((0)) FOR [ServerErrorCount]
GO
ALTER TABLE [dbo].[Resource.LinkCheck] ADD  CONSTRAINT [DF_Resource.LinkCheck_IsBadLink]  DEFAULT ((0)) FOR [IsBadLink]
GO
ALTER TABLE [dbo].[Resource.PublishedBy] ADD  CONSTRAINT [DF_Resource.PublishedBy_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Recommendation] ADD  CONSTRAINT [DF_Resource.Recommendation_TypeId]  DEFAULT ((1)) FOR [TypeId]
GO
ALTER TABLE [dbo].[Resource.Recommendation] ADD  CONSTRAINT [DF_Resource.Recommendation_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Resource.Recommendation] ADD  CONSTRAINT [DF_Resource.Recommendation_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.RelatedUrl] ADD  CONSTRAINT [DF_Resource.RelatedUrl_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Resource.RelatedUrl] ADD  CONSTRAINT [DF_Resource.RelatedUrl_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.ResourceType] ADD  CONSTRAINT [DF_Resource.ResourceType_RowId]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Resource.ResourceType] ADD  CONSTRAINT [DF_Resource.ResourceType_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Site] ADD  CONSTRAINT [DF_Resource.Site_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Standard] ADD  CONSTRAINT [DF_Resource.Standard_AlignmentDegreeId]  DEFAULT ((2)) FOR [AlignmentDegreeId]
GO
ALTER TABLE [dbo].[Resource.Standard] ADD  CONSTRAINT [DF_Resource.Standard_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.StandardEvaluation] ADD  CONSTRAINT [DF_Resource.StandardEvaluation_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Subject] ADD  CONSTRAINT [DF_Resource.Subject_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Tag] ADD  CONSTRAINT [DF_Resource.Tag_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Text] ADD  CONSTRAINT [DF_Resource.Text_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Resource.Version] ADD  CONSTRAINT [DF_Resource.Version2_Imported]  DEFAULT (getdate()) FOR [Imported]
GO
ALTER TABLE [dbo].[Resource.Version] ADD  CONSTRAINT [DF_Resource.Version2_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Resource.Version] ADD  CONSTRAINT [DF_Resource.Version_RowId_1]  DEFAULT (newid()) FOR [RowId]
GO
ALTER TABLE [dbo].[Resource_Index] ADD  CONSTRAINT [DF_Resource_Index_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Resource_IndexV2] ADD  CONSTRAINT [DF_Resource_IndexV2_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Sort.SpecialCharacter] ADD  CONSTRAINT [DF_Sort.SpecialCharacter_IsReplaceDescription]  DEFAULT ((0)) FOR [IsReplaceDescription]
GO
ALTER TABLE [dbo].[Sort.SpecialCharacter] ADD  CONSTRAINT [DF_Sort.SpecialCharacter_IsDropDescription]  DEFAULT ((0)) FOR [IsDropDescription]
GO
ALTER TABLE [dbo].[StandardBody.Subject] ADD  CONSTRAINT [DF_StandardBody.Subject_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[System.GenerateLoginId] ADD  CONSTRAINT [DF_System.GenerateLoginId_RowId]  DEFAULT (newid()) FOR [ProxyId]
GO
ALTER TABLE [dbo].[System.GenerateLoginId] ADD  CONSTRAINT [DF_System.GenerateLoginId_ProxyType]  DEFAULT ('Forgot Password') FOR [ProxyType]
GO
ALTER TABLE [dbo].[System.GenerateLoginId] ADD  CONSTRAINT [DF_System.GenerateLoginId_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[System.GenerateLoginId] ADD  CONSTRAINT [DF_System.GenerateLoginId_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[System.Process] ADD  CONSTRAINT [DF_System.Process_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[System.Process] ADD  CONSTRAINT [DF_System.Process_CreatedBy]  DEFAULT (suser_name()) FOR [CreatedBy]
GO
ALTER TABLE [dbo].[System.Process] ADD  CONSTRAINT [DF_System.Process_LastUpdated]  DEFAULT (getdate()) FOR [LastUpdated]
GO
ALTER TABLE [dbo].[System.Process] ADD  CONSTRAINT [DF_System.Process_LastUpdatedBy]  DEFAULT (suser_name()) FOR [LastUpdatedBy]
GO
ALTER TABLE [dbo].[System.Process] ADD  CONSTRAINT [DF_System.Process_LastRunDate]  DEFAULT (getdate()) FOR [LastRunDate]
GO
ALTER TABLE [dbo].[Codes.AccessibilityHazard]  WITH CHECK ADD  CONSTRAINT [FK_Codes.AccessibilityHazard_Codes.AccessibilityHazard] FOREIGN KEY([AntonymId])
REFERENCES [dbo].[Codes.AccessibilityHazard] ([Id])
GO
ALTER TABLE [dbo].[Codes.AccessibilityHazard] CHECK CONSTRAINT [FK_Codes.AccessibilityHazard_Codes.AccessibilityHazard]
GO
ALTER TABLE [dbo].[Codes.SiteTagCategory]  WITH CHECK ADD  CONSTRAINT [FK_Codes.SiteTagCategory_Codes.Site] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Codes.Site] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Codes.SiteTagCategory] CHECK CONSTRAINT [FK_Codes.SiteTagCategory_Codes.Site]
GO
ALTER TABLE [dbo].[Codes.SiteTagCategory]  WITH CHECK ADD  CONSTRAINT [FK_Codes.SiteTagCategory_Codes.TagCategory] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Codes.TagCategory] ([Id])
GO
ALTER TABLE [dbo].[Codes.SiteTagCategory] CHECK CONSTRAINT [FK_Codes.SiteTagCategory_Codes.TagCategory]
GO
ALTER TABLE [dbo].[Codes.TagValue]  WITH CHECK ADD  CONSTRAINT [FK_Codes.TagValue_Codes.TagCategory] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Codes.TagCategory] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Codes.TagValue] CHECK CONSTRAINT [FK_Codes.TagValue_Codes.TagCategory]
GO
ALTER TABLE [dbo].[Codes.TagValueKeyword]  WITH CHECK ADD  CONSTRAINT [FK_Codes.TagValueKeyword_Codes.TagValue] FOREIGN KEY([TagValueId])
REFERENCES [dbo].[Codes.TagValue] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Codes.TagValueKeyword] CHECK CONSTRAINT [FK_Codes.TagValueKeyword_Codes.TagValue]
GO
ALTER TABLE [dbo].[Evaluation.Dimension]  WITH CHECK ADD  CONSTRAINT [FK_Evaluation.Dimension_Evaluation] FOREIGN KEY([EvaluationId])
REFERENCES [dbo].[Evaluation] ([Id])
GO
ALTER TABLE [dbo].[Evaluation.Dimension] CHECK CONSTRAINT [FK_Evaluation.Dimension_Evaluation]
GO
ALTER TABLE [dbo].[Evaluation.DimensionCriteria]  WITH CHECK ADD  CONSTRAINT [FK_Evaluation.DimensionCriteria_Evaluation.Dimension] FOREIGN KEY([DimensionId])
REFERENCES [dbo].[Evaluation.Dimension] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Evaluation.DimensionCriteria] CHECK CONSTRAINT [FK_Evaluation.DimensionCriteria_Evaluation.Dimension]
GO
ALTER TABLE [dbo].[EvaluationTool.Section]  WITH CHECK ADD  CONSTRAINT [FK_EvaluationTool.Section_EvaluationTool] FOREIGN KEY([EvalToolId])
REFERENCES [dbo].[EvaluationTool] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EvaluationTool.Section] CHECK CONSTRAINT [FK_EvaluationTool.Section_EvaluationTool]
GO
ALTER TABLE [dbo].[Map.AccessibilityApi]  WITH CHECK ADD  CONSTRAINT [FK_Map.AccessibilityApi_Codes.AccessibilityApi] FOREIGN KEY([CodeId])
REFERENCES [dbo].[Codes.AccessibilityApi] ([Id])
GO
ALTER TABLE [dbo].[Map.AccessibilityApi] CHECK CONSTRAINT [FK_Map.AccessibilityApi_Codes.AccessibilityApi]
GO
ALTER TABLE [dbo].[Map.AccessibilityControl]  WITH CHECK ADD  CONSTRAINT [FK_Map.AccessibilityControl_Codes.AccessibilityControl] FOREIGN KEY([CodeId])
REFERENCES [dbo].[Codes.AccessibilityControl] ([Id])
GO
ALTER TABLE [dbo].[Map.AccessibilityControl] CHECK CONSTRAINT [FK_Map.AccessibilityControl_Codes.AccessibilityControl]
GO
ALTER TABLE [dbo].[Map.AccessibilityFeature]  WITH CHECK ADD  CONSTRAINT [FK_Map.AccessibilityFeature_Codes.AccessibilityFeature] FOREIGN KEY([CodeId])
REFERENCES [dbo].[Codes.AccessibilityFeature] ([Id])
GO
ALTER TABLE [dbo].[Map.AccessibilityFeature] CHECK CONSTRAINT [FK_Map.AccessibilityFeature_Codes.AccessibilityFeature]
GO
ALTER TABLE [dbo].[Map.AccessibilityHazard]  WITH CHECK ADD  CONSTRAINT [FK_Map.AccessibilityHazard_Codes.AccessibilityHazard] FOREIGN KEY([CodeId])
REFERENCES [dbo].[Codes.AccessibilityHazard] ([Id])
GO
ALTER TABLE [dbo].[Map.AccessibilityHazard] CHECK CONSTRAINT [FK_Map.AccessibilityHazard_Codes.AccessibilityHazard]
GO
ALTER TABLE [dbo].[Map.GroupType]  WITH CHECK ADD  CONSTRAINT [FK_Map.GroupType_Codes.GroupType] FOREIGN KEY([CodeId])
REFERENCES [dbo].[Codes.GroupType] ([Id])
GO
ALTER TABLE [dbo].[Map.GroupType] CHECK CONSTRAINT [FK_Map.GroupType_Codes.GroupType]
GO
ALTER TABLE [dbo].[Map.K12Subject]  WITH CHECK ADD  CONSTRAINT [FK_Map.K12Subject_Codes.Subject] FOREIGN KEY([MappedSubjectId])
REFERENCES [dbo].[Codes.Subject] ([Id])
GO
ALTER TABLE [dbo].[Map.K12Subject] CHECK CONSTRAINT [FK_Map.K12Subject_Codes.Subject]
GO
ALTER TABLE [dbo].[Patron.ExternalAccount]  WITH CHECK ADD  CONSTRAINT [FK_Patron.ExternalAccount_Patron] FOREIGN KEY([PatronId])
REFERENCES [dbo].[Patron] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Patron.ExternalAccount] CHECK CONSTRAINT [FK_Patron.ExternalAccount_Patron]
GO
ALTER TABLE [dbo].[Patron.Following]  WITH CHECK ADD  CONSTRAINT [FK_Patron.FollowedBy_Patron] FOREIGN KEY([FollowedByUserId])
REFERENCES [dbo].[Patron] ([Id])
GO
ALTER TABLE [dbo].[Patron.Following] CHECK CONSTRAINT [FK_Patron.FollowedBy_Patron]
GO
ALTER TABLE [dbo].[Patron.Following]  WITH CHECK ADD  CONSTRAINT [FK_Patron.Following_Patron] FOREIGN KEY([FollowingUserId])
REFERENCES [dbo].[Patron] ([Id])
GO
ALTER TABLE [dbo].[Patron.Following] CHECK CONSTRAINT [FK_Patron.Following_Patron]
GO
ALTER TABLE [dbo].[Patron.Note]  WITH CHECK ADD  CONSTRAINT [FK_Patron.Note_Patron] FOREIGN KEY([UserId])
REFERENCES [dbo].[Patron] ([Id])
GO
ALTER TABLE [dbo].[Patron.Note] CHECK CONSTRAINT [FK_Patron.Note_Patron]
GO
ALTER TABLE [dbo].[Patron.Profile]  WITH CHECK ADD  CONSTRAINT [FK_Patron.Profile_Patron] FOREIGN KEY([UserId])
REFERENCES [dbo].[Patron] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Patron.Profile] CHECK CONSTRAINT [FK_Patron.Profile_Patron]
GO
ALTER TABLE [dbo].[Patron.SearchFilter]  WITH CHECK ADD  CONSTRAINT [FK_Patron.SearchFilter_Patron] FOREIGN KEY([UserId])
REFERENCES [dbo].[Patron] ([Id])
GO
ALTER TABLE [dbo].[Patron.SearchFilter] CHECK CONSTRAINT [FK_Patron.SearchFilter_Patron]
GO
ALTER TABLE [dbo].[Patron.Tag]  WITH CHECK ADD  CONSTRAINT [FK_Patron.Tag_Codes.TagCategory] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Codes.TagCategory] ([Id])
GO
ALTER TABLE [dbo].[Patron.Tag] CHECK CONSTRAINT [FK_Patron.Tag_Codes.TagCategory]
GO
ALTER TABLE [dbo].[Patron.Tag]  WITH CHECK ADD  CONSTRAINT [FK_Patron.Tag_Patron] FOREIGN KEY([PatronId])
REFERENCES [dbo].[Patron] ([Id])
GO
ALTER TABLE [dbo].[Patron.Tag] CHECK CONSTRAINT [FK_Patron.Tag_Patron]
GO
ALTER TABLE [dbo].[Resource.AccessibilityApi]  WITH CHECK ADD  CONSTRAINT [FK_Resource.AccessibilityApi_Codes.AccessibilityApi] FOREIGN KEY([AccessibilityApiId])
REFERENCES [dbo].[Codes.AccessibilityApi] ([Id])
GO
ALTER TABLE [dbo].[Resource.AccessibilityApi] CHECK CONSTRAINT [FK_Resource.AccessibilityApi_Codes.AccessibilityApi]
GO
ALTER TABLE [dbo].[Resource.AccessibilityControl]  WITH CHECK ADD  CONSTRAINT [FK_Resource.AccessibilityControl_Codes.AccessibilityControl] FOREIGN KEY([AccessibilityControlId])
REFERENCES [dbo].[Codes.AccessibilityControl] ([Id])
GO
ALTER TABLE [dbo].[Resource.AccessibilityControl] CHECK CONSTRAINT [FK_Resource.AccessibilityControl_Codes.AccessibilityControl]
GO
ALTER TABLE [dbo].[Resource.AccessibilityFeature]  WITH CHECK ADD  CONSTRAINT [FK_Resource.AccessibilityFeature_Codes.AccessibilityFeature] FOREIGN KEY([AccessibilityFeatureId])
REFERENCES [dbo].[Codes.AccessibilityFeature] ([Id])
GO
ALTER TABLE [dbo].[Resource.AccessibilityFeature] CHECK CONSTRAINT [FK_Resource.AccessibilityFeature_Codes.AccessibilityFeature]
GO
ALTER TABLE [dbo].[Resource.AccessibilityFeature]  WITH CHECK ADD  CONSTRAINT [FK_Resource.AccessibilityFeature_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
GO
ALTER TABLE [dbo].[Resource.AccessibilityFeature] CHECK CONSTRAINT [FK_Resource.AccessibilityFeature_Resource]
GO
ALTER TABLE [dbo].[Resource.AccessibilityHazard]  WITH CHECK ADD  CONSTRAINT [FK_Resource.AccessibilityHazard_Codes.AccessibilityHazard] FOREIGN KEY([AccessibilityHazardId])
REFERENCES [dbo].[Codes.AccessibilityHazard] ([Id])
GO
ALTER TABLE [dbo].[Resource.AccessibilityHazard] CHECK CONSTRAINT [FK_Resource.AccessibilityHazard_Codes.AccessibilityHazard]
GO
ALTER TABLE [dbo].[Resource.AccessibilityHazard]  WITH CHECK ADD  CONSTRAINT [FK_Resource.AccessibilityHazard_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
GO
ALTER TABLE [dbo].[Resource.AccessibilityHazard] CHECK CONSTRAINT [FK_Resource.AccessibilityHazard_Resource]
GO
ALTER TABLE [dbo].[Resource.AssessmentType]  WITH CHECK ADD  CONSTRAINT [FK_Resource.AssessmentType_Codes.AssessmentType] FOREIGN KEY([AssessmentTypeId])
REFERENCES [dbo].[Codes.AssessmentType] ([Id])
GO
ALTER TABLE [dbo].[Resource.AssessmentType] CHECK CONSTRAINT [FK_Resource.AssessmentType_Codes.AssessmentType]
GO
ALTER TABLE [dbo].[Resource.AssessmentType]  WITH CHECK ADD  CONSTRAINT [FK_Resource.AssessmentType_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.AssessmentType] CHECK CONSTRAINT [FK_Resource.AssessmentType_Resource]
GO
ALTER TABLE [dbo].[Resource.Cluster]  WITH CHECK ADD  CONSTRAINT [FK_ResourceCluster_Cluster] FOREIGN KEY([ClusterId])
REFERENCES [dbo].[CareerCluster] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Cluster] CHECK CONSTRAINT [FK_ResourceCluster_Cluster]
GO
ALTER TABLE [dbo].[Resource.Comment]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Comment_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Comment] CHECK CONSTRAINT [FK_Resource.Comment_Resource]
GO
ALTER TABLE [dbo].[Resource.EducationLevel]  WITH CHECK ADD  CONSTRAINT [FK_Resource.EducationLevel_Codes.GradeLevel] FOREIGN KEY([EducationLevelId])
REFERENCES [dbo].[Codes.GradeLevel] ([Id])
GO
ALTER TABLE [dbo].[Resource.EducationLevel] CHECK CONSTRAINT [FK_Resource.EducationLevel_Codes.GradeLevel]
GO
ALTER TABLE [dbo].[Resource.EducationLevel]  WITH CHECK ADD  CONSTRAINT [FK_Resource.EducationLevel_Codes.PathwaysEducationLevel] FOREIGN KEY([PathwaysEducationLevelId])
REFERENCES [dbo].[Codes.PathwaysEducationLevel] ([Id])
GO
ALTER TABLE [dbo].[Resource.EducationLevel] CHECK CONSTRAINT [FK_Resource.EducationLevel_Codes.PathwaysEducationLevel]
GO
ALTER TABLE [dbo].[Resource.EducationUse]  WITH CHECK ADD  CONSTRAINT [FK_Resource.EducationUse_Codes.EducationalUse] FOREIGN KEY([EducationUseId])
REFERENCES [dbo].[Codes.EducationalUse] ([Id])
GO
ALTER TABLE [dbo].[Resource.EducationUse] CHECK CONSTRAINT [FK_Resource.EducationUse_Codes.EducationalUse]
GO
ALTER TABLE [dbo].[Resource.EducationUse]  WITH CHECK ADD  CONSTRAINT [FK_Resource.EducationUse_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.EducationUse] CHECK CONSTRAINT [FK_Resource.EducationUse_Resource]
GO
ALTER TABLE [dbo].[Resource.Evaluation]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Evaluation_Evaluation] FOREIGN KEY([EvaluationId])
REFERENCES [dbo].[Evaluation] ([Id])
GO
ALTER TABLE [dbo].[Resource.Evaluation] CHECK CONSTRAINT [FK_Resource.Evaluation_Evaluation]
GO
ALTER TABLE [dbo].[Resource.Evaluation]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Evaluation_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
GO
ALTER TABLE [dbo].[Resource.Evaluation] CHECK CONSTRAINT [FK_Resource.Evaluation_Resource]
GO
ALTER TABLE [dbo].[Resource.EvaluationSection]  WITH CHECK ADD  CONSTRAINT [FK_Resource.EvaluationSection_Resource.Evaluation1] FOREIGN KEY([ResourceEvalId])
REFERENCES [dbo].[Resource.Evaluation] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.EvaluationSection] CHECK CONSTRAINT [FK_Resource.EvaluationSection_Resource.Evaluation1]
GO
ALTER TABLE [dbo].[Resource.EvaluationSection]  WITH CHECK ADD  CONSTRAINT [FK_Resource.EvaluationSection_Resource.EvaluationSection] FOREIGN KEY([EvalDimensionId])
REFERENCES [dbo].[Evaluation.Dimension] ([Id])
GO
ALTER TABLE [dbo].[Resource.EvaluationSection] CHECK CONSTRAINT [FK_Resource.EvaluationSection_Resource.EvaluationSection]
GO
ALTER TABLE [dbo].[Resource.EvaluationSummary]  WITH CHECK ADD  CONSTRAINT [FK_Resource.EvaluationSummary_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.EvaluationSummary] CHECK CONSTRAINT [FK_Resource.EvaluationSummary_Resource]
GO
ALTER TABLE [dbo].[Resource.Format]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Format_Codes.ResourceFormat] FOREIGN KEY([CodeId])
REFERENCES [dbo].[Codes.ResourceFormat] ([Id])
GO
ALTER TABLE [dbo].[Resource.Format] CHECK CONSTRAINT [FK_Resource.Format_Codes.ResourceFormat]
GO
ALTER TABLE [dbo].[Resource.Format]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Format_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Format] CHECK CONSTRAINT [FK_Resource.Format_Resource]
GO
ALTER TABLE [dbo].[Resource.GradeLevel]  WITH CHECK ADD  CONSTRAINT [FK_Resource.GradeLevel_Codes.GradeLevel] FOREIGN KEY([GradeLevelId])
REFERENCES [dbo].[Codes.GradeLevel] ([Id])
GO
ALTER TABLE [dbo].[Resource.GradeLevel] CHECK CONSTRAINT [FK_Resource.GradeLevel_Codes.GradeLevel]
GO
ALTER TABLE [dbo].[Resource.GradeLevel]  WITH CHECK ADD  CONSTRAINT [FK_Resource.GradeLevel_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.GradeLevel] CHECK CONSTRAINT [FK_Resource.GradeLevel_Resource]
GO
ALTER TABLE [dbo].[Resource.GroupType]  WITH CHECK ADD  CONSTRAINT [FK_Resource.GroupType_Codes.GroupType] FOREIGN KEY([GroupTypeId])
REFERENCES [dbo].[Codes.GroupType] ([Id])
GO
ALTER TABLE [dbo].[Resource.GroupType] CHECK CONSTRAINT [FK_Resource.GroupType_Codes.GroupType]
GO
ALTER TABLE [dbo].[Resource.GroupType]  WITH CHECK ADD  CONSTRAINT [FK_Resource.GroupType_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.GroupType] CHECK CONSTRAINT [FK_Resource.GroupType_Resource]
GO
ALTER TABLE [dbo].[Resource.IntendedAudience]  WITH CHECK ADD  CONSTRAINT [FK_IntendedAudience_CodesAudienceType] FOREIGN KEY([AudienceId])
REFERENCES [dbo].[Codes.AudienceType] ([Id])
GO
ALTER TABLE [dbo].[Resource.IntendedAudience] CHECK CONSTRAINT [FK_IntendedAudience_CodesAudienceType]
GO
ALTER TABLE [dbo].[Resource.ItemType]  WITH CHECK ADD  CONSTRAINT [FK_Resource.ItemType_Codes.ItemType] FOREIGN KEY([ItemTypeId])
REFERENCES [dbo].[Codes.ItemType] ([Id])
GO
ALTER TABLE [dbo].[Resource.ItemType] CHECK CONSTRAINT [FK_Resource.ItemType_Codes.ItemType]
GO
ALTER TABLE [dbo].[Resource.ItemType]  WITH CHECK ADD  CONSTRAINT [FK_Resource.ItemType_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.ItemType] CHECK CONSTRAINT [FK_Resource.ItemType_Resource]
GO
ALTER TABLE [dbo].[Resource.Keyword]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Keyword_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Keyword] CHECK CONSTRAINT [FK_Resource.Keyword_Resource]
GO
ALTER TABLE [dbo].[Resource.Language]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Language_Codes.Language] FOREIGN KEY([LanguageId])
REFERENCES [dbo].[Codes.Language] ([Id])
GO
ALTER TABLE [dbo].[Resource.Language] CHECK CONSTRAINT [FK_Resource.Language_Codes.Language]
GO
ALTER TABLE [dbo].[Resource.Language]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Language_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Language] CHECK CONSTRAINT [FK_Resource.Language_Resource]
GO
ALTER TABLE [dbo].[Resource.LearningMapReference]  WITH CHECK ADD  CONSTRAINT [FK_Resource.LearningMapReference_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
GO
ALTER TABLE [dbo].[Resource.LearningMapReference] CHECK CONSTRAINT [FK_Resource.LearningMapReference_Resource]
GO
ALTER TABLE [dbo].[Resource.Like]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Like_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Like] CHECK CONSTRAINT [FK_Resource.Like_Resource]
GO
ALTER TABLE [dbo].[Resource.LikeSummary]  WITH CHECK ADD  CONSTRAINT [FK_Resource.LikeSummary_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.LikeSummary] CHECK CONSTRAINT [FK_Resource.LikeSummary_Resource]
GO
ALTER TABLE [dbo].[Resource.PublishedBy]  WITH CHECK ADD  CONSTRAINT [FK_Resource.PublishedBy_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.PublishedBy] CHECK CONSTRAINT [FK_Resource.PublishedBy_Resource]
GO
ALTER TABLE [dbo].[Resource.RatingSummary]  WITH CHECK ADD  CONSTRAINT [FK_Resource.RatingSummary_Codes.RatingType] FOREIGN KEY([RatingTypeId])
REFERENCES [dbo].[Codes.RatingType] ([Id])
GO
ALTER TABLE [dbo].[Resource.RatingSummary] CHECK CONSTRAINT [FK_Resource.RatingSummary_Codes.RatingType]
GO
ALTER TABLE [dbo].[Resource.Recommendation]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Recommendation_Patron] FOREIGN KEY([CreatedById])
REFERENCES [dbo].[Patron] ([Id])
GO
ALTER TABLE [dbo].[Resource.Recommendation] CHECK CONSTRAINT [FK_Resource.Recommendation_Patron]
GO
ALTER TABLE [dbo].[Resource.Recommendation]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Recommendation_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Recommendation] CHECK CONSTRAINT [FK_Resource.Recommendation_Resource]
GO
ALTER TABLE [dbo].[Resource.RelatedUrl]  WITH CHECK ADD  CONSTRAINT [FK_Resource.RelatedUrl_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.RelatedUrl] CHECK CONSTRAINT [FK_Resource.RelatedUrl_Resource]
GO
ALTER TABLE [dbo].[Resource.ResourceType]  WITH CHECK ADD  CONSTRAINT [FK_Resource.ResourceType_Codes.ResourceType] FOREIGN KEY([ResourceTypeId])
REFERENCES [dbo].[Codes.ResourceType] ([Id])
GO
ALTER TABLE [dbo].[Resource.ResourceType] CHECK CONSTRAINT [FK_Resource.ResourceType_Codes.ResourceType]
GO
ALTER TABLE [dbo].[Resource.ResourceType]  WITH CHECK ADD  CONSTRAINT [FK_Resource.ResourceType_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.ResourceType] CHECK CONSTRAINT [FK_Resource.ResourceType_Resource]
GO
ALTER TABLE [dbo].[Resource.Site]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Site_Codes.Site] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Codes.Site] ([Id])
GO
ALTER TABLE [dbo].[Resource.Site] CHECK CONSTRAINT [FK_Resource.Site_Codes.Site]
GO
ALTER TABLE [dbo].[Resource.Site]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Site_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Site] CHECK CONSTRAINT [FK_Resource.Site_Resource]
GO
ALTER TABLE [dbo].[Resource.Standard]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Standard_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Standard] CHECK CONSTRAINT [FK_Resource.Standard_Resource]
GO
ALTER TABLE [dbo].[Resource.Standard]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Standard_StandardBody.Node] FOREIGN KEY([StandardId])
REFERENCES [dbo].[StandardBody.Node] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Standard] CHECK CONSTRAINT [FK_Resource.Standard_StandardBody.Node]
GO
ALTER TABLE [dbo].[Resource.StandardEvaluation]  WITH CHECK ADD  CONSTRAINT [FK_Resource.StandardEvaluation_Resource.Standard] FOREIGN KEY([ResourceStandardId])
REFERENCES [dbo].[Resource.Standard] ([Id])
GO
ALTER TABLE [dbo].[Resource.StandardEvaluation] CHECK CONSTRAINT [FK_Resource.StandardEvaluation_Resource.Standard]
GO
ALTER TABLE [dbo].[Resource.Subject]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Subject_Codes.Subject] FOREIGN KEY([CodeId])
REFERENCES [dbo].[Codes.Subject] ([Id])
GO
ALTER TABLE [dbo].[Resource.Subject] CHECK CONSTRAINT [FK_Resource.Subject_Codes.Subject]
GO
ALTER TABLE [dbo].[Resource.Subject]  WITH CHECK ADD  CONSTRAINT [FK_Resource_Subject_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Subject] CHECK CONSTRAINT [FK_Resource_Subject_Resource]
GO
ALTER TABLE [dbo].[Resource.Tag]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Tag_Codes.TagValue] FOREIGN KEY([TagValueId])
REFERENCES [dbo].[Codes.TagValue] ([Id])
GO
ALTER TABLE [dbo].[Resource.Tag] CHECK CONSTRAINT [FK_Resource.Tag_Codes.TagValue]
GO
ALTER TABLE [dbo].[Resource.Tag]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Tag_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Tag] CHECK CONSTRAINT [FK_Resource.Tag_Resource]
GO
ALTER TABLE [dbo].[Resource.Text]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Text_Codes.TextType] FOREIGN KEY([TypeId])
REFERENCES [dbo].[Codes.TextType] ([Id])
GO
ALTER TABLE [dbo].[Resource.Text] CHECK CONSTRAINT [FK_Resource.Text_Codes.TextType]
GO
ALTER TABLE [dbo].[Resource.Text]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Text_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Text] CHECK CONSTRAINT [FK_Resource.Text_Resource]
GO
ALTER TABLE [dbo].[Resource.Version]  WITH CHECK ADD  CONSTRAINT [FK_Resource.Version2_Resource] FOREIGN KEY([ResourceIntId])
REFERENCES [dbo].[Resource] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource.Version] CHECK CONSTRAINT [FK_Resource.Version2_Resource]
GO
ALTER TABLE [dbo].[Resource_IndexV2.Tags]  WITH CHECK ADD  CONSTRAINT [FK_Resource_IndexV2.Tags_Resource_IndexV2] FOREIGN KEY([ResourceId])
REFERENCES [dbo].[Resource_IndexV2] ([intID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Resource_IndexV2.Tags] CHECK CONSTRAINT [FK_Resource_IndexV2.Tags_Resource_IndexV2]
GO
ALTER TABLE [dbo].[Standard.SubjectStandardConnector]  WITH CHECK ADD  CONSTRAINT [FK_Standard.SubjectStandardConnector_StandardBody.Node] FOREIGN KEY([DomainNodeId])
REFERENCES [dbo].[StandardBody.Node] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Standard.SubjectStandardConnector] CHECK CONSTRAINT [FK_Standard.SubjectStandardConnector_StandardBody.Node]
GO
ALTER TABLE [dbo].[Standard.SubjectStandardConnector]  WITH CHECK ADD  CONSTRAINT [FK_Standard.SubjectStandardConnector_StandardBody.Subject] FOREIGN KEY([StandardSubjectId])
REFERENCES [dbo].[StandardBody.Subject] ([Id])
GO
ALTER TABLE [dbo].[Standard.SubjectStandardConnector] CHECK CONSTRAINT [FK_Standard.SubjectStandardConnector_StandardBody.Subject]
GO
ALTER TABLE [dbo].[StandardBody.NodeGradeLevel]  WITH CHECK ADD  CONSTRAINT [FK_StandardBody.NodeGradeLevel_StandardBody.Node] FOREIGN KEY([ParentId])
REFERENCES [dbo].[StandardBody.Node] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StandardBody.NodeGradeLevel] CHECK CONSTRAINT [FK_StandardBody.NodeGradeLevel_StandardBody.Node]
GO
ALTER TABLE [dbo].[StandardBody.NodeGradeLevel]  WITH CHECK ADD  CONSTRAINT [FK_StandardBody.NodeGradeLevel_StandardBody.NodeGradeLevel] FOREIGN KEY([Id])
REFERENCES [dbo].[StandardBody.NodeGradeLevel] ([Id])
GO
ALTER TABLE [dbo].[StandardBody.NodeGradeLevel] CHECK CONSTRAINT [FK_StandardBody.NodeGradeLevel_StandardBody.NodeGradeLevel]
GO
ALTER TABLE [dbo].[StandardBody.Subject]  WITH CHECK ADD  CONSTRAINT [FK_StandardBody.Subject_StandardBody] FOREIGN KEY([StandardBodyId])
REFERENCES [dbo].[StandardBody] ([Id])
GO
ALTER TABLE [dbo].[StandardBody.Subject] CHECK CONSTRAINT [FK_StandardBody.Subject_StandardBody]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnHidden', @value=N'False' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnOrder', @value=0 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_ColumnWidth', @value=-1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotice', @level2type=N'COLUMN',@level2name=N'id'
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
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
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
         Begin Table = "IntendedAudience"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 214
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Codes.AudienceType"
            Begin Extent = 
               Top = 6
               Left = 252
               Bottom = 84
               Right = 419
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
      Begin ColumnWidths = 12
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'IntendedAudienceCounts'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'IntendedAudienceCounts'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
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
         Begin Table = "Resource.Version"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 236
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'PrivateResource'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'PrivateResource'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Resource.LanguagesList'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Resource.LanguagesList'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
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
         Begin Table = "Resource.Link (LinkChecker.dbo)"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 135
               Right = 249
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Resource.Link'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Resource.Link'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[25] 4[21] 2[25] 3) )"
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
         Begin Table = "StandardBody"
            Begin Extent = 
               Top = 0
               Left = 493
               Bottom = 161
               Right = 683
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "StandardBody.Subject"
            Begin Extent = 
               Top = 0
               Left = 242
               Bottom = 167
               Right = 485
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Standard.SubjectStandardConnector"
            Begin Extent = 
               Top = 190
               Left = 22
               Bottom = 319
               Right = 201
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "DomainNode"
            Begin Extent = 
               Top = 174
               Left = 251
               Bottom = 316
               Right = 451
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ClusterNode"
            Begin Extent = 
               Top = 166
               Left = 485
               Bottom = 328
               Right = 653
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "StandardNode"
            Begin Extent = 
               Top = 153
               Left = 709
               Bottom = 310
               Right = 911
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ComponentNode"
            Begin Extent = 
               Top = 162
               Left = 969
               Bottom = 317
            ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'StandardBodyNode_Summary'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'   Right = 1137
            End
            DisplayFlags = 280
            TopColumn = 3
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 15
         Width = 284
         Width = 1035
         Width = 3750
         Width = 1275
         Width = 2100
         Width = 555
         Width = 840
         Width = 1500
         Width = 2955
         Width = 780
         Width = 750
         Width = 1155
         Width = 1500
         Width = 1905
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 2790
         Alias = 1695
         Table = 3075
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'StandardBodyNode_Summary'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'StandardBodyNode_Summary'
GO
