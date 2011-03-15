using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using PSMoveSharp;

namespace PSMoveSharpMouser
{
    static class MouserProgram
    {
        public static PSMoveClientThreadedRead moveClient;
        public static MouserMainWindow form;
        public static Thread updateGuiThread;
        public static bool _updateGuiThreadQuit;

        public static bool client_connected;
        public static bool client_paused;
        public static uint update_delay;

        private static void updateGui()
        {
            while (_updateGuiThreadQuit == false)
            {
                try
                {
                    form.Invoke(form.updateGuiDelegate);
                    Thread.Sleep(Convert.ToInt32(update_delay));
                }
                catch
                {
                    return;
                }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            client_connected = false;
            client_paused = false;
            update_delay = 16;

            _updateGuiThreadQuit = false;
            updateGuiThread = new Thread(new ThreadStart(updateGui));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new MouserMainWindow();
            updateGuiThread.Start();
            Application.Run(form);

            _updateGuiThreadQuit = true;
            updateGuiThread.Join();

            client_disconnect();
        }

        public static void client_connect(String server_address, int server_port)
        {
            moveClient = new PSMoveClientThreadedRead();

            try
            {
                moveClient.Connect(Dns.GetHostAddresses(server_address)[0].ToString(), server_port);
                moveClient.StartThread();
            }
            catch
            {
                return;
            }

            client_connected = true;

            Properties.Settings.Default.most_recent_server = server_address;
            Properties.Settings.Default.Save();
        }

        public static void client_disconnect()
        {
            try
            {
                client_paused = false;

                moveClient.StopThread();
                moveClient.Close();
            }
            catch
            {
                return;
            }

            client_connected = false;
        }
    }
}
