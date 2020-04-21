using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Creates and controls the UI for unlocking and equipping skills
/// </summary>
public class SkillTreeGUI : MonoBehaviour {

    public Button skillButtonPrefab;
    public Button skillTreeButtonPrefab;
    public Image lineRendererPrefab;

    public GameObject displayWindow;
    public GameObject skillWindow;
    public GameObject skillInfo;
    public GameObject failedSkillUnlock;
    public GameObject quickSkillSwitcher;
    public Text skillPointCounter;
    public float UILineWidth;
    private int currentSkillTree;

    private List<Button> skillButtons = new List<Button>();
    [SerializeField]
    private List<Button> skillTreeButtons = new List<Button>();
    private List<Image> UILines = new List<Image>();

    //The ID of the player whose skill trees we are looking at
    private int playerID;

    //Which skill is selected to be equipped in the quick switcher
    private int selectedSkill;

    //The buffer space between the skill buttons
    // **MODIFIABLE BUT MIGHT BREAK, EDIT AT YOUR OWN RISK**
    private int buttonXSpacing = 300;
    private int buttonYSpacing = 255;

    //Used to tell if the mouse is over a skill button
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    //If this window cannot be traversed out of
    public bool holdingFocus => failedSkillUnlock.activeSelf || quickSkillSwitcher.activeSelf;

    // Use this for initialization
    void Start () {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();
    }

