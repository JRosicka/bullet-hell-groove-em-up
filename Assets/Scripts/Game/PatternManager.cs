using System;
using System.Collections;
using System.Collections.Generic;
using Rumia;
using UnityEngine;

/// <summary>
/// Keeps track of <see cref="Pattern"/> instances spawned from <see cref="RumiaController"/>s
/// </summary>
public class PatternManager : MonoBehaviour {
    private List<Pattern> PatternInstances = new List<Pattern>();

    public void RegisterPattern(Pattern newPattern) {
        PatternInstances.Add(newPattern);
    }

    public Pattern GetPatternOfType(Pattern pattern) {
        List<Pattern> matchedPatterns = PatternInstances.FindAll(e => e.GetType() == pattern.GetType());
        return matchedPatterns.Count switch {
            0 => null,
            1 => matchedPatterns[0],
            _ => throw new InvalidOperationException($"Tried to find a single pattern for type {pattern.GetType()}, "
                                                     + $"but more than one was found. I don't know what to do, help!!")
        };
    }
}
