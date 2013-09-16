// <copyright file="IWriteableNode.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public interface IWriteableNode
    {
        void Write(System.IO.TextWriter writer);
    }
}
