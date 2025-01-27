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
using DG.Tweening;
using UnityEngine;

namespace digectsoft
{
	public class CharacterImpact : MonoBehaviour
	{
		[SerializeField]
		private EffectType effectType;

		public EffectType EffectType { get { return effectType; } private set { } }
		
		private Vector2 baseScale;
		private Vector2 targetScale;
		private SpriteRenderer spriteRenderer;
		private float scaleDuration;
		private float fadeDuration;

		private void Awake()
		{
			baseScale = transform.localScale;
			spriteRenderer = GetComponent<SpriteRenderer>();
		}
		
		public void Init(float scaleMultiplier, float scaleDuration, float fadeDuration) 
		{
			transform.localScale = baseScale;
			gameObject.SetActive(false);
			targetScale = baseScale * scaleMultiplier;
			this.scaleDuration = scaleDuration;
			this.fadeDuration = fadeDuration;
		}
		
		public void Apply() 
		{
			gameObject.SetActive(true);
			transform.localScale = baseScale;
			Color spriteColor = spriteRenderer.color;
			spriteColor.a = 1;
			spriteRenderer.color = spriteColor;
			transform.DOScale(targetScale, scaleDuration).OnComplete(() =>
			{
				spriteRenderer.DOFade(0, fadeDuration).OnComplete(() =>
				{
					gameObject.SetActive(false);
				});
			});
		}
	}
}
