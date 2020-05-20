using UnityEngine;

public class TitleSceneGUI : MonoBehaviour
{
    public GameObject titleScreen;
    public GameObject optionsMenu;
    public GameObject saveSelect;

    /// <summary>
    /// Loads the information that doesn't care about save files
    /// </summary>
    void Awake()
    {
        Registry.FillRegistry();
    }

    /// <summary>
    /// Load in the save directory to check for any game saves
    /// </summary>
    void Start()
    {
    }

    /// <summary>
    /// Shows the title screen
    /// </summary>
    public void OpenTitleScreen()
    {
        titleScreen.SetActive(true);
        optionsMenu.SetActive(false);
        saveSelect.SetActive(false);
    }

    /// <summary>
    /// Shows the save select menu
    /// </summary>
    public void OpenSaveSelect()
    {
        titleScreen.SetActive(false);
        optionsMenu.SetActive(false);
        saveSelect.SetActive(true);
    }

    /// <summary>
    /// Shows the options menu
    /// </summary>
    public void OpenOptions()
    {
        titleScreen.SetActive(false);
        optionsMenu.SetActive(true);
        saveSelect.SetActive(false);
    }

    /// <summary>
    /// Loads the data for a given save slot
    /// </summary>
    /// <param name="slot">Slot ID to load</param>
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
