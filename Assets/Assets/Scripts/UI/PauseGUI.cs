using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseGUI : MonoBehaviour
{
    public GameObject pauseLandingScreen;
    public GameObject playerInfoScreen;
    public GameObject playerSkillScreen;
    public GearTurner outerGear;
    public GameObject[] playerButtons = new GameObject[4];

    public static bool paused = false;

    private int playerID;
    //0 = none, 1 = landing, 2 = player, 3 = skills, 4 = inventory
    private int loadedMenu = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Battle.matchPart == "")
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
        playerInfoScreen.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].name;
        playerInfoScreen.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = "Atk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveAtk();
        playerInfoScreen.transform.GetChild(1).GetChild(2).GetComponent<Text>().text = "Def: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveDef();
        playerInfoScreen.transform.GetChild(1).GetChild(3).GetComponent<Text>().text = "mAtk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveMAtk();
        playerInfoScreen.transform.GetChild(1).GetChild(4).GetComponent<Text>().text = "mDef: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveMDef();
        playerInfoScreen.transform.GetChild(1).GetChild(5).GetComponent<Text>().text = "Speed: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetMoveSpeed();
        playerInfoScreen.transform.GetChild(1).GetChild(6).GetComponent<Text>().text = "Crit: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveCrit() + "%";
        playerInfoScreen.transform.GetChild(1).GetChild(7).GetComponent<Text>().text = "Health: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].cHealth + "/" + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].mHealth;
        playerInfoScreen.transform.GetChild(1).GetChild(8).GetComponent<Text>().text = "Equipped Skills: \n1) " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[0] + "\n2) " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[1] + "\n3) " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[2];
        playerSkillScreen.SetActive(false);
    }

    public void OpenPlayerScreen(int playerID)
    {
        loadedMenu = 2;
        this.playerID = playerID;
        playerInfoScreen.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].name;
        playerInfoScreen.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = "Atk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveAtk();
        playerInfoScreen.transform.GetChild(1).GetChild(2).GetComponent<Text>().text = "Def: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveDef();
        playerInfoScreen.transform.GetChild(1).GetChild(3).GetComponent<Text>().text = "mAtk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveMAtk();
        playerInfoScreen.transform.GetChild(1).GetChild(4).GetComponent<Text>().text = "mDef: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveMDef();
        playerInfoScreen.transform.GetChild(1).GetChild(5).GetComponent<Text>().text = "Speed: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetMoveSpeed();
        playerInfoScreen.transform.GetChild(1).GetChild(6).GetComponent<Text>().text = "Crit: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].GetEffectiveCrit() + "%";
        playerInfoScreen.transform.GetChild(1).GetChild(7).GetComponent<Text>().text = "Health: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].cHealth + "/" + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].mHealth;
        playerInfoScreen.transform.GetChild(1).GetChild(8).GetComponent<Text>().text = "Equipped Skills: \n1) " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[0] + "\n2) " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[1] + "\n3) " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[2];
        playerInfoScreen.SetActive(true);
    }

    public void OpenSkillScreen()
    {
        loadedMenu = 3;
        gameObject.GetComponent<SkillTreeGUI>().OpenSkillMenu(playerID);
        playerSkillScreen.SetActive(true);
    }
}
