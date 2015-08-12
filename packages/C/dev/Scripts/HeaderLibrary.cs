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
namespace C
{
namespace V2
{
    public abstract class HeaderLibrary :
        CModule
    {
        private Bam.Core.Array<Bam.Core.V2.Module> headers = new Bam.Core.Array<Bam.Core.V2.Module>();
        private Bam.Core.Array<Bam.Core.V2.Module> forwardedDeps = new Bam.Core.Array<Bam.Core.V2.Module>();

        public override void Evaluate()
        {
            this.IsUpToDate = true;
        }

        protected override void ExecuteInternal(Bam.Core.V2.ExecutionContext context)
        {
            // do nothing
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> ForwardedStaticLibraries
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module>(this.forwardedDeps.ToArray());
            }
        }

        public HeaderFileCollection
        CreateHeaderContainer()
        {
            var headers = Bam.Core.V2.Module.Create<HeaderFileCollection>();
            this.headers.Add(headers);
            this.Requires(headers);
            return headers;
        }

        public void
        CompileAgainst<DependentModule>() where DependentModule : CModule, new()
        {
            // no graph dependency, as it's just using patches
            // note that this won't add the module into the graph, unless a link dependency is made
            var dependent = Bam.Core.V2.Graph.Instance.FindReferencedModule<DependentModule>();
            this.UsePublicPatches(dependent);
            if (!(dependent is HeaderLibrary))
            {
                this.forwardedDeps.AddUnique(dependent);
            }
        }
    }
}
    /// <summary>
    /// C/C++ header only library
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(INullOpTool))]
    public class HeaderLibrary :
        Bam.Core.BaseModule
    {}
}
