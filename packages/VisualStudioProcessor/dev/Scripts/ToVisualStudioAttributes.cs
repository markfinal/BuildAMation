// <copyright file="ToVisualStudioAttributes.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualStudioProcessor package</summary>
// <author>Mark Final</author>
namespace VisualStudioProcessor
{
    public static class ToVisualStudioAttributes
    {
        public static VisualStudioProcessor.ToolAttributeDictionary Execute(object sender, Opus.Core.Target target)
        {
            Opus.Core.BaseOptionCollection optionCollection = sender as Opus.Core.BaseOptionCollection;

            VisualStudioProcessor.ToolAttributeDictionary optionsDictionary = new VisualStudioProcessor.ToolAttributeDictionary();

            foreach (System.Collections.Generic.KeyValuePair<string, Opus.Core.Option> optionKeyValue in optionCollection)
            {
                string optionName = optionKeyValue.Key;
                Opus.Core.Option option = optionKeyValue.Value;

                if (null != option.PrivateData)
                {
                    IVisualStudioDelegate data = option.PrivateData as IVisualStudioDelegate;
                    if (null == data)
                    {
                        throw new Opus.Core.Exception(System.String.Format("Option data for '{0}', of type '{1}', does not implement the interface '{2}'", optionName, option.PrivateData.GetType().ToString(), typeof(IVisualStudioDelegate).ToString()));
                    }

                    Delegate visualStudioDelegate = data.VisualStudioProjectDelegate;
                    if (null != visualStudioDelegate)
                    {
                        if (null != visualStudioDelegate.Target)
                        {
                            // Not a requirement, but just a check
                            throw new Opus.Core.Exception(System.String.Format("Delegate for '{0}' should be static", optionName));
                        }

                        VisualStudioProcessor.ToolAttributeDictionary dictionary = data.VisualStudioProjectDelegate(optionCollection, option, target);
                        if (null != dictionary)
                        {
                            optionsDictionary.Merge(dictionary);
                        }
                    }
                }
                else
                {
                    throw new Opus.Core.Exception(System.String.Format("Archiver option '{0}' is not set up for VisualStudio project translation", optionName));
                }
            }

            return optionsDictionary;
        }
    }
}