using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace OpenGta2.Client.Diagnostics;

public class PerformanceCounter
{
    private readonly Dictionary<string, int> _counters = new();
    private readonly Dictionary<string, TimeSpan> _measurements = new();
    private readonly Stack<(string, long)> _runningMeasurements = new(16);
    private readonly StringBuilder _stringBuilder = new();

    [Conditional("DEBUG")]
    public void Add(string key, int count = 1)
    {
        _counters.TryGetValue(key, out var value);
        _counters[key] = value + count;
    }

    [Conditional("DEBUG")]
    public void StartMeasurement(string name)
    {
        _runningMeasurements.Push((name, Stopwatch.GetTimestamp()));
    }

    [Conditional("DEBUG")]
    public void StopMeasurement()
    {
        var end = Stopwatch.GetTimestamp();

        var (name, start) = _runningMeasurements.Pop();

        _measurements.TryGetValue(name, out var value);
        var time = TimeSpan.FromTicks(end - start);

        _measurements[name] = value + time;
    }

    public string GetText()
    {
        AppendText(_stringBuilder);

        var result = _stringBuilder.ToString();
        _stringBuilder.Clear();
        return result;
    }

    public void AppendText(StringBuilder stringBuilder)
    {
        foreach (var (key, value) in _counters)
        {
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{key}: {value}");
        }

        stringBuilder.AppendLine();

        foreach (var (key, value) in _measurements)
        {
            stringBuilder.Append(key);
            stringBuilder.Append(": ");
            stringBuilder.AppendLine(value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
        }
    }

    [Conditional("DEBUG")]
    public void Reset()
    {
        foreach (var key in _counters.Keys)
        {
            _counters[key] = 0;
        }

        foreach (var key in _measurements.Keys)
        {
            _measurements[key] = TimeSpan.Zero;
        }
    }

}