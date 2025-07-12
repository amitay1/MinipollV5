/***************************************************************
 * MinipollVisualController.cs - גרסה נקייה ועובדת
 * 
 * מנהל את המראה החיצוני של המיניפול
 ***************************************************************/

using UnityEngine;
using System.Collections;
using MinipollGame.Core;
using System;
using MinipollGame.Systems.Core;

namespace MinipollGame.Controllers
{
    public class MinipollVisualController : MonoBehaviour
    {
        [Header("References")]
        public Renderer[] renderers;
        public Animator animator;
        
        [Header("Color Settings")]
        public Color defaultColor = Color.blue;
        public Color happyColor = Color.green;
        public Color sadColor = Color.blue;
        public Color angryColor = Color.red;
        public Color fearColor = Color.yellow;
        public Color criticalHealthColor = Color.red;
        public Color poisonColor = Color.green;
        
        [Header("Scale & Animation")]
        public bool enablePulsing = true;
        public float pulseSpeed = 2f;
        public float pulseAmount = 0.05f;
        
        [Header("Effects")]
        public bool enableInvulnerabilityFlash = true;
        public float flashSpeed = 10f;
        
        // Private variables
        private Vector3 baseScale;
        private Color currentTargetColor;
        private Color originalColor;
        private bool isInvulnerable = false;
        private bool isPoisoned = false;
        private bool isCriticalHealth = false;
        
        // Components
        private MinipollBrain brain;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            SetupInitialState();
        }
        
        private void Update()
        {
            UpdateVisuals();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            // מצא או צור רכיבים
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>();
            }
            
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            
            // מצא את המוח
            brain = GetComponent<MinipollBrain>();
            if (brain == null)
            {
                brain = GetComponentInParent<MinipollBrain>();
            }
            
