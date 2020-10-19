using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;

namespace home_automation_server
{
    public class Program
    {
        [DllImport("user32.dll")]
        internal static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo); //keyboard event function from win32

        private const int port = 1200;

        UdpClient listener = new UdpClient(port);
        static IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port); //declaring new listener

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
            Console.WriteLine("Waiting for broadcast...");
            try
            {
                while (true)
                {
                    byte[] bytes = listener.Receive(ref groupEP);                    
                    string commandID = Encoding.ASCII.GetString(bytes);

                    commandDelegation(commandID);
                }
            } 
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                listener.Close();
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
                WriteToScreen(commandID,"");
            }

            else if (commandID == VOLUP)
            {
                 const int VK_VOLUME_UP = 0xAF;
                 for (int i = 0; i < 5; i++)
                 {
                     PressKeys(VK_VOLUME_UP);
                 }
                WriteToScreen(commandID,"");
            }

            else if (commandID == VOLDOWN)
            {
                const int VK_VOLUME_DOWN = 0xAE;
                for (int i = 0; i < 5; i++)
                {
                    PressKeys(VK_VOLUME_DOWN);
                }
                WriteToScreen(commandID, "");
            }  
            
            else if (commandID.Contains(COLOR)) //placeholder
            {
                WriteToScreen(commandID,"");
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
         public static void WriteToScreen(string lastCommand, string info)
        {
            Console.Clear();
            if (!(info == "")){
                Console.WriteLine($"{info}");
            }
            Console.WriteLine($"Recieved {lastCommand} request from {groupEP} :");
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
        readonly string COLOR = "3"; //listen for color packet
    }
}
