using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PSMoveSharp;

namespace PSMoveSharpTest
{

    public partial class MoveSharpGUI : Form
    {
        public MoveSharpGUI()
        {
            InitializeComponent();
            updateGuiDelegate = updateState;
        }

        public delegate void ProcessPSMoveStateDelegate();
        public ProcessPSMoveStateDelegate updateGuiDelegate;

        public enum TabPageIndex
        {
            Move = 0,
            Nav = 1,
            Laser = 2,
            Position = 3,
            Camera = 4,
        }

        public enum ClientCalibrationStep
        {
            Left = 0,
            Right = 1,
            Bottom = 2,
            Top = 3,
            Done = 4
        }

        static UInt32 processed_packet_index = 0;
        static UInt16[] last_buttons = new UInt16[4];
        static ClientCalibrationStep[] calibration_step = new ClientCalibrationStep[4];

        public Float4 quatToEuler(Float4 q)
        {
            Float4 euler;

            euler.y = Convert.ToSingle(Math.Asin(2.0 * ((q.x * q.z) - (q.w * q.y))));

            if (euler.y == 90.0)
            {
                euler.x = Convert.ToSingle(2.0 * Math.Atan2(q.x, q.w));
                euler.z = 0;
            }
            else if (euler.y == -90.0)
            {
                euler.x = Convert.ToSingle(-2.0 * Math.Atan2(q.x, q.w));
                euler.z = 0;
            }
            else
            {
                euler.x = Convert.ToSingle(Math.Atan2(2.0 * ((q.x * q.y) + (q.z * q.w)), 1.0 - (2.0 * ((q.y * q.y) + (q.z * q.z)))));                
                euler.z = Convert.ToSingle(Math.Atan2(2.0 * ((q.x * q.w) + (q.y * q.z)), 1.0 - (2.0 * ((q.z * q.z) + (q.w * q.w)))));
            }

            euler.w = 0;

            return euler;
        }

        private void updateState()
        {
            updateToolbar();

            buttonResetEnabled.Enabled = Program.client_connected;
            comboBoxSelectedMove.Enabled = Program.client_connected;
            comboBoxSelectedNav.Enabled = Program.client_connected;

            if (Program.moveClient == null)
            {
                return;
            }

            PSMoveSharpState state = Program.moveClient.GetLatestState();
            PSMoveSharpCameraFrameState camera_frame_state = Program.moveClient.GetLatestCameraFrameState();
            if (processed_packet_index == state.packet_index)
            {
                return;
            }
            
            processed_packet_index = state.packet_index;

            switch ((TabPageIndex) tabControlPosition.SelectedIndex)
            {
                case TabPageIndex.Move:
                    updateTabPageMove(state);
                    break;
                case TabPageIndex.Nav:
                    updateTabPageNav(state);
                    break;
                case TabPageIndex.Laser:
                    updateTabPageLaser(state);
                    break;
                case TabPageIndex.Position:
                    updateTabPagePosition(state);
                    break;
                case TabPageIndex.Camera:
                    updateTabPageCamera(camera_frame_state);
                    break;
            }
        }

        private void updateToolbar()
        {
            buttonConnect.Text = Program.client_connected ? "Disconnect" : "Connect";
            textBoxServerAddress.Enabled = !Program.client_connected;
            textBoxServerPort.Enabled = !Program.client_connected;
            buttonPause.Enabled = Program.client_connected;
            comboBoxDelay.Enabled = Program.client_connected;
            buttonPause.Text = Program.client_paused ? "Resume" : "Pause";
        }

        private void updateTabPageCamera(PSMoveSharpCameraFrameState camera_frame_state)
        {
            if (Program.image_paused)
            {
                ImagePausedToggleButton.Text = "Unpause";
            } else {
                ImagePausedToggleButton.Text = "Pause";
            }
            camera_frame_state.camera_frame_state_rwl.AcquireReaderLock(-1);
            PSMoveSharpState dummy_state = new PSMoveSharpState();
            imageBox.Image = camera_frame_state.GetCameraFrameAndState(ref dummy_state);
            camera_frame_state.camera_frame_state_rwl.ReleaseReaderLock();
        }

