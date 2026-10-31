using System;
using static ABSoftware.ABPixelEngine.Native;

namespace ABSoftware.ABPixelEngine
{
    public readonly struct EngineTimeSpan
    {
        public readonly long DeltaTicks;

        public EngineTimeSpan(long deltaTicks)
        {
            DeltaTicks = deltaTicks;
        }

        public EngineTimeSpan(double seconds)
        {
            DeltaTicks = (long)(seconds * EngineTime.Frequency);
        }

        public float Seconds => (float)(DeltaTicks / EngineTime.Frequency);
        public float Milliseconds => (float)((DeltaTicks * 1000.0) / EngineTime.Frequency);

        public static EngineTimeSpan operator *(EngineTimeSpan left, float scale) => new EngineTimeSpan((long)(left.DeltaTicks * scale));
        public static EngineTimeSpan operator *(float scale, EngineTimeSpan left) => left * scale;
        public static EngineTimeSpan operator /(EngineTimeSpan left, float scale) => new EngineTimeSpan((long)(left.DeltaTicks / scale));
        public static bool operator >(EngineTimeSpan left, EngineTimeSpan right) => left.DeltaTicks > right.DeltaTicks;
        public static bool operator <(EngineTimeSpan left, EngineTimeSpan right) => left.DeltaTicks < right.DeltaTicks;
        public static bool operator >=(EngineTimeSpan left, EngineTimeSpan right) => left.DeltaTicks >= right.DeltaTicks;
        public static bool operator <=(EngineTimeSpan left, EngineTimeSpan right) => left.DeltaTicks <= right.DeltaTicks;
        public static bool operator ==(EngineTimeSpan left, EngineTimeSpan right) => left.DeltaTicks == right.DeltaTicks;
        public static bool operator !=(EngineTimeSpan left, EngineTimeSpan right) => left.DeltaTicks != right.DeltaTicks;

        public override int GetHashCode() => DeltaTicks.GetHashCode();
        public override bool Equals(object obj) => obj is EngineTimeSpan t && DeltaTicks == t.DeltaTicks;
    }
}
