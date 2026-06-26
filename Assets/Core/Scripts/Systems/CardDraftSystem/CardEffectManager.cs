using UnityEngine;
using System.Collections;
using System;

public class CardEffectManager : MonoBehaviour
{
    public static CardEffectManager Instance;

    [Header("Battle References")]
    public RobotStats playerStats; 
    public RobotStats enemyStats;  
    public CharacterAnimator playerAnim; 
    public CharacterAnimator enemyAnim;

    [HideInInspector] public bool isResolvingEffect = false;

    private ArenaTile selectedBuzzTile = null;

    private void Awake() { Instance = this; }

    public IEnumerator ApplyCardEffect(CardData card, bool skipBuzzTile = false, int forcedCaster = -1)
    {
        isResolvingEffect = true;
        Debug.Log($"<color=cyan>MENGAKTIFKAN JURUS: {card.cardName}</color>");

        bool isPlayerCaster = (forcedCaster == -1) ? (TurnManager.Instance.CurrentPlayerIndex == 0) : (forcedCaster == 0);
        
        RobotStats caster = isPlayerCaster ? playerStats : enemyStats;
        CharacterAnimator casterAnim = isPlayerCaster ? playerAnim : enemyAnim;
        RobotStats target = (card.effectTarget == CardData.TargetSubject.Self) ? caster : (isPlayerCaster ? enemyStats : playerStats);
        CharacterAnimator targetAnim = (card.effectTarget == CardData.TargetSubject.Self) ? casterAnim : (isPlayerCaster ? enemyAnim : playerAnim);

        RobotVFXManager casterVFX = caster.GetComponent<RobotVFXManager>();
        RobotVFXManager targetVFX = target.GetComponent<RobotVFXManager>();

        string casterSkillAnimName = string.IsNullOrEmpty(card.animationClipName) ? "Attack" : card.animationClipName;
        string stateTargetName = card.targetState.ToString();
        string effectTypeName = card.effectType.ToString();

        bool isAttackCard = (card.weaponType != CardData.WeaponType.None) || (effectTypeName != "Add" && (stateTargetName == "HealthPoint" || stateTargetName == "AbilityPoint")) || stateTargetName == "Destruction";

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.SwitchCinematicPOV(isPlayerCaster, casterSkillAnimName);
        }

        yield return new WaitForSeconds(0.5f); 

        casterAnim.PlayAnim(casterSkillAnimName);

        float totalAnimDur = 1.5f; 
        if (casterAnim.anim != null)
        {
            casterAnim.anim.Update(0f); 
            totalAnimDur = casterAnim.anim.GetCurrentAnimatorStateInfo(0).length;
        }
        if (totalAnimDur < 1f) totalAnimDur = 1.5f; 

        if (!isAttackCard && targetVFX != null)
        {
            if (effectTypeName == "Add" && stateTargetName == "HealthPoint") targetVFX.PlayHealVFX(totalAnimDur);
            else if (stateTargetName == "AbilityPoint" || stateTargetName == "Dice") targetVFX.PlayGotPowerVFX(totalAnimDur);
            else if (stateTargetName == "Fame") targetVFX.PlayBraggingVFX(totalAnimDur);
        }

        float hitDelay = totalAnimDur * 0.5f;
        yield return new WaitForSeconds(hitDelay); 

        if (isAttackCard && BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.SwitchCinematicPOV(!isPlayerCaster, "Reaction");
            yield return new WaitForSeconds(0.2f); 
        }

        int finalValue = card.effectValue;

        Action deliverPackage = () => 
        {
            switch (stateTargetName)
            {
                case "HealthPoint":
                    if (effectTypeName == "Add") { target.Heal(finalValue); }
                    else { target.TakeDamage(finalValue); if(target.currentHP > 0 && targetAnim != null) targetAnim.PlayAnim("got attacked"); }
                    break;
                case "AbilityPoint":
                    if (effectTypeName == "Add") { target.AddEnergy(finalValue); }
                    else { target.LoseEnergy(finalValue); if (targetAnim != null && target.currentHP > 0) targetAnim.PlayAnim("got attacked"); }
                    break;
                case "Dice":
                case "DicePool":
                    target.AddbonusDice(finalValue);
                    break;

                case "Fame":
                    break;
                case "Destruction":
                    if (targetAnim != null) targetAnim.PlayAnim("got attacked");
                    break;
            }
        };

