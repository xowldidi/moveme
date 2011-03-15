using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using PSMoveSharp;

/* C# SendInput wrapper code references
 * [1]: http://homeofcox-cs.blogspot.com/2008/07/c-simulate-mouse-and-keyboard-events.html 
 * [2]: http://www.pinvoke.net/default.aspx/user32.SendInput
 */

namespace PSMoveSharpMouser
{
    public partial class MouserMainWindow : Form
    {
        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        const int INPUT_MOUSE = 0;
        const uint MOUSEEVENTF_MOVE = 0x0001;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        const uint MOUSEEVENTF_XDOWN = 0x0080;
        const uint MOUSEEVENTF_XUP = 0x0100;
        const uint MOUSEEVENTF_WHEEL = 0x0800;
        const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
        const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        public enum ClientCalibrationStep
        {
            Left = 0,
            Right = 1,
            Bottom = 2,
            Top = 3,
            Done = 4
        }

        private MOUSEINPUT CreateMouseInput(int x, int y, uint data, uint t, uint flag)
        {
            MOUSEINPUT mi = new MOUSEINPUT();
            mi.dx = x;
            mi.dy = y;
            mi.mouseData = data;
            mi.time = t;
            //mi.dwFlags = MOUSEEVENTF_ABSOLUTE| MOUSEEVENTF_MOVE;
            mi.dwFlags = flag;
            return mi;
        }

        private void SimulateMouseMove(int dx, int dy)
        {
            INPUT[] inp = new INPUT[1];
            // relative movement: move to virtual cursor position
            inp[0].type = INPUT_MOUSE;
            inp[0].mi = CreateMouseInput(dx, dy, 0, 0, MOUSEEVENTF_MOVE);
            uint r = SendInput((uint)inp.Length, inp, Marshal.SizeOf(inp[0].GetType()));
            if (r == 0)
            {
                // use this:
                int lastError = Marshal.GetLastWin32Error();
                Console.WriteLine("SendInput failed: {0}", lastError);
            }
        }

        private void SimulateMouseClickEvent(uint click_event)
        {
            INPUT[] inp = new INPUT[1];
            inp[0].type = INPUT_MOUSE;
            inp[0].mi = CreateMouseInput(0, 0, 0, 0, click_event);
            SendInput((uint)inp.Length, inp, Marshal.SizeOf(inp[0].GetType()));
        }

        public MouserMainWindow()
        {
            InitializeComponent();
            updateGuiDelegate = updateState;
        }

        public delegate void ProcessPSMoveStateDelegate();
        public ProcessPSMoveStateDelegate updateGuiDelegate;

        static UInt32 processed_packet_index = 0;
        static UInt16 last_buttons = 0;
        static ClientCalibrationStep calibration_step = 0;
        static bool fake_mouse_events = false;
        static float mouser_center_px = 0;
        static float mouser_center_py = 0;
        static float mouser_previous_px = 0;
        static float mouser_previous_py = 0;

        private void MouserRecenter(float px, float py)
        {
            mouser_center_px = px;
            mouser_center_py = py;

            mouser_previous_px = mouser_center_px;
            mouser_previous_py = mouser_center_py;
        }

        private void MouserMove(float px, float py)
        {
            px -= mouser_center_px;
            py -= mouser_center_py;

            px = Math.Min(Math.Max(px, -1.0f), 1.0f);
            py = -Math.Min(Math.Max(py, -1.0f), 1.0f);

            float mouser_dpx = px - mouser_previous_px;
            float mouser_dpy = py - mouser_previous_py;

            mouser_previous_px = px;
            mouser_previous_py = py;

            float screen_half_width = Convert.ToSingle(SystemInformation.VirtualScreen.Width) / 2.0f;
            float screen_half_height = Convert.ToSingle(SystemInformation.VirtualScreen.Height) / 2.0f;

            int mouser_dx = Convert.ToInt32(screen_half_width * mouser_dpx);
            int mouser_dy = Convert.ToInt32(screen_half_height * mouser_dpy);

            if ((mouser_dx != 0) || (mouser_dy != 0))
            {
                SimulateMouseMove(mouser_dx, mouser_dy);
            }
        }

        public void updateState()
        {
            updateToolbar();

            buttonEnable.Enabled = MouserProgram.client_connected;

            if (MouserProgram.moveClient == null)
            {
                return;
            }

            PSMoveSharpState state = MouserProgram.moveClient.GetLatestState();

            if (processed_packet_index == state.packet_index)
            {
                return;
            }

            processed_packet_index = state.packet_index;

            updateMouser(state);
        }

        private void updateToolbar()
        {
            buttonConnect.Text = MouserProgram.client_connected ? "Disconnect" : "Connect";
            textBoxServerAddress.Enabled = !MouserProgram.client_connected;
            textBoxServerPort.Enabled = !MouserProgram.client_connected;
            buttonPause.Enabled = MouserProgram.client_connected;
            comboBoxDelay.Enabled = MouserProgram.client_connected;
            buttonPause.Text = MouserProgram.client_paused ? "Resume" : "Pause";
        }

