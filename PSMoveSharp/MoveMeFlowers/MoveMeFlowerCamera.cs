using System;
using System.Threading;
using System.Collections.Generic;
using PSMoveSharp;

namespace MoveMeFlowers
{
    class MoveMeFlowerCamera
    {
        protected float _max_angle_z;
        protected float _min_angle_z;

        protected float _delta_angle;

        protected float _min_distance;
        protected float _max_distance;
        protected float _delta_distance;


        protected float _angle_x;
        protected float _angle_z;
        protected float _distance;
        
        
        protected int _digitalpad1;
        protected int _digitalpad2;

        protected bool _sphere_visible;

        public MoveMeFlowerCamera()
        {
            _min_angle_z = -1.48352986f;
            _max_angle_z = 1.48352986f;

            _delta_angle = 0.0610865238f; // 3.5 degrees

            _min_distance = 50.0f;
            _max_distance = 10000.0f;
            _delta_distance = 50.0f;

            _angle_x = 0.0f;
            _angle_z = 0.0f;
            _distance = _min_distance;
        }

        float Clamp(float f, float min, float max)
        {
            if (f < min)
            {
                return min;
            } else if (f > max) {
                return max;
            }

            return f;
        }

        float ZeroInRange(float f, float min_range, float max_range)
        {
            if (f > min_range && f < max_range)
            {
                return 0.0f;
            }
            return f;
        }

        void RotateX(float scale)
        { 
            // scale from -1.0f to 1.0f
            _angle_x += _delta_angle * scale;
        }

        void RotateZ(float scale)
        {
            // scale from -1.0f to 1.0f
            _angle_z += _delta_angle * scale;
            _angle_z = Clamp(_angle_z, _min_angle_z, _max_angle_z);
        }

        void ZoomIn(float scale)
        {
            _distance += _delta_distance * scale;
            _distance = Clamp(_distance, _min_distance, _max_distance);
        }

        void ZoomOut(float scale)
        {
            _distance -= _delta_distance * scale;
            _distance = Clamp(_distance, _min_distance, _max_distance);
        }

        public float GetAngleX()
        {
            return _angle_x;
        }

        public float GetAngleZ()
        {
            return _angle_z;
        }

        public float GetDistance()
        {
            return _distance;
        }

        public bool SphereVisible()
        {
            return _sphere_visible;
        }

        public void UpdateState(PSMoveSharpState state)
        {
            // not connected
            if ((state.navInfo.port_status[0] & 0x1) == 0)
            {
                return;
            }

            int digitalpad1_changed = (int)state.padData[0].button[2] ^ _digitalpad1;
            int digitalpad1_pressed = digitalpad1_changed & (int)state.padData[0].button[2];
            int digitalpad1_released = digitalpad1_changed & (int)~state.padData[0].button[2];

            int digitalpad2_changed = (int)state.padData[0].button[3] ^ _digitalpad2;
            int digitalpad2_pressed = digitalpad2_changed & (int)state.padData[0].button[3];
            int digitalpad2_released = digitalpad2_changed & (int)~state.padData[0].button[3];

            _digitalpad1 = (int)state.padData[0].button[2];
            _digitalpad2 = (int)state.padData[0].button[3];

            bool cross_released = (digitalpad2_released & (1 << 6)) != 0;
            bool circle_released = (digitalpad2_released & (1 << 5)) != 0;
            bool dpad_down_released = (digitalpad1_released & (1 << 6)) != 0;
 
            float pressure_L1 = (float)state.padData[0].button[16] / 255.0f;
            float pressure_L2 = (float)state.padData[0].button[18] / 255.0f;

            float left_right = (float)state.padData[0].button[6] / 255.0f;
            float up_down = (float)state.padData[0].button[7] / 255.0f;
            left_right *= 2.0f;
            up_down *= 2.0f;
            left_right -= 1.0f;
            up_down -= 1.0f;
            up_down *= -1.0f;
            left_right = ZeroInRange(left_right, -0.25f, 0.25f);
            up_down = ZeroInRange(up_down, -0.25f, 0.25f);

            RotateX(left_right);
            RotateZ(up_down);
            ZoomIn(pressure_L1);
            ZoomOut(pressure_L2);

            if (dpad_down_released)
            {
                Program.flower.Reset();
            }

            if (cross_released)
            {
                Program.flower.NextCapture();
            }

            if (circle_released)
            {
                Program.flower.ToggleCaptureState();
            }

            _sphere_visible = state.imageStates[0].visible != 0;
        }
    }
}