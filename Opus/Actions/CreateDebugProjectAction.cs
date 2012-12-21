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
            // TODO: should be false but other work has to happen first
            Core.PackageUtilities.IdentifyMainAndDependentPackages(true);

            if (0 == Core.State.PackageInfo.Count)
            {
                throw new Core.Exception("Package has not been specified. Run Opus from the package directory.", false);
            }

            Core.PackageUtilities.ProcessLazyArguments();
            Core.PackageUtilities.HandleUnprocessedArguments();

            Core.PackageInformation mainPackage = Core.State.PackageInfo.MainPackage;

            Core.Log.DebugMessage("Package is '{0}' in '{1}'", mainPackage.Identifier.ToString("-"), mainPackage.Identifier.Root);

            Core.BuilderUtilities.SetBuilderPackage();

            // Create resource file containing package information
            string resourceFilePathName = Core.PackageListResourceFile.WriteResXFile();

            // Project to debug the script
            CSharpProject.Create(mainPackage, VisualStudioVersion.VS2008, new string[] { resourceFilePathName });

            Core.Log.Info("Successfully created debug project for package '{0}' at '{1}'",
                          mainPackage.Identifier.ToString("-"),
                          mainPackage.DebugProjectFilename);

            return true;
        }
    }
}