        private void updateTabPageMove(PSMoveSharpState state)
        {
            checkBoxMove1.Checked = (state.gemStatus[0].connected == 1);
            checkBoxMove2.Checked = (state.gemStatus[1].connected == 1);
            checkBoxMove3.Checked = (state.gemStatus[2].connected == 1);
            checkBoxMove4.Checked = (state.gemStatus[3].connected == 1);

            checkBoxMoveResetEnabled.Checked = Program.reset_enabled[Program.selected_move];
            buttonResetEnabled.Text = Program.reset_enabled[Program.selected_move] ? "Disable" : "Enable";

            bool selected_move_connected = (state.gemStatus[Program.selected_move].connected == 1);
            PSMoveSharpGemState selected_gem = state.gemStates[Program.selected_move];

            labelPosValX.Text = selected_move_connected ? Convert.ToInt32(selected_gem.pos.x).ToString() : "N/A";
            labelPosValY.Text = selected_move_connected ? Convert.ToInt32(selected_gem.pos.y).ToString() : "N/A";
            labelPosValZ.Text = selected_move_connected ? Convert.ToInt32(selected_gem.pos.z).ToString() : "N/A";

            labelVelValX.Text = selected_move_connected ? Convert.ToInt32(selected_gem.vel.x).ToString() : "N/A";
            labelVelValY.Text = selected_move_connected ? Convert.ToInt32(selected_gem.vel.y).ToString() : "N/A";
            labelVelValZ.Text = selected_move_connected ? Convert.ToInt32(selected_gem.vel.z).ToString() : "N/A";

            labelAccValX.Text = selected_move_connected || true ? Convert.ToInt32(selected_gem.accel.x).ToString() : "N/A";
            labelAccValY.Text = selected_move_connected || true ? Convert.ToInt32(selected_gem.accel.y).ToString() : "N/A";
            labelAccValZ.Text = selected_move_connected || true ? Convert.ToInt32(selected_gem.accel.z).ToString() : "N/A";

            labelQuatValH.Text = selected_move_connected ? Convert.ToInt32((180.0 / Math.PI) * quatToEuler(selected_gem.quat).x).ToString() : "N/A";
            labelQuatValP.Text = selected_move_connected ? Convert.ToInt32((180.0 / Math.PI) * quatToEuler(selected_gem.quat).y).ToString() : "N/A";
            labelQuatValR.Text = selected_move_connected ? Convert.ToInt32((180.0 / Math.PI) * quatToEuler(selected_gem.quat).z).ToString() : "N/A";

            labelAngVelValH.Text = selected_move_connected ? Convert.ToInt32((180.0 / Math.PI) * selected_gem.angvel.x).ToString() : "N/A";
            labelAngVelValP.Text = selected_move_connected ? Convert.ToInt32((180.0 / Math.PI) * selected_gem.angvel.y).ToString() : "N/A";
            labelAngVelValR.Text = selected_move_connected ? Convert.ToInt32((180.0 / Math.PI) * selected_gem.angvel.z).ToString() : "N/A";

            labelAngAccValH.Text = selected_move_connected ? Convert.ToInt32((180.0 / Math.PI) * selected_gem.angaccel.x).ToString() : "N/A";
            labelAngAccValP.Text = selected_move_connected ? Convert.ToInt32((180.0 / Math.PI) * selected_gem.angaccel.y).ToString() : "N/A";
            labelAngAccValR.Text = selected_move_connected ? Convert.ToInt32((180.0 / Math.PI) * selected_gem.angaccel.z).ToString() : "N/A";

            labelHandlePosValX.Text = selected_move_connected ? Convert.ToInt32(selected_gem.handle_pos.x).ToString() : "N/A";
            labelHandlePosValY.Text = selected_move_connected ? Convert.ToInt32(selected_gem.handle_pos.y).ToString() : "N/A";
            labelHandlePosValZ.Text = selected_move_connected ? Convert.ToInt32(selected_gem.handle_pos.z).ToString() : "N/A";

            labelHandleVelValX.Text = selected_move_connected ? Convert.ToInt32(selected_gem.handle_vel.x).ToString() : "N/A";
            labelHandleVelValY.Text = selected_move_connected ? Convert.ToInt32(selected_gem.handle_vel.y).ToString() : "N/A";
            labelHandleVelValZ.Text = selected_move_connected ? Convert.ToInt32(selected_gem.handle_vel.z).ToString() : "N/A";

            labelHandleAccValX.Text = selected_move_connected ? Convert.ToInt32(selected_gem.handle_accel.x).ToString() : "N/A";
            labelHandleAccValY.Text = selected_move_connected ? Convert.ToInt32(selected_gem.handle_accel.y).ToString() : "N/A";
            labelHandleAccValZ.Text = selected_move_connected ? Convert.ToInt32(selected_gem.handle_accel.z).ToString() : "N/A";

            tableLayoutPanelMoveMotion.Update();
            
            checkBoxMoveSquareVal.Checked =     ((selected_gem.pad.digitalbuttons & PSMoveSharpConstants.ctrlSquare) != 0);
            checkBoxMoveCrossVal.Checked =      ((selected_gem.pad.digitalbuttons & PSMoveSharpConstants.ctrlCross) != 0);
            checkBoxMoveCircleVal.Checked =     ((selected_gem.pad.digitalbuttons & PSMoveSharpConstants.ctrlCircle) != 0);
            checkBoxMoveTriangleVal.Checked =   ((selected_gem.pad.digitalbuttons & PSMoveSharpConstants.ctrlTriangle) != 0);

            checkBoxMoveMoveVal.Checked =       ((selected_gem.pad.digitalbuttons & PSMoveSharpConstants.ctrlTick) != 0);
            labelMoveTVal.Text =                selected_move_connected ? selected_gem.pad.analog_trigger.ToString() : "N/A";

            checkBoxMoveStartVal.Checked =      ((selected_gem.pad.digitalbuttons & PSMoveSharpConstants.ctrlStart) != 0);
            checkBoxMoveSelectVal.Checked =     ((selected_gem.pad.digitalbuttons & PSMoveSharpConstants.ctrlSelect) != 0);

            tableLayoutPanelMoveInput.Update();

            if (Program.reset_enabled[Program.selected_move] && ((state.gemStates[Program.selected_move].pad.digitalbuttons & PSMoveSharpConstants.ctrlSelect) != 0))
            {
                Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestControllerReset, Convert.ToUInt32(Program.selected_move));
            }
        }

