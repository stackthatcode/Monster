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

UPDATE AspNetUsers SET Email = 'info@logicautomated.com' WHERE Id = '0d8d758b-0815-4e92-a464-863b5c4d7291';

