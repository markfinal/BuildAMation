#include "myobject.h"

#include <QtGui/QApplication>
#include <QtGui/QWidget>

int main(int argc, char *argv[])
{
    QApplication app(argc, argv);

    MyClass myClass;
    myClass.DoSomething();

    QWidget window;
    window.resize(250, 150);
    window.setWindowTitle("Simple example");
    window.show();

    return app.exec();
}
