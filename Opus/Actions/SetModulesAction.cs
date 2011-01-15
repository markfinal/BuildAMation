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
                return "Set modules from the top level package to build";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.Modules = new Opus.Core.StringArray(arguments.Split(';'));
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