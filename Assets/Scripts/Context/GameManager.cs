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
using DG.Tweening;
using digectsoft;
using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
	private IServerAdapter serverAdapter;
	private bool initialized = false;
	private bool inAction = false;
	private Character player;
	private Character enemy;

	[Inject]
	public void Init(IServerAdapter serverAdapter, 
					 [Inject(Id = CharacterType.PLAYER)] Character player,
					 [Inject(Id = CharacterType.ENEMY)] Character enemy)
	{
		this.serverAdapter = serverAdapter;
		this.player = player;
		this.enemy = enemy;
	}
	
	async void Start()
	{
		DOTween.Init(true, true, LogBehaviour.Verbose);
		Dictionary<CharacterType, CharacterValue> characterValues = await serverAdapter.Init();
		initialized = true;
		player.Init(characterValues[CharacterType.PLAYER].health);
		enemy.Init(characterValues[CharacterType.ENEMY].health);
	}

	void Update()
	{
	}
	
	public async UniTask Action(EffectType actionType)
	{
		if (!initialized || inAction) 
		{
			return;
		}
		inAction = true;
		Debug.Log("GameManager: " + actionType);
		EffectAction effectAction = await serverAdapter.Action(actionType);
		inAction = false;
		Debug.Log(effectAction.type);
		switch (effectAction.type) 
		{
			case EffectType.ATTACK:
				player.Attack();
				enemy.Damage(effectAction.value.action);
			break;
		}
	}
	
	public void ActionListener()
	{
		
	}
}
