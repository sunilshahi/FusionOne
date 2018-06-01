////////////////////////////////////
// INSTALL TOOLS
////////////////////////////////////
#tool nuget:?package=Microsoft.Data.Tools.Msbuild

////////////////////////////////////
// INSTALL ADDINS
////////////////////////////////////
#addin nuget:?package=Cake.SqlPackage
//#addin nuget:?package=Cake.ArgumentHelpers

////////////////////////////////////
// INSTALL MODULES
////////////////////////////////////
#module nuget:?package=Cake.LongPath.Module

////////////////////////////////////
// SETUP ARGUMENTS
////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var connection = Argument("connection", "");
var environment = ArgumentOrEnvironmentVariable("environment", "plexosoft_", "local");

var dacpac = File($"./../src/FusionOne.Database/bin/{configuration}/FusionOne.Database.dacpac");
var bacpac = File("./../publish/export/FusionOne.bacpac");

var environmentQualifier =  (environment == "local") ? string.Empty : $".{environment}";
var publishProfile = File(
    $"./../src/FusionOne.Database/PublishProfile/FusionOne.Database{environmentQualifier}.publish.xml");

////////////////////////////////////
// SETUP/TEAR DOWN
////////////////////////////////////
Setup(context =>
{
    Information("Running pre build check.");

    var fileSystemType = Context.FileSystem.GetType();

    if (fileSystemType.ToString()=="Cake.LongPath.Module.LongPathFileSystem")
    {
        Information($"Sucessfully loaded {fileSystemType.Assembly.Location}");
    }
    else
    {
        Error("Failed to load Cake.LongPath.Module");
    }

	Information($"Target Cake Task: {target}");
});

Teardown(context => 
{
	Information("Target Cake Task: {0}", target);
    Information("Utc Completion Time: {0}", DateTime.UtcNow);
});

////////////////////////////////////
// TASKS
////////////////////////////////////
Task("Build")
	.Does(() =>
	{
        MSBuild("./../FusionOne.sln", settings =>
            settings.UseToolVersion(MSBuildToolVersion.VS2017)
				.SetPlatformTarget(PlatformTarget.MSIL)
                .WithProperty("TreatWarningsAsErrors","true")
                .SetVerbosity(Verbosity.Quiet)
                .WithTarget("Build")
                .SetConfiguration(configuration));

        Information("Build completed.");
	});

Task("CleanUp")
    .Does(() =>
    {
        Information("Cleaning up publish folder.");
        EnsureDirectoryExists("./../publish");
        CleanDirectories(new List<string> {"./../publish"});
    });

Task("DeploymentReport")
    .IsDependentOn("Build")
    .IsDependentOn("CleanUp")
    .Does(() =>
    {
        EnsureDirectoryExists("./../publish/scripts");

        SqlPackageDeployReport(settings => 
        {
            settings.SourceFile = dacpac;
            settings.Profile = publishProfile;
            settings.OutputPath = File("./../publish/scripts/DeploymentReport.xml");
        }); 

        Information("DeploymentReport generation completed.");
    });

Task("Script:Isolated")
    .Does(() =>
    {
        EnsureDirectoryExists("./../publish/scripts");

        SqlPackageScript(settings => 
        {
            settings.SourceFile = dacpac;
            settings.Profile = publishProfile;
            settings.OutputPath = File("./../publish/scripts/FusionOne.sql");
        });

        Information("Script generation completed.");
    });

Task("Script")
    .IsDependentOn("DeploymentReport")
    .IsDependentOn("Script:Isolated");

Task("Publish:Isolated")
    .Does(() =>
    {
        SqlPackagePublish(settings => 
        {
            settings.SourceFile = dacpac;
            settings.Profile = publishProfile;
        });

        Information("Publish completed.");
    });

Task("Deploy")
    .IsDependentOn("Publish:Isolated");

Task("Deploy:Script")
    .IsDependentOn("Script:Isolated");

Task("Publish")
	.IsDependentOn("Build")
    .IsDependentOn("Publish:Isolated");

Task("SetConnectionString")
    .WithCriteria(string.IsNullOrEmpty(connection))
    .Does(() => 
    {
        Information("Getting connectionstring from publish profile because no connection string was passed.");

        var xmlPeekSettings = new XmlPeekSettings {
            Namespaces = new Dictionary<string, string> {
                { "msbuild", "http://schemas.microsoft.com/developer/msbuild/2003" }
            }
        };

        // this does not have database
        var connectionString = XmlPeek(
            publishProfile,
            "/msbuild:Project/msbuild:PropertyGroup/msbuild:TargetConnectionString",
            xmlPeekSettings
        );

        //get database name to add it to connection string.
        var database = XmlPeek(
            publishProfile,
            "/msbuild:Project/msbuild:PropertyGroup/msbuild:TargetDatabaseName",
            xmlPeekSettings
        );

        connection = $"{connectionString};database={database}";
    });

Task("Export")
    .IsDependentOn("CleanUp")
    .IsDependentOn("SetConnectionString")
    .Does(() =>
    {
        EnsureDirectoryExists("./../publish/export");
        CleanDirectories(new List<string> {"./../publish/export"});

        SqlPackageExport(settings =>
        {
            settings.SourceConnectionString = connection;
            settings.TargetFile = bacpac;
        });

        Information("Export completed.");
    });

Task("Import")
    .IsDependentOn("SetConnectionString")
    .Does(() =>
    {
        SqlPackageImport(settings => 
        {
            settings.SourceFile = bacpac;
            settings.TargetConnectionString = connection;
        });

        Information("Import completed.");        
    });

Task("Package")
    .IsDependentOn("Build")
    .IsDependentOn("CleanUp")
    .Does(() => 
    {
        CopyDirectory("./../src/FusionOne.Database/bin", 
            "./../publish/src/FusionOne.Database/bin");
        CopyDirectory("./../src/FusionOne.Database/PublishProfile", 
            "./../publish/src/FusionOne.Database/PublishProfile");
        CopyDirectory("./", "./../publish/scripts"); 
        
        CopyDirectory($"./../publish/src", "./../package/src"); 

        EnsureDirectoryExists("./../package");
        Zip("./../publish/", "./../package/package.zip");
        MoveFile("./../package/package.zip","./../publish/package.zip");

        DeleteDirectory("./../package/", new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });

        Information("Publish Package generation completed.");       
    });

Task("Default")
    .IsDependentOn("Publish");


public string ArgumentOrEnvironmentVariable(string name, string envPrefix, string defaultValue)
{
    return Argument<string>(name, EnvironmentVariable(envPrefix + name)) ?? defaultValue;
}

RunTarget(target);