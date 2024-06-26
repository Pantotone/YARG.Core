using System;

namespace YARG.Core.Engine
{
    public struct EngineTimer
    {
        private double _startTime;
        private double _speed;

        public readonly double TimeThreshold;

        public readonly double StartTime => _startTime;
        public readonly double EndTime => _startTime + TimeThreshold * _speed;

        public EngineTimer(double threshold)
        {
            _startTime = double.MaxValue;
            TimeThreshold = threshold;
            _speed = 1.0;
        }

        public void Start(double currentTime)
            => Start(ref _startTime, currentTime);

        public void StartWithOffset(double currentTime, double offset)
            => StartWithOffset(ref _startTime, currentTime, TimeThreshold * _speed, offset);

        public void Reset()
            => Reset(ref _startTime);

        public readonly bool IsActive(double currentTime)
            => IsActive(_startTime, currentTime, TimeThreshold * _speed);

        public readonly bool IsExpired(double currentTime)
            => IsExpired(_startTime, currentTime, TimeThreshold * _speed);

        public void SetSpeed(double speed)
        {
            _speed = speed;
        }

        public static void Start(ref double startTime, double currentTime)
        {
            startTime = currentTime;
        }

        public static void StartWithOffset(ref double startTime, double currentTime, double threshold, double offset)
        {
            double diff = Math.Abs(threshold - offset);
            startTime = currentTime - diff;
        }

        public static void Reset(ref double startTime)
        {
            startTime = double.MaxValue;
        }

        public static bool IsActive(double startTime, double currentTime, double threshold)
        {
            double elapsed = currentTime - startTime;
            return elapsed < threshold && elapsed >= 0;
        }

        public static bool IsExpired(double startTime, double currentTime, double threshold)
        {
            return currentTime - startTime >= threshold;
        }
    }
}