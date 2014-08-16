// <copyright file="GlobalOptionCollectionOverrideAttribute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class GlobalOptionCollectionOverrideAttribute :
        System.Attribute
    {
        public
        GlobalOptionCollectionOverrideAttribute(
            System.Type instanceType)
        {
            var instance = System.Activator.CreateInstance(instanceType);
            var overrideInterface = instance as IGlobalOptionCollectionOverride;
            if (null == overrideInterface)
            {
                throw new Exception("'{0}' does not implement the Opus.Core.IGlobalOptionCollectionOverride interface", instance.ToString());
            }

            this.OverrideInterface = overrideInterface;
        }

        public IGlobalOptionCollectionOverride OverrideInterface
        {
            get;
            private set;
        }
    }
}