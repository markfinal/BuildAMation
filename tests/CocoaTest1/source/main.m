/*
Copyright (c) 2010-2016, Mark Final
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of BuildAMation nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
