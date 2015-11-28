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
    /// Static utility class representing all statistics for the build
    /// </summary>
    public static class Statistics
    {
        private static double
        BytesToMegaBytes(
            long bytes)
        {
            return bytes / 1024.0 / 1024.0;
        }

        /// <summary>
        /// Display the statistics for the build.
        /// </summary>
        public static void
        Display()
        {
            Log.Info("\nBuildAMation Statistics");
            Log.Info("Memory Usage");
            Log.Info("Peak working set size : {0:N2}MB", BytesToMegaBytes(System.Diagnostics.Process.GetCurrentProcess().PeakWorkingSet64));
            Log.Info("Peak virtual size     : {0:N2}MB", BytesToMegaBytes(System.Diagnostics.Process.GetCurrentProcess().PeakVirtualMemorySize64));
            Log.Info("GC total memory       : {0:N2}MB (after GC, {1:N2}MB)", BytesToMegaBytes(System.GC.GetTotalMemory(false)), BytesToMegaBytes(System.GC.GetTotalMemory(true)));
            Log.Info("\nObject counts");
            Log.Info("Tokenized strings     : {0} ({1} unshared)", TokenizedString.Count, TokenizedString.UnsharedCount);
            TokenizedString.DumpCache();
            Log.Info("Modules               : {0}", Module.Count);
            TimingProfileUtilities.DumpProfiles();
        }
    }
}
