// <copyright file="PropertySignatureList.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Option Code Generator</summary>
// <author>Mark Final</author>
namespace OpusOptionCodeGenerator
{
    class PropertySignatureList :
        System.Collections.Generic.List<PropertySignature>
    {
        public string InterfaceName
        {
            get;
            set;
        }
    }
}
