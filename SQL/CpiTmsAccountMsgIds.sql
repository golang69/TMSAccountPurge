USE [TFM_DC]
GO

/****** Object:  Table [SDS].[CpiTmsAccountMsgIds]    Script Date: 5/29/2015 3:33:07 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [SDS].[CpiTmsAccountMsgIds](
	[IncomingMessageId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_CpiTmsAccountMsgIds] PRIMARY KEY CLUSTERED 
(
	[IncomingMessageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [TFM_DC_Group1]
) ON [TFM_DC_Group1]

GO

