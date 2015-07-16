using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace SpotifyConsoleApp
{
    class History
    {
        private List<Song> lastSongsPlayed;
        private List<Album> lastAlbumPlayed;
        private List<Artist> lastArtistPlayed;
        ~History() 
        {
            Save();
        }
        public History() 
        {
            StreamReader read;
            XmlSerializer serializer;
            XmlReader reader;
            if (File.Exists(@"./songs.xml"))
            {
                 read = new StreamReader(@"./songs.xml");
                 serializer = new XmlSerializer(typeof(List<Song>));
                 reader = XmlReader.Create(read);
                lastSongsPlayed = (List<Song>)serializer.Deserialize(reader);
                read.Close();
            }
            else
                lastSongsPlayed = new List<Song>();

            if (File.Exists(@"./albums.xml"))
            {
                read = new StreamReader(@"./albums.xml");
                serializer = new XmlSerializer(typeof(List<Album>));
                reader = XmlReader.Create(read);
                lastAlbumPlayed = (List<Album>)serializer.Deserialize(reader);
                read.Close();
            }
            else
                lastAlbumPlayed = new List<Album>();

            if (File.Exists(@"./artists.xml"))
            {
                read = new StreamReader(@"./artists.xml");
                serializer = new XmlSerializer(typeof(List<Artist>));
                reader = XmlReader.Create(read);
                lastArtistPlayed = (List<Artist>)serializer.Deserialize(reader);
                read.Close();
            }
            else
                lastArtistPlayed = new List<Artist>();
        }

        public void AddArtist(Artist artist) 
        {
            lastArtistPlayed.Add(artist);
        }

        public void AddSong(Song song) 
        {
            lastSongsPlayed.Add(song);
        }

        public void AddAlbum(Album album) 
        {
            lastAlbumPlayed.Add(album);
        }

        public void Save() 
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Song>));
            StreamWriter writer = new StreamWriter(@"./songs.xml");
            serializer.Serialize(writer, lastSongsPlayed);
            writer.Close();

            writer = new StreamWriter(@"./albums.xml");
            serializer = new XmlSerializer(typeof(List<Album>));
            serializer.Serialize(writer, lastAlbumPlayed);
            writer.Close();

            writer = new StreamWriter(@"./artists.xml");
            serializer = new XmlSerializer(typeof(List<Artist>));
            serializer.Serialize(writer, lastArtistPlayed);
            writer.Close();
        }


    }
}
