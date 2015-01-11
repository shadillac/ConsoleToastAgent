using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;

namespace ConsoleToast
{
    public class PushItems
    {
        public string Id { get; set; }
        public string usrname { get; set; }
        public string channeluri { get; set; }
        public string gameid { get; set; }
    }

    class Program
    {
        static IMobileServiceTable<PushItems> PushTable;
        static MobileServiceCollection<PushItems, PushItems> pushdata;
        //static string token = "";

        static void Main(string[] args)
        {

            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            try
            {
                MobileServiceClient MobileService = new MobileServiceClient(
                "https://topcheddarwinphone.azure-mobile.net/",
                "LXtralTsDVWKwaJDJpsbFiQOYWHbgB13"
                );

                PushTable = MobileService.GetTable<PushItems>();

                pushdata = await PushTable
                    //.Where(Item => Item.usrname == username)
                    .ToCollectionAsync();

                /////////////// GET GAMES FOR TODAY
                //WebClient wc = new WebClient();
                //wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                //wc.UploadStringCompleted += wc_UploadStringCompleted;
                //string parameters = "username=shadillac&password=Niggaplease9&key=9e1a8b1978ef122c171e471f15419c9f";
                //wc.UploadStringAsync(new Uri("https://api.hockeystreams.com/Login?"), "POST", parameters);
 

                HttpClient hc = new HttpClient();
                string jsonResp = await hc.GetStringAsync("https://api.hockeystreams.com/GetLive?date=" + DateTime.Now.ToString("MM/dd/yyyy") + "&token=dVdwSDUxbU14NVZNWDZReVoyN2RTcVlDdWhlYStzcFBGMllTbUlYV2ZSN0dUTmd4Uk52dmRPNGdsN0FoUnhvaHgyVHAxVk5YMWJNUWM5M3paTy9MM0FCTU5NTStxUm00SE5wNm12NUVhRFE9");
                JObject o = new JObject();
                try
                {
                    o = JObject.Parse(jsonResp);
                }
                catch (Exception)//System.Reflection.TargetInvocationException)
                {
                    Console.WriteLine("No Live games found for this day.");
                }
                //////////////////////////////////////////////////////
                foreach (JToken game in o["schedule"])
                {
                    
                    foreach (PushItems pdata in pushdata)
                    {
                        if ((pdata.gameid == game["id"].ToString()) && (game["isPlaying"].ToString() == "1"))
                        {
                            // Get the URI that the Microsoft Push Notification Service returns to the push client when creating a notification channel.
                            // Normally, a web service would listen for URIs coming from the web client and maintain a list of URIs to send
                            // notifications out to.
                            string subscriptionUri = pdata.channeluri.ToString();
                            HttpWebRequest sendNotificationRequest = (HttpWebRequest)WebRequest.Create(subscriptionUri);

                            // Create an HTTPWebRequest that posts the toast notification to the Microsoft Push Notification Service.
                            // HTTP POST is the only method allowed to send the notification.
                            sendNotificationRequest.Method = "POST";

                            // The optional custom header X-MessageID uniquely identifies a notification message. 
                            // If it is present, the same value is returned in the notification response. It must be a string that contains a UUID.
                            // sendNotificationRequest.Headers.Add("X-MessageID", "<UUID>");

                            // Create the toast message.
                            string toastMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                            "<wp:Notification xmlns:wp=\"WPNotification\">" +
                                "<wp:Toast>" +
                                    "<wp:Text1>" + "Game Alert:" + "</wp:Text1>" +
                                    "<wp:Text2>" + "The " + game["awayTeam"].ToString() + " / " + game["homeTeam"].ToString() +" game has started." + "</wp:Text2>" +
                                    "<wp:Param>/Splash.xaml?NavigatedFrom=Toast Notification</wp:Param>" +
                                "</wp:Toast> " +
                            "</wp:Notification>";

                            // Set the notification payload to send.
                            byte[] notificationMessage = Encoding.Default.GetBytes(toastMessage);

                            // Set the web request content length.
                            sendNotificationRequest.ContentLength = notificationMessage.Length;
                            sendNotificationRequest.ContentType = "text/xml";
                            sendNotificationRequest.Headers.Add("X-WindowsPhone-Target", "toast");
                            sendNotificationRequest.Headers.Add("X-NotificationClass", "2");


                            using (Stream requestStream = sendNotificationRequest.GetRequestStream())
                            {
                                requestStream.Write(notificationMessage, 0, notificationMessage.Length);
                            }

                            // Send the notification and get the response.
                            HttpWebResponse response = (HttpWebResponse)sendNotificationRequest.GetResponse();
                            string notificationStatus = response.Headers["X-NotificationStatus"];
                            string notificationChannelStatus = response.Headers["X-SubscriptionStatus"];
                            string deviceConnectionStatus = response.Headers["X-DeviceConnectionStatus"];

                            // Display the response from the Microsoft Push Notification Service.  
                            // Normally, error handling code would be here. In the real world, because data connections are not always available,
                            // notifications may need to be throttled back if the device cannot be reached.
                            Console.WriteLine(notificationStatus + " | " + deviceConnectionStatus + " | " + notificationChannelStatus);

                            //delete the row after toasting.
                            await PushTable.DeleteAsync(pdata);
                        }
                    }
                }
           
            }        
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //static void wc_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        //{
        //    JObject o = new JObject();
        //    try
        //    {
        //        o = JObject.Parse(e.Result);
        //    }
        //    catch (System.Reflection.TargetInvocationException)
        //    {

        //    }

        //    token = o["token"].ToString();
        //}        
    }
}