        private void updateMouser(PSMoveSharpState state)
        {
            UInt16 just_pressed;
            UInt16 just_released;
            {
                UInt16 changed_buttons = Convert.ToUInt16(state.gemStates[0].pad.digitalbuttons ^ last_buttons);
                just_pressed = Convert.ToUInt16(changed_buttons & state.gemStates[0].pad.digitalbuttons);
                just_released = Convert.ToUInt16(changed_buttons & ~state.gemStates[0].pad.digitalbuttons);
                last_buttons = state.gemStates[0].pad.digitalbuttons;
            }

            checkBoxCalibrated.Checked = (state.positionPointerStates[0].valid == 1);
            checkBoxEnabled.Checked = fake_mouse_events;
            labelMovePosValX.Text = (state.positionPointerStates[0].valid == 1) ? state.positionPointerStates[0].normalized_x.ToString("N3") : "N/A";
            labelMovePosValY.Text = (state.positionPointerStates[0].valid == 1) ? state.positionPointerStates[0].normalized_y.ToString("N3") : "N/A";
            buttonEnable.Text = fake_mouse_events ? "Disable" : "Enable";

            if (state.positionPointerStates[0].valid == 1)
            {
                if ((just_pressed & PSMoveSharpConstants.ctrlSelect) == PSMoveSharpConstants.ctrlSelect)
                {
                    MouserProgram.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPositionPointerDisable, 0);
                    Console.WriteLine("Disabling pointer");
                    calibration_step = 0;
                }

                if (fake_mouse_events)
                {
                    if ((just_pressed & PSMoveSharpConstants.ctrlCross) == PSMoveSharpConstants.ctrlCross)
                    {
                        MouserRecenter(state.positionPointerStates[0].normalized_x, state.positionPointerStates[0].normalized_y);
                    }
                    else
                    {
                        MouserMove(state.positionPointerStates[0].normalized_x, state.positionPointerStates[0].normalized_y);
                    }

                    if (((just_pressed & PSMoveSharpConstants.ctrlTick) == PSMoveSharpConstants.ctrlTick))
                    {
                        SimulateMouseClickEvent(MOUSEEVENTF_LEFTDOWN);
                    }
                    else if (((just_released & PSMoveSharpConstants.ctrlTick) == PSMoveSharpConstants.ctrlTick))
                    {
                        SimulateMouseClickEvent(MOUSEEVENTF_LEFTUP);
                    }

                    if (((just_pressed & PSMoveSharpConstants.ctrlCircle) == PSMoveSharpConstants.ctrlCircle))
                    {
                        SimulateMouseClickEvent(MOUSEEVENTF_RIGHTDOWN);
                    }
                    else if (((just_released & PSMoveSharpConstants.ctrlCircle) == PSMoveSharpConstants.ctrlCircle))
                    {
                        SimulateMouseClickEvent(MOUSEEVENTF_RIGHTUP);
                    }
                }
            }
            else if (((just_pressed & PSMoveSharpConstants.ctrlTick) == PSMoveSharpConstants.ctrlTick) &&
                    ((last_buttons & PSMoveSharpConstants.ctrlTrigger) == PSMoveSharpConstants.ctrlTrigger))
            {
                switch (calibration_step)
                {
                    case ClientCalibrationStep.Left:
                        MouserProgram.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPositionPointerSetLeft, 0);
                        break;
                    case ClientCalibrationStep.Right:
                        MouserProgram.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPositionPointerSetRight, 0);
                        break;
                    case ClientCalibrationStep.Bottom:
                        MouserProgram.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPositionPointerSetBottom, 0);
                        break;
                    case ClientCalibrationStep.Top:
                        MouserProgram.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPositionPointerSetTop, 0);
                        break;
                }

                Console.WriteLine("Calibration tick");
                calibration_step++;

                if (calibration_step == ClientCalibrationStep.Done)
                {
                    MouserProgram.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPositionPointerEnable, 0);
                    Console.WriteLine("Enabling pointer");
                }
            }
        }

        private void textBoxServerPort_TextChanged(object sender, EventArgs e)
        {
            try
            {
                textBoxServerPort.Text = Math.Min(Math.Max(Convert.ToInt32(textBoxServerPort.Text), 0), 65535).ToString();
            }
            catch
            {}
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (!MouserProgram.client_connected)
            {
                try
                {
                    MouserProgram.client_connect(textBoxServerAddress.Text, Convert.ToInt32(textBoxServerPort.Text));
                }
                catch
                {}
            }
            else
            {
                MouserProgram.client_disconnect();
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (!MouserProgram.client_paused)
            {
                MouserProgram.moveClient.Pause();
            }
            else
            {
                MouserProgram.moveClient.Resume();
            }

            MouserProgram.client_paused = !MouserProgram.client_paused;
        }

        private void buttonEnable_Click(object sender, EventArgs e)
        {
            fake_mouse_events = !fake_mouse_events;
        }
    }
}
