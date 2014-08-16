// <copyright file="AddPackageRootsAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.AddPackageRootsAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class AddPackageRootsAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-addpackageroots";
            }
        }

        public string Description
        {
            get
            {
                return "Add package roots (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        private Core.StringArray PackageRoots
        {
            get;
            set;
        }

        void
        Opus.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            var roots = arguments.Split(System.IO.Path.PathSeparator);
            this.PackageRoots = new Opus.Core.StringArray(roots);
        }

        public bool
        Execute()
        {
            bool isWellDefined;
            var mainPackageId = Core.PackageUtilities.IsPackageDirectory(Core.State.WorkingDirectory, out isWellDefined);
            if (null == mainPackageId)
            {
                throw new Core.Exception("Working directory, '{0}', is not a package", Core.State.WorkingDirectory);
            }
            if (!isWellDefined)
            {
                throw new Core.Exception("Working directory, '{0}', is not a valid package", Core.State.WorkingDirectory);
            }

            var xmlFile = new Core.PackageDefinitionFile(mainPackageId.DefinitionPathName, true);
            if (isWellDefined)
            {
                // note: do not validate package locations, as the package roots may not yet be set up
                xmlFile.Read(true, false);
            }

            var success = false;
            foreach (var packageRoot in this.PackageRoots)
            {
                var standardPackageRoot = packageRoot.Replace('\\', '/');
                if (!xmlFile.PackageRoots.Contains(standardPackageRoot))
                {
                    // note: adding the relative path, so this package can be moved around
                    xmlFile.PackageRoots.Add(standardPackageRoot);
                    Core.Log.MessageAll("Added package root '{0}' to package '{1}'", standardPackageRoot, mainPackageId.ToString());
                    success = true;
                }
                else
                {
                    Core.Log.MessageAll("Package root '{0}' already used by package '{1}'", standardPackageRoot, mainPackageId.ToString());
                }

                var absolutePackageRoot = Core.RelativePathUtilities.MakeRelativePathAbsoluteToWorkingDir(standardPackageRoot);
                Core.State.PackageRoots.Add(Core.DirectoryLocation.Get(absolutePackageRoot));
            }

            if (success)
            {
                xmlFile.Write();
                return true;
            }
            else
            {
                return false;
            }
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