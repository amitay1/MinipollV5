# 🎨 Minipoll Brand Usage Guide

## Quick Start

### 1. Setup Brand Assets
```csharp
// Create the ScriptableObject assets
// Right-click in Project → Create → Minipoll → Branding → Brand Colors
// Right-click in Project → Create → Minipoll → Branding → Typography
```

### 2. Add Brand Manager to Scene
```csharp
// Add MinipollBrandManager component to a GameObject
// Assign the brand assets in the inspector
// The manager will persist across scenes automatically
```

## 🎯 Common Usage Patterns

### Styling Buttons
```csharp
// Primary button (Minipoll Blue)
MinipollBrandManager.StyleButton(playButton, ButtonStyle.Primary);

// Secondary button (Heart Pink)
MinipollBrandManager.StyleButton(settingsButton, ButtonStyle.Secondary);

// Success button (Growth Green)
MinipollBrandManager.StyleButton(saveButton, ButtonStyle.Success);
```

### Styling Text
```csharp
// Main title
MinipollBrandManager.StyleText(titleText, TypographyStyle.H1);

// Body text with custom color
MinipollBrandManager.StyleText(bodyText, TypographyStyle.Body, MinipollBrandManager.Colors.DeepNavy);

// Numbers/stats
MinipollBrandManager.StyleText(statsText, TypographyStyle.Number, MinipollBrandManager.Colors.GrowthGreen);
```

### Styling Panels
```csharp
// Light background panel
MinipollBrandManager.StylePanel(backgroundPanel, PanelStyle.Light);

// Primary colored panel
MinipollBrandManager.StylePanel(headerPanel, PanelStyle.Primary);
```

### Direct Color Access
```csharp
// Use brand colors directly
image.color = MinipollBrandManager.Colors.MinipollBlue;
text.color = MinipollBrandManager.Colors.HeartPink;

// Emotion-based colors for Minipoll character
Color happyColor = MinipollBrandManager.Colors.GetEmotionColor(EmotionType.Happy);
Color babyColor = MinipollBrandManager.Colors.GetLifeStageColor(LifeStage.Baby);
```

## 🎨 Color Palette

### Primary Colors
- **Minipoll Blue**: `#4A90E2` - Main brand color, trust and reliability
- **Heart Pink**: `#FF6B9D` - Emotional connection, care
- **Growth Green**: `#7ED321` - Progress, health, success

### Secondary Colors  
- **Soft Purple**: `#BD78FF` - Magic, special moments
- **Warm Orange**: `#F5A623` - Energy, excitement
- **Cloud White**: `#F8F9FA` - Clean backgrounds, text areas

### Supporting Colors
- **Deep Navy**: `#2C3E50` - Text, strong contrast
- **Light Gray**: `#ECF0F1` - Subtle backgrounds
- **Success Green**: `#27AE60` - Confirmations, achievements
- **Warning Amber**: `#F39C12` - Alerts, attention

## 🔤 Typography Hierarchy

### Font Stack
1. **Comfortaa** (Primary) - Headers, logo, emotional text
2. **Open Sans** (Secondary) - Body text, UI elements, readable content
3. **Fredoka One** (Accent) - Numbers, playful elements, stats

### Text Styles
- **H1** (48px, Comfortaa Bold) - Page titles, main headers
- **H2** (36px, Comfortaa Bold) - Section headers
- **H3** (28px, Comfortaa Regular) - Subsection headers
- **H4** (24px, Comfortaa Regular) - Small headers
- **Body** (16px, Open Sans Regular) - Main content text
- **Button** (18px, Open Sans SemiBold) - Interactive elements
- **Caption** (14px, Open Sans Regular) - Small descriptions
- **Number** (32px, Fredoka One) - Stats, scores, counts
- **Logo** (42px, Comfortaa Bold) - Brand text

## 📱 Responsive Guidelines

### Screen Breakpoints
- **< 375px**: Small mobile (80% scale)
- **375-768px**: Mobile (90% scale)
- **> 768px**: Tablet/Desktop (100% scale)

### Touch Targets
- Minimum 44px height for mobile
- 8-16px border radius for friendly feel
- Adequate spacing between interactive elements

## ✅ Do's and Don'ts

### ✅ DO
- Use Minipoll Blue as the primary color (40% of design)
- Apply consistent typography hierarchy
- Maintain 8-16px border radius for UI elements
- Use emotion colors for Minipoll character states
- Scale fonts responsively for different screen sizes
- Use the Brand Manager for consistent styling

### ❌ DON'T
- Use colors outside the defined palette
- Mix multiple accent fonts in the same context
- Make text smaller than 14px on mobile
- Use harsh shadows or sharp corners
- Override brand colors with arbitrary values
- Forget to test on different screen sizes

## 🎮 Game-Specific Usage

### Minipoll Character
```csharp
// Color based on emotion
Color characterColor = MinipollBrandManager.Colors.GetEmotionColor(EmotionType.Happy);

// Color based on life stage
Color stageColor = MinipollBrandManager.Colors.GetLifeStageColor(LifeStage.Baby);
```

### UI States
```csharp
// Success state (feeding, achievements)
MinipollBrandManager.StyleButton(feedButton, ButtonStyle.Success);

// Warning state (low health, needs attention)
statusText.color = MinipollBrandManager.Colors.WarningAmber;

// Neutral/calm state
backgroundPanel.color = MinipollBrandManager.Colors.CloudWhite;
```

### Logo Usage
```csharp
// Main logo (with icon and text)
logoImage.sprite = MinipollBrandManager.MinipollLogo;

// Icon only (for small spaces)
iconImage.sprite = MinipollBrandManager.MinipollIcon;

// Studio logo (splash screen)
studioLogoImage.sprite = MinipollBrandManager.HeartCodeStudiosLogo;
```

## 🔧 Technical Implementation

### Required Setup
1. Create `MinipollBrandColors` ScriptableObject asset
2. Create `MinipollTypography` ScriptableObject asset  
3. Add `MinipollBrandManager` to scene and assign assets
4. Import font assets (Comfortaa, Open Sans, Fredoka One)

### Performance Notes
- Brand Manager uses singleton pattern - one instance per game
- Color calculations are cached for better performance
- Typography applies responsive scaling automatically
- Assets persist across scene changes

This brand system ensures visual consistency throughout the Minipoll creature simulation game while providing flexibility for different UI states and character emotions.
