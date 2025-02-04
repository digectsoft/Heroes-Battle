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
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace digectsoft
{
	public class ActionAdapter : MonoBehaviour
	{	
		[Header("Actions")]
		[SerializeField]
		private List<ActionEffectStatus> actionEffects;
		[SerializeField]
		private Button pauseButton;
		[SerializeField]
		private Button helpButton;
		[SerializeField]
		private GameObject processing;
		
		[Header("Battle notification")]
		[SerializeField]
		private TextMeshProUGUI fightText;
		[SerializeField]
		private float scaleMultiplier;
		[SerializeField]
		[Min(0)]
		private float scaleDuration;
		[SerializeField]
		[Min(0)]
		private float fadeDuration;
		
		[Header("Action status")]
		[SerializeField]
		private StatusPanel statusPanel;

		private ActionPresenter actionPresenter;
		private PanelAdapter panelAdapter;
		private Dictionary<EffectType, ActionEffectStatus> effectTypes = new Dictionary<EffectType, ActionEffectStatus>();
		private Vector2 baseFightScale;
		private Vector2 targetFightScale;

		[Inject]
		public void Init(ActionPresenter actionPresenter, PanelAdapter panelAdapter) 
		{
			this.actionPresenter = actionPresenter;
			this.panelAdapter = panelAdapter;
		}

		private void Start()
		{
			pauseButton.onClick.AddListener(() => panelAdapter.ShowPanel(PanelType.PAUSE));
			helpButton.onClick.AddListener(() => panelAdapter.ShowPanel(PanelType.HELP));
			ShowProcessing(false);
			ShowFight(false);
			baseFightScale = transform.localScale;
			targetFightScale = baseFightScale * scaleMultiplier;
			foreach (ActionEffectStatus actionEffect in actionEffects)
			{
				actionEffect.Init(() => actionPresenter.TakeAction(actionEffect.EffectType));
				if (actionEffect.IsRechargable())
				{
					effectTypes.Add(actionEffect.EffectType, actionEffect);
				}
			}
		}

		/// <summary>
		/// Initializes with the specified effect values.
		/// </summary>
		/// <param name="effects">A dictionary mapping effect types to their corresponding values.</param>
		public void Init(Dictionary<EffectType, EffectValue> effects) 
		{
			ShowFight(true);
			statusPanel.Init(effects);
		}

		/// <summary>
		/// Sets the status based on the provided effect type and value.
		/// </summary>
		/// <param name="effectType">The type of effect to apply.</param>
		/// <param name="effectValue">The value associated with the effect.</param>
		public void SetStatus(EffectType effectType, EffectValue effectValue)
		{
			//Update action effect.
			if (effectTypes.ContainsKey(effectType))
			{
				int recharge = effectValue.recharge + effectValue.duration;
				bool status = recharge == 0;
				if (EffectType.CLEANUP == effectType)
				{
					CharacterEffectStatus effectStatus = actionPresenter.Player.CharacterEffect.GetCharacterEffectStatus(EffectType.FIREBALL);
					status = effectStatus.Duration > 0;
				}
				ActionEffectStatus actionEffect = effectTypes[effectType];
				actionEffect.UpdateRecharge(recharge);
			}
			//Update status panel.
			statusPanel.UpdateStatus(effectType, effectValue);
		}

		/// <summary>
		/// Updates the status panel based on the specified effect type.
		/// </summary>
		/// <param name="effectType">The type of effect to update the status panel with.</param>
		public void UpdateStatusPanel(EffectType effectType) 
		{
			statusPanel.UpdateStatus(effectType);
		}

		/// <summary>
		/// Shows or hides the processing indicator based on the provided active state.
		/// </summary>
		/// <param name="active">A boolean indicating whether to show (true) or hide (false) the processing indicator.</param>
		public void ShowProcessing(bool active) 
		{
			processing.SetActive(active);
		}

		/// <summary>
		/// Shows or hides the fight UI based on the provided active state.
		/// </summary>
		/// <param name="active">A boolean indicating whether to show (true) or hide (false) the fight UI.</param>
		public void ShowFight(bool active) 
		{
			fightText.gameObject.SetActive(active);
			if (active) 
			{
				fightText.transform.localScale = baseFightScale;
				Color color = fightText.color;
				color.a = 1;
				fightText.color = color;
				fightText.transform.DOScale(targetFightScale, scaleDuration).OnComplete(() =>
				{
					fightText.DOFade(0, fadeDuration).OnComplete(() =>
					{
						fightText.gameObject.SetActive(false);
					});
				});
			}
		}
	}
}
