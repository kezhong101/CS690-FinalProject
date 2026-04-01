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

    public Dictionary<DateTime, List<StrengthTrainingData>> GetStrengthTrainingHistoryByDate(User user, IEnumerable<StrengthTrainingData> strengthData)
    {
        return strengthData
            .Where(s => s.User.Name == user.Name)
            .GroupBy(s => s.RecordedAt.Date)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public Dictionary<DateTime, (TimeSpan TotalJogDuration, int TotalPushUps, TimeSpan TotalStrengthDuration)> GetWorkoutSummaryByDate(User user, IEnumerable<JogData> jogData, IEnumerable<PushUpData> pushUpData, IEnumerable<StrengthTrainingData> strengthData)
    {
        var jogHistory = GetJogHistoryByDate(user, jogData);
        var pushUpHistory = GetPushUpHistoryByDate(user, pushUpData);
        var strengthHistory = GetStrengthTrainingHistoryByDate(user, strengthData);

        var allDates = jogHistory.Keys.Union(pushUpHistory.Keys).Union(strengthHistory.Keys).OrderByDescending(d => d);

        return allDates.ToDictionary(
            date => date,
            date => (
                TotalJogDuration: jogHistory.ContainsKey(date)
                    ? TimeSpan.FromTicks(jogHistory[date].Sum(j => j.Duration.Ticks))
                    : TimeSpan.Zero,
                TotalPushUps: pushUpHistory.ContainsKey(date)
                    ? pushUpHistory[date].Sum(p => p.Count)
                    : 0,
                TotalStrengthDuration: strengthHistory.ContainsKey(date)
                    ? TimeSpan.FromTicks(strengthHistory[date].Sum(s => s.Duration.Ticks))
                    : TimeSpan.Zero
            )
        );
    }
}

