using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Drawing;

namespace PSMoveSharp
{
    public class PSMoveClientDelegate : PSMoveClient
    {
        public delegate void UpdatedStateDelegate(PSMoveSharpState state);
        public UpdatedStateDelegate _state_update_delegate; // register delegate here

        protected PSMoveSharpState _latest_state;
        protected Thread _readerThread;
        protected uint _readerThreadExit;
        protected bool _reading;

        protected class UdpState
        {
            public IPEndPoint e;
            public UdpClient u;
        }

        protected void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;
            Byte[] buffer = null;

            try
            {
                buffer = u.EndReceive(ar, ref e);
            }
            catch (System.ObjectDisposedException)
            {
                _reading = false;
                return;
            }
            catch (System.Net.Sockets.SocketException)
            {
                _reading = false;
                return;
            }

            int packet_length = NetworkReaderHelper.ReadInt32(ref buffer, 12);
            int magic = NetworkReaderHelper.ReadInt32(ref buffer, 0);
            int code = NetworkReaderHelper.ReadInt32(ref buffer, 8);
            uint packet_index = NetworkReaderHelper.ReadUint32(ref buffer, 16);

            if (code == 1)
            {
                _latest_state.packet_index = NetworkReaderHelper.ReadUint32(ref buffer, 16);
                _latest_state.serverConfig.FillFromNetworkBuffer(ref buffer);
                for (int i = 0; i < PSMoveSharpState.PSMoveSharpNumMoveControllers; i++)
                {
                    _latest_state.gemStatus[i].FillFromNetworkBuffer(ref buffer, i);
                    _latest_state.gemStates[i].FillFromNetworkBuffer(ref buffer, i);
                    _latest_state.imageStates[i].FillFromNetworkBuffer(ref buffer, i);
                    _latest_state.pointerStates[i].FillFromNetworkBuffer(ref buffer, i);
                    _latest_state.sphereStates[i].FillFromNetworkBuffer(ref buffer, i);
                    _latest_state.positionPointerStates[i].FillFromNetworkBuffer(ref buffer, i);
                }
                _latest_state.navInfo.FillFromNetworkBuffer(ref buffer);
                for (int i = 0; i < PSMoveSharpNavInfo.PSMoveSharpNumNavControllers; i++)
                {
                    _latest_state.padData[i].FillFromNetworkBuffer(ref buffer, i);
                }

                if (_state_update_delegate != null) {
                    _state_update_delegate(_latest_state);
                }
                
                // fire delegate here
            }
            _reading = false;
        }

        protected void PSMoveClienDelegateReadThread()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            UdpState s = new UdpState();
            while (_readerThreadExit == 0)
            {
                s.e = remoteEP;
                s.u = _udpClient;
                _reading = true;
                try
                {
                    if (_udpClient != null)
                        _udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), s);
                }
                catch (System.Exception)
                {
                    // socket closed exception
                    Console.WriteLine("except from udp client begin receive");
                }
                while (_reading)
                {
                    if (_readerThreadExit == 1)
                    {
                        break;
                    }
                    Thread.Sleep(0);
                }
            }
        }

        public PSMoveClientDelegate()
        {
            _readerThread = new Thread(new ThreadStart(PSMoveClienDelegateReadThread));
            _readerThreadExit = 0;
            _latest_state = new PSMoveSharpState();
            _reading = false;
        }

        ~PSMoveClientDelegate()
        {
            _readerThreadExit = 1;

            try
            {
                _readerThread.Join();
            }
            catch (System.Exception)
            {

            }
        }

        public void StartThread()
        {
            _readerThread.Start();
        }

        public void StopThread()
        {
            _readerThreadExit = 1;
            _readerThread.Join();
        }
    }
}