    void Update()
    {
        //Set up the new Pointer Event
        m_PointerEventData = new PointerEventData(m_EventSystem);
        //Set the Pointer Event Position to that of the mouse position
        m_PointerEventData.position = Input.mousePosition;

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse position
        m_Raycaster.Raycast(m_PointerEventData, results);

        bool overSkill = false;
        //Checks to see if the player is mousing over a skill
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<SkillUnlockButton>() != null)
            {
                overSkill = true;
                MouseOverSkill(result.gameObject.GetComponent<SkillUnlockButton>().skillID, result.gameObject.GetComponent<SkillUnlockButton>().GetUnlockState());
            }
        }
        if (!overSkill)
            MouseLeaveSkill();
        if ((InputManager.BoundKeyPressed(PlayerKeybinds.UIConfirm) || InputManager.BoundKeyPressed(PlayerKeybinds.UIBack)) && GetComponentInParent<SkillTreeGUI>().failedSkillUnlock.activeSelf == true)
            AcknowledgeFailedSkillUnlock();
        if (InputManager.BoundKeyPressed(PlayerKeybinds.UIBack) && GetComponentInParent<SkillTreeGUI>().quickSkillSwitcher.activeSelf == true)
            CancelQuickSwitch();
    }

    /// <summary>
    /// Opens the skill trees of the selected player, generates the buttons to switch skill trees and the visuals for the first skill tree
    /// </summary>
    /// <param name="playerID"></param>
    public void OpenSkillMenu(int playerID)
    {
        this.playerID = playerID;
        skillPointCounter.text = "Skill Points: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].SkillPoints;
        //Clears the remnants of a previous player's skill tree list
        foreach (Button button in skillTreeButtons)
        {
            Destroy(button.gameObject);
        }
        skillTreeButtons.Clear();
        //Generates buttons off of the new player's skill tree list
        int i = 0;
        foreach(int tree in GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList.Keys)
        {
            i++;
            Vector2 size = skillTreeButtonPrefab.GetComponent<RectTransform>().sizeDelta;
            skillTreeButtons.Add(Instantiate(skillTreeButtonPrefab, new Vector3((i * size.x - 860) * (Screen.width / 1920.0f), 445 * (Screen.height / 1080.0f), 0) + skillWindow.transform.position, Quaternion.Euler(Vector3.zero), skillWindow.transform));
            int j = tree;
            skillTreeButtons[skillTreeButtons.Count - 1].GetComponentInChildren<Text>().text = "" + j;
            skillTreeButtons[skillTreeButtons.Count - 1].name = "" + j;
            skillTreeButtons[skillTreeButtons.Count - 1].onClick.AddListener(delegate { ChangeShownSkillTree(j); });
        }
        Debug.Log("Amt of skill trees: " + skillTreeButtons.Count);
        Debug.Log(GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList.Count);
        currentSkillTree = GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList.Keys.ElementAt(0);
        ChangeShownSkillTree(currentSkillTree);
    }

    /// <summary>
    /// Destroys the currently displayed visuals and generates new ones based on the given tree
    /// </summary>
    /// <param name="skillTree">The ID of the tree to generate visuals from</param>
    public void ChangeShownSkillTree(int skillTree)
    {
        Debug.Log("Former tree: " + currentSkillTree + " New Skill tree: " + skillTree + " Amt of skill trees: " + skillTreeButtons.Count);
        foreach (Button b in skillTreeButtons)
        {
            Debug.Log("Button name: " + b.name);
            if (b.name == "" + currentSkillTree)
            {
                Debug.Log("Changing former");
                //Resets the color of the previous skill tree's selection button
                var colors = b.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.white;
                colors.selectedColor = Color.white;
                b.colors = colors;
            }

            if (b.name == "" + skillTree)
            {
                Debug.Log("Changing former");
                //Changes the color of the new skill tree's selection button to symbolize that it is selected
                var colors2 = b.colors;
                colors2.normalColor = colors2.disabledColor;
                colors2.highlightedColor = colors2.disabledColor;
                colors2.selectedColor = colors2.disabledColor;
                b.colors = colors2;
            }
        }

        currentSkillTree = skillTree;

        //Deletes all current buttons and lines
        foreach (Transform child in displayWindow.transform)
        {
            Destroy(child.gameObject);
        }
        skillButtons.Clear();
        UILines.Clear();

        float minY = float.MaxValue;
        float maxY = float.MinValue;
        int xPos = 0;
        Dictionary<int, Vector2> madeButtons = new Dictionary<int, Vector2>();
        while (madeButtons.Count < Registry.SpellTreeRegistry[skillTree].spells.Count) {
            //Makes a list of all buttons that need to be generated this round
            List<int> halfMadeButtons = new List<int>();
            for (int i = 0; i < Registry.SpellTreeRegistry[skillTree].spells.Count; i++)
            {
                if(!madeButtons.ContainsKey(i))
                {
                    bool valid = true;
                    foreach(int j in Registry.SpellTreeRegistry[skillTree][i].dependencies)
                    {
                        if (!madeButtons.ContainsKey(j))
                            valid = false;
                    }
                    if (valid)
                        halfMadeButtons.Add(i);
                }
            }
            Vector2Int modifiedButtonSpacing = new Vector2Int(Mathf.RoundToInt(buttonXSpacing * Screen.width / 1920.0f), Mathf.RoundToInt(buttonYSpacing * Screen.height / 1080.0f));
            //Find where all of the buttons belong
            foreach(int i in halfMadeButtons)
            {
                float pos = 0;
                foreach(int j in Registry.SpellTreeRegistry[skillTree][i].dependencies)
                {
                    pos += madeButtons[j].y;
                }
                Debug.Log(pos + " Depends " + Registry.SpellTreeRegistry[skillTree][i].dependencies.Count);
                if (Registry.SpellTreeRegistry[skillTree][i].dependencies.Count > 1)
                {
                    pos /= Registry.SpellTreeRegistry[skillTree][i].dependencies.Count;
                }
                Debug.Log(pos);
                //Normalizes the position of skills that have multiple dependencies
                if(halfMadeButtons.Count % 2 == 0)
                {
                    if(pos % modifiedButtonSpacing.y != Mathf.Sign(pos) * (modifiedButtonSpacing.y / 2.0f) && pos % modifiedButtonSpacing.y != 0)
                    {
                        pos = modifiedButtonSpacing.y * (Mathf.RoundToInt(pos / modifiedButtonSpacing.y)) + Mathf.Sign(pos) * (modifiedButtonSpacing.y / 2.0f);
                    }
                }
                else
                {
                    if (pos % modifiedButtonSpacing.y != 0 && pos % modifiedButtonSpacing.y != Mathf.Sign(pos) * (modifiedButtonSpacing.y / 2.0f))
                    {
                        pos = modifiedButtonSpacing.y * (Mathf.RoundToInt(pos / modifiedButtonSpacing.y));
                    }
                }
                Debug.Log(pos);
                float displacement = 0;
                if(halfMadeButtons.Count % 2 == 0)
                    displacement = (modifiedButtonSpacing.y / 2.0f);
                bool conflictionSwitch = true;
                //Makes sure none of them overlap with another skill
                while (madeButtons.ContainsValue(new Vector2(xPos, pos + displacement)))
                {
                    if(conflictionSwitch)
                        displacement *= -1;
                    if (displacement >= 0)
                         displacement += modifiedButtonSpacing.y;
                    else if(!conflictionSwitch)
                        displacement -= modifiedButtonSpacing.y;
                    if (madeButtons.ContainsValue(new Vector2(xPos, pos + displacement))) {
                        int impactedSkill = -1;
                        foreach (int key in madeButtons.Keys)
                        {
                            if (madeButtons[key] == new Vector2(xPos, pos + displacement))
                                impactedSkill = key;
                        }
                        if (!CompareLists(Registry.SpellTreeRegistry[skillTree][impactedSkill].dependencies, Registry.SpellTreeRegistry[skillTree][i].dependencies))
                        {
                            foreach (int key in madeButtons.Keys)
                            {
                                if (madeButtons[key] == new Vector2(xPos, pos - displacement))
                                    impactedSkill = key;
                            }
                            if (!madeButtons.ContainsValue(new Vector2(xPos, pos - displacement)) || !CompareLists(Registry.SpellTreeRegistry[skillTree][impactedSkill].dependencies, Registry.SpellTreeRegistry[skillTree][i].dependencies))
                            {
                                conflictionSwitch = false;
                                displacement *= -1;
                            }
                        }
                    }
                }
                Debug.Log(pos + " " + displacement);
                if (pos + displacement > maxY)
                    maxY = pos + displacement;
                if (pos + displacement < minY)
                    minY = pos + displacement;
                madeButtons.Add(i, new Vector2(xPos, pos + displacement));
            }
            xPos += Mathf.RoundToInt(modifiedButtonSpacing.x);
        }
        displayWindow.GetComponent<LayoutElement>().minWidth = xPos;
        displayWindow.GetComponent<LayoutElement>().minHeight = maxY - minY;
        displayWindow.transform.localPosition = Vector2.zero;
        //Generates the visuals based on the previously generated data
        foreach(int id in madeButtons.Keys)
        {
            skillButtons.Add(Instantiate(skillButtonPrefab, displayWindow.transform));
            skillButtons[skillButtons.Count - 1].transform.position = new Vector3(madeButtons[id].x - Mathf.Clamp(displayWindow.GetComponent<LayoutElement>().minWidth * 0.5f, 300, float.MaxValue) + 50, madeButtons[id].y, 0) + skillWindow.transform.position;
            skillButtons[skillButtons.Count - 1].GetComponent<SkillUnlockButton>().skillID = id;
            skillButtons[skillButtons.Count - 1].GetComponent<SkillUnlockButton>().guiController = this;
            if (GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList[skillTree][id].unlocked)
            {
                skillButtons[skillButtons.Count - 1].image.color = Color.grey;
            }
            else
            {
                bool unlockable = true;
                foreach (int toCheck in Registry.SpellTreeRegistry[skillTree][id].dependencies)
                {
                    if (!GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList[skillTree][toCheck].unlocked)
                    {
                        unlockable = false;
                    }
                }
                if (!unlockable)
                {
                    skillButtons[skillButtons.Count - 1].image.color = new Color(1, 0.4f, 0.4f, 1);
                }
                else
                {
                    skillButtons[skillButtons.Count - 1].image.color = Color.green;
                }
            }
            skillButtons[skillButtons.Count - 1].image.sprite = Resources.Load<Sprite>("Images/SkillIcons/" + Registry.SpellTreeRegistry[skillTree][id].name);
            skillButtons[skillButtons.Count - 1].image.overrideSprite = Resources.Load<Sprite>("Images/SkillIcons/" + Registry.SpellTreeRegistry[skillTree][id].name);
            //Debug.Log(skillButtons[skillButtons.Count - 1].transform.position + "|" + skillWindow.transform.position + "|" + displayWindow.transform.position);
        }
        //Renders the dependency lines between the skills
        foreach (int key in madeButtons.Keys)
        {
            foreach (int j in Registry.SpellTreeRegistry[skillTree][key].dependencies)
            {
                Vector3 differenceVector = (madeButtons[key] - madeButtons[j]) * (new Vector2(1920.0f / Screen.width, 1080.0f / Screen.height));

                UILines.Add(Instantiate(lineRendererPrefab, displayWindow.transform));
                UILines[UILines.Count - 1].rectTransform.sizeDelta = new Vector2(differenceVector.magnitude, UILineWidth);
                UILines[UILines.Count - 1].rectTransform.position = madeButtons[j] + new Vector2(skillWindow.transform.position.x - Mathf.Clamp(displayWindow.GetComponent<LayoutElement>().minWidth * 0.5f, 300, float.MaxValue) + 50, skillWindow.transform.position.y);
                float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
                UILines[UILines.Count - 1].rectTransform.rotation = Quaternion.Euler(0, 0, angle);
                UILines[UILines.Count - 1].transform.SetAsFirstSibling();
            }
        }
        Debug.Log("normalized: " + skillWindow.GetComponent<RectTransform>().sizeDelta);
        displayWindow.GetComponent<LayoutElement>().minWidth *= (2000.0f / Screen.width);
        displayWindow.GetComponent<LayoutElement>().minHeight *= (1200.0f / Screen.height);
        if(displayWindow.GetComponent<LayoutElement>().minWidth > Screen.width)
            displayWindow.transform.localPosition = new Vector3(displayWindow.GetComponent<LayoutElement>().minWidth / 7.5f, 0, 0);
    }

    /// <summary>
    /// Displays the information about a skill when the player mouses over it
    /// </summary>
    /// <param name="skill">ID of the skill to display</param>
    /// <param name="unlockState">Whether that skill is unlocked, unlockable or not unlockable (0-2)</param>
    public void MouseOverSkill(int skill, int unlockState)
    {
        skillInfo.SetActive(true);
        skillInfo.transform.position = Input.mousePosition + new Vector3(2, -2, 0);
        //if (skillInfo.transform.localPosition.y < 0 && Mathf.Abs(skillInfo.transform.localPosition.y) + skillInfo.GetComponent<VerticalLayoutGroup>().preferredHeight > Screen.height / 2)
        //    skillInfo.transform.position = new Vector3(skillInfo.transform.position.x, Screen.height / 2 - skillInfo.GetComponent<VerticalLayoutGroup>().preferredHeight / 2, skillInfo.transform.position.z);
        skillInfo.transform.GetChild(0).GetComponent<Text>().text = Registry.SpellTreeRegistry[currentSkillTree][skill].name;
        string type = "";
        if (Registry.SpellTreeRegistry[currentSkillTree][skill].partList.OfType<DamagePart>().FirstOrDefault() != null)
            type += "Damage";
        if (Registry.SpellTreeRegistry[currentSkillTree][skill].partList.OfType<HealingPart>().FirstOrDefault() != null) {
            if (type != "")
                type += ", ";
            type += "Heal";
        }
        if (Registry.SpellTreeRegistry[currentSkillTree][skill].partList.OfType<StatChangePart>().FirstOrDefault() != null)
        {
            if (type != "")
                type += ", ";
            type += "Stat Change";
        }
        if (Registry.SpellTreeRegistry[currentSkillTree][skill].partList.OfType<StatusEffectPart>().FirstOrDefault() != null)
        {
            if (type != "")
                type += ", ";
            type += "Status Effect";
        }
        skillInfo.transform.GetChild(1).GetComponent<Text>().text = "Skill Type: " + type;
        skillInfo.transform.GetChild(1).GetComponent<Text>().text += "\nRange: " + Registry.SpellTreeRegistry[currentSkillTree][skill].targettingRange;
        //1 = self, 2 = enemy, 3 = ally, 4 = passive, 5 = anywhere
        if (Registry.SpellTreeRegistry[currentSkillTree][skill].targetType == TargettingType.Self)
            skillInfo.transform.GetChild(1).GetComponent<Text>().text += "\nTarget Type: Caster";
        else if (Registry.SpellTreeRegistry[currentSkillTree][skill].targetType == TargettingType.Ally)
            skillInfo.transform.GetChild(1).GetComponent<Text>().text += "\nTarget Type: Ally";
        else if (Registry.SpellTreeRegistry[currentSkillTree][skill].targetType == TargettingType.Enemy)
            skillInfo.transform.GetChild(1).GetComponent<Text>().text += "\nTarget Type: Enemy";
        else if (Registry.SpellTreeRegistry[currentSkillTree][skill].targetType == TargettingType.AllAllies)
            skillInfo.transform.GetChild(1).GetComponent<Text>().text += "\nTarget Type: All Allies";
        else if (Registry.SpellTreeRegistry[currentSkillTree][skill].targetType == TargettingType.AllEnemies)
            skillInfo.transform.GetChild(1).GetComponent<Text>().text += "\nTarget Type: All Enemies";
        else if (Registry.SpellTreeRegistry[currentSkillTree][skill].targetType == TargettingType.AllAlliesNotSelf)
            skillInfo.transform.GetChild(1).GetComponent<Text>().text += "\nTarget Type: All Allies not including Caster";
        else if (Registry.SpellTreeRegistry[currentSkillTree][skill].targetType == TargettingType.AllInRange)
            skillInfo.transform.GetChild(1).GetComponent<Text>().text += "\nTarget Type: Anywhere";
        skillInfo.transform.GetChild(1).GetComponent<Text>().text += "\nAOE: " + Registry.SpellTreeRegistry[currentSkillTree][skill].xRange + "x" + Registry.SpellTreeRegistry[currentSkillTree][skill].yRange;
        if (GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList[currentSkillTree][skill].unlocked)
        {
            skillInfo.transform.GetChild(1).GetComponent<Text>().text += "\nUnlocked";
        }
        else
        {
            bool unlockable = true;
            List<int> needed = new List<int>();
            foreach (int toCheck in Registry.SpellTreeRegistry[currentSkillTree][skill].dependencies)
            {
                if (!GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList[currentSkillTree][toCheck].unlocked)
                {
                    unlockable = false;
                    needed.Add(toCheck);
                }
            }
            if (unlockable)
            {
                skillInfo.transform.GetChild(1).GetComponent<Text>().text += "\nUnlockable\nUnlock cost: " + Registry.SpellTreeRegistry[currentSkillTree][skill].unlockCost;
            }
            else
            {
                skillInfo.transform.GetChild(1).GetComponent<Text>().text += "\nNot Unlockable.\nNeeded Skills: ";
                skillInfo.transform.GetChild(1).GetComponent<Text>().text += needed[0];
                for (int i = 1; i < needed.Count; i++)
                {
                    skillInfo.transform.GetChild(1).GetComponent<Text>().text += ", " + needed[i];
                }
            }
        }
        RectTransform infoTransform = skillInfo.GetComponent<RectTransform>();
        if (infoTransform.position.x + infoTransform.sizeDelta.x * infoTransform.lossyScale.x > Screen.width)
            skillInfo.transform.position = new Vector3(Screen.width - infoTransform.sizeDelta.x * infoTransform.lossyScale.x, skillInfo.transform.position.y, skillInfo.transform.position.z);
        if (infoTransform.position.y - infoTransform.sizeDelta.y * infoTransform.lossyScale.y < 0)
            skillInfo.transform.position = new Vector3(skillInfo.transform.position.x, infoTransform.sizeDelta.y * infoTransform.lossyScale.y, skillInfo.transform.position.z);
    }

    /// <summary>
    /// Hides the skill info screen when a player stops mousing voer it
    /// </summary>
    public void MouseLeaveSkill()
    {
        skillInfo.SetActive(false);
    }

    /// <summary>
    /// Checks to see if two skill dependency lists contain the same information
    /// </summary>
    /// <param name="list1">The first skill dependency list</param>
    /// <param name="list2">The second skill dependency list</param>
    /// <returns>True if they contain the same information</returns>
    private bool CompareLists(List<int> list1, List<int> list2)
    {
        var firstNotSecond = list1.Except(list2).ToList();
        var secondNotFirst = list2.Except(list1).ToList();

        return !firstNotSecond.Any() && !secondNotFirst.Any();
    }

    /// <summary>
    /// When a player clicks on a skill button
    /// </summary>
    /// <param name="skillID">The ID of the skill they clicked on</param>
    public void SkillInteraction(int skillID)
    {
        //If it is unlocked, open the equipping pop-up
        if (GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList[currentSkillTree][skillID].unlocked)
        {
            selectedSkill = skillID;
            quickSkillSwitcher.SetActive(true);
            quickSkillSwitcher.GetComponentInChildren<Text>().text = "Equip " + Registry.SpellTreeRegistry[currentSkillTree][skillID].name + " for battle?";
        }
        //If not, try to unlock it
        else
        {
            TryUnlockSkill(skillID);
        }
    }

    /// <summary>
    /// Tries to unlock a skill when the player clicks on it and updates everything accordingly
    /// </summary>
    /// <param name="skillID">The ID of the skill they want to unlock</param>
    public void TryUnlockSkill(int skillID)
    {
        string errorMessage = "This skill is not unlockable yet. You need to unlock ";
        bool unlockable = true;
        List<int> needed = new List<int>();
        //Checks to make sure all of the dependencies for that skill have been unlocked
        foreach (int toCheck in Registry.SpellTreeRegistry[currentSkillTree][skillID].dependencies)
        {
            if (!GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList[currentSkillTree][toCheck].unlocked)
            {
                needed.Add(toCheck);
            }
        }
        //If it cannot be unlocked due to a skill it depends on not being unlocked, tells the player what skills are needed
        if(needed.Count > 0)
        {
            unlockable = false;
            errorMessage += needed[0];
            for(int i = 1; i < needed.Count - 1; i++)
            {
                errorMessage += ", " + needed[i];
            }
            if(needed.Count > 1)
                errorMessage += " and " + needed[needed.Count - 1];
        }
        errorMessage += " first.";
        //If it cannot be unlocked due to a lack of skill points, notifies the player
        if (unlockable && GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].SkillPoints < Registry.SpellTreeRegistry[currentSkillTree][skillID].unlockCost)
        {
            errorMessage = "You do not have enough skill points to unlock this skill.";
            unlockable = false;
        }
        //If it can be unlocked, unlock it and update the colors of it and the other skill buttons
        if (unlockable)
        {
            GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].UnlockSkill(currentSkillTree, skillID);
            foreach(Button b in skillButtons)
            {
                if (GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList[currentSkillTree][b.GetComponent<SkillUnlockButton>().skillID].unlocked)
                {
                    b.image.color = Color.grey;
                }
                else
                {
                    unlockable = true;
                    foreach (int toCheck in Registry.SpellTreeRegistry[currentSkillTree][b.GetComponent<SkillUnlockButton>().skillID].dependencies)
                    {
                        if (!GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList[currentSkillTree][toCheck].unlocked)
                        {
                            unlockable = false;
                        }
                    }
                    if (unlockable)
                    {
                        b.image.color = Color.green;
                    }
                    else
                    {
                        b.image.color = new Color(1, 0.4f, 0.4f, 1);
                    }
                }
            }
            skillPointCounter.text = "Skill Points: " + GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].SkillPoints;
        }
        else
        {
            //Shows the failed skill unlock window and updates its message with the correct error
            failedSkillUnlock.SetActive(true);
            failedSkillUnlock.GetComponentInChildren<Text>().text = errorMessage;
        }
    }

    /// <summary>
    /// When the player acknowledges the unlock failed
    /// </summary>
    public void AcknowledgeFailedSkillUnlock()
    {
        GetComponentInParent<SkillTreeGUI>().failedSkillUnlock.SetActive(false);
    }

    /// <summary>
    /// Equips an unlocked skill for use in battle
    /// </summary>
    /// <param name="place">Which slot to equip the skill to</param>
    public void SwapQuickSkill(int place)
    {
        GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[place] = new Vector2Int(currentSkillTree, selectedSkill);
        GetComponentInParent<SkillTreeGUI>().quickSkillSwitcher.SetActive(false);
    }

    /// <summary>
    /// Cancels equipping a skill
    /// </summary>
    public void CancelQuickSwitch()
    {
        GetComponentInParent<SkillTreeGUI>().quickSkillSwitcher.SetActive(false);
    }
}
