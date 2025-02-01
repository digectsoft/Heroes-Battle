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
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace digectsoft 
{
	public class GameManager : MonoBehaviour
	{
		private IServerAdapter serverAdapter;
		private ActionPresenter actionPresenter;
		private bool initialized =  false;
		private bool inAction = false;

		[Inject]
		public void Init(IServerAdapter serverAdapter, ActionPresenter actionPresenter)
		{
			this.serverAdapter = serverAdapter;
			this.actionPresenter = actionPresenter;
		}

		void Start()
		{
			DOTween.Init(true, true, LogBehaviour.Verbose);
		}
		
		/// <summary>
		/// Asynchronously initializes and starts the game.
		/// </summary>
		/// <returns>A <see cref="UniTask"/> representing the asynchronous operation.</returns>
		public async UniTask OnStart()
		{
			InitStart();
			RequestStart();
			Dictionary<CharacterType, CharacterAction> characterActoins = await serverAdapter.Init();
			actionPresenter.OnInit(characterActoins);
			RequestComplete();
			InitComplete();
		}

		/// <summary>
		/// Processes a request based on the specified effect type asynchronously.
		/// </summary>
		/// <param name="effectType">The type of effect to apply in the request.</param>
		/// <returns>A <see cref="UniTask"/> representing the asynchronous operation.</returns>
		public async UniTask OnRequest(EffectType effectType)
		{
			if (!initialized || inAction)
			{
				return;
			}
			RequestStart();
			Dictionary<CharacterType, CharacterAction> effectActions = await serverAdapter.Action(effectType);
			if (initialized)
			{
				if (EffectType.DEFAULT != effectActions[CharacterType.PLAYER].effectType)
				{
					actionPresenter.OnAction(effectActions);
				}
				else
				{
					RequestComplete(effectType, effectActions);
				}
			}
		}

		/// <summary>
		/// Initiates the start of a request process.
		/// </summary>
		public void RequestStart() 
		{
			inAction = true;
			actionPresenter.OnRequestStart();
		}

		/// <summary>
		/// Marks the request as complete and processes the resulting effect actions.
		/// </summary>
		/// <param name="effectType">The type of effect applied in the request.</param>
		/// <param name="effectActions">A dictionary mapping character types to their corresponding effect actions.</param>
		public void RequestComplete(EffectType effectType, Dictionary<CharacterType, CharacterAction> effectActions)
		{
			inAction = false;
			actionPresenter.OnRequestComplete(effectType, effectActions);
		}
		
		/// <summary>
		/// Marks the request as complete.
		/// </summary>
		public void RequestComplete()
		{
			inAction = false;
			actionPresenter.OnRequestComplete();
		}

		/// <summary>
		/// Initiates the start of the initialization process.
		/// </summary>
		public void InitStart() 
		{
			initialized = false;
		}
		
		/// <summary>
		/// Marks the initialization process as complete.
		/// </summary>
		public void InitComplete() 
		{
			initialized = true;
		}
		
		/// <summary>
		/// Resets the initialization process.
		/// </summary>
		public void InitReset()
		{
			InitStart();
		}
	}
}