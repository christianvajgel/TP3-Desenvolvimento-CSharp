using ClassLibrary_tp3_csharp;
using System;
using System.Threading;
using static ClassLibrary_tp3_csharp.Parsing;
using static ClassLibrary_tp3_csharp.Validations;
using static ClassLibrary_tp3_csharp.Person;
using static ClassLibrary_tp3_csharp.Repository;
using System.Linq;
using System.Runtime.InteropServices;

namespace tp3_csharp
{
    class Program
    {

        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);


        static void Main(string[] args)
        {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            uint mode;
            GetConsoleMode(handle, out mode);
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            SetConsoleMode(handle, mode);

            const string UNDERLINE = "\x1B[4m";
            const string RESET = "\x1B[0m";

            var id = 0;
            while (true)
            {
                ShowMenu();
                var operation = ReadNumber("menu");
                if (operation.Equals("3"))
                {
                    break;
                }
                else
                {
                    switch (operation)
                    {
                        case "1":
                            while (true)
                            {
                                ShowMenuSearchPeople();
                                var firstName = ReadString("firstName");
                                var surname = ReadString("surname");
                                var resultList = Repository.SearchPeople(firstName, surname);
                                Loading();
                                if (resultList.Any())
                                {
                                    Console.WriteLine($"\n\nSearch Results " +
                                                          $"for the contexts {UNDERLINE}{firstName.ToUpper()}{RESET} " +
                                                          $"and {UNDERLINE}{surname.ToUpper()}{RESET}:\n");
                                    foreach (var result in resultList)
                                    {
                                        Console.WriteLine($"ID: {result.Id}\n" +
                                                          $"Name: {result.FirstName}\n" +
                                                          $"Surname: {result.SurnameName}\n" +
                                                          $"Birthday: {result.Birthday}\n" +
                                                          $"\n- - - - - - - - - - - - - - - - - - - - - -\n");
                                    }
                                    Console.WriteLine($"Choose de ID of the desired " +
                                                      $"person to check the countdown.\n");
                                    var numberID = ReadNumber("id");
                                    var countdown = DateCountdown(numberID);
                                    var verb = "are";
                                    var dayWord = "days";
                                    if (countdown == 1) { verb = "is"; dayWord = "day"; }
                                    Console.WriteLine($"There {verb} {countdown} {dayWord} before {PersonFullName(numberID)}'s birthday.");
                                }
                                else
                                {
                                    Console.WriteLine($"\nNo results were found with these " +
                                                      $"search contexts: '{UNDERLINE}{firstName}{RESET}' " +
                                                      $"and '{UNDERLINE}{surname}{RESET}'.\nTry again.");
                                }
                                break;
                            }
                            break;
                        case "2":
                            while (true)
                            {
                                ShowMenuAddPeople();

                                var firstName = ReadString("firstName");
                                var surname = ReadString("surname");

                                var birthday = new Func<DateTime>(() =>
                                {
                                    var completeDate = "";
                                    var finalDate = new DateTime();
                                    do
                                    {
                                        var day = ReadNumber("day");
                                        var month = ReadNumber("month");
                                        var year = ReadNumber("year");
                                        completeDate = year + "/" + month + "/" + day;
                                        if (DateValidation(completeDate) == default)
                                        {
                                            Console.WriteLine("Invalid date.\nTry again.");
                                            ClearScreen();
                                        }
                                        else
                                        {
                                            finalDate = ConvertToDateTimeObject(day, month, year)[0];
                                        }
                                    } while (DateValidation(completeDate) == default);
                                    return finalDate;
                                })();

                                var person = new Person(id, firstName, surname, birthday);
                                var message = Repository.AddPerson(person);
                                if (message.Equals("Person added.")) { id++; }
                                Console.WriteLine(message);

                                //if (message.Equals("Person added."))
                                //{
                                //    var bList = Repository.SearchPeople();
                                //    foreach (var p in bList)
                                //    {
                                //        Console.WriteLine($"ID: {p.Id} Name: {p.FirstName} Surname: {p.SurnameName} Birthday: {p.Birthday}");
                                //    }
                                //    id++;
                                //}
                                break;
                            };
                            break;
                    }
                    //Console.Write("Enter with the first number: ");
                    //var firstInput = ReadNumber();
                    //Console.Write("Enter with the second number: ");
                    //var secondInput = ReadNumber();
                    //Console.WriteLine("\n" + ExecuteOperations(operation, firstInput, secondInput));
                    //Thread.Sleep(1000);
                    //Console.Clear();
                }
            }
        }

