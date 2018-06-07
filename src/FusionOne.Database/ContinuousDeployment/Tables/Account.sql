CREATE TABLE [Account].[Account]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] INT NOT NULL, 
    [Balance] DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    [AccountTypeId] INT NOT NULL, 
    CONSTRAINT [FK_Account_User] FOREIGN KEY ([UserId]) REFERENCES [User]([Id]), 
    CONSTRAINT [FK_Account_AccountType] FOREIGN KEY ([AccountTypeId]) REFERENCES [Account].[AccountType]([Id])
)
