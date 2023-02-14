using Multimorphic.P3App.GUI;
using System;
using System.Collections;
using UnityEngine.Networking;

namespace Op3nPinball.Utils.GUI
{
    /// <summary>
    /// Class to send notifications to a ntfy server. Adding this class to your Attract Scene will
    /// enable its use. To use it, the ntfy server and topic need to be set via EventSetNtfyServer
    /// and EventSetNtfyTopic ModeToGUI events. Then it will send notification of the given string
    /// using EventSendNtfy ModeToGUI event.
    /// </summary>
    public class NtfyManager : P3Aware 
    {
        // We only want one NtfyManager in the scene, so we create a singleton.
        private static NtfyManager _instance;
        // Constants strings for our Handlers.
        public static readonly string EventSendNtfy = "Evt_SendNtfyEvent";
        public static readonly string EventSetNtfyTopic = "Evt_SetNtfyTopic";
        public static readonly string EventSetNtfyServer = "Evt_SetNtfyServer";
        
        // String of the pubsub topic. This is the unique string the user will subscribe to
        private string _ntfyTopic;
        // String of the nfty server. We default to the public instance.
        private string _ntfyServer = "https://ntfy.sh/";
        
        void Awake() {
            // Assign the singleton
            if (_instance != null && _instance != this)
                Destroy(this.gameObject);
            else
            {
                _instance = this;
                // We need our instance to persist across Attract and Home modes.
                DontDestroyOnLoad(this.gameObject);
            }
        }

        protected override void CreateEventHandlers()
        {
            base.CreateEventHandlers ();
            AddModeEventHandler(EventSendNtfy, NotificationEventHandler);
            AddModeEventHandler(EventSetNtfyTopic, SetNtfyTopicEventHandler);
            AddModeEventHandler(EventSetNtfyServer, SetNtfyServerEventHandler);
        }

        /// <summary>
        /// Set the nfty topic <paramref name="eventData"/> as a string.
        /// </summary>
        /// <param name="eventName">Default eventName string.</param>
        /// <param name="eventData">string of topic as an object.</param>
        public void SetNtfyTopicEventHandler(string eventName, object eventData)
        {
            _ntfyTopic = (string)eventData; 
        }

        /// <summary>
        /// Set the nfty server <paramref name="eventData"/> as a string.
        /// </summary>
        /// <param name="eventName">Default eventName string.</param>
        /// <param name="eventData">string of topic as an object. If not set, will use
        /// https://ntfy.sh</param>
        public void SetNtfyServerEventHandler(string eventName, object eventData)
        {
            _ntfyServer = (string)eventData; 
        }

        /// <summary>
        /// Send notification via nfty with message in <paramref name="eventData"/> as a string.
        /// </summary>
        /// <param name="eventName">Default eventName string.</param>
        /// <param name="eventData">string of message as an object.</param>
        public void NotificationEventHandler(string eventName, object eventData)
        {
            if (String.IsNullOrEmpty(_ntfyTopic))
            {
                Multimorphic.P3App.Logging.Logger.LogWarning(
                    "NftManager: ntfy topic not set. Please call SetNtfyTopic before sending notification."
                );    
                return;
            }
            string text = (string)eventData;
            StartCoroutine(SendNotificaiton(text));
        }

        IEnumerator SendNotificaiton(string text) {
            var serverUri = new Uri(_ntfyServer);
            var requestUri = new Uri(serverUri, _ntfyTopic);
            UnityWebRequest request = new UnityWebRequest(requestUri.ToString());
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(text));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.method = UnityWebRequest.kHttpVerbPOST;
            yield return request.Send();
            
            if (request.isError)
            {
                Multimorphic.P3App.Logging.Logger.LogError(request.error);
            }
        }
    }
}
