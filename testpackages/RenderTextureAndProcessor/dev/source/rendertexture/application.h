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
