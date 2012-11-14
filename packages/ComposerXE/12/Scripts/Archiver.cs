// <copyright file="Archiver.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXE package</summary>
// <author>Mark Final</author>
namespace ComposerXE
{
    // NEW STYLE
#if true
#else
    public sealed class Archiver : ComposerXECommon.Archiver, Opus.Core.IToolSupportsResponseFile
    {
        public Archiver(Opus.Core.Target target)
            : base(target)
        {
        }

        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }
    }
#endif
}