// <copyright file="SetUndefineAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SetUndefineAction))]

namespace Opus
{
    [Core.PreambleAction]
    internal class SetUndefineAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-U";
            }
        }

        public string Description
        {
            get
            {
                return "Undefines any previously defined #define for package compilation (semi-colon separated)";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.Undefines = new Opus.Core.StringArray(arguments.Split(';'));
        }

        private Core.StringArray Undefines
        {
            get;
            set;
        }

        public bool Execute()
        {
            Core.State.PackageCompilationUndefines = this.Undefines;

            return true;
        }
    }
}