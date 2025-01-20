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
using UnityEngine;

namespace digectsoft
{
	public class MockServerAdapter : MonoBehaviour, IServerAdapter
	{
		[SerializeField]
		[Min(0)]
		private int delayMs = 1000; //Delay in milliseconds
		[SerializeField]
		private List<EffectAction> _effectActions = new List<EffectAction>();
		
		private Dictionary<EffectType, EffectValue> effectActions = new Dictionary<EffectType, EffectValue>();
		
		private void Awake()
		{
			foreach (EffectAction effectAction in _effectActions) 
			{
				effectActions.Add(effectAction.type, effectAction.value);
			}
		}

		public async UniTask<EffectAction> Action(EffectType type)
		{
			await UniTask.Delay(delayMs);
			EffectValue effectValue = effectActions[type];
			EffectAction effectAction = new EffectAction(type, effectValue);
			return effectAction;
		}
	}
}
