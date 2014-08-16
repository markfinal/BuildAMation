namespace CodeGenTest
{
    public class ExportCodeGenOptionsDelegateAttribute :
        System.Attribute
    {}

    public class LocalCodeGenOptionsDelegateAttribute :
        System.Attribute
    {}

    public sealed class PrivateData :
        CommandLineProcessor.ICommandLineDelegate
    {
        public
        PrivateData(
            CommandLineProcessor.Delegate commandLineDelegate)
        {
            this.CommandLineDelegate = commandLineDelegate;
        }

        public CommandLineProcessor.Delegate CommandLineDelegate
        {
            get;
            set;
        }
    }

    public sealed partial class CodeGenOptionCollection :
        Bam.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport,
        ICodeGenOptions
    {
        public
        CodeGenOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode owningNode)
        {
            var options = this as ICodeGenOptions;
            options.OutputSourceDirectory = owningNode.GetTargettedModuleBuildDirectoryLocation("src").GetSingleRawPath();
            options.OutputName = "function";

            if (!owningNode.Module.Locations[CodeGenModule.OutputDir].IsValid)
            {
                (owningNode.Module.Locations[CodeGenModule.OutputDir] as Bam.Core.ScaffoldLocation).SetReference(owningNode.GetTargettedModuleBuildDirectoryLocation("src"));
            }
        }

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            if (!node.Module.Locations[CodeGenModule.OutputFile].IsValid)
            {
                var options = node.Module.Options as ICodeGenOptions;
                (node.Module.Locations[CodeGenModule.OutputFile] as Bam.Core.ScaffoldLocation).SpecifyStub(node.Module.Locations[CodeGenModule.OutputDir], options.OutputName + ".c", Bam.Core.Location.EExists.WillExist);
            }

            base.FinalizeOptions(node);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Bam.Core.StringArray commandLineStringBuilder, Bam.Core.Target target, Bam.Core.StringArray excludedOptionNames)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineStringBuilder, target, excludedOptionNames);
        }
    }

    class CodeGeneratorTool :
        C.Application
    {
        public static string VersionString
        {
            get
            {
                return "dev";
            }
        }

        public
        CodeGeneratorTool()
        {
            var sourceDir = this.PackageLocation.SubDirectory("source");
            var codeGenToolSourceDir = sourceDir.SubDirectory("codegentool");
            this.source.Include(codeGenToolSourceDir, "main.c");
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(CodeGeneratorTool_UpdateOptions);
        }

        void
        CodeGeneratorTool_UpdateOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            options.DoNotAutoIncludeStandardLibraries = false;
        }

        [Bam.Core.SourceFiles]
        C.ObjectFile source = new C.ObjectFile();

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray vcDependents = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }

    /// <summary>
    /// Code generation of C++ source
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(ICodeGenTool))]
    public abstract class CodeGenModule :
        Bam.Core.BaseModule,
        Bam.Core.IInjectModules
    {
        public static readonly Bam.Core.LocationKey OutputFile = new Bam.Core.LocationKey("GeneratedSource", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("GeneratedSourceDir", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        [Bam.Core.RequiredModules]
        protected Bam.Core.TypeArray requiredModules = new Bam.Core.TypeArray(new System.Type[]{
            typeof(CodeGeneratorTool)
        });

        #region IInjectModules Members

        string
        Bam.Core.IInjectModules.GetInjectedModuleNameSuffix(
            Bam.Core.BaseTarget baseTarget)
        {
            return "CodeGen";
        }

        System.Type
        Bam.Core.IInjectModules.GetInjectedModuleType(
            Bam.Core.BaseTarget baseTarget)
        {
            return typeof(C.ObjectFile);
        }

        Bam.Core.DependencyNode
        Bam.Core.IInjectModules.GetInjectedParentNode(
            Bam.Core.DependencyNode node)
        {
            var dependentFor = node.ExternalDependentFor;
            var firstDependentFor = dependentFor[0];
            return firstDependentFor;
        }

        void
        Bam.Core.IInjectModules.ModuleCreationFixup(
            Bam.Core.DependencyNode node)
        {
            var dependent = node.ExternalDependents;
            var firstDependent = dependent[0];
            var dependentModule = firstDependent.Module;
            var module = node.Module as C.ObjectFile;
            var sourceFile = new Bam.Core.ScaffoldLocation(Bam.Core.ScaffoldLocation.ETypeHint.File);
            sourceFile.SetReference(dependentModule.Locations[CodeGenModule.OutputFile]);
            module.SourceFileLocation = sourceFile;
        }

        #endregion
    }
}
