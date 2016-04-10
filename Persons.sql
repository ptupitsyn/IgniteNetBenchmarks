USE [IgniteNetBenchmarks]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Persons]') AND type in (N'U'))
DROP TABLE [dbo].[Persons]
GO

USE [IgniteNetBenchmarks]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Persons](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar] (100) NOT NULL,
	[Data] [nvarchar] (100) NOT NULL,
    CONSTRAINT [PK_Persons] PRIMARY KEY CLUSTERED (	[ID] ASC ) ) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO