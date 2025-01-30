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

		public async UniTask OnStart() 
		{
			InitStart();
			RequestStart();
			Dictionary<CharacterType, CharacterAction> characterActoins = await serverAdapter.Init();
			actionPresenter.OnInit(characterActoins);
			RequestComplete();
			InitComplete();
		}

		public async UniTask OnRequest(EffectType actionType)
		{
			if (!initialized || inAction)
			{
				return;
			}
			RequestStart();
			Dictionary<CharacterType, CharacterAction> effectActions = await serverAdapter.Action(actionType);
			if (initialized) 
			{
				// if (EffectType.DEFAULT != effectActions[CharacterType.PLAYER].effectType)
				{
					actionPresenter.OnAction(effectActions);
				}
				// else
				{
					// RequestComplete();
				}
			}
		}
		
		public void RequestStart() 
		{
			inAction = true;
			actionPresenter.OnRequestStart();
		}

		public void RequestComplete()
		{
			inAction = false;
			actionPresenter.OnRequestComplete();
		}
		
		public void InitStart() 
		{
			initialized = false;
		}
		
		public void InitComplete() 
		{
			initialized = true;
		}
		
		public void InitReset() 
		{
			InitStart();
		}
	}
}