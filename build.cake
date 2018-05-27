////////////////////////////////////
// INSTALL TOOLS
////////////////////////////////////
#tool "nuget:https://www.nuget.org/api/v2?package=Microsoft.Data.Tools.Msbuild&version=10.0.61026"

////////////////////////////////////
// INSTALL ADDINS
////////////////////////////////////
#addin "nuget:https://www.myget.org/F/cake-sqlpackage/api/v2?package=Cake.SqlPackage&version=0.2.0-alpha0008"


////////////////////////////////////
// SETUP ARGUMENTS
////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");


var dacpac = File("./src/FusionOne.Database/bin/" + configuration + "/FusionOne.Database.dacpac");
var publishProfile = File("./src/FusionOne.Database/Publish/FusionOne.Database.publish.xml");

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
        MSBuild("./src/FusionOne.sln", settings =>
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
        EnsureDirectoryExists("./scripts");

        SqlPackageDeployReport(settings => 
        {
            settings.SourceFile = dacpac;
            settings.Profile = publishProfile;
            settings.OutputPath = File("./scripts/DeploymentReport.txt");
        }); 

        Information("DeploymentReport generation completed.");
    });

Task("Script")
    .IsDependentOn("Build")
    .IsDependentOn("DeploymentReport")
    .Does(() =>
    {
        EnsureDirectoryExists("./scripts");

        SqlPackageScript(settings => 
        {
            settings.SourceFile = dacpac;
            settings.Profile = publishProfile;
            settings.OutputPath = File("./scripts/FusionOne.sql");
        });

        Information("Script generation completed.");
    });


Task("Default")
  .IsDependentOn("Script");

RunTarget(target);