using UnityEngine;
using GPOyun.Newspaper;
using GPOyun.UI;

namespace GPOyun.UseCases
{
    /// <summary>
    /// Encapsulates the Use Case of selecting and focusing on a picture in the player's photo portfolio.
    /// </summary>
    public class SelectPictureUseCase : MonoBehaviour
    {
        public static SelectPictureUseCase Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Execute(PhotoData photo)
        {
            if (photo == null)
            {
                Debug.LogWarning("[SelectPictureUseCase] Cannot select a null photo entry!");
                return;
            }

            if (GalleryController.Instance != null)
            {
                GalleryController.Instance.SelectPhoto(photo);
            }

            // Sync visual zoom view in the PhotoGalleryUI panel
            if (PhotoGalleryUI.Instance != null)
            {
                PhotoGalleryUI.Instance.SelectPhoto(photo);
            }
        }
    }
}
