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
using UnityEngine;
using Zenject;

namespace digectsoft
{
	public class Character : MonoBehaviour
	{
		[SerializeField]
		private CharacterType type;
		[SerializeField]
		private CharacterHealth health;
		[SerializeField]
		private CharacterEffect effect;
		[SerializeField]
		private string attackValue;
		[SerializeField]
		private string hitValue;
		[SerializeField]
		private string deathValue;
		
		public CharacterType CharacterType { get {return type; } private set { } }
		
		[Inject]
		private ActionPresenter actionPresenter;
		private Animator animator;
		
		private void Awake()
		{
			animator = GetComponent<Animator>();
		}
		
		public void Init(int healthValue) 
		{
			health.Init(healthValue);
		}

		void Start()
		{
		
		}

		void Update()
		{
		
		}
		
		public void Attack() 
		{
			animator.SetBool(attackValue, true);
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
		
		public void UpdateHealth(int value) 
		{
			health.Set(value);
		}
		
		public void Hit(int value) 
		{
			UpdateHealth(value);
			animator.SetBool(hitValue, true);
		}
	}
}
