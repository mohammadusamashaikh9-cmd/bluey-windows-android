using System.IO;
using System.Text.Json;
using Bluey.Core;
namespace Bluey.Windows;
internal static class SettingsStore
{
    private sealed record Saved(int Version, double Size, double PhonePosition, bool Glow, bool ShowCursor, bool FollowMouse, PointerTrail Trail, Mood Mood);
    private static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BlueyPort", "overlay-v1.json");
    internal static string? Warning { get; private set; }
    internal static CursorSettings Load()
    {
        var settings = new CursorSettings();
        try
        {
            if (!File.Exists(FilePath)) return settings;
            if (new FileInfo(FilePath).Length > 16384) throw new InvalidDataException("Settings file exceeds 16 KiB.");
            var saved = JsonSerializer.Deserialize<Saved>(File.ReadAllText(FilePath));
            if (saved is not { Version: 1 } || !double.IsFinite(saved.Size) || !double.IsFinite(saved.PhonePosition)) throw new InvalidDataException("Unsupported settings.");
            settings.Size = Math.Clamp(saved.Size, 48, 120); settings.PhonePosition = Math.Clamp(saved.PhonePosition, 0, 1);
            settings.Glow = saved.Glow; settings.ShowCursor = saved.ShowCursor; settings.FollowMouse = saved.FollowMouse;
            settings.Trail = Enum.IsDefined(saved.Trail) ? saved.Trail : PointerTrail.Comet;
            settings.Mood = Enum.IsDefined(saved.Mood) ? saved.Mood : Mood.Listening;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        { Warning = "Settings could not be read; using defaults. Existing file retained."; }
        return settings;
    }
    internal static bool Save(CursorSettings s)
    {
        try
        {
            var path = FilePath; Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var temp = path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(new Saved(1, s.Size, s.PhonePosition, s.Glow, s.ShowCursor, s.FollowMouse, s.Trail, s.Mood)));
            File.Move(temp, path, true); return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { return false; }
    }
}
