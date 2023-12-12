using System.Xml.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace BlazorChatWeb.Server
{
    public class ChatServer
    {
        WebSocketServer wssv = new WebSocketServer(8080);
        NotyficationChannels notyfication = new NotyficationChannels();


        public class ChatBehavior : WebSocketBehavior
        {
            List<string> pass = new List<string>();
   

            protected override void OnMessage(MessageEventArgs e)
            {
                pass.Add(e.Data);
                Console.WriteLine($"Received message: {e.Data}");

                foreach (var session in base.Sessions.IDs)
                    base.Sessions.SendTo(e.Data, session);
            }
        }

        public class ChannelSocketBehavior : WebSocketBehavior
        {
            WebSocketServer Server;
            public ChannelSocketBehavior(WebSocketServer serv) => Server=serv;


            protected override void OnMessage(MessageEventArgs e)
            {
                if (e.Data.ToUpper().StartsWith("ADD "))
                    AddChannel(e.Data.Substring(4));
                else if (e.Data.ToUpper().StartsWith("UPDATE "))
                    ForceUpdateChannelList();
            }

            protected override void OnOpen()
            {
                base.OnOpen();
            }

            private void ForceUpdateChannelList()
            {
                var service = Server.WebSocketServices.Hosts.Where(x => x.Path == "/channel").FirstOrDefault();
                string tags = "";

                foreach (var tag in Server.WebSocketServices.Paths)
                {
                    tags += tag.Substring(1) + ";";
                }

                if (service != null)
                {
                    foreach (var session in service.Sessions.IDs)
                        foreach (var channelNames in Server.WebSocketServices.Paths)
                            base.Sessions.SendTo(tags, session);
                }
            }

            public void AddChannel(string name)
            {
                if (!Server.WebSocketServices.Hosts.Any(x=>x.Path.Contains(name)))
                {
                    Server.AddWebSocketService<ChatBehavior>($"/{name}");
                    string tags = "";

                    foreach(var tag in Server.WebSocketServices.Paths)
                    {
                        tags += tag.Substring(1) + ";";
                    }

                    foreach (var session in Server.WebSocketServices["/channel"].Sessions.IDs)
                        base.Sessions.SendTo(tags, session);
                }
            }

            public string[] GetChannels()
            {
                List<string> channelList = new List<string>();

                foreach (var sessionName in Server.WebSocketServices.Paths)
                {
                    channelList.Add(sessionName);
                }
                return channelList.ToArray();
            }
        }

        [Obsolete]
        public ChatServer()
        {
            wssv.AddWebSocketService<ChatBehavior>("/");
            wssv.AddWebSocketService<ChannelSocketBehavior>("/channel",() => new ChannelSocketBehavior(wssv));
            wssv.Start();
            Console.WriteLine("WebSocket Server started");
        }



    }
}
