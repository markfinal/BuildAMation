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

[assembly: Bam.Core.RegisterAction(typeof(XcodeBuilder.WarmXcodeSchemeCacheAction))]

namespace XcodeBuilder
{
    [Bam.Core.PreambleAction]
    public sealed class WarmXcodeSchemeCacheAction :
        Bam.Core.IAction
    {
        public
        WarmXcodeSchemeCacheAction()
        {
            if (!Bam.Core.State.HasCategory("XcodeBuilder"))
            {
                Bam.Core.State.AddCategory("XcodeBuilder");
            }
            Bam.Core.State.Add<bool>("XcodeBuilder", "WarmSchemeCache", false);
        }

        string Bam.Core.IAction.CommandLineSwitch
        {
            get
            {
                return "-warmschemecache";
            }
        }

        string Bam.Core.IAction.Description
        {
            get
            {
                return "Warms Xcode project scheme caches, in order to use xcodebuild on a container workspace without loading it into the Xcode UI";
            }
        }

        bool
        Bam.Core.IAction.Execute()
        {
            Bam.Core.State.Set("XcodeBuilder", "WarmSchemeCache", true);

            Bam.Core.Log.DebugMessage("Xcode project scheme caches will be warmed at the end of the build");

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
