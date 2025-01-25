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
				if (EffectType.DEFAULT != effectAction.type)
				{
					effectActions.Add(effectAction.type, effectAction.value);
					if (effectAction.value.duration > 0)
					{
						EffectValue effectValue = effectAction.value.Clone();
						effectValue.duration = 0;
						effectValue.recharge = 0;
						charachterActions[CharacterType.PLAYER].effects.Add(effectAction.type, effectValue);
						charachterActions[CharacterType.ENEMY].effects.Add(effectAction.type, effectValue);
					}
				}
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
			CharacterAction playerAction = charachterActions[CharacterType.PLAYER];
			playerAction.effectType = EffectType.DEFAULT;
			CharacterAction enemyAction = charachterActions[CharacterType.ENEMY];
			enemyAction.effectType = EffectType.DEFAULT;
			bool inAction = playerAction.effects.ContainsKey(type) &&
							(playerAction.effects[type].duration > 0 || playerAction.effects[type].recharge > 0);
			if (!inAction)
			{
				UpdateEffects(ref playerAction);
				UpdateEffects(ref enemyAction);
				switch (type)
				{
					case EffectType.ATTACK:
					{
						//Player action.
						DecreaseHealth(ref enemyAction, ref playerAction, EffectType.ATTACK);
						//Enemy action.
						InvokeEnemyAction(ref playerAction, ref enemyAction);
						break;
					}
					case EffectType.SHIELD:
					case EffectType.REGENERATION:
					{
						if (SetEffect(ref playerAction, type))
						{
							//Enemy action.
							InvokeEnemyAction(ref playerAction, ref enemyAction);
						}
						break;
					}
					case EffectType.FIREBALL:
					{
						if (SetEffect(ref enemyAction, type))
						{
							//Player action.
							DecreaseHealth(ref enemyAction, playerAction.effects[type].action);
							//Enemy action.
							InvokeEnemyAction(ref playerAction, ref enemyAction);
							enemyAction.effectType = EffectType.ATTACK;
							playerAction.effectType = EffectType.FIREBALL;
						}
						break;
					}
					case EffectType.CLEANUP:
					{
						EffectValue effectValue = playerAction.effects[EffectType.FIREBALL];
						if (effectValue.duration + effectValue.recharge > 0) 
						{
							if (SetEffect(ref playerAction, type))
							{
								//Enemy action.
								InvokeEnemyAction(ref playerAction, ref enemyAction);
							}
						}
						break;
					}
				}
				ApplyEffects(ref playerAction);
				ApplyEffects(ref enemyAction);
			}
			charachterActions[CharacterType.PLAYER] = playerAction;
			charachterActions[CharacterType.ENEMY] = enemyAction;
			return charachterActions;
		}
		
		private bool SetEffect(ref CharacterAction characterAction, EffectType effectType) 
		{
			EffectValue effectValue = characterAction.effects[effectType];
			if (effectValue.duration == 0 && effectValue.recharge == 0)
			{
				characterAction.effectType = effectType;
				effectValue.duration = effectActions[effectType].duration;
				effectValue.recharge = effectActions[effectType].recharge;
				characterAction.effects[effectType] = effectValue;
				return true;
			}
			return false;
		}
		
		private void ApplyEffects(ref CharacterAction characterAction) 
		{
			Dictionary<EffectType, EffectValue> effects = new Dictionary<EffectType, EffectValue>(characterAction.effects);
			foreach (KeyValuePair<EffectType, EffectValue> keyValues in effects) 
			{
				EffectType effectType = keyValues.Key;
				EffectValue effectValue = keyValues.Value;
				int effectRate = characterAction.effects[effectType].rate;
				if (effectValue.duration > 0)
				{	
					switch (effectType) 
					{
						case EffectType.SHIELD:
						{
							effectValue.rate = effectActions[effectType].rate;
							characterAction.effects[effectType] = effectValue;
							break;
						}
						case EffectType.REGENERATION:
						{
							IncreaseHealth(ref characterAction, effectRate);
							break;
						}
						case EffectType.FIREBALL:
						{
							DecreaseHealth(ref characterAction, effectRate);
							break;
						}
						case EffectType.CLEANUP:
						{
							EffectValue fireballValue = characterAction.effects[EffectType.FIREBALL];
							if (fireballValue.duration > 0) 
							{
								fireballValue.duration = 0;
								characterAction.effects[EffectType.FIREBALL] = fireballValue;
								IncreaseHealth(ref characterAction, characterAction.effects[EffectType.FIREBALL].rate);
							}
							break;
						}
					}
				}
			}
		}
		
		private void UpdateEffects(ref CharacterAction characterAction) 
		{
			Dictionary<EffectType, EffectValue> effects = new Dictionary<EffectType, EffectValue>(characterAction.effects);
			foreach (KeyValuePair<EffectType, EffectValue> keyValues in effects) 
			{
				EffectType effectType = keyValues.Key;
				EffectValue effectValue = keyValues.Value;
				if (effectValue.duration == 0 && effectValue.recharge > 0)
				{
					effectValue.recharge--;
				}
				if (effectValue.duration > 0)
				{
					effectValue.duration--;
				}
				characterAction.effects[effectType] = effectValue;
			}
		}
		
		private void IncreaseHealth(ref CharacterAction characterAction, int value)
		{
			ChangeHealth(ref characterAction, value);
		}
		
		private void DecreaseHealth(ref CharacterAction characterAction1, 
									ref CharacterAction characterAction2,
									EffectType effectType) 
		{
			DecreaseHealth(ref characterAction1, effectActions[effectType].action);
			characterAction2.effectType = effectType;
		}
		
		private void DecreaseHealth(ref CharacterAction characterAction, int value) 
		{
			int damage = value;
			EffectValue shieldValue = characterAction.effects[EffectType.SHIELD];
			if (shieldValue.duration > 0) 
			{
				int rate = value - shieldValue.rate;
				if (rate >= 0)
				{
					shieldValue.rate = 0;
					damage = rate;
				}
				else
				{
					shieldValue.rate = -rate;
					damage = 0;
				}
				characterAction.effects[EffectType.SHIELD] = shieldValue;
			}
			ChangeHealth(ref characterAction, -damage);
		}
		
		private void ChangeHealth(ref CharacterAction characterAction, int value)
		{
			int currentHealth = characterAction.characterValue.health + value;
			characterAction.characterValue.health = Math.Clamp(currentHealth, 0, health);
		}
		
		private void InvokeEnemyAction(ref CharacterAction playerAction, ref CharacterAction enemyAction) 
		{
			//Enemy action.
			DecreaseHealth(ref playerAction, ref enemyAction, EffectType.ATTACK);
		}
	}
}
