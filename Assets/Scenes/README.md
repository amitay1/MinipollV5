# Minipoll V5 - Scene Organization System

## Overview

הפרויקט מאורגן במערכת של 5 סצנות שונות, כל אחת מתמחה בפיתוח וטסטינג של מערכות ספציפיות במשחק. זה מאפשר פיתוח מסודר ובקרה טובה על הפרויקט הגדול.

## Scene Structure

### 01_CoreCreatureScene

**מטרה**: פיתוח המערכות הבסיסיות של היצור  

**מערכות נכללות**:

- MinipollCore + MinipollBrain (המערכות המרכזיות)
- MinipollHealth + MinipollStats (בריאות וסטטיסטיקות)
- MinipollNeedsSystem (מערכת צרכים - רעב, צמא, אנרגיה)
- MinipollEmotionsSystem (מערכת רגשות)
- MinipollMemorySystem (מערכת זיכרון)
- MinipollMovementController (תנועה)
- MinipollVisualController + MinipollBlinkController (ויזואליה)

**סביבה**: סביבה פשוטה עם אוכל, מים ומקומות מנוחה

### 02_SocialSystemsScene

**מטרה**: פיתוח מערכות חברתיות ויחסים בין יצורים  

**מערכות נכללות**:

- כל המערכות מ-CoreCreatureScene
- MinipollSocialRelations (יחסים חברתיים)
- MinipollTribeSystem (מערכת שבטים)
- MinipollBattleSystem (מערכת קרבות)

**סביבה**: מספר יצורים לאינטראקציה חברתית

### 03_WorldEconomyScene

**מטרה**: מערכות עולם, כלכלה ובנייה  

**מערכות נכללות**:

- MinipollBuildingSystem (מערכת בנייה)
- MinipollEconomySystem (מערכת כלכלה)
- FoodChainManager (שרשרת מזון)
- WorldManager (ניהול עולם)
- MinipollWorldInteraction (אינטראקציה עם העולם)
- UtilityAI (בינה מלאכותית מתקדמת)

**סביבה**: עולם עשיר עם משאבים, אתרי בנייה, מסחר

### 04_AdvancedFeaturesScene

**מטרה**: מערכות מתקדמות ותכונות מיוחדות  

**מערכות נכללות**:

- MinipollGeneticsSystem (מערכת גנטיקה)
- MinipollReproductionSystem (מערכת רבייה)
- MinipollArtifactSystem (מערכת חפצים)
- MinipollNarrativeSystem (מערכת סיפור)
- MinipollTaskSystem (מערכת משימות)
- MinipollMountSystem (מערכת רכיבה)
- Unity ML-Agents integration

**סביבה**: עולם מלא עם תכונות מיוחדות לטסטינג

### 05_IntegrationTestScene

**מטרה**: טסטינג של כל המערכות יחד  

**מערכות נכללות**:

- כל המערכות יחד
- Performance testing
- Full gameplay experience

**סביבה**: עולם שלם לחוויית משחק מלאה

## Navigation System

### Scene Navigation Manager
נוסף מנהל מעבר בין סצנות שמאפשר:
- מעבר קל בין סצנות עם מקשי מספרים (1-5)
- ניווט עם חיצי כיוון (←→)
- תפריט ניווט עם מקש Tab
- תמיכה במעבר מהיר לפיתוח

### Keyboard Shortcuts
- **Tab**: הצגת/הסתרת תפריט הניווט
- **1-5**: מעבר ישיר לסצנה המתאימה
- **←→**: מעבר לסצנה הקודמת/הבאה

## Development Workflow

### Phase 1: Core Development (01_CoreCreatureScene)
1. פיתוח יצור בסיסי עם צרכים בסיסיים
2. מימוש מערכת תנועה ואנימציות
3. טסטינג מערכת זיכרון ורגשות
4. אופטימיזציה של ביצועים בסיסיים

### Phase 2: Social Development (02_SocialSystemsScene)
1. הוספת יצורים נוספים
2. פיתוח מערכות אינטראקציה חברתית
3. מימוש מערכת שבטים וקרבות
4. טסטינג דינמיקות חברתיות

### Phase 3: World Systems (03_WorldEconomyScene)
1. פיתוח מערכת כלכלה ומסחר
2. מימוש מערכת בנייה ופיתוח
3. יצירת שרשרת מזון מורכבת
4. אינטגרציה עם מערכות AI מתקדמות

### Phase 4: Advanced Features (04_AdvancedFeaturesScene)
1. מימוש מערכת גנטיקה ורבייה
2. פיתוח מערכת חפצים ומשימות
3. אינטגרציה של Unity ML-Agents
4. פיתוח מערכת סיפור דינמית

### Phase 5: Integration & Polish (05_IntegrationTestScene)
1. חיבור כל המערכות יחד
2. אופטימיזציה של ביצועים
3. טסטינג חוויית משחק מלאה
4. הכנה לגרסת שחרור

## Best Practices

### Scene Organization
- כל סצנה מתמחה במערכות ספציפיות
- שימוש בתחיליות ברורות (01_, 02_, וכו')
- תיעוד ברור של מערכות בכל סצנה

### Development Process
- פיתוח חדרגתי: התחל עם הבסיס, הוסף מורכבות
- טסטינג מתמיד בכל סצנה לפני מעבר להבאה
- שמירה על ביצועים טובים בכל שלב

### Navigation
- השתמש ב-SceneNavigationManager למעבר מהיר
- בדוק שכל מערכת עובדת לפני מעבר לסצנה הבאה
- השתמש בקיצורי המקלדת לפיתוח יעיל

## Next Steps
1. ✅ יצירת סצנות הבסיס והמעבר ביניהן
2. 🔄 פיתוח CoreCreatureScene עם מערכות בסיסיות
3. ⏳ הוספת מערכות חברתיות ב-SocialSystemsScene
4. ⏳ פיתוח מערכות עולם ב-WorldEconomyScene
5. ⏳ מימוש תכונות מתקדמות ב-AdvancedFeaturesScene
6. ⏳ אינטגרציה מלאה ב-IntegrationTestScene
