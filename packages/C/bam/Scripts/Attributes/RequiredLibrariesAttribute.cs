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
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public sealed class RequiredLibrariesAttribute :
        Bam.Core.BaseTargetFilteredAttribute,
        Bam.Core.IFieldAttributeProcessor
    {
        void
        Bam.Core.IFieldAttributeProcessor.Execute(
            object sender,
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            if (!Bam.Core.TargetUtilities.MatchFilters(target, this))
            {
                return;
            }

            var field = sender as System.Reflection.FieldInfo;
            string[] libraries = null;
            if (field.GetValue(module) is Bam.Core.Array<string>)
            {
                libraries = (field.GetValue(module) as Bam.Core.Array<string>).ToArray();
            }
            else if (field.GetValue(module) is string[])
            {
                libraries = field.GetValue(module) as string[];
            }
            else
            {
                throw new Bam.Core.Exception("RequiredLibrary field in {0} must be of type string[], Bam.Core.StringArray or Bam.Core.Array<string>", module.ToString());
            }

            module.UpdateOptions += delegate(Bam.Core.IModule dlgModule, Bam.Core.Target dlgTarget)
            {
                var linkerOptions = dlgModule.Options as ILinkerOptions;
                foreach (var library in libraries)
                {
                    linkerOptions.Libraries.Add(library);
                }
            };
        }
    }
}
