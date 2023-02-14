using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Op3nPinball.Utils.GUI;
using System;

namespace Op3nPinball.Utils.Modes
{
    /// <summary> This is a simple P3Mode that posts a message to a ntfy server. This class expects
    /// the following GameAttributes: NtfyTopic and NtfyServer.
    /// </summary>
    public class HighScoreNtfyMode : P3Mode
    {
        // used to track if we have sent the server and topic to the GUI layer.
        private bool _ntfySet;
        public HighScoreNtfyMode (P3Controller controller, int priority)
            : base(controller, priority)
        {
            _ntfySet = false;
            AddModeEventHandler("Evt_HighScoreEntryFinished", PostHighScoreEvent, Priority);
        }

        /// <summary>
        /// Send a notifcation that a new high score has been obtained.
        /// </summary>
        /// <param name="eventName">Default eventName string.</param>
        /// <param name="eventData">Ignored.</param>
        public bool PostHighScoreEvent(string eventName, object eventData)
        {
            if (!_ntfySet)
            {
                string ntfyTopic = data.GetGameAttributeValue("NtfyTopic").ToString();
                string ntfyServer = data.GetGameAttributeValue("NtfyServer").ToString();

                if (String.IsNullOrEmpty(ntfyTopic))
                {
                    Multimorphic.P3App.Logging.Logger.LogWarning("No ntfy topic set. Aborting sending notificaiton.");
                    return EVENT_CONTINUE;
                }
                PostModeEventToGUI(NtfyManager.EventSetNtfyTopic, ntfyTopic);
                if (!String.IsNullOrEmpty(ntfyServer)) // if server is not set, it will default to ntfy.sh
                {
                    PostModeEventToGUI(NtfyManager.EventSetNtfyServer, ntfyServer);
                }
                
                _ntfySet = true;
            }

            string appName = data.GetGameAttributeValue("Application Name").ToString();

            PostModeEventToGUI(NtfyManager.EventSendNtfy, "New P3 highscore on " + appName);
            return EVENT_CONTINUE;
        }
    } 
}