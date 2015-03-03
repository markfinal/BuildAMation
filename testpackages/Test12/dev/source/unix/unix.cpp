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
#include <X11/Xlib.h>
#include <stdio.h>
#include <unistd.h> // for sleep

Display *display = 0;
Window win;
Atom wmDeleteMessage;

int MyCreateWindow()
{
    display = XOpenDisplay(0);
    if (0 == display)
    {
        printf("Failed to open X display\n");
        return -1;
    }

    int blackColour = BlackPixel(display, DefaultScreen(display));
    //int whiteColour = WhitePixel(display, DefaultScreen(display));

    win = XCreateSimpleWindow(display, DefaultRootWindow(display), 0, 0, 100, 100, 0, blackColour, blackColour);

    XSelectInput(display, win, StructureNotifyMask);

    // register interest in the delete window message
    wmDeleteMessage = XInternAtom(display, "WM_DELETE_WINDOW", False);
    XSetWMProtocols(display, win, &wmDeleteMessage, 1);

    XMapWindow(display, win);
    for(;;)
    {
        XEvent e;
        XNextEvent(display, &e);
        if (e.type == MapNotify)
           break;
    }
    XFlush(display);

    return 0;
}

int MyWindowLoop()
{
    XEvent event;
    bool loop = true;
    while (loop)
    {
        XNextEvent(display, &event);
        switch (event.type)
        {
        case ClientMessage:
            if (event.xclient.data.l[0] == static_cast<int long>(wmDeleteMessage))
            {
                loop = false;
            }
            break;
        }
    }

    return 0;
}

void MyDestroyWindow()
{
    XDestroyWindow(display, win);
    XCloseDisplay(display);
    display = 0;
}
