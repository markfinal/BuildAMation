// <copyright file="AssignOptionCollectionAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class AssignOptionCollectionAttribute : System.Attribute
    {
        public AssignOptionCollectionAttribute(System.Type optionCollectionType)
        {
            this.OptionCollectionType = optionCollectionType;
        }

        public System.Type OptionCollectionType
        {
            get;
            private set;
        }
    }
}
