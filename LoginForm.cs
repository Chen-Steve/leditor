using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using MaterialSkin;
using MaterialSkin.Controls;

namespace LightNovelEditor
{
    public class LoginForm : MaterialForm
    {
        private readonly MaterialTextBox2 emailTextBox;
        private readonly MaterialTextBox2 passwordTextBox;
        private bool skippedLogin = false;
        private readonly HttpClient httpClient;
        private readonly MaterialSkinManager materialSkinManager;
        private readonly PictureBox logoBox;
        
        // Add these constants for form dragging
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        
        public static bool IsLoggedIn { get; private set; }
        public static string? CurrentUserAvatarUrl { get; private set; }
        public static string? CurrentUsername { get; private set; }
        public bool SkippedLogin => skippedLogin;

        public LoginForm()
        {
            httpClient = new HttpClient();
            
            // Initialize MaterialSkinManager
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            
            // Create a consistent blue color scheme - make sure title bar and title area match
            Color titleBarColor = Color.FromArgb(33, 150, 243); // Material Blue 500
            materialSkinManager.ColorScheme = new ColorScheme(
                titleBarColor,
                titleBarColor, // Using same color for DarkPrimary to ensure consistency
                titleBarColor, // Using same color for LightPrimary to ensure consistency
                Color.FromArgb(3, 169, 244),  // Material Light Blue 500 as accent
                TextShade.WHITE
            );
            
            // Ensure title bar is styled properly with the same color as the title
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            
            // Remove the title bar completely
            FormStyle = FormStyles.StatusAndActionBar_None;
            Padding = new Padding(3);
            
            // Form settings
            Text = ""; // Empty title since we won't show it anyway
            Size = new Size(600, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None; // No border
            MaximizeBox = false;
            MinimizeBox = false;
            Sizable = false;
            MinimumSize = new Size(500, 500);
            
            // No direct property to set title bar color, so we use the material skin manager
            
            // Add event handler for form closing to exit the application when close button is clicked
            this.FormClosed += LoginForm_FormClosed;
            
            // Create a panel for login content
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40)
            };
            contentPanel.MouseDown += LoginForm_MouseDown; // Make panel draggable too
            Controls.Add(contentPanel);
            
