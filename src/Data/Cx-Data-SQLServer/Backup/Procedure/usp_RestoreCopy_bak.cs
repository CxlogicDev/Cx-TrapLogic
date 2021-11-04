using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxData.SQLServer.Backup
{
    internal class usp_RestoreCopy_bak
    {
    }
}


/* ------ WORKS With >> usp_CreateBackup
 -- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	Use the scriptId to Restore A bak file to DB
-- =============================================
CREATE PROCEDURE [dbo].[usp_Restore_bak]
	-- Add the parameters for the stored procedure here
	@scriptId nvarchar(64)
	,@dbRestoreName nvarchar(64)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	if (len(isnull(@scriptId, '')) <= 0) OR (len(isnull(@dbRestoreName, '')) <= 0)
		return 0;--Do not process if no Script ID or Restore Name
		
	
	IF DB_ID(@dbRestoreName) IS NOT NULL-- EXISTS (SELECT name FROM sys.databases WHERE name = @dbRestoreName)
	begin --Return if the Restore name is already in the database
		return 0
	end

	declare @bakPath nvarchar(512)
	select @bakPath = field1 from [dbo].[DbAttributes] where [identifier] like 'BackupPath';

	declare @rfileName nvarchar(128) 
	select @rfileName = @bakPath + [bak_FileName] from [dbo].[BackupsProcessed] where [scriptId] like @scriptId
	
	DECLARE @mdflName [varchar](128)
	select @mdflName = [LDbName] from [dbo].[BackupsProcessed] where [scriptId] like @scriptId

	DECLARE @ldflName [varchar](128)
	select @ldflName = [LLogName] from [dbo].[BackupsProcessed] where [scriptId] like @scriptId

	declare @moveto varchar(128)
	select @moveto = field1 + @dbRestoreName + '_db.mdf' from [dbo].[DbAttributes] where [identifier] like 'DefaultDataSource';
	--print @moveto
	declare @moveLogto varchar(128)
	select @moveLogto = field1 + @dbRestoreName + '_log.ldf' from [dbo].[DbAttributes] where [identifier] like 'DefaultLogSource';
	--print @moveLogto

	
		-------------------------------------------------------------
		RESTORE DATABASE @dbRestoreName
			FROM DISK = @rfileName
			 
				WITH move @mdflName TO @moveto,
				MOVE @ldflName TO @moveLogto,
				RECOVERY;

				--declare @isql nvarchar(max);
				--set @isql = 'ALTER DATABASE ' + @dbRestoreName + ' MODIFY FILE (NAME = ''' + @mdflName + ''', NEWNAME = ''' + @dbRestoreName + '_data'')'
				----print @isql
				--EXEC sp_executesql @isql
				--set @isql = 'ALTER DATABASE ' + @dbRestoreName + ' MODIFY FILE (NAME = ''' + @ldflName + ''', NEWNAME = ''' + @dbRestoreName + '_log'')'
				----print @isql
				--EXEC sp_executesql @isql

END

 
 */