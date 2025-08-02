using System.Collections.Concurrent;
using System.Text.Json;

namespace IzinTakipPaneli.Services
{
    /*====================================================================
      Bu sınıf, iki tarih arasındaki NET iş günü sayısını hesaplar.
      • Hafta sonlarını (Cumartesi-Pazar) düşer
      • Nager API (https://date.nager.at) üzerinden TR resmî tatillerini
        çekip bellekte yıl-bazlı cache’ler
    ====================================================================*/
    public class ApiWorkingDayCalculator : IWorkingDayCalculator
    {
        private readonly IHttpClientFactory _httpClientFactory;

        /*  Yıl  →  Tatil tarihleri  (in-memory cache)  */
        private readonly ConcurrentDictionary<int, HashSet<DateTime>> _holidayCache = new();

        public ApiWorkingDayCalculator(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /* İki tarih arasındaki iş günü adedi */
        public int CountBusinessDays(DateTime start, DateTime end)
        {
            if (end < start) return 0;

            int count = 0;
            var d = start.Date;
            var last = end.Date;

            while (d <= last)
            {
                /* Tatil veya hafta sonu değilse gün say */
                if (!IsWeekend(d) && !IsHoliday(d))
                    count++;

                d = d.AddDays(1);
            }
            return count;
        }

        /* Cumartesi / Pazar kontrolü */
        public bool IsWeekend(DateTime date) =>
            date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

        /* Senkron arayüz talebi – async metodu bloklayarak çağırıyoruz */
        public bool IsHoliday(DateTime date) =>
            IsHolidayAsync(date).GetAwaiter().GetResult();

        /* Belirli bir gün resmî tatil mi?  (async) */
        private async Task<bool> IsHolidayAsync(DateTime date)
        {
            var set = await GetHolidaysForYear(date.Year);
            return set.Contains(date.Date);
        }

        /* Bir yılın tatillerini API’den çek (veya cache’ten oku) */
        private async Task<HashSet<DateTime>> GetHolidaysForYear(int year)
        {
            /* Cache varsa direkt dön */
            if (_holidayCache.TryGetValue(year, out var cached))
                return cached;

            var client = _httpClientFactory.CreateClient();
            var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/TR";

            /* API çağrısı */
            using var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            /* JSON parse */
            await using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var set = new HashSet<DateTime>();
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                // Ör.  { "date": "2025-01-01", ... }
                if (el.TryGetProperty("date", out var dp))
                    set.Add(dp.GetDateTime().Date);
            }

            /* Cache’e ekle ve döndür */
            _holidayCache[year] = set;
            return set;
        }
    }
}
