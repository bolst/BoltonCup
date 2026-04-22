namespace BoltonCup.Common;

public static class DateTimeExtensions
{
    public static TimeZoneInfo GetEasternTimeZone()
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

    public static DateTime ConvertEstToUtc(this DateTime dateTime)
    {
        var estZone = GetEasternTimeZone();
        return TimeZoneInfo.ConvertTimeToUtc(dateTime, estZone);
    }
    
    public static DateTime ToEst(this DateTime dateTime)
    {
        var utcDate = dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : dateTime.ToUniversalTime();

        var estZone = GetEasternTimeZone();
        var estTime = TimeZoneInfo.ConvertTimeFromUtc(utcDate, estZone);
        return estTime;
    }
    
    public static string ToEstString(this DateTime dateTime, string format = "g")
    {
        return dateTime.ToEst().ToString(format);
    }

    public static string ToEstString(this DateTime? dateTime, string format = "g")
    {
        return dateTime == null 
            ? string.Empty 
            : dateTime.Value.ToEstString(format);
    }
}