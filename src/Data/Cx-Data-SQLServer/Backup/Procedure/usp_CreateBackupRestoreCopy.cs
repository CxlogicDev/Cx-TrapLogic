using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxData.SQLServer.Backup
{
    internal class usp_CreateBackupRestoreCopy
    {
    }
}


/*
 
 -- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	Use this script to create a copy of a database as the Restore name or by default  (@dbName)_copy
-- =============================================
ALTER PROCEDURE [dbo].[usp_CreateBackupRestoreCopy2]
	-- Add the parameters for the stored procedure here
	@dbName nvarchar(64)
	,@dbRestoreName nvarchar(64) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	declare @scriptId nvarchar(64) = cast(NEWID() as nvarchar(64))

	exec dbo.usp_CreateBackup @dbName, @scriptId
	
	-- Pre set up
	declare @dbRstName nvarchar(64)
	if(@dbRestoreName is null or @dbRestoreName = '')
	begin
		set @dbRstName = @dbName + '_copy'
	end
	else 
	begin
		set @dbRstName = @dbRestoreName
	end

	IF  EXISTS (SELECT name FROM sys.databases WHERE name = @dbRstName)
	begin
		return 0
	end

    -- Insert statements for procedure here
		
	--declare @dbRst table (dbname nvarchar(64))
	--insert into @dbRst (dbname) values (@dbRstName)
	
	--Use These Variables Throughout the rest of the Procedure
	--declare @name nvarchar(64)
	
	--DECLARE db_cursor CURSOR FOR  
	--SELECT dbname 
	--FROM @dbRst
	 
	--OPEN db_cursor   
	--FETCH NEXT FROM db_cursor INTO @name   
	 
	--WHILE @@FETCH_STATUS = 0   
	--BEGIN  
	
	-- do not restore to same database 
	if(@dbRstName = @dbName)
	begin	
		return 0
	end
	
	declare @bakPath nvarchar(512)
	select @bakPath = field1 from [dbo].[DbAttributes] where [identifier] like 'BackupPath';

	declare @rfileName nvarchar(128) 
	select @rfileName = @bakPath + [bak_FileName] from [dbo].[BackupsProcessed] where [scriptId] like @scriptId
	--=  @BackupPath + @dbName + '-' + @scriptId + @BackupPostfix
	--'D:\Storage\ScriptBackup\' + @dbName + '_' + @scriptId + '.scriptBackup.bak'--'D:\Storage\ScriptBackup\' + @dbName + '.scriptBackup.bak'
	DECLARE @mdflName [varchar](128)
	select @mdflName = [LDbName] from [dbo].[BackupsProcessed] where [scriptId] like @scriptId

	DECLARE @ldflName [varchar](128)
	select @ldflName = [LLogName] from [dbo].[BackupsProcessed] where [scriptId] like @scriptId

	declare @moveto varchar(128)
	select @moveto = field1 + @dbRstName + '_db.mdf' from [dbo].[DbAttributes] where [identifier] like 'DefaultDataSource';
	--print @moveto
	declare @moveLogto varchar(128)
	select @moveLogto = field1 + @dbRstName + '_log.ldf' from [dbo].[DbAttributes] where [identifier] like 'DefaultDataSource';
	--print @moveLogto

	
		-------------------------------------------------------------
		RESTORE DATABASE @dbRstName
			FROM DISK = @rfileName
			 
				WITH move @mdflName TO @moveto,
				MOVE @ldflName TO @moveLogto,
				RECOVERY;

				declare @isql nvarchar(max);
				set @isql = 'ALTER DATABASE ' + @dbRstName + ' MODIFY FILE (NAME = ''' + @mdflName + ''', NEWNAME = ''' + @dbRstName + '_data'')'
				--print @isql
				EXEC sp_executesql @isql
				set @isql = 'ALTER DATABASE ' + @dbRstName + ' MODIFY FILE (NAME = ''' + @ldflName + ''', NEWNAME = ''' + @dbRstName + '_log'')'
				--print @isql
				EXEC sp_executesql @isql


		
--		FETCH NEXT FROM db_cursor INTO @name 
--	End
	
--	CLOSE db_cursor   
--DEALLOCATE db_cursor
	
	
	
END
 
 */