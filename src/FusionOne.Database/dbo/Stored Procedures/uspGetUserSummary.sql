CREATE PROCEDURE [dbo].[uspGetUsersSummary]
	@UserId INT = 0
AS
	SELECT 
		 [Id]		
		,[FirstName] + ' ' + [MiddleName] + ' ' + [LastName] AS [FullName]	
		,[UserName]	
		,[Email]	
	FROM 
		[dbo].[User]
	WHERE 
		[Id] = @UserId
RETURN 0
