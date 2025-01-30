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
		private AudioManager audioManager;

		[Inject]
		public void Init(GameManager gameManager,
						 PanelAdapter panelAdapter,
						 [InjectOptional] AudioManager audioManager,
						 [Inject(Id = CharacterType.PLAYER)] Character player,
					 	 [Inject(Id = CharacterType.ENEMY)] Character enemy)
		{
			this.gameManager = gameManager;
			this.panelAdapter = panelAdapter;
			this.audioManager = audioManager;
			Player = player;
			Enemy = enemy;
		}
		
		void Start()
		{
			BeginGame();
		}

		/// <summary>
		/// Begins the game by showing the gameplay panel and playing the background music.
		/// </summary>
		public void BeginGame() 
		{
			panelAdapter.ShowPanel(PanelType.PLAY);
			audioManager?.PlayMusic(AudioMusicType.MUSIC_MENU);
		}

		/// <summary>
		/// Starts the game asynchronously, initializing the game environment and processes.
		/// </summary>
		/// <exception cref="Exception">
		/// Any exception thrown during the game start process will be caught and passed to the <see cref="OnError"/> method.
		/// </exception>
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

		/// <summary>
		/// Initiates an action based on the specified effect type asynchronously.
		/// </summary>
		/// <param name="effectType">The type of effect to apply during the action.</param>
		/// <exception cref="Exception">
		/// Any exception thrown during the action request will be caught and passed to the <see cref="OnError"/> method.
		/// </exception>
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

		/// <summary>
		/// Initializes the character actions with the provided dictionary of character types and actions.
		/// </summary>
		/// <param name="characterActions">A dictionary mapping each character type to its corresponding action.</param>
		public void OnInit(Dictionary<CharacterType, CharacterAction> characterActoins) 
		{
			actionAdapter.ShowFight(true);
			CharacterAction playerAction = characterActoins[Player.CharacterType];
			CharacterAction enemyAction = characterActoins[Enemy.CharacterType];
			Player.Init(playerAction.characterValue.health, Enemy.transform.position);
			Enemy.Init(enemyAction.characterValue.health, Player.transform.position);
			UpdateEffects(characterActoins, Player, Enemy);
			UpdateEffects(characterActoins, Enemy, Player);
			audioManager?.PlayMusic(AudioMusicType.MUSIC_BATTLE);
		}

		/// <summary>
		/// Handles the actions of characters based on the provided dictionary of character types and actions.
		/// </summary>
		/// <param name="characterActions">A dictionary mapping each character type to its corresponding action.</param>
		public void OnAction(Dictionary<CharacterType, CharacterAction> characterActoins)
		{
			CharacterAction playerAction = characterActoins[Player.CharacterType];
			CharacterAction enemyAction = characterActoins[Enemy.CharacterType];
			UpdateEffects(characterActoins, Player, Enemy);
			UpdateEffects(characterActoins, Enemy, Player);
			Sequence sequence = DOTween.Sequence();
			sequence.AppendCallback(() => Action(characterActoins, playerAction.effectType, Player, Enemy));
			if (IsAlive(enemyAction)) 
			{
				sequence.AppendInterval(stepInterval);
				sequence.AppendCallback(() => Action(characterActoins, enemyAction.effectType, Enemy, Player));
			}
			sequence.AppendInterval(completeInterval);
			sequence.AppendCallback(gameManager.RequestComplete);
			sequence.AppendCallback(() => CheckWinner(playerAction, enemyAction));
		}

		/// <summary>
		/// Checks the winner based on the player’s and enemy’s actions.
		/// </summary>
		/// <param name="playerAction">The action performed by the player.</param>
		/// <param name="enemyAction">The action performed by the enemy.</param>
		private void CheckWinner(CharacterAction playerAction, CharacterAction enemyAction) 
		{
			if (!IsAlive(enemyAction))
			{
				Enemy.Death();
				panelAdapter.ShowPanel(PanelType.WIN);
				gameManager.InitReset();
				return;
			}
			if (!IsAlive(playerAction))
			{
				Player.Death();
				panelAdapter.ShowPanel(PanelType.GAME_OVER);
				gameManager.InitReset();
			}
		}

		/// <summary>
		/// Checks if the character is still alive based on the given character action.
		/// </summary>
		/// <param name="characterAction">The action of the character to evaluate for life status.</param>
		/// <returns>
		/// <c>true</c> if the character is alive; otherwise, <c>false</c>.
		/// </returns>
		private bool IsAlive(CharacterAction characterAction) 
		{
			return characterAction.characterValue.health > 0;
		}

		/// <summary>
		/// Initiates the start of a request process.
		/// </summary>
		public void OnRequestStart() 
		{
			actionAdapter.ShowProcessing(true);
		}

		/// <summary>
		/// Finalizes the request process once it is complete.
		/// </summary>
		public void OnRequestComplete() 
		{
			actionAdapter.ShowProcessing(false);
		}

		/// <summary>
		/// Handles errors by processing the given exception.
		/// </summary>
		/// <param name="exception">The exception that occurred during the operation.</param>
		public void OnError(Exception exception)
		{
			Debug.Log(exception);
			panelAdapter.ShowPanel(PanelType.ERROR);
		}

		/// <summary>
		/// Executes the specified action based on the character actions and effect type for two characters.
		/// </summary>
		/// <param name="characterActions">A dictionary mapping character types to their corresponding actions.</param>
		/// <param name="effectType">The type of effect to apply during the action.</param>
		/// <param name="character1">The first character involved in the action.</param>
		/// <param name="character2">The second character involved in the action.</param>
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

		/// <summary>
		/// Updates the effects for the specified characters based on the provided character actions.
		/// </summary>
		/// <param name="effectActions">A dictionary mapping character types to their corresponding effect actions.</param>
		/// <param name="character1">The first character whose effects will be updated.</param>
		/// <param name="character2">The second character whose effects will be updated.</param>
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
