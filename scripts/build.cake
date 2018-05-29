////////////////////////////////////
// INSTALL TOOLS
////////////////////////////////////
#tool nuget:?package=Microsoft.Data.Tools.Msbuild

////////////////////////////////////
// INSTALL ADDINS
////////////////////////////////////
#addin nuget:?package=Cake.SqlPackage

////////////////////////////////////
// INSTALL MODULES
////////////////////////////////////
#module nuget:?package=Cake.LongPath.Module

////////////////////////////////////
// SETUP ARGUMENTS
////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var connection = Argument("connection", "Server=.;Database=FusionOne;Trusted_Connection=True;");
var environment = Argument("environment", "production");

var dacpac = File($"./../src/FusionOne.Database/bin/{configuration}/FusionOne.Database.dacpac");
var bacpac = File("./../publish/export/FusionOne.bacpac");
var publishProfile = File("./../src/FusionOne.Database/PublishProfile/FusionOne.Database.publish.xml");

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

Task("DeploymentReport")
    .IsDependentOn("Build")
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

Task("Script")
    .IsDependentOn("Build")
    .IsDependentOn("DeploymentReport")
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

Task("Publish")
	.IsDependentOn("Build")
    .Does(() =>
    {
        SqlPackagePublish(settings => 
        {
            settings.SourceFile = dacpac;
            settings.Profile = publishProfile;
        });

        Information("Publish completed.");
    });


Task("Export")
    .Does(() =>
    {
        EnsureDirectoryExists("./../publish/export");
        CleanDirectories(new List<string> {"./../publish/export"});

        SqlPackageExport(settings =>
        {
            settings.SourceConnectionString = connection;
            settings.Profile = publishProfile;
            settings.TargetFile = bacpac;
        });

        Information("Export completed.");
    });

Task("Import")
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
    .Does(() => {

        EnsureDirectoryExists("./../publish");
        CleanDirectories(new List<string> {"./../publish"});
        
        CopyDirectory("./../src/FusionOne.Database/bin", "./../publish/bin");

        EnsureDirectoryExists("./../publish/PublishProfile");
        CopyDirectory($"./../src/FusionOne.Database/PublishProfile", "./../publish/PublishProfile");


        EnsureDirectoryExists("./../package");
        CopyDirectory($"./../scripts", "./../package/scripts"); 
        CopyDirectory($"./../publish", "./../package/publish"); 

        Zip("./../package/", "./../publish/package.zip"); 

        DeleteDirectory("./../package/", new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });

        Information("Publish Package generation completed.");       
    });

Task("Deploy")
    .Does(() =>
    {
        var localDacpac = File($"./../publish/bin/{configuration}/FusionOne.Database.dacpac");
        var localPublishProfile = 
            File($"./../publish/PublishProfile/FusionOne.Database.{environment}.publish.xml");

        SqlPackagePublish(settings => 
        {
            settings.SourceFile = localDacpac;
            settings.Profile = localPublishProfile;
        });

        Information("Deploy completed.");
    });

Task("Default")
  .IsDependentOn("Publish");

RunTarget(target);