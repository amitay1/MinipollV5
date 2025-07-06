using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Creates an animated background with floating particles and gradient effects
/// Designed for professional main menu experience
/// </summary>
public class AnimatedBackground : MonoBehaviour
{
    [Header("Gradient Settings")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color topColor = new Color(0.29f, 0.56f, 0.89f, 1f); // Minipoll Blue
    [SerializeField] private Color bottomColor = new Color(0.74f, 0.47f, 1f, 1f); // Soft Purple
    
    [Header("Animation Settings")]
    [SerializeField] private float colorTransitionDuration = 3f;
    [SerializeField] private bool enableColorAnimation = true;
    
    [Header("Floating Particles")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private int particleCount = 15;
    [SerializeField] private float particleSpeed = 1f;
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(1920, 1080);
    
    private Color[] gradientColors;
    private int currentColorIndex = 0;
    
    private void Start()
    {
        // Initialize gradient colors using TASK001 branding
        InitializeGradientColors();
        
        // Find background image if not assigned
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        
        // Start background animation
        if (enableColorAnimation)
            StartColorAnimation();
        
        // Create floating particles for ambiance
        CreateFloatingParticles();
    }
    
    private void InitializeGradientColors()
    {
        // Using TASK001 color palette for beautiful gradients
        gradientColors = new Color[]
        {
            new Color(0.29f, 0.56f, 0.89f, 1f), // Minipoll Blue
            new Color(0.74f, 0.47f, 1f, 1f),    // Soft Purple
            new Color(1f, 0.42f, 0.62f, 1f),    // Heart Pink
            new Color(0.49f, 0.83f, 0.13f, 1f), // Growth Green
            new Color(0.96f, 0.65f, 0.14f, 1f)  // Warm Orange
        };
    }
    
    private void StartColorAnimation()
    {
        AnimateToNextColor();
    }
    
    private void AnimateToNextColor()
    {
        if (backgroundImage == null) return;
        
        Color targetColor = gradientColors[currentColorIndex];
        
        // Smooth color transition with DOTween
        backgroundImage.DOColor(targetColor, colorTransitionDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                // Move to next color in sequence
                currentColorIndex = (currentColorIndex + 1) % gradientColors.Length;
                
                // Continue animation loop
                Invoke(nameof(AnimateToNextColor), 1f);
            });
    }
    
    private void CreateFloatingParticles()
    {
        if (particlePrefab == null)
        {
            // Create simple particle GameObjects if no prefab provided
            CreateSimpleParticles();
            return;
        }
        
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 randomPosition = GetRandomPosition();
            GameObject particle = Instantiate(particlePrefab, randomPosition, Quaternion.identity, transform);
            
            // Add floating animation to each particle
            AnimateParticle(particle);
        }
    }
    
    private void CreateSimpleParticles()
    {
        for (int i = 0; i < particleCount; i++)
        {
            // Create simple UI particle (small circle)
            GameObject particle = new GameObject($"Particle_{i}");
            particle.transform.SetParent(transform);
            
            // Add Image component for visibility
            Image particleImage = particle.AddComponent<Image>();
            particleImage.color = new Color(1f, 1f, 1f, 0.1f); // Very subtle white
            
            // Set size and position
            RectTransform rectTransform = particle.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(Random.Range(4f, 12f), Random.Range(4f, 12f));
            rectTransform.anchoredPosition = GetRandomUIPosition();
            
            // Animate the particle
            AnimateParticle(particle);
        }
    }
    
    private Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            0
        );
    }
    
    private Vector2 GetRandomUIPosition()
    {
        return new Vector2(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2)
        );
    }
    
    private void AnimateParticle(GameObject particle)
    {
        // Random floating movement
        Vector3 startPos = particle.transform.position;
        Vector3 targetPos = startPos + new Vector3(
            Random.Range(-100f, 100f),
            Random.Range(-100f, 100f),
            0
        );
        
        float duration = Random.Range(3f, 8f);
        
        // Smooth floating animation
        particle.transform.DOMove(targetPos, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        
        // Subtle scale animation
        particle.transform.DOScale(Vector3.one * Random.Range(0.8f, 1.2f), Random.Range(2f, 4f))
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        
        // Fade in/out animation
        if (particle.GetComponent<Image>() != null)
        {
            particle.GetComponent<Image>().DOFade(Random.Range(0.05f, 0.15f), Random.Range(1f, 3f))
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
    
    public void SetBackgroundColor(Color color)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }
    
    public void StartColorTransition(Color targetColor, float duration = 1f)
    {
        if (backgroundImage != null)
        {
            backgroundImage.DOColor(targetColor, duration).SetEase(Ease.InOutSine);
        }
    }
    
    private void OnValidate()
    {
        // Validation in editor
        if (particleCount < 0) particleCount = 0;
        if (particleCount > 50) particleCount = 50; // Limit for performance
        
        if (colorTransitionDuration < 0.5f) colorTransitionDuration = 0.5f;
    }
}
