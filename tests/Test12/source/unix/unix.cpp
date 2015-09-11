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
