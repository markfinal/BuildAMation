// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public abstract class Linker : C.Linker, Opus.Core.ITool, Opus.Core.IToolSupportsResponseFile
    {
        public abstract string Executable(Opus.Core.Target target);

        protected override string StartLibraryList
        {
            get
            {
                return "-Wl,--start-group";
            }
        }

        protected override string EndLibraryList
        {
            get
            {
                return "-Wl,--end-group";
            }
        }
 
        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }
   }
}

