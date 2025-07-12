using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Advanced Art Asset Manager - מנהל נכסי אמנות חכם
/// טוען ומנהל את כל הנכסים מתיקיית ART
/// </summary>
public class MinipollArtAssetManager : MonoBehaviour
{
    [Header("🎨 Art Asset Manager")]
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool debugMode = true;
    
    [Header("📁 Asset Paths")]
    [SerializeField] private string[] spritePaths = {
        "Art/Sprites/SpriteCoinMaster",
        "Art/Sprites/ButtonsAndIcons",
        "Art/Sprites/Backgrounds",
        "Art/Sprites/Effects"
    };
    
    [SerializeField] private string[] texturePaths = {
        "Art/Textures/Backgrounds",
        "Art/Textures/UI",
        "Art/Textures/Effects"
    };
    
    [Header("🔧 Auto-Loading")]
    [SerializeField] private bool autoLoadButtons = true;
    [SerializeField] private bool autoLoadIcons = true;
    [SerializeField] private bool autoLoadBackgrounds = true;
    [SerializeField] private bool autoLoadEffects = true;
    
    // Asset collections
    public Dictionary<string, Sprite> Sprites { get; private set; } = new Dictionary<string, Sprite>();
    public Dictionary<string, Texture2D> Textures { get; private set; } = new Dictionary<string, Texture2D>();
    public Dictionary<string, Font> Fonts { get; private set; } = new Dictionary<string, Font>();
    public Dictionary<string, Material> Materials { get; private set; } = new Dictionary<string, Material>();
    
    // Categorized assets
    public Dictionary<string, Sprite> Buttons { get; private set; } = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> Icons { get; private set; } = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> Backgrounds { get; private set; } = new Dictionary<string, Sprite>();
    public Dictionary<string, Sprite> Effects { get; private set; } = new Dictionary<string, Sprite>();
    
