// <copyright file="OutputFileFlags.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
#if false
    public sealed class OutputFileFlags : Opus.Core.FlagsBase
    {
        public static readonly OutputFileFlags ObjectFile = new OutputFileFlags("ObjectFile");
        public static readonly OutputFileFlags PreprocessedFile = new OutputFileFlags("PreprocessedFile");
        public static readonly OutputFileFlags StaticLibrary = new OutputFileFlags("StaticLibrary");
        public static readonly OutputFileFlags Executable = new OutputFileFlags("Executable");
        public static readonly OutputFileFlags StaticImportLibrary = new OutputFileFlags("StaticImportLibrary");
        public static readonly OutputFileFlags MapFile = new OutputFileFlags("MapFile");

        // VisualC specific (easier to include here)
        public static readonly OutputFileFlags CompilerProgramDatabase = new OutputFileFlags("CompilerProgramDatabase");
        public static readonly OutputFileFlags LinkerProgramDatabase = new OutputFileFlags("LinkerProgramDatabase");

        private OutputFileFlags(string name)
            : base(name)
        {
        }
    }
#endif

    [System.Flags]
    public enum OutputFileFlags
    {
        ObjectFile = (1 << 0),
        PreprocessedFile = (1 << 1),
        StaticLibrary = (1 << 2),
        Executable = (1 << 3),
        StaticImportLibrary = (1 << 4),
        MapFile = (1 << 5),

        CompilerProgramDatabase = (1 << 6),
        LinkerProgramDatabase = (1 << 7)
    }
}
