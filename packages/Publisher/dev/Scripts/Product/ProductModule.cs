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
namespace Publisher
{
    [Bam.Core.ModuleToolAssignment(typeof(IPublishProductTool))]
    public abstract class ProductModule :
        Bam.Core.BaseModule,
        Bam.Core.IIdentifyExternalDependencies
    {
        public static readonly Bam.Core.LocationKey OSXAppBundle = new Bam.Core.LocationKey("OSXPrimaryApplicationBundle", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Bam.Core.LocationKey OSXAppBundleContents = new Bam.Core.LocationKey("OSXPrimaryAppBundleContents", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Bam.Core.LocationKey OSXAppBundleMacOS = new Bam.Core.LocationKey("OSXPrimaryAppBundleMacOS", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Bam.Core.LocationKey PublishDir = new Bam.Core.LocationKey("PrimaryModuleDirectory", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        #region IIdentifyExternalDependencies Members

        Bam.Core.TypeArray
        Bam.Core.IIdentifyExternalDependencies.IdentifyExternalDependencies(
            Bam.Core.Target target)
        {
            var dependentModuleTypes = new Bam.Core.TypeArray();

            var flags = System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic;
            var fields = this.GetType().GetFields(flags);
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(true);
                if (attributes.Length != 1)
                {
                    throw new Bam.Core.Exception("Found {0} attributes on field {1} of module {2}. Should be just one",
                                                  attributes.Length, field.Name, this.OwningNode.ModuleName);
                }

                var currentAttr = attributes[0];
                if (currentAttr.GetType() == typeof(PrimaryTargetAttribute))
                {
                    var fieldValue = field.GetValue(this) as System.Type;
                    if (null == fieldValue)
                    {
                        throw new Bam.Core.Exception("PrimaryTarget attribute field was not of type System.Type");
                    }
                    dependentModuleTypes.AddUnique(fieldValue);
                }
                else if (currentAttr.GetType() == typeof(OSXInfoPListAttribute))
                {
                    var fieldValue = field.GetValue(this) as System.Type;
                    if (null == fieldValue)
                    {
                        throw new Bam.Core.Exception("OSXInfoPList attribute field was not of type System.Type");
                    }
                    dependentModuleTypes.AddUnique(fieldValue);
                }
                else if (currentAttr.GetType() == typeof(AdditionalDirectoriesAttribute))
                {
                    // do nothing
                }
                else
                {
                    throw new Bam.Core.Exception("Unrecognized attribute of type {0} on field {1} of module {2}",
                                                  currentAttr.GetType().ToString(), field.Name, this.OwningNode.ModuleName);
                }
            }

            return dependentModuleTypes;
        }

        #endregion
    }
}
