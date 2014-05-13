#ifndef MYOBJECT_H
#define MYOBJECT_H

#include <QtCore/QObject>

class MyClass : public QObject
{
    Q_OBJECT

public:
    MyClass(QObject *parent = 0);
    ~MyClass();

    void DoSomething();

signals:
    void mySignal();

public slots:
    void mySlot();
};

#endif // MYOBJECT_H
