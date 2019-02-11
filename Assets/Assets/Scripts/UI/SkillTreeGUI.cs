using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//this is for later, for unlocking and equipping skills

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

    private List<Button> skillButtons;
    private List<Button> skillTreeButtons;
    private List<Image> UILines;

    //The ID of the player whose skill trees we are looking at
    private int playerID;

    //Which skill is selected to be equipped in the quick switcher
    private int selectedSkill;

    //The buffer space between the skill buttons
    // **MODIFIABLE BUT MIGHT BREAK, EDIT AT YOUR OWN RISK**
    private int buttonXSpacing = 100;
    private int buttonYSpacing = 70;

    //Used to tell if the mouse is over a skill button
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    // Use this for initialization
    void Start () {
        skillButtons = new List<Button>();
        skillTreeButtons = new List<Button>();
        UILines = new List<Image>();
        displayWindow.transform.position = new Vector3(242.5f, 210, 0);

        Debug.Log(Screen.width + "|" + Screen.height);

        buttonXSpacing = (int)(buttonXSpacing * Screen.width / 930.0f);
        buttonYSpacing = (int)(buttonYSpacing * Screen.height / 470.0f);

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

        //Raycast using the Graphics Raycaster and mouse click position
        m_Raycaster.Raycast(m_PointerEventData, results);

        bool overSkill = false;
        //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
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
        if (Input.GetKeyDown(KeyCode.Escape) && GetComponentInParent<SkillTreeGUI>().failedSkillUnlock.activeSelf == true)
            AcknowledgeFailedSkillUnlock();
        if (Input.GetKeyDown(KeyCode.Escape) && GetComponentInParent<SkillTreeGUI>().failedSkillUnlock.activeSelf == true)
            CancelQuickSwitch();
    }

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


            //X is jsut barely off, y is pretty far off


            skillTreeButtons.Add(Instantiate(skillTreeButtonPrefab, new Vector3((i - 1) * (size.x / (2140.0f / Screen.width)) - (Screen.width / 2 - 185 / (2140.0f / Screen.width)), Screen.height / 2 - (185 / (1080.0f / Screen.height) - (size.y / (1080.0f / Screen.height))), 0) + skillWindow.transform.position, Quaternion.Euler(Vector3.zero), skillWindow.transform));
            int j = tree;
            skillTreeButtons[skillTreeButtons.Count - 1].GetComponentInChildren<Text>().text = j + "";
            skillTreeButtons[skillTreeButtons.Count - 1].onClick.AddListener(delegate { ChangeShownSkillTree(j); });
        }
        currentSkillTree = GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList.Keys.ElementAt(0);
        ChangeShownSkillTree(currentSkillTree);
    }

    public void ChangeShownSkillTree(int skillTree)
    {
        foreach (Button b in skillTreeButtons)
        {
            if (b.GetComponentInChildren<Text>().text == "" + currentSkillTree)
            {
                //resets the color of the previous skill tree's selection button
                var colors = b.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.white;
                b.colors = colors;
            }

            if (b.GetComponentInChildren<Text>().text == "" + skillTree)
            {
                //changes the color of the new skill tree's selection button to symbolize that it is selected
                var colors2 = b.colors;
                colors2.normalColor = colors2.disabledColor;
                colors2.highlightedColor = colors2.disabledColor;
                b.colors = colors2;
            }
        }

        currentSkillTree = skillTree;

        //deletes all current buttons and lines
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
        while (madeButtons.Count < GameStorage.skillTreeList[skillTree].Count) {
            //make a list of all buttons that need to be generated this round
            List<int> halfMadeButtons = new List<int>();
            for (int i = 1; i <= GameStorage.skillTreeList[skillTree].Count; i++)
            {
                if(!madeButtons.ContainsKey(i))
                {
                    bool valid = true;
                    foreach(int j in GameStorage.skillTreeList[skillTree][i].dependencies)
                    {
                        if (!madeButtons.ContainsKey(j))
                            valid = false;
                    }
                    if (valid)
                        halfMadeButtons.Add(i);
                }
            }
            //find where to place all of the buttons and place them
            foreach(int i in halfMadeButtons)
            {
                float pos = 0;
                foreach(int j in GameStorage.skillTreeList[skillTree][i].dependencies)
                {
                    pos += madeButtons[j].y;
                }
                Debug.Log(pos + " Depends " + GameStorage.skillTreeList[skillTree][i].dependencies.Count);
                if (GameStorage.skillTreeList[skillTree][i].dependencies.Count > 1)
                {
                    pos /= GameStorage.skillTreeList[skillTree][i].dependencies.Count;
                }
                Debug.Log(pos);
                //normalizes the position of skills that need to be between other skills
                if(halfMadeButtons.Count % 2 == 0)
                {
                    if(pos % buttonYSpacing != Mathf.Sign(pos) * (buttonYSpacing / 2.0f) && pos % buttonYSpacing != 0)
                    {
                        pos = buttonYSpacing * (Mathf.RoundToInt(pos / buttonYSpacing)) + Mathf.Sign(pos) * (buttonYSpacing / 2.0f);
                    }
                }
                else
                {
                    if (pos % buttonYSpacing != 0 && pos % buttonYSpacing != Mathf.Sign(pos) * (buttonYSpacing / 2.0f))
                    {
                        pos = buttonYSpacing * (Mathf.RoundToInt(pos / buttonYSpacing));
                    }
                }
                Debug.Log(pos);
                float displacement = 0;
                if(halfMadeButtons.Count % 2 == 0)
                    displacement = (buttonYSpacing / 2.0f);
                bool conflictionSwitch = true;
                while (madeButtons.ContainsValue(new Vector2(xPos, pos + displacement)))
                {
                    if(conflictionSwitch)
                        displacement *= -1;
                    if (displacement >= 0)
                         displacement += buttonYSpacing;
                    else if(!conflictionSwitch)
                        displacement -= buttonYSpacing;
                    if (madeButtons.ContainsValue(new Vector2(xPos, pos + displacement))) {
                        int impactedSkill = -1;
                        foreach (int key in madeButtons.Keys)
                        {
                            if (madeButtons[key] == new Vector2(xPos, pos + displacement))
                                impactedSkill = key;
                        }
                        if (!CompareLists(GameStorage.skillTreeList[skillTree][impactedSkill].dependencies, GameStorage.skillTreeList[skillTree][i].dependencies))
                        {
                            foreach (int key in madeButtons.Keys)
                            {
                                if (madeButtons[key] == new Vector2(xPos, pos - displacement))
                                    impactedSkill = key;
                            }
                            if (!madeButtons.ContainsValue(new Vector2(xPos, pos - displacement)) || !CompareLists(GameStorage.skillTreeList[skillTree][impactedSkill].dependencies, GameStorage.skillTreeList[skillTree][i].dependencies))
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
            xPos += Mathf.RoundToInt(buttonXSpacing);
        }
        displayWindow.GetComponent<LayoutElement>().minWidth = xPos;
        displayWindow.GetComponent<LayoutElement>().minHeight = maxY - minY;
        displayWindow.transform.localPosition = Vector2.zero;
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
                foreach (int toCheck in GameStorage.skillTreeList[skillTree][id].dependencies)
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
            skillButtons[skillButtons.Count - 1].image.overrideSprite = Resources.Load<Sprite>("Images/SkillIcons/" + GameStorage.skillTreeList[skillTree][id].name);
            //Debug.Log(skillButtons[skillButtons.Count - 1].transform.position + "|" + skillWindow.transform.position + "|" + displayWindow.transform.position);
        }
        //renders the lines between the skills
        foreach (int key in madeButtons.Keys)
        {
            foreach (int j in GameStorage.skillTreeList[skillTree][key].dependencies)
            {
                Vector3 differenceVector = (madeButtons[key] - madeButtons[j]) * (new Vector2(2140.0f / Screen.width, 1080.0f / Screen.height));

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
        //if(displayWindow.GetComponent<LayoutElement>().minWidth > 600)
        //    displayWindow.transform.localPosition = new Vector3(displayWindow.GetComponent<LayoutElement>().minWidth * 0.2f, 0, 0);
    }

    public void MouseOverSkill(int skill, int unlockState)
    {
        skillInfo.SetActive(true);
        skillInfo.transform.position = Input.mousePosition + new Vector3(2, -2, 0);
        //if (skillInfo.transform.localPosition.y < 0 && Mathf.Abs(skillInfo.transform.localPosition.y) + skillInfo.GetComponent<VerticalLayoutGroup>().preferredHeight > Screen.height / 2)
        //    skillInfo.transform.position = new Vector3(skillInfo.transform.position.x, Screen.height / 2 - skillInfo.GetComponent<VerticalLayoutGroup>().preferredHeight / 2, skillInfo.transform.position.z);
        skillInfo.transform.GetChild(0).GetComponent<Text>().text = GameStorage.skillTreeList[currentSkillTree][skill].name;
        string type = "";
        if (GameStorage.skillTreeList[currentSkillTree][skill].partList.OfType<DamagePart>().FirstOrDefault() != null)
            type += "Damage";
        if (GameStorage.skillTreeList[currentSkillTree][skill].partList.OfType<HealingPart>().FirstOrDefault() != null) {
            if (type != "")
                type += ", ";
            type += "Heal";
        }
        if (GameStorage.skillTreeList[currentSkillTree][skill].partList.OfType<StatChangePart>().FirstOrDefault() != null)
        {
            if (type != "")
                type += ", ";
            type += "Stat Change";
        }
        if (GameStorage.skillTreeList[currentSkillTree][skill].partList.OfType<StatusEffectPart>().FirstOrDefault() != null)
        {
            if (type != "")
                type += ", ";
            type += "Status Effect";
        }
        skillInfo.transform.GetChild(1).GetComponent<Text>().text = "Skill Type: " + type;
        skillInfo.transform.GetChild(2).GetComponent<Text>().text = "Range: " + GameStorage.skillTreeList[currentSkillTree][skill].targettingRange;
        //1 = self, 2 = enemy, 3 = ally, 4 = passive, 5 = anywhere
        if (GameStorage.skillTreeList[currentSkillTree][skill].targetType == 1)
            skillInfo.transform.GetChild(3).GetComponent<Text>().text = "Target Type: Caster";
        else if (GameStorage.skillTreeList[currentSkillTree][skill].targetType == 2)
            skillInfo.transform.GetChild(3).GetComponent<Text>().text = "Target Type: Enemy";
        else if (GameStorage.skillTreeList[currentSkillTree][skill].targetType == 3)
            skillInfo.transform.GetChild(3).GetComponent<Text>().text = "Target Type: Ally";
        else if (GameStorage.skillTreeList[currentSkillTree][skill].targetType == 4)
            skillInfo.transform.GetChild(3).GetComponent<Text>().text = "Passive";
        else if (GameStorage.skillTreeList[currentSkillTree][skill].targetType == 5)
            skillInfo.transform.GetChild(3).GetComponent<Text>().text = "Target Type: Anywhere";
        skillInfo.transform.GetChild(4).GetComponent<Text>().text = "AOE: " + GameStorage.skillTreeList[currentSkillTree][skill].xRange + "x" + GameStorage.skillTreeList[currentSkillTree][skill].yRange;
        if (GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList[currentSkillTree][skill].unlocked)
        {
            skillInfo.transform.GetChild(5).GetComponent<Text>().text = "Unlocked";
        }
        else
        {
            bool unlockable = true;
            List<int> needed = new List<int>();
            foreach (int toCheck in GameStorage.skillTreeList[currentSkillTree][skill].dependencies)
            {
                if (!GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList[currentSkillTree][toCheck].unlocked)
                {
                    unlockable = false;
                    needed.Add(toCheck);
                }
            }
            if (unlockable)
            {
                skillInfo.transform.GetChild(5).GetComponent<Text>().text = "Unlockable\nUnlock cost: " + GameStorage.skillTreeList[currentSkillTree][skill].unlockCost;
            }
            else
            {
                skillInfo.transform.GetChild(5).GetComponent<Text>().text = "Not Unlockable.\nNeeded Skills: ";
                skillInfo.transform.GetChild(5).GetComponent<Text>().text += needed[0];
                for (int i = 1; i < needed.Count; i++)
                {
                    skillInfo.transform.GetChild(5).GetComponent<Text>().text += ", " + needed[i];
                }
            }
        }
    }

    public void MouseLeaveSkill()
    {
        skillInfo.SetActive(false);
    }

    private bool CompareLists(List<int> list1, List<int> list2)
    {
        var firstNotSecond = list1.Except(list2).ToList();
        var secondNotFirst = list2.Except(list1).ToList();

        return !firstNotSecond.Any() && !secondNotFirst.Any();
    }

    public void SkillInteraction(int skillID)
    {
        if (GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList[currentSkillTree][skillID].unlocked)
        {
            selectedSkill = skillID;
            quickSkillSwitcher.SetActive(true);
            quickSkillSwitcher.GetComponentInChildren<Text>().text = "Equip " + GameStorage.skillTreeList[currentSkillTree][skillID].name + " for battle?";
        }
        else
        {
            TryUnlockSkill(skillID);
        }
    }

    public void TryUnlockSkill(int skillID)
    {
        string errorMessage = "This skill is not unlockable yet. You need to unlock ";
        bool unlockable = true;
        List<int> needed = new List<int>();
        foreach (int toCheck in GameStorage.skillTreeList[currentSkillTree][skillID].dependencies)
        {
            if (!GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillTreeList[currentSkillTree][toCheck].unlocked)
            {
                unlockable = false;
                needed.Add(toCheck);
            }
        }
        if(needed.Count > 0)
        {
            errorMessage += needed[0];
            for(int i = 1; i < needed.Count - 1; i++)
            {
                errorMessage += ", " + needed[i];
            }
            if(needed.Count > 1)
                errorMessage += " and " + needed[needed.Count - 1];
        }
        errorMessage += " first.";
        if(unlockable && GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].SkillPoints < GameStorage.skillTreeList[currentSkillTree][skillID].unlockCost)
        {
            errorMessage = "You do not have enough skill points to unlock this skill.";
            unlockable = false;
        }
            
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
                    foreach (int toCheck in GameStorage.skillTreeList[currentSkillTree][b.GetComponent<SkillUnlockButton>().skillID].dependencies)
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
            GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].SavePlayer();
        }
        else
        {
            failedSkillUnlock.SetActive(true);
            failedSkillUnlock.GetComponentInChildren<Text>().text = errorMessage;
        }
    }

    public void AcknowledgeFailedSkillUnlock()
    {
        GetComponentInParent<SkillTreeGUI>().failedSkillUnlock.SetActive(false);
    }

    public void SwapQuickSkill(int place)
    {
        GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].skillQuickList[place] = new Vector2Int(currentSkillTree, selectedSkill);
        GameStorage.playerMasterList[GameStorage.activePlayerList[playerID]].SavePlayer();
        GetComponentInParent<SkillTreeGUI>().quickSkillSwitcher.SetActive(false);
    }

    public void CancelQuickSwitch()
    {
        GetComponentInParent<SkillTreeGUI>().quickSkillSwitcher.SetActive(false);
    }
}
