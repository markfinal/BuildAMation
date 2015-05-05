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
using C.V2.DefaultSettings;
namespace Mingw
{
    public static class NativeImplementation
    {
        public static void Convert(this C.V2.ICommonCompilerOptions options, Bam.Core.V2.Module module)
        {
            var commandLine = module.CommandLine;
            if (options.Bits == C.V2.EBit.ThirtyTwo)
            {
                commandLine.Add("-m32");
            }
            else
            {
                commandLine.Add("-m64");
            }
        }

        public static void Convert(this C.V2.ICOnlyCompilerOptions options, Bam.Core.V2.Module module)
        {
        }

        public static void Convert(this MingwCommon.V2.ICommonCompilerOptions options, Bam.Core.V2.Module module)
        {
        }

        public static void Convert(this MingwCommon.V2.ICOnlyCompilerOptions options, Bam.Core.V2.Module module)
        {
        }

        public static void Convert(this Mingw.V2.ICommonCompilerOptions options, Bam.Core.V2.Module module)
        {
        }

        public static void Convert(this Mingw.V2.ICOnlyCompilerOptions options, Bam.Core.V2.Module module)
        {
        }
    }

namespace V2
{
    public class CompilerSettings :
        Bam.Core.V2.Settings,
        C.V2.ICommonCompilerOptions,
        C.V2.ICOnlyCompilerOptions,
        MingwCommon.V2.ICommonCompilerOptions,
        MingwCommon.V2.ICOnlyCompilerOptions,
        Mingw.V2.ICommonCompilerOptions,
        Mingw.V2.ICOnlyCompilerOptions,
        CommandLineProcessor.V2.IConvertToCommandLine
    {
        public CompilerSettings()
        {
            (this as C.V2.ICommonCompilerOptions).Defaults();
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

        bool MingwCommon.V2.ICommonCompilerOptions.MCommonCommon
        {
            get;
            set;
        }

        bool MingwCommon.V2.ICOnlyCompilerOptions.MCommonCOnly
        {
            get;
            set;
        }

        bool ICommonCompilerOptions.M48Common
        {
            get;
            set;
        }

        bool ICOnlyCompilerOptions.M48COnly
        {
            get;
            set;
        }

        void CommandLineProcessor.V2.IConvertToCommandLine.Convert(Bam.Core.V2.Module module)
        {
            (this as C.V2.ICommonCompilerOptions).Convert(module);
            (this as C.V2.ICOnlyCompilerOptions).Convert(module);
            (this as MingwCommon.V2.ICommonCompilerOptions).Convert(module);
            (this as MingwCommon.V2.ICOnlyCompilerOptions).Convert(module);
            (this as Mingw.V2.ICommonCompilerOptions).Convert(module);
            (this as Mingw.V2.ICOnlyCompilerOptions).Convert(module);
        }
    }

    public sealed class CxxCompilerSettings :
        Bam.Core.V2.Settings,
        C.V2.ICommonCompilerOptions,
        C.V2.ICxxOnlyCompilerOptions,
        MingwCommon.V2.ICommonCompilerOptions,
        MingwCommon.V2.ICxxOnlyCompilerOptions,
        Mingw.V2.ICommonCompilerOptions,
        Mingw.V2.ICxxOnlyCompilerOptions
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

        bool MingwCommon.V2.ICommonCompilerOptions.MCommonCommon
        {
            get;
            set;
        }

        bool MingwCommon.V2.ICxxOnlyCompilerOptions.MCommonCxxOnly
        {
            get;
            set;
        }

        bool ICommonCompilerOptions.M48Common
        {
            get;
            set;
        }

        int ICxxOnlyCompilerOptions.M48CxxOnly
        {
            get;
            set;
        }
    }

    public abstract class CompilerBase :
        C.V2.CompilerTool
    {
        public CompilerBase()
        {
            this.InheritedEnvironmentVariables.Add("TEMP");
        }

        public override string Executable
        {
            get
            {
                return @"C:\MinGW\bin\mingw32-gcc-4.8.1.exe";
            }
        }

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
        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.Bits = C.V2.EBit.ThirtyTwo;
        }
    }

    public sealed class Compiler64 :
        CompilerBase
    {
        protected override void OverrideDefaultSettings(Bam.Core.V2.Settings settings)
        {
            var cSettings = settings as C.V2.ICommonCompilerOptions;
            cSettings.Bits = C.V2.EBit.SixtyFour;
        }
    }
}
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
