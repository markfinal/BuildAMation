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
namespace C.ObjCxx
{
namespace V2
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void Defaults(this C.V2.IObjectiveCxxOnlyCompilerOptions settings, Bam.Core.V2.Module module)
        {
        }
            public static void Empty(this C.V2.IObjectiveCxxOnlyCompilerOptions settings)
        {
        }
    }
}
    public class ObjectFile :
        C.V2.ObjectFile
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.Compiler = C.V2.DefaultToolchain.ObjectiveCxx_Compiler(this.BitDepth);
        }
    }
}
    /// <summary>
    /// ObjectiveC++ object file
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(IObjCxxCompilerTool))]
    public class ObjectFile :
        C.ObjectFile
    {}
}
