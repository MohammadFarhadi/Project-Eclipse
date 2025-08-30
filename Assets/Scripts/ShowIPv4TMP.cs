using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;

public class ShowIPv4TMP : MonoBehaviour
{
    public TMP_Text ipText; // بکش توی Inspector به TextMeshPro UI

    void Start()
    {
        ipText.text = "IPv4 Address: " + GetLocalIPv4();
    }

    string GetLocalIPv4()
    {
        string localIP = "";
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) // فقط IPv4
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }
}