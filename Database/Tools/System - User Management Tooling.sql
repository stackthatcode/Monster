USE MonsterSys
GO;

-- Clean Synchronization data
--
DROP PROCEDURE IF EXISTS dbo.DeleteUserAccount
GO

CREATE PROCEDURE dbo.DeleteUserAccount
	@Email varchar(50)
AS
	UPDATE Instance SET OwnerUserId = NULL, OwnerNickname = NULL, OwnerDomain = NULL
	WHERE OwnerUserId IN ( SELECT Id FROM AspNetUsers WHERE Email = @Email );

	DELETE FROM AspNetUserRoles WHERE UserId IN ( SELECT Id FROM AspNetUsers WHERE Email = @Email );

	DELETE FROM AspNetUserLogins WHERE UserId IN ( SELECT Id FROM AspNetUsers WHERE Email = @Email );

	DELETE FROM AspNetUsers WHERE Email = @Email;
GO

USE MonsterSys;
GO

SELECT t1.Id, t1.Email, t1.UserName, t2.LoginProvider, t2.ProviderKey, t4.Name AS RoleName, t5.OwnerNickname, t5.OwnerDomain, t5.ConnectionString
FROM AspNetUsers t1 
	LEFT JOIN AspNetUserLogins t2
		ON t1.Id = t2.UserId
	LEFT JOIN AspNetUserRoles t3
		ON t1.Id = t3.UserId
	LEFT JOIN AspNetRoles t4
		ON t3.RoleId = t4.Id
	LEFT JOIN Instance t5
		ON t1.Id = t5.OwnerUserId


/*SELECT * FROM AspNetUsers
SELECT * FROM AspNetUserLogins;
SELECT * FROM Instance;*/

/*
EXEC dbo.DeleteUserAccount 'aleks@logicautomated.com';
*/


