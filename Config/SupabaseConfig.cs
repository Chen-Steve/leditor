namespace LightNovelEditor
{
    public static class SupabaseConfig
    {
        public static string Url { get; set; } = EnvironmentConfig.GetValue("SUPABASE_URL");
        public static string Key { get; set; } = EnvironmentConfig.GetValue("SUPABASE_KEY");
        public static string? AccessToken { get; set; }
    }
} 