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
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace digectsoft
{
	public class ActionEffectStatus : MonoBehaviour
	{
		[SerializeField]
		private EffectType effectType;
		[SerializeField]
		private TextMeshProUGUI textRecharge;
		
		public EffectType EffectType { get { return effectType; } }

		private Button button;
		private bool registered;

		private void Awake()
		{
			button = GetComponent<Button>();
		}

		public void Init(UnityAction call)
		{
			if (!registered)
			{
				button.onClick.AddListener(call);
				registered = true;
			}
			UpdateRecharge(0);
		}

		public void UpdateRecharge(int recharge)
		{
			if (IsRechargable()) 
			{
				textRecharge.text = recharge.ToString();
				bool status = recharge > 0;
				textRecharge.gameObject.SetActive(status);
			}
		}
		
		public bool IsRechargable() 
		{
			return textRecharge != null;
		}
	}
}
