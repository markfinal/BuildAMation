#region License
// Copyright 2010-2015 Mark Final
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

[assembly: Bam.Core.RegisterToolset("Publish", typeof(Publisher.Toolset))]

namespace Publisher
{
namespace V2
{
    public interface IPackagePolicy
    {
        void
        Package(
            Package sender,
            Bam.Core.V2.TokenizedString packageRoot,
            System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.V2.TokenizedString, string> packageObjects);
    }

    public abstract class Package :
        Bam.Core.V2.Module
    {
        private System.Collections.Generic.List<Bam.Core.V2.Module> dependents = new System.Collections.Generic.List<Bam.Core.V2.Module>();
        private System.Collections.Generic.Dictionary<Bam.Core.V2.TokenizedString, string> paths = new System.Collections.Generic.Dictionary<Bam.Core.V2.TokenizedString, string>();
        private IPackagePolicy Policy = null;

        public void
        Include<DependentModule>(
            Bam.Core.V2.FileKey key,
            string subdir) where DependentModule : Bam.Core.V2.Module, new()
        {
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.Requires(dependent);
            this.dependents.Add(dependent);

            this.paths[dependent.GeneratedPaths[key]] = subdir;
        }

        public override void Evaluate()
        {
            // TODO: should this do at least a timestamp check?
            // do nothing
        }

        protected override void ExecuteInternal()
        {
            var paths = new System.Collections.ObjectModel.ReadOnlyDictionary<Bam.Core.V2.TokenizedString, string>(this.paths);
            var executable = Bam.Core.V2.TokenizedString.Create("$(buildroot)/$(modulename)", this);
            this.Policy.Package(this, executable, paths);
        }

        protected override void GetExecutionPolicy(string mode)
        {
            var className = "Publisher.V2." + mode + "Packager";
            this.Policy = Bam.Core.V2.ExecutionPolicyUtilities<IPackagePolicy>.Create(className);
        }
    }
}
    // Add modules here
}
