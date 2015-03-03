/*
Copyright 2010-2015 Mark Final

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
#include <Cocoa/Cocoa.h>
#include "library.h"

@interface MyApplicationDelegate : NSObject <NSApplicationDelegate, NSWindowDelegate>
{
    NSWindow* window;
}
@end

@implementation MyApplicationDelegate : NSObject
- (id)init
{
    if (self == [super init])
    {
        NSRect frame = NSMakeRect(0, 0, 200, 200);
        id menubar = [[NSMenu new] autorelease];
        id appMenuItem = [[NSMenuItem new] autorelease];
        id appMenu = [[NSMenu new] autorelease];
        id appName = [[NSProcessInfo processInfo] processName];
        id quitTitle = [@"Quit " stringByAppendingString:appName];
        id quitMenuItem = [[[NSMenuItem alloc] initWithTitle:quitTitle action:@selector(terminate:) keyEquivalent:@"q"] autorelease];

        window  = [[[NSWindow alloc] initWithContentRect:frame
                                     styleMask:(NSTitledWindowMask|NSClosableWindowMask|NSMiniaturizableWindowMask|NSResizableWindowMask)
                                     backing:NSBackingStoreBuffered /* supports GPU acceleration */
                                     defer:NO] autorelease];
        [window setBackgroundColor:[NSColor blueColor]];
        [window setTitle:[NSString stringWithUTF8String:getWindowTitle()]];
        [window center];

        [menubar addItem:appMenuItem];
        [NSApp setMainMenu:menubar];
        [appMenu addItem:quitMenuItem];
        [appMenuItem setSubmenu:appMenu];
    }
    return self;
}

- (void)applicationWillFinishLaunching:(NSNotification *)notification
{
    (void)notification;
    [window makeKeyAndOrderFront:self];
}

- (void)dealloc
{
    [super dealloc];
}

- (BOOL)applicationShouldTerminateAfterLastWindowClosed:(NSApplication *)theApplication {
    (void)theApplication;
    return YES;
}

@end

int main(int argc, char *argv[])
{
    NSAutoreleasePool *pool = [[NSAutoreleasePool alloc] init];
    NSApplication *application = [NSApplication sharedApplication];

    MyApplicationDelegate *appDelegate = [[[MyApplicationDelegate alloc] init] autorelease];
    (void)argc;
    (void)argv;

    [application setDelegate:appDelegate];
    [application run];

    [pool drain];

    return EXIT_SUCCESS;
}
