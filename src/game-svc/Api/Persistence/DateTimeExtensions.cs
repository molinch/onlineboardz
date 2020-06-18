using System;

namespace Api.Persistence
{
    public static class DateTimeExtensions
    {
        public static string ToIso(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("u").Replace(' ', 'T');
        }
    }
}
