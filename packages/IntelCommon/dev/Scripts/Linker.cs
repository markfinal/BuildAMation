// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>IntelCommon package</summary>
// <author>Mark Final</author>
namespace IntelCommon
{
    public abstract class Linker : C.Linker, Opus.Core.ITool
    {
        public abstract string Executable(Opus.Core.Target target);

        protected override string StartLibraryList
        {
            get
            {
                return "--start-group";
            }
        }

        protected override string EndLibraryList
        {
            get
            {
                return "--end-group";
            }
        }
    }
}

