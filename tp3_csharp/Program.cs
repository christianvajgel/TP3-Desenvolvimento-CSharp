using System;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using ClassLibrary_tp3_csharp;
using static ClassLibrary_tp3_csharp.Person;
using static ClassLibrary_tp3_csharp.Parsing;
using static ClassLibrary_tp3_csharp.Repository;
using static ClassLibrary_tp3_csharp.Validations;
using System.Collections.Generic;

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
            var resultList = Enumerable.Empty<Person>();
            while (true)
            {
                ShowMenu();
                var operation = ReadNumber("menu", resultList);
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
                                ClearScreen(false);
                                ShowMenuSearchPeople();
                                var firstName = ReadString("firstName");
                                var surname = ReadString("surname");
                                resultList = Repository.SearchPeople(firstName, surname);
                                Loading();
                                if (resultList.Any())
                                {
                                    ClearScreen(false);
                                    Console.WriteLine($"\n\nSearch Results " +
                                                          $"for the contexts {UNDERLINE}{firstName.ToUpper()}{RESET} " +
                                                          $"and {UNDERLINE}{surname.ToUpper()}{RESET}:\n");
                                    PrintResultList(resultList);
                                    Console.WriteLine($"Choose de ID of the desired " +
                                                      $"person to check the countdown.\n");
                                    var numberID = ReadNumber("id", resultList);
                                    var countdown = DateCountdown(numberID);
                                    var verb = "are";
                                    var dayWord = "days";
                                    if (countdown == 1) { verb = "is"; dayWord = "day"; }
                                    ClearScreen(false);
                                    Console.WriteLine($"There {verb} {countdown} {dayWord} " +
                                                      $"before {PersonFullName(numberID)}'s birthday.");
                                }
                                else
                                {
                                    ClearScreen(false);
                                    Console.WriteLine($"\nNo results were found with these " +
                                                      $"search contexts: '{UNDERLINE}{firstName}{RESET}' " +
                                                      $"and '{UNDERLINE}{surname}{RESET}'.\nTry again.");
                                }
                                ClearScreen(true);
                                break;
                            }
                            break;
                        case "2":
                            while (true)
                            {
                                ClearScreen(false);
                                ShowMenuAddPeople();

                                var firstName = ReadString("firstName");
                                var surname = ReadString("surname");

                                var birthday = new Func<DateTime>(() =>
                                {
                                    var completeDate = "";
                                    var finalDate = new DateTime();
                                    do
                                    {
                                        var day = ReadNumber("day", resultList);
                                        var month = ReadNumber("month", resultList);
                                        var year = ReadNumber("year", resultList);
                                        completeDate = year + "/" + month + "/" + day;
                                        if (DateValidation(completeDate) == default)
                                        {
                                            Console.WriteLine("Invalid date.\nTry again.");
                                            ClearScreen(false);
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
                                ClearScreen(false);
                                break;
                            };
                            break;
                    }
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
        }

        public static void ShowMenuAddPeople()
        {
            Console.WriteLine("\nAdd a new person:\n\n");
        }

        public static void ShowMenuSearchPeople()
        {
            Console.WriteLine("\nSearch for a person:\n\n");
        }

        public static string ReadNumber(string option, IEnumerable<Person> resultList)
        {
            return new Func<string>(() =>
            {
                return option switch
                {
                    "menu" => InputLoopNumber("1", "3", "operation", resultList),
                    "day" => InputLoopNumber("1", "31", "day", resultList),
                    "month" => InputLoopNumber("1", "12", "month", resultList),
                    "year" => InputLoopNumber("1", "9999", "year", resultList),
                    "id" => InputLoopNumber("0", (Repository.people.Count() - 1).ToString(), "ID number", resultList),
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

        public static string InputLoopNumber(string minimum, string maximum, string type, IEnumerable<Person> resultList)
        {
            var min = Parsing.StringToInt(minimum)[0];
            var max = Parsing.StringToInt(maximum)[0];
            var ok = false;
            while (true)
            {
                if (type.Equals("ID number") && ok == true) { PrintResultList(resultList); }
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
                        ok = true;
                        Console.WriteLine("\nInvalid number.\n" +
                                          "It must be an interger number between " +
                                          (Parsing.StringToInt(minimum)[0]).ToString() +
                                          " and " + (Parsing.StringToInt(maximum)[0]).ToString() +
                                          ". \nTry again.");
                    }
                }
                else
                {
                    Console.WriteLine("\nError: Empty field.\nTry again.");
                }
                ClearScreen(false);
            }
        }

        public static void ClearScreen(bool returnType)
        {
            if (returnType == true) { Console.WriteLine("\nPress any key to return..."); Console.ReadKey(); }
            Thread.Sleep(1000);
            Console.Clear();
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
                ClearScreen(false);
            }
        }

        public static void PrintResultList(IEnumerable<Person> resultList)
        {
            foreach (var result in resultList)
            {
                Console.WriteLine($"ID: {result.Id}\n" +
                                  $"Name: {result.FirstName}\n" +
                                  $"Surname: {result.SurnameName}\n" +
                                  $"Birthday: {result.Birthday}\n" +
                                  $"\n- - - - - - - - - - - - - - - - - - - - - -\n");
            }
        }

        public static void Loading()
        {
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
