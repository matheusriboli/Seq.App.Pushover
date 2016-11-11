using System;
using PushoverClient;
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

        private PushoverClient.Pushover Client { get; set; }

        protected override void OnAttached() {
            base.OnAttached();

            this.Client = new PushoverClient.Pushover(this.ApiKey);
        }

        public void On(Event<LogEventData> evt) {

            try {
                PushResponse response = this.Client.Push(title: this.Title, message: this.MessageTemplate, userKey: this.UserKey, device: this.Device);

                this.Log.Information("{@PushoverResponse}", response);
            }
            catch (Exception ex) {
                this.Log.Error(ex, "Error pushing notification.");
            }
        }
    }
}
