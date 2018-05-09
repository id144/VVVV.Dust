#region usings
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "IP", Category = "Network", Version = "Advanced", Help = "Extended IP node, get multicast info", Tags = "c#",Author = "id144")]
	#endregion PluginInfo
	public class AdvancedNetworkIPNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Update", IsBang = true, IsSingle = true)]
		public ISpread<bool> FInput;

        [Output("Name")]
        public ISpread<string> FOutputName;

        [Output("Description")]
		public ISpread<string> FOutputDescription;

		[Output("IP", StringType = StringType.IP )]
		public ISpread<string> FOutputIP;

        [Output("DHCP Enabled")]
        public ISpread<bool> FOutputDHCPEnabled;

        [Output("Standard Gateway")]
        public ISpread<string> FOutputGateway;

      //  [Output("Subnet Mask")]
     //   public ISpread<string> FOutputSubnetMask;

        [Output("Multicast IP", StringType = StringType.IP)]
        public ISpread<string> FOutputMulticastIP;

        [Output("Wireless")]
        public ISpread<bool> FOutputWireless;

        [Output("Multicast")]
        public ISpread<bool> FOutputMulticast;

        [Output("Speed")]
        public ISpread<long> FOutputSpeed;

        [Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
        {
            FOutputName.SliceCount = 0;
            FOutputDescription.SliceCount = 0;
            FOutputIP.SliceCount = 0;
            FOutputMulticastIP.SliceCount = 0;
            FOutputWireless.SliceCount = 0;
            FOutputMulticast.SliceCount = 0;
            FOutputSpeed.SliceCount = 0;
            FOutputDHCPEnabled.SliceCount = 0;
            FOutputGateway.SliceCount = 0;
        //    FOutputSubnetMask.SliceCount = 0;

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (var adapter in interfaces)
			{
			    var ipProps = adapter.GetIPProperties();
                
			    foreach (var ip in ipProps.UnicastAddresses)
			    {
                    if ((adapter.OperationalStatus == OperationalStatus.Up)
                    && (ip.Address.AddressFamily == AddressFamily.InterNetwork))
                    {                        
                        FOutputName.Add(adapter.Name.ToString());
                        FOutputDescription.Add(adapter.Description.ToString());
                        FOutputIP.Add(ip.Address.ToString());
                        FOutputWireless.Add(adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);
                        FOutputMulticast.Add(adapter.SupportsMulticast);
                        FOutputSpeed.Add((long)adapter.Speed);
                        

                        //generate or get multicast IPv4 address
                        byte[] b_ipAddress = ip.Address.GetAddressBytes();
                        if (b_ipAddress.Length == 4) b_ipAddress[3] = 255;

                        IPAddress multicastIP = new IPAddress(b_ipAddress);

                        foreach (var ipMulticast in ipProps.MulticastAddresses)
                        {
                            if (ipMulticast.Address.AddressFamily == AddressFamily.InterNetwork) multicastIP = ipMulticast.Address;
                        }
                        FOutputMulticastIP.Add(multicastIP.ToString());

                        string x = IPAddress.Broadcast.ToString();
                        
                        //return first gateway
                        IPAddress gatewayIP = new IPAddress(0);

                        if (ipProps.GatewayAddresses.Count > 0)
                        {
                            gatewayIP = ipProps.GatewayAddresses[0].Address;
                        };
                        FOutputGateway.Add(gatewayIP.ToString()); 

                        IPAddressCollection addresses = ipProps.DhcpServerAddresses;

                        IPv4InterfaceProperties _ipv4_props = ipProps.GetIPv4Properties();
                        FOutputDHCPEnabled.Add(_ipv4_props.IsDhcpEnabled);
                    }
			    }
			}
		}
	}
}
