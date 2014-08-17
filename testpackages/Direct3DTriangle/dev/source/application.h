/*
Copyright 2010-2014 Mark Final

This file is part of BuildAMation.

BuildAMation is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

BuildAMation is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
*/
#ifndef APPLICATION_H
#define APPLICATION_H

// forward declarations
class Renderer;

class Application
{
public:
    static Application *GetInstance();

    Application(int argc, char *argv[]);
    void SetWin32Instance(void *instance);
    int Run();

    void SetRenderer(Renderer *);
    Renderer *GetRenderer();

private:
    void RegisterWindowClass();
    void UnregisterWindowClass();

    void CreateMainWindow();
    void DestroyMainWindow();

    void MainLoop();

private:
    static Application *spInstance;
    Renderer *mpRenderer;
    void *mhWin32Instance;
    int mi32ExitCode;
};

#endif // APPLICATION_H
