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
        public static string? CurrentUserAvatarUrl { get; private set; }
        public static string? CurrentUsername { get; private set; }
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

            [JsonProperty("user")]
            public User? User { get; set; }
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

                MessageBox.Show("Starting login process...", "Debug");
                
                // First get the user data
                var loginRequest = new LoginRequest
                {
                    Email = email,
                    Password = password
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(loginRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("apikey", SupabaseConfig.Key);
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.PostAsync(
                    $"{SupabaseConfig.Url}/auth/v1/token?grant_type=password",
                    content
                );

                var responseContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Login response status: {response.StatusCode}\nContent: {responseContent}", "Debug");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                    if (result?.AccessToken != null && result.User?.Id != null)
                    {
                        // Get profile data directly using the user ID from the login response
                        httpClient.DefaultRequestHeaders.Clear();
                        httpClient.DefaultRequestHeaders.Add("apikey", SupabaseConfig.Key);
                        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {result.AccessToken}");
                        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");

                        // Query the profiles table using the user ID
                        var profileResponse = await httpClient.GetAsync(
                            $"{SupabaseConfig.Url}/rest/v1/profiles?id=eq.{result.User.Id}&select=*"
                        );
                        var profileContent = await profileResponse.Content.ReadAsStringAsync();
                        MessageBox.Show($"Profile request URL: {SupabaseConfig.Url}/rest/v1/profiles?id=eq.{result.User.Id}\nHeaders: {string.Join(", ", httpClient.DefaultRequestHeaders)}\nResponse status: {profileResponse.StatusCode}\nProfile response: {profileContent}", "Debug - Profile Request");

                        if (profileResponse.IsSuccessStatusCode)
                        {
                            var profiles = JsonConvert.DeserializeObject<List<ProfileResponse>>(profileContent);

                            if (profiles?.Count > 0)
                            {
                                CurrentUserAvatarUrl = profiles[0].AvatarUrl;
                                CurrentUsername = profiles[0].Username;
                                MessageBox.Show($"Profile data found:\nUsername: {CurrentUsername}\nAvatar URL: {CurrentUserAvatarUrl}\nRole: {profiles[0].Role}\nCoins: {profiles[0].Coins}", "Debug - Profile Data");
                            }
                            else
                            {
                                MessageBox.Show("No profile data found in the response", "Debug - Profile Data");
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Profile request failed:\nStatus: {profileResponse.StatusCode}\nContent: {profileContent}", "Debug - Profile Error");
                        }

                        SupabaseConfig.AccessToken = result.AccessToken;
                        IsLoggedIn = true;
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        throw new Exception("Login failed: No access token or user ID received");
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Login failed: {error}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex}", "Debug Error");
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
            IsLoggedIn = false;
            CurrentUserAvatarUrl = null;
            CurrentUsername = null;
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

        private class UserResponse
        {
            [JsonProperty("user")]
            public User? User { get; set; }
        }

        private class User
        {
            [JsonProperty("id")]
            public string? Id { get; set; }
        }

        private class ProfileResponse
        {
            [JsonProperty("id")]
            public string? Id { get; set; }

            [JsonProperty("username")]
            public string? Username { get; set; }

            [JsonProperty("avatar_url")]
            public string? AvatarUrl { get; set; }

            [JsonProperty("role")]
            public string? Role { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public DateTime UpdatedAt { get; set; }

            [JsonProperty("current_streak")]
            public int CurrentStreak { get; set; }

            [JsonProperty("last_visit")]
            public DateTime? LastVisit { get; set; }

            [JsonProperty("kofi_url")]
            public string? KofiUrl { get; set; }

            [JsonProperty("patreon_url")]
            public string? PatreonUrl { get; set; }

            [JsonProperty("custom_url")]
            public string? CustomUrl { get; set; }

            [JsonProperty("custom_url_label")]
            public string? CustomUrlLabel { get; set; }

            [JsonProperty("author_bio")]
            public string? AuthorBio { get; set; }

            [JsonProperty("coins")]
            public decimal Coins { get; set; }
        }
    }
} 