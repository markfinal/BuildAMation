// <copyright file="OptionCollectionFactory.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class OptionCollectionFactory
    {
        private readonly static System.Type optionCollectionType = typeof(BaseOptionCollection);

        public static BaseOptionCollection CreateOptionCollection(System.Type requiredOptionCollectionType)
        {
            if (!requiredOptionCollectionType.IsSubclassOf(optionCollectionType))
            {
                throw new Exception(System.String.Format("Type '{0}' does not derive from the base class {1}", requiredOptionCollectionType.ToString(), optionCollectionType.ToString()),false);
            }

            if (null == requiredOptionCollectionType.GetConstructor(System.Type.EmptyTypes))
            {
                throw new Exception(System.String.Format("Default constructor for type '{0}' does not exist", requiredOptionCollectionType.ToString()), false);
            }

            BaseOptionCollection optionCollection = System.Activator.CreateInstance(requiredOptionCollectionType) as BaseOptionCollection;
            return optionCollection;
        }

        public static DerivedType CreateOptionCollection<DerivedType>(DependencyNode owningNode) where DerivedType : BaseOptionCollection
        {
            System.Type requiredOptionCollectionType = typeof(DerivedType);
            if (!requiredOptionCollectionType.IsSubclassOf(optionCollectionType))
            {
                throw new Exception(System.String.Format("Type '{0}' does not derive from the base class {1}", requiredOptionCollectionType.ToString(), optionCollectionType.ToString()), false);
            }

            if (null == requiredOptionCollectionType.GetConstructor(new System.Type[] { typeof(DependencyNode) }))
            {
                throw new Exception(System.String.Format("Missing constructor: '{0}(DependencyNode)'", requiredOptionCollectionType.ToString()), false);
            }

            BaseOptionCollection optionCollection = System.Activator.CreateInstance(requiredOptionCollectionType, new object[] { owningNode }) as BaseOptionCollection;

            DerivedType derivedOptionCollection = optionCollection as DerivedType;
            return derivedOptionCollection;
        }
    }
}