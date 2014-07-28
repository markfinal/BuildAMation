// <copyright file="TextFileOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    public partial class TextFileOptionCollection :
        Opus.Core.BaseOptionCollection
    {
        public
        TextFileOptionCollection(
            Opus.Core.DependencyNode owningNode) : base(owningNode)
        {}

        protected override void
        SetNodeSpecificData(
            Opus.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;
            var outputDirLoc = locationMap[TextFileModule.OutputDir] as Opus.Core.ScaffoldLocation;
            if (!outputDirLoc.IsValid)
            {
                outputDirLoc.SetReference(locationMap[Opus.Core.State.ModuleBuildDirLocationKey]);
            }

            var outputFileLoc = locationMap[TextFileModule.OutputFile] as Opus.Core.ScaffoldLocation;
            if (!outputFileLoc.IsValid)
            {
                outputFileLoc.SpecifyStub(outputDirLoc, "_sysconfigdata.py", Opus.Core.Location.EExists.WillExist);
            }

            base.SetNodeSpecificData(node);
        }

        #region implemented abstract members of BaseOptionCollection
        protected override void
        SetDefaultOptionValues(
            Opus.Core.DependencyNode owningNode)
        {}
        protected override void
        SetDelegates(
            Opus.Core.DependencyNode owningNode)
        {}
        #endregion
    }
}
