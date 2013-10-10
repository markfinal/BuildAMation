// <copyright file="BaseOptionCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public abstract class BaseOptionCollection : System.Collections.IEnumerable, System.ICloneable
    {
        protected System.Collections.Generic.Dictionary<string, Option> table = new System.Collections.Generic.Dictionary<string, Option>();

        public OutputPaths OutputPaths
        {
            get;
            private set;
        }

        private DependencyNode OwningNode
        {
            get;
            set;
        }

        protected BaseOptionCollection()
        {
            this.OutputPaths = new OutputPaths();
        }

        protected BaseOptionCollection(DependencyNode owningNode)
            : this()
        {
            this.OwningNode = owningNode;
            this.SetNodeOwnership(owningNode);
            this.InitializeDefaults(owningNode);
            this.SetDelegates(owningNode);
        }

        protected abstract void InitializeDefaults(DependencyNode owningNode);
        protected abstract void SetDelegates(DependencyNode owningNode);

        public virtual void SetNodeOwnership(DependencyNode node)
        {
            // do nothing by default
        }

        public Option this[string key]
        {
            get
            {
                if (!this.Contains(key))
                {
                    var builder = new System.Text.StringBuilder();
                    builder.AppendFormat("Option '{0}' has not been registered in collection '{1}'. Is a default value missing?\n", key, this.ToString());
                    builder.Append("Options registered are:\n");
                    foreach (var keyName in this.table.Keys)
                    {
                        builder.AppendFormat("\t'{0}'\n", keyName);
                    }
                    throw new Exception(builder.ToString());
                }

                return this.table[key];
            }

            set
            {
                this.table[key] = value;
            }
        }

        public virtual object Clone()
        {
            var optionsType = this.GetType();
            var clonedOptions = OptionCollectionFactory.CreateOptionCollection(optionsType);

            // TODO: can I use var here? check in Mono
            foreach (System.Collections.Generic.KeyValuePair<string, Option> option in this.table)
            {
                clonedOptions.table.Add(option.Key, option.Value.Clone() as Option);
            }
            return clonedOptions;
        }

        private void InvokeSetHandler(System.Reflection.MethodInfo setHandler, Option option)
        {
            if (null != setHandler)
            {
                if (2 != setHandler.GetParameters().Length)
                {
                    throw new Exception("SetHandler requires the signature 'void {0}(BaseOptionCollection, Option)', not '{1}'", setHandler.Name, setHandler.ToString());
                }

                setHandler.Invoke(null, new object[] { this, option });
            }
        }

        public void ProcessNamedSetHandler(string setHandlerName, Option option)
        {
            var type = this.GetType();
            var bindingFlags = System.Reflection.BindingFlags.Static |           // don't need an instance
                               System.Reflection.BindingFlags.NonPublic |        // generally hidden - should be protected
                               System.Reflection.BindingFlags.FlattenHierarchy;  // bring in protected static functions
            InvokeSetHandler(type.GetMethod(setHandlerName, bindingFlags), option);
        }

        public virtual void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            // do nothing
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.table.GetEnumerator();
        }

        public bool Contains(string key)
        {
            return this.table.ContainsKey(key);
        }

        public StringArray OptionNames
        {
            get
            {
                return new StringArray(this.table.Keys);
            }
        }

        protected Type GetValueTypeOption<Type>(string optionName) where Type : struct
        {
            return (this[optionName] as Core.ValueTypeOption<Type>).Value;
        }

        protected Type GetValueTypeOption<Type>(string optionName, BaseOptionCollection superSetOptionCollection) where Type : struct
        {
            if (this.Contains(optionName))
            {
                return (this[optionName] as Core.ValueTypeOption<Type>).Value;
            }
            else
            {
                if (null == superSetOptionCollection)
                {
                    throw new Exception("Unable to locate option '{0}'", optionName);
                }
                return (superSetOptionCollection[optionName] as Core.ValueTypeOption<Type>).Value;
            }
        }

        protected void SetValueTypeOption<Type>(string optionName, Type value) where Type : struct
        {
            if (this.Contains(optionName))
            {
                (this[optionName] as Core.ValueTypeOption<Type>).Value = value;
            }
            else
            {
                this[optionName] = new Core.ValueTypeOption<Type>(value);
            }
        }

        protected Type GetReferenceTypeOption<Type>(string optionName) where Type : class
        {
            return (this[optionName] as Core.ReferenceTypeOption<Type>).Value;
        }

        protected Type GetReferenceTypeOption<Type>(string optionName, BaseOptionCollection superSetOptionCollection) where Type : class
        {
            if (this.Contains(optionName))
            {
                return (this[optionName] as Core.ReferenceTypeOption<Type>).Value;
            }
            else
            {
                if (null == superSetOptionCollection)
                {
                    throw new Exception("Unable to locate option '{0}'", optionName);
                }
                return (superSetOptionCollection[optionName] as Core.ReferenceTypeOption<Type>).Value;
            }
        }

        protected void SetReferenceTypeOption<Type>(string optionName, Type value) where Type : class
        {
            if (this.Contains(optionName))
            {
                (this[optionName] as Core.ReferenceTypeOption<Type>).Value = value;
            }
            else
            {
                this[optionName] = new Core.ReferenceTypeOption<Type>(value);
            }
        }

        public void FilterOutputPaths(System.Enum filter, StringArray outPaths)
        {
            var filterType = filter.GetType();
            int filterValue = System.Convert.ToInt32(filter);

            var outputPaths = this.OutputPaths;
            if (State.RunningMono)
            {
                foreach (var key in outputPaths.Types)
                {
                    if (key.GetType() != filterType)
                    {
                        throw new Exception("Incompatible enum type comparison");
                    }

                    int keyValue = System.Convert.ToInt32(key);

                    if (keyValue == (filterValue & keyValue))
                    //if (o.Key.Includes(filter))
                    {
                        string paths = outputPaths[key];
                        outPaths.Add(paths);
                    }
                }
            }
            else
            {
                // TODO: this causes a System.InvalidCastException, Cannot cast from source type to destination type
                // because it's a SortedDictionary? Can't find any reference to this though
                foreach (System.Collections.Generic.KeyValuePair<System.Enum, string> o in outputPaths)
                {
                    if (o.Key.GetType() != filterType)
                    {
                        throw new Exception("Incompatible enum type comparison");
                    }

                    int keyValue = System.Convert.ToInt32(o.Key);

                    if (keyValue == (filterValue & keyValue))
                    //if (o.Key.Includes(filter))
                    {
                        outPaths.Add(o.Value);
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as BaseOptionCollection;
            foreach (var optionKey in this.table.Keys)
            {
                var thisOption = this.table[optionKey];
                if (!other.table.ContainsKey(optionKey))
                {
                    return false;
                }
                var otherOption = other.table[optionKey];
                if (thisOption.GetType() != otherOption.GetType())
                {
                    throw new Exception("Option values for key {0} have different types: {1} & {2}",
                                        optionKey, thisOption.GetType().ToString(), otherOption.GetType().ToString());
                }

                if (!thisOption.Equals(otherOption))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public BaseOptionCollection Complement(Opus.Core.BaseOptionCollection other)
        {
            // TODO: essentially need to check whether the two option collections share a base class other than BaseOptionCollection
            // can this be done by going backward up the class hierarchy from BaseOptionCollection, since it's likely that the next
            // subclass after that is common
#if false
            if (!(this.GetType().IsAssignableFrom(other.GetType()) || other.GetType().IsAssignableFrom(this.GetType())))
            {
                throw new Exception("Option collections have uncomparable types {0} & {1}", this.GetType().FullName, other.GetType().FullName);
            }
#endif

            var complement = OptionCollectionFactory.CreateOptionCollection(this.GetType());
            foreach (var optionKey in this.table.Keys)
            {
                var thisOption = this.table[optionKey];
                if (!other.table.ContainsKey(optionKey))
                {
                    complement[optionKey] = thisOption.Clone() as Option;
                    continue;
                }
                var otherOption = other.table[optionKey];
                if (thisOption.GetType() != otherOption.GetType())
                {
                    throw new Exception("Option values for key {0} have different types: {1} & {2}",
                                        optionKey, thisOption.GetType().ToString(), otherOption.GetType().ToString());
                }

                if (!thisOption.Equals(otherOption))
                {
                    var complementOption = thisOption.Complement(otherOption);
                    if (null == complementOption)
                    {
                        throw new Exception("Complement option collection for option '{0}' does not exist. Is there something wrong with the equivalence checks?", optionKey);
                    }

                    complement[optionKey] = complementOption;
                }
            }

            if (complement != null)
            {
                complement.SuperSetOptionCollection = this;
            }

            return complement;
        }

        public BaseOptionCollection Intersect(Opus.Core.BaseOptionCollection other)
        {
            // TODO: essentially need to check whether the two option collections share a base class other than BaseOptionCollection
            // can this be done by going backward up the class hierarchy from BaseOptionCollection, since it's likely that the next
            // subclass after that is common
#if false
            if (!(this.GetType().IsAssignableFrom(other.GetType()) || other.GetType().IsAssignableFrom(this.GetType())))
            {
                throw new Exception("Option collections have uncomparable types {0} & {1}", this.GetType().FullName, other.GetType().FullName);
            }
#endif

            BaseOptionCollection intersect = null;
            foreach (var optionKey in this.table.Keys)
            {
                var thisOption = this.table[optionKey];
                if (!other.table.ContainsKey(optionKey))
                {
                    continue;
                }
                var otherOption = other.table[optionKey];
                if (thisOption.GetType() != otherOption.GetType())
                {
                    throw new Exception("Option values for key {0} have different types: {1} & {2}",
                                        optionKey, thisOption.GetType().ToString(), otherOption.GetType().ToString());
                }

                if (thisOption.Equals(otherOption))
                {
                    if (null == intersect)
                    {
                        intersect = OptionCollectionFactory.CreateOptionCollection(this.GetType());
                    }
                    intersect[optionKey] = thisOption.Clone() as Option;
                }
                else
                {
                    var intersectOption = thisOption.Intersect(otherOption);
                    if (null != intersectOption)
                    {
                        if (null == intersect)
                        {
                            intersect = OptionCollectionFactory.CreateOptionCollection(this.GetType());
                        }
                        intersect[optionKey] = intersectOption;
                    }
                }
            }

            if (null != intersect)
            {
                // only set it once, as Intersect may be called iteratively
                if (null == intersect.SuperSetOptionCollection)
                {
                    intersect.SuperSetOptionCollection = this;
                }
            }

            return intersect;
        }

        public bool Empty
        {
            get
            {
                return (0 == this.table.Count);
            }
        }

        /// <summary>
        /// When two BaseOptionCollections are operated on, the resulting BaseOptionCollection may still need
        /// to refer to the source, so this reference is kept.
        /// </summary>
        /// <value>The complement parent.</value>
        public BaseOptionCollection SuperSetOptionCollection
        {
            get;
            private set;
        }
    }
}
