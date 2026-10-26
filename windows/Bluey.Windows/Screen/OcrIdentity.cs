using System.IO;
using System.Runtime.InteropServices;
using Bluey.Screen;
namespace Bluey.Windows.Screen;
// Only the separately built developer variant requires identity. No registration or data writes.
internal static class OcrIdentity
{
    internal const string Name="Bluey.Port.OcrDev",Publisher="CN=Bluey OCR Development";
    internal static void Require()
    {
        if(!NativeCapture.HasPackageIdentity)throw new ScreenException("ocr-identity-not-registered");
        var id=global::Windows.ApplicationModel.Package.Current.Id;
        if(id.Name!=Name||id.Publisher!=Publisher)throw new ScreenException("ocr-identity-mismatch");
        var folder=new Guid("F1B32785-6FBA-4FCF-9D55-7B8E7F157091");nint path=0;
        try {
            Marshal.ThrowExceptionForHR(SHGetKnownFolderPath(in folder,0x10000,0,out path));
            RequireDataPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),Marshal.PtrToStringUni(path)??"");
        } finally {if(path!=0)Marshal.FreeCoTaskMem(path);}
    }
    internal static void RequireDataPath(string actual,string original)
    {
        if(string.IsNullOrWhiteSpace(actual)||string.IsNullOrWhiteSpace(original)||!string.Equals(Path.TrimEndingDirectorySeparator(Path.GetFullPath(actual)),Path.TrimEndingDirectorySeparator(Path.GetFullPath(original)),StringComparison.OrdinalIgnoreCase))throw new ScreenException("ocr-data-path-redirected");
    }
    [DllImport("shell32.dll")]private static extern int SHGetKnownFolderPath(in Guid folder,uint flags,nint token,out nint path);
}
