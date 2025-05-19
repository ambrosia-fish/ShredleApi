using System.Text.Json;
using System.Text;
using ShredleApi.Models;

namespace ShredleApi.Services
{
    public class SupabaseService
    {
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;

        public SupabaseService(IConfiguration configuration)
        {
            _supabaseUrl = configuration["Supabase:Url"]!;
            _supabaseKey = configuration["Supabase:Key"]!;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
        }

        public async Task<List<Solo>> GetSolosAsync()
        {
            var response = await _httpClient.GetAsync($"{_supabaseUrl}/rest/v1/solos?select=*");

            if (!response.IsSuccessStatusCode)
                return new List<Solo>();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Solo>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Solo>();
        }

        public async Task<Solo?> GetSoloByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_supabaseUrl}/rest/v1/solos?id=eq.{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var solos = JsonSerializer.Deserialize<List<Solo>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return solos?.FirstOrDefault();
        }

        public async Task<DailyGame?> GetDailyGameAsync(DateTime date)
        {
            var formattedDate = date.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"{_supabaseUrl}/rest/v1/daily_games?date=eq.{formattedDate}");

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var games = JsonSerializer.Deserialize<List<DailyGame>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return games?.FirstOrDefault();
        }

        public async Task<DailyGame?> GetTodaysDailyGameAsync()
        {
            var todayDate = DateTime.UtcNow.Date;
            return await GetDailyGameAsync(todayDate);
        }

        public async Task<List<DailyGame>> GetRecentDailyGamesAsync(int days)
        {
            // Get daily games from the last 'days' days
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var formattedDate = startDate.ToString("yyyy-MM-dd");
            
            var response = await _httpClient.GetAsync(
                $"{_supabaseUrl}/rest/v1/daily_games?date=gte.{formattedDate}&order=date.desc");
            
            if (!response.IsSuccessStatusCode)
                return new List<DailyGame>();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DailyGame>>(content, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<DailyGame>();
        }

        public async Task<DailyGame?> CreateDailyGameAsync(DailyGame dailyGame)
        {
            var json = JsonSerializer.Serialize(dailyGame);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_supabaseUrl}/rest/v1/daily_games", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DailyGame>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> UpdateDailyGameAsync(DailyGame dailyGame)
        {
            var json = JsonSerializer.Serialize(dailyGame);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync($"{_supabaseUrl}/rest/v1/daily_games?id=eq.{dailyGame.Id}", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<Solo?> CreateSoloAsync(Solo solo)
        {
            var json = JsonSerializer.Serialize(solo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_supabaseUrl}/rest/v1/solos", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Solo>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> UpdateSoloAsync(Solo solo)
        {
            var json = JsonSerializer.Serialize(solo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync($"{_supabaseUrl}/rest/v1/solos?id=eq.{solo.Id}", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSoloAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_supabaseUrl}/rest/v1/solos?id=eq.{id}");

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDailyGameAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_supabaseUrl}/rest/v1/daily_games?id=eq.{id}");

            return response.IsSuccessStatusCode;
        }
    }
}