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
        internal static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const int port = 1200;
        UdpClient listener = new UdpClient(port);
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, port);

        const int KEYEVENTIF_KEYDOWN = 0x0000;
        const int KEYEVENTIF_KEYUP = 0x0002;

        static void Main(string[] args)
        {
            Program p = new Program();
            Console.WriteLine("press enter to close the window.");
            Console.ReadLine();
        }

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
                            for (int i = 0; i < 5;  i++)
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
         
        public static void PressKeys(byte key)
        {
            keybd_event(key, 0, KEYEVENTIF_KEYDOWN, 0);
            keybd_event(key, 0, KEYEVENTIF_KEYUP, 0);
        }
    }
}