    // Singleton instance
    public static MinipollArtAssetManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (loadOnStart)
        {
            LoadAllAssets();
        }
    }
    
    void Update()
    {
        // F4 key to reload all assets
        if (InputHelper.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("🔄 F4 pressed - reloading all assets!");
            LoadAllAssets();
        }
        
        // F5 key to show asset report
        if (InputHelper.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("📊 F5 pressed - showing asset report!");
            ShowAssetReport();
        }
    }
    
    /// <summary>
    /// טעינת כל הנכסים
    /// </summary>
    [ContextMenu("Load All Assets")]
    public void LoadAllAssets()
    {
        Debug.Log("🎨 Loading all art assets...");
        
        try
        {
            // נקה אוספים קיימים
            ClearAssets();
            
            // טען ספרייטים
            LoadSprites();
            
            // טען טקסטורות
            LoadTextures();
            
            // טען פונטים
            LoadFonts();
            
            // טען חומרים
            LoadMaterials();
            
            // סווג נכסים
            CategorizeAssets();
            
            Debug.Log($"✅ Loaded {Sprites.Count} sprites, {Textures.Count} textures, {Fonts.Count} fonts, {Materials.Count} materials");
            
            if (debugMode)
            {
                ShowAssetReport();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to load assets: {e.Message}");
        }
    }
    
    /// <summary>
    /// ניקוי נכסים קיימים
    /// </summary>
    private void ClearAssets()
    {
        Sprites.Clear();
        Textures.Clear();
        Fonts.Clear();
        Materials.Clear();
        Buttons.Clear();
        Icons.Clear();
        Backgrounds.Clear();
        Effects.Clear();
    }
    
    /// <summary>
    /// טעינת ספרייטים
    /// </summary>
    private void LoadSprites()
    {
        Debug.Log("🖼️ Loading sprites...");
        
        foreach (string path in spritePaths)
        {
            LoadSpritesFromPath(path);
        }
        
        // טען ספרייטים ידועים
        LoadKnownSprites();
    }
    
    /// <summary>
    /// טעינת ספרייטים מנתיב
    /// </summary>
    private void LoadSpritesFromPath(string path)
    {
        try
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            foreach (Sprite sprite in sprites)
            {
                string key = sprite.name.ToLower().Replace(" ", "_");
                Sprites[key] = sprite;
                
                if (debugMode)
                {
                    Debug.Log($"✅ Loaded sprite: {sprite.name} -> {key}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Could not load sprites from {path}: {e.Message}");
        }
    }
    
    /// <summary>
    /// טעינת ספרייטים ידועים
    /// </summary>
    private void LoadKnownSprites()
    {
        string[] knownSprites = {
            "buttonEnabled", "buttonDisabled", "blue button", "green button", 
            "red button", "yellow button", "coin", "spins icon", "shield_icon",
            "hammer", "setting icon", "info_button", "Background", "panel_back",
            "golden_panel_back", "Panel_white", "glow", "rays", "spark", "flasher"
        };
        
        foreach (string spriteName in knownSprites)
        {
            LoadSpriteByName(spriteName);
        }
    }
    
    /// <summary>
    /// טעינת ספרייט לפי שם
    /// </summary>
    private void LoadSpriteByName(string name)
    {
        try
        {
            Sprite sprite = Resources.Load<Sprite>($"Art/Sprites/{name}");
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>($"Art/Sprites/SpriteCoinMaster/{name}");
            }
            
            if (sprite != null)
            {
                string key = name.ToLower().Replace(" ", "_");
                Sprites[key] = sprite;
                
                if (debugMode)
                {
                    Debug.Log($"✅ Loaded known sprite: {name} -> {key}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Could not load sprite {name}: {e.Message}");
        }
    }
    
    /// <summary>
    /// טעינת טקסטורות
    /// </summary>
    private void LoadTextures()
    {
        Debug.Log("🖼️ Loading textures...");
        
        foreach (string path in texturePaths)
        {
            try
            {
                Texture2D[] textures = Resources.LoadAll<Texture2D>(path);
                foreach (Texture2D texture in textures)
                {
                    string key = texture.name.ToLower().Replace(" ", "_");
                    Textures[key] = texture;
                    
                    if (debugMode)
                    {
                        Debug.Log($"✅ Loaded texture: {texture.name} -> {key}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ Could not load textures from {path}: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// טעינת פונטים
    /// </summary>
    private void LoadFonts()
    {
        Debug.Log("🔤 Loading fonts...");
        
        try
        {
            Font[] fonts = Resources.LoadAll<Font>("Art/Fonts");
            foreach (Font font in fonts)
            {
                string key = font.name.ToLower().Replace(" ", "_");
                Fonts[key] = font;
                
                if (debugMode)
                {
                    Debug.Log($"✅ Loaded font: {font.name} -> {key}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Could not load fonts: {e.Message}");
        }
    }
    
    /// <summary>
    /// טעינת חומרים
    /// </summary>
    private void LoadMaterials()
    {
        Debug.Log("🎨 Loading materials...");
        
        try
        {
            Material[] materials = Resources.LoadAll<Material>("Art/Materials");
            foreach (Material material in materials)
            {
                string key = material.name.ToLower().Replace(" ", "_");
                Materials[key] = material;
                
                if (debugMode)
                {
                    Debug.Log($"✅ Loaded material: {material.name} -> {key}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Could not load materials: {e.Message}");
        }
    }
    
    /// <summary>
    /// סיווג נכסים לקטגוריות
    /// </summary>
    private void CategorizeAssets()
    {
        Debug.Log("📂 Categorizing assets...");
        
        if (autoLoadButtons)
        {
            CategorizeButtons();
        }
        
        if (autoLoadIcons)
        {
            CategorizeIcons();
        }
        
        if (autoLoadBackgrounds)
        {
            CategorizeBackgrounds();
        }
        
        if (autoLoadEffects)
        {
            CategorizeEffects();
        }
    }
    
    /// <summary>
    /// סיווג כפתורים
    /// </summary>
    private void CategorizeButtons()
    {
        string[] buttonKeywords = { "button", "btn", "press", "click" };
        
        foreach (var sprite in Sprites)
        {
            if (buttonKeywords.Any(keyword => sprite.Key.Contains(keyword)))
            {
                Buttons[sprite.Key] = sprite.Value;
                
                if (debugMode)
                {
                    Debug.Log($"🔘 Categorized button: {sprite.Key}");
                }
            }
        }
    }
    
    /// <summary>
    /// סיווג אייקונים
    /// </summary>
    private void CategorizeIcons()
    {
        string[] iconKeywords = { "icon", "ico", "symbol", "sign", "coin", "shield", "hammer", "setting" };
        
        foreach (var sprite in Sprites)
        {
            if (iconKeywords.Any(keyword => sprite.Key.Contains(keyword)))
            {
                Icons[sprite.Key] = sprite.Value;
                
                if (debugMode)
                {
                    Debug.Log($"🔗 Categorized icon: {sprite.Key}");
                }
            }
        }
    }
    
    /// <summary>
    /// סיווג רקעים
    /// </summary>
    private void CategorizeBackgrounds()
    {
        string[] backgroundKeywords = { "background", "bg", "panel", "backdrop", "back" };
        
        foreach (var sprite in Sprites)
        {
            if (backgroundKeywords.Any(keyword => sprite.Key.Contains(keyword)))
            {
                Backgrounds[sprite.Key] = sprite.Value;
                
                if (debugMode)
                {
                    Debug.Log($"🖼️ Categorized background: {sprite.Key}");
                }
            }
        }
    }
    
    /// <summary>
    /// סיווג אפקטים
    /// </summary>
    private void CategorizeEffects()
    {
        string[] effectKeywords = { "glow", "spark", "flash", "ray", "effect", "particle", "fx" };
        
        foreach (var sprite in Sprites)
        {
            if (effectKeywords.Any(keyword => sprite.Key.Contains(keyword)))
            {
                Effects[sprite.Key] = sprite.Value;
                
                if (debugMode)
                {
                    Debug.Log($"✨ Categorized effect: {sprite.Key}");
                }
            }
        }
    }
    
    /// <summary>
    /// הצגת דוח נכסים
    /// </summary>
    [ContextMenu("Show Asset Report")]
    public void ShowAssetReport()
    {
        Debug.Log("📊 === ART ASSET REPORT ===");
        Debug.Log($"📈 Total Sprites: {Sprites.Count}");
        Debug.Log($"📈 Total Textures: {Textures.Count}");
        Debug.Log($"📈 Total Fonts: {Fonts.Count}");
        Debug.Log($"📈 Total Materials: {Materials.Count}");
        Debug.Log("");
        Debug.Log("📂 CATEGORIZED ASSETS:");
        Debug.Log($"🔘 Buttons: {Buttons.Count}");
        Debug.Log($"🔗 Icons: {Icons.Count}");
        Debug.Log($"🖼️ Backgrounds: {Backgrounds.Count}");
        Debug.Log($"✨ Effects: {Effects.Count}");
        Debug.Log("");
        
        if (debugMode)
        {
            Debug.Log("🔘 BUTTONS:");
            foreach (var button in Buttons)
            {
                Debug.Log($"  - {button.Key}");
            }
            
            Debug.Log("🔗 ICONS:");
            foreach (var icon in Icons)
            {
                Debug.Log($"  - {icon.Key}");
            }
            
            Debug.Log("🖼️ BACKGROUNDS:");
            foreach (var bg in Backgrounds)
            {
                Debug.Log($"  - {bg.Key}");
            }
            
            Debug.Log("✨ EFFECTS:");
            foreach (var effect in Effects)
            {
                Debug.Log($"  - {effect.Key}");
            }
        }
        
        Debug.Log("==================");
    }
    
    // Public getter methods
    public Sprite GetSprite(string key)
    {
        key = key.ToLower().Replace(" ", "_");
        return Sprites.ContainsKey(key) ? Sprites[key] : null;
    }
    
    public Sprite GetButton(string key)
    {
        key = key.ToLower().Replace(" ", "_");
        return Buttons.ContainsKey(key) ? Buttons[key] : null;
    }
    
    public Sprite GetIcon(string key)
    {
        key = key.ToLower().Replace(" ", "_");
        return Icons.ContainsKey(key) ? Icons[key] : null;
    }
    
    public Sprite GetBackground(string key)
    {
        key = key.ToLower().Replace(" ", "_");
        return Backgrounds.ContainsKey(key) ? Backgrounds[key] : null;
    }
    
    public Sprite GetEffect(string key)
    {
        key = key.ToLower().Replace(" ", "_");
        return Effects.ContainsKey(key) ? Effects[key] : null;
    }
    
    public Texture2D GetTexture(string key)
    {
        key = key.ToLower().Replace(" ", "_");
        return Textures.ContainsKey(key) ? Textures[key] : null;
    }
    
    public Font GetFont(string key)
    {
        key = key.ToLower().Replace(" ", "_");
        return Fonts.ContainsKey(key) ? Fonts[key] : null;
    }
    
    public Material GetMaterial(string key)
    {
        key = key.ToLower().Replace(" ", "_");
        return Materials.ContainsKey(key) ? Materials[key] : null;
    }
    
    void OnGUI()
    {
        GUI.Label(new Rect(10, 200, 400, 200), 
            "🎨 Art Asset Manager\n" +
            $"Sprites: {Sprites.Count}\n" +
            $"Textures: {Textures.Count}\n" +
            $"Fonts: {Fonts.Count}\n" +
            $"Materials: {Materials.Count}\n\n" +
            "F4 - Reload Assets\n" +
            "F5 - Show Report\n\n" +
            "Categories:\n" +
            $"🔘 Buttons: {Buttons.Count}\n" +
            $"🔗 Icons: {Icons.Count}\n" +
            $"🖼️ Backgrounds: {Backgrounds.Count}\n" +
            $"✨ Effects: {Effects.Count}");
    }
}
