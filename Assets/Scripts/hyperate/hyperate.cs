using System;
using System.Collections.Generic;
using UnityEngine;

class AverageHeartbeatStatistic
{
    public ulong sum;
    public ulong amount;

    public AverageHeartbeatStatistic(ulong sum, ulong amount)
    {
        this.sum = sum;
        this.amount = amount;
    }

    public double CalculateAverage()
    {
        return Mathf.RoundToInt((float)this.sum / this.amount);
    }

    public void AddHeartbeat(int heartbeat)
    {
        this.sum += (ulong)heartbeat;
        this.amount += (ulong)1;
    }
}

class HeartbeatStatistics
{
    private Dictionary<string, int> min_heartbeats = new Dictionary<string, int>();
    private Dictionary<string, int> max_heartbeats = new Dictionary<string, int>();
    private Dictionary<string, AverageHeartbeatStatistic> avg_heartbeats = new Dictionary<string, AverageHeartbeatStatistic>();

    public int GetMinHeartbeatForId(string id)
    {
        if (this.min_heartbeats.ContainsKey(id) == false)
        {
            return 80;
        }

        return this.min_heartbeats[id];
    }

    public int GetMaxHeartbeatForId(string id)
    {
        if (this.max_heartbeats.ContainsKey(id) == false)
        {
            return -1;
        }

        return this.max_heartbeats[id];
    }

    public double GetAverageHeartbeatForId(string id)
    {
        if (this.avg_heartbeats.ContainsKey(id) == false)
        {
            return -1;
        }

        var avgHeartbeatStatistics = this.avg_heartbeats[id];

        return avgHeartbeatStatistics.CalculateAverage();
    }

    public void UpdateHeartbeat(string id, int heartbeat)
    {
        var newMin = heartbeat;
        var newMax = heartbeat;
        var newAvgStatistics = new AverageHeartbeatStatistic((ulong)0, (ulong)0);

        if (this.min_heartbeats.ContainsKey(id))
        {
            newMin = Math.Min(this.min_heartbeats[id], heartbeat);
        }

        if (this.max_heartbeats.ContainsKey(id))
        {
            newMax = Math.Max(this.max_heartbeats[id], heartbeat);
        }

        if (this.avg_heartbeats.ContainsKey(id))
        {
            newAvgStatistics = this.avg_heartbeats[id];
        }

        newAvgStatistics.AddHeartbeat(heartbeat);

        this.min_heartbeats[id] = newMin;
        this.max_heartbeats[id] = newMax;
        this.avg_heartbeats[id] = newAvgStatistics;
    }
}

public class HelloWorld
{
    public static void Main(string[] args)
    {
        var id = "player";
        var test = new HeartbeatStatistics();

        test.UpdateHeartbeat(id, 80);
        test.UpdateHeartbeat(id, 90);

        Console.WriteLine("Min: {0}"+ test.GetMinHeartbeatForId(id));
        Console.WriteLine("Max: {0}", test.GetMaxHeartbeatForId(id));
        Console.WriteLine("Avg: {0}", test.GetAverageHeartbeatForId(id));

        var otherId = "my-id";
        Console.WriteLine("Min: {0}", test.GetMinHeartbeatForId(otherId));
        Console.WriteLine("Max: {0}", test.GetMaxHeartbeatForId(otherId));
        Console.WriteLine("Avg: {0}", test.GetAverageHeartbeatForId(otherId));
    }
}