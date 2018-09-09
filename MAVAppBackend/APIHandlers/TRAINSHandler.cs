using Newtonsoft.Json.Linq;

namespace MAVAppBackend.APIHandlers
{
    public class TRAINSHandler
    {
        private JArray trainArray;
        public TRAINSHandler(JObject apiResponse)
        {
            if (apiResponse == null) return;

            trainArray = apiResponse["d"]["result"]["Trains"]["Train"] as JArray;
        }

        public void UpdateDatabase()
        {
            
        }
    }
}
