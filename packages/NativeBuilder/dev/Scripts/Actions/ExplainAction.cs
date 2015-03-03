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

[assembly: Bam.Core.RegisterAction(typeof(NativeBuilder.ExplainAction))]

namespace NativeBuilder
{
    [Bam.Core.PreambleAction]
    public sealed class ExplainAction :
        Bam.Core.IAction
    {
        public
        ExplainAction()
        {
            if (!Bam.Core.State.HasCategory("NativeBuilder"))
            {
                Bam.Core.State.AddCategory("NativeBuilder");
            }
            Bam.Core.State.Add<bool>("NativeBuilder", "Explain", false);
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-explain";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Explain why builds occur due to dependency checking";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.State.Set("NativeBuilder", "Explain", true);

            Bam.Core.Log.DebugMessage("Explained builds are enabled");

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
