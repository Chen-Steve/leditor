using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LightNovelEditor
{
    public class ChapterUploader
    {
        private readonly HttpClient _client;

        public ChapterUploader()
        {
            _client = new HttpClient();
        }

        public class ChapterUploadRequest
        {
            [JsonProperty("chapterNumber")]
            public int ChapterNumber { get; set; }

            [JsonProperty("title")]
            public string? Title { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; } = "";

            [JsonProperty("publishAt")]
            public string? PublishAt { get; set; }

            [JsonProperty("ageRating")]
            public string AgeRating { get; set; } = "EVERYONE";

            [JsonProperty("authorThoughts")]
            public string? AuthorThoughts { get; set; }

            [JsonProperty("volumeId")]
            public string? VolumeId { get; set; }
        }

        public async Task<bool> UploadChapterAsync(string novelId, ChapterUploadRequest chapter)
        {
            try
            {
                if (string.IsNullOrEmpty(SupabaseConfig.AccessToken))
                {
                    throw new InvalidOperationException("Not authenticated. Please log in first.");
                }

                var endpoint = $"/api/novels/{novelId}/chapters";
                var json = JsonConvert.SerializeObject(chapter);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SupabaseConfig.AccessToken);

                var response = await _client.PostAsync(SupabaseConfig.Url + endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Upload failed: {response.StatusCode} - {errorContent}");
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error uploading chapter: {ex.Message}",
                    "Upload Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
    }
} 