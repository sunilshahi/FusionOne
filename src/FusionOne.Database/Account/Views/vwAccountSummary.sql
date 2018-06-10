CREATE VIEW [Account].[vwAccountSummary]
	AS 
SELECT 
	U.[FirstName] + ' ' + U.[MiddleName] + ' ' + U.[LastName]	AS [AccountOwnerFullName],
	[AT].[TypeName]												AS [AccountType],
	A.[Balance]													AS [Balance]		
FROM 
	[dbo].[User] U INNER JOIN [Account].[Account] A ON A.[UserId] = U.[Id]
				   INNER JOIN [Account].[AccountType] [AT] ON [AT].Id = A.[AccountTypeId]
