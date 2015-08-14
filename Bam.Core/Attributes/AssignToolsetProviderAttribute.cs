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
    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = true)]
    public class AssignToolsetProviderAttribute :
        System.Attribute
    {
        private delegate string ProviderDelegate(System.Type toolType);

        private string toolsetName;
        private ProviderDelegate providerFn;

        public
        AssignToolsetProviderAttribute(
            string toolsetName)
        {
            this.toolsetName = toolsetName;
        }

        public
        AssignToolsetProviderAttribute(
            System.Type providerClass,
            string methodName)
        {
            var flags = System.Reflection.BindingFlags.Static |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic;
            var method = providerClass.GetMethod(methodName, flags);
            if (null == method)
            {
                throw new Exception("Unable to locate a static method called '{0}' in class '{1}'", methodName, providerClass.ToString());
            }
            var dlg = System.Delegate.CreateDelegate(typeof(ProviderDelegate), method, false);
            if (null == dlg)
            {
                throw new Exception("Unable to match method '{0}' in class '{1}' to the delegate 'string fn(System.Type)'", method, providerClass.ToString());
            }
            this.providerFn = dlg as ProviderDelegate;
        }

        public string
        ToolsetName(
            System.Type toolType)
        {
            if (null == this.providerFn)
            {
                return this.toolsetName;
            }
            else
            {
                var toolsetName = this.providerFn.Method.Invoke(null, new object[] { toolType }) as string;
                return toolsetName;
            }
        }
    }
}
