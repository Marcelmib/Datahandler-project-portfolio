using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Datahandler
{
    class DataService
    {
        public string connectionString { get; set; }

        public NpgsqlConnection conn { get; set; }
        public DataService()
        {
            connectionString = "host=localhost;db=postgres;uid=postgres;pwd=jqe69wxn";
            conn = new NpgsqlConnection(connectionString);
            // conn.Open();
        }

        private NpgsqlCommand OpenSqlConnection()
        {
            var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.Connection.Open();
            return cmd;
        }

        public List<Title_basics> searchTitles(string searchString)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"Select * from string_search('{searchString}');";
            var reader = cmd.ExecuteReader();
            var Titles = new List<Title_basics>();
            while (reader.Read())
            {
                Title_basics newTitle = new Title_basics(reader.GetString(0), reader.GetString(1));
                Titles.Add(newTitle);
            }
            cmd.Connection.Close();
            return Titles;
        }

        public Title_basics GetTitle(string id)
        {

            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"Select * from title_basics natural join title_ratings natural join omdb_data where tconst = '{id}';";
            var reader = cmd.ExecuteReader();

            reader.Read();
            Title_basics newTitle = new Title_basics();

            newTitle.Tconst = reader.GetString(0);
            newTitle.TitleType = reader.GetString(1);
            newTitle.PrimaryTitle = reader.GetString(2);
            newTitle.OriginalTitle = reader.GetString(3);
            newTitle.IsAdult = reader.GetBoolean(4);
            newTitle.StartYear = reader.GetString(5);
            newTitle.EndYear = reader.GetString(6);
            newTitle.RuntimeMinutes = reader.GetInt32(7);
            newTitle.Genre = reader.GetString(8);
            newTitle.Rating = reader.GetDouble(9);
            newTitle.NumVotes = reader.GetInt32(10);
            newTitle.Poster = reader.GetString(11);
            newTitle.Plot = reader.GetString(12);
            cmd.Connection.Close();

            newTitle.Writers = GetWriters(newTitle.Tconst);
            newTitle.Directors = GetDirectors(newTitle.Tconst);
            newTitle.Crew = GetCrew(newTitle.Tconst);
            newTitle.Title_Akas = getTitleAkas(newTitle.Tconst);
            return newTitle;
        }

        public List<String> GetDirectors(string id)
        {
            var Directors = new List<String>();
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"Select nconst from title_directors where tconst = '{id}'";
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Directors.Add(reader.GetString(0));
            }
            cmd.Connection.Close();
            return Directors;
        }

        public List<String> GetWriters(string id)
        {
            var Writers = new List<String>();
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"Select nconst from title_writers where tconst = '{id}'";
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Writers.Add(reader.GetString(0));
            }
            cmd.Connection.Close();
            return Writers;
        }

        public List<Title_Principals> GetCrew(string id)
        {
            var Crew = new List<Title_Principals>();

            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"Select * from title_principals where tconst = '{id}' order by ordering";
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var person = new Title_Principals();
                person.Tconst = reader.GetString(0);
                person.Ordering = reader.GetInt32(1);
                person.Nconst = reader.GetString(2);
                person.Category = reader.GetString(3);
                person.Job = reader.GetString(4);

                //Sorting character names into a list, since some actors have more roles. 
                var temp = reader.GetString(5);
                if (temp != "")
                {
                    temp = temp.Remove(0, 1);
                    temp = temp.Remove(temp.Length - 1, 1);
                    person.Characters = temp.Split(',').ToList();
                    for (int x = 0; x < person.Characters.Count; x++)
                    {
                        person.Characters[x] = person.Characters[x].Remove(0, 1);
                        person.Characters[x] = person.Characters[x].Remove(person.Characters[x].Length - 1, 1);
                    }
                }
                Crew.Add(person);
            }
            cmd.Connection.Close();
            foreach (Title_Principals x in Crew)
            {
                x.Name_basics = GetNameBasics(x.Nconst);
            }
            return Crew;
        }


        public Name_Basics GetNameBasics(string id)
        {
            var person = new Name_Basics();
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"Select * from name_basics natural left join name_ratings where nconst = '{id}'";
            var reader = cmd.ExecuteReader();
            reader.Read();
            person.Nconst = reader.GetString(0);
            person.PrimaryName = reader.GetString(1);
            person.BirthYear = reader.GetString(2);
            person.DeathYear = reader.GetString(3);
            person.PrimaryProfession = reader.GetString(4);
            //checking if column is null, if not get float or int.... needs this or it crashes. 
            if (!reader.IsDBNull(5)) person.Averagerating = reader.GetFloat(5);
            if (!reader.IsDBNull(6)) person.NumVotes = reader.GetInt32(6);

            cmd.Connection.Close();
            return person;
        }

        public List<Title_Akas> getTitleAkas(string id)
        {
            var titleakas = new List<Title_Akas>();

            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"Select * from title_akas where titleid = '{id}'";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var title = new Title_Akas();
                title.Tconst = reader.GetString(0);
                title.Ordering = reader.GetInt32(1);
                title.Title = reader.GetString(2);
                title.Region = reader.GetString(3);
                title.Language = reader.GetString(4);
                title.Types = reader.GetString(5);
                title.Attributes = reader.GetString(6);
                title.IsOriginalTitle = reader.GetBoolean(7);
                titleakas.Add(title);
            }


            cmd.Connection.Close();
            return titleakas;
        }

        public List<Title_basics> GetSimilarTitles(String exactTitle)
        {
            var SimilarTitleList = new List<Title_basics>();
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"Select * from similar_movies('{exactTitle}') natural join title_ratings where id = tconst";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Title_basics newTitle = new Title_basics(reader.GetString(0), reader.GetString(1));
                newTitle.Rating = reader.GetDouble(5);
                newTitle.NumVotes = reader.GetInt32(6);
                SimilarTitleList.Add(newTitle);
            }


            cmd.Connection.Close();

            return SimilarTitleList;

        }

        public bool AddUser(string username, string name, string email, string passworrd)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"select add_user('{username}', '{name}', '{email}', '{passworrd}');";
            var reader = cmd.ExecuteReader();
            return checkBoolFromReaderandclosereader(cmd, reader);

        }
        public bool DeleteUser(string username, string email)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $"select delete_user('{username}', '{email}');";
            var reader = cmd.ExecuteReader();
            return checkBoolFromReaderandclosereader(cmd, reader);
        }

        public List<User> SearchUsers(string search, bool showemail = false, bool showname = false)
        {
            var FoundUsers = new List<User>();
            NpgsqlCommand cmd = OpenSqlConnection();
            var email = "";
            var name = "";

            if (showemail) email = ", email";
            if (showname) name = ", name";

            cmd.CommandText = $"select username {email} {name} from users where username like '%{search}%';";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var user = new User();
                user.Username = reader.GetString(0);
                if (showemail)
                {
                    user.Email = reader.GetString(1);
                    if (showname && showemail) user.Name = reader.GetString(2);
                }
                else if (showname && !showemail) user.Name = reader.GetString(1);

                FoundUsers.Add(user);
            }
            cmd.Connection.Close();
            return FoundUsers;
        }

        public List<User> GetAllUsers(bool showemail = false, bool showname = false)
        {
            var FoundUsers = new List<User>();
            NpgsqlCommand cmd = OpenSqlConnection();
            var email = "";
            var name = "";

            if (showemail) email = ", email";
            if (showname) name = ", name";

            cmd.CommandText = $"select username {email} {name} from users;";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var user = new User();
                user.Username = reader.GetString(0);
                if (showemail)
                {
                    user.Email = reader.GetString(1);
                    if (showname && showemail) user.Name = reader.GetString(2);
                }
                else if (showname && !showemail) user.Name = reader.GetString(1);

                FoundUsers.Add(user);
            }
            cmd.Connection.Close();
            return FoundUsers;
        }

        public List<Title_basics> GetUserFavorites(string username)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            var userfavorites = new List<Title_basics>();
            var tempList = new List<String>();
            cmd.CommandText = $"select tconst from favorites where username = '{username}'";
            var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                tempList.Add(reader.GetString(0));
            }
            cmd.Connection.Close();
            foreach (string xc in tempList) userfavorites.Add(GetTitle(xc));
            return userfavorites;
        }
        public bool RatingFunction(string username, string id, double rating)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $" select Rating_function_titles('{username}', '{id}','{rating}');";
            var reader = cmd.ExecuteReader();
            return checkBoolFromReaderandclosereader(cmd, reader);
        }
        public bool UndoUserRating(string username, string id)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $" select remove_user_rating('{username}', '{id}');";
            var reader = cmd.ExecuteReader();
            return checkBoolFromReaderandclosereader(cmd, reader);
        }
        public bool RatingFunctionActors(string username, string id, double rating)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $" select Rating_function_names('{username}', '{id}','{rating}');";
            var reader = cmd.ExecuteReader();
            return checkBoolFromReaderandclosereader(cmd, reader);
        }
        public bool UndoUserRatingActors(string username, string id)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $" select remove_user_rating_names('{username}', '{id}');";
            var reader = cmd.ExecuteReader();
            return checkBoolFromReaderandclosereader(cmd, reader);
        }
        public bool UserAddToFavorites(string username, string id)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $" select add_to_fav('{username}', '{id}');";
            var reader = cmd.ExecuteReader();
            return checkBoolFromReaderandclosereader(cmd, reader);
        }
        public bool UserRemoveFromFavorites(string username, string id)
        {
            NpgsqlCommand cmd = OpenSqlConnection();
            cmd.CommandText = $" select remove_from_fav_movie('{username}', '{id}');";
            var reader = cmd.ExecuteReader();
            return checkBoolFromReaderandclosereader(cmd, reader);
        }


        private static bool checkBoolFromReaderandclosereader(NpgsqlCommand cmd, NpgsqlDataReader reader)
        {
            reader.Read();
            if (reader.GetBoolean(0))
            {
                cmd.Connection.Close();
                return true;
            }
            else
            {
                cmd.Connection.Close();
                return false;
            }
        }
    }
}
