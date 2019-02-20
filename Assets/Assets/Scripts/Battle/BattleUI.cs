using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour {
    //All of the ui pieces that make up a battle menu
    public Battle BattleScript;
    public Button toggleaEther;
    public Button toggleDanger;
    public Button swap;
    public Button confirmMove;
    public Button endPlayerTurn;

    //Text on Children: 0 = name, 1 = attack, 2 = defense, 3 = mAttack, 4 = mDefense, 5 = speed, 6 = crit, 7 = health
    public GameObject playerStats;

    //Text on Children: 0 = name, 1 = attack, 2 = defense, 3 = mAttack, 4 = mDefense, 5 = speed, 6 = crit, 7 = health
    public GameObject enemyStats;

    public Text damageNote1;
    public Text damageNote2;

    public Button confirmAttack;
    public Button denyAttack;

    public Button quickSkill1;
    public Button quickSkill2;
    public Button quickSkill3;

    void Start()
    {
        playerStats.gameObject.SetActive(false);

        enemyStats.gameObject.SetActive(false);

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
        endPlayerTurn.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        //If a battle is actually going on
        if(Battle.battleState != BattleState.None)
        {
            toggleDanger.gameObject.SetActive(true);
            toggleaEther.gameObject.SetActive(true);

            if (BattleScript.canSwap)
            {
                swap.gameObject.SetActive(true);
            }
            else
            {
                swap.gameObject.SetActive(false);
            }

            //If a player has a movement spot selected
            if (BattleScript.moveMarker != null && BattleScript.moveMarker.activeSelf)
            {
                confirmMove.gameObject.SetActive(true);
            }
            else
            {
                confirmMove.gameObject.SetActive(false);
            }

            //If a player is selected
            if (BattleScript.selectedPlayer != -1)
            {
                playerStats.SetActive(true);
                Text[] stats = playerStats.GetComponentsInChildren<Text>();
                stats[1].text = "Atk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]].GetEffectiveAtk();
                stats[2].text = "Def: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]].GetEffectiveDef();
                stats[3].text = "mAtk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]].GetEffectiveMAtk();
                stats[4].text = "mDef: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]].GetEffectiveMDef();
                stats[5].text = "Speed: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]].GetMoveSpeed();
                stats[6].text = "Crit: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]].GetEffectiveCrit() + "%";
                stats[7].text = "Health: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]].cHealth + "/" + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]].mHealth;
                if (Battle.battleState != BattleState.Swap && !GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]].moved)
                {
                    quickSkill1.gameObject.SetActive(true);
                    quickSkill2.gameObject.SetActive(true);
                    quickSkill3.gameObject.SetActive(true);
                }

                //If both an enemy and a player are selected
                if (BattleScript.selectedEnemy != -1)
                {
                    //If it is actually an enemy
                    if (BattleScript.selectedEnemy < BattleScript.enemyList.Length){
                        float ed = (BattleScript.enemyList[BattleScript.selectedEnemy].cHealth - BattleScript.GetDamageValues(GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]], BattleScript.enemyList[BattleScript.selectedEnemy])) / (BattleScript.enemyList[BattleScript.selectedEnemy].mHealth * 1.0f);
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
                        float pd = (GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]].cHealth - BattleScript.GetDamageValues(BattleScript.enemyList[BattleScript.selectedEnemy], GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]])) / (GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]].mHealth * 1.0f);
                        if (Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedPlayer]].equippedWeapon]).subType].ranged != Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[BattleScript.enemyList[BattleScript.selectedEnemy].equippedWeapon]).subType].ranged)
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
                        damageNote2.gameObject.SetActive(true);
                    }
                    //If it is actually a second player
                    else
                    {
                        damageNote1.text = "Healing is fun.";
                    }
                    damageNote1.gameObject.SetActive(true);
                    confirmAttack.gameObject.SetActive(true);
                    denyAttack.gameObject.SetActive(true);
                }
            }
            else
            {
                playerStats.gameObject.SetActive(false);
                quickSkill1.gameObject.SetActive(false);
                quickSkill2.gameObject.SetActive(false);
                quickSkill3.gameObject.SetActive(false);
                damageNote1.gameObject.SetActive(false);
                damageNote2.gameObject.SetActive(false);
                confirmAttack.gameObject.SetActive(false);
                denyAttack.gameObject.SetActive(false);
            }

            //If an enemy is selected
            if (BattleScript.selectedEnemy != -1)
            {
                enemyStats.SetActive(true);
                Text[] stats = enemyStats.GetComponentsInChildren<Text>();
                //If it is actually an enemy
                if (BattleScript.selectedEnemy < BattleScript.enemyList.Length)
                {
                    stats[1].text = "Atk: " + BattleScript.enemyList[BattleScript.selectedEnemy].GetEffectiveAtk();
                    stats[2].text = "Def: " + BattleScript.enemyList[BattleScript.selectedEnemy].GetEffectiveDef();
                    stats[3].text = "mAtk: " + BattleScript.enemyList[BattleScript.selectedEnemy].GetEffectiveMAtk();
                    stats[4].text = "mDef: " + BattleScript.enemyList[BattleScript.selectedEnemy].GetEffectiveMDef();
                    stats[5].text = "Speed: " + BattleScript.enemyList[BattleScript.selectedEnemy].GetMoveSpeed();
                    stats[6].text = "Crit: " + BattleScript.enemyList[BattleScript.selectedEnemy].GetEffectiveCrit() + "%";
                    stats[7].text = "Health: " + BattleScript.enemyList[BattleScript.selectedEnemy].cHealth + "/" + BattleScript.enemyList[BattleScript.selectedEnemy].mHealth;
                }
                //If it is actually a second player
                else
                {
                    stats[1].text = "Atk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedEnemy - BattleScript.enemyList.Length]].GetEffectiveAtk();
                    stats[2].text = "Def: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedEnemy - BattleScript.enemyList.Length]].GetEffectiveDef();
                    stats[3].text = "mAtk: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedEnemy - BattleScript.enemyList.Length]].GetEffectiveMAtk();
                    stats[4].text = "mDef: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedEnemy - BattleScript.enemyList.Length]].GetEffectiveMDef();
                    stats[5].text = "Speed: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedEnemy - BattleScript.enemyList.Length]].GetMoveSpeed();
                    stats[6].text = "Crit: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedEnemy - BattleScript.enemyList.Length]].GetEffectiveCrit() + "%";
                    stats[7].text = "Health: " + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedEnemy - BattleScript.enemyList.Length]].cHealth + "/" + GameStorage.playerMasterList[GameStorage.activePlayerList[BattleScript.selectedEnemy - BattleScript.enemyList.Length]].mHealth;
                }
            }
            else
            {
                enemyStats.gameObject.SetActive(false);
                damageNote1.gameObject.SetActive(false);
                damageNote2.gameObject.SetActive(false);
                confirmAttack.gameObject.SetActive(false);
                denyAttack.gameObject.SetActive(false);
            }

            if (Battle.battleState == BattleState.Player)
            {
                endPlayerTurn.gameObject.SetActive(true);
            }
            else
            {
                endPlayerTurn.gameObject.SetActive(false);
            }

            //If it's damage time
            if (Battle.battleState == BattleState.Attack)
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
            playerStats.gameObject.SetActive(false);

            enemyStats.gameObject.SetActive(false);

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
    }
}
