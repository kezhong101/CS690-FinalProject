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
}