            // שמור את הסקייל הבסיסי
            baseScale = transform.localScale;
            originalColor = defaultColor;
            currentTargetColor = defaultColor;
        }
        
        private void SetupInitialState()
        {
            SetColor(defaultColor);
        }
        
        #endregion
        
        #region Main Update
        
        private void UpdateVisuals()
        {
            // עדכון צבע לפי רגש
            UpdateColorByEmotion();
            
            // עדכון פולס
            if (enablePulsing)
            {
                UpdatePulseEffect();
            }
            
            // עדכון אפקטי סטטוס
            UpdateStatusEffects();
            
            // עדכון אנימטור
            UpdateAnimator();
        }
        
        /// <summary>
        /// עדכון צבע לפי מצב רגשי - זה שהיה כשל!
        /// </summary>
        private void UpdateColorByEmotion()
        {
            if (brain == null)
            {
                SetColor(defaultColor);
                return;
            }
            
            // קביעת צבע לפי מצב הבריאות והרגשות
            Color targetColor = GetEmotionBasedColor();
            
            // מעבר הדרגתי
            if (currentTargetColor != targetColor)
            {
                currentTargetColor = targetColor;
            }
            
            // החלת הצבע (עם אפקטים אם יש)
            Color finalColor = currentTargetColor;
            
            // אפקטי סטטוס
            if (isCriticalHealth)
            {
                finalColor = Color.Lerp(finalColor, criticalHealthColor, 0.5f);
            }
            
            if (isPoisoned)
            {
                finalColor = Color.Lerp(finalColor, poisonColor, 0.3f);
            }
            
            // אפקט הבזקי אי-פגיעות
            if (isInvulnerable && enableInvulnerabilityFlash)
            {
                float flashAmount = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
                finalColor.a = Mathf.Lerp(0.3f, 1f, flashAmount);
            }
            
            SetColor(finalColor);
        }
        
        /// <summary>
        /// קביעת צבע לפי מצב רגשי
        /// </summary>
        private Color GetEmotionBasedColor()
        {
            if (brain == null) return defaultColor;
            
            // חישוב על בסיס הסטטיסטיקות הבסיסיות
            float happiness = brain.happiness;
            float health = brain.health;
            float energy = brain.energy;
            
            // בריאות קריטית
            if (health < 20f)
                return criticalHealthColor;
            
            // מצב רוח
            if (happiness > 75f)
                return happyColor;
            else if (happiness < 25f)
                return sadColor;
            else if (energy < 25f)
                return Color.Lerp(defaultColor, Color.gray, 0.5f);
            else
                return defaultColor;
        }
        
        #endregion
        
        #region Visual Effects
        
        private void UpdatePulseEffect()
        {
            if (brain == null) return;
            
            // פולס רק כש-idle או במנוחה
            bool shouldPulse = brain.currentState == MinipollStats.Idle || 
                              brain.currentState == MinipollStats.Sleeping;
            
            if (shouldPulse)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                transform.localScale = baseScale * pulse;
            }
            else
            {
                transform.localScale = baseScale;
            }
        }
        
        private void UpdateStatusEffects()
        {
            if (brain == null) return;
            
            // בדיקת בריאות קריטית
            bool shouldShowCriticalHealth = brain.health < 20f;
            if (shouldShowCriticalHealth != isCriticalHealth)
            {
                isCriticalHealth = shouldShowCriticalHealth;
            }
        }
        
        private void UpdateAnimator()
        {
            if (animator == null || brain == null) return;
            
            // עדכון פרמטרים
            animator.SetFloat("Health", brain.health / 100f);
            animator.SetFloat("Energy", brain.energy / 100f);
            animator.SetFloat("Happiness", brain.happiness / 100f);
            
            // עדכון מצב
            animator.SetInteger("State", (int)brain.currentState);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// שינוי צבע ידני
        /// </summary>
        public void SetPrimaryColor(Color color)
        {
            defaultColor = color;
            originalColor = color;
            currentTargetColor = color;
        }
        
        /// <summary>
        /// הצגת נזק
        /// </summary>
        public void ShowDamage(float damage)
        {
            Debug.Log($"Minipoll took {damage} damage!");
            StartCoroutine(DamageFlashEffect());
        }
        
        /// <summary>
        /// הצגת ריפוי
        /// </summary>
        public void ShowHealing(float healing)
        {
            Debug.Log($"Minipoll healed {healing} HP!");
            StartCoroutine(HealingEffect());
        }
        
        /// <summary>
        /// אפקט אי-פגיעות
        /// </summary>
        public void StartInvulnerabilityEffect(float duration)
        {
            StartCoroutine(InvulnerabilityCoroutine(duration));
        }
        
        /// <summary>
        /// אפקט רעל
        /// </summary>
        public void SetPoisonEffect(bool enabled)
        {
            isPoisoned = enabled;
        }
        
        /// <summary>
        /// עדכון לפי גיל
        /// </summary>
         public void UpdateForAgeStage(int ageStage)
        {
            float scaleMultiplier = 1.0f;
            
            if (ageStage == 0) // Baby
                scaleMultiplier = 0.5f;
            else if (ageStage == 1) // Child
                scaleMultiplier = 0.75f;
            else if (ageStage == 2) // Teen
                scaleMultiplier = 0.9f;
            else if (ageStage == 3) // Adult
                scaleMultiplier = 1.0f;
            else if (ageStage == 4) // Elder
                scaleMultiplier = 0.95f;
            
            baseScale = Vector3.one * scaleMultiplier;
        }
        
        /// <summary>
        /// אנימציית מוות
        /// </summary>
        public void PlayDeathAnimation()
        {
            StartCoroutine(DeathAnimationCoroutine());
        }
        
        #endregion
        
        #region Coroutines
        
        private IEnumerator DamageFlashEffect()
        {
            Color originalColor = GetCurrentColor();
            SetColor(Color.red);
            yield return new WaitForSeconds(0.1f);
            SetColor(originalColor);
        }
        
        private IEnumerator HealingEffect()
        {
            Color originalColor = GetCurrentColor();
            SetColor(Color.green);
            yield return new WaitForSeconds(0.2f);
            SetColor(originalColor);
        }
        
        private IEnumerator InvulnerabilityCoroutine(float duration)
        {
            isInvulnerable = true;
            yield return new WaitForSeconds(duration);
            isInvulnerable = false;
        }
        
        private IEnumerator DeathAnimationCoroutine()
        {
            float duration = 2f;
            Vector3 startScale = transform.localScale;
            Color startColor = GetCurrentColor();
            
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float progress = t / duration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
                
                Color fadeColor = startColor;
                fadeColor.a = Mathf.Lerp(1f, 0f, progress);
                SetColor(fadeColor);
                
                yield return null;
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void SetColor(Color color)
        {
            if (renderers == null) return;
            
            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = color;
                }
            }
        }
        
        private Color GetCurrentColor()
        {
            if (renderers != null && renderers.Length > 0 && renderers[0] != null)
            {
                return renderers[0].material.color;
            }
            return defaultColor;
        }

        internal void ShowDamageNumber(float actualDamage)
        {
            throw new NotImplementedException();
        }

        internal void ShowHealNumber(float actualHealing)
        {
            throw new NotImplementedException();
        }

        internal void EnableCriticalHealthEffect(bool v)
        {
            throw new NotImplementedException();
        }

        internal void EnablePoisonEffect(bool v)
        {
            throw new NotImplementedException();
        }

        internal void SpawnBloodDrop()
        {
            throw new NotImplementedException();
        }

        internal void ForceColor(Color c)
        {
            throw new NotImplementedException();
        }

        internal void UpdateForAgeStage(AgeStage ageStage)
        {
            throw new NotImplementedException();
        }

        internal void PlayHurtAnimation()
        {
            throw new NotImplementedException();
        }

        internal void SetEmotionalState(MinipollEmotionsSystem.EmotionType emotion)
        {
            throw new NotImplementedException();
        }

        internal void PlaySleepAnimation()
        {
            throw new NotImplementedException();
        }

        internal void SetGender(Gender gender)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}