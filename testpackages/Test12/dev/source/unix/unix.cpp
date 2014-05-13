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
