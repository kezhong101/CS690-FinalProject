namespace FitnessApp;

using System;
using System.Collections.Generic;
using System.Linq;

public class Reporter
{
    public Dictionary<DateTime, List<JogData>> GetJogHistoryByDate(User user, IEnumerable<JogData> jogData)
    {
        return jogData
            .Where(j => j.User.Name == user.Name)
            .GroupBy(j => j.RecordedAt.Date)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public Dictionary<DateTime, List<PushUpData>> GetPushUpHistoryByDate(User user, IEnumerable<PushUpData> pushUpData)
    {
        return pushUpData
            .Where(p => p.User.Name == user.Name)
            .GroupBy(p => p.RecordedAt.Date)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public Dictionary<DateTime, (TimeSpan TotalJogDuration, int TotalPushUps)> GetWorkoutSummaryByDate(User user, IEnumerable<JogData> jogData, IEnumerable<PushUpData> pushUpData)
    {
        var jogHistory = GetJogHistoryByDate(user, jogData);
        var pushUpHistory = GetPushUpHistoryByDate(user, pushUpData);

        var allDates = jogHistory.Keys.Union(pushUpHistory.Keys).OrderByDescending(d => d);

        return allDates.ToDictionary(
            date => date,
            date => (
                TotalJogDuration: jogHistory.ContainsKey(date)
                    ? TimeSpan.FromTicks(jogHistory[date].Sum(j => j.Duration.Ticks))
                    : TimeSpan.Zero,
                TotalPushUps: pushUpHistory.ContainsKey(date)
                    ? pushUpHistory[date].Sum(p => p.Count)
                    : 0
            )
        );
    }
}

