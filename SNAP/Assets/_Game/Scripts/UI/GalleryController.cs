using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GPOyun.Newspaper;

namespace GPOyun.UI
{
    /// <summary>
    /// Decoupled controller that manages user focus and selection layouts inside the Photo Gallery UI.
    /// </summary>
    public class GalleryController : MonoBehaviour
    {
        public static GalleryController Instance { get; private set; }

        private PhotoData _selectedPhoto;
        private readonly List<PhotoData> _loadedPhotos = new();

        public PhotoData SelectedPhoto => _selectedPhoto;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void PopulatePhotos(List<PhotoData> photos)
        {
            _loadedPhotos.Clear();
            _loadedPhotos.AddRange(photos);
            
            if (_loadedPhotos.Count > 0)
            {
                SelectPhoto(_loadedPhotos[0]);
            }
            else
            {
                _selectedPhoto = null;
            }
        }

        public void SelectPhoto(PhotoData photo)
        {
            _selectedPhoto = photo;
            Debug.Log($"[GalleryController] Selection shifted to: {(photo != null && photo.PrimarySubject != null ? photo.PrimarySubject.SubjectName : "Scenic")}");
        }

        public PhotoData GetNextPhoto(bool forward)
        {
            if (_loadedPhotos.Count == 0) return null;
            if (_selectedPhoto == null) return _loadedPhotos[0];

            int currentIndex = _loadedPhotos.IndexOf(_selectedPhoto);
            int nextIndex = currentIndex + (forward ? 1 : -1);

            if (nextIndex >= _loadedPhotos.Count) nextIndex = 0;
            if (nextIndex < 0) nextIndex = _loadedPhotos.Count - 1;

            return _loadedPhotos[nextIndex];
        }
    }
}
