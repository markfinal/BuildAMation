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

[assembly: Bam.Core.RegisterAction(typeof(CommandLineProcessor.NoResponseFileAction))]

namespace CommandLineProcessor
{
    [Bam.Core.PreambleAction]
    public sealed class NoResponseFileAction :
        Bam.Core.IAction
    {
        public
        NoResponseFileAction()
        {
            if (!Bam.Core.State.HasCategory("Build"))
            {
                Bam.Core.State.AddCategory("Build");
            }
            Bam.Core.State.Add<bool>("Build", "DisableResponseFiles", false);
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-noresponsefiles";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Disable use of response files";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.State.Set("Build", "DisableResponseFiles", true);

            Bam.Core.Log.DebugMessage("Response files have been disabled");

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
