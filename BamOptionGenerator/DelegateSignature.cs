// <copyright file="DelegateSignature.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Option Code Generator</summary>
// <author>Mark Final</author>
namespace OpusOptionCodeGenerator
{
    class DelegateSignature
    {
        public string InNamespace
        {
            get;
            set;
        }

        public string ReturnType
        {
            get;
            set;
        }

        public System.Collections.Generic.List<string> Arguments
        {
            get;
            set;
        }

        public System.Collections.Specialized.StringCollection Body
        {
            get;
            set;
        }
    }
}
