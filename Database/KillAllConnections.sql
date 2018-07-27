USE master

/** This script will clear all open connections to ProfitWise **/

ALTER DATABASE Bundler SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
ALTER DATABASE Bundler SET MULTI_USER


