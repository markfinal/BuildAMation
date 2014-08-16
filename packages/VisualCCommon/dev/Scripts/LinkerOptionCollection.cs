// <copyright file="LinkerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract partial class LinkerOptionCollection :
        C.LinkerOptionCollection,
        C.ILinkerOptions,
        ILinkerOptions,
        VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            var linkerInterface = this as ILinkerOptions;

            linkerInterface.NoLogo = true;
            linkerInterface.StackReserveAndCommit = null;
            linkerInterface.IgnoredLibraries = new Bam.Core.StringArray();

            var target = node.Target;
            linkerInterface.IncrementalLink = target.HasConfiguration(Bam.Core.EConfiguration.Debug);

            var linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;

            foreach (var libPath in linkerTool.LibPaths((Bam.Core.BaseTarget)target))
            {
                (this as C.ILinkerOptions).LibraryPaths.Add(libPath);
            }
        }

        public
        LinkerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var options = this as C.ILinkerOptions;
            if (options.DebugSymbols)
            {
                var locationMap = node.Module.Locations;
                var pdbDir = locationMap[Linker.PDBDir] as Bam.Core.ScaffoldLocation;
                if (!pdbDir.IsValid)
                {
                    pdbDir.SetReference(locationMap[C.Application.OutputDir]);
                }

                var pdbFile = locationMap[Linker.PDBFile] as Bam.Core.ScaffoldLocation;
                if (!pdbFile.IsValid)
                {
                    pdbFile.SpecifyStub(pdbDir, this.OutputName + ".pdb", Bam.Core.Location.EExists.WillExist);
                }
            }

            base.FinalizeOptions(node);
        }

        VisualStudioProcessor.ToolAttributeDictionary
        VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(
            Bam.Core.Target target)
        {
            var vsTarget = (target.Toolset as VisualStudioProcessor.IVisualStudioTargetInfo).VisualStudioTarget;
            switch (vsTarget)
            {
                case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
                case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                    break;

                default:
                    throw new Bam.Core.Exception("Unsupported VisualStudio target, '{0}'", vsTarget);
            }
            var dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}
