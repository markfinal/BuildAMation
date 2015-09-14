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
using C.V2.DefaultSettings;
namespace VisualC
{
    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this C.V2.ICommonArchiverOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            //var libraryFile = module as C.V2.StaticLibrary;
            switch (options.OutputType)
            {
                case C.EArchiverOutput.StaticLibrary:
                    commandLine.Add(System.String.Format("-OUT:{0}", module.GeneratedPaths[C.V2.StaticLibrary.Key].ToString()));
                    break;
            }
        }

        public static void
        Convert(
            this V2.ICommonArchiverOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (options.NoLogo.GetValueOrDefault())
            {
                commandLine.Add("-NOLOGO");
            }
        }
    }

    public static partial class VSSolutionImplementation
    {
        public static void
        Convert(
            this C.V2.ICommonArchiverOptions options,
            Bam.Core.V2.Module module,
            VSSolutionBuilder.V2.VSSettingsGroup settingsGroup,
            string condition)
        {
            switch (options.OutputType)
            {
                case C.EArchiverOutput.StaticLibrary:
                    {
                        var outPath = module.GeneratedPaths[C.V2.StaticLibrary.Key].ToString();
                        settingsGroup.AddSetting("OutputFile", System.String.Format("$(OutDir)\\{0}", System.IO.Path.GetFileName(outPath)), condition);
                    }
                    break;

                default:
                    throw new Bam.Core.Exception("Unknown output type, {0}", options.OutputType.ToString());
            }
        }

        public static void
        Convert(
            this V2.ICommonArchiverOptions options,
            Bam.Core.V2.Module module,
            VSSolutionBuilder.V2.VSSettingsGroup settingsGroup,
            string condition)
        {
            if (options.NoLogo.GetValueOrDefault(false))
            {
                settingsGroup.AddSetting("SuppressStartupBanner", options.NoLogo.Value, condition);
            }
        }
    }

namespace V2
{
namespace DefaultSettings
{
    static partial class DefaultSettingsExtensions
    {
        public static void Defaults(this ICommonArchiverOptions settings, Bam.Core.V2.Module module)
        {
            settings.NoLogo = true;
        }
    }
}
    [Bam.Core.V2.SettingsExtensions(typeof(VisualC.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICommonArchiverOptions : Bam.Core.V2.ISettingsBase
    {
        bool? NoLogo
        {
            get;
            set;
        }
    }

    public class ArchiverSettings :
        C.V2.SettingsBase,
        C.V2.ICommonArchiverOptions,
        ICommonArchiverOptions,
        CommandLineProcessor.V2.IConvertToCommandLine,
        VisualStudioProcessor.V2.IConvertToProject
    {
        public ArchiverSettings(Bam.Core.V2.Module module)
        {
#if true
            this.InitializeAllInterfaces(module, false, true);
#else
            (this as C.V2.ICommonArchiverOptions).Defaults(module);
#endif
        }

        C.EArchiverOutput C.V2.ICommonArchiverOptions.OutputType
        {
            get;
            set;
        }

        bool? ICommonArchiverOptions.NoLogo
        {
            get;
            set;
        }

        void
        CommandLineProcessor.V2.IConvertToCommandLine.Convert(
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            (this as C.V2.ICommonArchiverOptions).Convert(module, commandLine);
            (this as ICommonArchiverOptions).Convert(module, commandLine);
        }

        void
        VisualStudioProcessor.V2.IConvertToProject.Convert(
            Bam.Core.V2.Module module,
            VSSolutionBuilder.V2.VSSettingsGroup settings,
            string condition)
        {
            (this as C.V2.ICommonArchiverOptions).Convert(module, settings, condition);
        }
    }

    [C.V2.RegisterArchiver("VisualC", Bam.Core.EPlatform.Windows, C.V2.EBit.ThirtyTwo)]
    [C.V2.RegisterArchiver("VisualC", Bam.Core.EPlatform.Windows, C.V2.EBit.SixtyFour)]
    public sealed class Librarian :
        C.V2.LibrarianTool
    {
        public Librarian()
        {
            this.Macros.Add("InstallPath", Configure.InstallPath);
            this.Macros.Add("ArchiverPath", Bam.Core.V2.TokenizedString.Create(@"$(InstallPath)\VC\bin\lib.exe", this));
            this.Macros.Add("libprefix", string.Empty);
            this.Macros.Add("libext", ".lib");
        }

        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            var settings = new ArchiverSettings(module);
            return settings;
        }

        public override Bam.Core.V2.TokenizedString Executable
        {
            get
            {
                return this.Macros["ArchiverPath"];
            }
        }
    }
}
}
