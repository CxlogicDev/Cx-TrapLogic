using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxData.SQLServer.Backup
{
    internal class usp_RestoreDatabase
    {
    }
}


/*
 
 ALTER PROCEDURE [dbo].[usp_RestoreDatabase]
	@dbName nvarchar(64)
AS
	BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	

		if exists(select 1 from [dbo].[BackupsProcessed] where [database] like @dbName)
		begin

			declare @scriptId nvarchar(64)
			
			
			select @scriptId = scriptId from [dbo].[BackupsProcessed] where [database] like @dbName
			
	IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name like @dbName)
	begin
	

    -- Insert statements for procedure here
	declare @bakPath nvarchar(512)
	select @bakPath = field1 from [dbo].[DbAttributes] where [identifier] like 'BackupPath';
	
	--Use These Variables Throughout the rest of the Procedure
		
	declare @rfileName nvarchar(128) 
	select @rfileName = @bakPath + [bak_FileName] from [dbo].[BackupsProcessed] where [scriptId] like @scriptId
	
	DECLARE @mdflName [varchar](128)
	select @mdflName = [LDbName] from [dbo].[BackupsProcessed] where [scriptId] like @scriptId

	DECLARE @ldflName [varchar](128)
	select @ldflName = [LLogName] from [dbo].[BackupsProcessed] where [scriptId] like @scriptId

	declare @moveto varchar(128)
	select @moveto = field1 + @dbName + '_db.mdf' from [dbo].[DbAttributes] where [identifier] like 'DefaultDataSource';
	--print @moveto
	declare @moveLogto varchar(128)
	select @moveLogto = field1 + @dbName + '_log.ldf' from [dbo].[DbAttributes] where [identifier] like 'DefaultLogSource';
	--print @moveLogto

	
		-------------------------------------------------------------
		RESTORE DATABASE @dbName
			FROM DISK = @rfileName
			 
				WITH move @mdflName TO @moveto,
				MOVE @ldflName TO @moveLogto,
				RECOVERY;

				declare @isql nvarchar(max);
				set @isql = 'ALTER DATABASE ' + @dbName + ' MODIFY FILE (NAME = ''' + @mdflName + ''', NEWNAME = ''' + @dbName + '_data'')'
				--print @isql
				EXEC sp_executesql @isql
				set @isql = 'ALTER DATABASE ' + @dbName + ' MODIFY FILE (NAME = ''' + @ldflName + ''', NEWNAME = ''' + @dbName + '_log'')'
				--print @isql
				EXEC sp_executesql @isql
				
			end
	
		end
	
	END

 
 */