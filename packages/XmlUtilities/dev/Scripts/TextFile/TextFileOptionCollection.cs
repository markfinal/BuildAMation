#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
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
