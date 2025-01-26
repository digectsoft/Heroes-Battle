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
	public class ActionAdapter : MonoBehaviour
	{		
		[SerializeField]
		private List<ActionEffectStatus> actionEffects;

		
		private GameManager gameManager;
		private ActionPresenter actionPresenter;
		private Dictionary<EffectType, ActionEffectStatus> effectTypes = new Dictionary<EffectType, ActionEffectStatus>();

		[Inject]
		public void Init(GameManager gameManager, ActionPresenter actionPresenter) 
		{
			this.gameManager = gameManager;
			this.actionPresenter = actionPresenter;
		}

		private void Start()
		{
			Init();
		}
		
		public void Init() 
		{
			foreach (ActionEffectStatus actionEffect in actionEffects)
			{
				actionEffect.Init(async () => await gameManager.OnRequestStart(actionEffect.EffectType));
				actionEffect.Activate(EffectType.CLEANUP != actionEffect.EffectType);
				if (actionEffect.IsRechargable())
				{
					effectTypes.Add(actionEffect.EffectType, actionEffect);
				}
			}
		}
		
		public void SetStatus(EffectType effectType, EffectValue effectValue)
		{
			int recharge = effectValue.recharge + effectValue.duration;
			ActionEffectStatus actionEffect = effectTypes[effectType];
			actionEffect.UpdateRecharge(recharge);
			bool status = recharge == 0;
			if (EffectType.CLEANUP == effectType)
			{
				CharacterEffectStatus effectStatus = actionPresenter.Player.CharacterEffect.GetCharacterEffectStatus(EffectType.FIREBALL);
				status = effectStatus.Duration > 0;
			}
			actionEffect.Activate(status);
		}
	}
}
