using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace RunningDinner
{
    class Program
    {
        public static Team[] allTeams;
        public static Dictionary<CourseType, string> mailTemplates = new Dictionary<CourseType, string>();

        public static List<Course> allCourses = new List<Course>();
        public static List<Course> appetizerCourses = new List<Course>();
        public static List<Course> mainCourses = new List<Course>();
        public static List<Course> dessertCourses = new List<Course>();

        public static readonly string TeamAppetizerName = "[TEAM_VORSPEISE_NAME]";
        public static readonly string TeamAppetizerNumber = "[TEAM_VORSPEISE_NUMMER]";
        public static readonly string TeamMainName = "[TEAM_MAIN_NAME]";
        public static readonly string TeamMainNumber = "[TEAM_MAIN_NUMMER]";
        public static readonly string TeamDessertName = "[TEAM_DESSERT_NAME]";
        public static readonly string TeamDessertNumber = "[TEAM_DESSERT_NUMMER]";
        public static readonly string AdditionalInformation = "[ZUSATZINFO]";

        static void Main(string[] args)
        {
            bool shuffle = true;

            foreach (var s in args)
            {
                if (s == "--noshuffle")
                {
                    shuffle = false;
                }
            }

            if (!Init())
            {
                Console.WriteLine("Not completed...");
                return;
            }

            if (allTeams.Length % 3 != 0)
            {
                Console.WriteLine("Teams nicht durch 3 Teilbar...");
                return;
            }

            var shuffledTeams = new List<Team>(allTeams);
            if (shuffle)
            {
                shuffledTeams.Shuffle();
            }

            var firstCourseCooks = new List<Team>();
            var secondCourseCooks = new List<Team>();
            var thirdCourseCooks = new List<Team>();

            var n = shuffledTeams.Count / 3;

            var i = 0;

            while (i < n)
            {
                firstCourseCooks.Add(shuffledTeams[i]);
                ++i;
            }

            while (i < n * 2)
            {
                secondCourseCooks.Add(shuffledTeams[i]);
                ++i;
            }

            while (i < n * 3)
            {
                thirdCourseCooks.Add(shuffledTeams[i]);
                ++i;
            }

            // Appetizer Courses...
            for (i = 0; i < n; i++)
            {
                var course = new Course
                {
                    Type = CourseType.Appetiser,
                    Cook = firstCourseCooks[i]
                };
                course.Guests.Add(secondCourseCooks[(i + 1) % secondCourseCooks.Count]);
                course.Guests.Add(thirdCourseCooks[(i + 2) % thirdCourseCooks.Count]);
                appetizerCourses.Add(course);
            }

            // Main Courses...
            for (i = 0; i < n; i++)
            {
                var course = new Course
                {
                    Type = CourseType.MainCourse,
                    Cook = secondCourseCooks[i]
                };
                course.Guests.Add(firstCourseCooks[(i + 1) % firstCourseCooks.Count]);
                course.Guests.Add(thirdCourseCooks[(i + 2) % thirdCourseCooks.Count]);
                mainCourses.Add(course);
            }

            // Dessert Courses...
            for (i = 0; i < n; i++)
            {
                var course = new Course
                {
                    Type = CourseType.Dessert,
                    Cook = thirdCourseCooks[i]
                };
                course.Guests.Add(secondCourseCooks[(i + 1) % secondCourseCooks.Count]);
                course.Guests.Add(firstCourseCooks[(i + 2) % firstCourseCooks.Count]);
                dessertCourses.Add(course);
            }

            allCourses.AddRange(appetizerCourses);
            allCourses.AddRange(mainCourses);
            allCourses.AddRange(dessertCourses);

            Directory.CreateDirectory("Plan");

            CreatePlan("Vorspeise", "Plan\\VorspeisePlan.txt", appetizerCourses);
            CreatePlan("Hauptspeise", "Plan\\HauptspeisePlan.txt", mainCourses);
            CreatePlan("Dessert", "Plan\\DessertPlan.txt", dessertCourses);


            Directory.CreateDirectory("Mail");

            foreach (var team in allTeams)
            {
                CreateMail(team);
            }

            Console.WriteLine("Done...");
        }

        static bool Init()
        {
            var jsonTeams = File.ReadAllText("teams.json");
            if (File.Exists("Templates\\AppetizerMailTemplate.txt"))
            {
                mailTemplates.Add(CourseType.Appetiser, File.ReadAllText("Templates\\AppetizerMailTemplate.txt"));
            }
            else
            {
                Console.WriteLine("Missing template for Appetizer... please fix..");
                Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(),
                    "Templates\\AppetizerMailTemplate.txt"));
                return false;
            }

            if (File.Exists("Templates\\MainCourseMailTemplate.txt"))
            {
                mailTemplates.Add(CourseType.MainCourse, File.ReadAllText("Templates\\MainCourseMailTemplate.txt"));
            }
            else
            {
                Console.WriteLine("Missing template for Main Course... please fix..");
                Console.WriteLine(
                    Path.Combine(Directory.GetCurrentDirectory(), "Templates\\MainCourseMailTemplate.txt"));
                return false;
            }

            if (File.Exists("Templates\\DessertMailTemplate.txt"))
            {
                mailTemplates.Add(CourseType.Dessert, File.ReadAllText("Templates\\DessertMailTemplate.txt"));
            }
            else
            {
                Console.WriteLine("Missing template for Dessert... please fix..");
                Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), "Templates\\DessertMailTemplate.txt"));
                return false;
            }

            allTeams = JsonConvert.DeserializeObject<Team[]>(jsonTeams);
            return true;
        }

        static void CreatePlan(string name, string filename, List<Course> courses)
        {
            var builder = new StringBuilder();
            builder.Append($"{name}:\n\n");
            foreach (var course in courses)
            {
                builder.Append($"Köche: {course.Cook.Name}\n");
                builder.Append($"Zimmer: {course.Cook.RoomNumber}\n");
                builder.Append($"Gäste:\n");
                foreach (var guest in course.Guests)
                {
                    builder.Append($"\t{guest.Name}\n");
                }

                builder.Append("\n");
            }

            File.WriteAllText(filename, builder.ToString());
        }

        static void CreateMail(Team team)
        {
            var type = (from course in allCourses where course.Cook.Equals(team) select course.Type)
                .FirstOrDefault();

            var builder = new StringBuilder();
            builder.Append(team.ToString());
            builder.Append("\n\n");

            builder.Append("\n---------------------------------------------\n");
            builder.Append(mailTemplates[type]);
            builder.Append("\n---------------------------------------------\n");

            Team appetizer;
            Team main;
            Team dessert;

            var additionalInfo = "";

            switch (type)
            {
                case CourseType.Appetiser:
                    appetizer = team;
                    main = FindCourseWithGuest(CourseType.MainCourse, team).Cook;
                    dessert = FindCourseWithGuest(CourseType.Dessert, team).Cook;
                    additionalInfo += main.AdditionalFoodInfo + ", " + dessert.AdditionalFoodInfo;
                    break;
                case CourseType.MainCourse:
                    appetizer = FindCourseWithGuest(CourseType.Appetiser, team).Cook;
                    main = team;
                    dessert = FindCourseWithGuest(CourseType.Dessert, team).Cook;
                    additionalInfo += appetizer.AdditionalFoodInfo + ", " + dessert.AdditionalFoodInfo;
                    break;
                case CourseType.Dessert:
                    appetizer = FindCourseWithGuest(CourseType.Appetiser, team).Cook;
                    main = FindCourseWithGuest(CourseType.MainCourse, team).Cook;
                    dessert = team;
                    additionalInfo += appetizer.AdditionalFoodInfo + ", " + main.AdditionalFoodInfo;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            builder.Replace(TeamAppetizerName, appetizer.Name);
            builder.Replace(TeamAppetizerNumber, appetizer.RoomNumber);
            builder.Replace(TeamMainName, main.Name);
            builder.Replace(TeamMainNumber, main.RoomNumber);
            builder.Replace(TeamDessertName, dessert.Name);
            builder.Replace(TeamDessertNumber, dessert.RoomNumber);

            builder.Replace(AdditionalInformation, additionalInfo);

            var fileNameBuilder = new StringBuilder();
            fileNameBuilder.Append("Mail___");
            fileNameBuilder.Append(team.Email);
            fileNameBuilder.Append(".txt");
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileNameBuilder.Replace(c, '_');
            }

            File.WriteAllText("Mail\\" + fileNameBuilder.ToString(), builder.ToString());
        }

        static Course FindCourseWithGuest(CourseType type, Team team)
        {
            return allCourses
                .Where(course => course.Type == type)
                .FirstOrDefault(course => Enumerable.Any<Team>(course.Guests, guest => guest == team));
        }
    }
}