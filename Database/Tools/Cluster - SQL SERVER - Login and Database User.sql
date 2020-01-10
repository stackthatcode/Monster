
-- ### SAVE THIS SCRIPT ###
SELECT pr.principal_id, pr.name, pr.type_desc, pe.state_desc, pe.permission_name   
FROM sys.server_principals AS pr   
	JOIN sys.server_permissions AS pe   
		ON pe.grantee_principal_id = pr.principal_id;  

-- ### COPY THIS SCRIPT FOR ALL OF THE DATABASES ###
USE BSA01System
GO
CREATE USER [IIS APPPOOL\BSA01] FOR LOGIN [IIS APPPOOL\BSA01]
    EXEC sp_addrolemember N'db_owner', N'IIS APPPOOL\BSA01'

USE BSA01Inst001
GO
CREATE USER [IIS APPPOOL\BSA01] FOR LOGIN [IIS APPPOOL\BSA01]
EXEC sp_addrolemember N'db_owner', N'IIS APPPOOL\BSA01'

USE BSA01Inst002
GO
CREATE USER [IIS APPPOOL\BSA01] FOR LOGIN [IIS APPPOOL\BSA01]
EXEC sp_addrolemember N'db_owner', N'IIS APPPOOL\BSA01'

USE BSA01Inst003
GO
CREATE USER [IIS APPPOOL\BSA01] FOR LOGIN [IIS APPPOOL\BSA01]
EXEC sp_addrolemember N'db_owner', N'IIS APPPOOL\BSA01'

