#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License

[assembly: Bam.Core.RegisterAction(typeof(Bam.CreateDebugProjectAction))]

namespace Bam
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
                throw new Core.Exception("Package has not been specified. Re-run from the package directory.");
            }

            var fatal = false;
            Core.PackageUtilities.ProcessLazyArguments(fatal);
            Core.PackageUtilities.HandleUnprocessedArguments(fatal);

            var mainPackage = Core.State.PackageInfo.MainPackage;

            Core.Log.DebugMessage("Package is '{0}' in '{1}'", mainPackage.Identifier.ToString("-"), mainPackage.Identifier.Root.GetSingleRawPath());

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