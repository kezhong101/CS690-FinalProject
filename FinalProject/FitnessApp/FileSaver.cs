namespace FitnessApp;
using System.IO;
public class FileSaver
{
    string fileName;

    public FileSaver(string fileName)
    {
        this.fileName = fileName;

        if (!File.Exists(this.fileName)) {
            File.Create(this.fileName).Close();
        }     
    }

    // overload for jog data so the same FileSaver can be reused
    public void AppendData(JogData data) {
        File.AppendAllText(this.fileName, data.User.Name + "|" + data.StartTime + "|" + data.EndTime + "|" + data.RecordedAt.ToString("o") + Environment.NewLine);
    }

    // overload for push-up data
    public void AppendData(PushUpData data) {
        File.AppendAllText(this.fileName, data.User.Name + "|" + data.Count + "|" + data.RecordedAt.ToString("o") + Environment.NewLine);
    }

    // overload for goal data
    public void AppendData(Goal data) {
        File.AppendAllText(this.fileName, data.ToString() + Environment.NewLine);
    }
}