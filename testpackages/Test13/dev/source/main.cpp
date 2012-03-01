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
