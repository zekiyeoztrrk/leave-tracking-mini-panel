namespace IzinTakipPaneli.Services
{
    public interface IWorkingDayCalculator
    {
        int CountBusinessDays(DateTime start, DateTime end); // start ve end dahil
        bool IsHoliday(DateTime date);
        bool IsWeekend(DateTime date);
    }
}
