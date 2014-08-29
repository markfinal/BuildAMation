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
namespace C
{
    public class ThirdPartyOptionCollection :
        Bam.Core.BaseOptionCollection
    {
        public
        ThirdPartyOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode owningNode)
        {}

        protected override void
        SetDelegates(
            Bam.Core.DependencyNode owningNode)
        {}

        // TODO: this needs to be updated for Location
        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var thirdPartyModule = node.Module as C.ThirdPartyModule;
            if (null != thirdPartyModule)
            {
                var packagePath = node.Package.Identifier.Path;
                var proxyPath = (node.Module as Bam.Core.BaseModule).ProxyPath;
                if (null != proxyPath)
                {
                    packagePath = proxyPath.Combine(node.Package.Identifier.Location).AbsolutePath;
                }

                thirdPartyModule.RegisterOutputFiles(this, node.Target, packagePath);
            }

            base.FinalizeOptions(node);
        }
    }
}
