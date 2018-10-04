USE master

/** This script will clear all open connections to ProfitWise **/

ALTER DATABASE Monster SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
ALTER DATABASE Monster SET MULTI_USER


