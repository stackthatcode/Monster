USE MonsterSys;

DELETE FROM MonsterSys..usrTenant;
DELETE FROM Monster0001..usrTenantContext;
DELETE FROM Monster0002..usrTenantContext;


DECLARE @nickName1 varchar(100) = 'Bridge Over Monsters';
DECLARE @connString1 varchar(250) = 'Server=localhost; Database=Monster0001; Trusted_Connection=True;';

IF NOT EXISTS ( SELECT * FROM usrTenant WHERE Nickname = @nickName1)
BEGIN
INSERT INTO usrTenant VALUES ( 
	'51aa413d-e679-4f38-ba47-68129b3f9212', @connString1, 1, @nickName1 );
END


DECLARE @nickName2 varchar(100) = '3D Universe';
DECLARE @connString2 varchar(250) = 'Server=localhost; Database=Monster0002; Trusted_Connection=True;';

IF NOT EXISTS ( SELECT * FROM usrTenant WHERE Nickname = @nickName2)
BEGIN
INSERT INTO usrTenant VALUES ( 
	'1adacc65-43eb-4083-9a14-1d3601f52328', @connString2, 1, @nickName2 );
END


SELECT * FROM MonsterSys..usrInstallation;
SELECT * FROM Monster0001..usrTenant;
SELECT * FROM Monster0002..usrTenant;

--DELETE FROM usrTenant;



