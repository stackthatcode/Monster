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

