﻿using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.Xml.Dom;
using System.Text.RegularExpressions;

namespace Harmony
{
    class ConfigClient : HarmonyClient
    {
        public string Config { get; set; }
        public string SessionToken { get; set; }

        public ConfigClient(string ipAddress, int port, string token)
            : base(ipAddress, port)
        {
            SessionToken = token;
            string username = string.Format("{0}@x.com", token);

            Xmpp.OnIq += OnIq;
            Xmpp.Open(username, token);

            WaitForData(5);
        }

        private Document GetConfigMessage()
        {
            var document = new Document { Namespace = "connect.logitech.com" };

            var element = new Element("oa");
            element.Attributes.Add("xmlns", "connect.logitech.com");
            element.Attributes.Add("mime", "vnd.logitech.harmony/vnd.logitech.harmony.engine?config");

            document.AddChild(element);
            return document;
        }

        public void GetConfig()
        {
            var iqToSend = new IQ { Type = IqType.get, Namespace = "", From = "1", To = "guest" };
            iqToSend.AddChild(GetConfigMessage());
            iqToSend.GenerateId();

            var iqGrabber = new IqGrabber(Xmpp);
            iqGrabber.SendIq(iqToSend, 10);

            WaitForData(5);
        }

        void OnIq(object sender, IQ iq)
        {
            if (iq.HasTag("oa"))
            {
                if (iq.InnerXml.Contains("errorcode=\"200\""))
                {
                    const string identityRegEx = "errorstring=\"OK\">(.*)</oa>";
                    var regex = new Regex(identityRegEx, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var match = regex.Match(iq.InnerXml);
                    if (match.Success)
                    {
                        Config = match.Groups[1].ToString();
                    }

                    Wait = false;
                }
            }
        }
    }
}
