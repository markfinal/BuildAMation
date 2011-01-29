// <copyright file="PostExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>NativeBuilder package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public void PostExecute(Opus.Core.DependencyNodeCollection nodeCollection)
        {
            Opus.Core.Log.MessageAll("PostExecute for Native builds");

            Opus.Core.Log.MessageAll("There are {0} data entries", nodeCollection.Count);
            foreach (Opus.Core.DependencyNode node in nodeCollection)
            {
                Opus.Core.Log.MessageAll("\tModule {0}", node.UniqueModuleName);
                if (null == node.Module.Options)
                {
                    Opus.Core.Log.MessageAll("\t\tHas no options");
                    continue;
                }

                Opus.Core.DependencyNodeCollection dependees = node.ExternalDependentFor;
                if (null == dependees)
                {
                    Opus.Core.Log.MessageAll("\t\tHas no dependees");
                }
                else
                {
                    Opus.Core.Log.MessageAll("\t\tDependee node is {0}", dependees[0].UniqueModuleName);
                }

                System.Type moduleType = node.Module.GetType();
                var copyToParentAttributes = moduleType.GetCustomAttributes(typeof(Opus.Core.CopyToParentAttribute), false);

                Opus.Core.OutputPaths o = node.Module.Options.OutputPaths;
                foreach (System.Collections.Generic.KeyValuePair<object, string> i in o)
                {
                    if (copyToParentAttributes.Length > 0)
                    {
                        // TODO: could work out the type of the OptionCollection from node.Module.Options meta data, and check that it matches the type of the enum value
                        int enumValue = (int)(copyToParentAttributes[0] as Opus.Core.CopyToParentAttribute).EnumValue;
                        int keyValue = (int)i.Key;
                        if (keyValue == (enumValue & keyValue))
                        {
                            Opus.Core.Log.MessageAll("\t\t\t{0} - {1} **", i.Key, i.Value);
                        }
                        else
                        {
                            Opus.Core.Log.MessageAll("\t\t\t{0} - {1}", i.Key, i.Value);
                        }
                    }
                    else
                    {
                        Opus.Core.Log.MessageAll("\t\t\t{0} - {1}", i.Key, i.Value);
                    }
                }

                // TODO: we need to discover
                // 1) the list of files to copy
                // 2) the destination directory
                // module options therefore need to expose
                // a) their output directory
                // b) the list of files in that directory
                // I think we can rule out multiple output directoriess
            }
        }
    }
}