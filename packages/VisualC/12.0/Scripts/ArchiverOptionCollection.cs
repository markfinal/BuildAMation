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
namespace VisualC
{
    public static partial class NativeImplementation
    {
        public static void Convert(this C.V2.ICommonArchiverOptions options, Bam.Core.V2.Module module)
        {
            var commandLine = module.CommandLine;
            var libraryFile = module as C.V2.StaticLibrary;
            switch (options.OutputType)
            {
                case C.EArchiverOutput.StaticLibrary:
                    commandLine.Add(System.String.Format("-OUT:{0}", module.GeneratedPaths[C.V2.StaticLibrary.Key].ToString()));
                    break;
            }
        }

        public static void Convert(this V2.ICommonArchiverOptions options, Bam.Core.V2.Module module)
        {
        }
    }

namespace V2
{
    public interface ICommonArchiverOptions
    {
    }

    public class ArchiverSettings :
        Bam.Core.V2.Settings,
        C.V2.ICommonArchiverOptions,
        ICommonArchiverOptions,
        CommandLineProcessor.V2.IConvertToCommandLine
    {
        public ArchiverSettings()
        {
            (this as C.V2.ICommonArchiverOptions).Defaults();
        }

        C.EArchiverOutput C.V2.ICommonArchiverOptions.OutputType
        {
            get;
            set;
        }

        void CommandLineProcessor.V2.IConvertToCommandLine.Convert(Bam.Core.V2.Module module)
        {
            (this as C.V2.ICommonArchiverOptions).Convert(module);
            (this as ICommonArchiverOptions).Convert(module);
        }
    }

    public sealed class Librarian :
        C.V2.LibrarianTool
    {
        public Librarian()
        {
            this.Macros.Add("InstallPath", new Bam.Core.V2.TokenizedString(@"C:\Program Files (x86)\Microsoft Visual Studio 12.0", null));
        }

        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            var settings = new ArchiverSettings();
            return settings;
        }

        public override string Executable
        {
            get
            {
                return @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC\bin\lib.exe";
            }
        }
    }
}

    public sealed partial class ArchiverOptionCollection :
        VisualCCommon.ArchiverOptionCollection
    {
        public
        ArchiverOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
