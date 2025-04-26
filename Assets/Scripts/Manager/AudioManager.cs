using System.Collections.Generic;
using System.Threading;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.DataController;
using JetBrains.Annotations;
using UnityEngine;

namespace ETD.Scripts.Manager
{
    public class AudioManager :  Singleton<AudioManager>
    {
        private static CancellationTokenSource _cts; 
        private Dictionary<AudioClipType, List<AudioSource>> _audioClips;

        [SerializeField] private float[] preAudioVolume;
        [SerializeField] private AudioClipType[] preAudioClipTypes;
        [SerializeField] private AudioClip[] preAudioClips;

        public void PlaySound(AudioClipType type, float delay = 0f, bool isLoop = false)
        {
            var index = (int)type;
            var audioSource = GetAudioSource(type);
            if (!audioSource) return;
            
#if UNITY_EDITOR
            return;
#endif
            
            audioSource.clip = preAudioClips[index];
            audioSource.volume = GetVolum(type);
            
            audioSource.PlayDelayed(delay);
            audioSource.loop = isLoop;
            audioSource.Play();
        }
        
        public override void Init(CancellationTokenSource cts)
        {
            _cts = cts;
            _audioClips ??= new Dictionary<AudioClipType, List<AudioSource>>();
            
            PlaySound(AudioClipType.BGM, 0, true);

            EnemyManager.Instance.onBindDieEnemy += (controllerEnemy) =>
            {
                PlaySound(AudioClipType.KillEnemy);
            };
            
            DataController.Instance.setting.onBindBGM += isOn => VolumeOn(SoundType.BGM, isOn);
            DataController.Instance.setting.onBindSFX += isOn => VolumeOn(SoundType.SFX, isOn);
        }

        private float GetVolum(AudioClipType type)
        {
            if (type == AudioClipType.BGM
                && !DataController.Instance.setting.isBGM)
                return 0;
            else
            {
                if (!DataController.Instance.setting.isSFX)
                    return 0;
            }

            return preAudioVolume[(int)type];
        }
        
        private void VolumeOn(SoundType type, bool flag)
        {
            if (type == SoundType.BGM)
            {
                foreach (var audioSource in _audioClips[AudioClipType.BGM])
                {
                    audioSource.volume = flag ? preAudioVolume[(int)type] : 0;
                }
            }
            else
            {
                foreach (var audioSourcePair in _audioClips)
                {
                    if (audioSourcePair.Key != AudioClipType.BGM)
                    {
                        foreach (var audioSource in audioSourcePair.Value)
                        {
                            audioSource.volume = flag ? preAudioVolume[(int)type] : 0;
                        }
                    }
                }
            }
        }

        private AudioSource GetAudioSource(AudioClipType type)
        {
            if (_audioClips == null) return null;
            
            if (!_audioClips.ContainsKey(type))
                _audioClips.Add(type, new List<AudioSource>());

            foreach (var audioSource in _audioClips[type])
            {
                if (!audioSource.isPlaying)
                    return audioSource;
            }

            if (_audioClips[type].Count > 3) return null;

            var audioSouceObject = new GameObject($"{type.ToString()}({_audioClips.Count})");
            audioSouceObject.transform.SetParent(transform);
            audioSouceObject.AddComponent<AudioSource>();
            
            _audioClips[type].Add(audioSouceObject.GetComponent<AudioSource>());
            return _audioClips[type][^1];
        }
    }
}