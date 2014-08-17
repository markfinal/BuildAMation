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

[assembly: Bam.Core.RegisterAction(typeof(Bam.EnableDebugSymbolsInBuildsAction))]

namespace Bam
{
    [Core.PreambleAction]
    internal class EnableDebugSymbolsInBuildsAction :
        Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-debugsymbols";
            }
        }

        public string Description
        {
            get
            {
                return "Enable debug symbols in package builds";
            }
        }

        public bool
        Execute()
        {
            Core.State.CompileWithDebugSymbols = true;

            Core.Log.Detail("Package builds will use debug symbols");

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