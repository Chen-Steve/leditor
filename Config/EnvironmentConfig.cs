using System;
using System.IO;
using System.Collections.Generic;

namespace LightNovelEditor
{
    public static class EnvironmentConfig
    {
        private static readonly Dictionary<string, string> _variables = new();

        static EnvironmentConfig()
        {
            LoadEnvFile();
        }

        private static void LoadEnvFile()
        {
            try
            {
                string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
                if (File.Exists(envPath))
                {
                    foreach (string line in File.ReadAllLines(envPath))
                    {
                        string trimmedLine = line.Trim();
                        if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("#"))
                        {
                            int equalIndex = trimmedLine.IndexOf('=');
                            if (equalIndex > 0)
                            {
                                string key = trimmedLine.Substring(0, equalIndex).Trim();
                                string value = trimmedLine.Substring(equalIndex + 1).Trim();

                                // Remove quotes if present
                                if (value.StartsWith("\"") && value.EndsWith("\""))
                                {
                                    value = value.Substring(1, value.Length - 2);
                                }

                                _variables[key] = value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error loading .env file: {ex.Message}", "Configuration Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        public static string GetValue(string key, string defaultValue = "")
        {
            return _variables.TryGetValue(key, out string? value) ? value : defaultValue;
        }
    }
} 