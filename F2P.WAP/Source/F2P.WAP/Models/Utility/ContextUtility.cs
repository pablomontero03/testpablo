using F2P.WAP.Models.DTO;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace F2P.WAP.Models.Utility
{
    public class ContextUtility
    {
        public static ContextObject ValidateService([CallerMemberName]string InvokeMember = "")
        {
            var prop = System.Web.HttpContext.Current;
            //prop.Server.MachineName
            //var remp = (RemoteEndpointMessageProperty)prop[RemoteEndpointMessageProperty.Name];
            //var hostEntry = Dns.GetHostEntry(remp.Address);
            //string hostName = hostEntry.HostName;
            //string a = remp.Address;

            //var baseAddress = OperationContext.Current.Host.BaseAddresses[0].Authority;
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            IPHostEntry ipHostInfo = Dns.GetHostEntry(prop.Server.MachineName); // `Dns.Resolve()` method is deprecated.
            IPAddress ipAddress = ipHostInfo.AddressList[1];

            var returnval = new ContextObject
            {
                AppName = InvokeMember,
                AppVersion = version,
                HostName = prop.Server.MachineName,
                BaseUrl = prop.ApplicationInstance.Request.Url.Authority,
                ClientIp = ipAddress.ToString()
            };

            return returnval;
        }
    }
}