namespace FitnessApp;
using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

public class ConsoleUI {
    DataManager dataManager;
    Reporter reporter;

    public ConsoleUI() {
        dataManager = new DataManager();
        reporter = new Reporter();
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
                        if (!JogData.TryParseTime(startTime, out _)) {
                            Console.WriteLine("Please enter time in HHmm or HH:mm format");
                        }
                    } while (!JogData.TryParseTime(startTime, out _));

                    string endTime;
                    do {
                        endTime = AskForInput("Enter End Time (HHMM):");
                        if (!JogData.TryParseTime(endTime, out _)) {
                            Console.WriteLine("Please enter time in HHmm or HH:mm format");
                        }
                    } while (!JogData.TryParseTime(endTime, out _));

                    // record the entry time automatically
                    JogData newJogData = new JogData(selectedUser, startTime, endTime, DateTime.Now);
                    dataManager.AddNewJogData(newJogData);

                    Console.WriteLine("You entered " + startTime + " as start time and " + endTime + " as end time.");

                    // display all the entries in a nice table format including calculated duration
                    var table = new Table();
                    table.Title = new TableTitle("Jogging Data");
                    table.AddColumns("User", "Start Time", "End Time", "Duration", "Recorded At");

                    // make sure jog data is loaded before iterating
                    foreach (var entry in dataManager.GetAllJogData()) {
                        table.AddRow(
                            entry.User.ToString(),
                            entry.StartTime,
                            entry.EndTime,
                            entry.Duration.ToString(@"hh\:mm"),
                            entry.RecordedAt.ToString("yyyy-MM-dd HH:mm"));
                    }

                    AnsiConsole.Write(table);

