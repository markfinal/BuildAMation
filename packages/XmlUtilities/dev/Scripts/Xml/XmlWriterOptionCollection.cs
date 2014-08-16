// <copyright file="XmlWriterOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    public partial class XmlWriterOptionCollection :
        Bam.Core.BaseOptionCollection,
        IXmlOptions
    {
        public
        XmlWriterOptionCollection(
            Bam.Core.DependencyNode owningNode) : base(owningNode)
        {}

        #region implemented abstract members of BaseOptionCollection
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode owningNode)
        {}
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode owningNode)
        {}
        #endregion
    }
}
