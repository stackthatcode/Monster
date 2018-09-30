USE Monster;

--
-- This is a DEV setup where MonsterSys piggybacks on the Monster Instance
--

IF NOT EXISTS ( SELECT * FROM usrTenant WHERE Nickname = 'DEV Testing')
BEGIN
INSERT INTO usrTenant VALUES ( 
	'51aa413d-e679-4f38-ba47-68129b3f9212', 
	'Server=localhost; Database=Monster; Trusted_Connection=True;',
	1, 'DEV Testing' );
END

IF NOT EXISTS ( SELECT * FROM usrTenantContext WHERE CompanyID = 1 )
BEGIN

	INSERT INTO usrTenantContext 
		VALUES ( 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL );
END
