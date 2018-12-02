using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour {
    //all of the ui pieces that make up a battle menu
    public GameObject BattleScript;
    public Button toggleaEther;
    public Button toggleDanger;
    public Button swap;
    public Button confirmMove;

    public GameObject PlayerStatPos;
    public Text pName;
    public Text pAtk;
    public Text pDef;
    public Text pMAtk;
    public Text pMDef;
    public Text pSpeed;
    public Text pCrit;
    public Text pHealth;

    public GameObject EnemyStatPos;
    public Text eName;
    public Text eAtk;
    public Text eDef;
    public Text eMAtk;
    public Text eMDef;
    public Text eSpeed;
    public Text eCrit;
    public Text eHealth;

    public Text damageNote1;
    public Text damageNote2;

    public Button confirmAttack;
    public Button denyAttack;

    public Button quickSkill1;
    public Button quickSkill2;
    public Button quickSkill3;

    void Start()
    {
        pName.gameObject.SetActive(false);
        pAtk.gameObject.SetActive(false);
        pDef.gameObject.SetActive(false);
        pMAtk.gameObject.SetActive(false);
        pMDef.gameObject.SetActive(false);
        pSpeed.gameObject.SetActive(false);
        pCrit.gameObject.SetActive(false);
        pHealth.gameObject.SetActive(false);

        eName.gameObject.SetActive(false);
        eAtk.gameObject.SetActive(false);
        eDef.gameObject.SetActive(false);
        eMAtk.gameObject.SetActive(false);
        eMDef.gameObject.SetActive(false);
        eSpeed.gameObject.SetActive(false);
        eCrit.gameObject.SetActive(false);
        eHealth.gameObject.SetActive(false);

        damageNote1.gameObject.SetActive(false);
        damageNote2.gameObject.SetActive(false);
        confirmAttack.gameObject.SetActive(false);
        denyAttack.gameObject.SetActive(false);

        quickSkill1.gameObject.SetActive(false);
        quickSkill2.gameObject.SetActive(false);
        quickSkill3.gameObject.SetActive(false);

        swap.gameObject.SetActive(false);
        confirmMove.gameObject.SetActive(false);
        toggleDanger.gameObject.SetActive(false);
        toggleaEther.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        //if a battle is actually going on
        if(BattleScript.GetComponent<Battle>().matchPart.CompareTo("") != 0)
        {
            toggleDanger.gameObject.SetActive(true);
            toggleaEther.gameObject.SetActive(true);

            if (BattleScript.GetComponent<Battle>().canSwap)
            {
                swap.gameObject.SetActive(true);
            }
            else
            {
                swap.gameObject.SetActive(false);
            }

            //if a player has a movement spot selected
            if (BattleScript.GetComponent<Battle>().moveMarker.activeSelf)
            {
                confirmMove.gameObject.SetActive(true);
            }
            else
            {
                confirmMove.gameObject.SetActive(false);
            }

            //if a player is selected
            if (BattleScript.GetComponent<Battle>().selectedPlayer != -1)
            {
                pName.gameObject.SetActive(true);
                pAtk.text = "Atk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]].GetEffectiveAtk();
                pAtk.gameObject.SetActive(true);
                pDef.text = "Def: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]].GetEffectiveDef();
                pDef.gameObject.SetActive(true);
                pMAtk.text = "mAtk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]].GetEffectiveMAtk();
                pMAtk.gameObject.SetActive(true);
                pMDef.text = "mDef: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]].GetEffectiveMDef();
                pMDef.gameObject.SetActive(true);
                pSpeed.text = "Speed: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]].GetMoveSpeed();
                pSpeed.gameObject.SetActive(true);
                pCrit.text = "Crit: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]].GetEffectiveCrit() + "%";
                pCrit.gameObject.SetActive(true);
                pHealth.text = "Health: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]].cHealth + "/" + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]].mHealth;
                pHealth.gameObject.SetActive(true);
                if (!GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]].moved)
                {
                    quickSkill1.gameObject.SetActive(true);
                    quickSkill2.gameObject.SetActive(true);
                    quickSkill3.gameObject.SetActive(true);
                }

                //if both an enemy and a player are selected
                if (BattleScript.GetComponent<Battle>().selectedEnemy != -1)
                {
                    PlayerStatPos.GetComponent<RectTransform>().localPosition = new Vector3(265, 70, 0);
                    EnemyStatPos.GetComponent<RectTransform>().localPosition = new Vector3(415, 70, 0);
                    float ed = (BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy].cHealth - BattleScript.GetComponent<Battle>().GetDamageValues(GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]], BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy])) / (BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy].mHealth * 1.0f);
                    if (ed >= 0.75)
                    {
                        damageNote1.text = "Enemy will probably just tank my hit.";
                    }
                    else if (ed >= 0.5)
                    {
                        damageNote1.text = "I should be able to do some damage.";
                    }
                    else if (ed >= 0.25)
                    {
                        damageNote1.text = "I can do some hefty damage.";
                    }
                    else
                    {
                        damageNote1.text = "This enemy will not live for much longer.";
                    }
                    float pd = (GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]].cHealth - BattleScript.GetComponent<Battle>().GetDamageValues(BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy], GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]])) / (GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]].mHealth * 1.0f);
                    if (Registry.WeaponTypeRegistry[Registry.WeaponRegistry[GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.GetComponent<Battle>().selectedPlayer]].equippedWeapon].weaponType].ranged != Registry.WeaponTypeRegistry[Registry.WeaponRegistry[BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy].equippedWeapon].weaponType].ranged)
                    {
                        damageNote2.text = "And they shouldn't be able to counterattack me from this range.";
                    }
                    else if (pd >= 0.5)
                    {
                        damageNote2.text = "I should be fine if they counterattack.";
                    }
                    else if (pd >= 0.25)
                    {
                        damageNote2.text = "Their counterattack will definitely hurt though.";
                    }
                    else
                    {
                        damageNote2.text = "May the gods help me if they survive though.";
                    }
                    damageNote1.gameObject.SetActive(true);
                    damageNote2.gameObject.SetActive(true);
                    confirmAttack.gameObject.SetActive(true);
                    denyAttack.gameObject.SetActive(true);
                }
                else
                {
                    PlayerStatPos.GetComponent<RectTransform>().localPosition = new Vector3(340, 70, 0);
                    EnemyStatPos.GetComponent<RectTransform>().localPosition = new Vector3(340, 70, 0);
                    damageNote1.gameObject.SetActive(false);
                    damageNote2.gameObject.SetActive(false);
                    confirmAttack.gameObject.SetActive(false);
                    denyAttack.gameObject.SetActive(false);
                }
            }
            else
            {
                pName.gameObject.SetActive(false);
                pAtk.gameObject.SetActive(false);
                pDef.gameObject.SetActive(false);
                pMAtk.gameObject.SetActive(false);
                pMDef.gameObject.SetActive(false);
                pSpeed.gameObject.SetActive(false);
                pCrit.gameObject.SetActive(false);
                pHealth.gameObject.SetActive(false);
                quickSkill1.gameObject.SetActive(false);
                quickSkill2.gameObject.SetActive(false);
                quickSkill3.gameObject.SetActive(false);
            }

            //if an enemy is selected
            if (BattleScript.GetComponent<Battle>().selectedEnemy != -1)
            {
                eName.gameObject.SetActive(true);
                eAtk.text = "Atk: " + BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy].GetEffectiveAtk();
                eAtk.gameObject.SetActive(true);
                eDef.text = "Def: " + BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy].GetEffectiveDef();
                eDef.gameObject.SetActive(true);
                eMAtk.text = "mAtk: " + BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy].GetEffectiveMAtk();
                eMAtk.gameObject.SetActive(true);
                eMDef.text = "mDef: " + BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy].GetEffectiveMDef();
                eMDef.gameObject.SetActive(true);
                eSpeed.text = "Speed: " + BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy].GetMoveSpeed();
                eSpeed.gameObject.SetActive(true);
                eCrit.text = "Crit: " + BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy].GetEffectiveCrit() + "%";
                eCrit.gameObject.SetActive(true);
                eHealth.text = "Health: " + BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy].cHealth + "/" + BattleScript.GetComponent<Battle>().enemyList[BattleScript.GetComponent<Battle>().selectedEnemy].mHealth;
                eHealth.gameObject.SetActive(true);
            }
            else
            {
                eName.gameObject.SetActive(false);
                eAtk.gameObject.SetActive(false);
                eDef.gameObject.SetActive(false);
                eMAtk.gameObject.SetActive(false);
                eMDef.gameObject.SetActive(false);
                eSpeed.gameObject.SetActive(false);
                eCrit.gameObject.SetActive(false);
                eHealth.gameObject.SetActive(false);
            }

            //if it's damage time
            if (BattleScript.GetComponent<Battle>().matchPart.CompareTo("attack") == 0)
            {
                denyAttack.gameObject.SetActive(true);
            }
            else
            {
                
                denyAttack.gameObject.SetActive(false);
                confirmAttack.gameObject.SetActive(false);
            }
        }
        else
        {
            toggleDanger.gameObject.SetActive(false);
            toggleaEther.gameObject.SetActive(false);
        }
    }
}
