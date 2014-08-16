// <copyright file="BuildAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.BuildAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class BuildAction :
        Core.IAction
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

        public bool
        Execute()
        {
            Core.Log.DebugMessage("Builder is {0}", Core.State.BuilderName);
            var compiledSuccessfully = Core.PackageUtilities.CompilePackageAssembly();
            if (compiledSuccessfully)
            {
                Core.PackageUtilities.LoadPackageAssembly();

                var additionalArgumentProfile = new Core.TimeProfile(Core.ETimingProfiles.AdditionalArgumentProcessing);
                additionalArgumentProfile.StartProfile();
                var fatal = true;
                Core.PackageUtilities.ProcessLazyArguments(fatal);
                Core.PackageUtilities.HandleUnprocessedArguments(fatal);
                Core.State.ShowTimingStatistics = true;
                additionalArgumentProfile.StopProfile();

                if (!Core.PackageUtilities.ExecutePackageAssembly())
                {
                    System.Environment.ExitCode = -6;
                }
            }
            else
            {
                System.Environment.ExitCode = -5;
            }

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