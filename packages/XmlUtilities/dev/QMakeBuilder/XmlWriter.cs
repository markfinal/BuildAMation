// <copyright file="XmlWriter.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(XmlUtilities.XmlModule moduleToBuild, out bool success)
        {
            Opus.Core.Log.MessageAll("Generating XML files in QMake is not yet supported");
            success = true;
            return null;
        }
    }
}