        private void updateTabPageNav(PSMoveSharpState state)
        {
            bool selected_nav_connected = ((state.navInfo.port_status[Program.selected_nav] & 0x1) == 0x1);

            checkBoxNav1.Checked = ((state.navInfo.port_status[0] & 0x1) == 0x1);
            checkBoxNav2.Checked = ((state.navInfo.port_status[1] & 0x1) == 0x1);
            checkBoxNav3.Checked = ((state.navInfo.port_status[2] & 0x1) == 0x1);
            checkBoxNav4.Checked = ((state.navInfo.port_status[3] & 0x1) == 0x1);
            checkBoxNav5.Checked = ((state.navInfo.port_status[4] & 0x1) == 0x1);
            checkBoxNav6.Checked = ((state.navInfo.port_status[5] & 0x1) == 0x1);
            checkBoxNav7.Checked = ((state.navInfo.port_status[6] & 0x1) == 0x1);

            checkBoxNavUpVal.Checked =      selected_nav_connected && ((state.padData[Program.selected_nav].button[PSMoveSharpConstants.offsetDigital1] & PSMoveSharpConstants.ctrlUp) != 0);
            checkBoxNavDownVal.Checked =    selected_nav_connected && ((state.padData[Program.selected_nav].button[PSMoveSharpConstants.offsetDigital1] & PSMoveSharpConstants.ctrlDown) != 0);
            checkBoxNavLeftVal.Checked =    selected_nav_connected && ((state.padData[Program.selected_nav].button[PSMoveSharpConstants.offsetDigital1] & PSMoveSharpConstants.ctrlLeft) != 0);
            checkBoxNavRightVal.Checked =   selected_nav_connected && ((state.padData[Program.selected_nav].button[PSMoveSharpConstants.offsetDigital1] & PSMoveSharpConstants.ctrlRight) != 0);

            labelNavAnalogXVal.Text = selected_nav_connected ? (state.padData[Program.selected_nav].button[PSMoveSharpConstants.offsetAnalogLeftX] - 128).ToString() : "N/A";
            labelNavAnalogYVal.Text = selected_nav_connected ? (state.padData[Program.selected_nav].button[PSMoveSharpConstants.offsetAnalogLeftY] - 128).ToString() : "N/A";

            checkBoxNavCrossVal.Checked =   selected_nav_connected && ((state.padData[Program.selected_nav].button[PSMoveSharpConstants.offsetDigital2] & PSMoveSharpConstants.ctrlCross) != 0);
            checkBoxNavCircleVal.Checked =  selected_nav_connected && ((state.padData[Program.selected_nav].button[PSMoveSharpConstants.offsetDigital2] & PSMoveSharpConstants.ctrlCircle) != 0);

            checkBoxNavL1Val.Checked =      selected_nav_connected && ((state.padData[Program.selected_nav].button[PSMoveSharpConstants.offsetDigital2] & PSMoveSharpConstants.ctrlL1) != 0);
            labelNavL2Val.Text =            selected_nav_connected ? state.padData[Program.selected_nav].button[PSMoveSharpConstants.offsetPressL2].ToString() : "N/A";
            checkBoxNavL3Val.Checked =      selected_nav_connected && ((state.padData[Program.selected_nav].button[PSMoveSharpConstants.offsetDigital1] & PSMoveSharpConstants.ctrlL3) != 0);
        }

