using System;
using System.Linq;
using System.Windows;
using MusicLibrary.Models;

namespace MusicLibrary
{
    public partial class AddItemWindow : Window
    {
        public AddItemWindow()
        {
            InitializeComponent();
        }

        private async void AddItem_Click(object sender, RoutedEventArgs e)
        {

            var artistName = ArtistTextBox.Text.Trim();
            var albumTitle = AlbumTextBox.Text.Trim();
            var trackName = TrackTextBox.Text.Trim();
            var lengthText = LengthTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(artistName) ||
                string.IsNullOrWhiteSpace(albumTitle) ||
                string.IsNullOrWhiteSpace(trackName))
            {
                MessageBox.Show("Artist, Album and Track name are required.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int milliseconds = 0;
            if (!string.IsNullOrEmpty(lengthText))
            {
                if (TimeSpan.TryParseExact(lengthText, @"m\:ss", null, out var ts) ||
                    TimeSpan.TryParse(lengthText, out ts))
                {
                    milliseconds = (int)ts.TotalMilliseconds;
                }
            }

            try
            {
                using var db = new MusicContext();

                var nextArtistId = (db.Artists.Any() ? db.Artists.Max(a => a.ArtistId) : 0) + 1;
                var nextAlbumId = (db.Albums.Any() ? db.Albums.Max(a => a.AlbumId) : 0) + 1;
                var nextTrackId = (db.Tracks.Any() ? db.Tracks.Max(t => t.TrackId) : 0) + 1;

                var artist = db.Artists.FirstOrDefault(a => a.Name == artistName);
                if (artist == null)
                {
                    artist = new Artist
                    {
                        ArtistId = nextArtistId,
                        Name = artistName
                    };
                    db.Artists.Add(artist);
                }

                var album = db.Albums.FirstOrDefault(a => a.Title == albumTitle && a.ArtistId == artist.ArtistId);
                if (album == null)
                {
                    album = new Album
                    {
                        AlbumId = nextAlbumId,
                        Title = albumTitle,
                        ArtistId = artist.ArtistId
                    };
                    db.Albums.Add(album);
                }

                var track = new Track
                {
                    TrackId = nextTrackId,
                    Name = trackName,
                    AlbumId = album.AlbumId,
                    MediaTypeId = 1,
                    Milliseconds = milliseconds,
                    UnitPrice = 0.0
                };

                db.Tracks.Add(track);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving track: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Owner is MainWindow main)
            {
                main.ReloadArtists();
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
