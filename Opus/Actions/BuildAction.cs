// <copyright file="BuildAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.BuildAction))]

namespace Opus
{
    [Core.TriggerAction]
    internal class BuildAction : Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-build";
            }
        }

        public string Description
        {
            get
            {
                return "Build (default trigger)";
            }
        }

        public bool Execute()
        {
            Core.Log.DebugMessage("Builder is {0}", Core.State.BuilderName);
            if (!Core.PackageUtilities.CompilePackageIntoAssembly())
            {
                System.Environment.ExitCode = -3;
            }
            else
            {
                Core.PackageUtilities.LoadPackageAssembly();
                Core.PackageUtilities.ProcessLazyArguments();
                Core.PackageUtilities.HandleUnprocessedArguments();
                Core.State.ShowTimingStatistics = true;
                if (!Core.PackageUtilities.ExecutePackageAssembly())
                {
                    System.Environment.ExitCode = -3;
                }
            }

            return true;
        }
    }
}