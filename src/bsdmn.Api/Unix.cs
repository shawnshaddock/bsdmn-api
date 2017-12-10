using System;

namespace bsdmn.Api
{
    public static class Unix
    {
        public static DateTime Epoch { get; } = new DateTime(1970, 1, 1);

        public static DateTime GetTimestampFromSeconds(int seconds)
        {
            return Epoch.AddSeconds(seconds);
        }
    }
}