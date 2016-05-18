USE [TFM]
GO

/****** Object:  UserDefinedTableType [dbo].[MessageIdsTableType]    Script Date: 5/29/2015 2:31:48 PM ******/
CREATE TYPE [dbo].[MessageIdsTableType] AS TABLE(
	[IncomingMessageId] [uniqueidentifier] NOT NULL
)
GO


