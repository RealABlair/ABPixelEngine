using System;
using static ABSoftware.ABPixelEngine.Native;

namespace ABSoftware.ABPixelEngine
{
    public readonly struct EngineTime
    {
        public static readonly double Frequency;
        public readonly long Ticks;

        static EngineTime()
        {
            QueryPerformanceFrequency(out long freq);
            Frequency = freq;
        }

        private EngineTime(long ticks)
        {
            Ticks = ticks;
        }

        public static EngineTime Now 
        { 
            get 
            {
                QueryPerformanceCounter(out long ticks);
                return new EngineTime(ticks);
            } 
        }

        public float Hours => (float)((Ticks / Frequency) / 3600.0);
        public float Minutes => (float)((Ticks / Frequency) / 60.0);
        public float Seconds => (float)(Ticks / Frequency);
        public float Milliseconds => (float)((Ticks * 1000.0) / Frequency);

        public static EngineTime operator +(EngineTime left, EngineTimeSpan right) => new EngineTime(left.Ticks + right.DeltaTicks);
        public static EngineTime operator -(EngineTime left, EngineTimeSpan right) => new EngineTime(left.Ticks - right.DeltaTicks);
        public static EngineTimeSpan operator -(EngineTime left, EngineTime right) => new EngineTimeSpan(left.Ticks - right.Ticks);
        public static bool operator >(EngineTime left, EngineTime right) => left.Ticks > right.Ticks;
        public static bool operator <(EngineTime left, EngineTime right) => left.Ticks < right.Ticks;
        public static bool operator >=(EngineTime left, EngineTime right) => left.Ticks >= right.Ticks;
        public static bool operator <=(EngineTime left, EngineTime right) => left.Ticks <= right.Ticks;
        public static bool operator ==(EngineTime left, EngineTime right) => left.Ticks == right.Ticks;
        public static bool operator !=(EngineTime left, EngineTime right) => left.Ticks != right.Ticks;

        public override int GetHashCode() => Ticks.GetHashCode();
        public override bool Equals(object obj) => obj is EngineTime t && Ticks == t.Ticks;
    }
}