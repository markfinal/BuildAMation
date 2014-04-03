// <copyright file="SetVerbosityLevelAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SetVerbosityLevelAction))]

namespace Opus
{
    [Core.ImmediateAction]
    internal class SetVerbosityLevelAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-verbosity";
            }
        }

        public string Description
        {
            get
            {
                return "Set the verbosity level between 0 and 3";
            }
        }

        void Opus.Core.IActionWithArguments.AssignArguments(string arguments)
        {
            int level = System.Convert.ToInt32(arguments);
            if (!System.Enum.IsDefined(typeof(Core.EVerboseLevel), level))
            {
                throw new Core.Exception("Verbosity level {0} is not defined", level);
            }

            Core.State.VerbosityLevel = (Core.EVerboseLevel)level;
        }

        private Core.EVerboseLevel Verbosity
        {
            get;
            set;
        }

        public bool Execute()
        {
            Core.Log.DebugMessage("Verbosity level set to '{0}'", Core.State.VerbosityLevel.ToString());

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