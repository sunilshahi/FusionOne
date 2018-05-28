////////////////////////////////////
// INSTALL TOOLS
////////////////////////////////////
#tool nuget:?package=Microsoft.Data.Tools.Msbuild

////////////////////////////////////
// INSTALL ADDINS
////////////////////////////////////
#addin nuget:?package=Cake.SqlPackage

////////////////////////////////////
// SETUP ARGUMENTS
////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var connection = Argument("connection", "Server=.;Database=FusionOne;Trusted_Connection=True;");


var dacpac = File("./../src/FusionOne.Database/bin/" + configuration + "/FusionOne.Database.dacpac");
var bacpac = File("./../publish/export/FusionOne.bacpac");
var publishProfile = File("./../src/FusionOne.Database/PublishProfile/FusionOne.Database.publish.xml");

////////////////////////////////////
// SETUP/TEAR DOWN
////////////////////////////////////
Setup(context =>
{
	Information("Target Cake Task: {0}", target);
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
        MSBuild("./../src/FusionOne.sln", settings =>
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
            settings.OutputPath = File("./../publish/scripts/DeploymentReport.txt");
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

Task("ProdPackage")
    .IsDependentOn("Build")
    .Does(() => {
        EnsureDirectoryExists("./../publish/export/bin");
        
        CleanDirectories(new List<string> {"./../publish/export"});

        CopyDirectory("./../src/FusionOne.Database/bin", "./../publish/export/bin");
        CopyFile("./deploy.cake", "./../publish/export/build.cake");
        CopyFile("./deploy.setup.ps1", "./../publish/export/setup.ps1");

        try{
            DownloadFile("https://cakebuild.net/download/bootstrapper/windows", "./../publish/export/build.ps1");
        }
        finally{
            //do nothing there is setup.ps1 file to help
        }
         
        CopyFile("./../src/FusionOne.Database/PublishProfile/FusionOne.Database.production.publish.xml"
               , "./../publish/export/FusionOne.Database.publish.xml"); 
        
        Zip("./../publish/export/", "./../publish/prodpackage.zip");

        Information("Prod Package generation completed.");       
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

Task("Default")
  .IsDependentOn("Publish");

RunTarget(target);