using System;

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
