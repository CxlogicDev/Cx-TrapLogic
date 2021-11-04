using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxData.SQLServer.Backup
{
    internal class usp_RestoreDatabasebackup
    {
    }
}


/*
 
 ALTER PROCEDURE [dbo].[usp_RestoreDatabasebackup]
	 @dbName nvarchar(64)-- = 'DatabaseName'
	,@scriptId nvarchar(64)-- = 'Guid or ID'
	,@mdflName nvarchar(128)-- = 'MainData File'
	,@ldflName nvarchar(128)-- = 'Log File'
	,@datamovetoprefix nvarchar(128)-- = 'MainData File Directory [EX: D:\MSSQL\Data\]' 
	,@logmovetoprefix nvarchar(128)-- = 'Log File Directory D:\MSSQL\Log\' 
	,@bakprefix nvarchar(128)-- = 'Backup restore file Directory [Ex: E:\BackUp-12-24-26\]'
AS
	Begin
	-- SET NOCOUNT ON added to prevent extra result sets from
	
	-- Pre set up
	
	declare @rfileName nvarchar(128) = @bakprefix + @dbName + '_' + @scriptId + '.scriptBackup.bak'	
	declare @moveto varchar(128)
	declare @moveLogto varchar(128)
		
	set @moveto = @datamovetoprefix + @dbName + '_data.mdf';
		print @moveto	
	
	set @moveLogto = @logmovetoprefix + @dbName + '_log.ldf';
		print @moveLogto
		-------------------------------------------------------------
		RESTORE DATABASE americanfreight26
			FROM DISK = @rfileName
			 
				WITH move @mdflName TO @moveto,
				MOVE @ldflName TO @moveLogto,
				RECOVERY;
		
	end
 
 */