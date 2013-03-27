// <copyright file="XmlWriterOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    public partial class XmlWriterOptionCollection: Opus.Core.BaseOptionCollection, IXmlOptions
    {
        public XmlWriterOptionCollection(Opus.Core.DependencyNode owningNode)
            : base(owningNode)
        {
        }

        #region implemented abstract members of BaseOptionCollection
        protected override void InitializeDefaults (Opus.Core.DependencyNode owningNode)
        {
        }
        protected override void SetDelegates (Opus.Core.DependencyNode owningNode)
        {
        }
        #endregion
    }
}
