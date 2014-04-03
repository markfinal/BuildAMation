// <copyright file="IVisualStudioDelegate.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualStudioProcessor package</summary>
// <author>Mark Final</author>
namespace VisualStudioProcessor
{
    public interface IVisualStudioDelegate
    {
        Delegate VisualStudioProjectDelegate
        {
            get;
            set;
        }
    }
}