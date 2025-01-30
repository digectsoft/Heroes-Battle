// ---------------------------------------------------------------------------
// Copyright (c) 2025 Andrew Peresko
// Contact: andrew.peresko@digectsoft.com
//
// Licensed under the Creative Commons Attribution-NonCommercial 4.0 
// International License (CC BY-NC 4.0). You may not use this file 
// except in compliance with the License. To view a copy of this license,
// visit https://creativecommons.org/licenses/by-nc/4.0/.
//
// You are free to:
// - Share: Copy and redistribute the material in any medium or format.
// - Adapt: Remix, transform, and build upon the material.
//
// Under the following terms:
// - Attribution: You must give appropriate credit, provide a link to the license,
//   and indicate if changes were made. You may do so in any reasonable manner, but 
//   not in any way that suggests the licensor endorses you or your use.
// - NonCommercial: You may not use the material for commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE, AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES, OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT, OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// ---------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace digectsoft
{
	public class Character : MonoBehaviour
	{
		[Header("Status")]
		[SerializeField]
		private CharacterType type;
		[SerializeField]
		private CharacterHealth health;
		[SerializeField]
		private CharacterEffect effect;
		
		[Header("Animations")]
		[SerializeField]
		private string attackValue;
		[SerializeField]
		private string hitValue;
		[SerializeField]
		private string deathValue;
		
		[Header("Arrow")]
		[SerializeField]
		private CharacterArrow arrow;
		[SerializeField]
		private float flightDuration;
		
		[Header("Impacts")]
		[SerializeField]
		private float scaleMultiplier;
		[SerializeField]
		[Min(0)]
		private float scaleDuration;
		[SerializeField]
		[Min(0)]
		private float fadeDuration;
		[SerializeField]
		private List<CharacterImpact> impacts;

		public CharacterType CharacterType { get { return type; } }
		public CharacterEffect CharacterEffect { get { return effect; } }

		[InjectOptional]
		private AudioManager audioManager;
		private Dictionary<EffectType, CharacterImpact> characterImpacts = new Dictionary<EffectType, CharacterImpact>();
		private Animator animator;
		
		private void Awake()
		{	
			animator = GetComponent<Animator>();
			foreach (CharacterImpact characterImpact in impacts) 
			{
				characterImpacts.Add(characterImpact.EffectType, characterImpact);
			}
		}
		
		public void Init(int healthValue, Vector2 targetPosition) 
		{
			health.Init(healthValue);
			arrow.Init(targetPosition, flightDuration);
			foreach (CharacterImpact characterImpact in characterImpacts.Values) 
			{
				characterImpact.Init(scaleMultiplier, scaleDuration, fadeDuration);
			}
			animator.Rebind();
		}

		public void Apply(EffectType effectType)
		{
			characterImpacts[effectType].Apply();
		}

		public void Attack() 
		{
			animator.SetBool(attackValue, true);
		}
		
		public void Shot() 
		{
			arrow.Flight();
			audioManager?.PlaySound(AudioSoundType.SOUND_ATTACK);
		}
		
		public void Effect(EffectType effectType, int duration) 
		{
			effect.SetStatus(effectType, duration);
		}
		
		public void Regeneration(int value) 
		{
			health.Increase(value);
		}
		
		public void Damage(int value) 
		{
			health.Decrease(value);
		}
		
		public void Recovery(int value) 
		{
			health.Set(value);
		}
		
		public void Hit(int value) 
		{
			UpdateHealth(value);
			animator.SetBool(hitValue, true);
			audioManager?.PlaySound(AudioSoundType.SOUND_HIT);
		}
		
		public void Death() 
		{
			animator.SetBool(deathValue, true);
		}

		public void UpdateHealth(int value)
		{
			health.Set(value);
		}
	}
}
