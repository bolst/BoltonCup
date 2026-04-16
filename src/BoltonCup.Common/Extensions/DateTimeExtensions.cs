namespace BoltonCup.Common;

public static class DateTimeExtensions
{
    public static string ToEstString(this DateTime dateTime, string format = "g")
    {
        var utcDate = dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : dateTime.ToUniversalTime();

        TimeZoneInfo estZone;
        try
        {
            estZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        }
        catch (TimeZoneNotFoundException)
        {
            estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }

        var estTime = TimeZoneInfo.ConvertTimeFromUtc(utcDate, estZone);
        return estTime.ToString(format);
    }

    public static string ToEstString(this DateTime? dateTime, string format = "g")
    {
        return dateTime == null 
            ? string.Empty 
            : dateTime.Value.ToEstString(format);
    }
}