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

        string casterSkillAnimName = string.IsNullOrEmpty(card.animationClipName) ? "Attack" : card.animationClipName;
        string stateTargetName = card.targetState.ToString();
        string effectTypeName = card.effectType.ToString();

        bool isAttackCard = (effectTypeName != "Add" && (stateTargetName == "HealthPoint" || stateTargetName == "AbilityPoint"));

        if (BattleUIManager.Instance != null)
        {
            string actionCam = isAttackCard ? "AttackAction" : "BuffAction";
            BattleUIManager.Instance.SwitchCinematicPOV(isPlayerCaster, actionCam);
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
        Debug.Log($"<color=yellow>🎬 Durasi Animasi '{casterSkillAnimName}' Terdeteksi: {totalAnimDur} detik.</color>");

        float hitDelay = totalAnimDur * 0.5f;
        yield return new WaitForSeconds(hitDelay); 

        if (isAttackCard && BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.SwitchCinematicPOV(isPlayerCaster, "Reaction");
            yield return new WaitForSeconds(0.2f); 
        }

        int finalValue = card.effectValue;

        Action deliverPackage = () => 
        {
            switch (stateTargetName)
            {
                case "HealthPoint":
                    if (effectTypeName == "Add") { target.Heal(finalValue); }
                    else 
                    { 
                        target.TakeDamage(finalValue);
                        if(target.currentHP > 0 && targetAnim != null) targetAnim.PlayAnim("got attacked");
                    }
                    break;
                    
                case "AbilityPoint":
                    if (effectTypeName == "Add") { target.AddEnergy(finalValue); }
                    else
                    {
                        target.LoseEnergy(finalValue);
                        if (targetAnim != null && target.currentHP > 0) targetAnim.PlayAnim("got attacked"); 
                    }
                    break;
                    
                case "Dice":
                case "DicePool":
                    target.AddbonusDice(finalValue);
                    break;
                    
                case "Fame":
                    int famePullIndex = (effectTypeName == "Add") ? TurnManager.Instance.CurrentPlayerIndex : (1 - TurnManager.Instance.CurrentPlayerIndex);
                    TugOfWarManager.Instance.MoveFame(finalValue, famePullIndex);
                    break;
                    
                case "Destruction":
                    int destructPullIndex = (effectTypeName == "Add") ? TurnManager.Instance.CurrentPlayerIndex : (1 - TurnManager.Instance.CurrentPlayerIndex);
                    TugOfWarManager.Instance.MoveDestruction(finalValue, destructPullIndex);
                    break;
            }
        };

        if (isAttackCard)
        {
            Transform selectedMuzzle = null;
            GameObject selectedProjPrefab = null;

            switch (card.weaponType)
            {
                case CardData.WeaponType.Attack_1:
                    selectedMuzzle = caster.attackMuzzle_1; selectedProjPrefab = caster.attackVFX_1; break;
                case CardData.WeaponType.Attack_2:
                    selectedMuzzle = caster.attackMuzzle_2; selectedProjPrefab = caster.attackVFX_2; break;
                case CardData.WeaponType.Attack_3:
                    selectedMuzzle = caster.attackMuzzle_3; selectedProjPrefab = caster.attackVFX_3; break;
                case CardData.WeaponType.Destruction_1:
                    selectedMuzzle = caster.destructionMuzzle_1; selectedProjPrefab = caster.destructionVFX_1; break;
                case CardData.WeaponType.Destruction_2:
                    selectedMuzzle = caster.destructionMuzzle_2; selectedProjPrefab = caster.destructionVFX_2; break;
                case CardData.WeaponType.Destruction_3:
                    selectedMuzzle = caster.destructionMuzzle_3; selectedProjPrefab = caster.destructionVFX_3; break;
            }

            if (selectedProjPrefab != null && selectedMuzzle != null && target.hitPoint != null) 
            {
                GameObject projObj = Instantiate(selectedProjPrefab, selectedMuzzle.position, selectedMuzzle.rotation);
                ProjectileController proj = projObj.GetComponent<ProjectileController>();

                bool isLanded = false; 

                proj.FireProjectile(target.hitPoint, () => 
                {
                    deliverPackage.Invoke(); 
                    isLanded = true;         

                    if (target.currentHP <= 0 && target.defeatVFX != null && target.defeatPoint != null)
                    {
                        Instantiate(target.defeatVFX, target.defeatPoint.position, target.defeatPoint.rotation);
                    }
                });

                yield return new WaitUntil(() => isLanded == true);
            }
            else
            {
                Debug.LogWarning($"<color=red>VFX {card.weaponType} tidak ada di {caster.gameObject.name}! Damage instan.</color>");
                deliverPackage.Invoke();
            }
        }
        else
        {
            deliverPackage.Invoke();

            GameObject vfxToPlay = null;
            Transform spawnSpot = null;

            if (effectTypeName == "Add" && stateTargetName == "HealthPoint") 
            {
                vfxToPlay = target.healVFX;
                spawnSpot = target.healPoint;
            }
            else if (stateTargetName == "AbilityPoint" || stateTargetName == "Dice") 
            {
                vfxToPlay = target.gotPowerVFX;
                spawnSpot = target.gotPowerPoint;
            }
            else if (stateTargetName == "Fame")
            {
                vfxToPlay = target.braggingVFX;
                spawnSpot = target.braggingPoint;
            }

            if (vfxToPlay != null && spawnSpot != null)
            {
                GameObject spawnedVFX = Instantiate(vfxToPlay, spawnSpot.position, Quaternion.identity);
                spawnedVFX.transform.SetParent(spawnSpot);
                Destroy(spawnedVFX, 3f);
            }
        }

        float remainingTime = totalAnimDur - hitDelay;
        if (remainingTime < 0) remainingTime = 0;

        yield return new WaitForSeconds(remainingTime + 0.3f);

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.ResetCamera();
        }

        if (card.produceBuzzTile && !string.IsNullOrEmpty(card.buzzTileID) && !skipBuzzTile)
        {
            yield return new WaitForSeconds(1.0f);

            if (BattleUIManager.Instance != null && BattleUIManager.Instance.VCamArena != null)
            {
                if (BattleUIManager.Instance.VCamPlayer != null) BattleUIManager.Instance.VCamPlayer.Priority = 10;
                if (BattleUIManager.Instance.VCamEnemy != null) BattleUIManager.Instance.VCamEnemy.Priority = 10;
                BattleUIManager.Instance.VCamArena.Priority = 30; 
            }

            yield return new WaitForSeconds(1.5f);

            yield return StartCoroutine(HandleBuzzTilePlacement(card.buzzTileID, isPlayerCaster));

            if (BattleUIManager.Instance != null && BattleUIManager.Instance.VCamArena != null)
            {
                BattleUIManager.Instance.VCamArena.Priority = 10;
            }
        }

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.ResetCamera();
        }

        if (card.cardCategory == CardData.CardCategory.Permanent && !skipBuzzTile)
        {
            if (PassiveCardManager.Instance != null)
            {
                PassiveCardManager.Instance.RegisterPassiveCard(card, isPlayerCaster);
            }
        }

        Debug.Log($"<color=green> Efek '{card.cardName}' Selesai! Coroutine selesai dan kartu siap dihancurkan.</color>");

        isResolvingEffect = false;
    }

    private IEnumerator HandleBuzzTilePlacement(string buzzID, bool isPlayer)
    {
        Debug.Log($"<color=magenta>🎬 Memasuki Fase Buzz Tile:{buzzID}</color>");

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
    }

    private void OnBuzzTileSelected(ArenaTile clickedTile)
    {
        selectedBuzzTile = clickedTile;
    }
}