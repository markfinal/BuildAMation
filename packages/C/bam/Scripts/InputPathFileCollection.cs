#region License
// Copyright (c) 2010-2019, Mark Final
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
    /// <summary>
    /// Abstract class that is the base for all input modules that implement IInputPath
    /// </summary>
    /// <typeparam name="ChildModuleType"></typeparam>
    abstract class InputPathFileCollection<ChildModuleType> :
        CModuleCollection<ChildModuleType>
        where ChildModuleType : Bam.Core.Module, Bam.Core.IChildModule, Bam.Core.IInputPath, new()
    {
        /// <summary>
        /// Add a single child file, given the source path, to the collection. Path must resolve to a single file.
        /// </summary>
        /// <returns>The child file module, in order to manage patches.</returns>
        /// <param name="path">Path.</param>
        public ChildModuleType
        AddFile(
            Bam.Core.TokenizedString path)
        {
            // TODO: how can I distinguish between creating a child module that inherits it's parents settings
            // and from a standalone object of type ChildModuleType which should have it's own copy of the settings?
            var child = Bam.Core.Module.Create<ChildModuleType>(this);

            var inputPathModule = child as Bam.Core.IInputPath;
            inputPathModule.InputPath = path;

            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }

        public override ChildModuleType
        AddFile(
            string path,
            Bam.Core.Module macroModuleOverride = null,
            bool verbatim = false)
        {
            if (path.Contains('*'))
            {
                throw new Bam.Core.Exception(
                    $"Single path '{path}' cannot contain a wildcard character. Use AddFiles instead of AddFile"
                );
            }
            // TODO: how can I distinguish between creating a child module that inherits it's parents settings
            // and from a standalone object of type ChildModuleType which should have it's own copy of the settings?
            var child = Bam.Core.Module.Create<ChildModuleType>(this);
            if (verbatim)
            {
                child.InputPath = Bam.Core.TokenizedString.CreateVerbatim(path);
            }
            else
            {
                var macroModule = macroModuleOverride ?? this;
                child.InputPath = macroModule.CreateTokenizedString(path);
            }

            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }
    }
}
