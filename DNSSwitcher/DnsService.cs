using System.Linq;
using System.Management;
using System.Net.NetworkInformation;

namespace DNSSwitcher;

public static class DnsService
{
    private static NetworkInterface GetActiveEthernetOrWifiNetworkInterface()
    {
        var Nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
            a => a.OperationalStatus == OperationalStatus.Up &&
                 (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                  a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                 a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));
        return Nic;
    }

    public static void SetDNS(Dns dns)
    {
        var networkInterface = GetActiveEthernetOrWifiNetworkInterface();
        if (networkInterface == null)
            return;
        foreach (ManagementObject instance in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            if ((bool)instance["IPEnabled"] && instance["Description"].ToString().Equals(networkInterface.Description))
            {
                var methodParameters = instance.GetMethodParameters("SetDNSServerSearchOrder");
                if (methodParameters != null)
                {
                    methodParameters["DNSServerSearchOrder"] = new[] { dns.Dns1, dns.Dns2 };
                    instance.InvokeMethod("SetDNSServerSearchOrder", methodParameters, null);
                }
            }
    }

    public static void ResetDNS()
    {
        var networkInterface = GetActiveEthernetOrWifiNetworkInterface();
        if (networkInterface == null)
            return;
        foreach (ManagementObject instance in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            if ((bool)instance["IPEnabled"] && instance["Description"].ToString().Equals(networkInterface.Description))
            {
                var methodParameters = instance.GetMethodParameters("SetDNSServerSearchOrder");
                if (methodParameters != null)
                {
                    methodParameters["DNSServerSearchOrder"] = null;
                    instance.InvokeMethod("SetDNSServerSearchOrder", methodParameters, null);
                }
            }
    }
}