            // Add a custom close button in the top right corner
            var closeButton = new MaterialButton
            {
                Text = "×",
                Type = MaterialButton.MaterialButtonType.Text,
                Size = new Size(40, 40),
                Location = new Point(this.Width - 45, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                UseAccentColor = false,
                HighEmphasis = false,
                Depth = 0,
                AutoSize = false,
                Font = new Font("Segoe UI", 18F, FontStyle.Regular)
            };
            closeButton.Click += (s, e) => this.Close();
            Controls.Add(closeButton);
            closeButton.BringToFront(); // Ensure it stays on top
            
            // Make the form draggable since we removed the title bar
            this.MouseDown += LoginForm_MouseDown;
            
            // Logo
            logoBox = new PictureBox
            {
                Size = new Size(120, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point((contentPanel.Width - 120) / 2, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.FixedSingle
            };
            logoBox.MouseDown += LoginForm_MouseDown; // Make logo draggable too
            
            try
            {
                // Attempt to load the logo
                string logoPath = "lanry.jpg";
                if (System.IO.File.Exists(logoPath))
                {
                    logoBox.Image = Image.FromFile(logoPath);
                }
                else
                {
                    // Create a text-based logo if image not found
                    MaterialMessageBox.Show("Logo file 'lanry.png' not found.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        
                    /* Placeholder LE logo
                    var bmp = new Bitmap(100, 100);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.Clear(materialSkinManager.ColorScheme.PrimaryColor);
                        g.DrawString("LE", new Font("Arial", 40, FontStyle.Bold), 
                            Brushes.White, new PointF(18, 18));
                    }
                    logoBox.Image = bmp;
                    */
                }
            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show($"Error loading logo: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                /* Placeholder LE logo
                var bmp = new Bitmap(100, 100);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(materialSkinManager.ColorScheme.PrimaryColor);
                    g.DrawString("LE", new Font("Arial", 40, FontStyle.Bold), 
                        Brushes.White, new PointF(18, 18));
                }
                logoBox.Image = bmp;
                */
            }
            
            contentPanel.Controls.Add(logoBox);
            
            // Welcome heading
            var welcomeLabel = new MaterialLabel
            {
                Text = "Welcome to Lanry Editor",
                Location = new Point(0, logoBox.Bottom + 20),
                Size = new Size(contentPanel.Width, 40),
                Depth = 0,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            contentPanel.Controls.Add(welcomeLabel);
            
            // Form fields
            int fieldWidth = Math.Min(400, contentPanel.Width - 80);
            int fieldStartX = (contentPanel.Width - fieldWidth) / 2;
            int fieldStartY = welcomeLabel.Bottom + 30;
            
            // Email field
            var emailLabel = new MaterialLabel
            {
                Text = "Email:",
                Location = new Point(fieldStartX, fieldStartY),
                AutoSize = true,
                Depth = 0,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            contentPanel.Controls.Add(emailLabel);

            emailTextBox = new MaterialTextBox2
            {
                Location = new Point(fieldStartX, emailLabel.Bottom + 5),
                Size = new Size(fieldWidth, 50),
                Depth = 0,
                LeadingIcon = null,
                Hint = "Enter your email address",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            contentPanel.Controls.Add(emailTextBox);

            // Password field
            var passwordLabel = new MaterialLabel
            {
                Text = "Password:",
                Location = new Point(fieldStartX, emailTextBox.Bottom + 20),
                AutoSize = true,
                Depth = 0,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            contentPanel.Controls.Add(passwordLabel);

            passwordTextBox = new MaterialTextBox2
            {
                Location = new Point(fieldStartX, passwordLabel.Bottom + 5),
                Size = new Size(fieldWidth, 50),
                UseSystemPasswordChar = true,
                Depth = 0,
                LeadingIcon = null,
                Hint = "Enter your password",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            contentPanel.Controls.Add(passwordTextBox);

            // Login button
            var loginButton = new MaterialButton
            {
                Text = "LOGIN",
                Type = MaterialButton.MaterialButtonType.Contained,
                Location = new Point(fieldStartX, passwordTextBox.Bottom + 30),
                Size = new Size((fieldWidth / 2) - 10, 50),
                Depth = 0,
                Icon = null,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            loginButton.Click += LoginButton_Click;
            contentPanel.Controls.Add(loginButton);

            // Skip/Guest login button
            var skipButton = new MaterialButton
            {
                Text = "CONTINUE AS GUEST",
                Type = MaterialButton.MaterialButtonType.Outlined,
                Location = new Point(fieldStartX + (fieldWidth / 2) + 10, loginButton.Top),
                Size = new Size((fieldWidth / 2) - 10, 50),
                Depth = 0,
                Icon = null,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            skipButton.Click += SkipButton_Click;
            contentPanel.Controls.Add(skipButton);

            // Register link
            var registerLink = new LinkLabel
            {
                Text = "Don't have an account? Register here",
                Location = new Point(fieldStartX, loginButton.Bottom + 20),
                AutoSize = true,
                LinkColor = materialSkinManager.ColorScheme.PrimaryColor,
                LinkBehavior = LinkBehavior.AlwaysUnderline,
                Font = new Font("Segoe UI", 9F),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            registerLink.Links.Clear();
            registerLink.Links.Add(new LinkLabel.Link(23, 13));
            registerLink.MouseEnter += (s, e) => {
                Cursor = Cursors.Hand;
            };
            registerLink.MouseLeave += (s, e) => {
                Cursor = Cursors.Default;
            };
            registerLink.LinkClicked += (s, e) => RegisterLink_Click(s, e);
            contentPanel.Controls.Add(registerLink);

            // Set focus to email field
            emailTextBox.Focus();
            AcceptButton = loginButton;
            
            // Handle resize events to adjust layout
            this.Resize += LoginForm_Resize;
        }

        private void LoginForm_Resize(object? sender, EventArgs e)
        {
            try
            {
                if (logoBox != null && this.Controls.Count > 0 && this.Controls[0] is Panel contentPanel)
                {
                    logoBox.Location = new Point((contentPanel.Width - logoBox.Width) / 2, 40);
                    
                    // Update other controls if needed
                    foreach (Control control in contentPanel.Controls)
                    {
                        if (control is MaterialLabel label && label.Text == "Welcome to Lanry Editor")
                        {
                            label.Size = new Size(contentPanel.Width, 40);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently ignore any layout errors during resize
            }
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
                    MaterialMessageBox.Show("Please enter both email and password.", "Login Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cursor = Cursors.WaitCursor;

                // Create loading overlay
                var loadingOverlay = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(120, Color.White)
                };
                
                var loadingLabel = new Label
                {
                    Text = "Signing in...",
                    Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 120, 212),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                
                loadingOverlay.Controls.Add(loadingLabel);
                this.Controls.Add(loadingOverlay);
                loadingOverlay.BringToFront();
                Application.DoEvents();
                
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

                        if (profileResponse.IsSuccessStatusCode)
                        {
                            var profiles = JsonConvert.DeserializeObject<List<ProfileResponse>>(profileContent);

                            if (profiles?.Count > 0)
                            {
                                CurrentUserAvatarUrl = profiles[0].AvatarUrl;
                                CurrentUsername = profiles[0].Username;
                            }
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
                MaterialMessageBox.Show($"Login failed: {ex.Message}", "Login Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                
                // Remove loading overlay if present
                foreach (Control control in this.Controls)
                {
                    if (control is Panel panel && panel.BackColor == Color.FromArgb(120, Color.White))
                    {
                        this.Controls.Remove(panel);
                        panel.Dispose();
                        break;
                    }
                }
            }
        }

        private void SkipButton_Click(object? sender, EventArgs e)
        {
            skippedLogin = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void RegisterLink_Click(object? sender, EventArgs e)
        {
            // Open registration page in default browser
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://lanrys.com/register",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show($"Could not open registration page: {ex.Message}", "Error",
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
            public DateTime LastVisit { get; set; }

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
            public int Coins { get; set; }
        }

        private void LoginForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            // Only exit the application when the dialog result is not OK
            // This ensures we only exit when the X button was clicked, not when login succeeded
            if (this.DialogResult != DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private void LoginForm_MouseDown(object? sender, MouseEventArgs e)
        {
            // Make the form draggable using Windows API
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
} 