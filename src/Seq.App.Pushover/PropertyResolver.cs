using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Seq.Apps;
using Seq.Apps.LogEvents;

namespace Seq.App.Pushover {

    public class PropertyResolver {

        public string ResolveProperties(string template, Event<LogEventData> evt) {

            string message = template;

            Regex regex = new Regex(@"{{(.*?)}}", RegexOptions.Compiled | RegexOptions.Multiline);
            MatchCollection matchCollection = regex.Matches(template);

            // Contains all referenced properties.
            Dictionary<string, string> propertiesDictionary = new Dictionary<string, string>(StringComparer.InvariantCulture);

            foreach (var match in matchCollection) {
                // Gets the property name removing '{{' and '}}'
                string propertyName = match.ToString();
                propertyName = propertyName.Substring(2, propertyName.Length - 4);

                if (propertiesDictionary.ContainsKey(propertyName) == false) {

                    string propertyValue;

                    if (propertyName.StartsWith("@")) {
                        propertyValue = this.GetSeqProperty(propertyName, evt);
                    }
                    else {
                        propertyValue = this.GetProperty(propertyName, evt.Data.Properties);
                    }

                    propertiesDictionary.Add(propertyName, propertyValue);
                }
            }

            // Applies all resolved values.
            foreach (var propertyName in propertiesDictionary) {
                message = message.Replace(string.Concat("{{", propertyName.Key, "}}"), propertyName.Value);
            }

            return message;
        }

        private string GetSeqProperty(string propertyName, Event<LogEventData> evt) {
            propertyName = propertyName.ToLower().Trim();

            string propertyValue = null;

            switch (propertyName) {
                case "@id":
                    propertyValue = evt.Id;
                    break;
                case "@timestamputc":
                    propertyValue = evt.TimestampUtc.ToString();
                    break;
                case "@eventtype":
                    propertyValue = evt.EventType.ToString();
                    break;
                case "@exception":
                    propertyValue = evt.Data.Exception;
                    break;
                case "@level":
                    propertyValue = evt.Data.Level.ToString();
                    break;
                case "@localtimestamp":
                    propertyValue = evt.Data.LocalTimestamp.ToString();
                    break;
                case "@messagetemplate":
                    propertyValue = evt.Data.MessageTemplate;
                    break;
                case "@renderedmessage":
                    propertyValue = evt.Data.RenderedMessage;
                    break;
            }

            return propertyValue;
        }

        private string GetProperty(string propertyName, object obj) {

            var properties = obj as IReadOnlyDictionary<string, object>;

            if (properties == null) { return null; }

            string parentPropertyName = propertyName;
            string childItemName = null;
            bool hasChild = false;

            int index = propertyName.IndexOf('.');
            if (index >= 0) {
                parentPropertyName = propertyName.Substring(0, index);
                childItemName = propertyName.Substring(index + 1);
                hasChild = true;
            }

            object item = null;
            if (properties.ContainsKey(parentPropertyName)) {
                item = properties[parentPropertyName];
            }

            if (item == null) { return null; }

            if (hasChild) { return this.GetProperty(childItemName, item); }

            if (item is IEnumerable && (item is string) == false) {
                return new ObjectFormatter().Format(item);
            }
            else {
                return item.ToString();
            }
        }
    }
}