        if (isAttackCard)
        {
            Transform selectedMuzzle1 = null; GameObject selectedProjPrefab1 = null;
            Transform selectedMuzzle2 = null; GameObject selectedProjPrefab2 = null;
            Transform selectedMuzzle3 = null; GameObject selectedProjPrefab3 = null;
            int totalShots = 1;

            if (casterVFX != null)
            {
                switch (card.weaponType)
                {
                    case CardData.WeaponType.Attack_1: 
                        selectedMuzzle1 = casterVFX.attackMuzzle_1; selectedProjPrefab1 = casterVFX.attackVFX_1; 
                        totalShots = 1; break;
                        
                    case CardData.WeaponType.Attack_2: 
                        selectedMuzzle1 = casterVFX.attackMuzzle_1; selectedProjPrefab1 = casterVFX.attackVFX_1;
                        selectedMuzzle2 = casterVFX.attackMuzzle_2; selectedProjPrefab2 = casterVFX.attackVFX_2;
                        totalShots = 2; break;
                        
                    case CardData.WeaponType.Attack_3: 
                        selectedMuzzle1 = casterVFX.attackMuzzle_1; selectedProjPrefab1 = casterVFX.attackVFX_1;
                        selectedMuzzle2 = casterVFX.attackMuzzle_2; selectedProjPrefab2 = casterVFX.attackVFX_2;
                        selectedMuzzle3 = casterVFX.attackMuzzle_3; selectedProjPrefab3 = casterVFX.attackVFX_3;
                        totalShots = 3; break;

                    case CardData.WeaponType.Destruction_1: 
                        selectedMuzzle1 = casterVFX.destructionMuzzle_1; selectedProjPrefab1 = casterVFX.destructionVFX_1; 
                        totalShots = 1; break;
                        
                    case CardData.WeaponType.Destruction_2: 
                        selectedMuzzle1 = casterVFX.destructionMuzzle_1; selectedProjPrefab1 = casterVFX.destructionVFX_1;
                        selectedMuzzle2 = casterVFX.destructionMuzzle_2; selectedProjPrefab2 = casterVFX.destructionVFX_2;
                        totalShots = 2; break;

                    case CardData.WeaponType.Destruction_3: 
                        selectedMuzzle1 = casterVFX.destructionMuzzle_1; selectedProjPrefab1 = casterVFX.destructionVFX_1;
                        selectedMuzzle2 = casterVFX.destructionMuzzle_2; selectedProjPrefab2 = casterVFX.destructionVFX_2;
                        selectedMuzzle3 = casterVFX.destructionMuzzle_3; selectedProjPrefab3 = casterVFX.destructionVFX_3;
                        totalShots = 3; break;
                }
            }

            if (selectedProjPrefab1 != null && selectedMuzzle1 != null && targetVFX != null && targetVFX.hitPoint != null) 
            {
                bool isLanded = false; 

                if (totalShots > 1)
                {
                    int landedCount = 0;

                    // Shoot 1
                    GameObject projObj1 = Instantiate(selectedProjPrefab1, selectedMuzzle1.position, selectedMuzzle1.rotation);
                    projObj1.GetComponent<ProjectileController>().FireProjectile(targetVFX.hitPoint, () => 
                    {
                        landedCount++;
                        if (landedCount == totalShots) { deliverPackage.Invoke(); isLanded = true; }
                    });

                    yield return new WaitForSeconds(0.12f); 

                    // Shoot 2
                    if (selectedMuzzle2 != null && selectedProjPrefab2 != null)
                    {
                        GameObject projObj2 = Instantiate(selectedProjPrefab2, selectedMuzzle2.position, selectedMuzzle2.rotation);
                        projObj2.GetComponent<ProjectileController>().FireProjectile(targetVFX.hitPoint, () => 
                        {
                            landedCount++;
                            if (landedCount == totalShots) { deliverPackage.Invoke(); isLanded = true; }
                        });
                    }
                    else { landedCount++; } 

                    // Shoot 3
                    if (totalShots == 3)
                    {
                        yield return new WaitForSeconds(0.12f); 
                        if (selectedMuzzle3 != null && selectedProjPrefab3 != null)
                        {
                            GameObject projObj3 = Instantiate(selectedProjPrefab3, selectedMuzzle3.position, selectedMuzzle3.rotation);
                            projObj3.GetComponent<ProjectileController>().FireProjectile(targetVFX.hitPoint, () => 
                            {
                                landedCount++;
                                if (landedCount == totalShots) { deliverPackage.Invoke(); isLanded = true; }
                            });
                        }
                        else { landedCount++; } 
                    }
                }
                else
                {
                    // Single Single
                    GameObject projObj = Instantiate(selectedProjPrefab1, selectedMuzzle1.position, selectedMuzzle1.rotation);
                    projObj.GetComponent<ProjectileController>().FireProjectile(targetVFX.hitPoint, () => 
                    {
                        deliverPackage.Invoke(); 
                        isLanded = true;         
                    });
                }
                
                yield return new WaitUntil(() => isLanded == true);
            }
            else
            {
                deliverPackage.Invoke();
            }
        }
        else
        {
            deliverPackage.Invoke();
        }

