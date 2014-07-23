// <copyright file="Exception.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Option Code Generator</summary>
// <author>Mark Final</author>
namespace OpusOptionCodeGenerator
{
    class Exception :
        System.Exception
    {
        public
        Exception(
            string message) :
        base(message)
        {}

        public
        Exception(
            string message,
            params object[] args) :
        base(System.String.Format(message, args))
        {}
    }
}
