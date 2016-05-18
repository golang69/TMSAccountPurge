USE [Fenergo]
GO

/****** Object:  StoredProcedure [dbo].[PurgeTmsAccounts]    Script Date: 5/29/2015 5:18:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		George Lang
-- Create date: 
-- Description:	Purge CPI TMS Accounts
-- =============================================
CREATE PROCEDURE [dbo].[PurgeTmsAccounts]
	@beforeStatus char(1),
	@afterStatus char(1),
	@updatedBy nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON

	UPDATE le
	SET [LastUpdatedDate] = GETDATE(),
		[LastUpdatedBy] =  @updatedBy
	FROM [gol].[LegalEntity] le
	INNER JOIN [gol].[LETMSAccount] tms
	ON le.[Id] = tms.[LegalEntityId]
	INNER JOIN [TFM_DC].[SDS].[CpiTmsAccountPurge] pur
	ON tms.[AccountNo] = pur.[SourceId]
	WHERE tms.[Status] = @beforeStatus

	SET NOCOUNT OFF
	
	UPDATE tms
	SET [Status] = 'P'
	FROM [gol].[LETMSAccount] tms
	INNER JOIN [TFM_DC].[SDS].[CpiTmsAccountPurge] pur
	ON tms.[AccountNo] = pur.[SourceId]
	WHERE tms.[Status] = 'O'

	RETURN @@ROWCOUNT
END





GO


