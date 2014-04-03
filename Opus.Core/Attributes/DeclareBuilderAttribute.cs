// <copyright file="DeclareBuilderAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsageAttribute(System.AttributeTargets.Assembly, AllowMultiple=true)]
    public class DeclareBuilderAttribute : System.Attribute
    {
        public DeclareBuilderAttribute(string name, System.Type type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name
        {
            get;
            set;
        }

        public System.Type Type
        {
            get;
            set;
        }
    }
}