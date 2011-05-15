﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Globalization;
using System.Runtime.Remoting;

namespace Blankie
{
    public class WebServer
    {
        private HttpListener _listener = new HttpListener();

        private string _hostIP = "127.0.0.1";

        private string _port = "1234";

        private string _streamerPort = "8080";

        private string serverLocalDirectory = Directory.GetCurrentDirectory();

        private string webappRelativePath = @"\page\";

        public string URL
        {
            get
            {
                return string.Format("http://{0}:{1}/", _hostIP, _port);
            }
        }

        public WebServer(string hostIP, string port, string streamerPort)
        {
            this._hostIP = hostIP;
            this._port = port;
            this._streamerPort = streamerPort;
        }

        public WebServer(string hostIP, string port, string serverLocalDirectory, string webappRelativePath)
        {
            this._hostIP = hostIP;
            this._port = port;
            this.serverLocalDirectory = serverLocalDirectory;
            this.webappRelativePath = webappRelativePath;
        }

        public void Start()
        {
            _listener.Prefixes.Add(String.Format("http://{0}:{1}/", _hostIP, _port));
            _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            _listener.Start();

            Console.WriteLine("Listening");

            while (true)
            {
                HttpListenerContext httpContext = _listener.GetContext();
                ProcessContext(httpContext);
            }
        }

        public void ProcessContext(HttpListenerContext httpListenerContext)
        {
            WriteRequestHeaderInformation(httpListenerContext);

            string path = httpListenerContext.Request.Url.PathAndQuery;
            string contentType = "text/html";

            if (path.EndsWith(".js")) contentType = "application/x-javascript"; else
            if (path.EndsWith(".swf")) contentType = "application/x-shockwave-flash";

            httpListenerContext.Response.ContentType = contentType;

            byte[] buffer = CreateResponseDocument(httpListenerContext);
            
            httpListenerContext.Response.OutputStream.Write(buffer, 0, buffer.Length);

            httpListenerContext.Response.Close();
        }

        private string ConstructPath(string requestPath)
        {
            return String.Format("{0}{1}{2}", serverLocalDirectory, webappRelativePath, requestPath);
        }

        private string ConstructHTML()
        {
            string output =
                @"
                    <!DOCTYPE html>

                    <html>
                    <head>
                    <title>Blankie Stream</title>
                    <script type='text/javascript' src='flowplayer-3.2.6.min.js'></script>
                    <meta content='text/html; charset=ISO-8859-1' http-equiv='content-type'>
                    </head>
                    <body>

                    <a href='http://" + _hostIP + @":" + _streamerPort + @"/Stream/stream.flv' style='display:block;width:520px;height:330px' id='player'></a>
  
                    <script>
                    flowplayer('player', 'flowplayer-3.2.7.swf', { plugins: {
                        controls: {
                            all:false,
                            fullscreen:true
                            }
                        }});
                    </script>
  
                    </body>
                    </html>
                ";

            return output;
        }

        private byte[] CreateResponseDocument(HttpListenerContext httpListenerContext)
        {
            WebClient client = new WebClient();

            Console.Out.WriteLine(httpListenerContext.Request.Url.PathAndQuery);

            try
            {
                if (httpListenerContext.Request.Url.PathAndQuery.Length > 1)
                {
                    return client.DownloadData(ConstructPath(httpListenerContext.Request.Url.AbsolutePath.Replace("/", "\\")));
                }

                return Encoding.UTF8.GetBytes(ConstructHTML());
            }
            catch (WebException exc)
            {
                return Encoding.UTF8.GetBytes(String.Format("<h1>Blankie error</h1><p>{0}</p>", exc.Message));
            }
        }

        private static void WriteRequestHeaderInformation(HttpListenerContext ctxt)
        {
            /*Console.WriteLine("Header Information:");
            ConsoleColor oldColour = Console.ForegroundColor;

            // Show all the headers in our request context
            foreach (string key in ctxt.Request.Headers.AllKeys)
            {
                Console.ForegroundColor = ConsoleColor.Green;

                Console.Write("[{0}]", key);

                Console.CursorLeft = 25;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" : ");
                Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine("[{0}]", ctxt.Request.Headers[key]);
            }
            Console.WriteLine("--");

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("User Information");
            Console.CursorLeft = 25;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" : ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.CursorLeft = 28;

            if (ctxt.User != null)
            {
                Console.WriteLine("Type = [{0}]", ctxt.User);
                Console.CursorLeft = 28;
                Console.WriteLine("Name = [{0}]", ctxt.User.Identity.Name);
            }
            else
            {
                Console.WriteLine("No User defined");
            }

            Console.ForegroundColor = oldColour;*/
        }

    }
}
