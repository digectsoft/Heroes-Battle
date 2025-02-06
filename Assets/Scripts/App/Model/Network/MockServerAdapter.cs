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
		//A delegate action for effects according a player and an enemy.
		private delegate void OnActionEffect(ref CharacterAction action1, ref CharacterAction action2);
		//Enemy actions.
		private List<EffectType> enemyActions = new List<EffectType>();
		//A delegate to update an effect.
		private delegate void OnUpdateEffect(EffectType effectType);

		public async UniTask<Dictionary<CharacterType, CharacterAction>> Init()
		{
			await UniTask.Delay(delayMs);
			effectActions.Clear();
			charachterActions.Clear();
			enemyActions.Clear();
			charachterActions.Add(CharacterType.PLAYER, new CharacterAction(health));
			charachterActions.Add(CharacterType.ENEMY, new CharacterAction(health));
			foreach (EffectAction effectAction in _effectActions)
			{
				if (EffectType.DEFAULT != effectAction.type)
				{
					effectActions.Add(effectAction.type, effectAction.value);
					EffectValue effectValue = effectAction.value.Clone();
					effectValue.duration = 0;
					effectValue.recharge = 0;
					charachterActions[CharacterType.PLAYER].effects.Add(effectAction.type, effectValue);
					charachterActions[CharacterType.ENEMY].effects.Add(effectAction.type, effectValue);
					enemyActions.Add(effectAction.type);
				}
			}
			return charachterActions;
		}

		public async UniTask<Dictionary<CharacterType, CharacterAction>> Action(EffectType effectType)
		{
			await UniTask.Delay(delayMs);
			CharacterAction playerAction = charachterActions[CharacterType.PLAYER];
			playerAction.effectType = EffectType.DEFAULT;
			CharacterAction enemyAction = charachterActions[CharacterType.ENEMY];
			enemyAction.effectType = EffectType.DEFAULT;
			//Select an appropriate action depending on an effect type.
			CharacterAction testAction = GetCharacterAction(CharacterType.PLAYER, effectType);
			bool inAction = testAction.effects.ContainsKey(effectType) && !testAction.effects[effectType].Complete;
			if (EffectType.CLEANUP == effectType)
			{
				//For a cleanup it is required to check a fireball effect.
				inAction = !testAction.effects[EffectType.FIREBALL].Action || inAction;
			}
			if (!inAction)
			{
				UpdateEffects(ref playerAction);
				UpdateEffects(ref enemyAction, (et) => UpdateEnemyEffect(et));
				ApplyAction(effectType, ref playerAction, ref enemyAction,
						   (ref CharacterAction pl, ref CharacterAction en) => InvokeEnemyAction(ref pl, ref en));
				ApplyEffects(ref playerAction);
				ApplyEffects(ref enemyAction);
			}
			charachterActions[CharacterType.PLAYER] = playerAction;
			charachterActions[CharacterType.ENEMY] = enemyAction;
			// CheckWinner();
			return charachterActions;
		}

		/// <summary>
		/// Applies an effect to the specified character actions.
		/// </summary>
		/// <param name="effectType">The type of effect to apply.</param>
		/// <param name="character1">A reference to the first character's action to modify.</param>
		/// <param name="character2">A reference to the second character's action to modify.</param>
		/// <param name="OnAction">An optional delegate invoked when the action is applied.</param>
		private void ApplyAction(EffectType effectType,
								 ref CharacterAction character1,
								 ref CharacterAction character2,
								 OnActionEffect OnAction = null) 
		{
			switch (effectType)
			{
				case EffectType.ATTACK:
				{
					//Character2 action.
					OnAction?.Invoke(ref character1, ref character2);
					//Character1 action.
					DecreaseHealth(ref character2, effectActions[effectType].action);
					break;
				}
				case EffectType.SHIELD:
				case EffectType.REGENERATION:
				case EffectType.CLEANUP:
				{
					//Character1 action.
					SetEffect(ref character1, effectType);
					//Character2 action.
					OnAction?.Invoke(ref character1, ref character2);
					break;
				}
				case EffectType.FIREBALL:
				{
					//Character2 action.
					OnAction?.Invoke(ref character1, ref character2);
					//Character1 action
					if (SetEffect(ref character2, effectType))
					{
						DecreaseHealth(ref character2, effectActions[effectType].action);
					}
					break;
				}
			}
			character1.effectType = effectType;
		}

		/// <summary>
		/// Applies the specified effect to the given character action.
		/// </summary>
		/// <param name="characterAction">A reference to the character action to modify.</param>
		/// <param name="effectType">The type of effect to apply.</param>
		/// <returns>
		/// <c>true</c> if the effect was successfully applied; otherwise, <c>false</c>.
		/// </returns>
		private bool SetEffect(ref CharacterAction characterAction, EffectType effectType) 
		{
			EffectValue effectValue = characterAction.effects[effectType];
			if (effectValue.Complete)
			{
				effectValue.duration = effectActions[effectType].duration;
				effectValue.recharge = effectActions[effectType].recharge;
				characterAction.effects[effectType] = effectValue;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Applies all relevant effects to the specified character action.
		/// </summary>
		/// <param name="characterAction">A reference to the character action to modify.</param>
		private void ApplyEffects(ref CharacterAction characterAction) 
		{
			Dictionary<EffectType, EffectValue> effects = new Dictionary<EffectType, EffectValue>(characterAction.effects);
			foreach (KeyValuePair<EffectType, EffectValue> keyValues in effects) 
			{
				EffectType effectType = keyValues.Key;
				EffectValue effectValue = keyValues.Value;
				if (effectValue.duration > 0)
				{	
					switch (effectType) 
					{
						case EffectType.REGENERATION:
						{
							IncreaseHealth(ref characterAction, effectValue.rate);
							break;
						}
						case EffectType.FIREBALL:
						{
							DecreaseHealth(ref characterAction, effectValue.rate);
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

		/// <summary>
		/// Updates the active effects on the specified character action.
		/// </summary>
		/// <param name="characterAction">A reference to the character action to modify.</param>
		/// <param name="OnUpdate">An optional delegate invoked when an effect is updated.</param>
		private void UpdateEffects(ref CharacterAction characterAction, OnUpdateEffect OnUpdate = null) 
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
				effectValue.rate = effectActions[effectType].rate;
				characterAction.effects[effectType] = effectValue;
				OnUpdate?.Invoke(effectType);
			}
		}

		/// <summary>
		/// Increases the health value of the specified character action.
		/// </summary>
		/// <param name="characterAction">A reference to the character action to modify.</param>
		/// <param name="value">The amount of health to add.</param>
		private void IncreaseHealth(ref CharacterAction characterAction, int value)
		{
			ChangeHealth(ref characterAction, value);
		}

		/// <summary>
		/// Decreases the health value of the specified character action.
		/// </summary>
		/// <param name="characterAction">A reference to the character action to modify.</param>
		/// <param name="value">The amount of health to subtract.</param>
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

		/// <summary>
		/// Changes the health value of the specified character action by the given amount.
		/// </summary>
		/// <param name="characterAction">A reference to the character action to modify.</param>
		/// <param name="value">The amount to change the health by (can be positive or negative).</param>
		private void ChangeHealth(ref CharacterAction characterAction, int value)
		{
			int currentHealth = characterAction.characterValue.health + value;
			characterAction.characterValue.health = currentHealth > health ? health : currentHealth;
		}

		/// <summary>
		/// Updates the effect on the enemy based on the specified effect type.
		/// </summary>
		/// <param name="effectType">The type of effect to apply to the enemy.</param>
		private void UpdateEnemyEffect(EffectType effectType) 
		{
			CharacterAction characterAction = GetCharacterAction(CharacterType.ENEMY, effectType);
			EffectValue effectValue = characterAction.effects[effectType]; 
			if (!enemyActions.Contains(effectType) && effectValue.Complete) 
			{
				enemyActions.Add(effectType);
			}
		}

		/// <summary>
		/// Invokes the enemy's action in response to the player's action.
		/// </summary>
		/// <param name="playerAction">A reference to the player's action to consider when invoking the enemy's action.</param>
		/// <param name="enemyAction">A reference to the enemy's action that will be updated.</param>
		private void InvokeEnemyAction(ref CharacterAction playerAction, ref CharacterAction enemyAction) 
		{
			if (enemyActions.Count == 0) 
			{
				return;
			}
			List<EffectType> effectTypes = new List<EffectType>(enemyActions);
			//Check the fireball effect and remove the cleanup effect whether the fireball effect is complete.
			EffectValue effectFireball = enemyAction.effects[EffectType.FIREBALL];
			if (!effectFireball.Action)
			{
				effectTypes.Remove(EffectType.CLEANUP);
			}
			//Select a random effect.
			int enemyIndex = Random.Range(0, effectTypes.Count);
			EffectType effectType = effectTypes[enemyIndex];
			// EffectType effectType = EffectType.FIREBALL;
			//The attack should always be in actions.
			if (EffectType.ATTACK != effectType)
			{
				enemyActions.Remove(effectType);
			}
			ApplyAction(effectType, ref enemyAction, ref playerAction);
		}

		/// <summary>
		/// Retrieves the character action based on the specified character type and effect type.
		/// </summary>
		/// <param name="characterType">The type of character whose action is to be retrieved.</param>
		/// <param name="effectType">The type of effect that influences the character's action.</param>
		/// <returns>
		/// The corresponding character action for the given character type and effect type.
		/// </returns>
		private CharacterAction GetCharacterAction(CharacterType characterType, EffectType effectType)
		{
			CharacterAction characterAction;
			if (CharacterType.PLAYER == characterType)
			{
				characterAction = EffectType.FIREBALL == effectType ?
								  charachterActions[CharacterType.ENEMY] :
								  charachterActions[CharacterType.PLAYER];
			}
			else
			{
				characterAction = EffectType.FIREBALL == effectType ?
								  charachterActions[CharacterType.PLAYER] :
								  charachterActions[CharacterType.ENEMY];
			}
			return characterAction;
		}

		/// <summary>
		/// Checks the current state to determine the winner.
		/// </summary>
		private void CheckWinner() 
		{
			CharacterAction playerAction = charachterActions[CharacterType.PLAYER];
			CharacterAction enemyAction = charachterActions[CharacterType.ENEMY];
			//First check the health of the enemy because the player begins an action first.
			if (enemyAction.characterValue.health <= 0) 
			{
				//Write a data when the enemy is the winner.
				return;
			}
			if (playerAction.characterValue.health <= 0) 
			{
				//Write a data when the player is the winner.
			}
		}
	}
}
