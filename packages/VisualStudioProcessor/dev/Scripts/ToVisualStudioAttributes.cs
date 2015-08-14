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
namespace VisualStudioProcessor
{
    public static class ToVisualStudioAttributes
    {
        public static VisualStudioProcessor.ToolAttributeDictionary
        Execute(
            object sender,
            Bam.Core.Target target,
            EVisualStudioTarget vsTarget)
        {
            var optionCollection = sender as Bam.Core.BaseOptionCollection;

            var optionsDictionary = new VisualStudioProcessor.ToolAttributeDictionary();

            // TODO: can I use a var here? especially on Mono
            foreach (System.Collections.Generic.KeyValuePair<string, Bam.Core.Option> optionKeyValue in optionCollection)
            {
                var optionName = optionKeyValue.Key;
                var option = optionKeyValue.Value;

                if (null == option.PrivateData)
                {
                    // state only
                    continue;
                }

                var data = option.PrivateData as IVisualStudioDelegate;
                if (null == data)
                {
                    throw new Bam.Core.Exception("Option data for '{0}', of type '{1}', does not implement the interface '{2}'", optionName, option.PrivateData.GetType().ToString(), typeof(IVisualStudioDelegate).ToString());
                }

                var visualStudioDelegate = data.VisualStudioProjectDelegate;
                if (null != visualStudioDelegate)
                {
                    if (null != visualStudioDelegate.Target)
                    {
                        // Not a requirement, but just a check
                        throw new Bam.Core.Exception("Delegate for '{0}' should be static", optionName);
                    }

                    var dictionary = data.VisualStudioProjectDelegate(optionCollection, option, target, vsTarget);
                    if (null != dictionary)
                    {
                        optionsDictionary.Merge(dictionary);
                    }
                }
            }

            return optionsDictionary;
        }
    }
}
