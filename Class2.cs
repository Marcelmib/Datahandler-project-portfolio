using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using Npgsql;

namespace Datahandler
{
    class User
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

    }
    class Name_Basics
    {
        public string Nconst { get; set; }
        public string PrimaryName { get; set; }
        public string BirthYear { get; set; }
        public string DeathYear { get; set; }
        public string PrimaryProfession { get; set; }

        //public List<Title_Principals> knownfor { get; set; }

        //inhereted attributes 

        public double Averagerating { get; set; }
        public int NumVotes { get; set; }
    }
    class Title_Principals
    {
        public Name_Basics Name_basics { get; set; }
        public string Tconst { get; set; }
        public int Ordering { get; set; }
        public string Nconst { get; set; }
        public string Category { get; set; }
        public string Job { get; set; }
        public List<String> Characters { get; set; }

        public Title_Principals()
        {
            Characters = new List<String>();
        }

    }

    class Title_Akas
    {
        public string Tconst { get; set; }
        public int Ordering { get; set; }
        public string Title { get; set; }
        public string Region { get; set; }
        public string Language { get; set; }
        public string Types { get; set; }
        public string Attributes { get; set; }
        public Boolean IsOriginalTitle { get; set; }
    }


    class Title_basics
    {

        //Title Basics Attributes
        public string Tconst { get; set; }
        public string TitleType { get; set; }
        public string PrimaryTitle { get; set; }
        public string OriginalTitle { get; set; }
        public Boolean IsAdult { get; set; }
        public string StartYear { get; set; }
        public string EndYear { get; set; }
        public int RuntimeMinutes { get; set; }
        public string Genre { get; set; }

        //Inherented structures 
        public double Rating { get; set; }

        public int NumVotes { get; set; }

        public List<String> Directors { get; set; }
        public List<String> Writers { get; set; }
        public List<Title_Principals> Crew { get; set; }
        public List<Title_Akas> Title_Akas { get; set; }

        public string Poster { get; set; }
        public string Plot { get; set; }
        public Title_basics()
        {
            Tconst = null;
            PrimaryTitle = null;
        }

            public Title_basics(string id, string title)
        {
            Tconst = id;
            PrimaryTitle = title; 
        }
        


    }
    



}
