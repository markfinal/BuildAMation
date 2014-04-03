// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>LLVMGcc package</summary>
// <author>Mark Final</author>
namespace LLVMGcc
{
    public sealed partial class LinkerOptionCollection : GccCommon.LinkerOptionCollection
    {
        public LinkerOptionCollection()
            : base()
        {}

        public LinkerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}