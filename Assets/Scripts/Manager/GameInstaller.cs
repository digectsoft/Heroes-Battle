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
using UnityEngine;
using Zenject;

namespace digectsoft 
{
	public class GameInstaller : MonoInstaller
	{
		[Header("Model")]
		[SerializeField]
		private MockServerAdapter serverAdapter;
		[SerializeField]
		private GameManager gameManager;
		
		[Header("View")]
		[SerializeField]
		private ActionAdapter actionAdapter;
		[SerializeField]
		private Character player;
		[SerializeField]
		private Character enemy;
		
		[Header("Presenter")]
		[SerializeField]
		private ActionPresenter actionPresenter;

		public override void InstallBindings()
		{
			Container.Bind<IServerAdapter>().To<MockServerAdapter>().FromInstance(serverAdapter).AsSingle();
			//Container.Bind<IServerAdapter>().To<NetworkServerAdapter>().AsSingle(); //Implement NetworkServerAdapter for a backend server.
			Container.Bind<GameManager>().FromInstance(gameManager).AsSingle();
			Container.Bind<ActionAdapter>().FromInstance(actionAdapter).AsSingle();
			Container.Bind<Character>().WithId(CharacterType.PLAYER).FromInstance(player).AsCached();
			Container.Bind<Character>().WithId(CharacterType.ENEMY).FromInstance(enemy).AsCached();
			Container.Bind<ActionPresenter>().FromInstance(actionPresenter).AsSingle();
		}
	}
}