using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseGUI : MonoBehaviour
{
    //Where the player first ends up when they enter the pause menu, the navigation hub
    public GameObject pauseLandingScreen;
    //Displays information about a selected pawn, allows for equipping and de-equipping of items and access of that pawn's skill tree
    public GameObject playerInfoScreen;
    //Displays the skill trees for a given pawn
    public GameObject playerSkillScreen;
    //
    public GridInventoryGUI equipInv;
    //The larger gear on the landing screen, used for secondary menu displays
    public GearTurner outerGear;
    public GameObject[] playerButtons = new GameObject[4];
    /// <summary>
    /// Displays the currently equipped items
    /// </summary>
    public Image[] playerEquipment = new Image[8];

    public static bool paused = false;
    
    /// <summary>
    /// The ID of the player whose information is currently displayed
    /// Only applicable in the player section of the pause menu
    /// </summary>
    public static int playerID;
    //0 = none, 1 = landing, 2 = player, 3 = skills, 4 = inventory
    private int loadedMenu = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Battle.battleState == BattleState.None)
        {
            //If the player wants to enter the pause menu
            if (loadedMenu == 0)
            {
                loadedMenu = 1;
                Cursor.lockState = CursorLockMode.None;
                pauseLandingScreen.SetActive(true);
                paused = true;
            }
            //If the player wants to exit the pause menu
            else if (loadedMenu == 1)
            {
                BackToGame();
            }
            //If the player wants to return to the landing page
            else if (loadedMenu == 2 || loadedMenu == 4)
            {
                BackToLanding();
                equipInv.Close();
            }
            //If the player wants to go back to the player info screen
            else if (loadedMenu == 3 && GetComponentInParent<SkillTreeGUI>().failedSkillUnlock.activeSelf != true)
            {
                BackToPlayer();
            }
        }
    }

    /// <summary>
    /// Closes the pause menu
    /// </summary>
    public void BackToGame()
    {
        loadedMenu = 0;
        Cursor.lockState = CursorLockMode.Locked;
        pauseLandingScreen.SetActive(false);
        paused = false;
    }

    /// <summary>
    /// Moves back to the landing page
    /// </summary>
    public void BackToLanding()
    {
        loadedMenu = 1;
        playerInfoScreen.SetActive(false);
    }

    /// <summary>
    /// Shows the buttons that allow the player to open the player info screen for a specified pawn
    /// </summary>
    public void ToPlayerSelection()
    {
        foreach(GameObject p in playerButtons)
        {
            p.SetActive(true);
        }
        outerGear.frozen = true;
    }

    /// <summary>
    /// Hides the buttons shown in ToPlayerSelection
    /// </summary>
    public void ClosePlayerSelection()
    {
        foreach (GameObject p in playerButtons)
        {
            p.SetActive(false);
        }
    }

    /// <summary>
    /// Returns to the player info screen
    /// </summary>
    public void BackToPlayer()
    {
        loadedMenu = 2;
        UpdatePlayerEquipped();
        playerSkillScreen.SetActive(false);
    }

    /// <summary>
    /// Opens the player info screen
    /// </summary>
    /// <param name="playerID">What Player ID to grab the info of</param>
    public void OpenPlayerScreen(int playerID)
    {
        loadedMenu = 2;
        PauseGUI.playerID = playerID;
        UpdatePlayerEquipped();
        playerInfoScreen.SetActive(true);
    }

    /// <summary>
    /// Grabs the updated stats of the currently selected pawn
    /// </summary>
    public void UpdatePlayerStatDisplay()
    {
        playerInfoScreen.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].name;
        playerInfoScreen.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = "Atk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveStat(Stats.Attack);
        playerInfoScreen.transform.GetChild(1).GetChild(2).GetComponent<Text>().text = "Def: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveStat(Stats.Defense);
        playerInfoScreen.transform.GetChild(1).GetChild(3).GetComponent<Text>().text = "mAtk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveStat(Stats.MagicAttack);
        playerInfoScreen.transform.GetChild(1).GetChild(4).GetComponent<Text>().text = "mDef: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveStat(Stats.MagicDefense);
        playerInfoScreen.transform.GetChild(1).GetChild(5).GetComponent<Text>().text = "Speed: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveStat(Stats.MaxMove);
        playerInfoScreen.transform.GetChild(1).GetChild(6).GetComponent<Text>().text = "Crit: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveStat(Stats.CritChance) + "%";
        playerInfoScreen.transform.GetChild(1).GetChild(7).GetComponent<Text>().text = "Health: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].cHealth + "/" + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveStat(Stats.MaxHealth);
        playerInfoScreen.transform.GetChild(1).GetChild(8).GetComponent<Text>().text = "Equipped Skills: \n1) " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[0] + "\n2) " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[1] + "\n3) " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[2];
    }

    /// <summary>
    /// Updates the displays for the character's currently equipped items
    /// </summary>
    public void UpdatePlayerEquipped()
    {
        for (int i = 0; i < 8; i++)
        {
            Debug.Log(GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEquipped(i));
            if (GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEquipped(i) != null)
                playerEquipment[i].overrideSprite = Resources.Load<Sprite>("Images/ItemIcons/" + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEquipped(i).Name);
            else
                playerEquipment[i].overrideSprite = null;
        }
        UpdatePlayerStatDisplay();
    }

    /// <summary>
    /// Opens the skill menu for the currently selected player
    /// </summary>
    public void OpenSkillScreen()
    {
        loadedMenu = 3;
        gameObject.GetComponent<SkillTreeGUI>().OpenSkillMenu(playerID);
        playerSkillScreen.SetActive(true);
    }
}
