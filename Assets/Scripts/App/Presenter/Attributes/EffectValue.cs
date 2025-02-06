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
using System;
using UnityEngine;

namespace digectsoft
{
	[Serializable]
	public struct EffectValue
	{
		[Min(0)]
		public int action;
		[Min(0)]
		public int rate;
		[Min(0)]
		public int duration;
		[Min(0)]
		public int recharge;
		
		public int Restore { get { return duration + recharge; } }
		public bool Complete { get { return Restore == 0; } }
		public bool Action { get { return duration > 0; } }
		
		public EffectValue(int action, int rate, int duration, int recharge)
		{
			this.action = action;
			this.rate = rate;
			this.duration = duration;
			this.recharge = recharge;
		}
		
		public EffectValue Clone() 
		{
			EffectValue cloneEffectValue = new EffectValue(action, rate, duration, recharge);
			return cloneEffectValue;
		}
	}
}
