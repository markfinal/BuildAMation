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