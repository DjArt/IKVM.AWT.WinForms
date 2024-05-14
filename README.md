## Usage
Add reference to IKVM.AWT.WinForms project and code below:
```
using ikvm.runtime;

using IKVM.AWT.WinForms;

using java.lang;

using System;
using System.Runtime.CompilerServices;

namespace PROJECT_NAMESPACE;

internal static class Module
{
    [ModuleInitializer]
    public static void Initialize()
    {
        Type geType = typeof(NetGraphicsEnvironment);
        Class geClass = geType;
        Startup.addBootClassPathAssembly(geType.Assembly);
        java.lang.System.setProperty("java.awt.graphicsenv", geClass.getName());
    }
}
```
