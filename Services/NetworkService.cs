using System.Net.NetworkInformation;

namespace AMANC_Inventory.Services
{
    public static class NetworkService
    {
        public static bool HasInternetConnection()
        {
            try
            {
                using (var ping = new Ping())
                {
                    var result = ping.Send("8.8.8.8", 2000);
                    return result.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}