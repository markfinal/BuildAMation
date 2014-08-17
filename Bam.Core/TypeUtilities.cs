// <copyright file="TypeUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public static class TypeUtilities
    {
        public static void
        CheckTypeDerivesFrom(
            System.Type type,
            System.Type baseClass)
        {
            if (!baseClass.IsAssignableFrom(type))
            {
                throw new Exception("Type '{0}' is not derived from {1}", type.ToString(), baseClass.ToString());
            }
        }

        public static void
        CheckTypeImplementsInterface(
            System.Type type,
            System.Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new Exception("Type '{0}' is not an interface", interfaceType.ToString());
            }

            if (!interfaceType.IsAssignableFrom(type))
            {
                throw new Exception("Type '{0}' does not implement the interface {1}", type.ToString(), interfaceType.ToString());
            }
        }

        public static void
        CheckTypeIsNotAbstract(
            System.Type type)
        {
            if (type.IsAbstract)
            {
                throw new Exception("Type '{0}' is abstract", type.ToString());
            }
        }
    }
}
