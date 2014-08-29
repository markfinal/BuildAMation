#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License

[assembly: Bam.Core.RegisterAction(typeof(Bam.BuildAction))]

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