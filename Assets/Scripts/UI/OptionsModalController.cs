using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

namespace MultiplayerTemplate.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class OptionsModalController : MonoBehaviour
    {
        [Tooltip("Referencia al AudioMixer del juego (GameAudioMixer). Si no se asigna, solo funcionará el Volumen Maestro.")]
        [SerializeField] private AudioMixer audioMixer;

        // Nombres exactos de los parámetros expuestos en el AudioMixer
        private const string PARAM_MASTER = "MasterVolume";
        private const string PARAM_MUSIC  = "MusicVolume";
        private const string PARAM_SFX    = "SFXVolume";

        private VisualElement modalOverlay;
        private Slider    sldMasterVolume;
        private Slider    sldMusicVolume;
        private Slider    sldSFXVolume;
        private Toggle    tglFullScreen;
        private DropdownField drpQuality;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            modalOverlay = root.Q<VisualElement>("ModalOverlay");

            var btnCloseOptions = root.Q<Button>("BtnCloseOptions");
            btnCloseOptions?.RegisterCallback<ClickEvent>(ev => HideModal());

            sldMasterVolume = root.Q<Slider>("SldMasterVolume");
            sldMusicVolume  = root.Q<Slider>("SldMusicVolume");
            sldSFXVolume    = root.Q<Slider>("SldSFXVolume");
            tglFullScreen   = root.Q<Toggle>("TglFullScreen");
            drpQuality      = root.Q<DropdownField>("DrpQuality");

            if (drpQuality != null)
            {
                drpQuality.choices = new System.Collections.Generic.List<string> { "Bajo", "Medio", "Alto" };
            }

            if (modalOverlay != null && !modalOverlay.ClassListContains("hidden"))
                modalOverlay.AddToClassList("hidden");

            // Cargar estado inicial desde PlayerPrefs hacia los sliders
            LoadSettings();

            // Registrar eventos para aplicar cambios en tiempo real
            sldMasterVolume?.RegisterValueChangedCallback(ev => ApplyRealTimeAudio());
            sldMusicVolume?.RegisterValueChangedCallback(ev => ApplyRealTimeAudio());
            sldSFXVolume?.RegisterValueChangedCallback(ev => ApplyRealTimeAudio());
            tglFullScreen?.RegisterValueChangedCallback(ev => Screen.fullScreen = ev.newValue);
            drpQuality?.RegisterValueChangedCallback(ev => { 
                if (drpQuality.index >= 0) QualitySettings.SetQualityLevel(drpQuality.index); 
            });

            // Aplicar configuraciones al motor (Audio, Gráficos)
            ApplySettings();
        }

        public void ShowModal()
        {
            LoadSettings();
            modalOverlay?.RemoveFromClassList("hidden");
        }

        public void HideModal()
        {
            modalOverlay?.AddToClassList("hidden");
            SaveSettings();
        }

        private void LoadSettings()
        {
            if (sldMasterVolume != null) sldMasterVolume.value = PlayerPrefs.GetFloat("MasterVol", 80f);
            if (sldMusicVolume  != null) sldMusicVolume.value  = PlayerPrefs.GetFloat("MusicVol",  80f);
            if (sldSFXVolume    != null) sldSFXVolume.value    = PlayerPrefs.GetFloat("SFXVol",    80f);
            if (tglFullScreen   != null) tglFullScreen.value   = PlayerPrefs.GetInt("FullScreen", 1) == 1;
            if (drpQuality      != null) 
            {
                int qLevel = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
                if (qLevel >= 0 && qLevel < drpQuality.choices.Count)
                    drpQuality.index = qLevel;
            }
        }

        private void SaveSettings()
        {
            if (sldMasterVolume != null) PlayerPrefs.SetFloat("MasterVol", sldMasterVolume.value);
            if (sldMusicVolume  != null) PlayerPrefs.SetFloat("MusicVol",  sldMusicVolume.value);
            if (sldSFXVolume    != null) PlayerPrefs.SetFloat("SFXVol",    sldSFXVolume.value);
            if (tglFullScreen   != null) PlayerPrefs.SetInt("FullScreen",  tglFullScreen.value ? 1 : 0);
            if (drpQuality      != null && drpQuality.index >= 0) PlayerPrefs.SetInt("Quality", drpQuality.index);

            PlayerPrefs.Save();
            ApplySettings();
        }

        /// <summary>
        /// Aplica los valores guardados en PlayerPrefs a los sistemas de Unity en runtime.
        ///
        /// AUDIO:
        ///   Si hay un AudioMixer asignado, los volúmenes se envían a sus grupos expuestos
        ///   usando la conversión lineal → dB: dB = log10(linear) * 20
        ///   El silencio absoluto requiere -80 dB (no 0 dB, que equivale al volumen máximo).
        ///   Sin AudioMixer, solo el Volumen Maestro funciona vía AudioListener.volume (escala 0-1).
        ///
        /// GRÁFICOS:
        ///   El índice del slider mapea directamente a los Quality Levels de Project Settings.
        ///   Asegúrate de tener exactamente 3 niveles: Bajo(0), Medio(1), Alto(2).
        /// </summary>
        private void ApplySettings()
        {
            ApplyRealTimeAudio();

            if (tglFullScreen != null)
                Screen.fullScreen = tglFullScreen.value;

            if (drpQuality != null && drpQuality.index >= 0)
                QualitySettings.SetQualityLevel(drpQuality.index);
        }

        /// <summary>
        /// Lee los valores actuales de los sliders (o PlayerPrefs si los sliders no están listos)
        /// y los aplica directamente al AudioMixer / AudioListener en tiempo real.
        /// </summary>
        private void ApplyRealTimeAudio()
        {
            float masterLinear = (sldMasterVolume != null ? sldMasterVolume.value : PlayerPrefs.GetFloat("MasterVol", 80f)) / 100f;
            float musicLinear  = (sldMusicVolume  != null ? sldMusicVolume.value  : PlayerPrefs.GetFloat("MusicVol",  80f)) / 100f;
            float sfxLinear    = (sldSFXVolume    != null ? sldSFXVolume.value    : PlayerPrefs.GetFloat("SFXVol",    80f)) / 100f;

            if (audioMixer != null)
            {
                audioMixer.SetFloat(PARAM_MASTER, LinearToDB(masterLinear));
                audioMixer.SetFloat(PARAM_MUSIC,  LinearToDB(musicLinear));
                audioMixer.SetFloat(PARAM_SFX,    LinearToDB(sfxLinear));
            }
            else
            {
                AudioListener.volume = masterLinear;
            }
        }

        /// <summary>
        /// Convierte un valor lineal (0.0 a 1.0) a decibelios (-80 dB a 0 dB).
        /// Usado para enviar valores al AudioMixer, que trabaja en escala logarítmica.
        /// </summary>
        private static float LinearToDB(float linear)
        {
            return Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
        }
    }
}
