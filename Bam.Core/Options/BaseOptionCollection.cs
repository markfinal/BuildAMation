#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Bam.Core
{
namespace V2
{
    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public sealed class SettingsExtensionsAttribute : System.Attribute
    {
        public SettingsExtensionsAttribute(System.Type extensionClass)
        {
            this.ClassType = extensionClass;
        }

        public System.Type ClassType
        {
            get;
            private set;
        }

        public System.Reflection.MethodInfo
        GetMethod(
            string name,
            params System.Type[] inputs)
        {
            return this.ClassType.GetMethod(name, inputs);
        }
    }

    public interface ISettingsBase
    {
    }

    /// <summary>
    /// Abstract base class for all settings
    /// </summary>
    public abstract class Settings
    {
    }
}
    public abstract class BaseOptionCollection :
        System.Collections.IEnumerable,
        System.ICloneable
    {
        protected System.Collections.Generic.Dictionary<string, Option> table = new System.Collections.Generic.Dictionary<string, Option>();

        public DependencyNode OwningNode
        {
            get;
            set;
        }

        protected
        BaseOptionCollection(
            DependencyNode owningNode)
        {
            this.OwningNode = owningNode;
            if (null != owningNode)
            {
                this.SetNodeSpecificData(owningNode);
            }
        }

        public void
        SetupNewOptions()
        {
            this.SetDefaultOptionValues(this.OwningNode);
            this.SetDelegates(this.OwningNode);
        }

        public void
        CopyExistingOptions(
            BaseOptionCollection other)
        {
            // TODO: can I use var here? check in Mono
            foreach (System.Collections.Generic.KeyValuePair<string, Option> option in other.table)
            {
                // TODO: this conditional is only present because an option collection has an output path in it's interface
                // this the IMocFile in QtCommon
                // when SetNodeSpecificData is called, the output path is set
                // when CopyExistingOptions is called, the option is already present in the target table so that Add() will fail
                // since SetDelegates has not been called on a child, the PrivateData is not set, so needs copying.
                if (!this.table.ContainsKey(option.Key))
                {
                    this.table.Add(option.Key, option.Value.Clone() as Option);
                }
                else
                {
                    this.table[option.Key].PrivateData = option.Value.PrivateData;
                }
            }
        }

        protected abstract void
        SetDefaultOptionValues(
            DependencyNode owningNode);

        protected abstract void
        SetDelegates(
            DependencyNode owningNode);

        protected virtual void
        SetNodeSpecificData(
            DependencyNode node)
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

        public virtual object
        Clone()
        {
            var optionsType = this.GetType();
            var clonedOptions = OptionCollectionFactory.CreateOptionCollection(optionsType);
            clonedOptions.CopyExistingOptions(this);
            return clonedOptions;
        }

        private void
        InvokeSetHandler(
            System.Reflection.MethodInfo setHandler,
            Option option)
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

        public void
        ProcessNamedSetHandler(
            string setHandlerName,
            Option option)
        {
            var type = this.GetType();
            var bindingFlags = System.Reflection.BindingFlags.Static |           // don't need an instance
                               System.Reflection.BindingFlags.NonPublic |        // generally hidden - should be protected
                               System.Reflection.BindingFlags.FlattenHierarchy;  // bring in protected static functions
            InvokeSetHandler(type.GetMethod(setHandlerName, bindingFlags), option);
        }

        public virtual void
        FinalizeOptions(
            DependencyNode node)
        {
            // do nothing
        }

        public System.Collections.IEnumerator
        GetEnumerator()
        {
            return this.table.GetEnumerator();
        }

        public bool
        Contains(
            string key)
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

        protected Type
        GetValueTypeOption<Type>(
            string optionName) where Type : struct
        {
            return (this[optionName] as Core.ValueTypeOption<Type>).Value;
        }

        protected Type
        GetValueTypeOption<Type>(
            string optionName,
            BaseOptionCollection superSetOptionCollection) where Type : struct
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

        protected void
        SetValueTypeOption<Type>(
            string optionName,
            Type value) where Type : struct
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

        protected Type
        GetReferenceTypeOption<Type>(
            string optionName) where Type : class
        {
            return (this[optionName] as Core.ReferenceTypeOption<Type>).Value;
        }

        protected Type
        GetReferenceTypeOption<Type>(
            string optionName,
            BaseOptionCollection superSetOptionCollection) where Type : class
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

        protected void
        SetReferenceTypeOption<Type>(
            string optionName,
            Type value) where Type : class
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

        public override bool
        Equals(object obj)
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

        public override int
        GetHashCode()
        {
            return base.GetHashCode();
        }

        public BaseOptionCollection
        Complement(
            BaseOptionCollection other)
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

        public BaseOptionCollection
        Intersect(
            BaseOptionCollection other)
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

        /// <summary>
        /// Is the module, owned by the option collection, of the specified type?
        /// </summary>
        /// <returns><c>true</c> if this instance is of type ModuleType; otherwise, <c>false</c>.</returns>
        /// <typeparam name="ModuleType">The module type to compare against</typeparam>
        public bool IsFromModuleType<ModuleType>()
        {
            var matchingType = this.OwningNode.Module is ModuleType;
            return matchingType;
        }

        /// <summary>
        /// Returns the location defined by the key, for the module owned by the option collection.
        /// </summary>
        /// <returns><c>Location</c>, of the module for the location key. Exception will be thrown if the key does not exist.</returns>
        /// <param name="locationKey">Location key.</param>
        public Location GetModuleLocation(LocationKey locationKey)
        {
            return this.OwningNode.Module.Locations[locationKey];
        }

        /// <summary>
        /// Returns the location defined by the key, for the module owned by the option collection, if it exists.
        /// </summary>
        /// <returns><c>Location</c>, if the module defines the location key, <c>null</c> otherwise.</returns>
        /// <param name="locationKey">Location key.</param>
        public Location GetModuleLocationSafe(LocationKey locationKey)
        {
            var containsLocation = this.OwningNode.Module.Locations.Contains(locationKey);
            if (containsLocation)
            {
                return this.GetModuleLocation(locationKey);
            }
            return null;
        }
    }
}
