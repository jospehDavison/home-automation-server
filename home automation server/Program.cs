using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace home_automation_server
{
    public class Program
    {
        [DllImport("user32.dll")]
        internal static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo); //keyboard event function from win32

        public List<lightClient> lightClients = new List<lightClient>();

        //declaring new instance of udp client and ipendpoint on a port
        public const int commandPort = 1200;
        UdpClient udpCommandClient = new UdpClient(commandPort);
        static IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, commandPort);

        //declaring new instance of tcp client 
        public const int lightPort = 1201;
        TcpListener tcpLightListener;

        /// <summary>
        /// initiates program.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Program p = new Program();
            Console.WriteLine("press enter to close the window.");
            Console.ReadLine();
        }

        /// <summary>
        /// recviever for upd broadcasts. calls function sorting commands.
        /// </summary>
        public Program()
        {
            Console.Title = "Home Automation Server";
            Console.WriteLine("~Home Automation Server Start Up~");

            tcpLightListener = new TcpListener(IPAddress.Any, lightPort);
            tcpLightListener.Start();

            new Thread(new ThreadStart(commandListener)).Start();
            new Thread(new ThreadStart(lightListener)).Start();
        }

        /// <summary>
        /// listener for tcp light connections
        /// </summary>
        public void lightListener()
        {
            Console.WriteLine("Waiting for light connection...");
            while (true)
            {
                TcpClient tcpLightClient = tcpLightListener.AcceptTcpClient();

                int index = lightClients.Count;
                lightClient lightClient = new lightClient(this, tcpLightClient, index);
                lightClients.Add(lightClient);
            }
        }

        /// <summary>
        /// listener for udp connections
        /// </summary>
        public void commandListener()
        {
            Console.WriteLine("Waiting for broadcast...");
            try
            {
                while (true)
                {
                    byte[] idBytes = udpCommandClient.Receive(ref groupEP);
                    string commandID = Encoding.ASCII.GetString(idBytes);

                    commandDelegation(commandID);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                udpCommandClient.Close();
            }
        }

        /// <summary>
        /// recieves commandID and decides which key to press
        /// </summary>
        /// <param name="commandID"></param>
        public void commandDelegation(string commandID)
        {
            if (commandID == PLAYPAUSE) //play pause
            {
                const int VK_MEDIA_PLAY_PAUSE = 0xB3;

                PressKeys(VK_MEDIA_PLAY_PAUSE);
                WriteToScreen(commandID,"PP");
            }

            else if (commandID == VOLUP)
            {
                 const int VK_VOLUME_UP = 0xAF;
                 for (int i = 0; i < 5; i++)
                 {
                     PressKeys(VK_VOLUME_UP);
                 }
                WriteToScreen(commandID,"UP");
            }

            else if (commandID == VOLDOWN)
            {
                const int VK_VOLUME_DOWN = 0xAE;
                for (int i = 0; i < 5; i++)
                {
                    PressKeys(VK_VOLUME_DOWN);
                }
                WriteToScreen(commandID, "DOWN");
            }  
            
            else if (commandID.Contains("#")) //placeholder
            {
                WriteToScreen(commandID, "COLOUR");

                //send color to light aplications
                for (int i = 0; i<lightClients.Count;i++)
                {
                    lightClient light = lightClients[i];
                    light.sendLightCode(commandID);
                }
            }

            else if (commandID == "new2") //placeholder
            {

            }
        }
        
        /// <summary>
        /// writes necesary information to the console 
        /// </summary>
        /// <param name="playPause"></param>
        /// <param name="lastCommand"></param>
         public void WriteToScreen(string lastCommand, string info)
        {
            Console.WriteLine($"{lastCommand}");

            if (!(info == "")){
                Console.WriteLine($"Recieved {info} request from {groupEP} :");
            }
        }

        /// <summary>
        /// key presser; recieves key code and presses it.
        /// </summary>
        /// <param name="key"></param>
        public static void PressKeys(byte key)
        {
            const int KEYEVENTIF_KEYDOWN = 0x0000;
            const int KEYEVENTIF_KEYUP = 0x0002;

            keybd_event(key, 0, KEYEVENTIF_KEYDOWN, 0);
            keybd_event(key, 0, KEYEVENTIF_KEYUP, 0);
        }

        readonly string PLAYPAUSE = "0"; //Play/pause packet
        readonly string VOLUP = "1"; //Volume up packet
        readonly string VOLDOWN = "2"; //Volume down packet
    }




    public class lightClient
    {
        public int index;
        private TcpClient client;
        private NetworkStream netStream;
        private BinaryReader br;
        private BinaryWriter bw;
        Program program;


        /// <summary>
        /// define lightClient
        /// </summary>
        /// <param name="p"></param>
        /// <param name="C"></param>
        /// <param name="i"></param>
        public lightClient(Program p , TcpClient C, int i)
        {
            client = C;
            program = p;
            index = i;

            new Thread(new ThreadStart(setupConn)).Start();
        }

        /// <summary>
        /// sees if connection is ok
        /// </summary>
        void setupConn()
        {
            try
            {
                netStream = client.GetStream();
                br = new BinaryReader(netStream, Encoding.UTF8);
                bw = new BinaryWriter(netStream, Encoding.UTF8);

                bw.Write(OK);
                bw.Flush();

                byte reply = br.ReadByte();
                if (reply == OK)
                {
                    Console.WriteLine("light connected");
                    reciever();
                }
                else
                {
                    Console.WriteLine("closing connection");
                    closeConn();
                }
            }
            catch
            {
                Console.WriteLine("error");
                closeConn();
            }
        }

        private void reciever()
        {
            try
            {
                while (client.Connected)
                {
                    byte packet = br.ReadByte();
                    if (packet == LOGOUT)
                    {
                        closeConn();
                        Console.WriteLine("light removed");
                    }
                }
            }
            catch
            {

            }
        }

        public void sendLightCode(string lightCode)
        {
            bw.Write(lightCode);
            bw.Flush();
        }

        private void closeConn()
        {
            bw.Close();
            br.Close();
            netStream.Close();
            client.Close();

            program.lightClients.RemoveAt(index);

            for(int i = index; i < program.lightClients.Count; i++)
            {
                lightClient c = program.lightClients[i];
                c.index = i;
            }
        }

        readonly byte OK = 3; //ok packet
        readonly byte LOGOUT = 4;
    }
}