        float remainingTime = totalAnimDur - hitDelay;
        if (remainingTime < 0) remainingTime = 0;
        yield return new WaitForSeconds(remainingTime + 0.3f);

        if (BattleUIManager.Instance != null) BattleUIManager.Instance.SwitchCinematicPOV(true, "Reset");

        if (stateTargetName == "Fame")
        {
            yield return new WaitForSeconds(0.4f);
            int famePullIndex = (effectTypeName == "Add") ? TurnManager.Instance.CurrentPlayerIndex : (1 - TurnManager.Instance.CurrentPlayerIndex);
            
            TugOfWarManager.Instance.MoveFame(finalValue, famePullIndex);
            yield return new WaitForSeconds(1.5f);
        }
        else if (stateTargetName == "Destruction")
        {
            yield return new WaitForSeconds(0.4f);
            int destructPullIndex = (effectTypeName == "Add") ? TurnManager.Instance.CurrentPlayerIndex : (1 - TurnManager.Instance.CurrentPlayerIndex);
            
            TugOfWarManager.Instance.MoveDestruction(finalValue, destructPullIndex);
            yield return new WaitForSeconds(1.5f);
        }

        if (BattleUIManager.Instance != null) BattleUIManager.Instance.ResetCamera();

        if (card.produceBuzzTile && !skipBuzzTile)
        {
            yield return StartCoroutine(HandleBuzzTilePlacement(card.buzzTileID, isPlayerCaster));
        }

        if (card.cardCategory == CardData.CardCategory.Permanent && !skipBuzzTile)
        {
            if (PassiveCardManager.Instance != null) PassiveCardManager.Instance.RegisterPassiveCard(card, isPlayerCaster);
        }

        isResolvingEffect = false;
    }

    private IEnumerator HandleBuzzTilePlacement(string buzzID, bool isPlayer)
    {
        Debug.Log($"<color=magenta>🎬 Memasuki Fase Buzz Tile:{buzzID}</color>");

        if (BattleUIManager.Instance != null && BattleUIManager.Instance.VCamArena != null)
        {
            BattleUIManager.Instance.SwitchCinematicPOV(true, "Reset");
            BattleUIManager.Instance.VCamArena.Priority = 50; 
        }

        ArenaTile[] allBuzzTiles = new ArenaTile[]
        {
            TugOfWarManager.Instance.playerFameBuzzTile,
            TugOfWarManager.Instance.playerDestructionBuzzTile,
            TugOfWarManager.Instance.enemyFameBuzzTile,
            TugOfWarManager.Instance.enemyDestructionBuzzTile
        };

        selectedBuzzTile = null;

        if (isPlayer)
        {
            Debug.Log("Menunggu Player memilih kotak Buzz Tile...");

            ArenaTile.OnTileClicked += OnBuzzTileSelected;
            foreach (var tile in allBuzzTiles)
            {
                if (tile != null) tile.SetClickable(true);
            }

            yield return new WaitUntil(() => selectedBuzzTile != null);

            ArenaTile.OnTileClicked -= OnBuzzTileSelected;
            foreach (var tile in allBuzzTiles)
            {
                if (tile != null) tile.SetClickable(false);
            }
        }
        else
        {
            Debug.Log("Enemy sedang berpikir memilih kotak Buzz Tile...");
            yield return new WaitForSeconds(1.5f);

            int randomIndex = UnityEngine.Random.Range(0, allBuzzTiles.Length);
            selectedBuzzTile = allBuzzTiles[randomIndex];
        }

        if (selectedBuzzTile != null)
        {
            Debug.Log($"<color=green>Buzz Tile {buzzID} berhasil ditanam di {selectedBuzzTile.gameObject.name}!</color>");
            selectedBuzzTile.SetBuzzTrap(buzzID);
        }
        
        yield return new WaitForSeconds(0.5f);

        if (BattleUIManager.Instance != null && BattleUIManager.Instance.VCamArena != null)
        {
            BattleUIManager.Instance.VCamArena.Priority = 10;
            BattleUIManager.Instance.ResetCamera();
        }
    }

    private void OnBuzzTileSelected(ArenaTile clickedTile)
    {
        selectedBuzzTile = clickedTile;
    }
}