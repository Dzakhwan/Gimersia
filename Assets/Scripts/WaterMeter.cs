using System;
using UnityEngine;

public class WaterMeter : MonoBehaviour
{
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private float maxWater = 100f;
    [SerializeField] private float drainPerSecond = 6f;
    [SerializeField] private float refillPerSecond = 65f;
    [SerializeField] private float landJumpCost = 10f;

    public event Action<float> OnWaterChanged;
    public event Action OnWaterDepleted;

    public float CurrentWater { get; private set; }
    public float MaxWater => maxWater;
    public float NormalizedWater => maxWater > 0f ? CurrentWater / maxWater : 0f;

    private bool isActive = true;

    private void Awake()
    {
        if (playerMove == null)
        {
            playerMove = GetComponent<PlayerMove>();
        }

        CurrentWater = maxWater;
    }

    private void OnEnable()
    {
        if (playerMove != null)
        {
            playerMove.OnLandJump += ConsumeJumpCost;
        }
    }

    private void OnDisable()
    {
        if (playerMove != null)
        {
            playerMove.OnLandJump -= ConsumeJumpCost;
        }
    }

    private void Start()
    {
        OnWaterChanged?.Invoke(NormalizedWater);
    }

    private void Update()
    {
        if (!isActive || playerMove == null)
        {
            return;
        }

        float delta = playerMove.IsInWater ? refillPerSecond : -drainPerSecond;
        ModifyWater(delta * Time.deltaTime);
    }

    public void RefillFull()
    {
        CurrentWater = maxWater;
        OnWaterChanged?.Invoke(NormalizedWater);
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    private void ConsumeJumpCost()
    {
        if (!isActive)
        {
            return;
        }

        ModifyWater(-landJumpCost);
    }

    private void ModifyWater(float amount)
    {
        float nextValue = Mathf.Clamp(CurrentWater + amount, 0f, maxWater);
        if (Mathf.Approximately(nextValue, CurrentWater))
        {
            return;
        }

        CurrentWater = nextValue;
        OnWaterChanged?.Invoke(NormalizedWater);

        if (CurrentWater <= 0f)
        {
            OnWaterDepleted?.Invoke();
        }
    }
}
