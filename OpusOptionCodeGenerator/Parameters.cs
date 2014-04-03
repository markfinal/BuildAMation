// <copyright file="Parameters.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Option Code Generator</summary>
// <author>Mark Final</author>
namespace OpusOptionCodeGenerator
{
    class Parameters
    {
        [System.Flags]
        public enum Mode
        {
            GenerateProperties = (1<<0),
            GenerateDelegates  = (1<<1)
        }

        static public System.Collections.Specialized.StringCollection excludedFlagsFromHeaders = new System.Collections.Specialized.StringCollection();

        static Parameters()
        {
            excludedFlagsFromHeaders.Add("-f");
            excludedFlagsFromHeaders.Add("-uh");
            excludedFlagsFromHeaders.Add("-ih");
        }

        public string[] args;
        public string outputPropertiesPathName;
        public string outputDelegatesPathName;
        public string[] inputPathNames;
        public string[] inputDelegates;
        public string outputNamespace;
        public string outputClassName;
        public string privateDataClassName;
        public Mode mode;
        public bool toStdOut;
        public bool forceWrite;
        public bool extendedDelegates;
        public bool isBaseClass;
        public bool updateHeader;
        public bool ignoreHeaderUpdates;
    }
}
