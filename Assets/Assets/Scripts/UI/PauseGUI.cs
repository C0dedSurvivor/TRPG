using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseGUI : MonoBehaviour
{
    public GameObject pauseLandingScreen;
    public GameObject playerInfoScreen;
    public GameObject playerSkillScreen;
    public GridInventoryGUI equipInv;
    public GearTurner outerGear;
    public GameObject[] playerButtons = new GameObject[4];
    public Image[] playerEquipment = new Image[8];

    public static bool paused = false;

    public static int playerID;
    //0 = none, 1 = landing, 2 = player, 3 = skills, 4 = inventory
    private int loadedMenu = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Battle.battleState == BattleState.None)
        {
            if (loadedMenu == 0)
            {
                loadedMenu = 1;
                Cursor.lockState = CursorLockMode.None;
                pauseLandingScreen.SetActive(true);
                paused = true;
            }
            else if (loadedMenu == 1)
            {
                BackToGame();
            }
            else if (loadedMenu == 2 || loadedMenu == 4)
            {
                BackToLanding();
                equipInv.Close();
            }
            else if (loadedMenu == 3 && GetComponentInParent<SkillTreeGUI>().failedSkillUnlock.activeSelf != true)
            {
                BackToPlayer();
            }
        }
    }

    public void BackToGame()
    {
        loadedMenu = 0;
        Cursor.lockState = CursorLockMode.Locked;
        pauseLandingScreen.SetActive(false);
        paused = false;
    }

    public void BackToLanding()
    {
        loadedMenu = 1;
        playerInfoScreen.SetActive(false);
    }

    public void ToPlayerSelection()
    {
        foreach(GameObject p in playerButtons)
        {
            p.SetActive(true);
        }
        outerGear.frozen = true;
    }

    public void ClosePlayerSelection()
    {
        foreach (GameObject p in playerButtons)
        {
            p.SetActive(false);
        }
    }

    public void BackToPlayer()
    {
        loadedMenu = 2;
        UpdatePlayerEquipped();
        playerSkillScreen.SetActive(false);
    }

    public void OpenPlayerScreen(int playerID)
    {
        loadedMenu = 2;
        PauseGUI.playerID = playerID;
        UpdatePlayerEquipped();
        playerInfoScreen.SetActive(true);
    }

    public void UpdatePlayerStatDisplay()
    {
        playerInfoScreen.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].name;
        playerInfoScreen.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = "Atk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveAtk();
        playerInfoScreen.transform.GetChild(1).GetChild(2).GetComponent<Text>().text = "Def: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveDef();
        playerInfoScreen.transform.GetChild(1).GetChild(3).GetComponent<Text>().text = "mAtk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveMAtk();
        playerInfoScreen.transform.GetChild(1).GetChild(4).GetComponent<Text>().text = "mDef: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveMDef();
        playerInfoScreen.transform.GetChild(1).GetChild(5).GetComponent<Text>().text = "Speed: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetMoveSpeed();
        playerInfoScreen.transform.GetChild(1).GetChild(6).GetComponent<Text>().text = "Crit: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveCrit() + "%";
        playerInfoScreen.transform.GetChild(1).GetChild(7).GetComponent<Text>().text = "Health: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].cHealth + "/" + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveMaxHealth();
        playerInfoScreen.transform.GetChild(1).GetChild(8).GetComponent<Text>().text = "Equipped Skills: \n1) " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[0] + "\n2) " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[1] + "\n3) " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[2];
    }

    public void UpdatePlayerEquipped()
    {
        for (int i = 0; i < 8; i++)
        {
            Debug.Log(GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEquipped(i));
            if (GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEquipped(i) != null)
                playerEquipment[i].overrideSprite = Resources.Load<Sprite>("Images/ItemIcons/" + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEquipped(i));
            else
                playerEquipment[i].overrideSprite = null;
        }
        UpdatePlayerStatDisplay();
    }

    public void OpenSkillScreen()
    {
        loadedMenu = 3;
        gameObject.GetComponent<SkillTreeGUI>().OpenSkillMenu(playerID);
        playerSkillScreen.SetActive(true);
    }
}
