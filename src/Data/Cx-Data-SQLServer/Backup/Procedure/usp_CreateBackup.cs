using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxData.SQLServer.Backup
{
    internal class usp_CreateBackup
    {
    }
}


/*
 
 IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_CreateBackup]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_CreateBackup] AS' 
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[usp_CreateBackup]
	-- Add the parameters for the stored procedure here
	@dbName nvarchar(64)
	,@scriptId nvarchar(64) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	if(@scriptId is null)
		set @scriptId = cast(NEWID() as nvarchar(64))

	    -- Insert statements for procedure here
	declare @dbBak table (dbname nvarchar(64))
	insert into @dbBak (dbname) values (@dbName)
	
	
	--Use These Variables Throughout the rest of the Procedure
	DECLARE @name VARCHAR(64)

	DECLARE db_cursor CURSOR FOR  
	SELECT dbname 
	FROM @dbBak
	 
	OPEN db_cursor   
	FETCH NEXT FROM db_cursor INTO @name 
	 
	WHILE @@FETCH_STATUS = 0   
	BEGIN   

		DECLARE @BackupPath NVARCHAR(512)
	select @BackupPath = field1 from [dbo].[DbAttributes] where [identifier] like 'BackupPath'
	print 'BackupPath: ' + @BackupPath
	DECLARE @BackupPostfix NVARCHAR(512)
	select @BackupPostfix = field1 from [dbo].[DbAttributes] where [identifier] like 'BackupPostfix'
	print 'BackupPostfix: ' + @BackupPostfix


		declare @fileName nvarchar(128) = @BackupPath + @dbName + @BackupPostfix
		
		if(@scriptId is not null)
		begin
			set @fileName = @BackupPath + @dbName + '-' + @scriptId + @BackupPostfix
		end

		print @fileName

		BACKUP DATABASE @name TO DISK = @fileName  
		with COPY_ONLY;
		-------------------------------------------------------------
		
		DECLARE @mdflName [varchar](128)
		DECLARE @ldflName [varchar](128)
	
		SELECT @mdflName = name FROM sys.master_files AS mf where DB_NAME(mf.database_id) = @dbName and mf.name not like '%log%'
       print 'mdf Logical Name: ' + @mdflName
		SELECT @ldflName = name FROM sys.master_files AS mf where DB_NAME(mf.database_id) = @dbName and mf.name like '%log%'
       print 'LDF Logical Name: ' + @ldflName

		
		INSERT INTO [dbo].[BackupsProcessed]
           ([status]
           ,[bak_FileName]
           ,[database]
           ,[LDbName]
           ,[LLogName]
           ,[scriptId])
     VALUES
           (1           
           ,REPLACE(@fileName, @BackupPath, '')
           ,@name
           ,@mdflName
           ,@ldflName
           ,@scriptId)

		   --------------------------------------------------------

		
		FETCH NEXT FROM db_cursor INTO @name
	End
	
CLOSE db_cursor   
DEALLOCATE db_cursor
	
END
 
 
 */