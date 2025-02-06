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
using UnityEngine.UI;

namespace digectsoft
{
	public class CharacterHealth : MonoBehaviour
	{
		[SerializeField]
		private Slider healthBar;
		[SerializeField]
		private TextMeshProUGUI healthValue;
		
		private int maxValue;
		private int currentValue;
		
		public void Init(int maxValue) 
		{
			this.maxValue = currentValue = maxValue;
			UpdateBar(this.maxValue);
		}
		
		public void Set(int value) 
		{
			UpdateBar(value);
		}
		
		public void Increase(int value) 
		{
			ChangeHealth(value);
		}
		
		public void Decrease(int value) 
		{
			ChangeHealth(-value);
		}
		
		private void ChangeHealth(int value)
		{
			int updateValue = currentValue + value;
			UpdateBar(updateValue);
		}
		
		private void UpdateBar(int value) 
		{
			currentValue = Mathf.Clamp(value, 0, maxValue);
			healthBar.value = currentValue * healthBar.maxValue / maxValue;
			healthValue.text = currentValue.ToString();
		}
	}
}
