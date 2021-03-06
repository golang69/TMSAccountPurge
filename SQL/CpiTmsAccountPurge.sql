USE [TFM_DC]
GO

/****** Object:  Table [SDS].[CpiTmsAccountPurge]    Script Date: 5/29/2015 3:33:25 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [SDS].[CpiTmsAccountPurge](
	[SourceId] [bigint] NOT NULL,
 CONSTRAINT [PK_CpiTmsAccountPurge] PRIMARY KEY CLUSTERED 
(
	[SourceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [TFM_DC_Group1]
) ON [TFM_DC_Group1]

GO

