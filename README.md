# Minipoll V5 - Unity Project

## 📋 תיאור הפרויקט
פרויקט Unity למשחק/אפליקציית Minipoll עם entrance sequence מושלם.

## 🎬 מערכת Entrance Sequence
המערכת כוללת:
- **Logo Video**: מוצג פעם אחת בתחילת הרצה
- **Minipoll Video**: לולאה עד 10 שניות או לחיצה למדלג
- **Menu Buttons**: הופעה לאחר סיום הווידאו

## 🛠️ רכיבים עיקריים

### SimpleEntranceManager.cs
הסקריפט הראשי המנהל את רצף הכניסה:
- ניהול video sequence
- מעבר בין שלבים
- טיפול בלחיצות לדילוג
- הגדרות fullscreen לווידאו

### MenuButtonsAnimationController.cs
מנהל אנימציות של כפתורי התפריט

## 🎯 התקנה והפעלה
1. פתח את הפרויקט ב-Unity 2022+
2. פתח את Scene: `Assets/Scenes/01_SplashMenu.unity`
3. בחר את GameObject `EntranceManager`
4. גרור את הרכיבים בInspector:
   - VideoPlayer → Video Player field
   - VideoDisplay → Video Display field  
   - MenuButtons → Menu Buttons field
   - הוסף video clips לשדות Logo Video ו-Minipoll Video
5. הפעל את המשחק

## 🔧 הגדרות
- `enableDebugLogs`: הפעלת לוגים למעקב
- `skipVideosForTesting`: דילוג על ווידאו לטסטים מהירים

## 📝 הערות טכניות
- הפרויקט משתמש ב-VideoPlayer עם RenderTexture
- תמיכה בfullscreen video display
- מותאם לרזולוציות שונות
- כולל מערכת דילוג עם כל מקש או לחיצה

## 🚀 גרסה נוכחית
גרסה נקייה ועובדת של entrance sequence עם תיקון כל בעיות הקומפילציה והתצוגה.

## 🎮 Git Version Control
הפרויקט מנוהל ב-Git לשמירה בטוחה של כל השינויים.

### פקודות Git בסיסיות:
```bash
# לבדוק סטטוס
git status

# להוסיף שינויים חדשים
git add .

# ליצור commit חדש
git commit -m "תיאור השינוי"

# ליצור tag לגרסה חדשה
git tag -a v1.1.0 -m "תיאור הגרסה"

# לראות היסטוריה
git log --oneline
```

### גרסאות:
- **v1.0.0**: גרסה ראשונה עובדת עם entrance sequence מלא

## 📅 תאריך עדכון אחרון
יולי 2025
