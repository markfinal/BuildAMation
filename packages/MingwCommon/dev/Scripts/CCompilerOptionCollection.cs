// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection : C.CompilerOptionCollection, C.ICCompilerOptions, ICCompilerOptions
    {
        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
        {
            ICCompilerOptions compilerInterface = this as ICCompilerOptions;
            compilerInterface.AllWarnings = true;
            compilerInterface.ExtraWarnings = true;

            base.SetDefaultOptionValues(node);

            Opus.Core.Target target = node.Target;
            compilerInterface.SixtyFourBit = target.HasPlatform(Opus.Core.EPlatform.Win64);

            if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
            {
                compilerInterface.StrictAliasing = false;
                compilerInterface.InlineFunctions = false;
            }
            else
            {
                compilerInterface.StrictAliasing = true;
                compilerInterface.InlineFunctions = true;
            }

            (this as C.ICCompilerOptions).TargetLanguage = C.ETargetLanguage.C;

            Opus.Core.IToolset toolset = target.Toolset;
            C.ICompilerTool compilerTool = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            (this as C.ICCompilerOptions).SystemIncludePaths.AddRange(compilerTool.IncludePaths((Opus.Core.BaseTarget)node.Target));

            compilerInterface.Pedantic = true;
        }

        public CCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (this.OutputPaths.Has(C.OutputFileFlags.ObjectFile))
            {
                string objPathName = this.ObjectFilePath;
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(objPathName));
            }

            return directoriesToCreate;
        }
    }
}
