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
namespace Mingw
{
    // V2
    public sealed class CompilerSettings
        : Bam.Core.V2.Settings
    {
        public CompilerSettings()
        {
            this.Bits = EBit.SixtyFour;
        }

        // TODO: needs to be in an interface, and in a super class, but to demonstrate...
        public enum EBit
        {
            ThirtyTwo = 32,
            SixtyFour = 64
        }

        public EBit Bits
        {
            get;
            set;
        }
    }

    public sealed class Compiler32 :
        C.V2.CompilerTool
    {
        public Compiler32()
        {
            //this.EnvironmentVariables[PATH] =
        }

        public override Bam.Core.V2.Settings CreateDefaultSettings()
        {
            var settings = new CompilerSettings();
            settings.Bits = CompilerSettings.EBit.ThirtyTwo;
            return settings;
        }

        public override string Executable
        {
            get
            {
                return @"C:\MinGW\bin\mingw32-gcc-4.8.1";
            }
        }
    }

    public sealed class Compiler64 :
        C.V2.CompilerTool
    {
        public override Bam.Core.V2.Settings CreateDefaultSettings()
        {
            var settings = new CompilerSettings();
            settings.Bits = CompilerSettings.EBit.SixtyFour;
            return settings;
        }

        public override string Executable
        {
            get
            {
                return @"C:\MinGW\bin\mingw32-gcc-4.8.1";
            }
        }
    }
    // -V2
    public partial class CCompilerOptionCollection :
        MingwCommon.CCompilerOptionCollection,
        ICCompilerOptions
    {
        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // requires gcc 4.0, and only works on ELFs, but doesn't seem to do any harm
            (this as ICCompilerOptions).Visibility = EVisibility.Hidden;
        }
    }
}
