using System;
using System.IO;
using System.Collections.Generic;

namespace LightNovelEditor
{
    public class ChapterManager
    {
        private readonly string chaptersDirectory;
        private readonly Dictionary<int, string> chapterContents = new();

        public ChapterManager()
        {
            // Create a chapters directory in the application folder
            chaptersDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Chapters");
            Directory.CreateDirectory(chaptersDirectory);
        }

        public void SaveChapterContent(int chapterNumber, string title, string content)
        {
            // Save in memory
            chapterContents[chapterNumber] = content;

            // Save to file
            try
            {
                string fileName = GetChapterFileName(chapterNumber, title);
                string filePath = Path.Combine(chaptersDirectory, fileName);
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving chapter file: {ex.Message}");
                // Continue even if file save fails - we still have content in memory
            }
        }

        public string LoadChapterContent(int chapterNumber, string title)
        {
            // Try to get from memory first
            if (chapterContents.TryGetValue(chapterNumber, out string? content))
            {
                return content;
            }

            // If not in memory, try to load from file
            try
            {
                string fileName = GetChapterFileName(chapterNumber, title);
                string filePath = Path.Combine(chaptersDirectory, fileName);
                if (File.Exists(filePath))
                {
                    content = File.ReadAllText(filePath);
                    chapterContents[chapterNumber] = content; // Cache in memory
                    return content;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading chapter file: {ex.Message}");
            }

            return ""; // Return empty string for new chapters
        }

        private string GetChapterFileName(int chapterNumber, string title)
        {
            // Create a safe filename from the chapter number and title
            string safeTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
            return $"Chapter_{chapterNumber:D3}_{safeTitle}.txt";
        }
    }
} 