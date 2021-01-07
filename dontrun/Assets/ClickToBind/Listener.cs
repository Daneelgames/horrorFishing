using UnityEngine;
using System.Collections;

/// <summary>
/// This script is included for the sake of the demo scene 
/// and is not required to use Click to Bind - feel free to delete
/// </summary>
public class Listener : MonoBehaviour {

	//Script used as a example of how to use keyBindingManager

	public GameObject dash;
	public GameObject run;
	public GameObject crouch;
	public GameObject interaction;
	public GameObject reload;
	public GameObject switchWeapon;
	public GameObject useTool;
	public GameObject throwTool;
	public GameObject selfAttack;
	public GameObject aim;
	public GameObject fire1;
	public GameObject quests;
	public GameObject drop;
	
	// Update is called once per frame
	void Update () {

		if(KeyBindingManager.GetKeyDown(KeyAction.Dash))
		{
			dash.SetActive(!dash.activeSelf);
		}
		if(KeyBindingManager.GetKeyDown(KeyAction.Run))
		{
			run.SetActive(!run.activeSelf);
		}
		if(KeyBindingManager.GetKeyDown(KeyAction.Crouch))
		{
			crouch.SetActive(!crouch.activeSelf);
		}
		if(KeyBindingManager.GetKeyDown(KeyAction.Interaction))
		{
			interaction.SetActive(!interaction.activeSelf);
		}
		if(KeyBindingManager.GetKeyDown(KeyAction.Reload))
		{
			reload.SetActive(!reload.activeSelf);
		}
		if(KeyBindingManager.GetKeyDown(KeyAction.SwitchWeapon))
		{
			switchWeapon.SetActive(!switchWeapon.activeSelf);
		}
		if(KeyBindingManager.GetKeyDown(KeyAction.UseTool))
		{
			useTool.SetActive(!useTool.activeSelf);
		}
		if(KeyBindingManager.GetKeyDown(KeyAction.ThrowTool))
		{
			throwTool.SetActive(!throwTool.activeSelf);
		}
		if(KeyBindingManager.GetKeyDown(KeyAction.SelfAttack))
		{
			selfAttack.SetActive(!selfAttack.activeSelf);
		}
		if(KeyBindingManager.GetKeyDown(KeyAction.Aim))
		{
			aim.SetActive(!aim.activeSelf);
		}
		if(KeyBindingManager.GetKeyDown(KeyAction.Fire1))
		{
			fire1.SetActive(!fire1.activeSelf);
		}
		if(KeyBindingManager.GetKeyDown(KeyAction.Quests))
		{
			quests.SetActive(!quests.activeSelf);
		}
		if(KeyBindingManager.GetKeyDown(KeyAction.Drop))
		{
			drop.SetActive(!drop.activeSelf);
		}
	}
}
