using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace LightNovelEditor
{
    public class LoginForm : Form
    {
        private readonly TextBox emailTextBox;
        private readonly TextBox passwordTextBox;
        private bool skippedLogin = false;
        private readonly HttpClient httpClient;

        public static bool IsLoggedIn { get; private set; }
        public bool SkippedLogin => skippedLogin;

        public LoginForm()
        {
            httpClient = new HttpClient();
            Text = "Login";
            Size = new Size(400, 300);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Email field
            var emailLabel = new Label
            {
                Text = "Email:",
                Location = new Point(20, 20),
                AutoSize = true
            };
            Controls.Add(emailLabel);

            emailTextBox = new TextBox
            {
                Location = new Point(20, 40),
                Width = 340,
                Font = new Font("Segoe UI", 10F)
            };
            Controls.Add(emailTextBox);

            // Password field
            var passwordLabel = new Label
            {
                Text = "Password:",
                Location = new Point(20, 80),
                AutoSize = true
            };
            Controls.Add(passwordLabel);

            passwordTextBox = new TextBox
            {
                Location = new Point(20, 100),
                Width = 340,
                PasswordChar = '•',
                Font = new Font("Segoe UI", 10F)
            };
            Controls.Add(passwordTextBox);

            // Login button
            var loginButton = new Button
            {
                Text = "Login",
                Location = new Point(20, 160),
                Width = 160,
                Height = 40,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            loginButton.Click += LoginButton_Click;
            Controls.Add(loginButton);

            // Skip login button
            var skipButton = new Button
            {
                Text = "Guest",
                Location = new Point(200, 160),
                Width = 160,
                Height = 40,
                Font = new Font("Segoe UI", 10F),
                FlatStyle = FlatStyle.Flat
            };
            skipButton.Click += SkipButton_Click;
            Controls.Add(skipButton);

            // Register link
            var registerLink = new LinkLabel
            {
                Text = "Don't have an account? Register here",
                Location = new Point(20, 220),
                AutoSize = true
            };
            registerLink.Click += RegisterLink_Click;
            Controls.Add(registerLink);

            AcceptButton = loginButton;
        }

        private class LoginRequest
        {
            [JsonProperty("email")]
            public string Email { get; set; } = "";

            [JsonProperty("password")]
            public string Password { get; set; } = "";
        }

        private class LoginResponse
        {
            [JsonProperty("access_token")]
            public string? AccessToken { get; set; }

            [JsonProperty("token_type")]
            public string? TokenType { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("refresh_token")]
            public string? RefreshToken { get; set; }
        }

        private async void LoginButton_Click(object? sender, EventArgs e)
        {
            try
            {
                string email = emailTextBox.Text.Trim();
                string password = passwordTextBox.Text;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please enter both email and password.", "Login Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cursor = Cursors.WaitCursor;
                // TODO: Implement actual login logic here
                await Task.Delay(1000); // Simulated login delay
                IsLoggedIn = true; // Set login state to true on successful login

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}", "Login Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void SkipButton_Click(object? sender, EventArgs e)
        {
            skippedLogin = true;
            IsLoggedIn = false; // Ensure logged out state when skipping
            DialogResult = DialogResult.OK;
            Close();
        }

        private void RegisterLink_Click(object? sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://lanry.space/auth",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open registration page: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 