using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Call RandomizeMoveKeys() from anywhere to shuffle the 4 WASD-slot keys
/// among the Move composite's up/down/left/right bindings.
/// </summary>
public static class MoveKeyRandomizer
{
    // Binding indices for the Move composite, per your .inputactions asset
    private const int UpIndex = 1;
    private const int DownIndex = 2;
    private const int LeftIndex = 3;
    private const int RightIndex = 4;

    /// <summary>
    /// Raised after keys are shuffled and saved, so UI buttons can refresh their labels.
    /// </summary>
    public static event Action OnBindingsChanged;

    public static void RandomizeMoveKeys(InputActionReference moveActionReference)
    {
        InputAction moveAction = moveActionReference.action;

        int[] indices = { UpIndex, DownIndex, LeftIndex, RightIndex };
        string[] paths = new string[indices.Length];

        for (int i = 0; i < indices.Length; i++)
        {
            paths[i] = moveAction.bindings[indices[i]].effectivePath;
        }

        ShuffleInPlace(paths);

        for (int i = 0; i < indices.Length; i++)
        {
            moveAction.ApplyBindingOverride(indices[i], paths[i]);
        }

        SaveOverrides(moveActionReference);
        OnBindingsChanged?.Invoke();
    }

    private static void ShuffleInPlace(string[] array)
    {
        System.Random rng = new System.Random();

        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    private static void SaveOverrides(InputActionReference actionReference)
    {
        string json = actionReference.asset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", json);
        PlayerPrefs.Save();
    }
}