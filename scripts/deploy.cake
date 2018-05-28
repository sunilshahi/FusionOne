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
var target = Argument("target", "Script");
var configuration = Argument("configuration", "Release");

var dacpac = File("./bin/" + configuration + "/FusionOne.Database.dacpac");
var publishProfile = File("./FusionOne.Database.publish.xml");

Task("Script")
    .Does(() =>
    {
        EnsureDirectoryExists("./output");
        CleanDirectories(new List<string> {"./output"});

        SqlPackageScript(settings => 
        {
            settings.SourceFile = dacpac;
            settings.Profile = publishProfile;
            settings.OutputPath = File("./output/FusionOne.sql");
        });

        Information("Script generation completed.");
    });

Task("Publish")
    .Does(() =>
    {
        SqlPackagePublish(settings => 
        {
            settings.SourceFile = dacpac;
            settings.Profile = publishProfile;
        });

        Information("Publish completed.");
    });


RunTarget(target);