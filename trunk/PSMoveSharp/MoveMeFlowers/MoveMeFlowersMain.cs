using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using PSMoveSharp;

namespace MoveMeFlowers
{
    static class Program
    {
        public static PSMoveClientDelegate moveClient;
        public static MoveMeFlowersGUI form;
        public static MoveMeFlowerState flower;
        public static MoveMeFlowerCamera camera;

        public static bool client_connected;
        public static bool client_paused;

        public static void client_connect(String server_address, int server_port)
        {
            moveClient = new PSMoveClientDelegate();
            
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

            moveClient.DelayChange(2);
            moveClient._state_update_delegate += new PSMoveClientDelegate.UpdatedStateDelegate(flower.UpdateState);
            moveClient._state_update_delegate += new PSMoveClientDelegate.UpdatedStateDelegate(camera.UpdateState);

            Properties.Settings.Default.most_recent_server = server_address;
            Properties.Settings.Default.Save();
        }

        public static void client_disconnect()
        {
            try
            {
                client_paused = false;

                if (moveClient != null)
                {
                    moveClient.StopThread();
                    moveClient.Close();
                }
            }
            catch
            {
                return;
            }

            client_connected = false;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            client_connected = false;
            client_paused = false;
            flower = new MoveMeFlowerState(1000);
            camera = new MoveMeFlowerCamera();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new MoveMeFlowersGUI();
            Application.Run(form);
            client_disconnect();
        }
    }
}
