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
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace digectsoft
{
	public class ActionPresenter : MonoBehaviour
	{
		[Header("Gameplay")]
		[SerializeField]
		[Min(0)]
		private float hitInterval = 1;
		[SerializeField]
		[Min(0)]
		private float stepInterval = 2;
		[SerializeField]
		[Min(0)]
		private float completeInterval = 1;
		
		[Header("Control")]
		[SerializeField]
		private ActionAdapter actionAdapter;

		public Character Player { get; private set; }
		public Character Enemy { get; private set; }

		private GameManager gameManager;
		private PanelAdapter panelAdapter;

		[Inject]
		public void Init(GameManager gameManager,
						 PanelAdapter panelAdapter,
						 [Inject(Id = CharacterType.PLAYER)] Character player,
					 	 [Inject(Id = CharacterType.ENEMY)] Character enemy)
		{
			this.gameManager = gameManager;
			this.panelAdapter = panelAdapter;
			Player = player;
			Enemy = enemy;
		}
		
		void Start()
		{
			BeginGame();
		}
		
		public void BeginGame() 
		{
			panelAdapter.ShowPanel(PanelType.PLAY);
		}

		public async void StartGame()
		{
			try 
			{
				await gameManager.OnStart();
			}
			catch (Exception ex) 
			{
				OnError(ex);
			}
		}

		public async void TakeAction(EffectType effectType)
		{
			try 
			{
				await gameManager.OnRequest(effectType);
			}
			catch (Exception ex)
			{
				OnError(ex);
			}
		}

		public void OnInit(Dictionary<CharacterType, CharacterAction> characterActoins) 
		{
			actionAdapter.ShowFight(true);
			CharacterAction playerAction = characterActoins[Player.CharacterType];
			CharacterAction enemyAction = characterActoins[Enemy.CharacterType];
			Player.Init(playerAction.characterValue.health, Enemy.transform.position);
			Enemy.Init(enemyAction.characterValue.health, Player.transform.position);
			UpdateEffects(characterActoins, Player, Enemy);
			UpdateEffects(characterActoins, Enemy, Player);
		}

		public void OnAction(Dictionary<CharacterType, CharacterAction> characterActoins)
		{
			CharacterAction playerAction = characterActoins[Player.CharacterType];
			CharacterAction enemyAction = characterActoins[Enemy.CharacterType];
			UpdateEffects(characterActoins, Player, Enemy);
			UpdateEffects(characterActoins, Enemy, Player);
			Sequence sequence = DOTween.Sequence();
			sequence.AppendCallback(() => Action(characterActoins, playerAction.effectType, Player, Enemy));
			sequence.AppendInterval(stepInterval);
			sequence.AppendCallback(() => Action(characterActoins, enemyAction.effectType, Enemy, Player));
			sequence.AppendInterval(completeInterval);
			sequence.AppendCallback(gameManager.RequestComplete);
		}
		
		public void OnRequestStart() 
		{
			actionAdapter.ShowProcessing(true);
		}
		
		public void OnRequestComplete() 
		{
			actionAdapter.ShowProcessing(false);
		}
		
		public void OnError(Exception exception)
		{
			// Debug.Log(exception);
			throw exception;
		}

		private void Action(Dictionary<CharacterType, CharacterAction> characterActoins,
							EffectType effectType,
							Character character1,
							Character character2)
		{
			Character characterEffect = character1;
			switch (effectType)
			{
				case EffectType.ATTACK:
				{
					Sequence sequence = DOTween.Sequence();
					sequence.AppendCallback(() => character1.Attack());
					sequence.AppendInterval(hitInterval);
					sequence.AppendCallback(() => character2.Hit(characterActoins[character2.CharacterType].characterValue.health));
					break;
				}
				case EffectType.FIREBALL:
				{
					characterEffect = character2;
					character2.Hit(characterActoins[character2.CharacterType].characterValue.health);
					break;
				}
			}
			if (EffectType.ATTACK != effectType) 
			{
				characterEffect.Apply(effectType);
			}
		}

		private void UpdateEffects(Dictionary<CharacterType, CharacterAction> effectActions,
								   Character character1,
								   Character character2) 
		{
			CharacterAction characterAction1 = effectActions[character1.CharacterType];
			CharacterAction characterAction2 = effectActions[character2.CharacterType];
			foreach (KeyValuePair<EffectType, EffectValue> keyValues in characterAction1.effects) 
			{
				//Update character effects.
				EffectType effectType = keyValues.Key;
				EffectValue effectValue = keyValues.Value;
				character1.Effect(effectType, effectValue.duration);
				if (effectValue.duration > 0)
				{
					if (EffectType.REGENERATION == effectType)
					{
						character1.Regeneration(effectValue.rate);
					}
					if (EffectType.FIREBALL == effectType)
					{
						if (characterAction2.effectType != effectType) 
						{
							character1.Damage(effectValue.rate);
						}
					}
				}
				//Update action effects.
				bool updateAdapter = (CharacterType.PLAYER == character1.CharacterType && EffectType.FIREBALL != effectType) ||
									 (CharacterType.ENEMY == character1.CharacterType && EffectType.FIREBALL == effectType);
				if (updateAdapter)
				{
					actionAdapter.SetStatus(effectType, effectValue);
				}
			}
		}
	}
}
