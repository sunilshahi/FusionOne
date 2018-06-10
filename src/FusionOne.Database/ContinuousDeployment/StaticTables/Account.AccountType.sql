SET NOCOUNT ON

SET IDENTITY_INSERT [Account].[AccountType] ON

MERGE INTO [Account].[AccountType] AS Target
	USING (
	VALUES
		 (1,N'Debit'	)
		,(2,N'Savings'	)
		,(3,N'Cash'		)
		,(4,N'Revenue'	)
		,(5,N'Credit'	)
	) AS Source ([Id],[TypeName])
ON (Target.[Id] = Source.[Id])
WHEN MATCHED AND (
	NULLIF(Source.[TypeName], Target.[TypeName]) IS NOT NULL OR NULLIF(Target.[TypeName], Source.[TypeName]) IS NOT NULL
) THEN
UPDATE 
	SET
	[TypeName] = Source.[TypeName]
WHEN NOT MATCHED BY TARGET THEN
	INSERT([Id],[TypeName])
VALUES(Source.[Id],Source.[TypeName])
WHEN NOT MATCHED BY SOURCE THEN 
DELETE;
GO

DECLARE @mergeError int, @mergeCount int
SELECT  @mergeError = @@ERROR, @mergeCount = @@ROWCOUNT
IF @mergeError != 0
	BEGIN
		PRINT 'ERROR OCCURRED IN MERGE FOR [Account].[AccountType]. Rows affected: ' + CAST(@mergeCount AS VARCHAR(100)); -- SQL should always return zero rows affected
	END
ELSE
	BEGIN
		PRINT '[Account].[AccountType] rows affected by MERGE: ' + CAST(@mergeCount AS VARCHAR(100));
	END
GO

SET IDENTITY_INSERT [Account].[AccountType] OFF
GO
SET NOCOUNT OFF
GO
