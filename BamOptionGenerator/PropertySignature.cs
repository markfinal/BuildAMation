// <copyright file="PropertySignature.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Option Code Generator</summary>
// <author>Mark Final</author>
namespace OpusOptionCodeGenerator
{
    class PropertySignature
    {
        public string Type
        {
            get;
            set;
        }

        public bool IsValueType
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public bool HasGet
        {
            get;
            set;
        }

        public bool HasSet
        {
            get;
            set;
        }

        public bool StateOnly
        {
            get;
            set;
        }
    }
}
