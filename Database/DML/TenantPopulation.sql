USE Monster;

IF NOT EXISTS ( SELECT * FROM usrTenant WHERE Nickname = 'DEV Testing')
BEGIN
INSERT INTO usrTenant VALUES ( 
	'51aa413d-e679-4f38-ba47-68129b3f9212', 
	'Server=localhost; Database=Monster; Trusted_Connection=True;',
	1, 'DEV Testing' );
END

SELECT * FROM usrTenant;