using System;
using System.Collections;
using UnityEngine;

public class MineComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonoBehaviour targetProviderBehaviour;
    private IGridTargetProvider targetProvider;

    [SerializeField] private CarriedItemComponent hand;
    [SerializeField] private InventoryController inventory;
    private StaminaComponent stamina;

    [Header("Mining")]
    [SerializeField] private float requiredHoldTime = 3f;
    [SerializeField] private int mineCost = 2;

    public event Action<float> OnMineProgress;
    public float RequiredHoldTime => requiredHoldTime;

    private Coroutine miningCor;
    private bool isEquipped;

    private void Awake()
    {
        targetProvider = targetProviderBehaviour as IGridTargetProvider;

        inventory = FindAnyObjectByType<InventoryController>();
        stamina = GetComponent<StaminaComponent>();
    }

    private void OnEnable()
    {
        hand.OnChanged += SyncEquippedFromHand;
        SyncEquippedFromHand();
    }

    private void OnDisable()
    {
        hand.OnChanged -= SyncEquippedFromHand;
        StopMining();
    }

    private void SyncEquippedFromHand()
    {
        bool equipped = (hand != null && hand.Mode == CARRIED_MODE.MINING);
        SetEquipped(equipped);
    }

    public void SetEquipped(bool equipped)
    {
        isEquipped = equipped;
        if (!isEquipped) StopMining();
    }

    private void Update()
    {
        if (!isEquipped) return;

        if (Input.GetMouseButtonDown(1)) StartMining();

        if (Input.GetMouseButtonUp(1)) StopMining();
    }

    private void StartMining()
    {
        if (miningCor != null) return;
        miningCor = StartCoroutine(MiningHoldRoutine());
    }

    private void StopMining()
    {
        if (miningCor == null) return; 
        StopCoroutine(miningCor);
        miningCor = null;
        OnMineProgress?.Invoke(0f);
    }

    private IEnumerator MiningHoldRoutine()
    {
        Debug.Log(1);
        if (targetProvider == null || !targetProvider.TryGetTarget(out GridTargetContext startCtx))
        {
            miningCor = null;
            yield break;
        }
        Debug.Log(2);

        if (!CanMine(startCtx))
        {
            miningCor = null;
            yield break;
        }

        float t = 0f;
        while (t < requiredHoldTime)
        {
            if (!targetProvider.TryGetTarget(out GridTargetContext nowCtx) || nowCtx.cell != startCtx.cell)
            {
                StopMining();
                yield break;
            }
            Debug.Log(3);

            t += Time.deltaTime;
            OnMineProgress?.Invoke(Mathf.Clamp01(t / requiredHoldTime));
            Debug.Log(4);
            yield return null;
        }

        if (stamina != null && !stamina.TrySpend(mineCost))
        {
            miningCor = null;
            OnMineProgress?.Invoke(0f);
            yield break;
        }
        DoMine(startCtx);
        miningCor = null;
        OnMineProgress?.Invoke(0f);
    }

    private bool CanMine(in GridTargetContext info)
    {
        if (!info.nearRule || !info.inside || !info.hasState) return false;

        if (info.isPlacedBlock) return false;

        // 스테이지 고정 블록만 깸
        if (info.state != MAP_STATE.STAGE_BLOCK) return false; 
        return true;
    }

    private void DoMine(in GridTargetContext info)
    {
        if (GridStateManager.i == null) return;

        if (GridStateManager.i.BreakStageBlock(info.cell))
        {
            FYPSoundManager.i?.PlaySFX(E_SFX.REMOVE);

            inventory?.AddItem(ITEM_TYPE.STAIR, 1);
        }    
    }
    //private void TryMine(GridTargetContext info)
    //{
    //    if (!info.nearRule || !info.inside || !info.hasState) return;

    //    if (info.isPlacedBlock) return;

    //    if (info.state != MAP_STATE.STAGE_BLOCK) return;

    //    //GridStateManager.i.BreakStageBlock(info.cell);
    //}
}
