#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using C.DefaultSettings;
using Mingw.DefaultSettings;
namespace Mingw
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void Defaults(this IArchiverSettings settings, Bam.Core.Module module)
        {
            settings.Ranlib = true;
            settings.DoNotWarnIfLibraryCreated = true;
            settings.Command = MingwCommon.EArchiverCommand.Replace;
        }
    }
}
    [Bam.Core.SettingsExtensions(typeof(Mingw.DefaultSettings.DefaultSettingsExtensions))]
    public interface IArchiverSettings : Bam.Core.ISettingsBase
    {
        bool Ranlib
        {
            get;
            set;
        }

        bool DoNotWarnIfLibraryCreated
        {
            get;
            set;
        }

        MingwCommon.EArchiverCommand Command
        {
            get;
            set;
        }
    }

    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this C.ICommonArchiverSettings options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            //var staticLibrary = module as C.StaticLibrary;
            switch (options.OutputType)
            {
                case C.EArchiverOutput.StaticLibrary:
                    commandLine.Add(module.GeneratedPaths[C.StaticLibrary.Key].ToString());
                    break;

                default:
                    throw new Bam.Core.Exception("Unsupported output type");
            }
        }

        public static void
        Convert(
            this IArchiverSettings options,
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (options.Ranlib)
            {
                commandLine.Add("-s");
            }
            if (options.DoNotWarnIfLibraryCreated)
            {
                commandLine.Add("-c");
            }
            switch (options.Command)
            {
                case MingwCommon.EArchiverCommand.Replace:
                    commandLine.Add("-r");
                    break;

                default:
                    throw new Bam.Core.Exception("No such archiver command");
            }
        }
    }

    public class ArchiverSettings :
        C.SettingsBase,
        C.ICommonArchiverSettings,
        IArchiverSettings,
        CommandLineProcessor.IConvertToCommandLine
    {
        public ArchiverSettings(Bam.Core.Module module)
        {
#if true
            this.InitializeAllInterfaces(module, false, true);
#else
            (this as C.ICommonArchiverSettings).Defaults(module);
            (this as IArchiverSettings).Defaults(module);
#endif
        }

        C.EArchiverOutput C.ICommonArchiverSettings.OutputType
        {
            get;
            set;
        }

        bool IArchiverSettings.Ranlib
        {
            get;
            set;
        }

        bool IArchiverSettings.DoNotWarnIfLibraryCreated
        {
            get;
            set;
        }

        MingwCommon.EArchiverCommand IArchiverSettings.Command
        {
            get;
            set;
        }

        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            (this as IArchiverSettings).Convert(module, commandLine);
            (this as C.ICommonArchiverSettings).Convert(module, commandLine);
        }
    }

    [C.RegisterLibrarian("Mingw", Bam.Core.EPlatform.Windows, C.EBit.ThirtyTwo)]
    public sealed class Librarian :
        C.LibrarianTool
    {
        public Librarian()
        {
            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("ArchiverPath", Bam.Core.TokenizedString.Create(@"$(InstallPath)\bin\ar.exe", this));
            this.Macros.Add("libprefix", "lib");
            this.Macros.Add("libext", ".a");

            this.InheritedEnvironmentVariables.Add("TEMP");
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return this.Macros["ArchiverPath"];
            }
        }

        public override Bam.Core.Settings CreateDefaultSettings<T>(T module)
        {
            var settings = new ArchiverSettings(module);
            return settings;
        }
    }
}
