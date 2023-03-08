using ProtoBuf;

namespace DNSSwitcher;

[ProtoContract]
public class Dns
{
    public Dns(string name, string dns1, string dns2)
    {
        Name = name;
        Dns1 = dns1;
        Dns2 = dns2;
    }

    // public Dns(string name, string dns1, string dns2, string? appLocation, string? appUrl)
    // {
    //     Name = name;
    //     Dns1 = dns1;
    //     Dns2 = dns2;
    //     AppLocation = appLocation;
    //     AppUrl = appUrl;
    // }
    public Dns()
    {
    }

    [ProtoMember(1)] public string Name { get; set; }

    [ProtoMember(2)] public string Dns1 { get; set; }

    [ProtoMember(3)] public string Dns2 { get; set; }

    // [ProtoMember(4)] public string? AppLocation { get; set; } TODO:AppOpening
    // [ProtoMember(5)] public string? AppUrl { get; set; }
}