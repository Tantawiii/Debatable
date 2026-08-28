using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to each keybind Button. Shows the current bound key and lets the
/// player click to rebind it. For composite parts (Move Up/Down/Left/Right),
/// set bindingIndex to that part's index in the action's bindings list;
/// otherwise leave it at -1 (auto-resolved for simple actions).
/// </summary>
public class RebindButtonUI : MonoBehaviour
{
    [Header("Binding target")]
    [SerializeField] private InputActionReference actionReference;
    [SerializeField] private int bindingIndex = -1; 

    [Header("UI")]
    [SerializeField] private TMP_Text bindingText;
    [SerializeField] private Button rebindButton;

    private InputActionRebindingExtensions.RebindingOperation rebindOperation;
    private int resolvedBindingIndex;
    private string originalPath;


    private void Awake()
    {
        rebindButton.onClick.AddListener(StartRebind);
    }

    private void OnEnable()
    {
        LoadSavedOverrides();
        ResolveBindingIndex();
        UpdateBindingText();
    }
        
    private void LoadSavedOverrides()
    {
        if (PlayerPrefs.HasKey("rebinds"))
        {
            string json = PlayerPrefs.GetString("rebinds");
            actionReference.asset.LoadBindingOverridesFromJson(json);
        }
    }

    private void OnDestroy()
    {
        rebindOperation?.Dispose();
    }

    private void ResolveBindingIndex()
    {
        if (bindingIndex >= 0)
        {
            resolvedBindingIndex = bindingIndex;
            return;
        }

        // Simple (non-composite) action — its first binding is index 0
        resolvedBindingIndex = 0;
    }

    private void UpdateBindingText()
    {
        InputAction action = actionReference.action;
        bindingText.text = action.GetBindingDisplayString(resolvedBindingIndex);
    }

    private void StartRebind()
    {
        InputAction action = actionReference.action;

        // Store the original path BEFORE rebinding, so we can revert if it's a duplicate
        originalPath = action.bindings[resolvedBindingIndex].effectivePath;

        action.Disable();

        bindingText.text = "Press any key...";
        rebindButton.interactable = false;

        rebindOperation = action.PerformInteractiveRebinding(resolvedBindingIndex)
            .WithControlsExcluding("Mouse/position")
            .WithControlsExcluding("Mouse/delta")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => RebindComplete())
            .OnCancel(operation => RebindComplete())
            .Start();
    }

    private void RebindComplete()
    {
        rebindOperation.Dispose();
        rebindOperation = null;

        InputAction action = actionReference.action;

        if (IsDuplicateBinding(action, resolvedBindingIndex))
        {
            // Revert to the original binding
            action.ApplyBindingOverride(resolvedBindingIndex, originalPath);
        }
        else
        {
            SaveBindingOverrides();
        }

        action.Enable();
        rebindButton.interactable = true;
        UpdateBindingText();
    }

    private bool IsDuplicateBinding(InputAction newAction, int newBindingIndex)
    {
        InputBinding newBinding = newAction.bindings[newBindingIndex];

        foreach (InputAction otherAction in newAction.actionMap.actions)
        {
            foreach (InputBinding binding in otherAction.bindings)
            {
                // Skip comparing the binding against itself
                if (binding.id == newBinding.id)
                    continue;

                // Skip composite "headers" (e.g. the "2D Vector" row itself), only compare actual bindable parts
                if (binding.isComposite)
                    continue;

                if (binding.effectivePath == newBinding.effectivePath)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void SaveBindingOverrides()
    {
        string json = actionReference.asset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", json);
        PlayerPrefs.Save();
    }
}