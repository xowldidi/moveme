using System;
using System.Threading;
using System.Collections.Generic;
using PSMoveSharp;

namespace MoveMeFlowers
{
    struct MoveSample
    {
        public float x;
        public float y;
        public float z;
        public long time;
    }

    static class MoveMeFlowerStateConstants
    {
        public const int MAX_FLOWERS = 8;
    }
    class MoveMeFlowerState
    {
        protected int _buffer_size;
        protected ReaderWriterLock _update_lock;
        protected LinkedList<MoveSample>[] _samples;
        protected int _capture_state;
        protected int _capture_slot;

        public MoveMeFlowerState(int buffer_size)  
        {
            _buffer_size = buffer_size;
            _update_lock = new ReaderWriterLock();
            _samples = new LinkedList<MoveSample>[MoveMeFlowerStateConstants.MAX_FLOWERS];
            for (int i = 0; i < MoveMeFlowerStateConstants.MAX_FLOWERS; i++)
            {
                _samples[i] = new LinkedList<MoveSample>();
            }
            _capture_state = 0;
        }

        public void UpdateState(PSMoveSharpState state)
        {
            if (_capture_state == 1)
            {
                // we are not capturing when in capture state 1
                return;
            }

            _update_lock.AcquireWriterLock(-1);
            if (_samples[_capture_slot].Count >= _buffer_size)
            {
                // remove the first (oldest sample)
                _samples[_capture_slot].RemoveFirst();
            }
            MoveSample sample;
            sample.x = state.gemStates[0].pos.x;
            sample.y = state.gemStates[0].pos.y;
            sample.z = state.gemStates[0].pos.z;
            sample.time = state.gemStates[0].timestamp;
            _samples[_capture_slot].AddLast(sample);
            _update_lock.ReleaseWriterLock();
        }

        public void Reset()
        {
            _update_lock.AcquireWriterLock(-1);
            for (int i = 0; i < MoveMeFlowerStateConstants.MAX_FLOWERS; i++)
            {
                _samples[i].Clear();
            }
            _update_lock.ReleaseWriterLock();
        }

        public void NextCapture()
        {
            _capture_slot++;
            if (_capture_slot >= MoveMeFlowerStateConstants.MAX_FLOWERS)
            {
                _capture_slot = 0;
            }
            // clear these samples
            _samples[_capture_slot].Clear();
        }

        public void ToggleCaptureState()
        {
            switch (_capture_state)
            {
                case 0: // capturing
                    // want to stop
                    
                    _capture_state = 1;
                    break;
                case 1: // not capturing
                    // want to start
                    // move to next capture
                    _capture_state = 0;
                    break;
                default:
                    // oh. better reset
                    _capture_state = 0;
                    break;
            }
        }

        public void StartRead()
        {
            _update_lock.AcquireReaderLock(-1);
        }

        public void StopRead()
        {
            _update_lock.ReleaseReaderLock();
        }

        public int GetCurrentSlot()
        {
            return _capture_slot;
        }

        public bool Capturing()
        {
            return _capture_state == 0;
        }

        public LinkedList<MoveSample> GetSamples(int i)
        {
            return _samples[i];
        }
    }
}