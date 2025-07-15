using UnityEngine;
using System.Collections.Generic;

public class EchoPuzzleController : PuzzleBase
{
    private List<EchoTarget> _targets = new List<EchoTarget>();
    private HashSet<EchoTarget> _countedTargets = new HashSet<EchoTarget>();
    private int _hitCount = 0;

    void Awake()
    {
        _targets.AddRange(GetComponentsInChildren<EchoTarget>());
        Log($"Found {_targets.Count} targets.");
    }

    /// <summary>
    /// Called by an EchoTarget when it is hit by a ping.
    /// </summary>
    public void RegisterHit(EchoTarget target)
    {
        if (IsSolved) return;
        if (!_targets.Contains(target)) return;
        if (_countedTargets.Contains(target)) return; // ✅ prevents double counting

        _countedTargets.Add(target);
        _hitCount++;

        Log($"Progress: {_hitCount}/{_targets.Count}");

        if (_hitCount >= _targets.Count)
        {
            MarkSolved();
        }
    }

    public override void ResetPuzzle()
    {
        IsSolved = false;
        _hitCount = 0;
        _countedTargets.Clear();
        foreach (var t in _targets)
        {
            t.ResetTarget();
        }
    }
}