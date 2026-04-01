using System;
using System.Globalization;
using System.Linq;

namespace FitnessApp;

public class User {
    public string Name { get; }

    public User(string name) {
        this.Name = name;
    }

    public override string ToString() {
        return this.Name;
    }
}

public class MainNavItem {
    public string Item { get; }

    public MainNavItem(string item) {
        this.Item = item;
    }

    public override string ToString() {
        return this.Item;
    }
}

public class Activity {
    public string Name { get; }

    public Activity(string name) {
        this.Name = name;
    }

    public override string ToString() {
        return this.Name;
    }
}

public class JogData {
    public User User { get; }
    public string StartTime { get; }
    public string EndTime { get; }
    public DateTime RecordedAt { get; }

    public JogData(User user, string startTime, string endTime, DateTime recordedAt) {
        this.User = user;
        this.StartTime = startTime;
        this.EndTime = endTime;
        this.RecordedAt = recordedAt;
    }

    public TimeSpan Duration {
        get {
            if (!TryParseTime(StartTime, out var start) || !TryParseTime(EndTime, out var end)) {
                return TimeSpan.Zero;
            }
            if (end < start) {
                // crossing midnight
                return (TimeSpan.FromDays(1) - start) + end;
            }
            return end - start;
        }
    }

    public static bool TryParseTime(string value, out TimeSpan result) {
        value = value?.Trim() ?? string.Empty;
        result = TimeSpan.Zero;

        if (value.Contains(':')) {
            var formats = new[] { "h\\:mm", "hh\\:mm", "h\\:m", "hh\\:m" };
            return TimeSpan.TryParseExact(value, formats, CultureInfo.InvariantCulture, out result);
        }

        if (value.All(char.IsDigit)) {
            if (value.Length == 4) {
                var hours = int.Parse(value.Substring(0, 2));
                var mins = int.Parse(value.Substring(2, 2));
                if (hours >= 0 && hours < 24 && mins >= 0 && mins < 60) {
                    result = new TimeSpan(hours, mins, 0);
                    return true;
                }
            }
            if (value.Length == 3) {
                var hours = int.Parse(value.Substring(0, 1));
                var mins = int.Parse(value.Substring(1, 2));
                if (hours >= 0 && hours < 24 && mins >= 0 && mins < 60) {
                    result = new TimeSpan(hours, mins, 0);
                    return true;
                }
            }
        }

        return false;
    }

    public override string ToString() {
        // use an invariant sortable format so it's easy to parse later
        return $"{User.Name}|{StartTime}|{EndTime}|{RecordedAt:o}";
    }
}

public class PushUpData {
    public User User { get; }
    public int Count { get; }
    public DateTime RecordedAt { get; }

    public PushUpData(User user, int count, DateTime recordedAt) {
        this.User = user;
        this.Count = count;
        this.RecordedAt = recordedAt;
    }

    public override string ToString() {
        return $"{User.Name}|{Count}|{RecordedAt:o}";
    }
}

public class StrengthTrainingData {
    public User User { get; }
    public string StartTime { get; }
    public string EndTime { get; }
    public DateTime RecordedAt { get; }

    public StrengthTrainingData(User user, string startTime, string endTime, DateTime recordedAt) {
        this.User = user;
        this.StartTime = startTime;
        this.EndTime = endTime;
        this.RecordedAt = recordedAt;
    }

    public TimeSpan Duration {
        get {
            if (!JogData.TryParseTime(StartTime, out var start) || !JogData.TryParseTime(EndTime, out var end)) {
                return TimeSpan.Zero;
            }
            if (end < start) {
                // crossing midnight
                return (TimeSpan.FromDays(1) - start) + end;
            }
            return end - start;
        }
    }

    public override string ToString() {
        // use an invariant sortable format so it's easy to parse later
        return $"{User.Name}|{StartTime}|{EndTime}|{RecordedAt:o}";
    }
}

public enum GoalType {
    Daily,
    Weekly,
    Monthly
}

public enum GoalTargetType {
    Count, // for push-ups
    Duration // for jogging
}

public class Goal {
    public User Creator { get; }
    public Activity Activity { get; }
    public GoalType Period { get; }
    public GoalTargetType TargetType { get; }
    public int TargetCount { get; } // for Count type
    public TimeSpan TargetDuration { get; } // for Duration type
    public DateTime CreatedAt { get; }
    public string Description { get; }
    public bool IsShared { get; }
    public List<User> Participants { get; }

    public Goal(User creator, Activity activity, GoalType period, GoalTargetType targetType, int targetCount, TimeSpan targetDuration, string description, bool isShared = false) {
        this.Creator = creator;
        this.Activity = activity;
        this.Period = period;
        this.TargetType = targetType;
        this.TargetCount = targetCount;
        this.TargetDuration = targetDuration;
        this.CreatedAt = DateTime.Now;
        this.Description = description;
        this.IsShared = isShared;
        this.Participants = new List<User> { creator };
    }

    public void AddParticipant(User user) {
        if (!Participants.Contains(user)) {
            Participants.Add(user);
        }
    }

    public override string ToString() {
        string participantsStr = string.Join(",", Participants.Select(u => u.Name));
        return $"{Creator.Name}|{Activity.Name}|{Period}|{TargetType}|{TargetCount}|{TargetDuration.TotalMinutes}|{CreatedAt:o}|{Description}|{IsShared}|{participantsStr}";
    }
}
