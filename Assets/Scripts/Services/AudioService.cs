using DG.Tweening;
using Solitaire.Models;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Services
{
    public class AudioService
    {
        Dictionary<string, AudioClip> _audioMap;
        Audio.Config _audioConfig;
        Transform _camTransform;
        AudioSource _music;
        Tween _tween;

        const float _fadeDuration = 2.0f;

        public AudioService(Audio.Config audioConfig)
        {
            _audioConfig = audioConfig;
            _audioMap = new Dictionary<string, AudioClip>(_audioConfig.AudioClips.Count);

            for (int i = 0; i < _audioConfig.AudioClips.Count; i++)
            {
                AudioClip clip = _audioConfig.AudioClips[i];
                _audioMap.Add(clip.name, clip);
            }
        }

        public void PlaySfx(string key, float volume)
        {
            // Try to get clip based on key
            if (!_audioMap.TryGetValue(key, out AudioClip clip))
            {
                Debug.LogWarning($"Couldn't find '{key}' AudioClip.");
                return;
            }

            // Cache camera transform
            if (_camTransform == null)
            {
                _camTransform = Camera.main.transform;
            }

            // This is just a quick and dirty solution. It is very bad for performance
            // because this call creates and destroys an AudioSource each time.
            // A more professional way would be to use Audio Mixer, buses and pooling.
            AudioSource.PlayClipAtPoint(clip, _camTransform.position, volume);
        }

        public void PlayMusic(string key)
        {
            // Try to get clip based on key
            if (!_audioMap.TryGetValue(key, out AudioClip clip))
            {
                Debug.LogWarning($"Couldn't find '{key}' AudioClip.");
                return;
            }

            // Create audio source if for the first time
            if (_music == null)
            {
                var go = new GameObject("Music");
                _music = go.AddComponent<AudioSource>();
                _music.spatialBlend = 0;
                _music.volume = 0;
                _music.loop = true;
            }

            // Fade in music
            _tween?.Kill();
            _tween = _music.DOFade(_audioConfig.MusicVolume, _fadeDuration)
                .SetEase(Ease.InQuad)
                .OnStart(() =>
                {
                    _music.clip = clip;
                    _music.volume = 0f;
                    _music.Play();
                })
                .OnComplete(() =>
                {
                    _music.volume = _audioConfig.MusicVolume;
                });
        }

        public void StopMusic()
        {
            // Handle error
            if (_music == null)
            {
                return;
            }

            // Fade out music
            _tween?.Kill();
            _tween = _music.DOFade(0f, _fadeDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _music.volume = 0f;
                    _music.Stop();
                });
        }
    }
}
