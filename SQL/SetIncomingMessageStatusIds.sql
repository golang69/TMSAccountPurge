USE [TFM]
GO

/****** Object:  StoredProcedure [dbo].[SetIncomingMessageStatusIds]    Script Date: 5/29/2015 3:30:42 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		George Lang
-- Create date: 2015-05-29
-- Description:	Set MessageStatusId of input set of IncomingMessageIds
-- =============================================
CREATE PROCEDURE [dbo].[SetIncomingMessageStatusIds]
	@incomingMessageIds MessageIdsTableType READONLY,
	@messageStatusId tinyint = 1
AS
BEGIN

	UPDATE inc
	SET [MessageStatusId] = @messageStatusId,
		[ProcessedOn] = GETDATE()
	FROM [dbo].[IncomingMessage] inc
	INNER JOIN @incomingMessageIds ids
	ON inc.IncomingMessageId = ids.IncomingMessageId 

	RETURN @@ROWCOUNT
END


GO