        public static void ShowMenu()
        {
            Console.WriteLine("\n*** C# Birthday Manager ***\n\n  " +
                                        "Select an option:\n    " +
                                        "1- Search people\n    " +
                                        "2- Add people\n    " +
                                        "3- EXIT\n\n");
            // Console.Write("Choose an operation: ");
        }

        public static void ShowMenuAddPeople()
        {
            Console.WriteLine("\nAdd a new person:\n\n");
        }

        public static void ShowMenuSearchPeople()
        {
            Console.WriteLine("\nSearch for a person:\n\n");
        }

        public static string ReadNumber(string option)
        {
            return new Func<string>(() =>
            {
                return option switch
                {
                    "menu" => InputLoopNumber("1", "3", "operation"),
                    "day" => InputLoopNumber("1", "31", "day"),
                    "month" => InputLoopNumber("1", "12", "month"),
                    "year" => InputLoopNumber("1", "9999", "year"),
                    "id" => InputLoopNumber("0", (Repository.people.Count()-1).ToString(), "ID number"),
                    _ => null,
                };
            })();
        }

        public static string ReadString(string option)
        {
            return new Func<string>(() =>
            {
                return option switch
                {
                    "firstName" => InputLoopString("first name"),
                    "surname" => InputLoopString("surname"),
                    _ => null,
                };
            })();
        }

        public static string InputLoopNumber(string minimum, string maximum, string type)
        {
            var min = Parsing.StringToInt(minimum)[0];
            var max = Parsing.StringToInt(maximum)[0];
            while (true)
            {
                Console.WriteLine("Enter with the " + type + ": ");
                var inputNumber = Console.ReadLine().Trim();
                if (!String.IsNullOrEmpty(inputNumber))
                {
                    var converted = Parsing.StringToInt(inputNumber);
                    if (converted != null && (converted[0] >= min && converted[0] <= max))
                    {
                        return converted[0].ToString();
                    }
                    else
                    {
                        Console.WriteLine("Invalid number.\n" +
                                          "It must be an interger number between " +
                                          (Parsing.StringToInt(minimum)[0]).ToString() +
                                          " and " + (Parsing.StringToInt(maximum)[0]).ToString() +
                                          ". \nTry again.");
                    }
                }
                else
                {
                    Console.WriteLine("Error: Empty field.\n" +
                                      "Try again.");
                }
                ClearScreen();
            }
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void ClearScreen()
        {
            Thread.Sleep(1000);
            System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
            Console.Clear();
            Environment.Exit(0);
        }

        public static string InputLoopString(string custom)
        {
            while (true)
            {
                Console.WriteLine("Enter with the " + custom + ": ");
                var inputString = Console.ReadLine().Trim();
                if (!String.IsNullOrEmpty(inputString))
                {
                    if (StringValidation(inputString)[0].Equals("valid") &&
                       !StringValidation(inputString)[1].Equals("valid"))
                    {
                        return inputString;
                    }
                    else
                    {
                        Console.WriteLine("Invalid data.\nTry again.");
                    }
                }
                else
                {
                    Console.WriteLine("Error: Empty field.\n" +
                                      "Try again.");
                }
                ClearScreen();
                //Thread.Sleep(1000);
                //System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
                //Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
                //Environment.Exit(0);
            }
        }

        public int ReadDay()
        {
            var number = InputLoopString("day");
            var day = StringToInt(number)[0];
            return day;
        }

        public int ReadMonth()
        {
            var number = InputLoopString("month");
            var month = StringToInt(number)[0];
            return month;
        }

        public int ReadYear()
        {
            var number = InputLoopString("year");
            var year = StringToInt(number)[0];
            return year;
        }

        public static void Loading() {
            Console.WriteLine("\n\nSearching");
            for (var i = 0; i <= 30; i++) 
            {
                Console.Write("-");
                Thread.Sleep(250);
                if (i == 30) { Console.WriteLine("\n"); }
            }
        }
    }
}