        private void updateTabPageLaser(PSMoveSharpState state)
        {
            checkBoxLaser1.Checked = (state.pointerStates[0].valid == 1);
            checkBoxLaser2.Checked = (state.pointerStates[1].valid == 1);
            checkBoxLaser3.Checked = (state.pointerStates[2].valid == 1);
            checkBoxLaser4.Checked = (state.pointerStates[3].valid == 1);

            labelLaserPos1ValX.Text = (state.pointerStates[0].valid == 1) ? state.pointerStates[0].normalized_x.ToString("N3") : "N/A";
            labelLaserPos1ValY.Text = (state.pointerStates[0].valid == 1) ? state.pointerStates[0].normalized_y.ToString("N3") : "N/A";
            labelLaserPos2ValX.Text = (state.pointerStates[1].valid == 1) ? state.pointerStates[1].normalized_x.ToString("N3") : "N/A";
            labelLaserPos2ValY.Text = (state.pointerStates[1].valid == 1) ? state.pointerStates[1].normalized_y.ToString("N3") : "N/A";
            labelLaserPos3ValX.Text = (state.pointerStates[2].valid == 1) ? state.pointerStates[2].normalized_x.ToString("N3") : "N/A";
            labelLaserPos3ValY.Text = (state.pointerStates[2].valid == 1) ? state.pointerStates[2].normalized_y.ToString("N3") : "N/A";
            labelLaserPos4ValX.Text = (state.pointerStates[3].valid == 1) ? state.pointerStates[3].normalized_x.ToString("N3") : "N/A";
            labelLaserPos4ValY.Text = (state.pointerStates[3].valid == 1) ? state.pointerStates[3].normalized_y.ToString("N3") : "N/A";

            for (int i = 0; i < PSMoveSharpState.PSMoveSharpNumMoveControllers; i++)
            {
                UInt16 just_pressed;

                {
                    UInt16 changed_buttons = (UInt16)(state.gemStates[i].pad.digitalbuttons ^ last_buttons[i]);
                    just_pressed = (UInt16)(changed_buttons & state.gemStates[i].pad.digitalbuttons);
                    last_buttons[i] = state.gemStates[i].pad.digitalbuttons;
                }

                pointerDisplayControlLaser.valid[i] = (state.pointerStates[i].valid == 1);

                if (state.pointerStates[i].valid == 1)
                {
                    pointerDisplayControlLaser.pointer_x[i] = state.pointerStates[i].normalized_x;
                    pointerDisplayControlLaser.pointer_y[i] = state.pointerStates[i].normalized_y;             

                    const int PadSelect = 1;
                    const int PadTriangle = 1 << 4;
                    
                    if ((just_pressed & PadSelect) == PadSelect)
                    {
                        Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPointerDisable, Convert.ToUInt32(i));
                        Console.WriteLine("Disabling pointer");
                        calibration_step[i] = ClientCalibrationStep.Left;
                    }
                }
                else
                {
                    const int PadTick = 1 << 2;
                    const int PadTrigger = 1 << 1;

                    if ((just_pressed & PadTick) == PadTick && (last_buttons[i] & PadTrigger) == PadTrigger)
                    {
                        switch (calibration_step[i])
                        {
                            case ClientCalibrationStep.Left:
                                Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPointerSetLeft, Convert.ToUInt32(i));
                                break;
                            case ClientCalibrationStep.Right:
                                Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPointerSetRight, Convert.ToUInt32(i));
                                break;
                            case ClientCalibrationStep.Bottom:
                                Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPointerSetBottom, Convert.ToUInt32(i));
                                break;
                            case ClientCalibrationStep.Top:
                                Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPointerSetTop, Convert.ToUInt32(i));
                                break;
                        }

                        Console.WriteLine("Calibration tick");
                        calibration_step[i]++;

                        if (calibration_step[i] == ClientCalibrationStep.Done)
                        {
                            Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPointerEnable, Convert.ToUInt32(i));
                            Console.WriteLine("Enabling pointer");
                        }
                    }
                }
            }

            pointerDisplayControlLaser.Invalidate();
            pointerDisplayControlLaser.Update();
        }

        private void updateTabPagePosition(PSMoveSharpState state)
        {
            checkBoxPosition1.Checked = (state.positionPointerStates[0].valid == 1);
            checkBoxPosition2.Checked = (state.positionPointerStates[1].valid == 1);
            checkBoxPosition3.Checked = (state.positionPointerStates[2].valid == 1);
            checkBoxPosition4.Checked = (state.positionPointerStates[3].valid == 1);

            labelPositionPos1ValX.Text = (state.positionPointerStates[0].valid == 1) ? state.positionPointerStates[0].normalized_x.ToString("N3") : "N/A";
            labelPositionPos1ValY.Text = (state.positionPointerStates[0].valid == 1) ? state.positionPointerStates[0].normalized_y.ToString("N3") : "N/A";
            labelPositionPos2ValX.Text = (state.positionPointerStates[1].valid == 1) ? state.positionPointerStates[1].normalized_x.ToString("N3") : "N/A";
            labelPositionPos2ValY.Text = (state.positionPointerStates[1].valid == 1) ? state.positionPointerStates[1].normalized_y.ToString("N3") : "N/A";
            labelPositionPos3ValX.Text = (state.positionPointerStates[2].valid == 1) ? state.positionPointerStates[2].normalized_x.ToString("N3") : "N/A";
            labelPositionPos3ValY.Text = (state.positionPointerStates[2].valid == 1) ? state.positionPointerStates[2].normalized_y.ToString("N3") : "N/A";
            labelPositionPos4ValX.Text = (state.positionPointerStates[3].valid == 1) ? state.positionPointerStates[3].normalized_x.ToString("N3") : "N/A";
            labelPositionPos4ValY.Text = (state.positionPointerStates[3].valid == 1) ? state.positionPointerStates[3].normalized_y.ToString("N3") : "N/A";

            for (int i = 0; i < PSMoveSharpState.PSMoveSharpNumMoveControllers; i++)
            {
                UInt16 just_pressed;

                {
                    UInt16 changed_buttons = (UInt16)(state.gemStates[i].pad.digitalbuttons ^ last_buttons[i]);
                    just_pressed = (UInt16)(changed_buttons & state.gemStates[i].pad.digitalbuttons);
                    last_buttons[i] = state.gemStates[i].pad.digitalbuttons;
                }

                pointerDisplayControlPosition.valid[i] = (state.positionPointerStates[i].valid == 1);

                if (state.positionPointerStates[i].valid == 1)
                {
                    pointerDisplayControlPosition.pointer_x[i] = state.positionPointerStates[i].normalized_x;
                    pointerDisplayControlPosition.pointer_y[i] = state.positionPointerStates[i].normalized_y;
                    
                    const int PadSelect = 1;
                    const int PadTriangle = 1 << 4;

                    if ((just_pressed & PadSelect) == PadSelect)
                    {
                        Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPositionPointerDisable, Convert.ToUInt32(i));
                        Console.WriteLine("Disabling pointer");
                        calibration_step[i] = ClientCalibrationStep.Left;
                    }
                }
                else
                {
                    const int PadTick = 1 << 2;
                    const int PadTrigger = 1 << 1;

                    if ((just_pressed & PadTick) == PadTick && (last_buttons[i] & PadTrigger) == PadTrigger)
                    {
                        switch (calibration_step[i])
                        {
                            case ClientCalibrationStep.Left:
                                Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPositionPointerSetLeft, Convert.ToUInt32(i));
                                break;
                            case ClientCalibrationStep.Right:
                                Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPositionPointerSetRight, Convert.ToUInt32(i));
                                break;
                            case ClientCalibrationStep.Bottom:
                                Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPositionPointerSetBottom, Convert.ToUInt32(i));
                                break;
                            case ClientCalibrationStep.Top:
                                Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPositionPointerSetTop, Convert.ToUInt32(i));
                                break;
                        }

                        Console.WriteLine("Calibration tick");
                        calibration_step[i]++;

                        if (calibration_step[i] == ClientCalibrationStep.Done)
                        {
                            Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestPositionPointerEnable, Convert.ToUInt32(i));
                            Console.WriteLine("Enabling pointer");
                        }
                    }
                }
            }

            pointerDisplayControlPosition.Invalidate();
            pointerDisplayControlPosition.Update();
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
            if (!Program.client_connected)
            {
                try
                {
                    Program.client_connect(textBoxServerAddress.Text, Convert.ToInt32(textBoxServerPort.Text));
                }
                catch
                {}
            }
            else
            {
                Program.client_disconnect();
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (!Program.client_paused)
            {
                Program.moveClient.Pause();
            }
            else
            {
                Program.moveClient.Resume();
            }

            Program.client_paused = !Program.client_paused;
        }

        private void comboBoxDelay_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.update_delay = Convert.ToUInt32(comboBoxDelay.SelectedItem.ToString());
            Program.moveClient.DelayChange(Program.update_delay);
        }

        private void comboBoxDelay_TextUpdate(object sender, EventArgs e)
        {
            try
            {
                uint input = Convert.ToUInt32(Math.Min(Math.Max(Convert.ToInt32(comboBoxDelay.Text), 1), 255));

                Program.moveClient.DelayChange(input);
                comboBoxDelay.Text = input.ToString();
            }
            catch
            {}
        }

        private void comboBoxSelectedMove_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.selected_move = Convert.ToInt32(comboBoxSelectedMove.SelectedItem.ToString()) - 1;
        }

        private void comboBoxSelectedMove_TextUpdate(object sender, EventArgs e)
        {
            try
            {
                int input = Math.Min(Math.Max(Convert.ToInt32(comboBoxSelectedMove.Text), 1), PSMoveSharpState.PSMoveSharpNumMoveControllers);

                Program.selected_move = input - 1;
                comboBoxSelectedMove.Text = input.ToString();
            }
            catch
            {}
        }

        private void comboBoxSelectedNav_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.selected_nav = Convert.ToInt32(comboBoxSelectedNav.SelectedItem.ToString()) - 1;
        }

        private void comboBoxSelectedNav_TextUpdate(object sender, EventArgs e)
        {
            try
            {
                int input = Math.Min(Math.Max(Convert.ToInt32(comboBoxSelectedNav.Text), 1), PSMoveSharpNavInfo.PSMoveSharpNumNavControllers);

                Program.selected_nav = input - 1;
                comboBoxSelectedNav.Text = input.ToString();
            }
            catch
            {}
        }

        private void buttonResetEnabled_Click(object sender, EventArgs e)
        {
            Program.reset_enabled[Program.selected_move] = !Program.reset_enabled[Program.selected_move];
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Console.WriteLine("Rumble = {0}", rumbleBar.Value);
            uint gem_num = (uint)Program.selected_move;
            uint rumble = (uint)rumbleBar.Value;
            Program.moveClient.SendRequestPacket(PSMoveClient.ClientRequest.PSMoveClientRequestSetRumble, gem_num, rumble);
        }

        private void ImagePausedToggleButton_Click(object sender, EventArgs e)
        {
            if (Program.image_paused)
            {
                Program.moveClient.CameraFrameResume();
                Program.image_paused = false;
                Program.moveClient.ForceRGB(0, 1.0f, 0.0f, 0.0f);
            }
            else
            {
                Program.moveClient.CameraFramePause();
                Program.image_paused = true;
                Program.moveClient.ForceRGB(0, 1.0f, 1.0f, 0.0f);
            }

            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            int selected_index = comboBox.SelectedIndex;
            string selected_name = (string)comboBox.SelectedItem;
            Console.WriteLine("{0} @ {1}", selected_name, selected_index);
            uint slices = (uint)selected_index + 1;
            if (slices < 1)
            {
                slices = 1;
            }
            else if (slices > 8) {
                slices = 8;
            }
            if (Program.moveClient != null)
            {
                Program.moveClient.CameraFrameSetNumSlices(slices);
                Program.moveClient.SetNumImageSlices((int)slices);
            }
        }

        private void textBoxServerAddress_TextChanged(object sender, EventArgs e)
        {

        }

        private void calibrateButton_Click(object sender, EventArgs e)
        {
            if (Program.moveClient != null)
            {
                Program.moveClient.CalibrateController(Program.selected_move);
            }
        }

        private void trackAllHuesButton_Click(object sender, EventArgs e)
        {
            if (Program.moveClient != null)
            {
                Program.moveClient.TrackAllHues();
            }
        }

    }

    public class PointerDisplayControl : Control
    {
        public float[] pointer_x;
        public float[] pointer_y;
        public bool[] valid;

        public PointerDisplayControl()
        {
            DoubleBuffered = true;

            pointer_x = new float[PSMoveSharpState.PSMoveSharpNumMoveControllers];
            pointer_y = new float[PSMoveSharpState.PSMoveSharpNumMoveControllers];
            valid = new bool[PSMoveSharpState.PSMoveSharpNumMoveControllers];
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics gr = e.Graphics;
            Rectangle rc = this.ClientRectangle;

            float length = rc.Width / 4.0f;
            float height = rc.Height / 4.0f;

            rc.X += 4;
            rc.Y += 4;
            rc.Width -= 8;
            rc.Height -= 8;

            gr.FillRectangle(new SolidBrush(Color.Black), rc);

            Pen linePen = new Pen(Brushes.DeepSkyBlue);
            linePen.Width = 4.0f;

            PointF[] corners = new PointF[5];
            corners[0] = new PointF(rc.X + length, rc.Y + height * 3);
            corners[1] = new PointF(rc.X + length * 3, rc.Y + height * 3);
            corners[2] = new PointF(rc.X + length * 3, rc.Y + height);
            corners[3] = new PointF(rc.X + length, rc.Y + height);
            corners[4] = new PointF(rc.X + length, rc.Y + height * 3);
            gr.DrawLines(linePen, corners);

            if (Program.moveClient != null)
            {
                PSMoveSharpState state = Program.moveClient.GetLatestState();

                for (int i = 0; i < PSMoveSharpState.PSMoveSharpNumMoveControllers; i++)
                {
                    if (valid[i])
                    {
                        SolidBrush pointerBrush = new SolidBrush(Color.FromArgb(Convert.ToInt32(255.0 * state.sphereStates[i].r),
                                                                                Convert.ToInt32(255.0 * state.sphereStates[i].g),
                                                                                Convert.ToInt32(255.0 * state.sphereStates[i].b)));

                        RectangleF pointerRC = new RectangleF();

                        pointerRC.X = rc.X + (length * 2) + (pointer_x[i] * length * 2);
                        pointerRC.Y = rc.Y + (height * 2) + (-pointer_y[i] * height * 2);
                        pointerRC.Width = 8.0f;
                        pointerRC.Height = 8.0f;

                        gr.FillEllipse(pointerBrush, pointerRC);
                    }
                }
            }
        }
    }
}
