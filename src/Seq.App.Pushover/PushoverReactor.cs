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

        [SeqAppSetting(DisplayName = "ApiKey", HelpText = "Your Pushover api key.", IsOptional = false)]
        public string ApiKey { get; set; }

        [SeqAppSetting(DisplayName = "Title", HelpText = "The title to be used in notifications.", IsOptional = false)]
        public string Title { get; set; }

        [SeqAppSetting(DisplayName = "MessageTemplate", HelpText = "The message template to be used in notifications.", IsOptional = false)]
        public string MessageTemplate { get; set; }

        [SeqAppSetting(DisplayName = "UserKey", HelpText = "The user that will receive notifications.", IsOptional = true)]
        public string UserKey { get; set; }

        [SeqAppSetting(DisplayName = "Device", HelpText = "The device that will receive notifications.", IsOptional = true)]
        public string Device { get; set; }

        [SeqAppSetting(DisplayName = "Supression time (Seconds)", HelpText = "The time (in seconds) to supress repeated events.", InputType = SettingInputType.Integer, IsOptional = true)]
        public int SupressionTime { get; set; }

        public void On(Event<LogEventData> evt) {

            if (this.ShouldSupressEvent(evt.EventType)) { return; }

            try {

                PropertyResolver resolver = new PropertyResolver();

                var parameters = new NameValueCollection {
                    { "token", this.ApiKey },
                    { "title", resolver.ResolveProperties(this.Title, evt) },
                    { "user", this.UserKey },
                    { "message", resolver.ResolveProperties(this.MessageTemplate, evt) },
                    { "device", this.Device }
                };

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
