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
#define C_UNUSED(_arg) (void)_arg

extern int MyCreateWindow();
extern int MyWindowLoop();
extern void MyDestroyWindow();

int Main()
{
    if (MyCreateWindow() < 0)
    {
        return -1;
    }
    int returnValue = MyWindowLoop();
    MyDestroyWindow();
    return returnValue;
}

#if defined(_WIN32)
#include <Windows.h>

HINSTANCE gInstance = 0;

int APIENTRY WinMain(
  HINSTANCE hInstance,
  HINSTANCE hPrevInstance,
  LPSTR lpCmdLine,
  int nCmdShow
)
{
    C_UNUSED(hPrevInstance);
    C_UNUSED(lpCmdLine);
    C_UNUSED(nCmdShow);

    gInstance = hInstance;
#else // defined(_WIN32)
int main()
{
#endif // defined(_WIN32)
    int returnValue = Main();
    return returnValue;
}
