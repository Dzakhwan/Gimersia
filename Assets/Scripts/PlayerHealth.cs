using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private WaterMeter waterMeter;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float respawnInvulnerability = 0.5f;

    public event Action<int, int> OnLivesChanged;
    public event Action OnGameOver;
    public event Action OnLevelCompleted;

    public int CurrentLives { get; private set; }
    public int MaxLives => maxLives;

    private Vector3 checkpointPosition;
    private float invulnerableUntil;
    private bool isFinished;

    private void Awake()
    {
        if (playerMove == null)
        {
            playerMove = GetComponent<PlayerMove>();
        }

        if (waterMeter == null)
        {
            waterMeter = GetComponent<WaterMeter>();
        }

        CurrentLives = maxLives;
        checkpointPosition = transform.position;
    }

    private void OnEnable()
    {
        if (waterMeter != null)
        {
            waterMeter.OnWaterDepleted += HandleWaterDepleted;
        }
    }

    private void OnDisable()
    {
        if (waterMeter != null)
        {
            waterMeter.OnWaterDepleted -= HandleWaterDepleted;
        }
    }

    private void Start()
    {
        OnLivesChanged?.Invoke(CurrentLives, maxLives);
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
    }

    public void TakeDamage()
    {
        if (isFinished || CurrentLives <= 0 || Time.time < invulnerableUntil)
        {
            return;
        }

        CurrentLives = Mathf.Max(0, CurrentLives - 1);
        OnLivesChanged?.Invoke(CurrentLives, maxLives);

        if (CurrentLives <= 0)
        {
            isFinished = true;
            DisableGameplayControls();
            OnGameOver?.Invoke();
            return;
        }

        Respawn();
    }

    public void ReachGoal()
    {
        if (isFinished || CurrentLives <= 0)
        {
            return;
        }

        isFinished = true;
        DisableGameplayControls();
        OnLevelCompleted?.Invoke();
    }

    private void HandleWaterDepleted()
    {
        TakeDamage();
    }

    private void Respawn()
    {
        invulnerableUntil = Time.time + respawnInvulnerability;

        if (playerMove != null)
        {
            playerMove.ResetMovement();
        }

        transform.position = checkpointPosition;

        if (waterMeter != null)
        {
            waterMeter.RefillFull();
        }
    }

    private void DisableGameplayControls()
    {
        if (playerMove != null)
        {
            playerMove.ResetMovement();
            playerMove.enabled = false;
        }

        if (waterMeter != null)
        {
            waterMeter.SetActive(false);
        }
    }
}
