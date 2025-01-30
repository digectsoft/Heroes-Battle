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
	public class BasePanel : MonoBehaviour
	{
		[Header("Options")]
		[SerializeField]
		protected PanelType type;
		[SerializeField]
		protected GameObject content;
		[SerializeField]
		protected float showDelay = 0f;
		[SerializeField]
		protected bool pauseGame = true;
		[SerializeField]
		protected bool showAtPause = true;

		[Header("Scale")]
		[SerializeField]
		protected float startScale = 1.2f;
		[SerializeField]
		protected float endScale = 1f;
		[SerializeField]
		protected float scaleTime = 0.1f;

		public PanelType PanelType { get { return type; } }
		public bool Visible { get { return gameObject.activeSelf; } }

		private bool initialized;
		private float timeScale = 1f;

		protected virtual void Init()
		{
			if (!initialized)
			{
				timeScale = Time.timeScale;
				initialized = true;
			}
		}

		public void Show()
		{
			Init();
			if (!gameObject.activeSelf)
			{
				InitPanel();
				Pause(true);
				DOTween.Sequence().AppendInterval(showDelay).OnComplete(() => 
				{
					gameObject.SetActive(true);
					content.transform.localScale = new Vector3(startScale, startScale, startScale);
					content.transform.DOScale(endScale, scaleTime).OnComplete(() => 
					{
						ShowComplete();
					}).SetUpdate(showAtPause);
				}).SetUpdate(showAtPause);
			}
		}

		public void Hide()
		{
			Init();
			if (gameObject.activeSelf)
			{
				content.transform.DOScale(startScale, scaleTime).OnComplete(() => 
				{
					Pause(false);
					HideComplete();
					gameObject.SetActive(false);
				}).SetUpdate(showAtPause);
			}
		}

		private void Pause(bool pause) 
		{
			if (pauseGame) 
			{
				Time.timeScale = pause ? 0 : timeScale;
			}
		}

		protected virtual void InitPanel() {}

		protected virtual void ShowComplete() {}

		protected virtual void HideComplete() {}
	}
}
