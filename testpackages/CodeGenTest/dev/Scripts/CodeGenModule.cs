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
        Opus.Core.BaseOptionCollection,
        CommandLineProcessor.ICommandLineSupport,
        ICodeGenOptions
    {
        public
        CodeGenOptionCollection(
            Opus.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Opus.Core.DependencyNode owningNode)
        {
            var options = this as ICodeGenOptions;
            options.OutputSourceDirectory = owningNode.GetTargettedModuleBuildDirectoryLocation("src").GetSingleRawPath();
            options.OutputName = "function";

            if (!owningNode.Module.Locations[CodeGenModule.OutputDir].IsValid)
            {
                (owningNode.Module.Locations[CodeGenModule.OutputDir] as Opus.Core.ScaffoldLocation).SetReference(owningNode.GetTargettedModuleBuildDirectoryLocation("src"));
            }
        }

        public override void
        FinalizeOptions(
            Opus.Core.DependencyNode node)
        {
            if (!node.Module.Locations[CodeGenModule.OutputFile].IsValid)
            {
                var options = node.Module.Options as ICodeGenOptions;
                (node.Module.Locations[CodeGenModule.OutputFile] as Opus.Core.ScaffoldLocation).SpecifyStub(node.Module.Locations[CodeGenModule.OutputDir], options.OutputName + ".c", Opus.Core.Location.EExists.WillExist);
            }

            base.FinalizeOptions(node);
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineStringBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
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
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(CodeGeneratorTool_UpdateOptions);
        }

        void
        CodeGeneratorTool_UpdateOptions(
            Opus.Core.IModule module,
            Opus.Core.Target target)
        {
            var options = module.Options as C.ILinkerOptions;
            options.DoNotAutoIncludeStandardLibraries = false;
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile source = new C.ObjectFile();

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray vcDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }

    /// <summary>
    /// Code generation of C++ source
    /// </summary>
    [Opus.Core.ModuleToolAssignment(typeof(ICodeGenTool))]
    public abstract class CodeGenModule :
        Opus.Core.BaseModule,
        Opus.Core.IInjectModules
    {
        public static readonly Opus.Core.LocationKey OutputFile = new Opus.Core.LocationKey("GeneratedSource", Opus.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Opus.Core.LocationKey OutputDir = new Opus.Core.LocationKey("GeneratedSourceDir", Opus.Core.ScaffoldLocation.ETypeHint.Directory);

        [Opus.Core.RequiredModules]
        protected Opus.Core.TypeArray requiredModules = new Opus.Core.TypeArray(new System.Type[]{
            typeof(CodeGeneratorTool)
        });

        #region IInjectModules Members

        string
        Opus.Core.IInjectModules.GetInjectedModuleNameSuffix(
            Opus.Core.BaseTarget baseTarget)
        {
            return "CodeGen";
        }

        System.Type
        Opus.Core.IInjectModules.GetInjectedModuleType(
            Opus.Core.BaseTarget baseTarget)
        {
            return typeof(C.ObjectFile);
        }

        Opus.Core.DependencyNode
        Opus.Core.IInjectModules.GetInjectedParentNode(
            Opus.Core.DependencyNode node)
        {
            var dependentFor = node.ExternalDependentFor;
            var firstDependentFor = dependentFor[0];
            return firstDependentFor;
        }

        void
        Opus.Core.IInjectModules.ModuleCreationFixup(
            Opus.Core.DependencyNode node)
        {
            var dependent = node.ExternalDependents;
            var firstDependent = dependent[0];
            var dependentModule = firstDependent.Module;
            var module = node.Module as C.ObjectFile;
            var sourceFile = new Opus.Core.ScaffoldLocation(Opus.Core.ScaffoldLocation.ETypeHint.File);
            sourceFile.SetReference(dependentModule.Locations[CodeGenModule.OutputFile]);
            module.SourceFileLocation = sourceFile;
        }

        #endregion
    }
}
