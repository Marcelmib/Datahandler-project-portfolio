using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Datahandler
{
    class Program
    {
        static void Main(string[] args)
        {
            //Testing string_search
            DataService dataservice = new DataService();
            var printTest = dataservice.searchTitles("Game of thrones");
            Console.WriteLine($"Found {printTest.Count} results!");
            Console.WriteLine("Title ID       Title");

            //Shows Id and primarytitle for a movie/series
            foreach (Title_basics x in printTest)
            {
                Console.WriteLine(x.Tconst + "  :  " + x.PrimaryTitle);
            }

            // Getting 1 title 
            Console.WriteLine("Getting 1 Title...");
            //this is a monty python short film...changed to friends

            var oneTitle = dataservice.GetTitle("tt0583459 ");

            //PrintSingleObj(oneTitle, false); // Print the whole object. A giant mess but atleast you won miss anything...

            PrintSingleObj(oneTitle);


            Console.WriteLine("Showalternative titles: ");
            PrintManyObj(oneTitle.Title_Akas);

            Console.WriteLine("Crew for the title:");
            PrintManyObj(oneTitle.Crew);

            var SimilarTitles = dataservice.GetSimilarTitles(oneTitle.PrimaryTitle);

            //Similar titles holds  a little less information so bc we only use the covers and name to show the user. .
            PrintManyObj(SimilarTitles, true);// Print a list with objects. A giant mess but atleast you won miss anything...
            

            //Creating a user. its in a if bc it returns true or false. from sql code. 
            if(dataservice.AddUser("userbob", "Bob Johnson", "bob@gmail.com", "1234password"))
            {
                Console.WriteLine("Created user: ");
                
            }
            if (dataservice.AddUser("userbob1", "Bob Johnson", "bob12@gmail.com", "1234password"))
            {
                Console.WriteLine("Created user: ");

            }

            //searching for a user. Using adm tool, getting both email and name
            var foundUsers = dataservice.SearchUsers("bob", true, true);

            Console.WriteLine($"found {foundUsers.Count} users!");
            PrintManyObj(foundUsers, true);
            
            //Deleting user. using if bc it returns true or false.
            if (dataservice.DeleteUser("userbob","bob@gmail.com"))
            {
                Console.WriteLine("deleted user...");

            }
            //getting all users in the database.
            var alluser = dataservice.GetAllUsers( true, true);
            PrintManyObj(alluser);

            //testing user rating for titles. rating a title. 
            dataservice.RatingFunction(dataservice.SearchUsers("bob")[0].Username, "tt11570056", 3);
            // removing user rating. 
            dataservice.UndoUserRating(dataservice.SearchUsers("bob")[0].Username, "tt11570056");
            //testing rating for actors...
            dataservice.RatingFunctionActors(dataservice.SearchUsers("bob")[0].Username, "nm0004517 ", 9);

            //getting the actor that was rated. 
            PrintSingleObj(dataservice.GetNameBasics("nm0004517"));
            
            // removing user rating. 
            dataservice.UndoUserRatingActors(dataservice.SearchUsers("bob")[0].Username, "nm0004517");

            //Adding to a users favorite: movies
            dataservice.UserAddToFavorites(dataservice.SearchUsers("bob")[0].Username, "tt11570056");

            //Get all of a users favorite movies
            var userfav = dataservice.GetUserFavorites(dataservice.SearchUsers("bob")[0].Username);
            PrintManyObj(userfav);
            //remove from users favorite movies
            dataservice.UserRemoveFromFavorites(dataservice.SearchUsers("bob")[0].Username, "tt11570056");
        }

        private static void PrintManyObj<T>(List<T> obj, bool shownull = false)
        {
            foreach (T xc in obj)
            {
                PrintSingleObj(xc, shownull);
            }
        }

        private static void PrintSingleObj<T>(T xc, Boolean showNull = false)
        {
            
                foreach (PropertyInfo propertyInfo in xc.GetType().GetProperties())
                {
                
                var val = propertyInfo.GetValue(xc);
                    if (val != null || showNull == true)
                    {
                    Console.WriteLine();
                    Console.Write(propertyInfo.Name + " : ");
                    var tempPrint = val.ToJson();
                    foreach (char n in tempPrint)
                    {
                        if (n == ',')
                        {
                            Console.WriteLine();
                        }
                        else Console.Write(n);
                    }
                }

                }
               
           Console.WriteLine();
           Console.WriteLine("!----------------------------------------------!");
        }

        private static void QuickTitleInfo(Title_basics oneTitle)
        {
            Console.WriteLine($"title id: {oneTitle.Tconst} - Primarytitle : {oneTitle.PrimaryTitle} - Rating : {oneTitle.Rating}");
            Console.WriteLine($"year : {oneTitle.StartYear}");
            Console.WriteLine($"Runtime in minutes: {oneTitle.RuntimeMinutes}");
        }

        private static void Showalternativetitles(Title_basics oneTitle)
        {
            Console.WriteLine($"Alternative titles: ");
            foreach (Title_Akas q in oneTitle.Title_Akas)
            {
                Console.WriteLine(" - " + q.Title + " - Region: " + q.Region);
            }
        }

        private static void ShowCrewForTitle(Title_basics oneTitle)
        {
            Console.WriteLine("Crew related to the title: ");
            foreach (Title_Principals x in oneTitle.Crew)
            {
                Console.WriteLine("");
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine(x.Name_basics.PrimaryName);
                Console.WriteLine("Primary Profession : " + x.Name_basics.PrimaryProfession);
                Console.WriteLine("Born : " + x.Name_basics.BirthYear);
                Console.WriteLine("Dead : " + x.Name_basics.DeathYear);
                Console.WriteLine("Worked as : " + x.Job);

                Console.Write("Stared as : ");
                foreach (string role in x.Characters) Console.Write(role + ", ");

            }
        }
    }
}
public static class Util
{
    public static T FromJson<T>(this string element)
    {
        return JsonSerializer.Deserialize<T>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
    public static string ToJson(this object data)
    {
        return JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }



}