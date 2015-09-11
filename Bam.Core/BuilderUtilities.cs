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
using System.Linq;
namespace Bam.Core
{
    public static class BuilderUtilities
    {
        public static bool
        IsBuilderPackage(
            string packageName)
        {
            return packageName.EndsWith("Builder");
        }

        public static void
        SetBuilderPackage()
        {
            if (null == State.BuildMode)
            {
                return;
            }

            var builderPackageName = System.String.Format("{0}Builder", State.BuildMode);
#if true
            var builderPackage = V2.Graph.Instance.Packages.Where(item => item.Name == builderPackageName).FirstOrDefault();
            if (null != builderPackage)
            {
                return;
            }
#else
            foreach (var package in State.PackageInfo)
            {
                if (builderPackageName == package.Name)
                {
                    package.IsBuilder = true;
                    State.BuilderPackage = package;
                    return;
                }
            }
#endif

            throw new Exception("Builder package '{0}' was not specified as a dependency", builderPackageName);
        }

        public static void
        CreateBuilderInstance()
        {
            if (null == State.BuildMode)
            {
                throw new Exception("Name of the Builder has not been specified");
            }

            if (null == State.ScriptAssembly)
            {
                throw new Exception("Script assembly has not been set");
            }

            IBuilder builderInstance = null;
            var attributes = State.ScriptAssembly.GetCustomAttributes(typeof(DeclareBuilderAttribute), false) as DeclareBuilderAttribute[];
            foreach (var attribute in attributes)
            {
                if (attribute.Name == State.BuildMode)
                {
                    builderInstance = BuilderFactory.CreateBuilder(attribute.Type);
                    break;
                }
            }
            if (null == builderInstance)
            {
                throw new Exception("Unsupported builder '{0}'. Please double check the spelling as the name is case sensitive", State.BuildMode);
            }

            State.BuilderInstance = builderInstance;
        }
    }
}
