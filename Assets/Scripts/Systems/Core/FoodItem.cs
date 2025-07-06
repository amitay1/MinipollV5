using System;
using MinipollGame.Core;
using UnityEngine;
using static MinipollCore.core.MinipollEmotionsSystem;

/// <summary>
/// FoodItem - מייצג פריט מזון שמיניפול יכול לאכול
/// </summary>
/// 
namespace MinipollGame.Systems.Core
{ 
[CreateAssetMenu(fileName = "NewFoodItem", menuName = "Minipoll/Items/Food Item")]
public class FoodItem : ScriptableObject
{
    [Header("Basic Info")]
    public string foodName = "Unknown Food";
    public string description = "A piece of food";
    public Sprite icon;
    
    [Header("Nutrition Values")]
    [Range(0, 100)]
    public float nutritionValue = 20f; // כמה רעב זה ממלא
    
    [Range(0, 100)]
    public float energyValue = 10f; // כמה אנרגיה זה נותן
    
    [Range(0, 50)]
    public float healthBonus = 0f; // בונוס בריאות
    
    [Header("Properties")]
    public FoodType foodType = FoodType.Plant;
    public float spoilTime = 0f; // 0 = never spoils
    public bool isRare = false;
    
    [Header("Effects")]
    public bool hasSpecialEffect = false;
    public SpecialFoodEffect specialEffect = SpecialFoodEffect.None;
    public float effectDuration = 0f;
    public float effectStrength = 1f;
    
    [Header("Visuals")]
    public GameObject foodModelPrefab;
    public Color foodColor = Color.white;
    public float modelScale = 1f;
    
    // Properties for compatibility with MinipollCore
    public string FoodName => foodName;
    public float NutritionValue => nutritionValue;
    
    // Get total value for AI decision making
    public float GetTotalValue()
    {
        float value = nutritionValue + energyValue + healthBonus;
        if (hasSpecialEffect && specialEffect != SpecialFoodEffect.None)
        {
            value += 20f; // Special foods are more valuable
        }
        if (isRare)
        {
            value *= 1.5f; // Rare foods are extra valuable
        }
        return value;
    }
    
    // Apply food effects to a minipoll
    public void ApplyEffects(MinipollGame.Core.MinipollCore minipoll)
    {
        if (minipoll == null) return;
        
        // Basic nutrition
        if (minipoll.Needs != null)
        {
            minipoll.Needs.FillNeed("Hunger", nutritionValue);
            minipoll.Needs.FillNeed("Energy", energyValue);
        }
        
        // Health bonus
        if (healthBonus > 0 && minipoll.Health != null)
        {
            minipoll.Health.Heal(healthBonus, null);
        }
        
        // Special effects
        if (hasSpecialEffect && specialEffect != SpecialFoodEffect.None)
        {
            ApplySpecialEffect(minipoll);
        }
        
        // Emotional response
        if (minipoll.Emotions != null)
        {
            float happiness = nutritionValue / 50f; // More nutritious = happier
            minipoll.Emotions.AddEmotionalEvent(EmotionType.Happy, happiness);
        }
        
        // Add memory of eating this food
        if (minipoll.Memory != null)
        {
            minipoll.Memory.AddMemory(
                $"Ate {foodName}",
                null,
                true,
                GetTotalValue() / 100f
            );
        }
    }

    private void ApplySpecialEffect(MinipollGame.Core.MinipollCore minipoll)
    {
        switch (specialEffect)
        {
            case SpecialFoodEffect.SpeedBoost:
                if (minipoll.Stats != null)
                {
                    minipoll.Stats.ApplyBuff(BuffType.Speed, effectDuration, effectStrength);
                }
                break;
                
            case SpecialFoodEffect.StrengthBoost:
                if (minipoll.Stats != null)
                {
                    minipoll.Stats.ApplyBuff(BuffType.Strength, effectDuration, effectStrength);
                }
                break;
                
            case SpecialFoodEffect.IntelligenceBoost:
                if (minipoll.Stats != null)
                {
                    minipoll.Stats.ApplyBuff(BuffType.Intelligence, effectDuration, effectStrength);
                }
                break;
                
            case SpecialFoodEffect.Healing:
                if (minipoll.Health != null)
                {
                    minipoll.Health.HealToFull(null);
                }
                break;
                
            case SpecialFoodEffect.Poison:
                if (minipoll.Health != null)
                {
                    minipoll.Health.ApplyPoison(effectStrength, effectDuration, null);
                }
                break;
                
            case SpecialFoodEffect.Growth:
                // Could trigger evolution or growth
                if (minipoll.Stats != null)
                {
                    minipoll.Stats.ModifyStat(StatType.MaxHealth, 10f);
                }
                break;
                
            case SpecialFoodEffect.Happiness:
                if (minipoll.Emotions != null)
                {
                    minipoll.Emotions.AddEmotionalEvent(EmotionType.Happy, 1f);
                    minipoll.Emotions.AddEmotionalEvent(EmotionType.Excited, 0.8f);
                }
                break;
        }
    }
}

// Food type categories
public enum FoodType
{
    Plant,      // Fruits, vegetables
    Meat,       // From hunting
    Insect,     // Small prey
    Seed,       // Seeds and nuts
    Mushroom,   // Fungi
    Fish,       // Aquatic food
    Processed,  // Cooked or prepared
    Special     // Magical or unique
}

// Special effects food can have
public enum SpecialFoodEffect
{
    None,
    SpeedBoost,
    StrengthBoost,
    IntelligenceBoost,
    Healing,
    Poison,
    Growth,
    Happiness
}
}