namespace FitnessApp;

public class DataManager {

    FileSaver jogFileSaver;
    FileSaver pushUpFileSaver;
    private bool jogDataLoaded;
    private bool pushUpDataLoaded;


    public List<User> Users { get; }
    public List<MainNavItem> MainNavItems { get; }
    public List<Activity> Activities { get; }
    public List<JogData> JogData { get; private set; }
    public List<PushUpData> PushUpData { get; private set; }

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

    public DataManager() {

        jogFileSaver = new FileSaver("jog-data.txt");
        pushUpFileSaver = new FileSaver("pushup-data.txt");

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
}

