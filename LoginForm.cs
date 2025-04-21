using System;
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
        private TextBox emailInput = new();
        private TextBox passwordInput = new();
        private Button loginButton = new();
        private Button cancelButton = new();
        private Label statusLabel = new();
        private readonly HttpClient httpClient;

        public LoginForm()
        {
            httpClient = new HttpClient();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Login to Website";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var emailLabel = new Label
            {
                Text = "Email:",
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(emailLabel);

            emailInput = new TextBox
            {
                Location = new Point(20, 40),
                Width = 340
            };
            this.Controls.Add(emailInput);

            var passwordLabel = new Label
            {
                Text = "Password:",
                Location = new Point(20, 70),
                AutoSize = true
            };
            this.Controls.Add(passwordLabel);

            passwordInput = new TextBox
            {
                Location = new Point(20, 90),
                Width = 340,
                PasswordChar = '•'
            };
            this.Controls.Add(passwordInput);

            statusLabel = new Label
            {
                Location = new Point(20, 120),
                Size = new Size(340, 40),
                ForeColor = Color.Red
            };
            this.Controls.Add(statusLabel);

            loginButton = new Button
            {
                Text = "Login",
                Location = new Point(180, 170),
                Width = 80
            };
            loginButton.Click += LoginButton_Click;
            this.Controls.Add(loginButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(280, 170),
                Width = 80
            };
            this.Controls.Add(cancelButton);

            this.AcceptButton = loginButton;
            this.CancelButton = cancelButton;
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
                loginButton.Enabled = false;
                statusLabel.Text = "Logging in...";
                statusLabel.ForeColor = Color.Black;

                var request = new LoginRequest
                {
                    Email = emailInput.Text,
                    Password = passwordInput.Text
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json"
                );

                httpClient.DefaultRequestHeaders.Add("apikey", SupabaseConfig.Key);

                var response = await httpClient.PostAsync(
                    $"{SupabaseConfig.Url}/auth/v1/token?grant_type=password",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<LoginResponse>(
                        await response.Content.ReadAsStringAsync()
                    );

                    if (result?.AccessToken != null)
                    {
                        SupabaseConfig.AccessToken = result.AccessToken;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        throw new Exception("Login failed: No access token received");
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
                statusLabel.Text = $"Login failed: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
                loginButton.Enabled = true;
            }
        }
    }
} 