// <copyright file="SetModulesAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SetModulesAction))]

namespace Opus
{
    [Core.PreambleAction]
    internal class SetModulesAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-modules";
            }
        }

        public string Description
        {
            get
            {
                return "Set modules from the top level package to build (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            this.Modules = new Core.StringArray(arguments.Split(System.IO.Path.PathSeparator));
        }

        private Core.StringArray Modules
        {
            get;
            set;
        }

        public bool Execute()
        {
            Core.State.BuildModules = this.Modules;

            return true;
        }
    }
}