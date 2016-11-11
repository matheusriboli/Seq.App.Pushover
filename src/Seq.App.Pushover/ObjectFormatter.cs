using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Seq.App.Pushover {
    public class ObjectFormatter {

        public string Format(object item) {
            StringBuilder builder = new StringBuilder();
            this.FormatEnumerable(item as IEnumerable, builder);
            return builder.ToString();
        }

        private void FormatEnumerable(IEnumerable items, StringBuilder builder) {

            if (items == null) {
                builder.Append("null");
                return;
            }

            bool isCollection = false;

            int index = 0;
            foreach (var item in items) {

                if (index > 0) {
                    builder.Append(", ");
                }
                else {
                    isCollection = (item is KeyValuePair<string, object>) == false;
                    builder.Append(isCollection ? "[ " : "{ ");
                }

                if (item is KeyValuePair<string, object>) {
                    var kvp = (KeyValuePair<string, object>)item;

                    builder.Append(kvp.Key);
                    builder.Append(": ");

                    this.AppendObject(kvp.Value, builder);
                }
                else {
                    this.AppendObject(item, builder);
                }

                index++;
            }

            builder.Append(isCollection ? " ]" : " }");
        }

        private void AppendObject(object value, StringBuilder builder) {
            if (value is IEnumerable && (value is string) == false) {
                this.FormatEnumerable(value as IEnumerable, builder);
            }
            else {
                builder.Append((value ?? "null").ToString());
            }
        }
    }
}
