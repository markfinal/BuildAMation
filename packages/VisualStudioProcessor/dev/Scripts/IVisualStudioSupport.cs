// <copyright file="IVisualStudioSupport.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualStudioProcessor package</summary>
// <author>Mark Final</author>
namespace VisualStudioProcessor
{
    public interface IVisualStudioSupport
    {
        ToolAttributeDictionary ToVisualStudioProjectAttributes(Opus.Core.Target target);
    }
}
