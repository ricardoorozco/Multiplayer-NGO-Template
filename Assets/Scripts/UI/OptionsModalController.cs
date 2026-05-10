using UnityEngine;
using UnityEngine.UIElements;

namespace MultiplayerTemplate.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class OptionsModalController : MonoBehaviour
    {
        private VisualElement modalOverlay;
        
        private Slider sldMasterVolume;
        private Slider sldMusicVolume;
        private Slider sldSFXVolume;
        private Toggle tglFullScreen;
        private SliderInt sldQuality;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            modalOverlay = root.Q<VisualElement>("ModalOverlay");

            var btnCloseOptions = root.Q<Button>("BtnCloseOptions");
            btnCloseOptions?.RegisterCallback<ClickEvent>(ev => HideModal());

            // Bind elements
            sldMasterVolume = root.Q<Slider>("SldMasterVolume");
            sldMusicVolume = root.Q<Slider>("SldMusicVolume");
            sldSFXVolume = root.Q<Slider>("SldSFXVolume");
            tglFullScreen = root.Q<Toggle>("TglFullScreen");
            sldQuality = root.Q<SliderInt>("SldQuality");

            // Si el overlay no está oculto por defecto, lo ocultamos
            if (modalOverlay != null && !modalOverlay.ClassListContains("hidden"))
            {
                modalOverlay.AddToClassList("hidden");
            }

            LoadSettings();
            ApplySettings(); // Aplica las configuraciones guardadas al arrancar
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
            if (sldMusicVolume != null) sldMusicVolume.value = PlayerPrefs.GetFloat("MusicVol", 80f);
            if (sldSFXVolume != null) sldSFXVolume.value = PlayerPrefs.GetFloat("SFXVol", 80f);
            
            if (tglFullScreen != null) tglFullScreen.value = PlayerPrefs.GetInt("FullScreen", 1) == 1;
            if (sldQuality != null) sldQuality.value = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        }

        private void SaveSettings()
        {
            if (sldMasterVolume != null) PlayerPrefs.SetFloat("MasterVol", sldMasterVolume.value);
            if (sldMusicVolume != null) PlayerPrefs.SetFloat("MusicVol", sldMusicVolume.value);
            if (sldSFXVolume != null) PlayerPrefs.SetFloat("SFXVol", sldSFXVolume.value);
            
            if (tglFullScreen != null) PlayerPrefs.SetInt("FullScreen", tglFullScreen.value ? 1 : 0);
            if (sldQuality != null) PlayerPrefs.SetInt("Quality", sldQuality.value);
            
            PlayerPrefs.Save();
            ApplySettings();
        }

        /// <summary>
        /// Aplica los valores de configuración a los sistemas de Unity en runtime.
        /// Los sliders van de 0 a 100; AudioListener.volume espera un valor de 0 a 1.
        /// </summary>
        private void ApplySettings()
        {
            // AudioListener.volume controla el volumen global del juego
            AudioListener.volume = PlayerPrefs.GetFloat("MasterVol", 80f) / 100f;

            if (tglFullScreen != null)
                Screen.fullScreen = tglFullScreen.value;

            if (sldQuality != null)
                QualitySettings.SetQualityLevel(sldQuality.value);
        }
    }
}
