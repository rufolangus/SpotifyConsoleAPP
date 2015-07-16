/*Spotify Console Controller
 * Description:
 * Query and play for your favorite artists from the command line.
 * Author: Rafael A. Valle Gonzalez:. AKA: (Rufolangus/Triton03)
 * Date: 7/14/2015
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SpotifyConsoleApp
{
    class SpotifyConsole
    {

        public static List<Artist> artists;
        private static History history;
       [DllImport ("User32.dll")]
static extern int SetForegroundWindow(IntPtr point);
        static void Main(string[] args)
        {
            CheckIfSpotifyAppRunning();
            ShowGreeting();
            if (args == null || args.Length == 0)
                AskArtist();
            else
            {
                string artist = "";
                foreach (var arg in args)
                    artist += arg + " ";
                SearchArtist(artist);
            }
        }

        public static void CheckIfSpotifyAppRunning() 
        {
           Process process =  System.Diagnostics.Process.GetProcessesByName("Spotify").FirstOrDefault();
           if (process != null) 
           {
               IntPtr ptr = process.MainWindowHandle;
               SetForegroundWindow(ptr);
               SendKeys.SendWait(" ");
               
           }

           
        }
        public static void ShowGreeting() 
        {
            history = new History();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Welcome to Spotify Console Command!");
            Console.ResetColor();
        }

        public static void AskArtist()
        {
            Console.WriteLine("Enter artist name to search spotify: ");
            string artist = Console.ReadLine();
            artist.Replace(' ', '+');
            SearchArtist(artist);
        }

        public static void SearchArtist(string artist)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            ClearArtist();
            Console.WriteLine("Searching for " + artist);
            Console.ResetColor();
            string url;
            url = "https://api.spotify.com/v1/search?q=" + artist + "&type=artist";
            var dyn = MakeRequest(url);
            HandleArtist(dyn);
        }

        public static void HandleArtist(dynamic obj)
        {
            foreach (var data in obj.artists.items)
            {
                var artist = new Artist();
                artist.name = data.name;
                artist.id = data.id;
                artist.url = data.external_urls.spotify;
                artists.Add(artist);
            }
            string next = obj.artists.next;
            if (!string.IsNullOrEmpty(next))
                HandleArtist(MakeRequest(next));
            else
                ShowResults();
        }

        public static dynamic MakeRequest(string url)
        {
            WebRequest request;
            request = WebRequest.Create(url);

            Stream objstream = request.GetResponse().GetResponseStream();
            StreamReader reader = new StreamReader(objstream);
            string line = "";
            string jsonObj = "";
            while (line != null)
            {
                line = reader.ReadLine();
                jsonObj += line;
            }
            dynamic dynObj = JsonConvert.DeserializeObject(jsonObj);

            return dynObj;
        }

        public static void ClearArtist()
        {
            artists = new List<Artist>();
        }

        public static void ShowResults()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Found " + artists.Count + " Results");
            int i = 1;
            foreach (var artist in artists)
            {
                Console.WriteLine("\nResult #" + i);
                Console.Write(artist.ToString());
                i++;
            }

            Console.ResetColor();
            Console.WriteLine("\nWould you like to play any of these artists?");
            string answer = Console.ReadLine();
            answer = answer.ToLower();

            if (answer.Equals("yes") || answer.Equals("y"))
            {
                Console.WriteLine("\nEnter artist number: ");
                string num = Console.ReadLine();
                int number;
                int.TryParse(num, out number);
                if (number != 0 || number <= artists.Count)
                {
                    var artist = artists[number - 1];
                    history.AddArtist(artist);
                    Console.WriteLine("\nWhat would you like to do?");
                    Console.WriteLine("0. Play Artist");
                    Console.WriteLine("1. Browse Albums.");
                    Console.WriteLine("2. Search Again.");
                    var ansr = Console.ReadLine();
                    int numb;
                    int.TryParse(ansr,out numb);
                    switch (numb) 
                    {
                        case 0:
                            System.Diagnostics.Process.Start(artists[number - 1].url);
                            break;
                        case 1:
                            string url = "https://api.spotify.com/v1/artists/" + artist.id + "/albums";
                            var dyn = MakeRequest(url);
                            GetArtistInfo(artists[number - 1], dyn);
                            break;
                            /*string uri = "https://api.spotify.com/v1/artists/" + artist.id + "/top-tracks";
                            var dyna = MakeRequest(uri);
                            GetTopSongInfo(artist,dyna);
                             */
                            break;
                        case 2:
                           AskArtist();
                            break;
                        case 3:
                            Console.WriteLine("Goodbye.");
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("\nWould you like to search again?");
                string response = Console.ReadLine();
                if (response.Equals("yes") || response.Equals("y"))
                    AskArtist();
                else
                    return;
            }

        }

        public static void GetTopSongInfo(Artist artist, dynamic dyn) 
        {
            if (artist.topSongs == null)
                artist.topSongs = new List<Song>();

            foreach (var song in dyn.tracks) 
            {
                var newSong = new Song();
                newSong.title = song.name;
                newSong.id = song.id;
                newSong.url = song.external_urls.spotify;
                artist.topSongs.Add(newSong);
            }

            ShowTopSongs(artist);
        }

        public static void ShowTopSongs(Artist artist) 
        {
            Console.ForegroundColor = ConsoleColor.Green;
            int i = 1;
            foreach (var song in artist.topSongs) 
            {
                Console.WriteLine("#" + i + song.title);
                i++;
            }

            Console.WriteLine("#0. Exit");
            Console.ResetColor();
            Console.WriteLine("What would you like to do?");
            string answer = Console.ReadLine();
            int num = 0;
            int.TryParse(answer, out num);
            if (num == 0)
                return;
            else
                System.Diagnostics.Process.Start(artist.topSongs[num - 1].url);
        }

        public static void GetArtistInfo(Artist artist, dynamic dyn)
        {
            if(artist.albums==null)
            artist.albums = new List<Album>();

            foreach (var data in dyn.items) 
            {
                var album = new Album();
                album.title = data.name;
                album.id = data.id;
                album.url = data.external_urls.spotify;
                artist.albums.Add(album);
            }

            string next = dyn.next;
            if (!string.IsNullOrEmpty(next))
                GetArtistInfo(artist, MakeRequest(next));
            else
                ShowAlbums(artist);
        }

        public static void ShowAlbums(Artist artist) 
        {
            int i = 1;
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var album in artist.albums) 
            {
                Console.WriteLine("#"+ i + " " + album.title);
                i++;
            }
            Console.ResetColor();
            Console.WriteLine("\nWhat album would you like to browse?");
            string albumNo = Console.ReadLine();
            int num = 0;
            int.TryParse(albumNo, out num);
            if (num > 0) 
            {
                var album = artist.albums[num - 1];
                    history.AddAlbum(album);
                string url = "https://api.spotify.com/v1/albums/" + album.id;
                var dyn = MakeRequest(url);
                GetAlbumSongs(album,dyn);
            }
        }

        public static void GetAlbumSongs(Album album, dynamic dyn) 
        {
            if (album.songs == null)
                album.songs = new List<Song>();

            foreach (var data in dyn.tracks.items)
            {
                var song = new Song();
                song.title = data.name;
                song.id = data.id;
                song.url = data.external_urls.spotify;
                album.songs.Add(song);
            }

            string next = dyn.next;
            if (!string.IsNullOrEmpty(next))
                GetAlbumSongs(album, MakeRequest(next));
            else
                ShowSongs(album);
        }

        public static void ShowSongs(Album album) 
        {
            int i = 1;
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var song in album.songs) 
            {
                Console.WriteLine("#" + i + " " + song.title);
                i++;
            }
            Console.WriteLine("#" + i + "Play Album.");
            Console.WriteLine("#0 Exit");
            Console.ResetColor();
            Console.WriteLine("What would you like to do?");
           string answer =  Console.ReadLine();
           int num = 0;
           int.TryParse(answer, out num);

           if (num == 0)
               return;
           else if (num == i)
           {
               System.Diagnostics.Process.Start(album.url);
               history.AddAlbum(album);
           }
           else
           {
               System.Diagnostics.Process.Start(album.songs[num - 1].url);
               history.AddSong(album.songs[num - 1]);
           }
            
        }
    }

    [Serializable]
    public class Artist
    {
        public string name;
        public string id;
        public string url;
        public List<Song> topSongs;
        public List<Album> albums;
       
        public string ToString()
        {
            return "Artist Name: " + name + "\n" + "ID: " + id + "\n" + "URL: " + url + "\n";
        }
    }

    [Serializable]
    public class Album
    {
        public string id;
        public string title;
        public string url;
        public  List<Song> songs;
    }

    [Serializable]
    public class Song
    {
        public string id;
        public string title;
        public string url;
    }
}
