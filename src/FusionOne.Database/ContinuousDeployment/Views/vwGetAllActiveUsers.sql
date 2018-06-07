CREATE VIEW [dbo].[vwGetAllActiveUser]
	AS 
SELECT 
	 [Id]		
	,[FirstName]	
	,[MiddleName]
	,[LastName]	
	,[UserName]	
	,[Email]		
	,[Active]	
FROM 
	[dbo].[User]
WHERE 
	[Active] = 1
