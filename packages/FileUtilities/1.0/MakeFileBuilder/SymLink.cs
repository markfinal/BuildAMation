// <copyright file="SymLink.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
#if false
        private void PostBuildEventSymLink(Opus.Core.IModule moduleForPostEvents,
                                           string destinationDirectory,
                                           Opus.Core.StringArray filesToCopy)
        {
            Opus.Core.DependencyNode sourceModuleNode = moduleForPostEvents.OwningNode;
            IProject nodeProjectData = sourceModuleNode.Data as IProject;

            ProjectConfigurationCollection configCollection = nodeProjectData.Configurations;
            string configurationName = configCollection.GetConfigurationNameForTarget(sourceModuleNode.Target);
            ProjectConfiguration configuration = configCollection[configurationName];

            string toolName = "VCPostBuildEventTool";
            ProjectTool vcPostBuildEventTool = configuration.GetTool(toolName);
            if (null == vcPostBuildEventTool)
            {
                vcPostBuildEventTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcPostBuildEventTool);
            }

            System.Text.StringBuilder commandLine = new System.Text.StringBuilder();
            commandLine.AppendFormat("IF NOT EXIST \"{0}\" MKDIR \"{0}\"\n\r", destinationDirectory);
            foreach (string sourceFile in filesToCopy)
            {
                commandLine.AppendFormat("cmd.exe /c COPY \"{0}\" \"{1}\"\n\r", sourceFile, destinationDirectory);
            }

            lock (vcPostBuildEventTool)
            {
                string attributeName = null;
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == nodeProjectData.VSTarget)
                {
                    attributeName = "CommandLine";
                }
                else if (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == nodeProjectData.VSTarget)
                {
                    attributeName = "Command";
                }
                if (vcPostBuildEventTool.HasAttribute(attributeName))
                {
                    string currentValue = vcPostBuildEventTool[attributeName];
                    currentValue += commandLine.ToString();
                    vcPostBuildEventTool[attributeName] = currentValue;
                }
                else
                {
                    vcPostBuildEventTool.AddAttribute(attributeName, commandLine.ToString());
                }
            }
        }
#endif

        public object Build(FileUtilities.SymLink symLink, out bool success)
        {
            Opus.Core.BaseModule symLinkModule = symLink as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = symLinkModule.OwningNode;
            Opus.Core.Target target = node.Target;

            // locate target
            string symlinkTarget = null;
            System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.NonPublic |
                                                          System.Reflection.BindingFlags.Public |
                                                          System.Reflection.BindingFlags.Instance;
            System.Reflection.FieldInfo[] fields = symLink.GetType().GetFields(bindingFlags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                var sourceModuleAttributes = field.GetCustomAttributes(typeof(Opus.Core.SourceFilesAttribute), false);
                if (1 == sourceModuleAttributes.Length)
                {
                    if (null != symlinkTarget)
                    {
                        throw new Opus.Core.Exception("Can only specify one target for a symlink");
                    }

                    Opus.Core.File file = field.GetValue(symLink) as Opus.Core.File;
                    if (null == file)
                    {
                        throw new Opus.Core.Exception("Target can only be of type Opus.Core.File");
                    }

                    symlinkTarget = file.AbsolutePath;
                }
            }

            if (null == symlinkTarget)
            {
                throw new Opus.Core.Exception("No symlink target specified");
            }

            Opus.Core.BaseOptionCollection symLinkOptions = symLinkModule.Options;

            string link = symLinkOptions.OutputPaths[FileUtilities.SymLinkOutputFileFlags.Link];

            // TODO
            Opus.Core.Log.MessageAll("TODO: MakeFile support for symlink for '{0}' to '{1}'", symlinkTarget, link);

            success = true;
            return null;
        }
    }
}
