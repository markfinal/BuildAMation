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

[assembly: Bam.Core.RegisterAction(typeof(Bam.ShowDirectoryAction))]

namespace Bam
{
    [Core.TriggerAction]
    internal class ShowDirectoryAction :
        Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-showdirectory";
            }
        }

        public string Description
        {
            get
            {
                return "Show the BuildAMation directory";
            }
        }

        public bool
        Execute()
        {
            Core.Log.MessageAll(Core.State.ExecutableDirectory);

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