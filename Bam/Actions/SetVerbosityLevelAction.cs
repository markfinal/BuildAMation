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

[assembly: Bam.Core.RegisterAction(typeof(Bam.SetVerbosityLevelAction))]

namespace Bam
{
    [Core.ImmediateAction]
    internal class SetVerbosityLevelAction :
        Core.IActionWithArguments
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

        void
        Core.IActionWithArguments.AssignArguments(
            string arguments)
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

        public bool
        Execute()
        {
            Core.Log.DebugMessage("Verbosity level set to '{0}'", Core.State.VerbosityLevel.ToString());

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