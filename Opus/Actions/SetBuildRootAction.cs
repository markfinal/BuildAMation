// <copyright file="SetBuildRootAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SetBuildRootAction))]

namespace Opus
{
    [Core.PreambleAction]
    internal class SetBuildRootAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-buildroot";
            }
        }

        public string Description
        {
            get
            {
                return "Specify the path of the build root (absolute or relative)";
            }
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            this.BuildRootDirectoryName = arguments;
        }

        private string BuildRootDirectoryName
        {
            get;
            set;
        }

        public bool Execute()
        {
            Core.State.BuildRoot = this.BuildRootDirectoryName;

            // the build root is refetched from the State here because if it was relative initially, the act of setting it above changes it to an absolute path
            if (!System.IO.Directory.Exists(Core.State.BuildRoot))
            {
                System.IO.Directory.CreateDirectory(Core.State.BuildRoot);
            }

            return true;
        }
    }
}