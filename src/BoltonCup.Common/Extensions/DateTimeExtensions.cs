namespace BoltonCup.Common;

public static class DateTimeExtensions
{
    public static string ToEstString(this DateTime? dateTime, string format = "g")
    {
        if (dateTime == null)
            return string.Empty;
        
        var utcDate = dateTime.Value.Kind == DateTimeKind.Utc
            ? dateTime.Value
            : dateTime.Value.ToUniversalTime();

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
}