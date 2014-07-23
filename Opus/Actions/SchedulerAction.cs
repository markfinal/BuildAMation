// <copyright file="SchedulerAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SchedulerAction))]

namespace Opus
{
    [Core.PreambleAction]
    internal class SchedulerAction :
        Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-scheduler";
            }
        }

        public string Description
        {
            get
            {
                return "Provide the typename for the build scheduler";
            }
        }

        void
        Opus.Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.SchedulerType = arguments;
        }

        private string SchedulerType
        {
            get;
            set;
        }

        public bool
        Execute()
        {
            Core.State.SchedulerType = this.SchedulerType;
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