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

[assembly: Bam.Core.RegisterAction(typeof(NativeBuilder.ForceBuildAction))]

namespace NativeBuilder
{
    [Bam.Core.PreambleAction]
    public sealed class ForceBuildAction :
        Bam.Core.IAction
    {
        public
        ForceBuildAction()
        {
            if (!Bam.Core.State.HasCategory("NativeBuilder"))
            {
                Bam.Core.State.AddCategory("NativeBuilder");
            }
            Bam.Core.State.Add<bool>("NativeBuilder", "ForceBuild", false);
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-forcebuild";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Force a build by not performing any dependency checks";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.State.Set("NativeBuilder", "ForceBuild", true);

            Bam.Core.Log.DebugMessage("Native builds are now forced");

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
