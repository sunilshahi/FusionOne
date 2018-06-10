SET NOCOUNT ON

SET IDENTITY_INSERT [User] ON

MERGE INTO [User] AS Target
USING (VALUES
  (1,N'Sunil',NULL,N'Shahi',N'Sunil.Shahi',N'sunil.shahi@fusionstak.com',1)
 ,(2,N'John',NULL,N'Doe',N'John.Doe',N'john.doe@email.com',1)
) AS Source ([Id],[FirstName],[MiddleName],[LastName],[UserName],[Email],[Active])
ON (Target.[Id] = Source.[Id])
WHEN MATCHED AND (
	NULLIF(Source.[FirstName], Target.[FirstName]) IS NOT NULL OR NULLIF(Target.[FirstName], Source.[FirstName]) IS NOT NULL OR 
	NULLIF(Source.[MiddleName], Target.[MiddleName]) IS NOT NULL OR NULLIF(Target.[MiddleName], Source.[MiddleName]) IS NOT NULL OR 
	NULLIF(Source.[LastName], Target.[LastName]) IS NOT NULL OR NULLIF(Target.[LastName], Source.[LastName]) IS NOT NULL OR 
	NULLIF(Source.[UserName], Target.[UserName]) IS NOT NULL OR NULLIF(Target.[UserName], Source.[UserName]) IS NOT NULL OR 
	NULLIF(Source.[Email], Target.[Email]) IS NOT NULL OR NULLIF(Target.[Email], Source.[Email]) IS NOT NULL OR 
	NULLIF(Source.[Active], Target.[Active]) IS NOT NULL OR NULLIF(Target.[Active], Source.[Active]) IS NOT NULL) THEN
 UPDATE SET
  [FirstName] = Source.[FirstName], 
  [MiddleName] = Source.[MiddleName], 
  [LastName] = Source.[LastName], 
  [UserName] = Source.[UserName], 
  [Email] = Source.[Email], 
  [Active] = Source.[Active]
WHEN NOT MATCHED BY TARGET THEN
 INSERT([Id],[FirstName],[MiddleName],[LastName],[UserName],[Email],[Active])
 VALUES(Source.[Id],Source.[FirstName],Source.[MiddleName],Source.[LastName],Source.[UserName],Source.[Email],Source.[Active])
WHEN NOT MATCHED BY SOURCE THEN 
 DELETE
;
GO
DECLARE @mergeError int
 , @mergeCount int
SELECT @mergeError = @@ERROR, @mergeCount = @@ROWCOUNT
IF @mergeError != 0
 BEGIN
 PRINT 'ERROR OCCURRED IN MERGE FOR [User]. Rows affected: ' + CAST(@mergeCount AS VARCHAR(100)); -- SQL should always return zero rows affected
 END
ELSE
 BEGIN
 PRINT '[User] rows affected by MERGE: ' + CAST(@mergeCount AS VARCHAR(100));
 END
GO

SET IDENTITY_INSERT [User] OFF
GO
SET NOCOUNT OFF
GO