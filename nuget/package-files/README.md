# Prebuilt platform package files

The final NuGet package is built on macOS, but macOS cannot compile the Windows
target framework. Run the following command on Windows before the Mac release:

```powershell
.\scripts\build-windows-package-files.ps1 -Version 10.0.4
```

Commit the resulting DLL and XML file under
`lib/net10.0-windows10.0.19041.0`. The macOS publish script includes those files
in the final `Plugin.InAppBilling.Extended` package and refuses to publish a
package that does not contain the Windows assembly.
