using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSceneGUI : MonoBehaviour
{
    public GameObject titleScreen;
    public GameObject optionsMenu;
    public GameObject saveSelect;

    // Initializes all of the storages and registries
    void Awake()
    {
        Registry.FillRegistry();
        GameStorage.FillStorage();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Load in the save directory to check for any game saves
    }

    public void OpenTitleScreen()
    {
        titleScreen.SetActive(true);
        optionsMenu.SetActive(false);
        saveSelect.SetActive(false);
    }

    public void OpenSaveSelect()
    {
        titleScreen.SetActive(false);
        optionsMenu.SetActive(false);
        saveSelect.SetActive(true);
    }

    public void OpenOptions()
    {
        titleScreen.SetActive(false);
        optionsMenu.SetActive(true);
        saveSelect.SetActive(false);
    }

    public void LoadSave(int slot)
    {
        Inventory.LoadInventory(slot);
        GameStorage.LoadSaveData(slot);
    }

    /// <summary>
    /// Exits the game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
