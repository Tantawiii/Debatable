using System.Collections;
using UnityEngine;

/// <summary>
/// End of Level 3. When the player reaches it, the narrator delivers the sign-off dialogue and
/// then the game returns to the main menu. One-shot.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class Level3Outro : MonoBehaviour
{
    [SerializeField] private NarrationLine[] lines;
    [Tooltip("Silence held after the last line before returning to the menu.")]
    [SerializeField] private float endPause = 1.2f;

    private bool _fired;

    private void Reset() => GetComponent<BoxCollider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (_fired || !PersistentPlayer.IsPlayerCollider(other)) return;
        _fired = true;
        GetComponent<Collider>().enabled = false;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        var narrator = NarrationDirector.Instance;
        if (narrator != null)
        {
            narrator.Speak(lines, priority: 1);
            yield return null;
            while (narrator.IsSpeaking) yield return null;
        }
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, endPause));

        GameProgress.FirstRunDone = true;   // a whole playthrough is done — no more scramble / first-run lines
        if (LevelStreamer.Instance != null) LevelStreamer.Instance.ReturnToMenu();
    }
}
