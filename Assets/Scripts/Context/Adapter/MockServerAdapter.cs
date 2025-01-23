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
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace digectsoft
{
	public class MockServerAdapter : MonoBehaviour, IServerAdapter
	{
		[SerializeField]
		[Min(0)]
		private int delayMs = 1000; //Delay in milliseconds
		[SerializeField]
		[Min(0)]
		private int health = 100;
		[SerializeField]
		private List<EffectAction> _effectActions = new List<EffectAction>();
		
		//Configuration effects.
		private Dictionary<EffectType, EffectValue> effectActions = new Dictionary<EffectType, EffectValue>();
		//Effects for characters during a battle.
		private Dictionary<CharacterType, CharacterAction> charachterActions = new Dictionary<CharacterType, CharacterAction>();
		
		private void Awake()
		{
			charachterActions.Add(CharacterType.PLAYER, new CharacterAction(health));
			charachterActions.Add(CharacterType.ENEMY, new CharacterAction(health));
			foreach (EffectAction effectAction in _effectActions) 
			{
				effectActions.Add(effectAction.type, effectAction.value);
				charachterActions[CharacterType.PLAYER].effects.Add(effectAction.type, new EffectValue());
				charachterActions[CharacterType.ENEMY].effects.Add(effectAction.type, new EffectValue());
			}
		}

		public async UniTask<Dictionary<CharacterType, CharacterValue>> Init()
		{
			await UniTask.Delay(delayMs);
			Dictionary<CharacterType, CharacterValue> characterValues = new Dictionary<CharacterType, CharacterValue>
			{
				{ CharacterType.PLAYER, new CharacterValue(health) },
				{ CharacterType.ENEMY, new CharacterValue(health) }
			};
			return characterValues;
		}

		public async UniTask<Dictionary<CharacterType, CharacterAction>> Action(EffectType type)
		{
			await UniTask.Delay(delayMs);
			EffectValue effectValue = effectActions[type];
			switch (type) 
			{
				case EffectType.ATTACK:
					//Player action.
					CharacterAction playerAction = charachterActions[CharacterType.PLAYER];
					DecreaseHealth(ref playerAction, effectActions[type].action);
					playerAction.effectType = EffectType.ATTACK;
					charachterActions[CharacterType.PLAYER] = playerAction;
					EffectValue playerEffectValue = charachterActions[CharacterType.PLAYER].effects[EffectType.ATTACK];
					playerEffectValue.action = effectActions[type].action;
					charachterActions[CharacterType.PLAYER].effects[EffectType.ATTACK] = playerEffectValue;
					//Enemy action.
					CharacterAction enemyAction = charachterActions[CharacterType.ENEMY];
					DecreaseHealth(ref enemyAction, effectActions[type].action);
					enemyAction.effectType = EffectType.ATTACK;
					charachterActions[CharacterType.ENEMY] = enemyAction;
					EffectValue enemyEffectValue = charachterActions[CharacterType.ENEMY].effects[EffectType.ATTACK];
					enemyEffectValue.action = effectActions[type].action;
					charachterActions[CharacterType.ENEMY].effects[EffectType.ATTACK] = enemyEffectValue;
					break;
			}
			return charachterActions;
		}
		
		private void IncreaseHealth(ref CharacterAction characterAction, int value) 
		{
			ChangeHealth(ref characterAction, value);
		}
		
		private void DecreaseHealth(ref CharacterAction characterAction, int value) 
		{
			ChangeHealth(ref characterAction, -value);
		}
		
		private void ChangeHealth(ref CharacterAction characterAction, int value) 
		{
			int currentHealth = characterAction.health + value;
			characterAction.health = Math.Clamp(currentHealth, 0, health);
		}
	}
}
