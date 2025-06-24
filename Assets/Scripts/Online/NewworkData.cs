// Scripts/NetworkInfo.cs
using System.Net;
using System.Net.Sockets;

public static class NetworkInfo
{
    public static string HostIP = "Unknown";

    public static void DetectHostIP()
    {
        foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                HostIP = ip.ToString();
                break;
            }
        }
    }
}