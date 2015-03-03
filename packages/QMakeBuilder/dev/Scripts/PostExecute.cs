#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder :
        Bam.Core.IBuilderPostExecute
    {
        #region IBuilderPostExecute Members

        void
        Bam.Core.IBuilderPostExecute.PostExecute(
            Bam.Core.DependencyNodeCollection executedNodes)
        {
            Bam.Core.Log.DebugMessage("PostExecute for QMakeBuilder");

            if (0 == executedNodes.Count)
            {
                Bam.Core.Log.Info("No QMake pro file written as there were no targets generated");
                return;
            }

            // find all nodes with the same unique name
            var similarNodes = new System.Collections.Generic.Dictionary<string, Bam.Core.Array<QMakeData>>();
            foreach (var node in executedNodes)
            {
                if (null == node.Data)
                {
                    Bam.Core.Log.DebugMessage("*** Null data for node {0}", node.UniqueModuleName);
                    continue;
                }

                if (similarNodes.ContainsKey(node.UniqueModuleName))
                {
                    similarNodes[node.UniqueModuleName].Add(node.Data as QMakeData);
                }
                else
                {
                    similarNodes[node.UniqueModuleName] = new Bam.Core.Array<QMakeData>(node.Data as QMakeData);
                }
            }

            foreach (var keyPair in similarNodes)
            {
                Bam.Core.Log.DebugMessage("{0} : {1} nodes", keyPair.Key, keyPair.Value.Count);
                QMakeData.Write(keyPair.Value);
            }

            var mainPackage = Bam.Core.State.PackageInfo[0];
            var proFileName = mainPackage + ".pro";
            var rootDirectory = Bam.Core.State.BuildRoot;
            var proFilePath = System.IO.Path.Combine(rootDirectory, proFileName);

            // relative paths need a trailing slash to work
            rootDirectory += System.IO.Path.DirectorySeparatorChar;

            using (var proWriter = new System.IO.StreamWriter(proFilePath))
            {
                proWriter.WriteLine("# -- Generated by BuildAMation --");
                proWriter.WriteLine("TEMPLATE = subdirs");
                proWriter.WriteLine("CONFIG += ordered");
                proWriter.WriteLine("SUBDIRS += \\");

                foreach (var collection in similarNodes.Values)
                {
                    var data = collection[0];
                    if (data.ProFilePath != null)
                    {
                        var subDirProjectDir = System.IO.Path.GetDirectoryName(data.ProFilePath) + System.IO.Path.DirectorySeparatorChar;
                        var relativeDir = Bam.Core.RelativePathUtilities.GetPath(subDirProjectDir, rootDirectory);
                        relativeDir = relativeDir.TrimEnd(System.IO.Path.DirectorySeparatorChar);
                        proWriter.WriteLine("\t{0}\\", relativeDir.Replace('\\', '/'));
                    }
                }
            }

            Bam.Core.Log.Info("Successfully created QMake .pro file for package '{0}'\n\t{1}", Bam.Core.State.PackageInfo[0].Name, proFilePath);
        }

        #endregion
    }
}
