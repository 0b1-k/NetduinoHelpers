using System;
using Microsoft.SPOT;

namespace PeteBrown.Sixty4Racer
{
    class Sprite
    {
        private readonly byte[] _frames;
        private const int Width = 8;
        private const int Height = 8;
        private int _frameCount;

        public Sprite (byte[] frames)
        {
            _frames = frames;

            _frameCount = _frames.Length / Height;
        }

        public int FrameCount
        {
            get { return _frameCount; }
        }

        public int CurrentFrame
        {
            get { return _currentFrame; }
        }

        public void Reset()
        {
            _currentFrame = 0;
        }

        private int _currentFrame = 0;
        public byte[] GetNextFrame()
        {
            return GetFrame(_currentFrame++);
        }

        public byte[] GetFrame(int index)
        {
            if (index >= FrameCount)
                throw new IndexOutOfRangeException("Index must be less than " + FrameCount);

            byte[] b = new byte[Height];

            for (int i = 0; i < Height; i++)
            {
                b[i] = _frames[index * Height + i]; 
            }

            return b;
        }
    }
}
