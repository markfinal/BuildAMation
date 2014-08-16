// <copyright file="TextFileOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    public partial class TextFileOptionCollection :
        Bam.Core.BaseOptionCollection
    {
        public
        TextFileOptionCollection(
            Bam.Core.DependencyNode owningNode) : base(owningNode)
        {}

        protected override void
        SetNodeSpecificData(
            Bam.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;
            var outputDirLoc = locationMap[TextFileModule.OutputDir] as Bam.Core.ScaffoldLocation;
            if (!outputDirLoc.IsValid)
            {
                outputDirLoc.SetReference(locationMap[Bam.Core.State.ModuleBuildDirLocationKey]);
            }

            var outputFileLoc = locationMap[TextFileModule.OutputFile] as Bam.Core.ScaffoldLocation;
            if (!outputFileLoc.IsValid)
            {
                outputFileLoc.SpecifyStub(outputDirLoc, "_sysconfigdata.py", Bam.Core.Location.EExists.WillExist);
            }

            base.SetNodeSpecificData(node);
        }

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
