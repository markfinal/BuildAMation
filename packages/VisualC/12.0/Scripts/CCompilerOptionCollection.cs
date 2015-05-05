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
namespace VisualC
{
namespace V2
{
    public interface ICommonCompilerOptions
    {
        bool VC12Common
        {
            get;
            set;
        }
    }

    public interface ICOnlyCompilerOptions
    {
        int VC12COnly
        {
            get;
            set;
        }
    }

    public interface ICxxOnlyCompilerOptions
    {
        Bam.Core.EPlatform VC12CxxOnly
        {
            get;
            set;
        }
    }
}
    // V2
    public class CompilerSettings :
        Bam.Core.V2.Settings,
        C.V2.ICommonCompilerOptions,
        C.V2.ICOnlyCompilerOptions,
        VisualCCommon.V2.ICommonCompilerOptions,
        VisualCCommon.V2.ICOnlyCompilerOptions,
        VisualC.V2.ICommonCompilerOptions,
        VisualC.V2.ICOnlyCompilerOptions
    {
        public CompilerSettings()
        {
            // TODO: extension wrappers to set defaults?
            (this as C.V2.ICommonCompilerOptions).Bits = C.V2.EBit.SixtyFour;
        }

        C.V2.EBit C.V2.ICommonCompilerOptions.Bits
        {
            get;
            set;
        }

        bool C.V2.ICOnlyCompilerOptions.C99Specific
        {
            get;
            set;
        }

        bool VisualCCommon.V2.ICommonCompilerOptions.VCCommonCommon
        {
            get;
            set;
        }

        int VisualCCommon.V2.ICOnlyCompilerOptions.VCCommonCOnly
        {
            get;
            set;
        }

        bool V2.ICommonCompilerOptions.VC12Common
        {
            get;
            set;
        }

        int V2.ICOnlyCompilerOptions.VC12COnly
        {
            get;
            set;
        }
    }

    public sealed class CxxCompilerSettings :
        Bam.Core.V2.Settings,
        C.V2.ICommonCompilerOptions,
        C.V2.ICxxOnlyCompilerOptions,
        VisualCCommon.V2.ICommonCompilerOptions,
        VisualCCommon.V2.ICxxOnlyCompilerOptions,
        VisualC.V2.ICommonCompilerOptions,
        VisualC.V2.ICxxOnlyCompilerOptions
    {
        C.V2.EBit C.V2.ICommonCompilerOptions.Bits
        {
            get;
            set;
        }

        C.Cxx.EExceptionHandler C.V2.ICxxOnlyCompilerOptions.ExceptionHandler
        {
            get;
            set;
        }

        bool VisualCCommon.V2.ICommonCompilerOptions.VCCommonCommon
        {
            get;
            set;
        }

        string VisualCCommon.V2.ICxxOnlyCompilerOptions.VCCommonCxxOnly
        {
            get;
            set;
        }

        bool V2.ICommonCompilerOptions.VC12Common
        {
            get;
            set;
        }

        Bam.Core.EPlatform V2.ICxxOnlyCompilerOptions.VC12CxxOnly
        {
            get;
            set;
        }
    }

    public abstract class CompilerBase :
        C.V2.CompilerTool
    {
        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            if (typeof(C.Cxx.V2.ObjectFile).IsInstanceOfType(module))
            {
                var settings = new CxxCompilerSettings();
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else if (typeof(C.V2.ObjectFile).IsInstanceOfType(module))
            {
                var settings = new CompilerSettings();
                this.OverrideDefaultSettings(settings);
                return settings;
            }
            else
            {
                throw new Bam.Core.Exception("Could not determine type of module {0}", typeof(T).ToString());
            }
        }

        protected abstract void OverrideDefaultSettings(Bam.Core.V2.Settings settings);
    }

    public sealed class Compiler32 :
        CompilerBase
    {
        public Compiler32()
        {
            //this.EnvironmentVariables[PATH] =
        }

        public override string Executable
        {
            get
            {
                return @"cl.exe"; // 32-bit one
            }
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.Bits = C.V2.EBit.ThirtyTwo;
        }
    }

    public sealed class Compiler64 :
        CompilerBase
    {
        public override string Executable
        {
            get
            {
                return @"cl.exe"; // 64-bit one
            }
        }

        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.Bits = C.V2.EBit.SixtyFour;
        }
    }
    // -V2
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        VisualCCommon.CCompilerOptionCollection
    {
        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
