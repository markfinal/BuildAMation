// <copyright file="CreateDebugProjectAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.CreateDebugProjectAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class CreateDebugProjectAction : Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-createdebugproject";
            }
        }

        public string Description
        {
            get
            {
                return "Create a VisualStudio project of the package scripts and dependencies to debug";
            }
        }

        private string PackagePath
        {
            get;
            set;
        }

        public bool Execute()
        {
            if (0 == Core.State.PackageInfo.Count)
            {
                throw new Core.Exception("Package has not been specified. Run Opus from the package directory.", false);
            }

            Core.PackageInformation mainPackage = Core.State.PackageInfo.MainPackage;

            Core.Log.DebugMessage("Package is '{0}-{1}' in '{2}", mainPackage.Name, mainPackage.Version, mainPackage.Root);

            // recursively discover dependent packages
            Core.XmlPackageDependencyDiscovery.Execute(mainPackage);

            Core.BuilderUtilities.EnsureBuilderPackageExists();

            // Create resource file containing package information
            string resourceFilePathName = Core.PackageListResourceFile.WriteResXFile();

            // Project to debug the script
            CSharpProject.Create(mainPackage, VisualStudioVersion.VS2008, new string[] { resourceFilePathName });

            Core.Log.Info("Successfully created debug project for package '{0}-{1}' at '{2}'",
                          mainPackage.Name,
                          mainPackage.Version,
                          mainPackage.DebugProjectFilename);

            return true;
        }
    }
}