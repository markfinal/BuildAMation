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

[assembly: Bam.Core.RegisterAction(typeof(Bam.SchedulerAction))]

namespace Bam
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
        Core.IActionWithArguments.AssignArguments(
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