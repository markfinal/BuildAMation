// <copyright file="OutputFileFlags.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
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

#if false
        private OutputFileFlags(string name, int value)
            : base(name, value)
        {
        }

        private OutputFileFlags(Opus.Core.FlagsBase flags)
            : base(flags.ToString(), flags.InternalValue)
        {
        }
#endif
    }
}
