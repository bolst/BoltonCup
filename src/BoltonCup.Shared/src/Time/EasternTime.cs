using System;

namespace BoltonCup.Shared;

/// <summary>The tournament's local time zone, shared by the API and the clients so both agree on day boundaries.</summary>
public static class EasternTime
{
    public static TimeZoneInfo GetTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }
    }
}