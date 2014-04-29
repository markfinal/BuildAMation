// <copyright file="ArchiverOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract partial class ArchiverOptionCollection : C.ArchiverOptionCollection, C.IArchiverOptions, IArchiverOptions, VisualStudioProcessor.IVisualStudioSupport
    {
        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            (this as IArchiverOptions).NoLogo = true;

            (this as C.IArchiverOptions).OutputType = C.EArchiverOutput.StaticLibrary;
        }

        public ArchiverOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            if (null != this.LibraryFilePath)
            {
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(this.LibraryFilePath));
            }

            return directoriesToCreate;
        }

        VisualStudioProcessor.ToolAttributeDictionary VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(Opus.Core.Target target)
        {
            VisualStudioProcessor.EVisualStudioTarget vsTarget = (target.Toolset as VisualStudioProcessor.IVisualStudioTargetInfo).VisualStudioTarget;
            switch (vsTarget)
            {
                case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
                case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                    break;

                default:
                    throw new Opus.Core.Exception("Unsupported VisualStudio target, '{0}'", vsTarget);
            }
            VisualStudioProcessor.ToolAttributeDictionary dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}