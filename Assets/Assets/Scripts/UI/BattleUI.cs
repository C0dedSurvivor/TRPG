using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public GameObject UIParent;

    public Battle battleController;

    //All of the ui pieces that make up a battle menu
    public GameObject swap;
    public GameObject confirmMove;
    public GameObject endPlayerTurn;
    public GameObject confirmAttack;
    public GameObject denyAttack;

    public GameObject quickSkill1;
    public GameObject quickSkill2;
    public GameObject quickSkill3;

    //Text on Children: 0 = name, 1 = attack, 2 = defense, 3 = mAttack, 4 = mDefense, 5 = speed, 6 = crit, 7 = health
    public GameObject playerStats;

    //Text on Children: 0 = name, 1 = attack, 2 = defense, 3 = mAttack, 4 = mDefense, 5 = speed, 6 = crit, 7 = health
    public GameObject enemyStats;

    public Text damageNote1;
    public Text damageNote2;

    public Text littleInfoSys;

    void Start()
    {
        UIParent.SetActive(false);
        littleInfoSys.text = "";
    }

    public void StartBattle()
    {
        UIParent.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        //If a battle is actually going on
        if (Battle.battleState != BattleState.None)
        {
            swap.SetActive(battleController.canSwap);

            //If a player has a movement spot selected
            confirmMove.SetActive(battleController.moveMarker != null && battleController.moveMarker.activeSelf);

            //If a player is selected
            if (battleController.selectedPlayer != -1)
            {
                playerStats.SetActive(true);
                UpdateStats(battleController.players[battleController.selectedPlayer], playerStats.GetComponentsInChildren<Text>());
                if (Battle.battleState != BattleState.Swap && !battleController.players[battleController.selectedPlayer].moved)
                {
                    quickSkill1.SetActive(true);
                    quickSkill2.SetActive(true);
                    quickSkill3.SetActive(true);
                }
                else
                {
                    quickSkill1.SetActive(false);
                    quickSkill2.SetActive(false);
                    quickSkill3.SetActive(false);
                }

                //If both an enemy and a player are selected
                if (battleController.selectedEnemy != -1)
                {
                    //If it is actually an enemy during an attack
                    if (battleController.selectedEnemy < battleController.enemies.Count && Battle.battleState == BattleState.Attack)
                    {
                        float ed = (battleController.enemies[battleController.selectedEnemy].cHealth - battleController.GetDamageValues(battleController.players[battleController.selectedPlayer], battleController.enemies[battleController.selectedEnemy]).First) / (battleController.enemies[battleController.selectedEnemy].GetEffectiveStat(Stats.MaxHealth) * 1.0f);
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
                        float pd = (battleController.players[battleController.selectedPlayer].cHealth - battleController.GetDamageValues(battleController.enemies[battleController.selectedEnemy], battleController.players[battleController.selectedPlayer]).First) / (battleController.players[battleController.selectedPlayer].GetEffectiveStat(Stats.MaxHealth) * 1.0f);
                        if (battleController.enemies[battleController.selectedEnemy].GetWeaponStatsAtDistance(battleController.players[battleController.selectedPlayer].position - battleController.enemies[battleController.selectedEnemy].position) == null)
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
                    else if (battleController.selectedEnemy >= battleController.enemies.Count)
                    {
                        damageNote1.text = "Healing is fun.";
                    }
                    damageNote1.gameObject.SetActive(true);
                    confirmAttack.SetActive(true);
                }
            }
            else
            {
                playerStats.SetActive(false);
                confirmAttack.SetActive(false);
                quickSkill1.SetActive(false);
                quickSkill2.SetActive(false);
                quickSkill3.SetActive(false);
                damageNote1.gameObject.SetActive(false);
                damageNote2.gameObject.SetActive(false);
            }

            //If an enemy is selected
            if (battleController.selectedEnemy != -1)
            {
                enemyStats.SetActive(true);
                BattleParticipant pawn = null;

                //If it is actually an enemy
                if (battleController.selectedEnemy < battleController.enemies.Count)
                    pawn = battleController.enemies[battleController.selectedEnemy];

                //If it is actually a second player
                else
                    pawn = battleController.players[battleController.selectedEnemy - battleController.enemies.Count];

                UpdateStats(pawn, enemyStats.GetComponentsInChildren<Text>());
            }
            else
            {
                enemyStats.SetActive(false);
                confirmAttack.SetActive(false);
                damageNote1.gameObject.SetActive(false);
                damageNote2.gameObject.SetActive(false);
            }

            endPlayerTurn.SetActive(Battle.battleState == BattleState.Player);

            //If it's damage time
            denyAttack.SetActive(Battle.battleState == BattleState.Attack);
        }
    }

    /// <summary>
    /// Updates the given stat display with the stats of the selected pawn
    /// </summary>
    private void UpdateStats(BattleParticipant pawn, Text[] statDisplays)
    {
        statDisplays[0].text = pawn.name;
        statDisplays[1].text = "Atk: " + pawn.GetEffectiveStat(Stats.Attack);
        statDisplays[2].text = "Def: " + pawn.GetEffectiveStat(Stats.Defense);
        statDisplays[3].text = "mAtk: " + pawn.GetEffectiveStat(Stats.MagicAttack);
        statDisplays[4].text = "mDef: " + pawn.GetEffectiveStat(Stats.MagicDefense);
        statDisplays[5].text = "Speed: " + pawn.GetEffectiveStat(Stats.MaxMove);
        statDisplays[6].text = "Crit: " + pawn.GetEffectiveStat(Stats.CritChance) + "%";
        statDisplays[7].text = "Health: " + pawn.cHealth + "/" + pawn.GetEffectiveStat(Stats.MaxHealth);
    }

    public void EndOfBattle()
    {
        UIParent.SetActive(false);
    }
}
