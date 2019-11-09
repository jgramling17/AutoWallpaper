using RedditSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Configuration;
using System.Net.Http;

namespace ConsoleApp1
{
    class AutoWallpaper
    {
        static void Main(string[] args)
        {
            string horizontalSub = ConfigurationManager.AppSettings["horizontalSub"];
            string verticalSub = ConfigurationManager.AppSettings["verticalSub"];
            string baseDir = ConfigurationManager.AppSettings["baseDir"];
            int amountOfPics = Convert.ToInt32(ConfigurationManager.AppSettings["amountOfPics"]);
            bool verticalDownloads = Convert.ToBoolean(ConfigurationManager.AppSettings["verticalDownloads"]);
            string horizontalDir = baseDir + @"Horizontal\";
            string verticalDir = baseDir + @"Veritcal\";
            var horizontalTasks = new List<Task>();
            var verticalTasks = new List<Task>();

            DeleteFilesInDir(new[] { horizontalDir, verticalDir});

            DownloadImages(horizontalSub, horizontalDir, amountOfPics);
            if (verticalDownloads)
                DownloadImages(verticalSub, verticalDir, amountOfPics);

            Console.WriteLine("Finished");
        }

        private static void DeleteFilesInDir(string[] SaveDir)
        {

            foreach (var dir in SaveDir)
            {
                DirectoryInfo di = new DirectoryInfo(dir);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
        }

        private static void DownloadImages(string Subreddit, string path, int amount)
        {
            Reddit reddit = new Reddit();
            List<Task> subTaskList = new List<Task>();
            var subreddit = reddit.GetSubreddit(Subreddit);
            var posts = subreddit.GetTop(RedditSharp.Things.FromTime.Week).Take(amount);
            Parallel.ForEach(posts, post =>
            {
                subTaskList.Add(DownloadAndSaveTask(post, path));
            });
            Task.WaitAll(subTaskList.ToArray());
            
        }

        private static Task DownloadAndSaveTask(RedditSharp.Things.Post post, string path)
        {
            return Task.Run(() =>
            {
                string postURL = !string.IsNullOrEmpty(Path.GetExtension(post.Url.OriginalString).ToString()) ? Convert.ToString(post.Url) :  Convert.ToString(post.Url + ".png");
                using (WebClient client = new WebClient())
                {
                    var response = client.DownloadData(postURL);
                    using (var stream = new MemoryStream(response))
                    using (Image image = Image.FromStream(stream))
                    using (Bitmap bitmap = new Bitmap(image))
                    {
                        if (bitmap != null)
                        {
                            Guid id = Guid.NewGuid();
                            bitmap.Save(path + id.ToString() + ".png", ImageFormat.Png);
                        }
                    }

                }
            });
        }
    }
}
