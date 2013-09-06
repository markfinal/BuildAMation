// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed partial class XCodeBuilder
    {
        public object Build(C.ObjectFile moduleToBuild, out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var moduleName = node.ModuleName;
            var target = node.Target;
            var baseTarget = (Opus.Core.BaseTarget)target;

            Opus.Core.Log.MessageAll("ObjectFile {0}", moduleName);
            var sourceFile = moduleToBuild.SourceFile.AbsolutePath;
            var fileRef = new PBXFileReference(moduleName, sourceFile, this.RootUri);
            fileRef.IsSourceCode = true;
            this.Project.FileReferences.Add(fileRef);

            var buildConfiguration = this.Project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
            // TODO: what to do when there are multiple configurations
            if (target.HasPlatform(Opus.Core.EPlatform.OSX64))
            {
                buildConfiguration.Options["ARCHS"] = "\"$(ARCHS_STANDARD_64_BIT)\"";
            }
            else
            {
                buildConfiguration.Options["ARCHS"] = "\"$(ARCHS_STANDARD_32_BIT)\"";
            }
            buildConfiguration.Options["ONLY_ACTIVE_ARCH"] = "YES";
            buildConfiguration.Options["MACOSX_DEPLOYMENT_TARGET"] = "10.8";
            buildConfiguration.Options["SDKROOT"] = "macosx";

            var data = new PBXBuildFile(moduleName);
            data.FileReference = fileRef;
            this.Project.BuildFiles.Add(data);

            var sourcesBuildPhase = this.Project.SourceBuildPhases.Get("Sources", moduleName);
            sourcesBuildPhase.Files.Add(data);
            data.BuildPhase = sourcesBuildPhase;

            success = true;
            return data;
        }
    }
}
