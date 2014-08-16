// <copyright file="SetDefineAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SetDefineAction))]

namespace Opus
{
    [Core.PreambleAction]
    internal class SetDefineAction :
        Core.IActionWithArguments
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
                return "Set defines on the Opus package compilation step (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Opus.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.Defines = new Opus.Core.StringArray(arguments.Split(System.IO.Path.PathSeparator));
        }

        private Core.StringArray Defines
        {
            get;
            set;
        }

        public bool
        Execute()
        {
            Core.State.PackageCompilationDefines = this.Defines;

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