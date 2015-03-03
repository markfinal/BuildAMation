#region License
// Copyright 2010-2015 Mark Final
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

[assembly: Bam.Core.RegisterAction(typeof(Bam.SetJobCountAction))]

namespace Bam
{
    [Core.PreambleAction]
    internal class SetJobCountAction :
    Core.IActionWithArguments
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
                return "Specify the number of concurrent jobs in the build (0 for all hardware threads)";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.JobCount = System.Convert.ToInt32(arguments);
            if (0 == this.JobCount)
            {
                this.JobCount = System.Environment.ProcessorCount;
            }
        }

        private int JobCount
        {
            get;
            set;
        }

        public bool
        Execute()
        {
            Core.State.JobCount = this.JobCount;

            Core.Log.Detail("Running with {0} jobs", Core.State.JobCount);

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