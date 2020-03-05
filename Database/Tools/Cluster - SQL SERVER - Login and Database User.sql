
-- ### SAVE THIS SCRIPT ###
SELECT pr.principal_id, pr.name, pr.type_desc, pe.state_desc, pe.permission_name   
FROM sys.server_principals AS pr   
	JOIN sys.server_permissions AS pe   
		ON pe.grantee_principal_id = pr.principal_id;  

-- ### COPY THIS SCRIPT FOR ALL OF THE DATABASES ###
USE BSASystem
GO
CREATE USER [IIS APPPOOL\BSA] FOR LOGIN [IIS APPPOOL\BSA]
    EXEC sp_addrolemember N'db_owner', N'IIS APPPOOL\BSA'

USE BSAInst001
GO
CREATE USER [IIS APPPOOL\BSA] FOR LOGIN [IIS APPPOOL\BSA]
EXEC sp_addrolemember N'db_owner', N'IIS APPPOOL\BSA'

USE BSAInst002
GO
CREATE USER [IIS APPPOOL\BSA] FOR LOGIN [IIS APPPOOL\BSA]
EXEC sp_addrolemember N'db_owner', N'IIS APPPOOL\BSA'

USE BSAInst003
GO
CREATE USER [IIS APPPOOL\BSA] FOR LOGIN [IIS APPPOOL\BSA]
EXEC sp_addrolemember N'db_owner', N'IIS APPPOOL\BSA'

