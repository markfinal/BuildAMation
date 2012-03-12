// <copyright file="AddPackageRootsAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.AddPackageRootsAction))]

namespace Opus
{
    [Core.PreambleAction]
    internal class AddPackageRootsAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-packageroots";
            }
        }

        public string Description
        {
            get
            {
                return "Add package roots (semi-colon separated)";
            }
        }

        private Core.StringArray PackageRoots
        {
            get;
            set;
        }

        public void AssignArguments(string arguments)
        {
            string[] roots = arguments.Split(System.IO.Path.PathSeparator);
            this.PackageRoots = new Opus.Core.StringArray(roots);
        }

        public bool Execute()
        {
            foreach (string packageRoot in this.PackageRoots)
            {
                string absolutePackageRoot = Core.RelativePathUtilities.MakeRelativePathAbsoluteToWorkingDir(packageRoot);

                Core.State.PackageRoots.Add(absolutePackageRoot);
            }

            return true;
        }
    }
}