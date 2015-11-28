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
    /// <summary>
    /// Enumeration for the timing profiles for Bam core operations
    /// </summary>
    public enum ETimingProfiles
    {
        /// <summary>
        /// Command line option processing time
        /// </summary>
        ProcessCommandLine = 0,

        /// <summary>
        /// Time to gather all package source
        /// </summary>
        GatherSource,

        /// <summary>
        /// Time to compile all the package source, and referenced assemblies
        /// </summary>
        AssemblyCompilation,

        /// <summary>
        /// Time to load the compiled package assembly.
        /// </summary>
        LoadAssembly,

        /// <summary>
        /// Time to generate the meta data for all packages.
        /// </summary>
        PackageMetaData,

        /// <summary>
        /// Time to identify each of the top-level buildable modules.
        /// </summary>
        IdentifyBuildableModules,

        /// <summary>
        /// Time to populate the remainder of the graph, and sort modules into their correct ranks to satisfy dependencies.
        /// Any modules with an associated tool has their default settings class created, and configured.
        /// </summary>
        PopulateGraph,

        /// <summary>
        /// Time to validate the graph, to ensure modules are in the expected ranks, no cyclic dependencies exist etc.
        /// </summary>
        ValidateGraph,

        /// <summary>
        /// Time to create and evaluate settings patches on any module with a tool.
        /// </summary>
        CreatePatches,

        /// <summary>
        /// Time to parse all TokenizedStrings.
        /// </summary>
        ParseTokenizedStrings,

        /// <summary>
        /// Time to execute the generated dependency graph in the chosen build mode.
        /// </summary>
        GraphExecution,

        /// <summary>
        /// Total execution time.
        /// </summary>
        TimedTotal
    }
}
