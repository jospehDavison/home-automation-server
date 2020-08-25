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
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port); //declaring new listener

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
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast.");
                    byte[] bytes = listener.Receive(ref groupEP);

                    string commandID = Encoding.ASCII.GetString(bytes);

                    Console.WriteLine($"Recieved broadcast from {groupEP} :");
                    Console.WriteLine($"{commandID}");

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
        public static void commandDelegation(string commandID)
        {
            if (commandID == "MediaPlayPause")
            {
                const int VK_MEDIA_PLAY_PAUSE = 0xB3;
                PressKeys(VK_MEDIA_PLAY_PAUSE);
            }
            else if (commandID.Contains("MediaVolume"))
            {
                if (commandID.Contains("Up"))
                {
                    const int VK_VOLUME_UP = 0xAF;
                    for (int i = 0; i < 5; i++)
                    {
                        PressKeys(VK_VOLUME_UP);
                    }
                }
                else if (commandID.Contains("Down"))
                {
                    const int VK_VOLUME_DOWN = 0xAE;
                    for (int i = 0; i < 5; i++)
                    {
                        PressKeys(VK_VOLUME_DOWN);
                    }
                }
            }
            else if (commandID == "new")
            {

            }
            else if (commandID == "new2")
            {

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
    }
}
