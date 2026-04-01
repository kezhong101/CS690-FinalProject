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
                    StrengthTrainingData newStrengthData = new StrengthTrainingData(selectedUser, startTime, endTime, DateTime.Now);
                    dataManager.AddNewStrengthTrainingData(newStrengthData);

                    Console.WriteLine("You entered " + startTime + " as start time and " + endTime + " as end time.");

                    // display all the entries in a nice table format including calculated duration
                    var table = new Table();
                    table.Title = new TableTitle("Strength Training Data");
                    table.AddColumns("User", "Start Time", "End Time", "Duration", "Recorded At");

                    // make sure strength data is loaded before iterating
                    foreach (var entry in dataManager.GetAllStrengthTrainingData()) {
                        table.AddRow(
                            entry.User.ToString(),
                            entry.StartTime,
                            entry.EndTime,
                            entry.Duration.ToString(@"hh\:mm"),
                            entry.RecordedAt.ToString("yyyy-MM-dd HH:mm"));
                    }

                    AnsiConsole.Write(table);

                    Console.WriteLine($"Last strength training duration: {newStrengthData.Duration.ToString(@"h\:mm")} (hours:minutes)");

                }

            } else if (selectedUser != null && selectedMainNavItem.Item == "View history & Compare") {
                ShowWorkoutHistoryAndCompare(selectedUser);
            } else if (selectedUser != null && selectedMainNavItem.Item == "Goals") {
                ShowGoalsMenu(selectedUser);
            } else if (selectedUser != null && selectedMainNavItem.Item == "Shared Goals") {
                ShowSharedGoalsMenu(selectedUser);
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
        var summary = reporter.GetWorkoutSummaryByDate(user, dataManager.GetAllJogData(), dataManager.GetAllPushUpData(), dataManager.GetAllStrengthTrainingData());

        if (!summary.Any())
        {
            Console.WriteLine($"No workout data found for {user.Name}.");
            return;
        }

        // Display overall history table
        var historyTable = new Table();
        historyTable.Title = new TableTitle($"Workout History for {user.Name}");
        historyTable.AddColumns("Date", "Total Jog Duration", "Total Push-ups", "Total Strength Duration");

        foreach (var entry in summary.OrderByDescending(s => s.Key))
        {
            historyTable.AddRow(
                entry.Key.ToString("yyyy-MM-dd"),
                entry.Value.TotalJogDuration.ToString(@"hh\:mm"),
                entry.Value.TotalPushUps.ToString(),
                entry.Value.TotalStrengthDuration.ToString(@"hh\:mm")
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
        var strengthHistory = reporter.GetStrengthTrainingHistoryByDate(user, dataManager.GetAllStrengthTrainingData());

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

        // Strength training comparison
        var strength1 = strengthHistory.ContainsKey(date1) ? TimeSpan.FromTicks(strengthHistory[date1].Sum(s => s.Duration.Ticks)) : TimeSpan.Zero;
        var strength2 = strengthHistory.ContainsKey(date2) ? TimeSpan.FromTicks(strengthHistory[date2].Sum(s => s.Duration.Ticks)) : TimeSpan.Zero;
        var strengthDiff = strength2 - strength1;
        comparisonTable.AddRow(
            "Strength Training",
            strength1.ToString(@"hh\:mm"),
            strength2.ToString(@"hh\:mm"),
            (strengthDiff.TotalMinutes > 0 ? "+" : "") + strengthDiff.ToString(@"hh\:mm")
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

        if (strengthHistory.ContainsKey(date1) || strengthHistory.ContainsKey(date2))
        {
            Console.WriteLine("\nDetailed Strength Training Sessions:");
            var detailedStrengthTable = new Table();
            detailedStrengthTable.AddColumns("Date", "Start Time", "End Time", "Duration");

            foreach (var date in new[] { date1, date2 })
            {
                if (strengthHistory.ContainsKey(date))
                {
                    foreach (var strength in strengthHistory[date])
                    {
                        detailedStrengthTable.AddRow(
                            date.ToString("yyyy-MM-dd"),
                            strength.StartTime,
                            strength.EndTime,
                            strength.Duration.ToString(@"hh\:mm")
                        );
                    }
                }
            }

            AnsiConsole.Write(detailedStrengthTable);
        }
    }

    private void ShowGoalsMenu(User user) {
        var goalMenuItems = new List<string> { "Create Goal", "View Goals", "View Goal Details", "Back" };
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Goals Menu")
                .AddChoices(goalMenuItems));

        if (selected == "Create Goal") {
            CreateGoal(user);
        } else if (selected == "View Goals") {
            ViewGoals(user);
        } else if (selected == "View Goal Details") {
            ViewGoalDetails(user);
        }
    }

    private void ShowSharedGoalsMenu(User user) {
        var sharedGoals = dataManager.GetAllGoals().Where(g => g.IsShared).ToList();

        if (!sharedGoals.Any()) {
            Console.WriteLine("No shared goals available.");
            return;
        }

        var table = new Table();
        table.Title = new TableTitle("Available Shared Goals");
        table.AddColumns("Creator", "Activity", "Period", "Target", "Description", "Participants", "Joined");

        foreach (var goal in sharedGoals) {
            string targetStr = goal.TargetType == GoalTargetType.Count 
                ? $"{goal.TargetCount}" 
                : $"{goal.TargetDuration.TotalMinutes} minutes";
            
            string participantsStr = string.Join(", ", goal.Participants.Select(p => p.Name));
            string joinedStr = goal.Participants.Any(p => p.Name == user.Name) ? "Yes" : "No";

            table.AddRow(
                goal.Creator.Name,
                goal.Activity.Name,
                goal.Period.ToString(),
                targetStr,
                goal.Description,
                participantsStr,
                joinedStr
            );
        }

        AnsiConsole.Write(table);

        // Allow joining
        var joinableGoals = sharedGoals.Where(g => !g.Participants.Any(p => p.Name == user.Name)).ToList();
        if (joinableGoals.Any()) {
            Console.WriteLine("\nWould you like to join a shared goal? (y/n)");
            var response = Console.ReadLine()?.ToLower();
            if (response == "y" || response == "yes") {
                var goalToJoin = AnsiConsole.Prompt(
                    new SelectionPrompt<Goal>()
                        .Title("Select a goal to join")
                        .AddChoices(joinableGoals)
                        .UseConverter(g => $"{g.Creator.Name}: {g.Description}"));

                dataManager.JoinGoal(goalToJoin, user);
                Console.WriteLine("Successfully joined the goal!");
            }
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
        if (activity.Name == "Jogging" || activity.Name == "Strength Training") {
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

        bool isShared = false;
        string sharedInput = AskForInput("Is this a shared goal? (y/n):");
        if (sharedInput.ToLower().StartsWith("y")) {
            isShared = true;
        }

        var goal = new Goal(user, activity, period, targetType, targetCount, targetDuration, description, isShared);
        dataManager.AddNewGoal(goal);

        Console.WriteLine("Goal created successfully!");
    }

    private void ViewGoals(User user) {
        var userGoals = dataManager.GetAllGoals().Where(g => g.Participants.Any(p => p.Name == user.Name)).ToList();

        if (!userGoals.Any()) {
            Console.WriteLine("No goals found for this user.");
            return;
        }

        var table = new Table();
        table.Title = new TableTitle($"Goals for {user.Name}");
        table.AddColumns("Type", "Activity", "Period", "Target", "Current Progress", "Description", "Created", "Participants");

        foreach (var goal in userGoals) {
            string typeStr = goal.IsShared ? "Shared" : "Personal";
            string targetStr = goal.TargetType == GoalTargetType.Count 
                ? $"{goal.TargetCount}" 
                : $"{goal.TargetDuration.TotalMinutes} minutes";
            
            string progressStr = CalculateProgress(goal, user);

            string participantsStr = string.Join(", ", goal.Participants.Select(p => p.Name));

            table.AddRow(
                typeStr,
                goal.Activity.Name,
                goal.Period.ToString(),
                targetStr,
                progressStr,
                goal.Description,
                goal.CreatedAt.ToString("yyyy-MM-dd"),
                participantsStr
            );
        }

        AnsiConsole.Write(table);
    }

    private string CalculateProgress(Goal goal, User user) {
        // For demo purposes, show all-time progress instead of period-based
        DateTime startDate = DateTime.MinValue;

        var participants = goal.IsShared ? goal.Participants : new List<User> { user };

        if (goal.TargetType == GoalTargetType.Count) {
            // For push-ups
            var pushUps = dataManager.GetAllPushUpData()
                .Where(p => participants.Any(part => part.Name == p.User.Name) && p.RecordedAt >= startDate)
                .Sum(p => p.Count);
            return $"{pushUps}/{goal.TargetCount}";
        } else {
            // For duration-based activities
            double totalMinutes = 0;
            if (goal.Activity.Name == "Jogging") {
                totalMinutes = dataManager.GetAllJogData()
                    .Where(j => participants.Any(part => part.Name == j.User.Name) && j.RecordedAt >= startDate)
                    .Sum(j => j.Duration.TotalMinutes);
            } else if (goal.Activity.Name == "Strength Training") {
                totalMinutes = dataManager.GetAllStrengthTrainingData()
                    .Where(s => participants.Any(part => part.Name == s.User.Name) && s.RecordedAt >= startDate)
                    .Sum(s => s.Duration.TotalMinutes);
            }
            return $"{totalMinutes:F0}/{goal.TargetDuration.TotalMinutes} min";
        }
    }

    private string CalculateProgressForUser(Goal goal, User participant) {
        // For demo purposes, show all-time progress instead of period-based
        DateTime startDate = DateTime.MinValue;

        if (goal.TargetType == GoalTargetType.Count) {
            // For push-ups
            var pushUps = dataManager.GetAllPushUpData()
                .Where(p => p.User.Name == participant.Name && p.RecordedAt >= startDate)
                .Sum(p => p.Count);
            return $"{pushUps}/{goal.TargetCount}";
        } else {
            // For duration-based activities
            double totalMinutes = 0;
            if (goal.Activity.Name == "Jogging") {
                totalMinutes = dataManager.GetAllJogData()
                    .Where(j => j.User.Name == participant.Name && j.RecordedAt >= startDate)
                    .Sum(j => j.Duration.TotalMinutes);
            } else if (goal.Activity.Name == "Strength Training") {
                totalMinutes = dataManager.GetAllStrengthTrainingData()
                    .Where(s => s.User.Name == participant.Name && s.RecordedAt >= startDate)
                    .Sum(s => s.Duration.TotalMinutes);
            }
            return $"{totalMinutes:F0}/{goal.TargetDuration.TotalMinutes} min";
        }
    }

    private void ViewGoalDetails(User user) {
        var userGoals = dataManager.GetAllGoals().Where(g => g.Participants.Any(p => p.Name == user.Name)).ToList();

        if (!userGoals.Any()) {
            Console.WriteLine("No goals found for this user.");
            return;
        }

        var selectedGoal = AnsiConsole.Prompt(
            new SelectionPrompt<Goal>()
                .Title("Select a goal to view details")
                .AddChoices(userGoals)
                .UseConverter(g => $"{(g.IsShared ? "Shared" : "Personal")} - {g.Activity.Name}: {g.Description}"));

        // Display goal summary
        Console.WriteLine($"Goal Details:");
        Console.WriteLine($"Type: {(selectedGoal.IsShared ? "Shared" : "Personal")}");
        Console.WriteLine($"Creator: {selectedGoal.Creator.Name}");
        Console.WriteLine($"Activity: {selectedGoal.Activity.Name}");
        Console.WriteLine($"Period: {selectedGoal.Period}");
        string targetStr = selectedGoal.TargetType == GoalTargetType.Count 
            ? $"{selectedGoal.TargetCount}" 
            : $"{selectedGoal.TargetDuration.TotalMinutes} minutes";
        Console.WriteLine($"Target: {targetStr}");
        Console.WriteLine($"Description: {selectedGoal.Description}");
        Console.WriteLine($"Created: {selectedGoal.CreatedAt:yyyy-MM-dd}");
        Console.WriteLine($"Participants: {string.Join(", ", selectedGoal.Participants.Select(p => p.Name))}");

        // Display progress
        if (selectedGoal.IsShared) {
            Console.WriteLine("\nIndividual Progress:");
            var progressTable = new Table();
            progressTable.AddColumns("Participant", "Progress");

            foreach (var participant in selectedGoal.Participants) {
                string progressStr = CalculateProgressForUser(selectedGoal, participant);
                progressTable.AddRow(participant.Name, progressStr);
            }

            // Add total progress
            string totalProgress = CalculateProgress(selectedGoal, user);
            progressTable.AddRow("Total", totalProgress);

            AnsiConsole.Write(progressTable);
        } else {
            string progressStr = CalculateProgressForUser(selectedGoal, user);
            Console.WriteLine($"Your Progress: {progressStr}");
        }
    }

    public static string AskForInput(string message)
    {
        Console.WriteLine(message);
        return Console.ReadLine();
    }
}