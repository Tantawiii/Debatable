using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds a UI <see cref="Slider"/> (0..1) to an <see cref="AudioBuses"/> channel. Self-wires in
/// <c>OnEnable</c> — no inspector hookup needed. Put on the Master / Music slider objects.
/// </summary>
[RequireComponent(typeof(Slider))]
public class MixerVolumeSlider : MonoBehaviour
{
    public enum Bus { Master, Music }

    [SerializeField] private Bus bus = Bus.Master;

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.minValue = 0f;
        _slider.maxValue = 1f;
        _slider.wholeNumbers = false;
    }

    private void OnEnable()
    {
        _slider.SetValueWithoutNotify(bus == Bus.Master ? AudioBuses.Master : AudioBuses.Music);
        _slider.onValueChanged.AddListener(OnChanged);
    }

    private void OnDisable() => _slider.onValueChanged.RemoveListener(OnChanged);

    private void OnChanged(float v)
    {
        if (bus == Bus.Master) AudioBuses.SetMaster(v);
        else AudioBuses.SetMusic(v);
    }
}
