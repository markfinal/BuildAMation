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
                return "Add package roots (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        private Core.StringArray PackageRoots
        {
            get;
            set;
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            var roots = arguments.Split(System.IO.Path.PathSeparator);
            this.PackageRoots = new Opus.Core.StringArray(roots);
        }

        public bool Execute()
        {
            foreach (var packageRoot in this.PackageRoots)
            {
                var absolutePackageRoot = Core.RelativePathUtilities.MakeRelativePathAbsoluteToWorkingDir(packageRoot);

                Core.State.PackageRoots.Add(Core.DirectoryLocation.Get(absolutePackageRoot));
            }

            return true;
        }

        #region ICloneable Members

        object System.ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}