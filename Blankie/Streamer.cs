using Implementation;
using Declarations.Media;
using Declarations.Players;
using Declarations;
using System.Net;
using System.Text;
using System;

namespace Blankie
{
    public class Streamer
    {
        private IPlayer player;
        private string ip;
        private int port;

        public string URL
        {
            get
            {
                return string.Format("http://{0}:{1}/Stream/stream.flv", ip, port);
            }
        }

        public Streamer(string ip, int port)
        {
            this.ip = ip;
            this.port = port;

            IMediaPlayerFactory factory = new MediaPlayerFactory();

            string output = @":sout=#transcode{vcodec=FLV1,acodec=none,vb=1200,fps=12,scale=1}:duplicate{dst=std{access=http,mux=ffmpeg{mux=flv},dst=" + ip + ":" + port + "/Stream/stream.flv}}";

            IMedia media = factory.CreateMedia<IMedia>("screen://", output);
            player = factory.CreatePlayer<IPlayer>();
            player.Open(media);
        }

        public void Play()
        {
            player.Play();
        }

        public void Stop()
        {
            player.Stop();
        }

        public static string GetExternalIp()
        {
            string whatIsMyIp = "http://www.whatismyip.com/automation/n09230945.asp";
            WebClient wc = new WebClient();
            UTF8Encoding utf8 = new UTF8Encoding();
            string requestHtml = "";

            try
            {
                requestHtml = utf8.GetString(wc.DownloadData(whatIsMyIp));
            }
            catch (WebException)
            {
                string strHostName = Dns.GetHostName();

                IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
                IPAddress[] addr = ipEntry.AddressList;

                return addr[addr.Length - 1].ToString();
            }

            return requestHtml;
        }
    }
}
