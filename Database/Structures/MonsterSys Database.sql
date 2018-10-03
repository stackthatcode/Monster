USE [MonsterSys]
GO
/****** Object:  Table [dbo].[usrInstallation]    Script Date: 10/3/2018 3:57:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrInstallation](
	[InstallationId] [uniqueidentifier] NULL,
	[ConnectionString] [varchar](500) NULL,
	[CompanyID] [bigint] NULL,
	[Nickname] [varchar](50) NULL
) ON [PRIMARY]
GO
