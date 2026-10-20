using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Text;
using Bluey.Link;
namespace Bluey.Windows.AI;
internal sealed class ProtectedApiKey : IMasterKey
{
    private readonly string directory,file;private readonly byte[]? entropy;private readonly object gate=new();
    internal ProtectedApiKey(string? root=null,string provider="openai"){directory=root??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Bluey","AI");if(provider is not ("openai" or "gemini"))throw new ArgumentException("provider");file=Path.Combine(directory,provider+".dpapi");entropy=provider=="gemini"?Encoding.UTF8.GetBytes("Bluey/Gemini/API/v1"):null;}
    internal string Status {get{lock(gate){if(!File.Exists(file))return "Not configured";try{var key=Read();CryptographicOperations.ZeroMemory(key);return "Configured (never displayed)";}catch(Exception e)when(e is IOException or UnauthorizedAccessException or CryptographicException){return "Unavailable/corrupted — Replace or Remove explicitly";}}}}
    public byte[] Read(){lock(gate){var protectedBytes=File.ReadAllBytes(file);if(protectedBytes.Length>8192)throw new IOException("key size");var key=ProtectedData.Unprotect(protectedBytes,entropy,DataProtectionScope.CurrentUser);if(key.Length is <16 or >512){CryptographicOperations.ZeroMemory(key);throw new CryptographicException();}return key;}}
    internal void Set(byte[] bytes,bool replace){lock(gate){
        if(bytes.Length is <16 or >512||bytes.Any(b=>b<33||b>126))throw new IOException("Invalid credential format");
        if(File.Exists(file)&&!replace)throw new IOException("Use Replace");
        var dir=Directory.CreateDirectory(directory);var sid=WindowsIdentity.GetCurrent().User??throw new UnauthorizedAccessException();
        var acl=new DirectorySecurity();acl.SetAccessRuleProtection(true,false);acl.SetOwner(sid);acl.AddAccessRule(new FileSystemAccessRule(sid,FileSystemRights.FullControl,InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit,PropagationFlags.None,AccessControlType.Allow));dir.SetAccessControl(acl);
        var encoded=ProtectedData.Protect(bytes,entropy,DataProtectionScope.CurrentUser);try{File.WriteAllBytes(file+".new",encoded);File.Move(file+".new",file,true);}finally{CryptographicOperations.ZeroMemory(encoded);}
    }}
    internal void Remove(){lock(gate){File.Delete(file);File.Delete(file+".new");}}
}
