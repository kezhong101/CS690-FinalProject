namespace FitnessApp;

using System;
using System.Collections.Generic;
using System.IO;

public class DataManager {

    FileSaver jogFileSaver;
    FileSaver pushUpFileSaver;
    FileSaver goalFileSaver;
    private bool jogDataLoaded;
    private bool pushUpDataLoaded;
    private bool goalDataLoaded;


    public List<User> Users { get; }
    public List<MainNavItem> MainNavItems { get; }
    public List<Activity> Activities { get; }
    public List<JogData> JogData { get; private set; }
    public List<PushUpData> PushUpData { get; private set; }
    public List<Goal> Goals { get; private set; }

    /// <summary>
    /// Public accessor that ensures data is loaded lazily.
    /// </summary>
    public IEnumerable<JogData> GetAllJogData() {
        EnsureJogDataLoaded();
        return JogData;
    }

    public IEnumerable<PushUpData> GetAllPushUpData() {
        EnsurePushUpDataLoaded();
        return PushUpData;
    }

    public IEnumerable<Goal> GetAllGoals() {
        EnsureGoalDataLoaded();
        return Goals;
    }

    public DataManager() {

        jogFileSaver = new FileSaver("jog-data.txt");
        pushUpFileSaver = new FileSaver("pushup-data.txt");
        goalFileSaver = new FileSaver("goals-data.txt");

        Users = new List<User>();
        Users.Add(new User("Ethan Smith"));
        Users.Add(new User("Jane Doe"));

        MainNavItems = new List<MainNavItem>();
        MainNavItems.Add(new MainNavItem("Select User"));
        MainNavItems.Add(new MainNavItem("Log Activity"));
        MainNavItems.Add(new MainNavItem("View history & Compare"));
        MainNavItems.Add(new MainNavItem("Goals"));
        MainNavItems.Add(new MainNavItem("Shared Goals"));
        MainNavItems.Add(new MainNavItem("End"));

        Activities = new List<Activity>();
        Activities.Add(new Activity("Jogging"));
        Activities.Add(new Activity("Push-ups"));
        Activities.Add(new Activity("Strength Training"));

        // do not load data immediately; load lazily when requested
        JogData = new List<JogData>();
        PushUpData = new List<PushUpData>();
        Goals = new List<Goal>();
    }

    public void AddNewJogData(JogData data) {
        EnsureJogDataLoaded();

        if (data.RecordedAt == DateTime.MinValue) {
            // ensure there's always a timestamp
            data = new JogData(data.User, data.StartTime, data.EndTime, DateTime.Now);
        }
        this.JogData.Add(data);
        this.jogFileSaver.AppendData(data);
    }

    public void AddNewPushUpData(PushUpData data) {
        EnsurePushUpDataLoaded();

        if (data.RecordedAt == DateTime.MinValue) {
            data = new PushUpData(data.User, data.Count, DateTime.Now);
        }
        this.PushUpData.Add(data);
        this.pushUpFileSaver.AppendData(data);
    }

    public void AddNewGoal(Goal goal) {
        EnsureGoalDataLoaded();

        this.Goals.Add(goal);
        this.goalFileSaver.AppendData(goal);
    }

    public void JoinGoal(Goal goal, User user) {
        EnsureGoalDataLoaded();

        if (goal.IsShared && !goal.Participants.Contains(user)) {
            goal.AddParticipant(user);
            // Resave all goals to update the file
            RewriteGoalsFile();
        }
    }

    private void RewriteGoalsFile() {
        var lines = Goals.Select(g => g.ToString()).ToArray();
        File.WriteAllLines("goals-data.txt", lines);
    }

    // make sure data is loaded from file before use
    private void EnsureJogDataLoaded() {
        if (jogDataLoaded) return;
        jogDataLoaded = true;

        JogData = new List<JogData>();
        if (!File.Exists("jog-data.txt")) {
            File.Create("jog-data.txt").Dispose();
        }
        var jogFileContent = File.ReadAllLines("jog-data.txt");
        foreach (var line in jogFileContent) {
            var splitted = line.Split("|", StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length < 3) continue; // malformed

            var userName = splitted[0];
            var user = new User(userName);

            var startTime = splitted[1];
            var endTime = splitted[2];

            DateTime recordedAt = DateTime.MinValue;
            if (splitted.Length >= 4 && DateTime.TryParse(splitted[3], null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)) {
                recordedAt = parsed;
            } else {
                recordedAt = DateTime.Now;
            }

            JogData.Add(new JogData(user, startTime, endTime, recordedAt));
        }
    }

    private void EnsurePushUpDataLoaded() {
        if (pushUpDataLoaded) return;
        pushUpDataLoaded = true;

        PushUpData = new List<PushUpData>();
        if (!File.Exists("pushup-data.txt")) {
            File.Create("pushup-data.txt").Dispose();
        }

        var fileContent = File.ReadAllLines("pushup-data.txt");
        foreach (var line in fileContent) {
            var splitted = line.Split("|", StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length < 3) continue;

            var userName = splitted[0];
            var user = new User(userName);

            if (!int.TryParse(splitted[1], out var count)) continue;

            DateTime recordedAt = DateTime.MinValue;
            if (splitted.Length >= 3 && DateTime.TryParse(splitted[2], null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)) {
                recordedAt = parsed;
            } else {
                recordedAt = DateTime.Now;
            }

            PushUpData.Add(new PushUpData(user, count, recordedAt));
        }
    }

    private void EnsureGoalDataLoaded() {
        if (goalDataLoaded) return;
        goalDataLoaded = true;

        Goals = new List<Goal>();
        if (!File.Exists("goals-data.txt")) {
            File.Create("goals-data.txt").Dispose();
        }

        var fileContent = File.ReadAllLines("goals-data.txt");
        foreach (var line in fileContent) {
            var splitted = line.Split("|", StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length < 8) continue;

            var creatorName = splitted[0];
            var creator = new User(creatorName);

            var activityName = splitted[1];
            var activity = new Activity(activityName);

            if (!Enum.TryParse<GoalType>(splitted[2], out var period)) continue;
            if (!Enum.TryParse<GoalTargetType>(splitted[3], out var targetType)) continue;

            if (!int.TryParse(splitted[4], out var targetCount)) continue;
            if (!double.TryParse(splitted[5], out var targetMinutes)) continue;
            var targetDuration = TimeSpan.FromMinutes(targetMinutes);

            if (!DateTime.TryParse(splitted[6], null, System.Globalization.DateTimeStyles.RoundtripKind, out var createdAt)) continue;

            var description = splitted[7];

            bool isShared = false;
            List<User> participants = new List<User> { creator };

            if (splitted.Length >= 10) {
                if (bool.TryParse(splitted[8], out var parsedShared)) {
                    isShared = parsedShared;
                }
                var participantsStr = splitted[9];
                participants = participantsStr.Split(',').Select(name => new User(name.Trim())).ToList();
            }

            var goal = new Goal(creator, activity, period, targetType, targetCount, targetDuration, description, isShared);
            goal.Participants.Clear();
            goal.Participants.AddRange(participants);
            Goals.Add(goal);
        }
    }

}

