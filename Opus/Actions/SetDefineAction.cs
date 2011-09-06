// <copyright file="SetDefineAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SetDefineAction))]

namespace Opus
{
    [Core.PreambleAction]
    internal class SetDefineAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-D";
            }
        }

        public string Description
        {
            get
            {
                return "Set defines on the Opus package compilation step (semi-colon separated)";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.Defines = new Opus.Core.StringArray(arguments.Split(';'));
        }

        private Core.StringArray Defines
        {
            get;
            set;
        }

        public bool Execute()
        {
            Core.State.PackageCompilationDefines = this.Defines;

            return true;
        }
    }
}