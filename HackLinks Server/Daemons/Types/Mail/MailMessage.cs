using HackLinks_Server.Files;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace HackLinks_Server.Daemons.Types.Mail {
    public class MailMessage {
        public string TimeSent;
        public string To;
        public string Body;
        public string From;

        public MailMessage(string to, string from, string body) {
            TimeSent = DateTime.Now.ToUniversalTime().ToString();
            To = to;
            Body = body;
            From = from;
        }
        public MailMessage(JObject jObject) {
            TimeSent = GetStringFromJObject(jObject, "Timestamp");
            Body = GetStringFromJObject(jObject, "Body");
            From = GetStringFromJObject(jObject, "From");
        }
        public MailMessage(File message) {
            JObject jObject;
            try {
                jObject = JObject.Parse(message.Content);
            } catch {
                jObject = new JObject();
            }
            TimeSent = GetStringFromJObject(jObject, "Timestamp");
            Body = GetStringFromJObject(jObject, "Body");
            From = GetStringFromJObject(jObject, "From");
        }

        public JObject ToJObject() {
            return new JObject(
                new JProperty("Body", Body),
                new JProperty("From", From),
                new JProperty("Timestamp", TimeSent));
        }

        #region Helpers

        private string GetStringFromJObject (JObject jObject, string key) {
            try {
                return jObject.Properties()
                    .Where(x => x.Name == key)
                    .Select(x => (string)x.Value)
                    .Single();
            } catch (Exception e) {
                Util.Logger.Exception(e, "Some idiot tried to edit a mail message and failed");
                return "null";
            }
        }

        #endregion
    }
}
