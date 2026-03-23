using System.IO;
using System.Linq;

namespace FitnessApp.Tests;

public class FileSaverTests
{
    [Fact]
    public void PushUpData_MultipleEntriesSameDay_AccumulatesCount()
    {
        // Arrange: keep test isolated from previous runs
        var pushupFile = "pushup-data.txt";
        if (File.Exists(pushupFile)) {
            File.Delete(pushupFile);
        }

        var dataManager = new DataManager();
        var user = new User("Ethan Smith");

        // Act
        dataManager.AddNewPushUpData(new PushUpData(user, 10, DateTime.Now));
        dataManager.AddNewPushUpData(new PushUpData(user, 20, DateTime.Now));

        var entries = dataManager.GetAllPushUpData().Where(x => x.User.Name == user.Name && x.RecordedAt.Date == DateTime.Now.Date).ToList();
        var total = entries.Sum(x => x.Count);

        // Assert
        Assert.Equal(2, entries.Count);
        Assert.Equal(30, total);
    }


    [Fact]
    public void JogData_DurationIsCalculatedFromStartAndEndTimes_HHMMFormat()
    {
        // Arrange
        var user = new User("Jane Doe");

        // Act
        var jog = new JogData(user, "0720", "0910", DateTime.Now);

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(110), jog.Duration);
    }

    [Fact]
    public void JogData_DurationIsCalculatedFromStartAndEndTimes_HHMMFormat_20xx()
    {
        // Arrange
        var user = new User("Jane Doe");

        // Act
        var jog = new JogData(user, "2010", "2110", DateTime.Now);

        // Assert
        Assert.Equal(TimeSpan.FromHours(1), jog.Duration);
    }

    [Fact]
    public void JogData_TryParseTime_RecognizesSupportedFormats()
    {
        // Arrange
        var expected = new TimeSpan(7, 20, 0);

        // Act
        var ok1 = JogData.TryParseTime("07:20", out var p1);
        var ok2 = JogData.TryParseTime("7:20", out var p2);
        var ok3 = JogData.TryParseTime("0720", out var p3);
        var ok4 = JogData.TryParseTime("720", out var p4);

        // Assert
        Assert.True(ok1);
        Assert.Equal(expected, p1);

        Assert.True(ok2);
        Assert.Equal(expected, p2);

        Assert.True(ok3);
        Assert.Equal(expected, p3);

        Assert.True(ok4);
        Assert.Equal(expected, p4);
    }

    [Fact]
    public void JogData_TryParseTime_RejectsInvalidInput()
    {
        // Act
        var invalid1 = JogData.TryParseTime("24:00", out _);
        var invalid2 = JogData.TryParseTime("1260", out _);
        var invalid3 = JogData.TryParseTime("abc", out _);
        var invalid4 = JogData.TryParseTime("", out _);

        // Assert
        Assert.False(invalid1);
        Assert.False(invalid2);
        Assert.False(invalid3);
        Assert.False(invalid4);
    }

    [Fact]
    public void DataManager_AddNewPushUpData_PersistsToFileAndReloads()
    {
        // Arrange: isolate test data
        var pushupFile = "pushup-data.txt";
        if (File.Exists(pushupFile)) {
            File.Delete(pushupFile);
        }

        var dataManager = new DataManager();
        var user = new User("Ethan Smith");

        // Act
        dataManager.AddNewPushUpData(new PushUpData(user, 15, DateTime.Now));

        // Create new instance to force reload from disk
        var dataManagerReloaded = new DataManager();
        var loaded = dataManagerReloaded.GetAllPushUpData().FirstOrDefault(x => x.User.Name == user.Name);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(15, loaded!.Count);
    }

    [Fact]
    public void DataManager_AddNewJogData_PersistsToFileAndReloads()
    {
        // Arrange: isolate test data
        var jogFile = "jog-data.txt";
        if (File.Exists(jogFile)) {
            File.Delete(jogFile);
        }

        var dataManager = new DataManager();
        var user = new User("Jane Doe");
        var startTime = "0700";
        var endTime = "0730";

        // Act
        dataManager.AddNewJogData(new JogData(user, startTime, endTime, DateTime.Now));

        // Create new instance to force reload from disk
        var dataManagerReloaded = new DataManager();
        var loaded = dataManagerReloaded.GetAllJogData().FirstOrDefault(x => x.User.Name == user.Name);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(startTime, loaded!.StartTime);
        Assert.Equal(endTime, loaded.EndTime);
    }
}
