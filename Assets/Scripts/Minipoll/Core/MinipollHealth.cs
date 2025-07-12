using UnityEngine;
using System;
using System.Collections;
using MinipollCore;
using UnityEditor.Rendering;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using MinipollGame.Managers;
using MinipollGame.Systems.Core;
using MinipollGame.Controllers;

/// <summary>
/// מערכת הבריאות של Minipoll
/// מנהלת חיים, נזק, ריפוי ומוות
/// </summary>


namespace MinipollGame.Core
{
    public class MinipollHealth : MonoBehaviour
    {
        #region Health Properties
        [Header("=== Health Settings ===")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float healthRegenRate = 0.1f; // per second
        [SerializeField] private bool enableHealthRegen = true;
        [SerializeField] private float regenStartDelay = 5f; // delay after damage

        [Header("=== Damage Settings ===")]
        [SerializeField] private float damageResistance = 0f; // 0-1, percentage reduction
        [SerializeField] private float invulnerabilityDuration = 0.5f; // after taking damage
        [SerializeField] private bool showDamageNumbers = true;

        [Header("=== Critical State ===")]
        [SerializeField] private float criticalHealthThreshold = 20f;
        [SerializeField] private bool inCriticalState = false;

        // Properties
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercentage => currentHealth / maxHealth;
        public bool IsCritical => currentHealth <= criticalHealthThreshold;
        public bool IsDead => currentHealth <= 0;
        public bool IsFullHealth => Mathf.Approximately(currentHealth, maxHealth);
        public bool IsInvulnerable { get; private set; }
        #endregion

        #region Events
        public event Action<float, GameObject> OnDamaged; // damage amount, source
        public event Action<float, GameObject> OnHealed; // heal amount, source
        public event Action<float, float> OnHealthChanged; // current, max
        public event Action OnDeath;
        public event Action OnRevived;
        public event Action OnCriticalHealth;
        public event Action OnHealthRestored;
        #endregion

        #region References
        private MinipollGame.Core.MinipollCore core;
        private MinipollStats stats;
        private MinipollVisualController visualController;

        private float lastDamageTime;
        private Coroutine regenCoroutine;
        private Coroutine invulnerabilityCoroutine;
        #endregion

        #region Initialization
        private void Awake()
        {
            core = GetComponent<MinipollCore>();
             stats = GetComponent<MinipollStats>();
            visualController = GetComponentInChildren<MinipollVisualController>();

            // Initialize health from stats if available
            if (stats)
            {
                maxHealth = stats.GetStat(StatType.MaxHealth);
                currentHealth = maxHealth;
            }
        }

        private void Start()
        {
            // Start health regeneration
            if (enableHealthRegen)
                StartHealthRegeneration();

            // Initial health update
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void OnEnable()
        {
            // Resume regeneration if enabled
            if (enableHealthRegen && !IsDead)
                StartHealthRegeneration();
        }

        private void OnDisable()
        {
            // Stop all coroutines
            StopAllCoroutines();
        }
        #endregion

        #region Health Management
        public void SetMaxHealth(float newMaxHealth, bool healToFull = false)
        {
            float healthPercentage = currentHealth / maxHealth;
            maxHealth = Mathf.Max(1f, newMaxHealth);

            if (healToFull)
            {
                currentHealth = maxHealth;
            }
            else
            {
                // Maintain health percentage
                currentHealth = maxHealth * healthPercentage;
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void ModifyHealth(float amount, GameObject source = null)
        {
            if (amount < 0)
                TakeDamage(-amount, source);
            else
                Heal(amount, source);
        }

        public void TakeDamage(float damage, GameObject source = null, DamageType damageType = DamageType.Physical)
        {
            if (IsDead || IsInvulnerable || damage <= 0) return;

            // Apply damage resistance
            float actualDamage = damage * (1f - damageResistance);

            // Apply armor/defense from stats
            if (stats)
            {
                float defense = stats.GetStat(StatType.Defense);
                actualDamage = Mathf.Max(1f, actualDamage - defense);
            }

            // Apply damage
            float previousHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - actualDamage);

            // Update last damage time
            lastDamageTime = Time.time;

            // Stop regeneration temporarily
            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
                regenCoroutine = StartCoroutine(DelayedRegeneration());
            }

            // Visual feedback
            if (showDamageNumbers && visualController)
                visualController.ShowDamageNumber(actualDamage);

            // Start invulnerability
            if (invulnerabilityDuration > 0)
            {
                if (invulnerabilityCoroutine != null)
                    StopCoroutine(invulnerabilityCoroutine);
                invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine());
            }

            // Fire events
            OnDamaged?.Invoke(actualDamage, source);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Check critical state
            if (!inCriticalState && IsCritical)
            {
                inCriticalState = true;
                OnCriticalHealth?.Invoke();
                HandleCriticalState();
            }

            // Check death
            if (currentHealth <= 0)
            {
                Die(source);
            }

            Debug.Log($"[Health] {core.Name} took {actualDamage:F1} damage from {(source ? source.name : "Unknown")}. Health: {currentHealth:F1}/{maxHealth:F1}");
        }

        public void Heal(float amount, GameObject source = null)
        {
            if (IsDead || amount <= 0 || IsFullHealth) return;

            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            float actualHealing = currentHealth - previousHealth;

            // Visual feedback
            if (showDamageNumbers && visualController)
                visualController.ShowHealNumber(actualHealing);

            // Fire events
            OnHealed?.Invoke(actualHealing, source);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Check if no longer critical
            if (inCriticalState && !IsCritical)
            {
                inCriticalState = false;
                OnHealthRestored?.Invoke();
            }

            Debug.Log($"[Health] {core.Name} healed {actualHealing:F1} from {(source ? source.name : "Unknown")}. Health: {currentHealth:F1}/{maxHealth:F1}");
        }

        public void HealToFull(GameObject source = null)
        {
            Heal(maxHealth, source);
        }

        public void Kill(GameObject killer = null)
        {
            if (IsDead) return;

            currentHealth = 0;
            Die(killer);
        }

        private void Die(GameObject killer = null)
        {
            Debug.Log($"[Health] {core.Name} has died! Killer: {(killer ? killer.name : "Natural causes")}");

            // Stop all health processes
            StopAllCoroutines();
            enableHealthRegen = false;

            // Fire death event
            OnHealthChanged?.Invoke(0, maxHealth);
            OnDeath?.Invoke();

            // Log death to manager
            if (MinipollManager.Instance)
            {
                MinipollManager.Instance.RecordDeath(core, killer);
            }
        }

        public void Revive(float healthPercentage = 1f)
        {
            if (!IsDead) return;

            currentHealth = maxHealth * Mathf.Clamp01(healthPercentage);
            enableHealthRegen = true;
            inCriticalState = false;

            // Restart regeneration
            if (enableHealthRegen)
                StartHealthRegeneration();

            // Fire events
            OnRevived?.Invoke();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            Debug.Log($"[Health] {core.Name} has been revived with {currentHealth:F1} health!");
        }
        #endregion

        #region Health Regeneration
        private void StartHealthRegeneration()
        {
            if (regenCoroutine != null)
                StopCoroutine(regenCoroutine);

            regenCoroutine = StartCoroutine(HealthRegenerationRoutine());
        }

        private IEnumerator HealthRegenerationRoutine()
        {
            while (enableHealthRegen && !IsDead)
            {
                yield return new WaitForSeconds(1f);

                // Don't regenerate if recently damaged
                if (Time.time - lastDamageTime < regenStartDelay)
                    continue;

                // Don't regenerate if at full health
                if (IsFullHealth)
                    continue;

                // Calculate regen amount
                float regenAmount = healthRegenRate;

                // Boost regen if resting/sleeping
                // if (core.Needs && core.Needs.GetNeedValue(NeedType.Sleep) > 80f)
                //     regenAmount *= 2f;

                // Apply regeneration
                Heal(regenAmount, gameObject);
            }
        }

        private IEnumerator DelayedRegeneration()
        {
            yield return new WaitForSeconds(regenStartDelay);
            StartHealthRegeneration();
        }
        #endregion

        #region Special States
        private IEnumerator InvulnerabilityRoutine()
        {
            IsInvulnerable = true;

            // Visual indication (flashing)
            if (visualController)
                visualController.StartInvulnerabilityEffect(invulnerabilityDuration);

            yield return new WaitForSeconds(invulnerabilityDuration);

            IsInvulnerable = false;
        }

        private void HandleCriticalState()
        {
            // Slow movement in critical state
            if (core.Movement)
                core.Movement.SetSpeedMultiplier(0.5f);

            // Add fear emotion
            if (core.Emotions)
                core.Emotions.AddEmotionalEvent(MinipollEmotionsSystem.EmotionType.Scared ,0.8f);

            // Visual effects
            if (visualController)
                visualController.EnableCriticalHealthEffect(true);

            // Alert nearby friends
            if (core.SocialRelations)
                core.SocialRelations.CallForHelp();
        }
        #endregion

        #region Status Effects
        public void ApplyPoison(float damagePerSecond, float duration, GameObject source = null)
        {
            StartCoroutine(PoisonRoutine(damagePerSecond, duration, source));
        }

        private IEnumerator PoisonRoutine(float dps, float duration, GameObject source)
        {
            float elapsed = 0;

            // Visual effect
            if (visualController)
                visualController.EnablePoisonEffect(true);

            while (elapsed < duration && !IsDead)
            {
                TakeDamage(dps, source, DamageType.Poison);
                elapsed += 1f;
                yield return new WaitForSeconds(1f);
            }

            // Remove visual effect
            if (visualController)
                visualController.EnablePoisonEffect(false);
        }

        public void ApplyBleeding(float damagePerSecond, float duration)
        {
            StartCoroutine(BleedingRoutine(damagePerSecond, duration));
        }

        private IEnumerator BleedingRoutine(float dps, float duration)
        {
            float elapsed = 0;

            while (elapsed < duration && !IsDead)
            {
                TakeDamage(dps, null, DamageType.Bleed);

                // Leave blood trail
                if (visualController)
                    visualController.SpawnBloodDrop();

                elapsed += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }
        }
        #endregion

        #region Save/Load
        public HealthData GetHealthData()
        {
            return new HealthData
            {
                currentHealth = currentHealth,
                maxHealth = maxHealth,
                isDead = IsDead,
                inCriticalState = inCriticalState
            };
        }

        public void LoadHealthData(HealthData data)
        {
            maxHealth = data.maxHealth;
            currentHealth = data.currentHealth;
            inCriticalState = data.inCriticalState;

            if (data.isDead)
            {
                Die();
            }
            else
            {
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
                if (inCriticalState)
                    HandleCriticalState();
            }
        }
        #endregion

        #region Debug
        [ContextMenu("Debug - Take 10 Damage")]
        private void DebugTakeDamage()
        {
            TakeDamage(10f, null);
        }

        [ContextMenu("Debug - Heal 20 HP")]
        private void DebugHeal()
        {
            Heal(20f, null);
        }

        [ContextMenu("Debug - Kill")]
        private void DebugKill()
        {
            Kill();
        }

        [ContextMenu("Debug - Revive")]
        private void DebugRevive()
        {
            Revive();
        }

        private void OnGUI()
        {
            if (!Application.isEditor) return;

            // Draw health bar above character in Scene view
            if (Camera.main != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
                if (screenPos.z > 0)
                {
                    float barWidth = 50f;
                    float barHeight = 5f;

                    // Background
                    GUI.color = Color.black;
                    GUI.DrawTexture(new Rect(screenPos.x - barWidth / 2 - 1, Screen.height - screenPos.y - 1, barWidth + 2, barHeight + 2), Texture2D.whiteTexture);

                    // Health bar
                    GUI.color = IsCritical ? Color.red : Color.green;
                    GUI.DrawTexture(new Rect(screenPos.x - barWidth / 2, Screen.height - screenPos.y, barWidth * HealthPercentage, barHeight), Texture2D.whiteTexture);

                    GUI.color = Color.white;
                }
            }
        }
        #endregion
    }

    #region Data Classes
    [System.Serializable]
    public class HealthData
    {
        public float currentHealth;
        public float maxHealth;
        public bool isDead;
        public bool inCriticalState;
    }

    public enum DamageType
    {
        Physical,
        Poison,
        Fire,
        Ice,
        Electric,
        Psychic,
        Bleed,
        Fall,
        Hunger,
        Disease
    }
    #endregion
}