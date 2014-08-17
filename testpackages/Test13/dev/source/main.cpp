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
#include "myobject.h"
#include "myobject2.h"

#include <QtGui/QApplication>
#include <QtGui/QWidget>

#if defined(_WINDOWS)
#include <Windows.h>

int APIENTRY WinMain(
  HINSTANCE hInstance,
  HINSTANCE hPrevInstance,
  LPSTR lpCmdLine,
  int nCmdShow
)
#else
int main(int argc, char *argv[])
#endif
{
#if defined(_WINDOWS)
    QApplication app(__argc, __argv);
#else
    QApplication app(argc, argv);
#endif

    MyClass myClass;
    myClass.DoSomething();

    MyClass2 myClass2;
    myClass2.DoSomething();

    QWidget window;
    window.resize(250, 150);
    window.setWindowTitle("Simple example");
    window.show();

    return app.exec();
}
