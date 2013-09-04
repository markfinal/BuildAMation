// <copyright file="IWriteableNode.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public interface IWriteableNode
    {
        void Write(System.IO.TextWriter writer);
    }
}
