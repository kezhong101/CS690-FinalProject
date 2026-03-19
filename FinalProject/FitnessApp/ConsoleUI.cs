namespace FitnessApp;
using System;
using System.Linq;
using Spectre.Console;

public class ConsoleUI {
    DataManager dataManager;

    public ConsoleUI() {
        dataManager = new DataManager();
    }

    public void Show() {

        User selectedUser = null;
        MainNavItem selectedMainNavItem;

        do {
            selectedMainNavItem = AnsiConsole.Prompt(
                                    new SelectionPrompt<MainNavItem>()
                                        .Title("Select a menu")
                                        .AddChoices(dataManager.MainNavItems));

            if (selectedMainNavItem.Item == "Select User") {
                selectedUser = AnsiConsole.Prompt(
                                    new SelectionPrompt<User>()
                                        .Title("Select a user")
                                        .AddChoices(dataManager.Users));

                Console.WriteLine("Current User: " + selectedUser.Name);

            } else if (selectedUser != null && selectedMainNavItem.Item == "Log Activity") {
                Activity selectedActivity = AnsiConsole.Prompt(
                                        new SelectionPrompt<Activity>()
                                            .Title("Select an activity")
                                            .AddChoices(dataManager.Activities));

                Console.WriteLine("You selected "+selectedActivity.Name+" activity!");

                if (selectedActivity.Name == "Jogging") {
                    // use AskForInput helper to reduce repeated prompting logic
                    string startTime;
                    do {
                        startTime = AskForInput("Enter Start Time (HHMM):");
                        if (!TimeSpan.TryParse(startTime, out _)) {
                            Console.WriteLine("Please enter time in HHmm format");
                        }
                    } while (!TimeSpan.TryParse(startTime, out _));

                    string endTime;
                    do {
                        endTime = AskForInput("Enter End Time (HHMM):");
                        if (!TimeSpan.TryParse(endTime, out _)) {
                            Console.WriteLine("Please enter time in HHmm format");
                        }
                    } while (!TimeSpan.TryParse(endTime, out _));

                    // record the entry time automatically
                    JogData newJogData = new JogData(selectedUser, startTime, endTime, DateTime.Now);
                    dataManager.AddNewJogData(newJogData);

                    Console.WriteLine("You entered " + startTime + " as start time and " + endTime + " as end time.");

                    // display all the entries in a nice table format
                    var table = new Table();
                    table.Title = new TableTitle("Jogging Data");
                    table.AddColumns("User", "Start Time", "End Time", "Recorded At");

                    // make sure jog data is loaded before iterating
                    foreach (var entry in dataManager.GetAllJogData()) {
                        table.AddRow(entry.User.ToString(), entry.StartTime, entry.EndTime, entry.RecordedAt.ToString("yyyy-MM-dd HH:mm"));
                    }

                    AnsiConsole.Write(table);

                } else if (selectedActivity.Name == "Push-ups") {
                    int count;
                    string countInput;

                    do {
                        countInput = AskForInput("Enter push-up count:");
                        if (!int.TryParse(countInput, out count) || count <= 0) {
                            Console.WriteLine("Please enter a positive whole number for push-up count.");
                        }
                    } while (!int.TryParse(countInput, out count) || count <= 0);

                    PushUpData newPushUpData = new PushUpData(selectedUser, count, DateTime.Now);
                    dataManager.AddNewPushUpData(newPushUpData);

                    Console.WriteLine($"Added {count} push-ups for {selectedUser.Name} at {newPushUpData.RecordedAt:yyyy-MM-dd HH:mm}.");

                    // show all push-up entries and daily running total for selected user
                    var table = new Table();
                    table.Title = new TableTitle("Push-ups Data");
                    table.AddColumns("User", "Count", "Recorded At");

                    foreach (var entry in dataManager.GetAllPushUpData().Where(e => e.User.Name == selectedUser.Name)) {
                        table.AddRow(entry.User.ToString(), entry.Count.ToString(), entry.RecordedAt.ToString("yyyy-MM-dd HH:mm"));
                    }

                    AnsiConsole.Write(table);

                    var todayTotal = dataManager.GetAllPushUpData()
                        .Where(e => e.User.Name == selectedUser.Name && e.RecordedAt.Date == DateTime.Now.Date)
                        .Sum(e => e.Count);

                    Console.WriteLine($"Total push-ups today for {selectedUser.Name}: {todayTotal}");

                } else if (selectedActivity.Name == "Strength Training") {
                }

            } else {
                if (selectedMainNavItem.Item != "End") {
                    Console.WriteLine("Please select a user first!");
                    selectedUser = AnsiConsole.Prompt(
                        new SelectionPrompt<User>()
                            .Title("Select a user")
                            .AddChoices(dataManager.Users));
                }
            }
        } while (selectedMainNavItem.Item != "End");
    }

    public static string AskForInput(string message)
    {
        Console.WriteLine(message);
        return Console.ReadLine();
    }
}