                    Console.WriteLine($"Last jog duration: {newJogData.Duration.ToString(@"h\:mm")} (hours:minutes)");

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

            } else if (selectedUser != null && selectedMainNavItem.Item == "View history & Compare") {
                ShowWorkoutHistoryAndCompare(selectedUser);
            } else if (selectedUser != null && selectedMainNavItem.Item == "Goals") {
                ShowGoalsMenu(selectedUser);
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

    private void ShowWorkoutHistoryAndCompare(User user)
    {
        var summary = reporter.GetWorkoutSummaryByDate(user, dataManager.GetAllJogData(), dataManager.GetAllPushUpData());

        if (!summary.Any())
        {
            Console.WriteLine($"No workout data found for {user.Name}.");
            return;
        }

        // Display overall history table
        var historyTable = new Table();
        historyTable.Title = new TableTitle($"Workout History for {user.Name}");
        historyTable.AddColumns("Date", "Total Jog Duration", "Total Push-ups");

        foreach (var entry in summary.OrderByDescending(s => s.Key))
        {
            historyTable.AddRow(
                entry.Key.ToString("yyyy-MM-dd"),
                entry.Value.TotalJogDuration.ToString(@"hh\:mm"),
                entry.Value.TotalPushUps.ToString()
            );
        }

        AnsiConsole.Write(historyTable);

        // Option to compare specific dates
        var dates = summary.Keys.OrderByDescending(d => d).ToList();
        if (dates.Count >= 2)
        {
            Console.WriteLine("\nWould you like to compare two specific days? (y/n)");
            var response = Console.ReadLine()?.ToLower();
            if (response == "y" || response == "yes")
            {
                var date1 = AnsiConsole.Prompt(
                    new SelectionPrompt<DateTime>()
                        .Title("Select first date to compare")
                        .AddChoices(dates)
                        .UseConverter(d => d.ToString("yyyy-MM-dd")));

                var remainingDates = dates.Where(d => d != date1).ToList();
                var date2 = AnsiConsole.Prompt(
                    new SelectionPrompt<DateTime>()
                        .Title("Select second date to compare")
                        .AddChoices(remainingDates)
                        .UseConverter(d => d.ToString("yyyy-MM-dd")));

                ShowComparison(user, date1, date2);
            }
        }
    }

    private void ShowComparison(User user, DateTime date1, DateTime date2)
    {
        var jogHistory = reporter.GetJogHistoryByDate(user, dataManager.GetAllJogData());
        var pushUpHistory = reporter.GetPushUpHistoryByDate(user, dataManager.GetAllPushUpData());

        var comparisonTable = new Table();
        comparisonTable.Title = new TableTitle($"Comparison: {date1:yyyy-MM-dd} vs {date2:yyyy-MM-dd} for {user.Name}");
        comparisonTable.AddColumns("Activity", date1.ToString("yyyy-MM-dd"), date2.ToString("yyyy-MM-dd"), "Difference");

        // Jog comparison
        var jog1 = jogHistory.ContainsKey(date1) ? TimeSpan.FromTicks(jogHistory[date1].Sum(j => j.Duration.Ticks)) : TimeSpan.Zero;
        var jog2 = jogHistory.ContainsKey(date2) ? TimeSpan.FromTicks(jogHistory[date2].Sum(j => j.Duration.Ticks)) : TimeSpan.Zero;
        var jogDiff = jog2 - jog1;
        comparisonTable.AddRow(
            "Jog Duration",
            jog1.ToString(@"hh\:mm"),
            jog2.ToString(@"hh\:mm"),
            (jogDiff.TotalMinutes > 0 ? "+" : "") + jogDiff.ToString(@"hh\:mm")
        );

        // Push-up comparison
        var push1 = pushUpHistory.ContainsKey(date1) ? pushUpHistory[date1].Sum(p => p.Count) : 0;
        var push2 = pushUpHistory.ContainsKey(date2) ? pushUpHistory[date2].Sum(p => p.Count) : 0;
        var pushDiff = push2 - push1;
        comparisonTable.AddRow(
            "Push-ups",
            push1.ToString(),
            push2.ToString(),
            (pushDiff > 0 ? "+" : "") + pushDiff.ToString()
        );

        AnsiConsole.Write(comparisonTable);

        // Show detailed sessions if available
        if (jogHistory.ContainsKey(date1) || jogHistory.ContainsKey(date2))
        {
            Console.WriteLine("\nDetailed Jog Sessions:");
            var detailedJogTable = new Table();
            detailedJogTable.AddColumns("Date", "Start Time", "End Time", "Duration");

            foreach (var date in new[] { date1, date2 })
            {
                if (jogHistory.ContainsKey(date))
                {
                    foreach (var jog in jogHistory[date])
                    {
                        detailedJogTable.AddRow(
                            date.ToString("yyyy-MM-dd"),
                            jog.StartTime,
                            jog.EndTime,
                            jog.Duration.ToString(@"hh\:mm")
                        );
                    }
                }
            }

            AnsiConsole.Write(detailedJogTable);
        }

        if (pushUpHistory.ContainsKey(date1) || pushUpHistory.ContainsKey(date2))
        {
            Console.WriteLine("\nDetailed Push-up Sessions:");
            var detailedPushTable = new Table();
            detailedPushTable.AddColumns("Date", "Count", "Time");

            foreach (var date in new[] { date1, date2 })
            {
                if (pushUpHistory.ContainsKey(date))
                {
                    foreach (var push in pushUpHistory[date])
                    {
                        detailedPushTable.AddRow(
                            date.ToString("yyyy-MM-dd"),
                            push.Count.ToString(),
                            push.RecordedAt.ToString("HH:mm")
                        );
                    }
                }
            }

            AnsiConsole.Write(detailedPushTable);
        }
    }

    private void ShowGoalsMenu(User user) {
        var goalMenuItems = new List<string> { "Create Goal", "View Goals", "Back" };
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Goals Menu")
                .AddChoices(goalMenuItems));

        if (selected == "Create Goal") {
            CreateGoal(user);
        } else if (selected == "View Goals") {
            ViewGoals(user);
        }
    }

    private void CreateGoal(User user) {
        var activity = AnsiConsole.Prompt(
            new SelectionPrompt<Activity>()
                .Title("Select activity for goal")
                .AddChoices(dataManager.Activities));

        var period = AnsiConsole.Prompt(
            new SelectionPrompt<GoalType>()
                .Title("Select goal period")
                .AddChoices(Enum.GetValues<GoalType>()));

        GoalTargetType targetType;
        if (activity.Name == "Jogging") {
            targetType = GoalTargetType.Duration;
        } else if (activity.Name == "Push-ups") {
            targetType = GoalTargetType.Count;
        } else {
            // For other activities, ask
            targetType = AnsiConsole.Prompt(
                new SelectionPrompt<GoalTargetType>()
                    .Title("Select target type")
                    .AddChoices(Enum.GetValues<GoalTargetType>()));
        }

        int targetCount = 0;
        TimeSpan targetDuration = TimeSpan.Zero;

        if (targetType == GoalTargetType.Count) {
            string countInput;
            do {
                countInput = AskForInput("Enter target count:");
                if (!int.TryParse(countInput, out targetCount) || targetCount <= 0) {
                    Console.WriteLine("Please enter a positive whole number.");
                }
            } while (!int.TryParse(countInput, out targetCount) || targetCount <= 0);
        } else {
            string durationInput;
            do {
                durationInput = AskForInput("Enter target duration in minutes:");
                if (!int.TryParse(durationInput, out var minutes) || minutes <= 0) {
                    Console.WriteLine("Please enter a positive whole number for minutes.");
                } else {
                    targetDuration = TimeSpan.FromMinutes(minutes);
                }
            } while (targetDuration == TimeSpan.Zero);
        }

        string description = AskForInput("Enter goal description:");

        var goal = new Goal(user, activity, period, targetType, targetCount, targetDuration, description);
        dataManager.AddNewGoal(goal);

        Console.WriteLine("Goal created successfully!");
    }

    private void ViewGoals(User user) {
        var userGoals = dataManager.GetAllGoals().Where(g => g.User.Name == user.Name).ToList();

        if (!userGoals.Any()) {
            Console.WriteLine("No goals found for this user.");
            return;
        }

        var table = new Table();
        table.Title = new TableTitle($"Goals for {user.Name}");
        table.AddColumns("Activity", "Period", "Target", "Current Progress", "Description", "Created");

        foreach (var goal in userGoals) {
            string targetStr = goal.TargetType == GoalTargetType.Count 
                ? $"{goal.TargetCount}" 
                : $"{goal.TargetDuration.TotalMinutes} minutes";
            
            string progressStr = CalculateProgress(goal);

            table.AddRow(
                goal.Activity.Name,
                goal.Period.ToString(),
                targetStr,
                progressStr,
                goal.Description,
                goal.CreatedAt.ToString("yyyy-MM-dd")
            );
        }

        AnsiConsole.Write(table);
    }

    private string CalculateProgress(Goal goal) {
        DateTime now = DateTime.Now;
        DateTime startDate;

        if (goal.Period == GoalType.Daily) {
            startDate = now.Date;
        } else if (goal.Period == GoalType.Weekly) {
            // Start of week (Monday)
            int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            startDate = now.AddDays(-diff).Date;
        } else { // Monthly
            startDate = new DateTime(now.Year, now.Month, 1);
        }

        if (goal.TargetType == GoalTargetType.Count) {
            // For push-ups
            var pushUps = dataManager.GetAllPushUpData()
                .Where(p => p.User.Name == goal.User.Name && p.RecordedAt >= startDate)
                .Sum(p => p.Count);
            return $"{pushUps}/{goal.TargetCount}";
        } else {
            // For jogging duration
            var jogDuration = dataManager.GetAllJogData()
                .Where(j => j.User.Name == goal.User.Name && j.RecordedAt >= startDate)
                .Sum(j => j.Duration.TotalMinutes);
            return $"{jogDuration:F0}/{goal.TargetDuration.TotalMinutes} min";
        }
    }

    public static string AskForInput(string message)
    {
        Console.WriteLine(message);
        return Console.ReadLine();
    }
}