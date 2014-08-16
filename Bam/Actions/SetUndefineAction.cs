// <copyright file="SetUndefineAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Bam.Core.RegisterAction(typeof(Bam.SetUndefineAction))]

namespace Bam
{
    [Core.PreambleAction]
    internal class SetUndefineAction :
        Core.IActionWithArguments
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
                return "Undefines any previously defined #define for package compilation (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.Undefines = new Core.StringArray(arguments.Split(System.IO.Path.PathSeparator));
        }

        private Core.StringArray Undefines
        {
            get;
            set;
        }

        public bool
        Execute()
        {
            Core.State.PackageCompilationUndefines = this.Undefines;

            return true;
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