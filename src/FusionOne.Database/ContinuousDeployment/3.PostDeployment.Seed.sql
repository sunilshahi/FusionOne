/*
	Include all your seed scripts in this file. You can conditionally turn on/of 
	executing this script using "Seed" variable.

	1. Invoke the following procedure to generate your MERGE statements:
		Example: EXEC sp_generate_merge @schema = 'dbo', @table_name ='Table'

	2. Paste it to a new file in the Data/Seed folder.

	3. Include it in the list below. Order of the tables in the list depends on your table relationships.
		Begin with the tables at the end of your relationship chain.

	** Set "Seed" to True if you want to run dev seed scripts while publishing.
*/

GO

IF N'$(Seed)' NOT LIKE N'True'
BEGIN
    PRINT N'				Skipping seed scripts execution.';
    SET NOEXEC ON;
END

PRINT N'				Running seed script(s)...';

:r .\Data\Seed\User.sql

SET NOEXEC OFF;