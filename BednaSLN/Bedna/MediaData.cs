using System;
using System.Windows.Controls;

namespace Bedna
{
    /// <summary>
    /// C:\Users\RomanNice\source\repos\Bedna\Bedna\bin\Debug\net5.0-windows\zvuky
    /// </summary>
    public class MediaData
    {
        public MediaElement Player { get { return _Player; }
            set 
            {
                value.Volume = 0.38;
                _Player = value;
            } 
        }
        private MediaElement _Player;
        
        public Uri[] MediaSources { get; set; }

        public MediaData(string[] mediaSources)
        {
            MediaSources = new Uri[mediaSources.Length];
            for (int i = 0; i < mediaSources.Length;i++)
            {
                MediaSources[i] = new Uri(mediaSources[i], UriKind.Relative);
            }
        }

        public enum MediaType
        {
            CoinIn = 0,
            Pull = 1,
            Fall = 2
        }

        public void PlayFile( MediaType t)
        {
            Player.Source = MediaSources[(int)t];
            Player.Position = new TimeSpan(0, 0, 0, 0, 0);
        }
    }
}
