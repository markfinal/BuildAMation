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
#include "application.h"
#include "common.h"

#include <Windows.h>

int APIENTRY WinMain(HINSTANCE hInstance, HINSTANCE UNUSEDARG(hPrevInstance), LPSTR UNUSEDARG(lpCmdLine), int UNUSEDARG(nShowCmd))
{
    Application app(__argc, __argv);
    app.SetWin32Instance(hInstance);
    int exitCode = app.Run();
    return exitCode;
}
