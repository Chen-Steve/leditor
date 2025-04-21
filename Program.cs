using System;
using System.Windows.Forms;

namespace LightNovelEditor
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Load environment variables from .env file
                DotNetEnv.Env.Load();

                // Initialize Supabase configuration from environment variables
                SupabaseConfig.Url = DotNetEnv.Env.GetString("SUPABASE_URL");
                SupabaseConfig.Key = DotNetEnv.Env.GetString("SUPABASE_KEY");

                if (string.IsNullOrEmpty(SupabaseConfig.Url) || string.IsNullOrEmpty(SupabaseConfig.Key))
                {
                    throw new Exception("Supabase configuration is missing. Please check your .env file.");
                }

                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application error: {ex.Message}\n\nStack trace: {ex.StackTrace}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 