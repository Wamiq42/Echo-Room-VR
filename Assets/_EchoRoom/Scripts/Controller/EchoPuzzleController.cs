using UnityEngine;
using System.Collections.Generic;

public class EchoPuzzleController : PuzzleBase
{
    private List<IPuzzleElement> _targets = new List<IPuzzleElement>();
    private HashSet<IPuzzleElement> _countedTargets = new HashSet<IPuzzleElement>();
    private int _hitCount = 0;

    void Awake()
    {
        _targets.AddRange(GetComponentsInChildren<IPuzzleElement>());
        Log($"Found {_targets.Count} targets.");
    }
    private void OnEnable()
    {
        EchoButtonInteractable.OnAnyButtonPressed += HandleButtonPressed;
    }

    private void OnDisable()
    {
        EchoButtonInteractable.OnAnyButtonPressed -= HandleButtonPressed;
    }

    /// <summary>
    /// Called by an EchoTarget when it is hit by a ping.
    /// </summary>
    public void RegisterHit(IPuzzleElement target)
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
    
    private void HandleButtonPressed(EchoButtonInteractable button)
    {
        if (IsSolved) return;
        if (!button.transform.IsChildOf(transform)) return; // filter

        RegisterHit(button); // this is still IPuzzleElement
    }
    public override void ResetPuzzle()
    {
        IsSolved = false;
        _hitCount = 0;
        _countedTargets.Clear();
        foreach (var t in _targets)
        {
            t.ResetElement();
        }
    }
}