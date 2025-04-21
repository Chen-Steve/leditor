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