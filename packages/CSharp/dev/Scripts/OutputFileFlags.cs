// <copyright file="OutputFileFlags.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    public sealed class OutputFileFlags : Opus.Core.FlagsBase
    {
        public static readonly OutputFileFlags AssemblyFile = new OutputFileFlags("AssemblyFile");
        public static readonly OutputFileFlags ProgramDatabaseFile = new OutputFileFlags("ProgramDatabaseFile");

        private OutputFileFlags(string name)
            : base(name)
        {
        }
    }
}
