using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleToast
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Get the URI that the Microsoft Push Notification Service returns to the push client when creating a notification channel.
                // Normally, a web service would listen for URIs coming from the web client and maintain a list of URIs to send
                // notifications out to.
                string subscriptionUri = "http://s.notify.live.net/u/1/bn1/HmQAAADM5RRSBH2a1YY3QZhNcXA9b1cOraVV-6s1toaRmc92g7oAMIpnHvoMuLYH1DfeKz5Xg0HfyJMujvvnD438cTU2/d2luZG93c3Bob25lZGVmYXVsdA/exnOHg13q0ufuWU7hAd-mQ/j9bCr9o-CKOVyARCxsmG-UFvhkQ";
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
                        "<wp:Text2>" + "The Winnipeg game has started." + "</wp:Text2>" +
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught sending update: " + ex.ToString());
            }

        }
    }
}
