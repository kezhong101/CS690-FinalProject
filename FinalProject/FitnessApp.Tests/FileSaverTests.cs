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
}
