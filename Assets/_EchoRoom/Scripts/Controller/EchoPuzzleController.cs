using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EchoPuzzleController : PuzzleBase
{
    private List<IPuzzleElement> _targets = new List<IPuzzleElement>();
    private HashSet<IPuzzleElement> _countedTargets = new HashSet<IPuzzleElement>();
    private int _hitCount = 0;

    /// <summary>How many levers/buttons this puzzle needs in total.</summary>
    public int TargetCount => _targets.Count;

    /// <summary>
    /// How many are currently active. Counted on demand rather than read from
    /// <see cref="_hitCount"/> so a listener on the same lever event cannot observe a
    /// stale value just because it ran before this puzzle's own handler.
    /// </summary>
    public int ActivatedCount
    {
        get
        {
            int count = 0;
            foreach (IPuzzleElement target in _targets)
            {
                if (target is LeverInteractable lever && lever.IsOn) count++;
                else if (target is EchoButtonInteractable button && button.IsOn) count++;
            }

            return count;
        }
    }

    void Awake()
    {
        RefreshTargets();
    }

    private void Update()
    {
        if (IsSolved) return;

        RefreshTargets();
        if (!_targets.Any(target => target is LeverInteractable || target is EchoButtonInteractable))
            return;

        RecountActiveTargets();
        if (_targets.Count > 0 && _hitCount >= _targets.Count)
            MarkSolved();
    }

    private void OnEnable()
    {
        EchoButtonInteractable.OnAnyButtonPressed += HandleButtonPressed;
        LeverInteractable.OnAnyLeverStateChanged += HandleLeverStateChanged;
    }

    private void OnDisable()
    {
        EchoButtonInteractable.OnAnyButtonPressed -= HandleButtonPressed;
        LeverInteractable.OnAnyLeverStateChanged -= HandleLeverStateChanged;
    }

    /// <summary>
    /// Called by an EchoTarget when it is hit by a ping.
    /// </summary>
    public void RegisterHit(IPuzzleElement target)
    {
        if (IsSolved) return;
        if (!_targets.Contains(target)) return;
        if (_countedTargets.Contains(target)) return; // prevents double counting

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

        RefreshTargets();
        RegisterHit(button); // this is still IPuzzleElement
    }

    private void HandleLeverStateChanged(LeverInteractable lever)
    {
        if (IsSolved) return;
        if (!lever.transform.IsChildOf(transform)) return;
        RefreshTargets();
        if (!_targets.Contains(lever)) return;

        RecountActiveTargets();
        Log($"Progress: {_hitCount}/{_targets.Count}");

        if (_targets.Count > 0 && _hitCount >= _targets.Count)
        {
            MarkSolved();
        }
    }

    public override void ResetPuzzle()
    {
        RefreshTargets();
        IsSolved = false;
        _hitCount = 0;
        _countedTargets.Clear();
        foreach (var t in _targets)
        {
            t.ResetElement();
        }
    }

    private void RefreshTargets()
    {
        List<IPuzzleElement> foundTargets = new List<IPuzzleElement>();
        foreach (var behaviour in GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour is IPuzzleElement target)
                foundTargets.Add(target);
        }

        List<IPuzzleElement> buttonTargets = foundTargets
            .Where(target => target is EchoButtonInteractable)
            .ToList();

        _targets = buttonTargets.Count > 0
            ? buttonTargets
            : foundTargets;

        Log($"Found {_targets.Count} targets.");
    }

    private void RecountActiveTargets()
    {
        _countedTargets.Clear();

        foreach (IPuzzleElement target in _targets)
        {
            if (target is LeverInteractable lever && lever.IsOn)
                _countedTargets.Add(target);
            else if (target is EchoButtonInteractable button && button.IsOn)
                _countedTargets.Add(target);
        }

        _hitCount = _countedTargets.Count;
    }
}

