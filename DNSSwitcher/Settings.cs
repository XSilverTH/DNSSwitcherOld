using ProtoBuf;

namespace DNSSwitcher;

[ProtoContract]
public class Settings
{
    public Settings(bool OmGArEYoUaHuMaN)
    {
        DarkTheme = true;
        PersianLang = false;
        AutoUpdate = false;
    }

    public Settings()
    {
    }

    [ProtoMember(1)] public bool DarkTheme { get; set; } //TODO:Add light theme
    [ProtoMember(2)] public bool PersianLang { get; set; } //TODO:Add persian language 
    [ProtoMember(3)] public bool AutoUpdate { get; set; }
}