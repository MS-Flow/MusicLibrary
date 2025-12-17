using Microsoft.EntityFrameworkCore;
using MusicLibrary.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MusicLibrary.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicLibrary;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    private MusicViewModel _vm = new MusicViewModel();

    public MainWindow()
    {
        InitializeComponent();

        DataContext = _vm;

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await _vm.LoadDataAsync();
        LoadArtists();

    }

    private void LoadArtists()
    {
        using var db = new MusicContext();

        var artists = db.Artists
            .Where(artist => artist.Albums.Count > 2)
            .Include(artist => artist.Albums)
            .ThenInclude(album => album.Tracks)
            .ToList();

        myTreeView.ItemsSource = new ObservableCollection<Artist>(artists);
    }

    private void TreeView_SelectedItemChanged(
    object sender,
    RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MusicViewModel vm &&
            e.NewValue is MusicLibrary.Models.Track track)
        {
            vm.SelectedLibraryTrack = track;
        }
    }
    private async void DataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight - 20)
        {
            if (DataContext is MusicViewModel vm)
            {
                await vm.LoadMoreTracksAsync();
            }
        }
    }
    private void RowHeader_DeleteClicked(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGridRowHeader header &&
            header.DataContext is MusicLibrary.Models.Track track &&
            DataContext is MusicViewModel vm)
        {
            vm.SelectedPlaylistTrack = track;

            if (vm.RemoveTrackFromPlaylistCommand.CanExecute(null))
            {
                vm.RemoveTrackFromPlaylistCommand.Execute(null);
            }
        }
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {

    }
}

// InverseBoolConverter.cs

public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : value;
    }
