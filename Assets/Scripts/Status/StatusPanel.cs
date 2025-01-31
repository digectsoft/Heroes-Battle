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
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace digectsoft
{
	public class StatusPanel : MonoBehaviour
	{
		[Header("Containers")]
		[SerializeField]
		private GameObject selectContainer;
		[SerializeField]
		private GameObject actionContainer;
		
		[Header("Actions")]
		[SerializeField]
		private LocalizeStringEvent statusNameLocale;
		[SerializeField]
		private LocalizeStringEvent statusDescriptionLocale;
		[SerializeField]
		private List<StatusText> statusNames;
		[SerializeField]
		private List<StatusText> statusDescriptions;
		[SerializeField]
		private List<StatusItem> statusItems;

		private Dictionary<EffectType, EffectValue> effects;
		private Dictionary<EffectType, LocalizedString> names = new Dictionary<EffectType, LocalizedString>();
		private Dictionary<EffectType, LocalizedString> descriptions = new Dictionary<EffectType, LocalizedString>();
		private Dictionary<StatusType, StatusItem> items = new Dictionary<StatusType, StatusItem>();
		private EffectType currentEffectType = EffectType.DEFAULT;

		public LocalizeStringEvent StatusNameLocale { get => statusNameLocale; set => statusNameLocale = value; }

		private void Awake()
		{
			foreach (StatusText statusName in statusNames) 
			{
				names.Add(statusName.type, statusName.text);
			}
			foreach (StatusText statusDescription in statusDescriptions)
			{
				descriptions.Add(statusDescription.type, statusDescription.text);
			}
			foreach (StatusItem statusItem in statusItems) 
			{
				items.Add(statusItem.StatusType, statusItem);
			}
		}
		
		public void Init(Dictionary<EffectType, EffectValue> effects) 
		{
			this.effects = new Dictionary<EffectType, EffectValue>(effects);
		}
		
		public void UpdateStatus(EffectType effectType) 
		{
			bool updateStatus = EffectType.DEFAULT != effectType;
			if (updateStatus) 
			{
				currentEffectType = effectType;
				statusNameLocale.StringReference = names[effectType];
				statusDescriptionLocale.StringReference = descriptions[effectType];
			}
			selectContainer.SetActive(!updateStatus);
			actionContainer.SetActive(updateStatus);
		}
		
		public void UpdateStatus(EffectType effectType, EffectValue effectValue) 
		{
			if (currentEffectType == effectType)
			{
				ResetStatusItems();
				switch (effectType)
				{
					case EffectType.ATTACK:
					{
						items[StatusType.DAMAGE].UpdateValue(effects[effectType].action);
						break;
					}
					case EffectType.SHIELD:
					{
						items[StatusType.PROTECTION].UpdateValue(effects[effectType].rate);
						ActivateAction(effectValue);
						break;
					}
					case EffectType.REGENERATION:
					{
						items[StatusType.HEAL_RATE].UpdateValue(effects[effectType].rate);
						ActivateAction(effectValue);
						break;
					}
					case EffectType.FIREBALL:
					{
						items[StatusType.DAMAGE].UpdateValue(effects[effectType].action);
						items[StatusType.DAMAGE_RATE].UpdateValue(effects[effectType].rate);
						ActivateAction(effectValue);
						break;
					}
					case EffectType.CLEANUP:
					{
						if (!effectValue.Action)
						{
							items[StatusType.UNAVAILABLE].UpdateValue();
							if (!effectValue.Complete)
							{
								items[StatusType.STEP_REUSE].UpdateValue(effectValue.Restore);
							}
						}
						else
						{
							items[StatusType.STEP_REUSE].UpdateValue(effectValue.Restore);
							items[StatusType.IN_ACTION].UpdateValue();
						}
						break;
					}
				}
			}
		}
		
		private void ResetStatusItems() 
		{
			foreach (StatusItem statusItem in statusItems)
			{
				statusItem.Activate(false);
			}
		}

		private void ActivateAction(EffectValue effectValue)
		{
			items[StatusType.STEP_REUSE].UpdateValue(effectValue.Restore);
			if (effectValue.Action)
			{
				items[StatusType.IN_ACTION].UpdateValue();
			}
			else if (!effectValue.Complete)
			{
				items[StatusType.UNAVAILABLE].UpdateValue();
			}
		}
	}
}
