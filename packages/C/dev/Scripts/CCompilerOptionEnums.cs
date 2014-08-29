#region License
// Copyright 2010-2014 Mark Final
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
namespace C
{
    public enum EOptimization
    {
        Off = 0,
        Size = 1,
        Speed = 2,
        Full = 3,
        Custom = 4 // TODO: confirm
    }

    public enum ECompilerOutput
    {
        CompileOnly = 0,
        Preprocess = 1
    }

    public enum ETargetLanguage
    {
        Default = 0,
        C = 1,
        Cxx = 2,
        ObjectiveC = 3,
        ObjectiveCxx = 4
    }

    public enum ECharacterSet
    {
        NotSet = 0,
        Unicode = 1,
        MultiByte = 2
    }

    public enum ELanguageStandard
    {
        NotSet,
        C89,
        C99,
        Cxx98,
        Cxx11
    }
}
