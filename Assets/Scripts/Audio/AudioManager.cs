// ---------------------------------------------------------------------------
// Copyright (c) 2025 Andrew Peresko
//
// Licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 
// International License (CC BY-NC-ND 4.0). You may not use this file 
// except in compliance with the License. To view a copy of this license,
// visit https://creativecommons.org/licenses/by-nc-nd/4.0/.
//
// You are free to:
// - Share: Copy and redistribute the material in any medium or format.
//
// Under the following terms:
// - Attribution: You must give appropriate credit, provide a link to the license,
//   and indicate if changes were made. You may do so in any reasonable manner, but 
//   not in any way that suggests the licensor endorses you or your use.
// - NonCommercial: You may not use the material for commercial purposes.
// - NoDerivatives: You may not remix, transform, or build upon the material in any way.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE, AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES, OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT, OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// ---------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

namespace digectsoft
{
	public class AudioManager : MonoBehaviour
	{
		[SerializeField]
		private AudioSource musicSource;
		[SerializeField]
		private AudioSource soundSource;
		[SerializeField]
		private List<AudioMusicValue> audioMusicValues;
		[SerializeField]
		private List<AudioSoundValue> audioSoundValues;

		private Dictionary<AudioMusicType, AudioClip> musicClips = new Dictionary<AudioMusicType, AudioClip>();
		private Dictionary<AudioSoundType, AudioClip> soundClips = new Dictionary<AudioSoundType, AudioClip>();
		
		private void Awake()
		{
			foreach (AudioMusicValue musicValue in audioMusicValues) 
			{
				musicClips.Add(musicValue.type, musicValue.clip);
			}
			foreach (AudioSoundValue soundValue in audioSoundValues) 
			{
				soundClips.Add(soundValue.type, soundValue.clip);
			}
		}
		
		public void PlayMusic(AudioMusicType musicType) 
		{
			if (musicClips.ContainsKey(musicType)) 
			{
				musicSource.clip = musicClips[musicType];
				musicSource.Play();
			}
		}
		
		public void PlaySound(AudioSoundType audioType) 
		{
			if (soundClips.ContainsKey(audioType))
			{
				soundSource.PlayOneShot(soundClips[audioType]);
			}
		}
		
		public void PlayeEffect(EffectType effectType) 
		{
			int audioId = (int)effectType;
			if (Enum.IsDefined(typeof(AudioSoundType), audioId))
			{
				AudioSoundType audioType = (AudioSoundType)audioId;
				PlaySound(audioType);
			}
		}
	}
}
