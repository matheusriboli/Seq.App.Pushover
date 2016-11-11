using System;
using System.Collections.Specialized;
using System.Net;
using Seq.Apps;
using Seq.Apps.LogEvents;

namespace Seq.App.Pushover {

    [SeqApp("Pushover App", Description = "Sends events to Pushover using a provided template message.")]
    public class PushoverReactor : Reactor, ISubscribeTo<LogEventData> {

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

        public void On(Event<LogEventData> evt) {

            try {

                PropertyResolver searcher = new PropertyResolver();

                var parameters = new NameValueCollection {
                    { "token", this.ApiKey },
                    { "title", searcher.ResolveProperties(this.Title, evt.Data.Properties) },
                    { "user", this.UserKey },
                    { "message", searcher.ResolveProperties(this.MessageTemplate, evt.Data.Properties) },
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
    }
}
