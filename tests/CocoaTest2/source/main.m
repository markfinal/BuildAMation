/*
Copyright (c) 2010-2019, Mark Final
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

@interface MyApplicationDelegate : NSObject<NSApplicationDelegate>
{
    NSWindow* window;
}
@end // MyApplicationDelegate

@implementation MyApplicationDelegate : NSObject
- (BOOL)applicationShouldTerminateAfterLastWindowClosed:(NSApplication *)theApplication
{
    return YES;
}
@end // MyApplicationDelegate

int main(int argc, char *argv[])
{
    NSAutoreleasePool *pool = [[NSAutoreleasePool alloc] init];
    NSApplication *application = [NSApplication sharedApplication];

    // add the quit menu to the application menu bar
    {
        id menubar = [[NSMenu new] autorelease];
        id appMenuItem = [[NSMenuItem new] autorelease];
        [menubar addItem:appMenuItem];
        [application setMainMenu:menubar];
        id appMenu = [[NSMenu new] autorelease];
        id appName = [[NSProcessInfo processInfo] processName];
        id quitTitle = [@"Quit " stringByAppendingString:appName];
        id quitMenuItem = [[[NSMenuItem alloc] initWithTitle:quitTitle action:@selector(terminate:) keyEquivalent:@"q"] autorelease];
        [appMenu addItem:quitMenuItem];
        [appMenuItem setSubmenu:appMenu];
    }

    // make the window
    {
        NSRect frameRect = NSMakeRect(0, 0, 640, 480);
        NSWindow *window = [[[NSWindow alloc] initWithContentRect:frameRect styleMask:NSWindowStyleMaskTitled|NSWindowStyleMaskClosable|NSWindowStyleMaskMiniaturizable|NSWindowStyleMaskResizable backing:NSBackingStoreBuffered defer:NO] autorelease];
        [window makeKeyAndOrderFront:nil];
        [window setBackgroundColor:[NSColor darkGrayColor]];
        [window setTitle:[NSString stringWithUTF8String:"CocoaTest2"]];
        [window center];

        // add a subview
        {
            NSRect viewRect = NSMakeRect(20, 20, 600, 240);
            NSView* view = [[[NSView alloc] initWithFrame:viewRect] autorelease];
            [view setWantsLayer:YES];
            [[view layer] setBackgroundColor:[[NSColor yellowColor] CGColor]];
            [[window contentView] addSubview:view];

            // add a text view
            {
                NSBundle *mainBundle = [NSBundle mainBundle];
                NSString *labelPath = [mainBundle pathForResource:@"label" ofType:@"txt"];
                NSString *labelContent = [NSString stringWithContentsOfFile:labelPath encoding:NSUTF8StringEncoding  error:NULL];

                id textField = [[NSTextField alloc] initWithFrame:NSMakeRect(10, 10, 580, 20)];
                [textField setStringValue:labelContent];
                [textField setBezeled:NO];
                [textField setDrawsBackground:YES];
                [textField setEditable:NO];
                [textField setSelectable:NO];
                [view addSubview:textField];
            }

            // add an image view
            {
                NSBundle *mainBundle = [NSBundle mainBundle];
                NSString *checkerboardPath = [mainBundle pathForResource:@"checkerboard" ofType:@"jpg"];
                NSImage *image = [[[NSImage alloc] initByReferencingFile:checkerboardPath] autorelease];

                id imageView = [[NSImageView alloc] initWithFrame:NSMakeRect(10, 40, 100, 100)];
                [imageView setImage:image];
                [view addSubview:imageView];
            }
        }
    }

    {
        MyApplicationDelegate *appDelegate = [[[MyApplicationDelegate alloc] init] autorelease];
        [application setDelegate:appDelegate];
        [application run];
    }

    [pool drain];

    return EXIT_SUCCESS;
}
