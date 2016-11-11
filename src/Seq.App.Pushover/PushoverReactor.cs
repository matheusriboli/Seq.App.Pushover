using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Seq.Apps;
using Seq.Apps.LogEvents;
using Serilog.Context;

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
            var parameters = new NameValueCollection {
                { "token", this.ApiKey },
                { "user", this.UserKey },
                { "message", this.MessageTemplate },
                { "device", this.Device }
            };

            try {
                byte[] response;
                using (var client = new WebClient()) {
                    response = client.UploadValues("https://api.pushover.net/1/messages.json", parameters);
                }

                using (LogContext.PushProperty("PushoverResponse", Encoding.Default.GetString(response))) {
                    this.Log.Verbose("Pushing event.");
                }
            }
            catch (Exception ex) {
                this.Log.Error(ex, "Error pushing event.");
            }
        }
    }
}
