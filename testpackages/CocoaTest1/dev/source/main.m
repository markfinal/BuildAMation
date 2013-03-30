#include <Cocoa/Cocoa.h>

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
        window  = [[[NSWindow alloc] initWithContentRect:frame
                                     styleMask:(NSTitledWindowMask|NSClosableWindowMask|NSMiniaturizableWindowMask|NSResizableWindowMask)
                                     backing:NSBackingStoreBuffered /* supports GPU acceleration */
                                     defer:NO] autorelease];
        [window setBackgroundColor:[NSColor blueColor]];
        [window setTitle:@"Hello world"];
        [window center];
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
    [window release];
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
