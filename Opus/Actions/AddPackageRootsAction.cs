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
        private Core.StringArray packageRoots = new Core.StringArray();

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
                return "Add package roots";
            }
        }

        public void AssignArguments(string arguments)
        {
            packageRoots.AddRange(arguments.Split(';'));
        }

        public bool Execute()
        {
            Core.State.PackageRoots.AddRange(packageRoots.ToArray());

            return true;
        }
    }
}