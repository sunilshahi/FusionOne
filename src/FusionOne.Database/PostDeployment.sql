/*
Post-Deployment Script Template
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.
 Use SQLCMD syntax to include a file in the post-deployment script.
 Example:      :r .\myfile.sql
 Use SQLCMD syntax to reference a variable in the post-deployment script.
 Example:      :setvar TableName MyTable
               SELECT * FROM [$(TableName)]
--------------------------------------------------------------------------------------
*/

GO
IF N'$(Flag)' NOT LIKE N'True'
BEGIN
    PRINT N'Turning execution off.';
    SET NOEXEC ON;
END


-- This section will only run when flag is set to true. 

SELECT 'HELLO'

SET NOEXEC OFF;

SELECT 'WORLD'
