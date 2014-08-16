// <copyright file="CreateDebugProjectAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.CreateDebugProjectAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class CreateDebugProjectAction :
        Core.IAction
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

        public bool
        Execute()
        {
            // TODO: would be nice not to have to resolve down to a single version in the debug project
            // but there are namespace clashes if you do
            Core.PackageUtilities.IdentifyMainAndDependentPackages(true, false);

            if (0 == Core.State.PackageInfo.Count)
            {
                throw new Core.Exception("Package has not been specified. Run Opus from the package directory.");
            }

            var fatal = false;
            Core.PackageUtilities.ProcessLazyArguments(fatal);
            Core.PackageUtilities.HandleUnprocessedArguments(fatal);

            var mainPackage = Core.State.PackageInfo.MainPackage;

            Core.Log.DebugMessage("Package is '{0}' in '{1}'", mainPackage.Identifier.ToString("-"), mainPackage.Identifier.Root);

            // this is now optional - if you pass -builder=<name> then the generated package will be limited to that
            // otherwise, all packages with names ending in 'Builder' will have their scripts added
            Core.BuilderUtilities.SetBuilderPackage();

            // Create resource file containing package information
            var resourceFilePathName = Core.PackageListResourceFile.WriteResXFile();

            // Project to debug the script
            CSharpProject.Create(mainPackage, VisualStudioVersion.VS2008, new string[] { resourceFilePathName });

            Core.Log.Info("Successfully created debug project for package '{0}'",
                          mainPackage.Identifier.ToString("-"));
            Core.Log.Info("\t{0}",
                          mainPackage.DebugProjectFilename);

            return true;
        }

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}