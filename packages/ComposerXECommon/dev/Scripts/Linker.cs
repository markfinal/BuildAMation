// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace ComposerXECommon
{
    public abstract class Linker : C.Linker, Opus.Core.ITool
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
    }
}

