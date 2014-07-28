// <copyright file="ToXcodeProject.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CommandLineProcessor package</summary>
// <author>Mark Final</author>
namespace XcodeProjectProcessor
{
    public static class ToXcodeProject
    {
        public static void
        Execute(
            object sender,
            XcodeBuilder.PBXProject project,
            XcodeBuilder.XcodeNodeData currentObject,
            XcodeBuilder.XCBuildConfiguration configuration,
            Opus.Core.Target target)
        {
            var optionCollection = sender as Opus.Core.BaseOptionCollection;
            var optionNames = optionCollection.OptionNames;

            foreach (var optionName in optionNames)
            {
                var option = optionCollection[optionName];
                if (null == option.PrivateData)
                {
                    continue;
                }

                var data = option.PrivateData as IXcodeProjectDelegate;
                if (null == data)
                {
                    throw new Opus.Core.Exception("Option data for '{0}', of type '{1}', does not implement the interface '{2}' in '{3}'", optionName, option.PrivateData.GetType().ToString(), typeof(IXcodeProjectDelegate).ToString(), sender.GetType().ToString());
                }

                var xcodeProjectDelegate = data.XcodeProjectDelegate;
                if (null != xcodeProjectDelegate)
                {
                    if (null != xcodeProjectDelegate.Target)
                    {
                        // Not a requirement, but just a check
                        throw new Opus.Core.Exception("Delegate for '{0}' should be static in '{1}'", optionName, sender.GetType().ToString());
                    }
                    xcodeProjectDelegate(sender, project, currentObject, configuration, option, target);
                }
            }
        }
    }
}
