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
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace digectsoft
{
	public class ActionPresenter : MonoBehaviour
	{
		[SerializeField]
		[Min(0)]
		private float hitInterval = 1;
		[SerializeField]
		[Min(0)]
		private float stepInterval = 2;
		[SerializeField]
		[Min(0)]
		private float completeInterval = 1;
		[SerializeField]
		private ActionAdapter actionAdapter;

		private GameManager gameManager;
		private Character player;
		private Character enemy;
		private CharacterValue playerValue;
		private CharacterValue enemyValue;

		[Inject]
		public void Init(GameManager gameManager,
						 [Inject(Id = CharacterType.PLAYER)] Character player,
					 	 [Inject(Id = CharacterType.ENEMY)] Character enemy)
		{
			this.gameManager = gameManager;
			this.player = player;
			this.enemy = enemy;
		}
		
		public void OnInit(CharacterValue playerValue, CharacterValue enemyValue) 
		{
			this.playerValue = playerValue;
			this.enemyValue = enemyValue;
			player.Init(this.playerValue.health);
			enemy.Init(this.enemyValue.health);
		}

		public void OnAction(Dictionary<CharacterType, CharacterAction> effectActions)
		{
			CharacterAction playerAction = effectActions[player.CharacterType];
			CharacterAction enemyAction = effectActions[enemy.CharacterType];
			UpdateEffects(effectActions, player);
			UpdateEffects(effectActions, enemy);
			Sequence sequence = DOTween.Sequence();
			sequence.AppendCallback(() => Action(effectActions, playerAction.effectType, player, enemy));
			sequence.AppendInterval(stepInterval);
			sequence.AppendCallback(() => Action(effectActions, enemyAction.effectType, enemy, player));
			sequence.AppendInterval(completeInterval);
			sequence.AppendCallback(gameManager.OnRequestComplete);
		}

		private void Action(Dictionary<CharacterType, CharacterAction> effectActions,
							EffectType effectType,
							Character character1,
							Character character2)
		{
			switch (effectType)
			{
				case EffectType.ATTACK:
				{
					Sequence sequence = DOTween.Sequence();
					sequence.AppendCallback(() => character1.Attack());
					sequence.AppendInterval(hitInterval);
					sequence.AppendCallback(() => character2.Hit(effectActions[character2.CharacterType].characterValue.health));
					break;
				}
				case EffectType.FIREBALL:
				{
					character2.Hit(effectActions[character2.CharacterType].characterValue.health);
					break;
				}
			}
		}

		private void UpdateEffects(Dictionary<CharacterType, CharacterAction> effectActions, Character character) 
		{
			CharacterAction characterAction = effectActions[character.CharacterType];
			foreach (KeyValuePair<EffectType, EffectValue> keyValues in characterAction.effects) 
			{
				//Update character effects.
				EffectType effectType = keyValues.Key;
				EffectValue effectValue = keyValues.Value;
				character.Effect(effectType, effectValue.duration);
				if (effectValue.duration > 0)
				{
					if (EffectType.REGENERATION == effectType)
					{
						character.Regeneration(effectValue.rate);
					}
					if (EffectType.FIREBALL == effectType)
					{
						character.Damage(effectValue.rate);
					}
				}
				//Update action effects.
				bool updateAdapter = (CharacterType.PLAYER == character.CharacterType) ||
									 ((CharacterType.ENEMY == character.CharacterType) && (EffectType.FIREBALL == effectType));
				if (updateAdapter)
				{
					actionAdapter.SetStatus(effectType, effectValue);
				}
			}
		}
	}
}
