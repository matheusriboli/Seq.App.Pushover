using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Net;
using Seq.Apps;
using Seq.Apps.LogEvents;

namespace Seq.App.Pushover {

    [SeqApp("Pushover App", Description = "Sends events to Pushover using a provided template message.")]
    public class PushoverReactor : Reactor, ISubscribeTo<LogEventData> {

        private readonly ConcurrentDictionary<uint, DateTime> _events = new ConcurrentDictionary<uint, DateTime>();
        private readonly static PropertyResolver _resolver = new PropertyResolver();
        private readonly static System.Collections.Generic.HashSet<string> _validSounds = new System.Collections.Generic.HashSet<string> {
            "pushover", "bike", "bugle", "cashregister", "classical", "cosmic", "falling", "gamelan", "incoming",
            "intermission", "magic", "mechanical", "pianobar", "siren", "spacealarm", "tugboat", "alien", "climb",
            "persistent", "echo", "updown", "none"
        };
        
        [SeqAppSetting(DisplayName = "ApiKey", HelpText = "Your Pushover api key.", IsOptional = false)]
        public string ApiKey { get; set; }

        [SeqAppSetting(DisplayName = "Display Title", HelpText = "The title to be used in notifications.", IsOptional = false)]
        public string DisplayTitle { get; set; }

        [SeqAppSetting(DisplayName = "MessageTemplate", HelpText = "The message template to be used in notifications.", InputType = SettingInputType.LongText, IsOptional = false)]
        public string MessageTemplate { get; set; }

        [SeqAppSetting(DisplayName = "UserKey", HelpText = "The user that will receive notifications.", IsOptional = true)]
        public string UserKey { get; set; }

        [SeqAppSetting(DisplayName = "Devices", HelpText = "The devices that will receive notifications. (Separated by pipe)", IsOptional = true)]
        public string Devices { get; set; }
        
        [SeqAppSetting(DisplayName = "Priority", HelpText = "The priority level for notifications. -2 (lowest), -1 (low), 0 (normal), 1 (high), 2 (emergency)", InputType = SettingInputType.Integer, IsOptional = false)]
        public int Priority { get; set; }
        
        [SeqAppSetting(DisplayName = "Emergency retry interval", HelpText = "(default : 60s) Specifies how often the Pushover servers will send the same notification to the user. Used when priority is emergency", InputType = SettingInputType.Integer, IsOptional = true)]
        public int Retry { get; set; }
        
        [SeqAppSetting(DisplayName = "Emergency retry limit", HelpText = "(default : 600s) Specifies how many seconds the notification will continue to be retried. Used when priority is emergency", InputType = SettingInputType.Integer, IsOptional = true)]
        public int Expire { get; set; }

        [SeqAppSetting(DisplayName = "Sound", HelpText = "The notification sound to play. When empty app settings are used. Valid values: Pushover, Bike, Bugle, CashRegister, Classical, Cosmic, Falling, Gamelan, Incoming, Intermission, Magic, Mechanical, PianoBar, Siren, SpaceAlarm, Tugboat, Alien, Climb, Persistent, Echo, UpDown, None", IsOptional = true)]
        public string Sound { get; set; }
        
        [SeqAppSetting(DisplayName = "Supression time (Seconds)", HelpText = "The time in seconds to supress repeated events.", InputType = SettingInputType.Integer, IsOptional = true)]
        public int SupressionTime { get; set; }

        public void On(Event<LogEventData> evt) {

            if (this.ShouldSupressEvent(evt.EventType)) { return; }

            try {

                var parameters = new NameValueCollection {
                    { "token", this.ApiKey },
                    { "title", PushoverReactor._resolver.ResolveProperties(this.DisplayTitle, evt) },
                    { "user", this.UserKey },
                    { "message", PushoverReactor._resolver.ResolveProperties(this.MessageTemplate, evt) },
                    { "device", this.Devices },
                    { "priority", this.Priority.ToString() }
                };
                
             
                var sound = Sound?.Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(sound)) 
                {
                    if (!_validSounds.Contains(sound)) {
                        Log.Warning("Invalid Sound {Sound}: using app default", Sound);
                    }
                    
                    parameters.Add("sound", sound);
                }
                
                if (Priority < -2)
                {
                    Priority = -2;
                    Log.Warning("Priority value {Priority} is too low: setting to -2", Priority);
                }
                else if (Priority > 2)
                {
                    Priority = 2;
                    Log.Warning("Priority value {Priority} is too high: setting to 2", Priority);
                }
                
                // Emergency priority requires retry and expire values
                if (Priority == 2) 
                {
                    parameters.Add("retry", (Retry > 0 ? Retry : 60).ToString());
                    parameters.Add("expire", (Expire > 0 ? Expire : 600).ToString());
                }

                byte[] response;
                using (var client = new WebClient()) {
                    response = client.UploadValues("https://api.pushover.net/1/messages.json", parameters);
                }
            }
            catch (Exception ex) {
                this.Log.Error(ex, "Error pushing event.");
            }
        }

        private bool ShouldSupressEvent(uint eventType) {

            bool added = false;
            var eventDate = this._events.GetOrAdd(eventType, p => { added = true; return DateTime.UtcNow; });
            if (added == false) {
                if (eventDate > DateTime.UtcNow.AddSeconds(-this.SupressionTime)) { return true; }
                this._events[eventType] = DateTime.UtcNow;
            }

            return false;
        }
    }
}
