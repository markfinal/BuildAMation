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
#endregion

[assembly: Bam.Core.RegisterAction(typeof(Bam.SetDefineAction))]

namespace Bam
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
                return "Set defines on the BuildAMation package compilation step (separated by " + System.IO.Path.PathSeparator + ")";
            }
        }

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
        {
            this.Defines = new Core.StringArray(arguments.Split(System.IO.Path.PathSeparator));
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