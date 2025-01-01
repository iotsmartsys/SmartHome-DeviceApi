using System.Globalization;

namespace Api.Models;

public class TimezoneResponse
{
    public TimezoneResponse(string timezone)
    {
        this.timezone =  timezone;
        this.day_of_week = (int)CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(DateTime.Now);
        this.day_of_year = DateTime.Now.DayOfYear;
        this.datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
        this.utc_datetime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
        this.week_number = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
    }
    public string timezone { get; set; }
    public int day_of_week { get; set; }
    public int day_of_year { get; set; }
    public string datetime { get; set; }
    public string utc_datetime { get; set; }
    public int week_number { get; set; }
}
