using UnityEngine;
using UnityEngine.UI;

public class PauseGUI : MonoBehaviour
{
    private enum PauseMenuState
    {
        Closed,
        LandingPage,
        PlayerInspector,
        SkillTreeScreen
    }

    //Where the player first ends up when they enter the pause menu, the navigation hub
    public GameObject pauseLandingScreen;
    //Displays the full inventory on the pause menu landing page
    public PauseInventory pauseInventory;
    //Displays information about a selected pawn, allows for equipping and de-equipping of items and access of that pawn's skill tree
    public GameObject playerInfoScreen;
    //Displays the skill trees for a given pawn
    public GameObject playerSkillScreen;
    //Displays the items able to be equipped to a given slot for a player
    public GridInventoryGUI equipInv;
    //The smaller gear on the landing screen, used for primary menu selection
    public GearTurner innerGear;
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
    private PauseMenuState loadedMenu = PauseMenuState.Closed;

    /// <summary>
    /// Checks for key presses that would transition between menu states
    /// </summary>
    void Update()
    {
        if (Battle.battleState == BattleState.None)
        {
            //If the player wants to open the inventory screen
            if (InputManager.KeybindTriggered(PlayerKeybinds.UIOpenInventory) &&
                (loadedMenu == PauseMenuState.Closed || loadedMenu == PauseMenuState.LandingPage || loadedMenu == PauseMenuState.PlayerInspector ||
                (loadedMenu == PauseMenuState.SkillTreeScreen && !playerSkillScreen.GetComponent<SkillTreeGUI>().holdingFocus)))
            {
                OpenPauseInventory();
            }

            //If the player wants to open the team screen
            if (InputManager.KeybindTriggered(PlayerKeybinds.UIOpenTeamPage) &&
                (loadedMenu == PauseMenuState.Closed || loadedMenu == PauseMenuState.LandingPage || loadedMenu == PauseMenuState.PlayerInspector ||
                (loadedMenu == PauseMenuState.SkillTreeScreen && !playerSkillScreen.GetComponent<SkillTreeGUI>().holdingFocus)))
            {
                ToPlayerSelection();
            }

            //If the player wants to enter the pause menu
            if (InputManager.KeybindTriggered(PlayerKeybinds.UIOpenPause) && loadedMenu == PauseMenuState.Closed)
            {
                OpenPauseMenu();
            }
            else if (InputManager.KeybindTriggered(PlayerKeybinds.UIBack))
            {
                //If the player wants to exit the pause menu
                if (loadedMenu == PauseMenuState.LandingPage)
                {
                    BackToGame();
                }
                //If the player wants to return to the landing page
                else if (loadedMenu == PauseMenuState.PlayerInspector)
                {
                    BackToLanding();
                }
                //If the player wants to go back to the player info screen
                else if (loadedMenu == PauseMenuState.SkillTreeScreen && !playerSkillScreen.GetComponent<SkillTreeGUI>().holdingFocus)
                {
                    ToPlayerScreen(playerID);
                }
            }
        }
    }

    /// <summary>
    /// Shows the pause menu landing screen and unlocks the cursor
    /// </summary>
    public void OpenPauseMenu()
    {
        loadedMenu = PauseMenuState.LandingPage;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseLandingScreen.SetActive(true);
        paused = true;
    }

    /// <summary>
    /// Shows the pause inventory and associated controls and generates the initial view of items
    /// </summary>
    public void OpenPauseInventory()
    {
        if (loadedMenu == PauseMenuState.Closed)
            OpenPauseMenu();
        else if (loadedMenu == PauseMenuState.PlayerInspector)
            BackToLanding();
        else if (loadedMenu == PauseMenuState.SkillTreeScreen)
        {
            playerSkillScreen.SetActive(false);
            BackToLanding();
        }
        loadedMenu = PauseMenuState.LandingPage;
        ClosePlayerSelection();
        innerGear.moveToButton(2);
        pauseInventory.SortAndChangeFilter();
    }

    /// <summary>
    /// Shows the buttons that allow the player to open the player info screen for a specified pawn
    /// </summary>
    public void ToPlayerSelection()
    {
        if (loadedMenu == PauseMenuState.Closed)
            OpenPauseMenu();
        else if (loadedMenu == PauseMenuState.PlayerInspector)
            BackToLanding();
        else if (loadedMenu == PauseMenuState.SkillTreeScreen)
        {
            playerSkillScreen.SetActive(false);
            BackToLanding();
        }
        loadedMenu = PauseMenuState.LandingPage;
        pauseInventory.Close();
        foreach (GameObject p in playerButtons)
        {
            p.SetActive(true);
        }
        innerGear.moveToButton(1);
        outerGear.frozen = true;
    }

    /// <summary>
    /// Opens the player info screen
    /// </summary>
    /// <param name="playerID">What Player ID to grab the info of</param>
    public void ToPlayerScreen(int playerID)
    {
        PauseGUI.playerID = playerID;
        playerSkillScreen.SetActive(false);
        playerInfoScreen.SetActive(true);
        loadedMenu = PauseMenuState.PlayerInspector;
        UpdatePlayerEquipped();
    }

    /// <summary>
    /// Opens the map in large view
    /// </summary>
    public void ToMap()
    {
        innerGear.moveToButton(3);
        ClosePlayerSelection();
        pauseInventory.Close();
    }

    /// <summary>
    /// Opens the enemy dictionary
    /// </summary>
    public void ToEnemyDict()
    {
        innerGear.moveToButton(4);
        ClosePlayerSelection();
        pauseInventory.Close();
    }

    /// <summary>
    /// Turns the gear to the save option and saves
    /// </summary>
    public void ToSave()
    {
        innerGear.moveToButton(5);
        ClosePlayerSelection();
        pauseInventory.Close();
        GameStorage.SaveAll(0);
    }

    /// <summary>
    /// Opens the settings menu
    /// </summary>
    public void ToSettings()
    {
        innerGear.moveToButton(6);
        ClosePlayerSelection();
        pauseInventory.Close();
    }

    /// <summary>
    /// Closes the pause menu
    /// </summary>
    public void BackToGame()
    {
        loadedMenu = PauseMenuState.Closed;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ClosePlayerSelection();
        outerGear.GetComponent<PauseInventory>().Close();
        innerGear.moveToButton(3);
        pauseLandingScreen.SetActive(false);
        paused = false;
    }

    /// <summary>
    /// Moves back to the landing page
    /// </summary>
    public void BackToLanding()
    {
        loadedMenu = PauseMenuState.LandingPage;
        playerInfoScreen.SetActive(false);
        if (equipInv.gameObject.activeSelf)
            equipInv.Close();
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
    /// Grabs the updated stats of the currently selected pawn
    /// <see cref="BattlePawnBase"/>
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
        loadedMenu = PauseMenuState.SkillTreeScreen;
        playerSkillScreen.SetActive(true);
        playerSkillScreen.GetComponent<SkillTreeGUI>().OpenSkillMenu(playerID);
    }

    /// <summary>
    /// Exits the game
    /// 
    /// 
    /// TODO: Add prompt to save when persistence works
    /// 
    /// 
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
