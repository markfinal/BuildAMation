// <copyright file="SetJobCount.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.SetJobCountAction))]

namespace Opus
{
    [Core.PreambleAction]
    internal class SetJobCountAction : Core.IActionWithArguments
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-j";
            }
        }

        public string Description
        {
            get
            {
                return "Specify the number of concurrent jobs in the build";
            }
        }

        public void AssignArguments(string arguments)
        {
            this.JobCount = System.Convert.ToInt32(arguments);
        }

        private int JobCount
        {
            get;
            set;
        }

        public bool Execute()
        {
            Core.State.JobCount = this.JobCount;

            Core.Log.Detail("Running with {0} jobs", Core.State.JobCount);

            return true;
        }
    }
}